using HaruyasumiRyokouki.Backend.Common.Helpers;
using HaruyasumiRyokouki.Backend.Common.ResultType;
using HaruyasumiRyokouki.Backend.DbContexts;
using HaruyasumiRyokouki.Backend.Models.Db;
using HaruyasumiRyokouki.Backend.Models.Db.Enums;
using HaruyasumiRyokouki.Backend.Models.InternalDtos;
using HaruyasumiRyokouki.Backend.Services;
using HaruyasumiRyokouki.Backend.Services.Interfaces;
using Meckbaig.Cqrs.Abstractons;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HaruyasumiRyokouki.Backend.Features.Media;

public record SyncMediaCommand : IRequest<SyncMediaResponse>
{
}

public class SyncMediaResponse : BaseResponse
{
}

internal class SyncMediaHandler : IRequestHandler<SyncMediaCommand, SyncMediaResponse>
{
	private readonly IAppDbContext _context;
	private readonly IFileStorage _fileStorage;
	private readonly IMediaProcessorService _mediaProcessorService;
	private readonly IMediaDateResolver _mediaDateResolver;
	private readonly ILogger<SyncMediaCommand> _logger;

	public SyncMediaHandler(IAppDbContext context, IFileStorage fileStorage, IMediaProcessorService mediaProcessorService, IMediaDateResolver mediaDateResolver, ILogger<SyncMediaCommand> logger)
	{
		_context = context;
		_fileStorage = fileStorage;
		_mediaProcessorService = mediaProcessorService;
		_mediaDateResolver = mediaDateResolver;
		_logger = logger;
	}

	public async Task<SyncMediaResponse> Handle(SyncMediaCommand request, CancellationToken cancellationToken)
	{
		var datesFromDb = await _context.Days.ToListAsync(cancellationToken);
		var filesFromDb = await _context.MediaFiles
			.Select(m => new MediaForMiniatureCheck(m.Id, m.FileName, !string.IsNullOrEmpty(m.Miniature), m.AdditionalFiles, m.Source))
			.ToListAsync(cancellationToken);

		await CheckForNewFiles(datesFromDb, filesFromDb, cancellationToken);

		await CheckForIncompleteFiles(filesFromDb, cancellationToken);

		return new SyncMediaResponse();
	}

	private async Task CheckForNewFiles(List<Day> datesFromDb, IEnumerable<MediaForMiniatureCheck> filesFromDb, CancellationToken cancellationToken)
	{
		_logger.LogInformation("New files check started");
		List<StorageFile> filesToCreate = [];
		var storageFiles = await _fileStorage.GetFilesAsync(cancellationToken);
		foreach (var storageFile in storageFiles)
		{
			string webFileName = _mediaProcessorService.GetVideoWebName(storageFile.FileName);
			string previewFileName = _mediaProcessorService.GetVideoPreviewName(storageFile.FileName);
			if (storageFile.FileName == webFileName || storageFile.FileName == previewFileName)
				continue;

			if (!filesFromDb.Any(f => f.FileName == storageFile.FileName))
			{
				filesToCreate.Add(storageFile);
			}
		}
		_logger.LogInformation("New files found: {Count}", filesToCreate.Count);

		foreach (var fileToCreate in filesToCreate)
		{
			try
			{
				var creationDate = ResolveCreationDate(fileToCreate.FileName, fileToCreate.Created);
				if (creationDate.IsFailure)
				{
					_logger.LogError("Error resolving date for {FileName}: {Error}", fileToCreate.FileName, creationDate.Error);
					_logger.LogWarning("File skipped: {FileName}", fileToCreate.FileName);
					continue;
				}
				await CreateMediaFileAsync(fileToCreate.FileName, creationDate.Value, datesFromDb, cancellationToken);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
				_logger.LogWarning("File skipped: {FileName}", fileToCreate.FileName);
			}
		}

		CheckForMissingFiles(filesFromDb, storageFiles);
	}

	private void CheckForMissingFiles(IEnumerable<MediaForMiniatureCheck> filesFromDb, IReadOnlyCollection<StorageFile> storageFiles)
	{
		var localFiles = filesFromDb.Where(f => f.Source == MediaSource.Local).ToList();
		List<string> expectedFileNames = localFiles.Select(x => x.FileName).ToList();
		foreach (var file in localFiles.Where(f => f.AdditionalFiles.Count > 0))
		{
			expectedFileNames.AddRange(file.AdditionalFiles);
		}

		var missingFiles = expectedFileNames.Where(f => !storageFiles.Any(sf => sf.FileName == f)).ToList();

		if (missingFiles.Count > 0)
			_logger.LogWarning
			(
				"{Count} files are missing from file storage! Missing files list:\n{List}",
				missingFiles.Count,
				string.Join("\n", missingFiles)
			);
	}

	private async Task CheckForIncompleteFiles(IEnumerable<MediaForMiniatureCheck> filesFromDb, CancellationToken cancellationToken)
	{
		_logger.LogInformation("Incomplete files check started");
		var incompleteFiles = filesFromDb.Where(f => f.Source == MediaSource.Local && !f.HasMiniature);
		_logger.LogInformation("Incomplete files found: {Count}", incompleteFiles.Count());

		foreach (var fileFromDb in incompleteFiles)
		{
			try
			{
				await CreateMiniatureAsync(fileFromDb.Id, fileFromDb.FileName, cancellationToken);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
			}
		}
	}

	private async Task CreateMediaFileAsync(string fileName, DateTime creationTime, List<Day> datesFromDb, CancellationToken cancellationToken)
	{
		DateOnly creationDate = DateOnly.FromDateTime(creationTime);
		var (creationDay, dayCreated) = DayResolver.GetOrCreate(datesFromDb, creationDate);
		if (dayCreated)
			_context.Days.Add(creationDay);

		var fileMediaType = GetMediaType(fileName);
		Result<ConvertionsResponseDto> conversionResult;
		switch (fileMediaType)
		{
			case MediaType.Image:
				conversionResult = await _mediaProcessorService.ConvertImageAsync(fileName, cancellationToken);
				break;
			case MediaType.Video:
				conversionResult = await _mediaProcessorService.ConvertVideoAsync(fileName, cancellationToken);
				break;
			default:
				_logger.LogWarning("File {FileName} has unknown media type", fileName);
				return;
		}

		if (conversionResult.IsFailure)
		{
			_logger.LogError("Error occured during {FileName} conversion: {ErrorMessage}", fileName, conversionResult.Error);
			return;
		}

		var mediaFile = new MediaFile
		{
			FileName = conversionResult.Value.NewFileName,
			AspectRatio = conversionResult.Value.AspectRatio,
			Created = creationTime,
			Day = creationDay,
			Type = fileMediaType,
			Miniature = conversionResult.Value.Miniature,
			AdditionalFiles = conversionResult.Value.AdditionalFileNames
		};
		_context.MediaFiles.Add(mediaFile);
		await _context.SaveChangesAsync(cancellationToken);
		
		if (dayCreated)
			_logger.LogDebug("Day {Date} created in database", creationDate.ToString("yyyy-MM-dd"));
		_logger.LogDebug("File {FileName} created in database", fileName);
	}

	private async Task CreateMiniatureAsync(int id, string fileName, CancellationToken cancellationToken)
	{
		string actualFileName;
		if (_mediaProcessorService.IsAnImage(fileName))
			actualFileName = fileName;
		else if (_mediaProcessorService.IsAVideo(fileName))
			actualFileName = _mediaProcessorService.GetVideoPreviewName(fileName);
		else
			throw new NotImplementedException($"File {fileName} is not an image or a video");

		var result = await _mediaProcessorService.CreateMiniatureAsync(actualFileName, cancellationToken);
		if (result.IsFailure)
		{
			_logger.LogError("Error occured during {FileName} conversion: {ErrorMessage}", fileName, result.Error);
			return;
		}
		await _context.MediaFiles
			.Where(m => m.Id == id)
			.ExecuteUpdateAsync
			(
				m => m.SetProperty(m => m.Miniature, result.Value),
				cancellationToken
			);
		_logger.LogInformation("Miniature for {FileName} was updated", fileName);
	}

	private MediaType GetMediaType(string fileName)
	{
		var extension = Path.GetExtension(fileName);

		if (string.IsNullOrEmpty(extension))
			return MediaType.Unknown;

		if (_mediaProcessorService.IsAnImage(fileName))
			return MediaType.Image;

		if (_mediaProcessorService.IsAVideo(fileName))
			return MediaType.Video;

		return MediaType.Unknown;
	}

	private Result<DateTime> ResolveCreationDate(string fileName, DateTime lastModifiedDate)
	{
		var fromName = _mediaDateResolver.TryExtractLocalFromName(fileName);
		if (fromName.HasValue)
			return Result<DateTime>.Success(fromName.Value);

		return _mediaDateResolver.ResolveFromUtc(lastModifiedDate);
	}

	private record MediaForMiniatureCheck(int Id, string FileName, bool HasMiniature, ICollection<string> AdditionalFiles, MediaSource Source);
}

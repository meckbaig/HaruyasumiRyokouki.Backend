using HaruyasumiRyokouki.Backend.Common.ResultType;
using HaruyasumiRyokouki.Backend.DbContexts;
using HaruyasumiRyokouki.Backend.Models.Db;
using HaruyasumiRyokouki.Backend.Models.Db.Enums;
using HaruyasumiRyokouki.Backend.Models.InternalDtos;
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
	private readonly ILogger<SyncMediaCommand> _logger;

	public SyncMediaHandler(IAppDbContext context, IFileStorage fileStorage, IMediaProcessorService mediaProcessorService, ILogger<SyncMediaCommand> logger)
	{
		_context = context;
		_fileStorage = fileStorage;
		_mediaProcessorService = mediaProcessorService;
		_logger = logger;
	}

	public async Task<SyncMediaResponse> Handle(SyncMediaCommand request, CancellationToken cancellationToken)
	{
		var datesFromDb = await _context.Days.ToListAsync(cancellationToken);
		var filesFromDb = await _context.MediaFiles
			.Select(m => new MediaForMiniatureCheck(m.Id, m.FileName, !string.IsNullOrEmpty(m.Miniature)))
			.ToListAsync(cancellationToken);

		await CheckForNewFiles(datesFromDb, filesFromDb, cancellationToken);

		await CheckForIncompleteFiles(filesFromDb, cancellationToken);

		return new SyncMediaResponse();
	}

	private async Task CheckForNewFiles(List<Day> datesFromDb, IEnumerable<MediaForMiniatureCheck> filesFromDb, CancellationToken cancellationToken)
	{
		_logger.LogInformation("New files check started");
		List<StorageFile> filesToCreate = [];
		foreach (var storageFile in await _fileStorage.GetFilesAsync(cancellationToken))
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
				await CreateMediaFileAsync(fileToCreate.FileName, fileToCreate.Created, datesFromDb, cancellationToken);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
			}
		}
	}

	private async Task CheckForIncompleteFiles(IEnumerable<MediaForMiniatureCheck> filesFromDb, CancellationToken cancellationToken)
	{
		_logger.LogInformation("Incomplete files check started");
		var incompleteFiles = filesFromDb.Where(f => !f.HasMiniature);
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
		bool dayCreated = false;
		if (datesFromDb.FirstOrDefault(d => d.Date == creationDate) is not Day creationDay)
		{
			creationDay = new Day { Date = creationDate };
			datesFromDb.Add(creationDay);
			_context.Days.Add(creationDay);
			dayCreated = true;
		}

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

		conversionResult.Switch
		(
			(response) =>
			{
				if (fileMediaType == MediaType.Image)
					fileName = response.NewFileName;
			},
			(error) => _logger.LogError("Error occured during {FileName} conversion: {ErrorMessage}", fileName, error)
		);
		if (conversionResult.IsFailure)
			return;

		var mediaFile = new MediaFile
		{
			FileName = fileName,
			Created = creationTime,
			Day = creationDay,
			Type = fileMediaType,
			Miniature = conversionResult.Value.Miniature
		};
		_context.MediaFiles.Add(mediaFile);
		await _context.SaveChangesAsync(cancellationToken);

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

	public MediaType GetMediaType(string fileName)
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

	private record MediaForMiniatureCheck(int Id, string FileName, bool HasMiniature);
}

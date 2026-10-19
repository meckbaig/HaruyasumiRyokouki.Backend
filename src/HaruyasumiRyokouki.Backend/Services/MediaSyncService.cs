using HaruyasumiRyokouki.Backend.Common.Exceptions;
using HaruyasumiRyokouki.Backend.Common.Helpers;
using HaruyasumiRyokouki.Backend.Common.Options;
using HaruyasumiRyokouki.Backend.Common.ResultType;
using HaruyasumiRyokouki.Backend.DbContexts;
using HaruyasumiRyokouki.Backend.Models.Db;
using HaruyasumiRyokouki.Backend.Models.Db.Enums;
using HaruyasumiRyokouki.Backend.Models.InternalDtos;
using HaruyasumiRyokouki.Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HaruyasumiRyokouki.Backend.Services;

/// <inheritdoc/>
internal class MediaSyncService : IMediaSyncService
{
	/// <summary>
	/// Allows only one sync at a time: parallel runs would convert and import the same files twice.
	/// </summary>
	private static readonly SemaphoreSlim SyncLock = new(1, 1);

	private readonly IAppDbContext _context;
	private readonly IFileStorage _fileStorage;
	private readonly IMediaProcessorService _mediaProcessorService;
	private readonly IMediaDateResolver _mediaDateResolver;
	private readonly MediaFormatOptions _mediaFormatOptions;
	private readonly ILogger<MediaSyncService> _logger;

	/// <summary>
	/// Constructor with parameters for DI.
	/// </summary>
	public MediaSyncService
	(
		IAppDbContext context,
		IFileStorage fileStorage,
		IMediaProcessorService mediaProcessorService,
		IMediaDateResolver mediaDateResolver,
		IOptions<MediaFormatOptions> mediaFormatOptions,
		ILogger<MediaSyncService> logger
	)
	{
		_context = context;
		_fileStorage = fileStorage;
		_mediaProcessorService = mediaProcessorService;
		_mediaDateResolver = mediaDateResolver;
		_mediaFormatOptions = mediaFormatOptions.Value;
		_logger = logger;
	}

	/// <inheritdoc/>
	public async Task SyncAsync(CancellationToken cancellationToken = default)
	{
		if (!await SyncLock.WaitAsync(0, cancellationToken))
			throw new ConflictException("Media sync is already running.");

		try
		{
			var datesFromDb = await _context.Days.ToListAsync(cancellationToken);
			var filesFromDb = await _context.MediaFiles
				.Select(m => new MediaForMiniatureCheck(m.Id, m.FileName, !string.IsNullOrEmpty(m.Miniature), m.AdditionalFiles, m.Source))
				.ToListAsync(cancellationToken);

			await CheckForNewFiles(datesFromDb, filesFromDb, cancellationToken);

			await CheckForIncompleteFiles(filesFromDb, cancellationToken);
		}
		finally
		{
			SyncLock.Release();
		}
	}

	private async Task CheckForNewFiles(List<Day> datesFromDb, IEnumerable<MediaForMiniatureCheck> filesFromDb, CancellationToken cancellationToken)
	{
		_logger.LogInformation("New files check started");
		List<StorageFile> filesToCreate = [];
		var storageFiles = await _fileStorage.GetFilesAsync(cancellationToken);
		foreach (var storageFile in storageFiles)
		{
			// Additional files of videos are not media files on their own.
			string webFileName = MediaFileNaming.GetVideoWebName(storageFile.FileName);
			string previewFileName = MediaFileNaming.GetVideoPreviewName(storageFile.FileName, _mediaFormatOptions.ImagePreset);
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
				await CreateMediaFileAsync(fileToCreate, datesFromDb, cancellationToken);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to import {FileName}", fileToCreate.FileName);
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
				_logger.LogError(ex, "Failed to create miniature for {FileName}", fileFromDb.FileName);
			}
		}
	}

	private async Task CreateMediaFileAsync(StorageFile file, List<Day> datesFromDb, CancellationToken cancellationToken)
	{
		string fileName = file.FileName;

		var fileMediaType = MediaFileNaming.GetMediaType(fileName);
		if (fileMediaType == MediaType.Unknown)
		{
			_logger.LogWarning("File {FileName} has unknown media type, skipped", fileName);
			return;
		}

		var creationTimeResult = ResolveCreationTime(fileName, file.Created);
		if (creationTimeResult.IsFailure)
		{
			_logger.LogError("Error resolving date for {FileName}: {Error}", fileName, creationTimeResult.Error);
			_logger.LogWarning("File skipped: {FileName}", fileName);
			return;
		}
		var creationTime = creationTimeResult.Value;

		var conversionResult = fileMediaType == MediaType.Image
			? await _mediaProcessorService.ConvertImageAsync(fileName, cancellationToken)
			: await _mediaProcessorService.ConvertVideoAsync(fileName, cancellationToken);

		if (conversionResult.IsFailure)
		{
			_logger.LogError("Error occurred during {FileName} conversion: {ErrorMessage}", fileName, conversionResult.Error);
			return;
		}

		// The day is resolved only after a successful conversion, so a skipped file never leaves an empty day.
		DateOnly creationDate = DateOnly.FromDateTime(creationTime);
		var (creationDay, dayCreated) = DayResolver.GetOrCreate(datesFromDb, creationDate);
		if (dayCreated)
			_context.Days.Add(creationDay);

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
		if (MediaFileNaming.IsImage(fileName))
			actualFileName = fileName;
		else if (MediaFileNaming.IsVideo(fileName))
			actualFileName = MediaFileNaming.GetVideoPreviewName(fileName, _mediaFormatOptions.ImagePreset);
		else
			throw new NotImplementedException($"File {fileName} is not an image or a video");

		var result = await _mediaProcessorService.CreateMiniatureAsync(actualFileName, cancellationToken);
		if (result.IsFailure)
		{
			_logger.LogError("Error occurred during {FileName} conversion: {ErrorMessage}", fileName, result.Error);
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

	/// <summary>
	/// Takes the local time from the file name if it contains one, otherwise converts the modification time.
	/// </summary>
	private Result<DateTime> ResolveCreationTime(string fileName, DateTime lastModifiedDate)
	{
		var fromName = _mediaDateResolver.TryExtractLocalFromName(fileName);
		if (fromName.HasValue)
			return Result<DateTime>.Success(fromName.Value);

		return _mediaDateResolver.ResolveFromUtc(lastModifiedDate);
	}

	private record MediaForMiniatureCheck(int Id, string FileName, bool HasMiniature, ICollection<string> AdditionalFiles, MediaSource Source);
}

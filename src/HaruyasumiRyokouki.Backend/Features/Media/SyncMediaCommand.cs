using HaruyasumiRyokouki.Backend.Common.ResultType;
using HaruyasumiRyokouki.Backend.DbContexts;
using HaruyasumiRyokouki.Backend.Models.Db;
using HaruyasumiRyokouki.Backend.Models.Db.Enums;
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
		var filesFromDb = await _context.MediaFiles.Select(m => m.FileName).ToListAsync(cancellationToken);
		foreach (var storageFile in await _fileStorage.GetFilesAsync(cancellationToken))
		{
			string webFileName = _mediaProcessorService.GetVideoWebName(storageFile.FileName);
			string previewFileName = _mediaProcessorService.GetVideoPreviewName(storageFile.FileName);
			if (storageFile.FileName == webFileName || storageFile.FileName == previewFileName)
				continue;

			if (!filesFromDb.Any(fileName => fileName == storageFile.FileName))
			{
				try
				{
					await CreateMediaFile(storageFile.FileName, storageFile.Created, datesFromDb, cancellationToken);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex.Message);
				}
			}
		}
		return new SyncMediaResponse();
	}

	private async Task CreateMediaFile(string fileName, DateTime creationTime, List<Day> datesFromDb, CancellationToken cancellationToken)
	{
		DateOnly creationDate = DateOnly.FromDateTime(creationTime);
		if (datesFromDb.FirstOrDefault(d => d.Date == creationDate) is not Day creationDay)
		{
			creationDay = new Day { Date = creationDate };
			datesFromDb.Add(creationDay);
			_context.Days.Add(creationDay);

			_logger.LogDebug("Day {Date} created.", creationDate);
		}

		var fileMediaType = GetMediaType(fileName);
		Result<string> conversionResult;
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
				if (creationDay.Id == 0)
					_logger.LogInformation("Day {Date} removed", creationDate);
				return;
		}
		conversionResult.Switch
		(
			(newFileName) => 
			{
				if (fileMediaType == MediaType.Image)
					fileName = newFileName;
			},
			(error) => _logger.LogError("Error occured during {FileName} conversion: {ErrorMessage}", fileName, error)
		);

		var mediaFile = new MediaFile
		{
			FileName = fileName,
			Created = creationTime,
			Day = creationDay,
			Type = fileMediaType
		};
		_context.MediaFiles.Add(mediaFile);
		await _context.SaveChangesAsync(cancellationToken);

		_logger.LogDebug("File {FileName} created in database", fileName);
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
}

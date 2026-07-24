using HaruyasumiRyokouki.Backend.DbContexts;
using HaruyasumiRyokouki.Backend.Models.Db;
using HaruyasumiRyokouki.Backend.Models.Db.Enums;
using HaruyasumiRyokouki.Backend.Services.Interfaces;
using Meckbaig.Cqrs.Abstractons;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HaruyasumiRyokouki.Backend.Features.Days;

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
	private readonly ILogger<SyncMediaCommand> _logger;

	public SyncMediaHandler(IAppDbContext context, IFileStorage fileStorage, ILogger<SyncMediaCommand> logger)
	{
		_context = context;
		_fileStorage = fileStorage;
		_logger = logger;
	}

	public async Task<SyncMediaResponse> Handle(SyncMediaCommand request, CancellationToken cancellationToken)
	{
		var datesFromDb = await _context.Days.ToListAsync(cancellationToken);
		var filesFromDb = await _context.MediaFiles.Select(m => m.FileName).ToListAsync(cancellationToken);
		foreach (var storageFile in await _fileStorage.GetFilesAsync(cancellationToken))
		{
			if (!filesFromDb.Any(fileName => fileName == storageFile.FileName))
			{
				await CreateMediaFile(storageFile.FileName, storageFile.Created, datesFromDb);
			}
		}
		await _context.SaveChangesAsync(cancellationToken);
		return new SyncMediaResponse();
	}

	private async Task CreateMediaFile(string fileName, DateTime creationTime, List<Day> datesFromDb)
	{
		DateOnly creationDate = DateOnly.FromDateTime(creationTime);
		if (datesFromDb.FirstOrDefault(d => d.Date == creationDate) is not Day creationDay)
		{
			creationDay = new Day { Date = creationDate };
			datesFromDb.Add(creationDay);
			_context.Days.Add(creationDay);

			_logger.LogDebug("Day {Date} created", creationDate);
		}

		var mediaFile = new MediaFile
		{
			FileName = fileName,
			Created = creationTime,
			Day = creationDay,
			Type = GetMediaType(fileName)
		};
		_context.MediaFiles.Add(mediaFile);

		_logger.LogDebug("File {FileName} created", fileName);
	}

	private static readonly HashSet<string> ImageExtensions =
	[
		".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".tif", ".tiff", ".heic", ".heif", ".avif"
	];

	private static readonly HashSet<string> VideoExtensions =
	[
		".mp4", ".mov", ".avi", ".mkv", ".wmv", ".flv", ".webm", ".m4v", ".3gp", ".mpeg", ".mpg"
	];

	public static MediaType GetMediaType(string fileName)
	{
		var extension = Path.GetExtension(fileName);

		if (string.IsNullOrEmpty(extension))
			return MediaType.Unknown;

		if (ImageExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
			return MediaType.Image;

		if (VideoExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
			return MediaType.Video;

		return MediaType.Unknown;
	}
}

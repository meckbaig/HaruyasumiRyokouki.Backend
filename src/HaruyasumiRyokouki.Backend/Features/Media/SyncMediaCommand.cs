using HaruyasumiRyokouki.Backend.DbContexts;
using HaruyasumiRyokouki.Backend.Models.Db;
using HaruyasumiRyokouki.Backend.Models.Db.Enums;
using Meckbaig.Cqrs.Abstractons;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace HaruyasumiRyokouki.Backend.Features.Days;

public record SyncMediaCommand : IRequest<SyncMediaResponse>
{
}

public class SyncMediaResponse : BaseResponse
{
}
internal class SyncMediaHandler : IRequestHandler<SyncMediaCommand, SyncMediaResponse>
{
	private const string PathToMedia = "E:\\WorkCashe\\японя1";
	private readonly IAppDbContext _context;
	private readonly ILogger<SyncMediaCommand> _logger;

	public SyncMediaHandler(IAppDbContext context, ILogger<SyncMediaCommand> logger)
	{
		_context = context;
		_logger = logger;
	}

	public async Task<SyncMediaResponse> Handle(SyncMediaCommand request, CancellationToken cancellationToken)
	{
		var datesFromDb = await _context.Days.ToListAsync(cancellationToken);
		var filesFromDb = await _context.MediaFiles.Select(m => m.FileName).ToListAsync(cancellationToken);
		foreach (string filePath in Directory.EnumerateFiles(PathToMedia))
		{
			var info = new FileInfo(filePath);
			if (!filesFromDb.Any(fileName => fileName == info.Name))
			{
				await CreateMediaFile(info.Name, DateTime.SpecifyKind(info.LastWriteTime, DateTimeKind.Unspecified), datesFromDb);
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
		".mp4", ".mov", ".avi", ".mkv", ".wmv",	".flv", ".webm", ".m4v", ".3gp", ".mpeg", ".mpg"
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

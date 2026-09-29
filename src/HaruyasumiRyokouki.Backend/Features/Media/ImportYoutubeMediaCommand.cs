using FluentValidation;
using HaruyasumiRyokouki.Backend.Common.Abstractions;
using HaruyasumiRyokouki.Backend.DbContexts;
using HaruyasumiRyokouki.Backend.Extensions;
using HaruyasumiRyokouki.Backend.Models.Db;
using HaruyasumiRyokouki.Backend.Models.Db.Enums;
using HaruyasumiRyokouki.Backend.Models.Dtos.Media;
using HaruyasumiRyokouki.Backend.Models.InternalDtos;
using HaruyasumiRyokouki.Backend.Services;
using HaruyasumiRyokouki.Backend.Services.Interfaces;
using Meckbaig.Cqrs.Abstractons;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace HaruyasumiRyokouki.Backend.Features.Media;

public record ImportYoutubeMediaCommand : IRequest<ImportYoutubeMediaResponse>, IDisplayAwareRequest
{
	[FromBody]
	public required BodyParameters Body { get; init; }

	[JsonIgnore]
	public ClientDisplay? ClientDisplay { get; set; }

	public record BodyParameters
	{
		public required string Url { get; init; }
	}
}

public class ImportYoutubeMediaResponse : BaseResponse
{
	public required MediaFileEditDto Media { get; init; }
}

internal class ImportYoutubeMediaCommandValidator : AbstractValidator<ImportYoutubeMediaCommand>
{
	public ImportYoutubeMediaCommandValidator()
	{
		RuleFor(x => x.Body.Url)
			.NotEmpty()
			.Must(url => YoutubeUrlParser.TryExtractVideoId(url, out _))
			.WithMessage("Invalid YouTube URL.");
	}
}

internal class ImportYoutubeMediaHandler : IRequestHandler<ImportYoutubeMediaCommand, ImportYoutubeMediaResponse>
{
	private readonly IAppDbContext _context;
	private readonly IYoutubeMetadataService _youtubeMetadataService;
	private readonly IMediaProcessorService _mediaProcessorService;
	private readonly IMediaDateResolver _mediaDateResolver;
	private readonly IMediaUrlsProvider _urlsProvider;
	private readonly IHttpClientFactory _httpClientFactory;
	private readonly ILogger<ImportYoutubeMediaCommand> _logger;

	public ImportYoutubeMediaHandler
	(
		IAppDbContext context,
		IYoutubeMetadataService youtubeMetadataService,
		IMediaProcessorService mediaProcessorService,
		IMediaDateResolver mediaDateResolver,
		IMediaUrlsProvider urlsProvider,
		IHttpClientFactory httpClientFactory,
		ILogger<ImportYoutubeMediaCommand> logger
	)
	{
		_context = context;
		_youtubeMetadataService = youtubeMetadataService;
		_mediaProcessorService = mediaProcessorService;
		_mediaDateResolver = mediaDateResolver;
		_urlsProvider = urlsProvider;
		_httpClientFactory = httpClientFactory;
		_logger = logger;
	}

	public async Task<ImportYoutubeMediaResponse> Handle(ImportYoutubeMediaCommand request, CancellationToken cancellationToken)
	{
		if (!YoutubeUrlParser.TryExtractVideoId(request.Body.Url, out var videoId))
			throw new ValidationException("Invalid YouTube URL.");

		bool alreadyImported = await _context.MediaFiles
			.AnyAsync(m => m.Source == MediaSource.YouTube && m.FileName == videoId, cancellationToken);
		if (alreadyImported)
			throw new ValidationException($"YouTube video '{videoId}' has already been imported.");

		var metadataResult = await _youtubeMetadataService.GetMetadataAsync(videoId, cancellationToken);
		if (metadataResult.IsFailure)
			throw new ValidationException(metadataResult.Error!.Message);
		var metadata = metadataResult.Value!;

		var createdResult = _mediaDateResolver.ResolveFromUtc(metadata.PublishedAtUtc);
		if (createdResult.IsFailure)
			throw new ValidationException(createdResult.Error!.Message);
		var created = createdResult.Value;

		var datesFromDb = await _context.Days.ToListAsync(cancellationToken);
		var (day, dayCreated) = DayResolver.GetOrCreate(datesFromDb, DateOnly.FromDateTime(created));
		if (dayCreated)
			_context.Days.Add(day);

		string previewFileName = await CreatePreviewAsync(videoId, metadata.ThumbnailUrl, cancellationToken);

		var miniatureResult = await _mediaProcessorService.CreateMiniatureAsync(previewFileName, cancellationToken);
		if (miniatureResult.IsFailure)
			throw new ValidationException(miniatureResult.Error!.Message);

		var mediaFile = new MediaFile
		{
			FileName = videoId,
			Source = MediaSource.YouTube,
			ExternalUrl = GetEmbedUrl(videoId),
			Type = MediaType.Video,
			AspectRatio = metadata.AspectRatio,
			Created = created,
			Day = day,
			Miniature = miniatureResult.Value,
			AdditionalFiles = [previewFileName]
		};

		_context.MediaFiles.Add(mediaFile);
		await _context.SaveChangesAsync(cancellationToken);

		_logger.LogInformation("YouTube media {VideoId} imported", videoId);

		return new ImportYoutubeMediaResponse
		{
			Media = mediaFile.ToEditDto().AddUrls(mediaFile, _urlsProvider, request.ClientDisplay)
		};
	}

	private async Task<string> CreatePreviewAsync(string videoId, string thumbnailUrl, CancellationToken cancellationToken)
	{
		var extension = Path.GetExtension(new Uri(thumbnailUrl).AbsolutePath);
		if (string.IsNullOrEmpty(extension))
			extension = ".jpg";

		try
		{
			using var httpClient = _httpClientFactory.CreateClient();
			await using var thumbnailStream = await httpClient.GetStreamAsync(thumbnailUrl, cancellationToken);

			var previewResult = await _mediaProcessorService.CreateYoutubePreviewAsync(videoId, extension, thumbnailStream, cancellationToken);
			if (previewResult.IsFailure)
				throw new ValidationException(previewResult.Error!.Message);

			return previewResult.Value!;
		}
		catch (ValidationException)
		{
			throw;
		}
		catch (Exception ex)
		{
			throw new ValidationException($"Failed to download YouTube preview: {ex.Message}");
		}
	}

	private string GetEmbedUrl(string videoId)
	{
		return $"https://www.youtube-nocookie.com/embed/{videoId}";
	}
}

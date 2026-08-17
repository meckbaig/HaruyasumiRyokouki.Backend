using HaruyasumiRyokouki.Backend.Common.Options;
using HaruyasumiRyokouki.Backend.Models.InternalDtos;
using HaruyasumiRyokouki.Backend.Models.InternalDtos.Enums;
using HaruyasumiRyokouki.Backend.Services.Builders;
using HaruyasumiRyokouki.Backend.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace HaruyasumiRyokouki.Backend.Services;

internal class NextcloudMediaPreviewService : IMediaPreviewService
{
	private readonly ILogger<NextcloudMediaPreviewService> _logger;
	private readonly IMediaProcessorService _mediaProcessor;
	private readonly IMediaResolutionCalculationService _resolutionCalculator;
	private readonly NextcloudPreviewUrlBuilder _builder;
	private readonly OriginStorageUrlBuilder _originBuilder;

	public NextcloudMediaPreviewService(ILogger<NextcloudMediaPreviewService> logger, IOptions<MediaPreviewOptions> options, IMediaResolutionCalculationService resolutionCalculator, IMediaProcessorService mediaProcessor)
	{
		_logger = logger;
		_mediaProcessor = mediaProcessor;
		_resolutionCalculator = resolutionCalculator;
		_builder = new NextcloudPreviewUrlBuilder
		(
			options.Value.Nextcloud.Endpoint,
			options.Value.Nextcloud.CdnEndpoint,
			options.Value.Nextcloud.PayloadStringBase,
			options.Value.Nextcloud.UseCdnForDownloads
		);
		_originBuilder = new OriginStorageUrlBuilder
		(
			options.Value.OriginStorage.Endpoint,
			options.Value.OriginStorage.CdnEndpoint,
			options.Value.OriginStorage.PayloadStringBase,
			options.Value.OriginStorage.UseCdnForDownloads
		);
		if (!_builder.Validate(out var _))
			throw new ArgumentException("Nextcloud base url is invalid", nameof(options));
		if (!_originBuilder.Validate(out var _))
			throw new ArgumentException("Video storage base url is invalid", nameof(options));
	}

	public string GetImageUrl(string fileName, ImageUrlType linkType, ClientDisplay? clientDisplay = null, float? aspectRatio = default)
	{
		int imageSize;
		switch (linkType)
		{
			case ImageUrlType.Download:
				return _originBuilder.BuildDownload(fileName);
			case ImageUrlType.FullScreen:
			case ImageUrlType.Preview:
				imageSize = _resolutionCalculator.GetResolution(linkType, clientDisplay?.Dpr, clientDisplay?.MinSide, aspectRatio);
				return _builder.BuildMedia(fileName, imageSize, imageSize);
			default:
				throw new NotImplementedException();
		}
	}

	public string GetVideoUrl(string fileName, VideoUrlType linkType, ICollection<string> additionalFiles, ClientDisplay? clientDisplay = null, float? aspectRatio = default)
	{
		switch (linkType)
		{
			case VideoUrlType.Download:
				return _originBuilder.BuildDownload(fileName);
			case VideoUrlType.Stream:
				string webFileName = _mediaProcessor.GetVideoWebName(fileName);
				if (additionalFiles.Contains(webFileName))
					return _originBuilder.BuildMedia(webFileName);
				return _originBuilder.BuildMedia(fileName);
			case VideoUrlType.Preview:
				string previewFileName = _mediaProcessor.GetVideoPreviewName(fileName);
				if (!additionalFiles.Contains(previewFileName))
					_logger.LogWarning("{PreviewFileName} does not exist in DB!", previewFileName);
				int imageSize = _resolutionCalculator.GetResolution(ImageUrlType.Preview, clientDisplay?.Dpr, clientDisplay?.MinSide, aspectRatio);
				return _builder.BuildMedia(_mediaProcessor.GetVideoPreviewName(fileName), imageSize, imageSize);
			default:
				throw new NotImplementedException();
		}
	}
}

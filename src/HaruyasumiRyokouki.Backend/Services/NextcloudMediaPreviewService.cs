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
	private readonly MediaPreviewOptions _options;
	private readonly IMediaProcessorService _mediaProcessor;
	private readonly IMediaResolutionCalculationService _resolutionCalculator;
	private readonly NextcloudPreviewUrlBuilder _builder;
	private readonly OriginStorageUrlBuilder _originBuilder;

	public NextcloudMediaPreviewService(ILogger<NextcloudMediaPreviewService> logger, IOptions<MediaPreviewOptions> options, IMediaResolutionCalculationService resolutionCalculator, IMediaProcessorService mediaProcessor)
	{
		_logger = logger;
		_options = options.Value;
		_mediaProcessor = mediaProcessor;
		_resolutionCalculator = resolutionCalculator;
		_builder = new NextcloudPreviewUrlBuilder(_options.Nextcloud?.PublicPreviewBase ?? "");
		_originBuilder = new OriginStorageUrlBuilder(_options.OriginStorageBase);

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
				return _originBuilder.Build(fileName);
			case ImageUrlType.FullScreen:
			case ImageUrlType.Preview:
				imageSize = _resolutionCalculator.GetResolution(linkType, clientDisplay?.Dpr, clientDisplay?.MinSide, aspectRatio);
				return _builder.Build(fileName, imageSize, imageSize);
			default:
				throw new NotImplementedException();
		}
	}

	public string GetVideoUrl(string fileName, VideoUrlType linkType, ICollection<string> additionalFiles, ClientDisplay? clientDisplay = null, float? aspectRatio = default)
	{
		switch (linkType)
		{
			case VideoUrlType.Download:
				return _originBuilder.Build(fileName);
			case VideoUrlType.Stream:
				string webFileName = _mediaProcessor.GetVideoWebName(fileName);
				if (additionalFiles.Contains(webFileName))
					return _originBuilder.Build(webFileName);
				return _originBuilder.Build(fileName);
			case VideoUrlType.Preview:
				string previewFileName = _mediaProcessor.GetVideoPreviewName(fileName);
				if (!additionalFiles.Contains(previewFileName))
					_logger.LogWarning("{PreviewFileName} does not exist in DB!", previewFileName);
				int imageSize = _resolutionCalculator.GetResolution(ImageUrlType.Preview, clientDisplay?.Dpr, clientDisplay?.MinSide, aspectRatio);
				return _builder.Build(_mediaProcessor.GetVideoPreviewName(fileName), imageSize, imageSize);
			default:
				throw new NotImplementedException();
		}
	}
}

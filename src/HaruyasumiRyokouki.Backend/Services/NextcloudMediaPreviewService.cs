using HaruyasumiRyokouki.Backend.Common.Options;
using HaruyasumiRyokouki.Backend.Models.InternalDtos;
using HaruyasumiRyokouki.Backend.Models.InternalDtos.Enums;
using HaruyasumiRyokouki.Backend.Services.Builders;
using HaruyasumiRyokouki.Backend.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace HaruyasumiRyokouki.Backend.Services;

internal class NextcloudMediaPreviewService : IMediaPreviewService
{
	private readonly MediaPreviewOptions _options;
	private readonly IMediaProcessorService _mediaProcessor;
	private readonly IMediaResolutionCalculationService _resolutionCalculator;
	private readonly NextcloudPreviewUrlBuilder _builder;
	private readonly OriginStorageUrlBuilder _originBuilder;

	public NextcloudMediaPreviewService(IOptions<MediaPreviewOptions> options, IMediaResolutionCalculationService resolutionCalculator, IMediaProcessorService mediaProcessor)
	{
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

	public string GetImageUrl(string fileName, ImageUrlType linkType, ClientDisplay? clientDisplay = null)
	{
		int imageSize;
		switch (linkType)
		{
			case ImageUrlType.Download:
				return _originBuilder.Build(fileName);
			case ImageUrlType.FullScreen:
			case ImageUrlType.Preview:
				imageSize = _resolutionCalculator.GetResolution(linkType, clientDisplay?.Dpr, clientDisplay?.MinSide);
				return _builder.Build(fileName, imageSize, imageSize);
			default:
				throw new NotImplementedException();
		}
	}

	public string GetVideoUrl(string fileName, VideoUrlType linkType, ClientDisplay? clientDisplay = null)
	{
		int imageSize;
		switch (linkType)
		{
			case VideoUrlType.Download:
				return _originBuilder.Build(fileName);
			case VideoUrlType.Stream:
				return _originBuilder.Build(_mediaProcessor.GetVideoWebName(fileName));
			case VideoUrlType.Preview:
				imageSize = _resolutionCalculator.GetResolution(ImageUrlType.Preview, clientDisplay?.Dpr, clientDisplay?.MinSide);
				return _builder.Build(_mediaProcessor.GetVideoPreviewName(fileName), imageSize, imageSize);
			default:
				throw new NotImplementedException();
		}
	}
}

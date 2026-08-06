using HaruyasumiRyokouki.Backend.Common.Options;
using HaruyasumiRyokouki.Backend.Models.InternalDtos;
using HaruyasumiRyokouki.Backend.Models.InternalDtos.Enums;
using HaruyasumiRyokouki.Backend.Services.Builders;
using HaruyasumiRyokouki.Backend.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace HaruyasumiRyokouki.Backend.Services;

internal class ImgproxyMediaPreviewService : IMediaPreviewService
{
	private readonly MediaPreviewOptions _options;
	private readonly IMediaProcessorService _mediaProcessor;
	private readonly IMediaResolutionCalculationService _resolutionCalculator;
	private readonly ImgproxyPreviewUrlBuilder _builder;
	private readonly OriginStorageUrlBuilder _originBuilder;

	public ImgproxyMediaPreviewService(IOptions<MediaPreviewOptions> options, IMediaResolutionCalculationService resolutionCalculator, IMediaProcessorService mediaProcessor)
	{
		_options = options.Value;
		_mediaProcessor = mediaProcessor;
		_resolutionCalculator = resolutionCalculator;
		_builder = new ImgproxyPreviewUrlBuilder
		(
			_options.Imgproxy?.PublicPreviewBase ?? "",
			_options.Imgproxy?.Insecure ?? false,
			_options.Imgproxy?.Key ?? "",
			_options.Imgproxy?.Salt ?? ""
		);
		_originBuilder = new OriginStorageUrlBuilder(_options.OriginStorageBase);

		if (!_builder.Validate(out var _))
			throw new ArgumentException("Imgproxy base url is invalid", nameof(options));
		if (!_originBuilder.Validate(out var _))
			throw new ArgumentException("Video storage base url is invalid", nameof(options));
	}

	public string GetImageUrl(string fileName, ImageUrlType linkType, ClientDisplay? clientDisplay = null)
	{
		int imageSize;
		switch (linkType)
		{
			case ImageUrlType.Original:
				return _builder.BuildRaw(fileName);
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

using HaruyasumiRyokouki.Backend.Common.Options;
using HaruyasumiRyokouki.Backend.Models.InternalDtos.Enums;
using HaruyasumiRyokouki.Backend.Services.Builders;
using HaruyasumiRyokouki.Backend.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace HaruyasumiRyokouki.Backend.Services;

internal class NextcloudMediaPreviewService : IMediaPreviewService
{
	private readonly MediaPreviewOptions _options;
	private readonly IMediaProcessorService _mediaProcessor;
	private readonly NextcloudPreviewUrlBuilder _builder;
	private readonly OriginStorageUrlBuilder _originBuilder;

	public NextcloudMediaPreviewService(IOptions<MediaPreviewOptions> options, IMediaProcessorService mediaProcessor)
	{
		_options = options.Value;
		_mediaProcessor = mediaProcessor;
		_builder = new NextcloudPreviewUrlBuilder(_options.Nextcloud?.PublicPreviewBase ?? "");
		_originBuilder = new OriginStorageUrlBuilder(_options.OriginStorageBase);

		if (!_builder.Validate(out var _))
			throw new ArgumentException("Nextcloud base url is invalid", nameof(options));
		if (!_originBuilder.Validate(out var _))
			throw new ArgumentException("Video storage base url is invalid", nameof(options));
	}

	public string GetImageUrl(string fileName, ImageUrlType linkType)
	{
		switch (linkType)
		{
			case ImageUrlType.Original:
				return _originBuilder.Build(fileName);
			case ImageUrlType.FullScreen:
				return _originBuilder.Build(fileName);
			case ImageUrlType.Preview:
				return _builder.Build(fileName, 256, 256);
			case ImageUrlType.MobileFullScreen:
				return _builder.Build(fileName, 1024, 1024);
			case ImageUrlType.MobilePreview:
				return _builder.Build(fileName, 1024, 1024);
			default:
				throw new NotImplementedException();
		}
	}

	public string GetVideoUrl(string fileName, VideoUrlType linkType)
	{
		switch (linkType)
		{
			case VideoUrlType.Download:
				return _originBuilder.Build(fileName);
			case VideoUrlType.Stream:
				return _originBuilder.Build(_mediaProcessor.GetVideoWebName(fileName));
			case VideoUrlType.Preview:
				return _builder.Build(_mediaProcessor.GetVideoPreviewName(fileName), 256, 256);
			case VideoUrlType.MobilePreview:
				return _builder.Build(_mediaProcessor.GetVideoPreviewName(fileName), 1024, 1024);
			default:
				throw new NotImplementedException();
		}
	}
}

using HaruyasumiRyokouki.Backend.Common.Options;
using HaruyasumiRyokouki.Backend.Models.InternalDtos;
using HaruyasumiRyokouki.Backend.Models.InternalDtos.Enums;
using HaruyasumiRyokouki.Backend.Services.Builders;
using HaruyasumiRyokouki.Backend.Services.Interfaces;
using ImgProxy;
using Microsoft.Extensions.Options;
using static HaruyasumiRyokouki.Backend.Common.Options.MediaPreviewOptions;

namespace HaruyasumiRyokouki.Backend.Services;

internal class ImgproxyMediaPreviewService : IMediaPreviewService
{
	private readonly ImgproxyOptions _options;
	private readonly IMediaProcessorService _mediaProcessor;
	private readonly IMediaResolutionCalculationService _resolutionCalculator;
	private readonly OriginStorageUrlBuilder _originBuilder;

	private ImgProxyBuilder Builder => ImgProxyBuilder.New
		.WithEndpoint(_options.Endpoint)
		.WithCredentials(_options.Key, _options.Salt);

	public ImgproxyMediaPreviewService(IOptions<MediaPreviewOptions> options, IMediaResolutionCalculationService resolutionCalculator, IMediaProcessorService mediaProcessor)
	{
		_options = options.Value.Imgproxy 
			?? throw new OptionsValidationException
			(
				nameof(MediaPreviewOptions), 
				typeof(MediaPreviewOptions), 
				["Imgproxy option is null."]
			);
		_mediaProcessor = mediaProcessor;
		_resolutionCalculator = resolutionCalculator;

		_originBuilder = new OriginStorageUrlBuilder(options.Value.OriginStorageBase);
		if (!_originBuilder.Validate(out var _))
			throw new ArgumentException("Video storage base url is invalid", nameof(options));
	}

	public string GetImageUrl(string fileName, ImageUrlType linkType, ClientDisplay? clientDisplay = null, float? aspectRatio = default)
	{
		int imageSize;
		switch (linkType)
		{
			case ImageUrlType.Download:
				return BuildImgproxyRawString(fileName, download: true);
			case ImageUrlType.Original:
				return BuildImgproxyRawString(fileName);
			case ImageUrlType.FullScreen:
			case ImageUrlType.Preview:
				imageSize = _resolutionCalculator.GetResolution(linkType, clientDisplay?.Dpr, clientDisplay?.MinSide, aspectRatio);
				return BuildImgproxyString(fileName, imageSize, imageSize);
			default:
				throw new NotImplementedException();
		}
	}

	public string GetVideoUrl(string fileName, VideoUrlType linkType, ClientDisplay? clientDisplay = null, float? aspectRatio = default)
	{
		int imageSize;
		switch (linkType)
		{
			case VideoUrlType.Download:
				return _originBuilder.Build(fileName);
			case VideoUrlType.Stream:
				return _originBuilder.Build(_mediaProcessor.GetVideoWebName(fileName));
			case VideoUrlType.Preview:
				imageSize = _resolutionCalculator.GetResolution(ImageUrlType.Preview, clientDisplay?.Dpr, clientDisplay?.MinSide, aspectRatio);
				return BuildImgproxyString(_mediaProcessor.GetVideoPreviewName(fileName), imageSize, imageSize);
			default:
				throw new NotImplementedException();
		}
	}

	public string BuildImgproxyString(string fileName, int xAxis, int yAxis, bool download = false)
	{
		List<ImgProxyOption> options = [new ResizeOption(xAxis, yAxis)];
		if (download)
			options.Add(new AttachmentOption(download));
		return Builder.WithOptions(options.ToArray()).Build(CreateFilePath(fileName));
	}

	public string BuildImgproxyRawString(string fileName, bool download = false)
	{
		var builder = Builder.WithRaw();
		if (download)
			builder.WithOptions(new AttachmentOption(download));
		return builder.Build(CreateFilePath(fileName));
	}

	private string CreateFilePath(string fileName)
	{
		return Path.Combine(_options.FilePath, fileName);
	}

	public class ResizeOption : ImgProxyOption
	{
		private int _xAxis;
		private int _yAxis;

		public ResizeOption(int xAxis, int yAxis)
		{
			_xAxis = xAxis;
			_yAxis = yAxis;
		}

		public override string ToString()
		{
			return $"rs:fit:{_xAxis}:{_yAxis}";
		}
	}

	public class AttachmentOption : ImgProxyOption
	{
		private bool _attachment;

		public AttachmentOption(bool attachment = true)
		{
			_attachment = attachment;
		}

		public override string ToString()
		{
			return "att:" + (_attachment ? 1 : 0);
		}
	}
}

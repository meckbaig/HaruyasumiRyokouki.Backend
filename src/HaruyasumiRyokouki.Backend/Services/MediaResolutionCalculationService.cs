using HaruyasumiRyokouki.Backend.Common.Options;
using HaruyasumiRyokouki.Backend.Models.InternalDtos.Enums;
using HaruyasumiRyokouki.Backend.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace HaruyasumiRyokouki.Backend.Services;

internal class MediaResolutionCalculationService : IMediaResolutionCalculationService
{
	private readonly MediaSizesOptions _options;

	private const int DefaultAspectRatio = 4 / 3;

	public MediaResolutionCalculationService(IOptions<MediaSizesOptions> options)
	{
		_options = options.Value;
	}

	public int GetResolution(ImageUrlType linkType, double? dpr, int? resolution)
	{
		return GetNearestResolution(CalculateSize(linkType, dpr, resolution));
	}

	public int CalculateSize(ImageUrlType linkType, double? dpr, int? resolution)
	{
		dpr ??= 1;
		resolution ??= _options.DefaultScreenResolution;
		return linkType switch
		{
			ImageUrlType.FullScreen => (int)(DefaultAspectRatio * dpr.Value * resolution.Value),
			ImageUrlType.Preview => (int)(DefaultAspectRatio * dpr.Value * _options.PreviewTargetCss),
			_ => throw new NotImplementedException(),
		};
	}

	public int GetNearestResolution(int calculatedSize)
	{
		return _options.SizeBuckets.FirstOrDefault(s => s >= calculatedSize) switch
		{
			> 0 and var result => result,
			_ => _options.SizeBuckets.Last()
		};
	}
}

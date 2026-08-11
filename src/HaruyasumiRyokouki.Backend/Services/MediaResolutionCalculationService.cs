using HaruyasumiRyokouki.Backend.Common.Options;
using HaruyasumiRyokouki.Backend.Models.InternalDtos.Enums;
using HaruyasumiRyokouki.Backend.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace HaruyasumiRyokouki.Backend.Services;

internal class MediaResolutionCalculationService : IMediaResolutionCalculationService
{
	private readonly MediaSizesOptions _options;

	public MediaResolutionCalculationService(IOptions<MediaSizesOptions> options)
	{
		_options = options.Value;
	}

	public int GetResolution(ImageUrlType linkType, float? dpr, int? resolution, float? aspectRatio)
	{
		dpr ??= 1;
		resolution ??= _options.DefaultScreenResolution;
		float ratioMultiplier = NormalizeAspectRatioMultiplier(aspectRatio ?? _options.DefaultAspectRatio);
		int minimalResolution = linkType switch
		{
			ImageUrlType.FullScreen => (int)(dpr.Value * resolution.Value),
			ImageUrlType.Preview => (int)(dpr.Value * _options.PreviewTargetCss),
			_ => throw new NotImplementedException(),
		};
		int nearestResolution = GetNearestResolution(minimalResolution);
		return (int)(nearestResolution * ratioMultiplier); 
	}

	private int GetNearestResolution(int calculatedSize)
	{
		return _options.SizeBuckets.FirstOrDefault(s => s >= calculatedSize) switch
		{
			> 0 and var result => result,
			_ => _options.SizeBuckets.Last()
		};
	}

	private float NormalizeAspectRatioMultiplier(float aspectRatio)
	{
		return aspectRatio > 1 ? aspectRatio : (float)(1 / aspectRatio);
	}
}

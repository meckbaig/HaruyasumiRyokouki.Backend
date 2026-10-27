using HaruyasumiRyokouki.Backend.Common.Options;
using HaruyasumiRyokouki.Backend.Models.InternalDtos.Enums;
using HaruyasumiRyokouki.Backend.Services;
using Microsoft.Extensions.Options;

namespace HaruyasumiRyokouki.Backend.UnitTests.Services;

public class MediaResolutionCalculationServiceTests
{
	private readonly MediaResolutionCalculationService _service = new(Options.Create(new MediaSizesOptions
	{
		// Unordered on purpose: the options sort the buckets.
		SizeBuckets = [1600, 400, 2400, 800, 1200],
		PreviewTargetCss = 300,
		DefaultScreenResolution = 1080,
		DefaultAspectRatio = 4f / 3f
	}));

	[Theory]
	[InlineData(1f, 1000, 1200)]
	[InlineData(2f, 1000, 2400)]
	[InlineData(1f, 800, 800)]
	[InlineData(1.25f, 640, 800)]
	public void FullScreen_RoundsUpScreenSizeTimesDprToBucket(float dpr, int resolution, int expected)
	{
		Assert.Equal(expected, _service.GetResolution(ImageUrlType.FullScreen, dpr, resolution, aspectRatio: 1f));
	}

	[Theory]
	[InlineData(1f, 400)]
	[InlineData(1.5f, 800)]
	[InlineData(3f, 1200)]
	public void Preview_RoundsUpTargetCssTimesDprToBucket(float dpr, int expected)
	{
		Assert.Equal(expected, _service.GetResolution(ImageUrlType.Preview, dpr, resolution: 5000, aspectRatio: 1f));
	}

	[Fact]
	public void AboveLargestBucket_ReturnsLargestBucket()
	{
		Assert.Equal(2400, _service.GetResolution(ImageUrlType.FullScreen, 2f, 3000, aspectRatio: 1f));
	}

	[Fact]
	public void NoClientDisplayAndAspectRatio_UsesDefaults()
	{
		// 1080 * 1 → bucket 1200, multiplied by the default aspect ratio 4/3.
		Assert.Equal(1600, _service.GetResolution(ImageUrlType.FullScreen, null, null, null));
	}

	[Theory]
	[InlineData(1.5f)]
	[InlineData(2f / 3f)]
	public void AspectRatio_LandscapeAndPortraitGiveSameMultiplier(float aspectRatio)
	{
		Assert.Equal(1800, _service.GetResolution(ImageUrlType.FullScreen, 1f, 1000, aspectRatio));
	}
}

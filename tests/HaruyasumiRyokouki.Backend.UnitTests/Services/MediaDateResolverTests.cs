using HaruyasumiRyokouki.Backend.Common.Options;
using HaruyasumiRyokouki.Backend.Services;
using Microsoft.Extensions.Options;
using static HaruyasumiRyokouki.Backend.Common.Options.MediaFormatOptions;

namespace HaruyasumiRyokouki.Backend.UnitTests.Services;

public class MediaDateResolverTests
{
	private static MediaDateResolver CreateResolver(params TravelDateOffset[] offsets)
	{
		return new MediaDateResolver(Options.Create(new MediaFormatOptions
		{
			VideoPreset = "video",
			VideoThumbnailPreset = "thumbnail",
			ImagePreset = "webp",
			MiniaturePreset = "webp",
			MiniatureSize = 16,
			FavoritesReturnCount = 15,
			TravelDateOffsets = offsets
		}));
	}

	[Theory]
	[InlineData("20260301_120000.jpg")]
	[InlineData("IMG_20260301_120000.jpg")]
	[InlineData("VID_20260301_120000.mp4")]
	[InlineData("2026-03-01 12.00.00.jpg")]
	[InlineData("PXL_20260301_120000123.jpg")]
	[InlineData("20260301120000.heic")]
	public void TryExtractLocalFromName_NameWithTimestamp_ReturnsTimestamp(string fileName)
	{
		var date = CreateResolver().TryExtractLocalFromName(fileName);

		Assert.Equal(new DateTime(2026, 3, 1, 12, 0, 0), date);
		Assert.Equal(DateTimeKind.Unspecified, date!.Value.Kind);
	}

	[Theory]
	[InlineData("20261340_120000.jpg")]
	[InlineData("20260301_250000.jpg")]
	[InlineData("DSC01234.jpg")]
	[InlineData("photo.jpg")]
	public void TryExtractLocalFromName_NoValidTimestamp_ReturnsNull(string fileName)
	{
		Assert.Null(CreateResolver().TryExtractLocalFromName(fileName));
	}

	[Fact]
	public void TryExtractLocalFromName_IgnoresDateInExtension()
	{
		Assert.Null(CreateResolver().TryExtractLocalFromName("clip.20260301120000"));
	}

	private static readonly TravelDateOffset Japan = new()
	{
		From = new DateTime(2026, 2, 26, 0, 0, 0),
		To = new DateTime(2026, 5, 26, 4, 59, 59),
		Offset = 9
	};

	private static readonly TravelDateOffset China = new()
	{
		From = new DateTime(2026, 5, 26, 5, 0, 0),
		To = new DateTime(2026, 5, 27, 23, 59, 59),
		Offset = 8
	};

	[Fact]
	public void ResolveFromUtc_InsideRange_AppliesItsOffset()
	{
		var resolver = CreateResolver(Japan, China);

		Assert.Equal(new DateTime(2026, 3, 1, 12, 0, 0), resolver.ResolveFromUtc(new DateTime(2026, 3, 1, 3, 0, 0)).Value);
		Assert.Equal(new DateTime(2026, 5, 26, 18, 0, 0), resolver.ResolveFromUtc(new DateTime(2026, 5, 26, 10, 0, 0)).Value);
	}

	[Fact]
	public void ResolveFromUtc_ReturnsUnspecifiedKind()
	{
		var result = CreateResolver(Japan).ResolveFromUtc(new DateTime(2026, 3, 1, 3, 0, 0, DateTimeKind.Utc));

		Assert.Equal(DateTimeKind.Unspecified, result.Value.Kind);
	}

	[Fact]
	public void ResolveFromUtc_RangeBoundaries_AreInclusive()
	{
		var resolver = CreateResolver(Japan, China);

		Assert.Equal(Japan.From!.Value.AddHours(9), resolver.ResolveFromUtc(Japan.From.Value).Value);
		Assert.Equal(Japan.To!.Value.AddHours(9), resolver.ResolveFromUtc(Japan.To.Value).Value);
		Assert.Equal(China.From!.Value.AddHours(8), resolver.ResolveFromUtc(China.From.Value).Value);
	}

	[Theory]
	[InlineData("2026-02-25T23:59:59")]
	[InlineData("2026-05-28T00:00:00")]
	public void ResolveFromUtc_OutsideAllRanges_Fails(string utc)
	{
		var result = CreateResolver(Japan, China).ResolveFromUtc(DateTime.Parse(utc));

		Assert.True(result.IsFailure);
	}

	[Fact]
	public void ResolveFromUtc_OpenRanges_MatchAnyDateOnTheOpenSide()
	{
		var resolver = CreateResolver(
			new TravelDateOffset { To = new DateTime(2026, 1, 1), Offset = 5 },
			new TravelDateOffset { From = new DateTime(2026, 1, 1, 0, 0, 1), Offset = 9 });

		Assert.Equal(new DateTime(2000, 1, 1, 5, 0, 0), resolver.ResolveFromUtc(new DateTime(2000, 1, 1)).Value);
		Assert.Equal(new DateTime(2100, 1, 1, 9, 0, 0), resolver.ResolveFromUtc(new DateTime(2100, 1, 1)).Value);
	}
}

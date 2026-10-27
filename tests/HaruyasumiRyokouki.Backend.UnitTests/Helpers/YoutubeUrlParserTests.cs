using HaruyasumiRyokouki.Backend.Common.Helpers;

namespace HaruyasumiRyokouki.Backend.UnitTests.Helpers;

public class YoutubeUrlParserTests
{
	private const string VideoId = "dQw4w9WgXcQ";

	[Theory]
	[InlineData("https://www.youtube.com/watch?v=dQw4w9WgXcQ")]
	[InlineData("https://youtube.com/watch?v=dQw4w9WgXcQ&t=42s")]
	[InlineData("https://www.youtube.com/watch?feature=share&v=dQw4w9WgXcQ")]
	[InlineData("https://m.youtube.com/watch?v=dQw4w9WgXcQ")]
	[InlineData("https://youtu.be/dQw4w9WgXcQ")]
	[InlineData("https://youtu.be/dQw4w9WgXcQ?si=abcdef")]
	[InlineData("https://www.youtube.com/shorts/dQw4w9WgXcQ")]
	[InlineData("https://www.youtube.com/embed/dQw4w9WgXcQ")]
	[InlineData("https://www.youtube.com/v/dQw4w9WgXcQ")]
	[InlineData("HTTPS://WWW.YOUTUBE.COM/watch?v=dQw4w9WgXcQ")]
	[InlineData("  https://youtu.be/dQw4w9WgXcQ  ")]
	[InlineData("dQw4w9WgXcQ")]
	public void TryExtractVideoId_SupportedFormats_ReturnsId(string url)
	{
		Assert.True(YoutubeUrlParser.TryExtractVideoId(url, out var videoId));
		Assert.Equal(VideoId, videoId);
	}

	[Theory]
	[InlineData("https://example.com/watch?v=dQw4w9WgXcQ")]
	[InlineData("https://www.youtube.com/watch?v=short")]
	[InlineData("https://www.youtube.com/channel/UCuAXFkgsw1L7xaCfnd5JJOw")]
	[InlineData("dQw4w9WgXc")]
	[InlineData("dQw4w9WgXcQ!")]
	[InlineData("")]
	[InlineData("   ")]
	[InlineData(null)]
	public void TryExtractVideoId_InvalidInput_ReturnsFalse(string? url)
	{
		Assert.False(YoutubeUrlParser.TryExtractVideoId(url!, out var videoId));
		Assert.Empty(videoId);
	}
}

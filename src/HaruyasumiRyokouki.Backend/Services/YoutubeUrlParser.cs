using YoutubeExplode.Videos;

namespace HaruyasumiRyokouki.Backend.Services;

internal static class YoutubeUrlParser
{
	public static bool TryExtractVideoId(string url, out string videoId)
	{
		var id = VideoId.TryParse(url);
		if (id is null)
		{
			videoId = string.Empty;
			return false;
		}

		videoId = id.Value.Value;
		return true;
	}
}

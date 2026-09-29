using System.Text.RegularExpressions;

namespace HaruyasumiRyokouki.Backend.Services;

internal static class YoutubeUrlParser
{
	private static readonly Regex UrlRegex = new(
		@"(?:youtu\.be/|youtube\.com/(?:watch\?(?:.*&)?v=|embed/|shorts/|v/))(?<id>[A-Za-z0-9_-]{11})",
		RegexOptions.Compiled | RegexOptions.IgnoreCase);

	private static readonly Regex BareIdRegex = new(
		@"^[A-Za-z0-9_-]{11}$",
		RegexOptions.Compiled);

	public static bool TryExtractVideoId(string url, out string videoId)
	{
		if (!string.IsNullOrWhiteSpace(url))
		{
			var trimmed = url.Trim();

			if (BareIdRegex.IsMatch(trimmed))
			{
				videoId = trimmed;
				return true;
			}

			var match = UrlRegex.Match(trimmed);
			if (match.Success)
			{
				videoId = match.Groups["id"].Value;
				return true;
			}
		}

		videoId = string.Empty;
		return false;
	}
}

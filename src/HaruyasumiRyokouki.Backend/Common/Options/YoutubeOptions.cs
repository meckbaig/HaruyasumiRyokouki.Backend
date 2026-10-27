namespace HaruyasumiRyokouki.Backend.Common.Options;

/// <summary>
/// Options of the <c>YouTube</c> configuration section; see the Configuration section of README.
/// </summary>
sealed class YoutubeOptions
{
	public const string ConfigurationSectionName = "YouTube";

	/// <summary>
	/// A proxy is needed to bypass server blocking when scraping YouTube through the library.
	/// </summary>
	public YoutubeProxyOptions? Proxy { get; set; }

	/// <summary>
	/// Proxy for yt-dlp.
	/// </summary>
	public sealed class YoutubeProxyOptions
	{
		/// <summary>
		/// Pass the proxy to yt-dlp.
		/// </summary>
		public bool Enabled { get; set; }

		/// <summary>
		/// Proxy URL, e.g. <c>socks5://host:1080</c>.
		/// </summary>
		public string? Address { get; set; }
	}
}

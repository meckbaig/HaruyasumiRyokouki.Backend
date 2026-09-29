namespace HaruyasumiRyokouki.Backend.Common.Options;

sealed class YoutubeOptions
{
	public const string ConfigurationSectionName = "YouTube";

	/// <summary>
	/// A proxy is needed to bypass server blocking when scraping YouTube through the library.
	/// </summary>
	public YoutubeProxyOptions? Proxy { get; set; }

	public sealed class YoutubeProxyOptions
	{
		public bool Enabled { get; set; }
		public string? Address { get; set; }
	}
}

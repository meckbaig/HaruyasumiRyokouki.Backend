namespace HaruyasumiRyokouki.Backend.Common.Options;

sealed class MediaPreviewOptions
{
	public const string ConfigurationSectionName = "MediaPreview";

	public required MediaPreviewProvider Provider { get; set; }

	public OriginStorageOptions OriginStorage { get; set; }

	public ImgproxyOptions? Imgproxy { get; set; }

	public NextcloudOptions? Nextcloud { get; set; }

	internal enum MediaPreviewProvider
	{
		Nextcloud,
		Imgproxy
	}

	public class ImgproxyOptions
	{
		public string Endpoint { get; set; }
		public string FilePath { get; set; }
		public string? Key { get; set; }
		public string? Salt { get; set; }
		public bool Insecure { get; set; }

		public string? CdnEndpoint { get; set; }
		public bool UseCdnForDownloads { get; set; }
	}

	public class NextcloudOptions
	{
		public string PayloadStringBase { get; set; }
		public string Endpoint { get; set; }
		public string? CdnEndpoint { get; set; }
		public bool UseCdnForDownloads { get; set; }
	}

	public class OriginStorageOptions
	{
		public string PayloadStringBase { get; set; }
		public string Endpoint { get; set; }
		public string? CdnEndpoint { get; set; }
		public bool UseCdnForDownloads { get; set; }
	}
}

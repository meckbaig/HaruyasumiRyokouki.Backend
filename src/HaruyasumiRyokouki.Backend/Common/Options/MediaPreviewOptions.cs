namespace HaruyasumiRyokouki.Backend.Common.Options;

sealed class MediaPreviewOptions
{
	public const string ConfigurationSectionName = "MediaPreview";

	public required MediaPreviewProvider Provider { get; set; }

	public string OriginStorageBase { get; set; }

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
	}

	public class NextcloudOptions
	{
		public string PublicPreviewBase { get; set; }
	}
}

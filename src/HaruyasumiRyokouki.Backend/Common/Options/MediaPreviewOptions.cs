namespace HaruyasumiRyokouki.Backend.Common.Options;

/// <summary>
/// Options of the <c>MediaPreview</c> configuration section.
/// </summary>
sealed class MediaPreviewOptions
{
	public const string ConfigurationSectionName = "MediaPreview";

	/// <summary>
	/// Preview service used for resized images.
	/// </summary>
	public required MediaPreviewProvider Provider { get; set; }

	/// <summary>
	/// URLs of the original files, for downloads and video streams.
	/// </summary>
	public required OriginStorageOptions OriginStorage { get; set; }

	/// <summary>
	/// imgproxy settings. Required when <see cref="Provider"/> is <see cref="MediaPreviewProvider.Imgproxy"/>.
	/// </summary>
	public ImgproxyOptions? Imgproxy { get; set; }

	/// <summary>
	/// Nextcloud preview settings. Required when <see cref="Provider"/> is <see cref="MediaPreviewProvider.Nextcloud"/>.
	/// </summary>
	public NextcloudOptions? Nextcloud { get; set; }

	/// <summary>
	/// Preview service.
	/// </summary>
	internal enum MediaPreviewProvider
	{
		Nextcloud,
		Imgproxy
	}

	/// <summary>
	/// imgproxy settings.
	/// </summary>
	public class ImgproxyOptions
	{
		/// <summary>
		/// Base URL of imgproxy.
		/// </summary>
		public required string Endpoint { get; set; }

		/// <summary>
		/// Source prefix imgproxy reads the original files from, e.g. <c>local:///</c>.
		/// </summary>
		public required string FilePath { get; set; }

		/// <summary>
		/// Hex-encoded signing key. May be empty only with <see cref="Insecure"/>.
		/// </summary>
		public string? Key { get; set; }

		/// <summary>
		/// Hex-encoded signing salt. May be empty only with <see cref="Insecure"/>.
		/// </summary>
		public string? Salt { get; set; }

		/// <summary>
		/// Allow unsigned URLs.
		/// </summary>
		public bool Insecure { get; set; }

		/// <summary>
		/// Base URL of a CDN in front of imgproxy. When set, it replaces <see cref="Endpoint"/> in media URLs.
		/// </summary>
		public string? CdnEndpoint { get; set; }

		/// <summary>
		/// Use <c>CdnEndpoint</c> for download URLs too.
		/// </summary>
		public bool UseCdnForDownloads { get; set; }
	}

	/// <summary>
	/// Nextcloud public preview settings.
	/// </summary>
	public class NextcloudOptions
	{
		/// <summary>
		/// Path template after the endpoint, with <c>{fileName}</c>, <c>{xAxis}</c> and <c>{yAxis}</c> tokens.
		/// </summary>
		public required string PayloadStringBase { get; set; }

		/// <summary>
		/// Base URL of the server.
		/// </summary>
		public required string Endpoint { get; set; }

		/// <summary>
		/// Base URL of a CDN in front of the server. When set, it replaces <see cref="Endpoint"/> in media URLs.
		/// </summary>
		public string? CdnEndpoint { get; set; }

		/// <summary>
		/// Use <c>CdnEndpoint</c> for download URLs too.
		/// </summary>
		public bool UseCdnForDownloads { get; set; }
	}

	/// <summary>
	/// URLs of the original files.
	/// </summary>
	public class OriginStorageOptions
	{
		/// <summary>
		/// Path template after the endpoint, with the <c>{fileName}</c> token.
		/// </summary>
		public required string PayloadStringBase { get; set; }

		/// <summary>
		/// Base URL of the server.
		/// </summary>
		public required string Endpoint { get; set; }

		/// <summary>
		/// Base URL of a CDN in front of the server. When set, it replaces <see cref="Endpoint"/> in media URLs.
		/// </summary>
		public string? CdnEndpoint { get; set; }

		/// <summary>
		/// Use <c>CdnEndpoint</c> for download URLs too.
		/// </summary>
		public bool UseCdnForDownloads { get; set; }
	}
}

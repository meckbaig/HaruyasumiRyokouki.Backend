namespace HaruyasumiRyokouki.Backend.Common.Options;

sealed class MediaStorageOptions
{
	public const string ConfigurationSectionName = "MediaStorage";

	public required FileStorageProvider Provider { get; set; }

	/// <summary>
	/// Base URL for the public preview of the WebDAV server.
	/// </summary>
	public required string PublicPreviewBase { get; set; }

	internal enum FileStorageProvider
	{
		Local,
		WebDav
	}
}

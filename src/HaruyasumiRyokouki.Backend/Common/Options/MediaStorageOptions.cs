namespace HaruyasumiRyokouki.Backend.Common.Options;

/// <summary>
/// Options of the <c>MediaStorage</c> configuration section; see the Configuration section of README.
/// </summary>
sealed class MediaStorageOptions
{
	public const string ConfigurationSectionName = "MediaStorage";

	/// <summary>
	/// File storage of the media.
	/// </summary>
	public required FileStorageProvider Provider { get; set; }

	/// <summary>
	/// File storage.
	/// </summary>
	internal enum FileStorageProvider
	{
		Local,
		WebDav
	}
}

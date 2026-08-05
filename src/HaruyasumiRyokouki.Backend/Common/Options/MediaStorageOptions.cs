namespace HaruyasumiRyokouki.Backend.Common.Options;

sealed class MediaStorageOptions
{
	public const string ConfigurationSectionName = "MediaStorage";

	public required FileStorageProvider Provider { get; set; }

	internal enum FileStorageProvider
	{
		Local,
		WebDav
	}
}

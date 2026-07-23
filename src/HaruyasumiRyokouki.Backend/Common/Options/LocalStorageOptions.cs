namespace HaruyasumiRyokouki.Backend.Common.Options;

sealed class LocalStorageOptions
{
	public const string ConfigurationSectionName = "LocalStorageProvider";

	/// <summary>
	/// Local folder path.
	/// </summary>
	public required string Path { get; set; }
}

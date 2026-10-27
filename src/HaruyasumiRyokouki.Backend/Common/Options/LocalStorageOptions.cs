namespace HaruyasumiRyokouki.Backend.Common.Options;

/// <summary>
/// Options of the <c>LocalStorageProvider</c> configuration section; see the Configuration section of README.
/// </summary>
sealed class LocalStorageOptions
{
	public const string ConfigurationSectionName = "LocalStorageProvider";

	/// <summary>
	/// Local folder path.
	/// </summary>
	public required string Path { get; set; }
}

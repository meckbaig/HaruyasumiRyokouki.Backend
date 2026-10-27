namespace HaruyasumiRyokouki.Backend.Common.Options;

/// <summary>
/// Options of the <c>ConnectionStrings</c> configuration section; see the Configuration section of README.
/// </summary>
sealed class ConnectionStringsOptions
{
	public const string ConfigurationSectionName = "ConnectionStrings";

	/// <summary>
	/// Database connection string for the application.
	/// </summary>
	public required string Default { get; set; }
}

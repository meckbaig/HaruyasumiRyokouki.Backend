namespace HaruyasumiRyokouki.Backend.Common.Options;

sealed class WebDavOptions
{
	public const string ConfigurationSectionName = "WebDavStorageProvider";

	/// <summary>
	/// Endpoint URL for the WebDAV server.
	/// </summary>
	public required string Endpoint { get; set; }

	/// <summary>
	/// Username for authenticating with the WebDAV server.
	/// </summary>
	public required string Username { get; set; }

	/// <summary>
	/// Password for authenticating with the WebDAV server.
	/// </summary>
	public required string Password { get; set; }
}

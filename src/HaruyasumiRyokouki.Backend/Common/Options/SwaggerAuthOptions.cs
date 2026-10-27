namespace HaruyasumiRyokouki.Backend.Common.Options;

/// <summary>
/// Options of the <c>SwaggerAuth</c> configuration section; see the Configuration section of README.
/// </summary>
sealed class SwaggerAuthOptions
{
	public const string ConfigurationSectionName = "SwaggerAuth";

	/// <summary>
	/// Swagger login; required when <see cref="RequireLogin"/> is set.
	/// </summary>
	public required string Username { get; set; }

	/// <summary>
	/// Swagger password; required when <see cref="RequireLogin"/> is set.
	/// </summary>
	public required string Password { get; set; }

	/// <summary>
	/// Protect <c>/swagger</c> with HTTP Basic authentication.
	/// </summary>
	public required bool RequireLogin { get; set; }
}

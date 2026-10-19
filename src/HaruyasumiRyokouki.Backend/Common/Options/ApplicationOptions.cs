namespace HaruyasumiRyokouki.Backend.Common.Options;

sealed class ApplicationOptions
{
	public const string ConfigurationSectionName = "Application";

	/// <summary>
	/// Maximum number of retries for database connection attempts.
	/// </summary>
	public int CheckDbRetryCount { get; set; }

	/// <summary>
	/// Maximum wait time for a response from the database during retry attempts (in seconds).
	/// </summary>
	public int CheckDbRetryDelay { get; set; }

	/// <summary>
	/// List of known proxies for the application to handle forwarded headers correctly.
	/// </summary>
	public string[] KnownProxies { get; set; } = [];

	/// TODO: add checks on startup
	public string AdminLogin { get; set; } = null!;

	/// TODO: add checks on startup
	public string AdminPassword { get; set; } = null!;

	/// <summary>
	/// Number of failed Basic authentication attempts from one IP address after which
	/// further attempts from it are rejected until <see cref="FailedLoginLockoutMinutes"/> pass.
	/// </summary>
	public int MaxFailedLoginAttempts { get; set; } = 10;

	/// <summary>
	/// Time window (in minutes), counted from the first failed attempt, during which failed attempts
	/// are counted; it is also the lockout duration once <see cref="MaxFailedLoginAttempts"/> is reached.
	/// </summary>
	public int FailedLoginLockoutMinutes { get; set; } = 15;
}

namespace HaruyasumiRyokouki.Backend.Services.Interfaces;

/// <summary>
/// Counts failed login attempts per client and blocks the client after too many of them.
/// </summary>
internal interface ILoginAttemptLimiter
{
	/// <summary>
	/// Returns <see langword="true"/> if the client has reached the failed attempt limit and must be rejected.
	/// </summary>
	/// <param name="clientKey">Client identifier, e.g. scope and IP address.</param>
	bool IsBlocked(string clientKey);

	/// <summary>
	/// Registers a failed login attempt of the client.
	/// </summary>
	/// <param name="clientKey">Client identifier, e.g. scope and IP address.</param>
	void RegisterFailure(string clientKey);

	/// <summary>
	/// Clears failed attempts of the client after a successful login.
	/// </summary>
	/// <param name="clientKey">Client identifier, e.g. scope and IP address.</param>
	void Reset(string clientKey);
}

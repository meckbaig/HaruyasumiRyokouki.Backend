using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;

namespace HaruyasumiRyokouki.Backend.Common.Helpers;

/// <summary>
/// Parsing and checking of HTTP Basic authentication credentials.
/// </summary>
internal static class BasicCredentials
{
	private const string Scheme = "Basic";

	/// <summary>
	/// Extracts the username and password from an <c>Authorization: Basic ...</c> header value.
	/// </summary>
	/// <param name="authorizationHeader">Raw value of the <c>Authorization</c> header.</param>
	/// <param name="username">Username, if parsing succeeded.</param>
	/// <param name="password">Password (may contain <c>:</c>), if parsing succeeded.</param>
	/// <returns><see langword="true"/> if the header is a well-formed Basic header.</returns>
	public static bool TryParse(string? authorizationHeader, out string username, out string password)
	{
		username = string.Empty;
		password = string.Empty;

		if (!AuthenticationHeaderValue.TryParse(authorizationHeader, out var header)
			|| !header.Scheme.Equals(Scheme, StringComparison.OrdinalIgnoreCase)
			|| string.IsNullOrEmpty(header.Parameter))
		{
			return false;
		}

		string decoded;
		try
		{
			decoded = Encoding.UTF8.GetString(Convert.FromBase64String(header.Parameter));
		}
		catch (FormatException)
		{
			return false;
		}

		var parts = decoded.Split(':', 2);
		if (parts.Length != 2)
			return false;

		username = parts[0];
		password = parts[1];
		return true;
	}

	/// <summary>
	/// Compares credentials in constant time, so the response time does not reveal how much of them matched.
	/// </summary>
	public static bool AreEqual(string username, string password, string expectedUsername, string expectedPassword)
	{
		// Bitwise AND (not &&) so both comparisons always run.
		return FixedTimeEquals(username, expectedUsername) & FixedTimeEquals(password, expectedPassword);
	}

	private static bool FixedTimeEquals(string actual, string expected)
	{
		// Hashing makes both inputs the same length, so the length of the expected value does not leak either.
		var actualHash = SHA256.HashData(Encoding.UTF8.GetBytes(actual));
		var expectedHash = SHA256.HashData(Encoding.UTF8.GetBytes(expected));
		return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
	}
}

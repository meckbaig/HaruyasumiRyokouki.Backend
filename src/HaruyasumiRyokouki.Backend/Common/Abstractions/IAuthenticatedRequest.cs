namespace HaruyasumiRyokouki.Backend.Common.Abstractions;

/// <summary>
/// A request whose result depends on whether the caller is the admin.
/// </summary>
public interface IAuthenticatedRequest
{
	/// <summary>
	/// <see langword="true"/> when the request came with valid admin credentials.
	/// Set by <c>ClientAuthenticationBehavior</c> after validation; not bound from the request.
	/// </summary>
	bool IsAuthenticated { get; set; }
}

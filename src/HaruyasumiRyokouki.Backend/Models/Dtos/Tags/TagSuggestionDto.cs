namespace HaruyasumiRyokouki.Backend.Models.Dtos.Tags;

/// <summary>
/// Tag found by <c>GET /v1/tags/suggestion</c>.
/// </summary>
public record TagSuggestionDto
{
	/// <summary>
	/// Unique tag key, used in links (<c>/v1/search?tag=</c>).
	/// </summary>
	public required string Slug { get; set; }

	/// <summary>
	/// Display name in the requested language.
	/// </summary>
	public required	string Value { get; set; }

	/// <summary>
	/// Number of media with the tag visible to the caller.
	/// </summary>
	public required int UsageCount { get; set; }
}

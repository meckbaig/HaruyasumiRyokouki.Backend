namespace HaruyasumiRyokouki.Backend.Models.Dtos.Tags;

/// <summary>
/// Tag suggested while the user types a search.
/// </summary>
public record TagSuggestionDto
{
	/// <summary>
	/// Unique key of the tag in English, e.g. <c>cherry_blossom</c>.
	/// </summary>
	/// <remarks>
	/// Used in links to the tag search: <c>GET /v1/search?tag=cherry_blossom</c>.
	/// </remarks>
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

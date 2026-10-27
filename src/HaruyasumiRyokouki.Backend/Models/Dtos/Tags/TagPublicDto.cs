namespace HaruyasumiRyokouki.Backend.Models.Dtos.Tags;

/// <summary>
/// Tag of a media file with its display name in the requested language.
/// </summary>
public class TagPublicDto
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
	public required string Value { get; set; }
}

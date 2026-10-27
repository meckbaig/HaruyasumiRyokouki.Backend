namespace HaruyasumiRyokouki.Backend.Models.Dtos.Tags;

/// <summary>
/// Tag of a media file in one language.
/// </summary>
public class TagPublicDto
{
	/// <summary>
	/// Unique tag key, used in links (<c>/v1/search?tag=</c>).
	/// </summary>
	public required string Slug { get; set; }

	/// <summary>
	/// Display name in the requested language.
	/// </summary>
	public required string Value { get; set; }
}

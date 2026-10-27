namespace HaruyasumiRyokouki.Backend.Models.Dtos.Tags;

/// <summary>
/// Tag with all texts, for the admin.
/// </summary>
public record TagDto
{
	/// <summary>
	/// Tag id.
	/// </summary>
	public int Id { get; set; }

	/// <summary>
	/// Unique tag key, used in links (<c>/v1/search?tag=</c>).
	/// </summary>
	public string Slug { get; set; }

	/// <summary>
	/// Display names, at most one per language.
	/// </summary>
	public ICollection<TagTranslationDto> Translations { get; set; }

	/// <summary>
	/// Additional search texts in any supported language; not displayed.
	/// </summary>
	public ICollection<TagTranslationDto> Aliases { get; set; }

	/// <summary>
	/// Number of media with the tag.
	/// </summary>
	public int UsageCount { get; set; }
}

namespace HaruyasumiRyokouki.Backend.Models.Dtos.Tags;

/// <summary>
/// Tag with all texts, for the editor.
/// </summary>
public record TagDto
{
	/// <summary>
	/// Identifier of the tag.
	/// </summary>
	public int Id { get; set; }

	/// <summary>
	/// Unique key of the tag in English, e.g. <c>cherry_blossom</c>.
	/// </summary>
	/// <remarks>
	/// Used in links to the tag search: <c>GET /v1/search?tag=cherry_blossom</c>.
	/// </remarks>
	public required string Slug { get; set; }

	/// <summary>
	/// Display names of the tag, at most one per language.
	/// </summary>
	public required ICollection<TagTranslationDto> Translations { get; set; }

	/// <summary>
	/// Search aliases of the tag: synonyms and spelling variants in any supported language.
	/// </summary>
	/// <remarks>
	/// Aliases are never displayed. They let the tag be found by other words.
	/// </remarks>
	public required ICollection<TagTranslationDto> Aliases { get; set; }

	/// <summary>
	/// Number of media with the tag.
	/// </summary>
	public int UsageCount { get; set; }
}

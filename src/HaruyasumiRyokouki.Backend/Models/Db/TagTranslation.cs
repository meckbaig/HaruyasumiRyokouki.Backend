namespace HaruyasumiRyokouki.Backend.Models.Db;

/// <summary>
/// Text of a tag in one language: a display name or a search alias.
/// </summary>
public class TagTranslation
{
	/// <summary>
	/// Id.
	/// </summary>
	public int Id { get; set; }

	/// <summary>
	/// Tag id.
	/// </summary>
	public int TagId { get; set; }

	/// <summary>
	/// Language code: <c>en</c>, <c>ru</c> or <c>ja</c>.
	/// </summary>
	public string LanguageCode { get; set; }

	/// <summary>
	/// The text; unique within the tag.
	/// </summary>
	public string Text { get; set; }

	/// <summary>
	/// <see langword="true"/> for the display name (one per language), <see langword="false"/> for a search alias.
	/// </summary>
	public bool IsPrimary { get; set; }

	/// <summary>
	/// The tag.
	/// </summary>
	public Tag Tag { get; set; }
}

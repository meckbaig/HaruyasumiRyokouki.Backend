namespace HaruyasumiRyokouki.Backend.Models.Db;

/// <summary>
/// Text of a tag in one language: a display name or a search alias.
/// </summary>
public class TagTranslation
{
	/// <summary>
	/// Identifier of the text.
	/// </summary>
	public int Id { get; set; }

	/// <summary>
	/// Identifier of the <see cref="Db.Tag"/>.
	/// </summary>
	public int TagId { get; set; }

	/// <summary>
	/// Language of the text: <c>en</c>, <c>ru</c> or <c>ja</c>.
	/// </summary>
	/// <remarks>
	/// See <see cref="Db.Enums.LanguageCode"/>.
	/// </remarks>
	public string LanguageCode { get; set; }

	/// <summary>
	/// The text. Unique within the tag.
	/// </summary>
	public string Text { get; set; }

	/// <summary>
	/// <see langword="true"/> when the text is the display name of the tag in its language; otherwise <see langword="false"/> (a search alias).
	/// </summary>
	public bool IsPrimary { get; set; }

	/// <summary>
	/// The tag of the text.
	/// </summary>
	public Tag Tag { get; set; }
}

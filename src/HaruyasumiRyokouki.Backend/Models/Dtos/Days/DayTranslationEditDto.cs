namespace HaruyasumiRyokouki.Backend.Models.Dtos.Days;

/// <summary>
/// Diary note of a day in one language.
/// </summary>
public record DayTranslationEditDto
{
	/// <summary>
	/// Identifier of the translation.
	/// </summary>
	/// <remarks>
	/// Ignored on input.
	/// </remarks>
	public int Id { get; set; }

	/// <summary>
	/// Language of the text: <c>en</c>, <c>ru</c> or <c>ja</c>.
	/// </summary>
	/// <remarks>
	/// See <see cref="Db.Enums.LanguageCode"/>.
	/// </remarks>
	public string LanguageCode { get; set; } = null!;

	/// <summary>
	/// The author's diary note for the day. May be empty.
	/// </summary>
	public string Note { get; set; } = null!;
}

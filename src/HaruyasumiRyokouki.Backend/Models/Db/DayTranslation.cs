namespace HaruyasumiRyokouki.Backend.Models.Db;

/// <summary>
/// Diary note of a day in one language.
/// </summary>
public class DayTranslation
{
	/// <summary>
	/// Identifier of the translation.
	/// </summary>
	public int Id { get; set; }

	/// <summary>
	/// Identifier of the <see cref="Day"/>.
	/// </summary>
	public int DayId { get; set; }

	/// <summary>
	/// Language of the text: <c>en</c>, <c>ru</c> or <c>ja</c>.
	/// </summary>
	/// <remarks>
	/// See <see cref="Db.Enums.LanguageCode"/>.
	/// </remarks>
	public string LanguageCode { get; set; } = null!;

	/// <summary>
	/// The author's diary note for the day.
	/// </summary>
	public string Note { get; set; } = null!;

	/// <summary>
	/// The day of the note.
	/// </summary>
	public Day Day { get; set; }
}

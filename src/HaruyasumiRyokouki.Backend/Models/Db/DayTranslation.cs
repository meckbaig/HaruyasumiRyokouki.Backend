namespace HaruyasumiRyokouki.Backend.Models.Db;

/// <summary>
/// Note of a day in one language.
/// </summary>
public class DayTranslation
{
	/// <summary>
	/// Id.
	/// </summary>
	public int Id { get; set; }

	/// <summary>
	/// Day id.
	/// </summary>
	public int DayId { get; set; }

	/// <summary>
	/// Language code: <c>en</c>, <c>ru</c> or <c>ja</c>.
	/// </summary>
	public string LanguageCode { get; set; } = null!;

	/// <summary>
	/// Note text.
	/// </summary>
	public string Note { get; set; } = null!;

	/// <summary>
	/// The day.
	/// </summary>
	public Day Day { get; set; }
}

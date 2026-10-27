namespace HaruyasumiRyokouki.Backend.Models.Dtos.Days;

/// <summary>
/// Note of a day in one language.
/// </summary>
public record DayTranslationEditDto
{
	/// <summary>
	/// Translation id; ignored on input.
	/// </summary>
	public int Id { get; set; }

	/// <summary>
	/// Language code: <c>en</c>, <c>ru</c> or <c>ja</c>.
	/// </summary>
	public string LanguageCode { get; set; } = null!;

	/// <summary>
	/// Note text, may be empty.
	/// </summary>
	public string Note { get; set; } = null!;
}

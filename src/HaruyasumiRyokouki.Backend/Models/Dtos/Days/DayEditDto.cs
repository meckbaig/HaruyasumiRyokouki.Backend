namespace HaruyasumiRyokouki.Backend.Models.Dtos.Days;

/// <summary>
/// Day with notes in all languages, for the admin.
/// </summary>
public record DayEditDto
{
	/// <summary>
	/// Date of the day. Ignored on input: the day is taken from the route.
	/// </summary>
	public DateOnly Date { get; set; }

	/// <summary>
	/// The notes of the day are finished.
	/// </summary>
	public bool IsReady { get; set; }

	/// <summary>
	/// Notes, one item per language.
	/// </summary>
	public ICollection<DayTranslationEditDto> Translations { get; set; } = [];
}

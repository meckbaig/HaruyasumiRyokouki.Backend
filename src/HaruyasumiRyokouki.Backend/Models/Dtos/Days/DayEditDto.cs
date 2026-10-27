namespace HaruyasumiRyokouki.Backend.Models.Dtos.Days;

/// <summary>
/// Day of the trip with the diary note in all languages, for the editor.
/// </summary>
public record DayEditDto
{
	/// <summary>
	/// Date of the day in the local time of the trip.
	/// </summary>
	/// <remarks>
	/// Ignored on input: the edited day is taken from the route.
	/// </remarks>
	public DateOnly Date { get; set; }

	/// <summary>
	/// The day note and all media of the day have been reviewed by the editor.
	/// </summary>
	public bool IsReady { get; set; }

	/// <summary>
	/// Diary note of the day, at most one translation per language.
	/// </summary>
	/// <remarks>
	/// On input the list replaces all stored translations.
	/// </remarks>
	public ICollection<DayTranslationEditDto> Translations { get; set; } = [];
}

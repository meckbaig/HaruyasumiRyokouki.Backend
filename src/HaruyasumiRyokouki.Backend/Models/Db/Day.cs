namespace HaruyasumiRyokouki.Backend.Models.Db;

/// <summary>
/// Day of the trip with the author's diary note and the media taken that day.
/// </summary>
/// <remarks>
/// Days are not created directly: sync and YouTube import create the day of a new media file when it does not exist yet.
/// </remarks>
public class Day
{
	/// <summary>
	/// Identifier of the day.
	/// </summary>
	public int Id { get; set; }

	/// <summary>
	/// Date in the local time of the trip. Unique.
	/// </summary>
	public DateOnly Date { get; set; }

	/// <summary>
	/// The day note and all media of the day have been reviewed by the editor.
	/// </summary>
	public bool IsReady { get; set; }

	/// <summary>
	/// Diary note of the day, one translation per language.
	/// </summary>
	public ICollection<DayTranslation> Translations { get; set; } = []; 

	/// <summary>
	/// Photos and videos taken that day.
	/// </summary>
	public ICollection<MediaFile> Media { get; set; } = [];
}

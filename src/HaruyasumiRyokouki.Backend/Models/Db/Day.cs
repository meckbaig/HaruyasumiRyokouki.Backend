namespace HaruyasumiRyokouki.Backend.Models.Db;

/// <summary>
/// Day of the trip. Created by sync or YouTube import for the date of a new media file.
/// </summary>
public class Day
{
	/// <summary>
	/// Id.
	/// </summary>
	public int Id { get; set; }

	/// <summary>
	/// Date in the local time of the trip; unique.
	/// </summary>
	public DateOnly Date { get; set; }

	/// <summary>
	/// The notes are finished.
	/// </summary>
	public bool IsReady { get; set; }

	/// <summary>
	/// Notes, one per language.
	/// </summary>
	public ICollection<DayTranslation> Translations { get; set; } = []; 

	/// <summary>
	/// Media of the day.
	/// </summary>
	public ICollection<MediaFile> Media { get; set; } = [];
}

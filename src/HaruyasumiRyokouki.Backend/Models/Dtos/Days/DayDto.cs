using HaruyasumiRyokouki.Backend.Models.Db;
using HaruyasumiRyokouki.Backend.Models.Dtos.Media;

namespace HaruyasumiRyokouki.Backend.Models.Dtos.Days;

/// <summary>
/// Day of the trip in one language with its media.
/// </summary>
public record DayDto
{
	/// <summary>
	/// Date of the day in the local time of the trip.
	/// </summary>
	public DateOnly Date { get; set; }

	/// <summary>
	/// The day note and all media of the day have been reviewed by the editor.
	/// </summary>
	public bool IsReady { get; set; }

	/// <summary>
	/// Language of <see cref="Note"/>.
	/// </summary>
	/// <remarks>
	/// Equals the requested language, or <see langword="null"/> when the note has no translation to it.
	/// </remarks>
	public string? LanguageCode { get; set; }

	/// <summary>
	/// The author's diary note for the day in the requested language.
	/// </summary>
	public string? Note { get; set; }

	/// <summary>
	/// Media of the day visible to the caller, in the order they were taken.
	/// </summary>
	public ICollection<MediaFileDto> Media { get; set; } = [];
}

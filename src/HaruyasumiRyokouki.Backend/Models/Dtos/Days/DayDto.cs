using HaruyasumiRyokouki.Backend.Models.Db;
using HaruyasumiRyokouki.Backend.Models.Dtos.Media;

namespace HaruyasumiRyokouki.Backend.Models.Dtos.Days;

/// <summary>
/// Day in one language with its media.
/// </summary>
public record DayDto
{
	/// <summary>
	/// Date of the day.
	/// </summary>
	public DateOnly Date { get; set; }

	/// <summary>
	/// The notes of the day are finished.
	/// </summary>
	public bool IsReady { get; set; }

	/// <summary>
	/// Language of <see cref="Note"/>; <see langword="null"/> when there is no note in the requested language.
	/// </summary>
	public string LanguageCode { get; set; } = null!;

	/// <summary>
	/// Note in the requested language.
	/// </summary>
	public string Note { get; set; } = null!;

	/// <summary>
	/// Media of the day visible to the caller, ordered by time.
	/// </summary>
	public ICollection<MediaFileDto> Media { get; set; } = [];
}

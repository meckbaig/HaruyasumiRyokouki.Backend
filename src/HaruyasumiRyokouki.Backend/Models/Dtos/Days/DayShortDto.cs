namespace HaruyasumiRyokouki.Backend.Models.Dtos.Days;

/// <summary>
/// Day in the list of days.
/// </summary>
public record DayShortDto
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
	/// Number of media of the day visible to the caller.
	/// </summary>
	public int MediaCount { get; set; }

}

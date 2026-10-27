namespace HaruyasumiRyokouki.Backend.Models.Dtos.Days;

/// <summary>
/// Day in the list of all days.
/// </summary>
public record DayShortDto
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
	/// Number of media of the day visible to the caller.
	/// </summary>
	public int MediaCount { get; set; }

}

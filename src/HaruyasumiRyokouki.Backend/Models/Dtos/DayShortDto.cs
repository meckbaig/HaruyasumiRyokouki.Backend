namespace HaruyasumiRyokouki.Backend.Models.Dtos;

public record DayShortDto
{
	public DateOnly Date { get; set; }
	public bool IsReady { get; set; }
	public int MediaCount { get; set; }

}

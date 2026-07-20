namespace HaruyasumiRyokouki.Backend.Models.Dtos;

public record DayEditDto
{
	public DateOnly Date { get; set; }
	public bool IsReady { get; set; }

	public ICollection<DayTranslationEditDto> Translations { get; set; } = [];
}

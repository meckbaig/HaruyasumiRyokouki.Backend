namespace HaruyasumiRyokouki.Backend.Models.Dtos.Days;

public record DayTranslationEditDto
{
	public int Id { get; set; }
	public string LanguageCode { get; set; } = null!;
	public string Note { get; set; } = null!;
}

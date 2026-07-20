namespace HaruyasumiRyokouki.Backend.Models.Dtos;

public record DayTranslationEditDto
{
	public Guid Id { get; set; }
	public string LanguageCode { get; set; } = null!;
	public string Note { get; set; } = null!;
}

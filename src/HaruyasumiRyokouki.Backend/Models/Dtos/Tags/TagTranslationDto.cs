namespace HaruyasumiRyokouki.Backend.Models.Dtos.Tags;

public record TagTranslationDto
{
	public string LanguageCode { get; set; }
	public string Text { get; set; }
}

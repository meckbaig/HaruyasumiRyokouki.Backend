namespace HaruyasumiRyokouki.Backend.Models.Dtos.Media;

public record MediaTranslationEditDto
{
	public int Id { get; set; }
	public string LanguageCode { get; set; } = null!;
	public string? Title { get; set; }
	public string? Description { get; set; }
}

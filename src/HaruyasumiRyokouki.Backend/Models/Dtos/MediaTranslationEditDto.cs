namespace HaruyasumiRyokouki.Backend.Models.Dtos;

public record MediaTranslationEditDto
{
	public Guid Id { get; set; }
	public string LanguageCode { get; set; } = null!;
	public string? Title { get; set; }
	public string? Description { get; set; }
	public ICollection<string> Tags { get; set; } = [];
}

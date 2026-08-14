namespace HaruyasumiRyokouki.Backend.Models.Dtos.Tags;

public record TagDto
{
	public int Id { get; set; }
	public string Slug { get; set; }
	public ICollection<TagTranslationDto> Translations { get; set; }
	public ICollection<TagTranslationDto> Aliases { get; set; }
	public int UsageCount { get; set; }
}

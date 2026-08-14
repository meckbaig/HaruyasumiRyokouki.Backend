namespace HaruyasumiRyokouki.Backend.Models.Dtos.Tags;

public record CreateTagDto
{
	public string Slug { get; set; }
	public ICollection<TagTranslationDto> Translations { get; set; }
	public ICollection<TagTranslationDto> Aliases { get; set; }
}

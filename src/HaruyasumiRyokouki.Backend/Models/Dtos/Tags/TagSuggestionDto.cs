namespace HaruyasumiRyokouki.Backend.Models.Dtos.Tags;

public record TagSuggestionDto
{
	public required int Id { get; set; }
	public required	string Value { get; set; }
	public required int UsageCount { get; set; }
}

namespace HaruyasumiRyokouki.Backend.Models.Dtos;

public record VideoUrlsDto
{
	public string? Download { get; set; }
	public string? Stream { get; set; }
	public string Preview { get; set; } = null!;
}

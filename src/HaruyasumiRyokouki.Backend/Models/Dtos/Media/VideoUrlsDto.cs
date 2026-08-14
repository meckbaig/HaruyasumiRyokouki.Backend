namespace HaruyasumiRyokouki.Backend.Models.Dtos.Media;

public record VideoUrlsDto
{
	public string? Download { get; set; }
	public string? Stream { get; set; }
	public string Preview { get; set; } = null!;
}

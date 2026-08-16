namespace HaruyasumiRyokouki.Backend.Models.Dtos.Media;

public record ImageUrlsDto
{
	public string? Download { get; set; }
	public string? FullScreen { get; set; }
	public string Preview { get; set; } = null!;
}

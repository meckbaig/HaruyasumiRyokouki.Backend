namespace HaruyasumiRyokouki.Backend.Models.Dtos;

public record VideoUrlsDto
{
	public string? Download { get; set; } = null!;
	public string? Stream { get; set; } = null!;
	public MobileImagesDto Mobile { get; set; }
	public DesktopImagesDto Desktop { get; set; }

	public record MobileImagesDto
	{
		public string Preview { get; set; } = null!;
	}

	public record DesktopImagesDto
	{
		public string Preview { get; set; } = null!;
	}
}

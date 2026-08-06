namespace HaruyasumiRyokouki.Backend.Models.Dtos;

public record ImageUrlsDto
{
	public string? Original { get; set; }
	public MobileImagesDto Mobile { get; set; }
	public DesktopImagesDto Desktop { get; set; }

	public record MobileImagesDto
	{
		public string? FullScreen { get; set; } = null!;
		public string Preview { get; set; } = null!;
	}

	public record DesktopImagesDto
	{
		public string? FullScreen { get; set; } = null!;
		public string Preview { get; set; } = null!;
	}
}

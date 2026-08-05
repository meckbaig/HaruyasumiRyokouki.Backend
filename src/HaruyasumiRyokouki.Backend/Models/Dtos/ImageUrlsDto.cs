namespace HaruyasumiRyokouki.Backend.Models.Dtos;

public record ImageUrlsDto
{
	public MobileImagesDto Mobile { get; set; }
	public DesktopImagesDto Desktop { get; set; }

	public record MobileImagesDto
	{
		public string Original { get; set; } = null!;
		public string Preview { get; set; } = null!;
	}

	public record DesktopImagesDto
	{
		public string Original { get; set; } = null!;
		public string Preview { get; set; } = null!;
	}
}

namespace HaruyasumiRyokouki.Backend.Models.Dtos;

public record MediaFileEditDto : IPreviewDto
{
	public int Id { get; set; }
	public DateTime Created { get; set; }
	public required string FileName { get; set; } = null!;
	public float AspectRatio { get; set; }
	public string Type { get; set; }
	public double? Latitude { get; set; }
	public double? Longitude { get; set; }
	public string Miniature { get; set; }
	public bool Favorite { get; set; }

	public List<MediaTranslationEditDto> Translations { get; set; } = [];
	public ImageUrlsDto? ImageUrls { get; set; } = null;
	public VideoUrlsDto? VideoUrls { get; set; } = null;
}

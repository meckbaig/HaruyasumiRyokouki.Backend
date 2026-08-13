namespace HaruyasumiRyokouki.Backend.Models.Dtos;

public record MediaFileDto
{
	public int Id { get; set; }
	public DateTime Created { get; set; }
	public required string FileName { get; set; } = null!;
	public float AspectRatio { get; set; }
	public string Type { get; set; }
	public double? Latitude { get; set; }
	public double? Longitude { get; set; }
	public bool IsApproved { get; set; }
	public string LanguageCode { get; set; } = null!;
	public string? Title { get; set; }
	public string? Description { get; set; }
	public string Miniature { get; set; }
	public bool? Private { get; set; }

	/// <summary>
	/// <see langword="null"/> when user is not admin.
	/// </summary>
	public bool? Favorite { get; set; }

	public ICollection<string> Tags { get; set; } = [];
	public ImageUrlsDto? ImageUrls { get; set; } = null;
	public VideoUrlsDto? VideoUrls { get; set; } = null;
}

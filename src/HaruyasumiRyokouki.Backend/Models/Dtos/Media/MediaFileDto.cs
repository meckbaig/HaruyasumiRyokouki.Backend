using HaruyasumiRyokouki.Backend.Models.Dtos.Tags;
using System.Text.Json.Serialization;

namespace HaruyasumiRyokouki.Backend.Models.Dtos.Media;

public record MediaFileDto
{
	public int Id { get; set; }
	public DateTime Created { get; set; }
	public required string FileName { get; set; } = null!;
	public float AspectRatio { get; set; }
	public string Type { get; set; }
	public string Source { get; set; }
	public double? Latitude { get; set; }
	public double? Longitude { get; set; }
	public string LanguageCode { get; set; } = null!;
	public string? Title { get; set; }
	public string? Description { get; set; }
	public string Miniature { get; set; }

	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	/// <summary>
	/// <see langword="null"/> when user is not admin.
	/// </summary>
	public bool? IsApproved { get; set; }

	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	/// <summary>
	/// <see langword="null"/> when user is not admin.
	/// </summary>
	public bool? Private { get; set; }

	/// <summary>
	/// <see langword="null"/> when user is not admin.
	/// </summary>
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public bool? Favorite { get; set; }

	public ICollection<TagPublicDto> Tags { get; set; } = [];

	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public ImageUrlsDto? ImageUrls { get; set; } = null;

	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public VideoUrlsDto? VideoUrls { get; set; } = null;
}

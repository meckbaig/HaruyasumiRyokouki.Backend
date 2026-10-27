using HaruyasumiRyokouki.Backend.Models.Dtos.Tags;
using System.Text.Json.Serialization;

namespace HaruyasumiRyokouki.Backend.Models.Dtos.Media;

/// <summary>
/// Media file in one language, as shown to visitors.
/// </summary>
public record MediaFileDto
{
	/// <summary>
	/// Media file id.
	/// </summary>
	public int Id { get; set; }

	/// <summary>
	/// Creation time in the local time of the trip (not UTC).
	/// </summary>
	public DateTime Created { get; set; }

	/// <summary>
	/// File name in the storage; the video id for YouTube media.
	/// </summary>
	public required string FileName { get; set; } = null!;

	/// <summary>
	/// Width divided by height.
	/// </summary>
	public float AspectRatio { get; set; }

	/// <summary>
	/// <c>Image</c> or <c>Video</c>.
	/// </summary>
	public string Type { get; set; }

	/// <summary>
	/// <c>Local</c> or <c>YouTube</c>.
	/// </summary>
	public string Source { get; set; }

	/// <summary>
	/// Latitude in degrees, <see langword="null"/> when unknown.
	/// </summary>
	public double? Latitude { get; set; }

	/// <summary>
	/// Longitude in degrees, <see langword="null"/> when unknown.
	/// </summary>
	public double? Longitude { get; set; }

	/// <summary>
	/// Language of <see cref="Title"/> and <see cref="Description"/>; <see langword="null"/> when there is no translation in the requested language.
	/// </summary>
	public string LanguageCode { get; set; } = null!;

	/// <summary>
	/// Title in the requested language.
	/// </summary>
	public string? Title { get; set; }

	/// <summary>
	/// Description in the requested language.
	/// </summary>
	public string? Description { get; set; }

	/// <summary>
	/// Tiny placeholder image, base64-encoded, shown while the image loads.
	/// </summary>
	public string Miniature { get; set; }

	/// <summary>
	/// Approved by the admin. <see langword="null"/> (omitted) when the caller is not the admin.
	/// </summary>
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public bool? IsApproved { get; set; }

	/// <summary>
	/// Hidden from visitors. <see langword="null"/> (omitted) when the caller is not the admin.
	/// </summary>
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public bool? Private { get; set; }

	/// <summary>
	/// Shown on the front page. <see langword="null"/> (omitted) when the caller is not the admin.
	/// </summary>
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public bool? Favorite { get; set; }

	/// <summary>
	/// Tags in the requested language.
	/// </summary>
	public ICollection<TagPublicDto> Tags { get; set; } = [];

	/// <summary>
	/// URLs of an image; <see langword="null"/> (omitted) for videos.
	/// </summary>
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public ImageUrlsDto? ImageUrls { get; set; } = null;

	/// <summary>
	/// URLs of a video; <see langword="null"/> (omitted) for images.
	/// </summary>
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public VideoUrlsDto? VideoUrls { get; set; } = null;
}

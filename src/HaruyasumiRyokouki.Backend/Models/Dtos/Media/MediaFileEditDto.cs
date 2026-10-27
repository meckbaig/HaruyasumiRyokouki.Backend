using HaruyasumiRyokouki.Backend.Models.Dtos.Tags;

namespace HaruyasumiRyokouki.Backend.Models.Dtos.Media;

/// <summary>
/// Media file with all translations, for the admin.
/// </summary>
public record MediaFileEditDto : IPreviewDto
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
	/// Tiny placeholder image, base64-encoded, shown while the image loads.
	/// </summary>
	public string Miniature { get; set; }

	/// <summary>
	/// Hidden from visitors.
	/// </summary>
	public bool Private { get; set; }

	/// <summary>
	/// Shown on the front page.
	/// </summary>
	public bool Favorite { get; set; }

	/// <summary>
	/// Approved by the admin; new media are not approved.
	/// </summary>
	public bool IsApproved { get; set; }

	/// <summary>
	/// Titles and descriptions, one item per language.
	/// </summary>
	public ICollection<MediaTranslationEditDto> Translations { get; set; } = [];

	/// <summary>
	/// Tags in the requested language.
	/// </summary>
	public ICollection<TagPublicDto> Tags { get; set; } = [];

	/// <summary>
	/// URLs of an image; <see langword="null"/> (omitted) for videos.
	/// </summary>
	public ImageUrlsDto? ImageUrls { get; set; } = null;

	/// <summary>
	/// URLs of a video; <see langword="null"/> (omitted) for images.
	/// </summary>
	public VideoUrlsDto? VideoUrls { get; set; } = null;
}

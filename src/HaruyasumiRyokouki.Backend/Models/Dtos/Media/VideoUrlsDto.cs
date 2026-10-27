using System.Text.Json.Serialization;

namespace HaruyasumiRyokouki.Backend.Models.Dtos.Media;

/// <summary>
/// URLs of a video.
/// </summary>
public record VideoUrlsDto
{
	/// <summary>
	/// Original file; <see langword="null"/> (omitted) for YouTube.
	/// </summary>
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? Download { get; set; }

	/// <summary>
	/// URL to play: the lower bitrate version if there is one, the original otherwise, the embed URL for YouTube.
	/// </summary>
	public string? Stream { get; set; }

	/// <summary>
	/// Resized preview image.
	/// </summary>
	public string Preview { get; set; } = null!;
}

using System.Text.Json.Serialization;

namespace HaruyasumiRyokouki.Backend.Models.Dtos.Media;

/// <summary>
/// URLs of a video.
/// </summary>
public record VideoUrlsDto
{
	/// <summary>
	/// Original file for downloading, or <see langword="null"/> for YouTube media.
	/// </summary>
	/// <remarks>
	/// Omitted from the response when <see langword="null"/>.
	/// </remarks>
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? Download { get; set; }

	/// <summary>
	/// URL to play the video.
	/// </summary>
	/// <remarks>
	/// The lower bitrate <c>_web</c> version when it exists, otherwise the original. For YouTube media it is the embed URL.
	/// </remarks>
	public string? Stream { get; set; }

	/// <summary>
	/// Preview image of the video resized as a thumbnail.
	/// </summary>
	public string Preview { get; set; } = null!;
}

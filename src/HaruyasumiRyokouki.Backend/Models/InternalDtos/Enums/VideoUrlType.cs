namespace HaruyasumiRyokouki.Backend.Models.InternalDtos.Enums;

/// <summary>
/// Kind of a video URL.
/// </summary>
public enum VideoUrlType
{
	/// <summary>
	/// Original file for downloading.
	/// </summary>
	Download,

	/// <summary>
	/// File to play: the lower bitrate <c>_web</c> version when it exists, otherwise the original.
	/// </summary>
	Stream,

	/// <summary>
	/// Preview image of the video resized as a thumbnail.
	/// </summary>
	Preview
}

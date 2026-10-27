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
	/// File for playback: the lower bitrate <c>_web</c> version when it exists, otherwise the original.
	/// </summary>
	Stream,

	/// <summary>
	/// Resized preview image of the video.
	/// </summary>
	Preview
}

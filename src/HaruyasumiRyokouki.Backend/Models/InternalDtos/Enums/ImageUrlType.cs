namespace HaruyasumiRyokouki.Backend.Models.InternalDtos.Enums;

/// <summary>
/// Kind of an image URL built by a preview service.
/// </summary>
public enum ImageUrlType
{
	/// <summary>
	/// Original file for downloading.
	/// </summary>
	Download,

	/// <summary>
	/// Original file through imgproxy. Not supported by the Nextcloud provider and not used in DTOs.
	/// </summary>
	Original,

	/// <summary>
	/// Resized for the client screen.
	/// </summary>
	FullScreen,

	/// <summary>
	/// Resized thumbnail.
	/// </summary>
	Preview
}

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
	/// Original file through imgproxy.
	/// </summary>
	/// <remarks>
	/// Not supported by the Nextcloud provider and not used in DTOs.
	/// </remarks>
	Original,

	/// <summary>
	/// Image resized for the screen of the client.
	/// </summary>
	FullScreen,

	/// <summary>
	/// Thumbnail for grids and lists.
	/// </summary>
	Preview
}

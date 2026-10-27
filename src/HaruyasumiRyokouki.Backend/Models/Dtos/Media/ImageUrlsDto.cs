namespace HaruyasumiRyokouki.Backend.Models.Dtos.Media;

/// <summary>
/// URLs of an image.
/// </summary>
/// <remarks>
/// The sizes of the resized images depend on the <c>X-Display</c> header of the request.
/// </remarks>
public record ImageUrlsDto
{
	/// <summary>
	/// Original file for downloading.
	/// </summary>
	public string? Download { get; set; }

	/// <summary>
	/// Image resized for the screen of the client.
	/// </summary>
	public string? FullScreen { get; set; }

	/// <summary>
	/// Thumbnail for grids and lists.
	/// </summary>
	public string Preview { get; set; } = null!;
}

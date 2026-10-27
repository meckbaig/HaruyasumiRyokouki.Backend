namespace HaruyasumiRyokouki.Backend.Models.Dtos.Media;

/// <summary>
/// URLs of an image. Sizes depend on the <c>X-Display</c> header.
/// </summary>
public record ImageUrlsDto
{
	/// <summary>
	/// Original file.
	/// </summary>
	public string? Download { get; set; }

	/// <summary>
	/// Resized for the screen.
	/// </summary>
	public string? FullScreen { get; set; }

	/// <summary>
	/// Resized thumbnail.
	/// </summary>
	public string Preview { get; set; } = null!;
}

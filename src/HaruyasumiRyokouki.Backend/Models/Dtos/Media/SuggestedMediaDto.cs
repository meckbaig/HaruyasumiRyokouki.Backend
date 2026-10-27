namespace HaruyasumiRyokouki.Backend.Models.Dtos.Media;

/// <summary>
/// Media file found by similarity search.
/// </summary>
public record SuggestedMediaDto
{
	/// <summary>
	/// The media file.
	/// </summary>
	public required MediaFileEditDto Media { get; init; }

	/// <summary>
	/// Cosine to the centroid.
	/// </summary>
	public required float Score { get; init; }
}

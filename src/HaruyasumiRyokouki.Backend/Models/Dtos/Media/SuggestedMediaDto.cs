namespace HaruyasumiRyokouki.Backend.Models.Dtos.Media;

/// <summary>
/// Media file found by the similarity search.
/// </summary>
public record SuggestedMediaDto
{
	/// <summary>
	/// The found media file.
	/// </summary>
	public required MediaFileEditDto Media { get; init; }

	/// <summary>
	/// Cosine similarity to the query: the media file or the average of the tagged media.
	/// </summary>
	/// <remarks>
	/// From -1 to 1, higher is more similar. There is no threshold, the list is cut only by its length.
	/// </remarks>
	public required float Score { get; init; }
}

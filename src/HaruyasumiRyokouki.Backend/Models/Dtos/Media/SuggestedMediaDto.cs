namespace HaruyasumiRyokouki.Backend.Models.Dtos.Media;

public record SuggestedMediaDto
{
	public required MediaFileEditDto Media { get; init; }

	/// <summary>
	/// Cosine to the centroid.
	/// </summary>
	public required float Score { get; init; }
}

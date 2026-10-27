namespace HaruyasumiRyokouki.Backend.Models.Dtos.Media;

/// <summary>
/// Title and description of a media file in one language.
/// </summary>
public record MediaTranslationEditDto
{
	/// <summary>
	/// Translation id; ignored on input.
	/// </summary>
	public int Id { get; set; }

	/// <summary>
	/// Language code: <c>en</c>, <c>ru</c> or <c>ja</c>.
	/// </summary>
	public string LanguageCode { get; set; } = null!;

	/// <summary>
	/// Title.
	/// </summary>
	public string? Title { get; set; }

	/// <summary>
	/// Description.
	/// </summary>
	public string? Description { get; set; }
}

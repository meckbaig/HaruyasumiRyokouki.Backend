namespace HaruyasumiRyokouki.Backend.Models.Dtos.Media;

/// <summary>
/// Title and description of a media file in one language.
/// </summary>
public record MediaTranslationEditDto
{
	/// <summary>
	/// Identifier of the translation.
	/// </summary>
	/// <remarks>
	/// Ignored on input.
	/// </remarks>
	public int Id { get; set; }

	/// <summary>
	/// Language of the text: <c>en</c>, <c>ru</c> or <c>ja</c>.
	/// </summary>
	/// <remarks>
	/// See <see cref="Db.Enums.LanguageCode"/>.
	/// </remarks>
	public string LanguageCode { get; set; } = null!;

	/// <summary>
	/// Title written by the author.
	/// </summary>
	public string? Title { get; set; }

	/// <summary>
	/// Description written by the author.
	/// </summary>
	public string? Description { get; set; }
}

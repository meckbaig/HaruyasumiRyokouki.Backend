namespace HaruyasumiRyokouki.Backend.Models.Db;

/// <summary>
/// Title and description of a media file in one language.
/// </summary>
public class MediaTranslation
{
	/// <summary>
	/// Identifier of the translation.
	/// </summary>
	public int Id { get; set; }

	/// <summary>
	/// Identifier of the <see cref="MediaFile"/>.
	/// </summary>
	public int MediaFileId { get; set; }

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

	/// <summary>
	/// The media file of the translation.
	/// </summary>
	public MediaFile MediaFile { get; set; }

	/// <summary>
	/// Creates a copy without identifiers.
	/// </summary>
	/// <remarks>
	/// Used to add the same texts to several media files edited together.
	/// </remarks>
	public MediaTranslation Clone()
	{
		return new MediaTranslation
		{
			LanguageCode = LanguageCode,
			Title = Title,
			Description = Description,
		};
	}
}

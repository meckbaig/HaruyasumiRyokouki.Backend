namespace HaruyasumiRyokouki.Backend.Models.Db;

/// <summary>
/// Title and description of a media file in one language.
/// </summary>
public class MediaTranslation
{
	/// <summary>
	/// Id.
	/// </summary>
	public int Id { get; set; }

	/// <summary>
	/// Media file id.
	/// </summary>
	public int MediaFileId { get; set; }

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

	/// <summary>
	/// The media file.
	/// </summary>
	public MediaFile MediaFile { get; set; }

	/// <summary>
	/// Copy without ids, to add the same texts to another media file.
	/// </summary>
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

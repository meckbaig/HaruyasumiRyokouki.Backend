namespace HaruyasumiRyokouki.Backend.Models.Db;

/// <summary>
/// Tag of media files.
/// </summary>
public class Tag
{
	/// <summary>
	/// Id.
	/// </summary>
	public int Id { get; set; }

	/// <summary>
	/// Unique tag key, used in links (<c>/v1/search?tag=</c>).
	/// </summary>
	public string Slug { get; set; }

	/// <summary>
	/// Display names (<see cref="TagTranslation.IsPrimary"/>) and search aliases; all texts are unique within the tag.
	/// </summary>
	public ICollection<TagTranslation> Translations { get; set; } = [];

	/// <summary>
	/// Rows of the media-tag join table.
	/// </summary>
	public ICollection<MediaFileTag> MediaTags { get; set; } = [];

	/// <summary>
	/// Media with the tag (many-to-many through <see cref="MediaTags"/>).
	/// </summary>
	public ICollection<MediaFile> Media { get; set; } = [];
}

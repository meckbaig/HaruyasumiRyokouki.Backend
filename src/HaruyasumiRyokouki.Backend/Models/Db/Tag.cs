namespace HaruyasumiRyokouki.Backend.Models.Db;

/// <summary>
/// Tag describing what is on a photo or video, e.g. a temple or cherry blossoms.
/// </summary>
public class Tag
{
	/// <summary>
	/// Identifier of the tag.
	/// </summary>
	public int Id { get; set; }

	/// <summary>
	/// Unique key of the tag in English, e.g. <c>cherry_blossom</c>.
	/// </summary>
	/// <remarks>
	/// Used in links to the tag search: <c>GET /v1/search?tag=cherry_blossom</c>.
	/// </remarks>
	public string Slug { get; set; }

	/// <summary>
	/// Display names and search aliases of the tag.
	/// </summary>
	/// <remarks>
	/// A display name has <see cref="TagTranslation.IsPrimary"/> set, at most one per language. All texts of a tag are unique, regardless of the language.
	/// </remarks>
	public ICollection<TagTranslation> Translations { get; set; } = [];

	/// <summary>
	/// Rows of the media-tag join table.
	/// </summary>
	public ICollection<MediaFileTag> MediaTags { get; set; } = [];

	/// <summary>
	/// Media with the tag.
	/// </summary>
	/// <remarks>
	/// Many-to-many through <see cref="MediaTags"/>.
	/// </remarks>
	public ICollection<MediaFile> Media { get; set; } = [];
}

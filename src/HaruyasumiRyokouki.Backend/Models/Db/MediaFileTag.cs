namespace HaruyasumiRyokouki.Backend.Models.Db;

/// <summary>
/// Row of the media-tag join table <c>media_file_tag</c>.
/// </summary>
public class MediaFileTag
{
	/// <summary>
	/// Identifier of the <see cref="MediaFile"/>.
	/// </summary>
	public int MediaId { get; set; }

	/// <summary>
	/// The tagged media file.
	/// </summary>
	public MediaFile Media { get; set; } = null!;

	/// <summary>
	/// Identifier of the <see cref="Db.Tag"/>.
	/// </summary>
	public int TagId { get; set; }

	/// <summary>
	/// The tag.
	/// </summary>
	public Tag Tag { get; set; } = null!;
}

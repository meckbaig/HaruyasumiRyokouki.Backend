namespace HaruyasumiRyokouki.Backend.Models.Db;

/// <summary>
/// Row of the media-tag join table <c>media_file_tag</c>.
/// </summary>
public class MediaFileTag
{
	/// <summary>
	/// Media file id.
	/// </summary>
	public int MediaId { get; set; }

	/// <summary>
	/// The media file.
	/// </summary>
	public MediaFile Media { get; set; } = null!;

	/// <summary>
	/// Tag id.
	/// </summary>
	public int TagId { get; set; }

	/// <summary>
	/// The tag.
	/// </summary>
	public Tag Tag { get; set; } = null!;
}

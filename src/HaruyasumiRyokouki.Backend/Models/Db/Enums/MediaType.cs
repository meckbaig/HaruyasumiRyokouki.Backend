namespace HaruyasumiRyokouki.Backend.Models.Db.Enums;

/// <summary>
/// Kind of a media file, detected by the file extension.
/// </summary>
public enum MediaType
{
	/// <summary>
	/// Photo or other image.
	/// </summary>
	Image = 0,

	/// <summary>
	/// Video from the file storage or YouTube.
	/// </summary>
	Video = 1,

	/// <summary>
	/// Unsupported extension. Such files are skipped by sync.
	/// </summary>
	Unknown = -1
}

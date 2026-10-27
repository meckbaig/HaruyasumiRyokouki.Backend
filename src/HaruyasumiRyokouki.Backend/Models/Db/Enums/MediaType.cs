namespace HaruyasumiRyokouki.Backend.Models.Db.Enums;

/// <summary>
/// Kind of a media file, detected by the file extension.
/// </summary>
public enum MediaType
{
	/// <summary>
	/// Image.
	/// </summary>
	Image = 0,

	/// <summary>
	/// Video, local or YouTube.
	/// </summary>
	Video = 1,

	/// <summary>
	/// Unsupported extension; such files are skipped by sync.
	/// </summary>
	Unknown = -1
}

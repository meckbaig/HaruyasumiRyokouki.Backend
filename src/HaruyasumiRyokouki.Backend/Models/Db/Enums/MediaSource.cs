namespace HaruyasumiRyokouki.Backend.Models.Db.Enums;

/// <summary>
/// Where a media file comes from.
/// </summary>
public enum MediaSource
{
	/// <summary>
	/// A file in the file storage, imported by sync.
	/// </summary>
	Local = 0,

	/// <summary>
	/// A YouTube video; only its preview is in the file storage.
	/// </summary>
	YouTube = 1
}

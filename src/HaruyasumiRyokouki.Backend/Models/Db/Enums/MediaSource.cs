namespace HaruyasumiRyokouki.Backend.Models.Db.Enums;

/// <summary>
/// Where a media file comes from.
/// </summary>
public enum MediaSource
{
	/// <summary>
	/// A file from the file storage, imported by sync.
	/// </summary>
	Local = 0,

	/// <summary>
	/// A YouTube video. Only its preview image is in the file storage.
	/// </summary>
	YouTube = 1
}

namespace HaruyasumiRyokouki.Backend.Models.InternalDtos;

/// <summary>
/// File in the file storage.
/// </summary>
public record StorageFile
{
	/// <summary>
	/// File name without folders.
	/// </summary>
	public string FileName { get; set; }

	/// <summary>
	/// File modification time in UTC.
	/// </summary>
	public DateTime Created { get; set; }
}

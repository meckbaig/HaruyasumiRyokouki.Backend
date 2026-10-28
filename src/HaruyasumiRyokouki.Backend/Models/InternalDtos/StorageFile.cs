namespace HaruyasumiRyokouki.Backend.Models.InternalDtos;

/// <summary>
/// File in the file storage.
/// </summary>
public record StorageFile
{
	/// <summary>
	/// File name without folders.
	/// </summary>
	public required string FileName { get; set; }

	/// <summary>
	/// Last modification time of the file in UTC.
	/// </summary>
	public DateTime Created { get; set; }
}

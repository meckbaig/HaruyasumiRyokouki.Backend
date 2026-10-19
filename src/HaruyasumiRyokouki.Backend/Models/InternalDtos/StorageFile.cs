namespace HaruyasumiRyokouki.Backend.Models.InternalDtos;

public record StorageFile
{
	public string FileName { get; set; }

	/// <summary>
	/// File modification time in UTC.
	/// </summary>
	public DateTime Created { get; set; }
}

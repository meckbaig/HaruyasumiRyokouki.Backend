namespace HaruyasumiRyokouki.Backend.Models.InternalDtos;

public record StorageFile
{
	public string FileName { get; set; }
	public DateTime Created { get; set; }
}

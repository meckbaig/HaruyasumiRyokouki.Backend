namespace HaruyasumiRyokouki.Backend.Models.Db;

public class MediaEmbedding
{
	public int MediaFileId { get; set; }
	public required float[] Vector { get; set; }
	public required string Model { get; set; }
	public MediaFile MediaFile { get; set; } = null!;
}

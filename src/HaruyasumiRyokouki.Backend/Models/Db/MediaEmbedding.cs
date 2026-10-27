namespace HaruyasumiRyokouki.Backend.Models.Db;

/// <summary>
/// Image embedding of a media file for similarity search. Filled by an external tool.
/// </summary>
public class MediaEmbedding
{
	/// <summary>
	/// Media file id; one embedding per media file.
	/// </summary>
	public int MediaFileId { get; set; }

	/// <summary>
	/// Vector of 768 values, expected to be normalized: the index uses the dot product as cosine similarity.
	/// </summary>
	public required float[] Vector { get; set; }

	/// <summary>
	/// Name of the model that produced the vector.
	/// </summary>
	public required string Model { get; set; }

	/// <summary>
	/// The media file.
	/// </summary>
	public MediaFile MediaFile { get; set; } = null!;
}

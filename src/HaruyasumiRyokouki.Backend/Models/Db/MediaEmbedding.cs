namespace HaruyasumiRyokouki.Backend.Models.Db;

/// <summary>
/// Image embedding of a media file, used to find similar media.
/// </summary>
/// <remarks>
/// The backend never writes this table: it is filled by a separate tool. Media added later have no embedding.
/// </remarks>
public class MediaEmbedding
{
	/// <summary>
	/// Identifier of the <see cref="MediaFile"/>. One embedding per media file.
	/// </summary>
	public int MediaFileId { get; set; }

	/// <summary>
	/// Embedding of 768 values.
	/// </summary>
	/// <remarks>
	/// Expected to be normalized, because the similarity index uses the dot product as the cosine similarity.
	/// </remarks>
	public required float[] Vector { get; set; }

	/// <summary>
	/// Name of the model that produced <see cref="Vector"/>.
	/// </summary>
	public required string Model { get; set; }

	/// <summary>
	/// The media file of the embedding.
	/// </summary>
	public MediaFile MediaFile { get; set; } = null!;
}

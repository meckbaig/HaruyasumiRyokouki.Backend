namespace HaruyasumiRyokouki.Backend.Common.Options;

/// <summary>
/// Options of the <c>SimilaritySearch</c> configuration section; see the Configuration section of README.
/// </summary>
sealed class SimilaritySearchOptions
{
	public const string ConfigurationSectionName = "SimilaritySearch";

	/// <summary>
	/// Enables the search of similar media by image embeddings (<c>media_embeddings</c> table).
	/// When disabled, the vectors are not loaded into memory and the similarity endpoints return 503.
	/// </summary>
	public bool Enabled { get; set; } = true;
}

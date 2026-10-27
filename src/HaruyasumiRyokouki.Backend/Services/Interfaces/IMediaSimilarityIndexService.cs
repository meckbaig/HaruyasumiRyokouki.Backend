using HaruyasumiRyokouki.Backend.DbContexts;

namespace HaruyasumiRyokouki.Backend.Services.Interfaces;

internal interface IMediaSimilarityIndexService
{
	int Count { get; }
	bool IsLoaded { get; }

	/// <summary>
	/// Whether the similarity search is enabled in <see cref="Common.Options.SimilaritySearchOptions"/>.
	/// </summary>
	bool IsEnabled { get; }

	/// <summary>
	/// Loads the vectors from DB if the index is empty.
	/// </summary>
	/// <exception cref="Common.Exceptions.ServiceUnavailableException">The similarity search is disabled.</exception>
	Task EnsureLoadedAsync(IAppDbContext context, CancellationToken cancellationToken = default);

	/// <summary>
	/// The average vector of a group of photos, normalized to length 1.
	/// </summary>
	float[] Centroid(IEnumerable<int> mediaFileIds);

	/// <summary>
	/// Top-N closest to an arbitrary query vector.
	/// </summary>
	IReadOnlyList<MediaSimilarityIndexService.SimilarityHit> MostSimilar(ReadOnlySpan<float> query, int take, IReadOnlySet<int>? exclude = null);

	/// <summary>
	/// Top-N similar to one specific photo.
	/// </summary>
	IReadOnlyList<MediaSimilarityIndexService.SimilarityHit> MostSimilarTo(int mediaFileId, int take);

	/// <summary>
	/// Reads all vectors from DB to memory.
	/// </summary>
	Task ReloadAsync(IAppDbContext context, CancellationToken cancellationToken = default);
}

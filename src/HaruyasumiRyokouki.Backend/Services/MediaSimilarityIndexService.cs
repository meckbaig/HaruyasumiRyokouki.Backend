using HaruyasumiRyokouki.Backend.DbContexts;
using HaruyasumiRyokouki.Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Numerics.Tensors;

namespace HaruyasumiRyokouki.Backend.Services;

internal class MediaSimilarityIndexService : IMediaSimilarityIndexService
{
	private readonly ILogger<MediaSimilarityIndexService> _logger;

	public MediaSimilarityIndexService(ILogger<MediaSimilarityIndexService> logger)
	{
		_logger = logger;
	}

	/// <summary>
	/// Vector length.
	/// </summary>
	private const int Dim = 768;

	/// <summary>
	/// The media file ids are in the same order as the strings in _vectors.
	/// </summary>
	private int[] _ids = [];

	/// <summary>
	/// All vectors are stored as a single flat array: row i takes up [i * Dim, (i + 1) * Dim). 
	/// One large array instead of float[][] is used to ensure that 
	/// the data is stored contiguously in memory and SIMD works efficiently.
	/// </summary>
	private float[] _vectors = [];

	/// <summary>
	/// MediaFileId => line number. This is needed to avoid a brute-force search.
	/// </summary>
	private Dictionary<int, int> _rowById = [];

	public bool IsLoaded => _ids.Length > 0;
	public int Count => _ids.Length;

	public async Task ReloadAsync(IAppDbContext context, CancellationToken cancellationToken = default)
	{
		var rows = await context.MediaEmbeddings
			.AsNoTracking()
			.OrderBy(e => e.MediaFileId)
			.Select(e => new { e.MediaFileId, e.Vector })
			.ToListAsync(cancellationToken);

		var ids = new int[rows.Count];
		var vectors = new float[rows.Count * Dim];
		var rowById = new Dictionary<int, int>(rows.Count);

		for (int i = 0; i < rows.Count; i++)
		{
			ids[i] = rows[i].MediaFileId;
			rowById[rows[i].MediaFileId] = i;
			rows[i].Vector.CopyTo(vectors, i * Dim);
		}

		_ids = ids;
		_vectors = vectors;
		_rowById = rowById;

		_logger.LogDebug($"{nameof(MediaSimilarityIndexService)} loaded.");
	}

	public float[] Centroid(IEnumerable<int> mediaFileIds)
	{
		var accumulator = new float[Dim];
		int used = 0;

		foreach (int id in mediaFileIds)
		{
			if (!_rowById.TryGetValue(id, out int row))
				continue;

			TensorPrimitives.Add(accumulator, _vectors.AsSpan(row * Dim, Dim), accumulator);
			used++;
		}

		if (used == 0)
			return accumulator;

		float norm = TensorPrimitives.Norm(accumulator);
		if (norm > 0)
			TensorPrimitives.Divide(accumulator, norm, accumulator);

		return accumulator;
	}

	public IReadOnlyList<SimilarityHit> MostSimilar
	(
		ReadOnlySpan<float> query,
		int take,
		IReadOnlySet<int>? exclude = null
	)
	{
		if (query.Length != Dim)
			throw new ArgumentException($"Ожидался вектор длины {Dim}", nameof(query));

		var scored = new List<SimilarityHit>(_ids.Length);

		for (int i = 0; i < _ids.Length; i++)
		{
			int id = _ids[i];
			if (exclude is not null && exclude.Contains(id))
				continue;

			float score = TensorPrimitives.Dot(query, _vectors.AsSpan(i * Dim, Dim));
			scored.Add(new SimilarityHit(id, score));
		}

		scored.Sort(static (a, b) => b.Score.CompareTo(a.Score));

		return scored.Count <= take
			? scored
			: scored.GetRange(0, take);
	}

	public IReadOnlyList<SimilarityHit> MostSimilarTo(int mediaFileId, int take)
	{
		if (!_rowById.TryGetValue(mediaFileId, out int row))
			return [];

		return MostSimilar(_vectors.AsSpan(row * Dim, Dim), take, new HashSet<int> { mediaFileId });
	}

	public readonly record struct SimilarityHit(int MediaFileId, float Score);
}

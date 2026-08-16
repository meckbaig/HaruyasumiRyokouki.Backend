using HaruyasumiRyokouki.Backend.Common.Abstractions;
using HaruyasumiRyokouki.Backend.DbContexts;
using HaruyasumiRyokouki.Backend.Extensions;
using HaruyasumiRyokouki.Backend.Models.Dtos.Media;
using HaruyasumiRyokouki.Backend.Models.InternalDtos;
using HaruyasumiRyokouki.Backend.Services.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json.Serialization;

namespace HaruyasumiRyokouki.Backend.Features.Tags;

public record SuggestMediaForTagQuery : IRequest<SuggestMediaForTagResponse>, IDisplayAwareRequest, ILocalizableRequest
{
	[FromRoute]
	public required int Id { get; init; }

	[FromQuery]
	public int Take { get; init; } = 300;

	[SwaggerIgnore]
	[JsonIgnore]
	public ClientDisplay? ClientDisplay { get; set; }

	[SwaggerIgnore]
	[JsonIgnore]
	public string? AcceptLanguage { get; set; }
}

public class SuggestMediaForTagResponse
{
	public required int SeedCount { get; init; }
	public required ICollection<SuggestedMediaDto> Items { get; init; }
}

internal class SuggestMediaForTagQueryHandler : IRequestHandler<SuggestMediaForTagQuery, SuggestMediaForTagResponse>
{
	private readonly IAppDbContext _context;
	private readonly IMediaSimilarityIndexService _similarityService;
	private readonly IMediaPreviewService _previewService;

	public SuggestMediaForTagQueryHandler(IAppDbContext context, IMediaSimilarityIndexService similarityService, IMediaPreviewService previewService)
	{
		_context = context;
		_similarityService = similarityService;
		_previewService = previewService;
	}

	public async Task<SuggestMediaForTagResponse> Handle(SuggestMediaForTagQuery request, CancellationToken cancellationToken)
	{
		var seedIds = await _context.MediaFiles
			.AsNoTracking()
			.Where(m => m.Tags.Any(t => t.Id == request.Id))
			.Select(m => m.Id)
			.ToListAsync(cancellationToken);

		// Minimum 3, 10+ work better
		if (seedIds.Count < 3)
		{
			return new SuggestMediaForTagResponse { SeedCount = seedIds.Count, Items = [] };
		}

		if (!_similarityService.IsLoaded)
			await _similarityService.ReloadAsync(_context, cancellationToken);

		// Centroid as a query vector
		float[] query = _similarityService.Centroid(seedIds);

		// Rank everything except what is already marked
		var hits = _similarityService.MostSimilar(query, request.Take, seedIds.ToHashSet());
		if (hits.Count == 0)
		{
			return new SuggestMediaForTagResponse { SeedCount = seedIds.Count, Items = [] };
		}

		// Prepare media
		var hitIds = hits.Select(h => h.MediaFileId).ToList();

		var mediaById = await _context.MediaFiles
			.AsNoTracking()
			.Include(m => m.Translations)
			.Include(m => m.Tags)
				.ThenIncludeFiltered(t => t.Translations, request.AcceptLanguage.LocalizedTags())
			.Where(m => hitIds.Contains(m.Id))
			.ToDictionaryAsync(m => m.Id, cancellationToken);

		// The order from SQL is not preserved, we restore it based on hits.
		// This is the order in which the photos must be placed in the grid, otherwise the whole point of ranking is lost.
		var items = hits
			.Where(h => mediaById.ContainsKey(h.MediaFileId))
			.Select(h => new SuggestedMediaDto
			{
				Media = mediaById[h.MediaFileId]
					.ToEditDto()
					.AddUrls(mediaById[h.MediaFileId].AdditionalFiles, _previewService, request.ClientDisplay),
				Score = h.Score
			})
			.ToList();

		return new SuggestMediaForTagResponse
		{
			SeedCount = seedIds.Count,
			Items = items
		};
	}
}


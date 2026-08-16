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

namespace HaruyasumiRyokouki.Backend.Features.Media;

public record GetSimilarMediaQuery : IRequest<GetSimilarMediaResponse>, IDisplayAwareRequest, ILocalizableRequest
{
	[FromRoute]
	public required int Id { get; init; }

	[FromQuery]
	public int Take { get; init; } = 50;

	[SwaggerIgnore]
	[JsonIgnore]
	public ClientDisplay? ClientDisplay { get; set; }

	[SwaggerIgnore]
	[JsonIgnore]
	public string? AcceptLanguage { get; set; }
}
public class GetSimilarMediaResponse
{
	public required IReadOnlyCollection<SuggestedMediaDto> Items { get; init; }
}

internal class GetSimilarMediaHandler : IRequestHandler<GetSimilarMediaQuery, GetSimilarMediaResponse>
{
	private readonly IAppDbContext _context;
	private readonly IMediaSimilarityIndexService _similarityService;
	private readonly IMediaPreviewService _previewService;

	public GetSimilarMediaHandler(IAppDbContext context, IMediaSimilarityIndexService similarityService, IMediaPreviewService previewService)
	{
		_context = context;
		_similarityService = similarityService;
		_previewService = previewService;
	}

	public async Task<GetSimilarMediaResponse> Handle(
		GetSimilarMediaQuery request,
		CancellationToken cancellationToken)
	{

		if (!_similarityService.IsLoaded)
			await _similarityService.ReloadAsync(_context, cancellationToken);

		var hits = _similarityService.MostSimilarTo(request.Id, request.Take);
		if (hits.Count == 0)
		{
			return new GetSimilarMediaResponse { Items = [] };
		}

		var hitIds = hits.Select(h => h.MediaFileId).ToList();

		var mediaById = await _context.MediaFiles
			.AsNoTracking()
			.Include(m => m.Translations)
			.Include(m => m.Tags)
				.ThenIncludeFiltered(t => t.Translations, request.AcceptLanguage.LocalizedTags())
			.Where(m => hitIds.Contains(m.Id))
			.ToDictionaryAsync(m => m.Id, cancellationToken);

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

		return new GetSimilarMediaResponse { Items = items };
	}
}

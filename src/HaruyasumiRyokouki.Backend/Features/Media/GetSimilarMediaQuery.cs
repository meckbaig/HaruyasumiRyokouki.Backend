using FluentValidation;
using HaruyasumiRyokouki.Backend.Common.Abstractions;
using HaruyasumiRyokouki.Backend.Common.Exceptions;
using HaruyasumiRyokouki.Backend.DbContexts;
using HaruyasumiRyokouki.Backend.Extensions;
using HaruyasumiRyokouki.Backend.Models.Db.Enums;
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
	public string AcceptLanguage { get; set; } = LanguageCode.Default;
}
public class GetSimilarMediaResponse
{
	public required IReadOnlyCollection<SuggestedMediaDto> Items { get; init; }
}

internal class GetSimilarMediaQueryValidator : AbstractValidator<GetSimilarMediaQuery>
{
	public GetSimilarMediaQueryValidator()
	{
		RuleFor(x => x.Id)
			.GreaterThan(0);
		RuleFor(x => x.Take)
			.GreaterThan(0);
	}
}

internal class GetSimilarMediaQueryHandler : IRequestHandler<GetSimilarMediaQuery, GetSimilarMediaResponse>
{
	private readonly IAppDbContext _context;
	private readonly IMediaSimilarityIndexService _similarityService;
	private readonly IMediaUrlsProvider _urlsProvider;

	public GetSimilarMediaQueryHandler(IAppDbContext context, IMediaSimilarityIndexService similarityService, IMediaUrlsProvider urlsProvider)
	{
		_context = context;
		_similarityService = similarityService;
		_urlsProvider = urlsProvider;
	}

	public async Task<GetSimilarMediaResponse> Handle(
		GetSimilarMediaQuery request,
		CancellationToken cancellationToken)
	{
		await _similarityService.EnsureLoadedAsync(_context, cancellationToken);

		if (!await _context.MediaFiles.AnyAsync(m => m.Id == request.Id, cancellationToken))
			throw new EntityNotFoundException($"Media file with Id {request.Id} not found.");

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
			.Select(h =>
			{
				var media = mediaById[h.MediaFileId];
				return new SuggestedMediaDto
				{
					Media = media.ToEditDto().AddUrls(media, _urlsProvider, request.ClientDisplay),
					Score = h.Score
				};
			})
			.ToList();

		return new GetSimilarMediaResponse { Items = items };
	}
}

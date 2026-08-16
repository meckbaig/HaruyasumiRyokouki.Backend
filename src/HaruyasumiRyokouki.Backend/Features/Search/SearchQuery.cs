using FluentValidation;
using HaruyasumiRyokouki.Backend.Common.Abstractions;
using HaruyasumiRyokouki.Backend.DbContexts;
using HaruyasumiRyokouki.Backend.Extensions;
using HaruyasumiRyokouki.Backend.Models.Db;
using HaruyasumiRyokouki.Backend.Models.Dtos.Days;
using HaruyasumiRyokouki.Backend.Models.InternalDtos;
using HaruyasumiRyokouki.Backend.Services.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.Linq.Expressions;
using System.Text.Json.Serialization;

namespace HaruyasumiRyokouki.Backend.Features.Search;

public record SearchQuery : IRequest<SearchResponse>, ILocalizableRequest, IDisplayAwareRequest, IAuthentificatedRequest
{
	[FromQuery]
	public string? Text { get; init; }

	[FromQuery]
	public int? TagId { get; init; }

	[FromQuery]
	public string? Tag { get; init; }

	[SwaggerIgnore]
	[JsonIgnore]
	public string? AcceptLanguage { get; set; }

	[SwaggerIgnore]
	[JsonIgnore]
	public ClientDisplay? ClientDisplay { get; set; }

	[SwaggerIgnore]
	[JsonIgnore]
	public bool IsAuthenticated { get; set; }
}

public class SearchResponse
{
	public List<DayDto> Items { get; set; }
}

internal class SearchQueryValidator : AbstractValidator<SearchQuery>
{
	public SearchQueryValidator()
	{
		RuleFor(x => x).Must(x => !string.IsNullOrWhiteSpace(x.Text) || !string.IsNullOrWhiteSpace(x.Tag) || x.TagId.HasValue);
	}
}

internal class SearchQueryHandler : IRequestHandler<SearchQuery, SearchResponse>
{
	private readonly IAppDbContext _context;
	private readonly IMediaPreviewService _previewService;

	public SearchQueryHandler(IAppDbContext context, IMediaPreviewService previewService)
	{
		_context = context;
		_previewService = previewService;
	}

	public async Task<SearchResponse> Handle(SearchQuery request, CancellationToken cancellationToken)
	{
		var searchFunction = request.Text != null 
			? SearchByTextAsync(request, cancellationToken)
			: SearchByTagAsync(request, cancellationToken);

		List<Day> searchResults = await searchFunction;

		var searchResultsDtos = searchResults.ToDtos(request.IsAuthenticated);
		var result = searchResultsDtos.AddUrls(searchResults, _previewService, request.ClientDisplay);

		return new SearchResponse
		{
			Items = result.ToList(),
		};
	}

	private async Task<List<Day>> SearchByTagAsync(SearchQuery request, CancellationToken cancellationToken)
	{
		Expression<Func<MediaFile, bool>> mediaFilter = null;
		if (request.TagId.HasValue)
		{
			mediaFilter = m => (request.IsAuthenticated || (m.IsApproved && !m.Private))
								&&	m.Tags.Any(t => t.Id == request.TagId!.Value);
		}
		if (request.Tag != null)
		{
			mediaFilter = m => (request.IsAuthenticated || (m.IsApproved && !m.Private))
								&& m.Tags.Any(t => EF.Functions.ILike(t.Slug, request.Tag));
		}
		if (mediaFilter == null)
			throw new ArgumentNullException("Media filter is null.");

		var searchResults = await _context.Days
			.AsNoTracking()
			.Where(d => d.Media.AsQueryable().Any(mediaFilter))
			.IncludeFiltered(d => d.Translations, request.AcceptLanguage!.LocalizedDays())
			.Include(d => d.Media.AsQueryable().Where(mediaFilter))
				.ThenIncludeFiltered(m => m.Translations, request.AcceptLanguage!.LocalizedMedia())
			.Include(d => d.Media.AsQueryable().Where(mediaFilter))
				.ThenInclude(m => m.Tags)
					.ThenIncludeFiltered(m => m.Translations, request.AcceptLanguage!.LocalizedTags())
			.OrderByDescending(d => d.Date)
			.ToListAsync(cancellationToken);
		return searchResults;
	}

	private async Task<List<Day>> SearchByTextAsync(SearchQuery request, CancellationToken cancellationToken)
	{
		string likePattern = $"%{request.Text}%";

		Expression<Func<MediaFile, bool>> mediaFilter = m =>
			(request.IsAuthenticated || (m.IsApproved && !m.Private)) &&
			m.Translations.Any(mt =>
				EF.Functions.ILike(mt.Title, likePattern) ||
				EF.Functions.ILike(mt.Description, likePattern)) ||
			m.Tags.Any(t => t.Translations.Any(l =>
					EF.Functions.ILike(l.Text, request.Text)));

		var searchResults = await _context.Days
			.AsNoTracking()
			.Where(d =>
				d.Translations.Any(dt => EF.Functions.ILike(dt.Note, likePattern)) ||
				d.Media.AsQueryable().Any(mediaFilter)
			)
			.IncludeFiltered(d => d.Translations, request.AcceptLanguage!.LocalizedDays())
			.Include(d => d.Media.AsQueryable().Where(mediaFilter))
				.ThenIncludeFiltered(m => m.Translations, request.AcceptLanguage!.LocalizedMedia())
			.Include(d => d.Media.AsQueryable().Where(mediaFilter))
				.ThenInclude(m => m.Tags)
					.ThenIncludeFiltered(m => m.Translations, request.AcceptLanguage!.LocalizedTags())
			.OrderByDescending(d => d.Date)
			.ToListAsync(cancellationToken);
		return searchResults;
	}
}

using FluentValidation;
using HaruyasumiRyokouki.Backend.Common.Abstractions;
using HaruyasumiRyokouki.Backend.DbContexts;
using HaruyasumiRyokouki.Backend.Extensions;
using HaruyasumiRyokouki.Backend.Models.Db;
using HaruyasumiRyokouki.Backend.Models.Dtos;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.Linq.Expressions;
using System.Text.Json.Serialization;

namespace HaruyasumiRyokouki.Backend.Features.Search;

public record SearchQuery : IRequest<SearchResponse>, ILocalizableRequest
{
	[FromQuery]
	public required string Text { get; init; }

	[SwaggerIgnore]
	[JsonIgnore]
	public string? AcceptLanguage { get; set; }
}

public class SearchResponse
{
	public List<DayDto> Items { get; set; }
}

internal class SearchQueryValidator : AbstractValidator<SearchQuery>
{
	public SearchQueryValidator()
	{

	}
}

internal class SearchQueryHandler : IRequestHandler<SearchQuery, SearchResponse>
{
	private readonly IAppDbContext _context;

	public SearchQueryHandler(IAppDbContext context)
	{
		_context = context;
	}

	public async Task<SearchResponse> Handle(SearchQuery request, CancellationToken cancellationToken)
	{
		string likePattern = $"%{request.Text}%";

		Expression<Func<MediaFile, bool>> mediaFilter = m =>
			//m.IsApproved &&
			m.Translations.Any(mt =>
				EF.Functions.ILike(mt.Title, likePattern) ||
				EF.Functions.ILike(mt.Description, likePattern) ||
				mt.Tags.Any(tag => EF.Functions.ILike(tag, likePattern))
			);

		var searchResults = await _context.Days
			.AsNoTracking()
			.Where(d =>
				d.Translations.Any(dt => EF.Functions.ILike(dt.Note, likePattern)) ||
				d.Media.AsQueryable().Any(mediaFilter)
			)
			.Include(d => d.Translations.Where(t => t.LanguageCode == request.AcceptLanguage))
			.Include(d => d.Media.AsQueryable().Where(mediaFilter))
				.ThenInclude(m => m.Translations.Where(t => t.LanguageCode == request.AcceptLanguage))
			.OrderByDescending(d => d.Date)
			.ToListAsync(cancellationToken);

		var result = searchResults.ToDtos();

		return new SearchResponse
		{
			Items = result.ToList(),
		};
	}
}

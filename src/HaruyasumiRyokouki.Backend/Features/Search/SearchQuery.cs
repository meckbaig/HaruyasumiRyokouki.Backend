using FluentValidation;
using HaruyasumiRyokouki.Backend.Common.Abstractions;
using HaruyasumiRyokouki.Backend.DbContexts;
using HaruyasumiRyokouki.Backend.Extensions;
using HaruyasumiRyokouki.Backend.Models.Dtos;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json.Serialization;

namespace HaruyasumiRyokouki.Backend.Features.Search;

public record SearchQuery : IRequest<SearchResponse>, ILocalizableRequest
{
	[FromQuery]
	public required string Query { get; init; }

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
		string likePattern = $"%{request.Query}%";

		var searchResults = await _context.Days
			.Where(d =>
				// 1. Looking in the notes of the day
				d.Translations.Any(dt => EF.Functions.ILike(dt.Note, likePattern))
				||
				// 2. OR in this day's media
				d.Media.Any(m => m.IsApproved && m.Translations.Any(mt =>
					EF.Functions.ILike(mt.Title, likePattern) ||
					EF.Functions.ILike(mt.Description, likePattern) ||
					// Since this is an array of strings, Postgres can ILike the array elements using Any
					mt.Tags.Any(tag => EF.Functions.ILike(tag, likePattern))
				))
			)
			// Load data only for the selected language
			.Include(d => d.Translations.Where(t => t.LanguageCode == request.AcceptLanguage))
			.Include(d => d.Media/*.Where(m => m.IsApproved)*/)
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


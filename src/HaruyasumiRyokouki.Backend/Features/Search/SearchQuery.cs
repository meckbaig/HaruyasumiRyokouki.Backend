using FluentValidation;
using HaruyasumiRyokouki.Backend.Common.Abstractions;
using HaruyasumiRyokouki.Backend.Common.Helpers;
using HaruyasumiRyokouki.Backend.DbContexts;
using HaruyasumiRyokouki.Backend.Extensions;
using HaruyasumiRyokouki.Backend.Models.Db;
using HaruyasumiRyokouki.Backend.Models.Db.Enums;
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

/// <summary>
/// Request of <c>GET /v1/search</c>.
/// </summary>
/// <remarks>
/// At least one of <see cref="Text"/>, <see cref="Tag"/> and <see cref="TagId"/> is required.
/// </remarks>
public record SearchQuery : IRequest<SearchResponse>, ILocalizableRequest, IDisplayAwareRequest, IAuthenticatedRequest
{
	/// <summary>
	/// Text to search for.
	/// </summary>
	/// <remarks>
	/// When given, <see cref="Tag"/> and <see cref="TagId"/> are ignored.
	/// </remarks>
	[FromQuery]
	public string? Text { get; init; }

	/// <summary>
	/// Identifier of a tag.
	/// </summary>
	/// <remarks>
	/// Used when <see cref="Text"/> and <see cref="Tag"/> are not given.
	/// </remarks>
	[FromQuery]
	public int? TagId { get; init; }

	/// <summary>
	/// Slug of a tag, case-insensitive.
	/// </summary>
	/// <remarks>
	/// Used when <see cref="Text"/> is not given.
	/// </remarks>
	[FromQuery]
	public string? Tag { get; init; }

	/// <inheritdoc/>
	[SwaggerIgnore]
	[JsonIgnore]
	public string AcceptLanguage { get; set; } = LanguageCode.Default;

	/// <inheritdoc/>
	[SwaggerIgnore]
	[JsonIgnore]
	public ClientDisplay? ClientDisplay { get; set; }

	/// <inheritdoc/>
	[SwaggerIgnore]
	[JsonIgnore]
	public bool IsAuthenticated { get; set; }
}

/// <summary>
/// Response of <c>GET /v1/search</c>.
/// </summary>
public class SearchResponse
{
	/// <summary>
	/// Days with the matching media, newest first.
	/// </summary>
	public List<DayDto> Items { get; set; }
}

internal class SearchQueryValidator : AbstractValidator<SearchQuery>
{
	public SearchQueryValidator()
	{
		RuleFor(x => x).Must(x => !string.IsNullOrWhiteSpace(x.Text) || !string.IsNullOrWhiteSpace(x.Tag) || x.TagId.HasValue)
			.WithMessage("Text, tag or tagId must be specified.");
	}
}

internal class SearchQueryHandler : IRequestHandler<SearchQuery, SearchResponse>
{
	private readonly IAppDbContext _context;
	private readonly IMediaUrlsProvider _urlsProvider;

	public SearchQueryHandler(IAppDbContext context, IMediaUrlsProvider urlsProvider)
	{
		_context = context;
		_urlsProvider = urlsProvider;
	}

	public async Task<SearchResponse> Handle(SearchQuery request, CancellationToken cancellationToken)
	{
		// Text has priority, tags are used only when there is no text.
		var searchFunction = !string.IsNullOrWhiteSpace(request.Text)
			? SearchByTextAsync(request, cancellationToken)
			: SearchByTagAsync(request, cancellationToken);

		List<Day> searchResults = await searchFunction;

		var searchResultsDtos = searchResults.ToDtos(request.IsAuthenticated);
		var result = searchResultsDtos.AddUrls(searchResults, _urlsProvider, request.ClientDisplay);

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
								&&	m.MediaTags.Any(t => t.TagId == request.TagId!.Value);
		}
		if (!string.IsNullOrWhiteSpace(request.Tag))
		{
			string tagPattern = LikePattern.Exact(request.Tag);
			mediaFilter = m => (request.IsAuthenticated || (m.IsApproved && !m.Private))
								&& m.Tags.Any(t => EF.Functions.ILike(t.Slug, tagPattern, LikePattern.EscapeCharacter));
		}
		if (mediaFilter == null)
			throw new ArgumentNullException("Media filter is null.");

		var searchResults = await _context.Days
			.AsNoTracking()
			.Where(d => d.Media.AsQueryable().Any(mediaFilter))
			.IncludeFiltered(d => d.Translations, request.AcceptLanguage.LocalizedDays())
			.Include(d => d.Media.AsQueryable().Where(mediaFilter).OrderBy(m => m.Created))
				.ThenIncludeFiltered(m => m.Translations, request.AcceptLanguage.LocalizedMedia())
			.Include(d => d.Media.AsQueryable().Where(mediaFilter).OrderBy(m => m.Created))
				.ThenInclude(m => m.Tags)
					.ThenIncludeFiltered(m => m.Translations, request.AcceptLanguage.LocalizedTags())
			.OrderByDescending(d => d.Date)
			.ToListAsync(cancellationToken);
		return searchResults;
	}

	private async Task<List<Day>> SearchByTextAsync(SearchQuery request, CancellationToken cancellationToken)
	{
		string likePattern = LikePattern.Contains(request.Text!);
		string tagPattern = LikePattern.Exact(request.Text!);

		Expression<Func<MediaFile, bool>> mediaFilter = m =>
			(request.IsAuthenticated || (m.IsApproved && !m.Private)) &&
			(
				m.Translations.Any(mt =>
					EF.Functions.ILike(mt.Title, likePattern, LikePattern.EscapeCharacter) ||
					EF.Functions.ILike(mt.Description, likePattern, LikePattern.EscapeCharacter)) ||
				m.Tags.Any(t => t.Translations.Any(l =>
						EF.Functions.ILike(l.Text, tagPattern, LikePattern.EscapeCharacter)))
			);

		var searchResults = await _context.Days
			.AsNoTracking()
			.Where(d =>
				d.Translations.Any(dt => EF.Functions.ILike(dt.Note, likePattern, LikePattern.EscapeCharacter)) ||
				d.Media.AsQueryable().Any(mediaFilter)
			)
			.IncludeFiltered(d => d.Translations, request.AcceptLanguage.LocalizedDays())
			.Include(d => d.Media.AsQueryable().Where(mediaFilter).OrderBy(m => m.Created))
				.ThenIncludeFiltered(m => m.Translations, request.AcceptLanguage.LocalizedMedia())
			.Include(d => d.Media.AsQueryable().Where(mediaFilter).OrderBy(m => m.Created))
				.ThenInclude(m => m.Tags)
					.ThenIncludeFiltered(m => m.Translations, request.AcceptLanguage.LocalizedTags())
			.OrderByDescending(d => d.Date)
			.ToListAsync(cancellationToken);
		return searchResults;
	}
}

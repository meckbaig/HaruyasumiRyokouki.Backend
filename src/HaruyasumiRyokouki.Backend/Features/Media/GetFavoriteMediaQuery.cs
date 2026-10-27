using FluentValidation;
using HaruyasumiRyokouki.Backend.Common.Abstractions;
using HaruyasumiRyokouki.Backend.Common.Options;
using HaruyasumiRyokouki.Backend.DbContexts;
using HaruyasumiRyokouki.Backend.Extensions;
using HaruyasumiRyokouki.Backend.Models.Db.Enums;
using HaruyasumiRyokouki.Backend.Models.Dtos.Media;
using HaruyasumiRyokouki.Backend.Models.InternalDtos;
using HaruyasumiRyokouki.Backend.Services.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json.Serialization;

namespace HaruyasumiRyokouki.Backend.Features.Media;

/// <summary>
/// Request of <c>GET /v1/media/favorites</c>.
/// </summary>
public record GetFavoriteMediaQuery : IRequest<GetFavoriteMediaResponse>, ILocalizableRequest, IDisplayAwareRequest, IAuthenticatedRequest
{
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
/// Response of <c>GET /v1/media/favorites</c>.
/// </summary>
public class GetFavoriteMediaResponse
{
	/// <summary>
	/// Random favorite media, <c>MediaFormat:FavoritesReturnCount</c> at most.
	/// </summary>
	public required ICollection<MediaFileDto> Items { get; init; }
}

internal class GetFavoriteMediaQueryHandler : IRequestHandler<GetFavoriteMediaQuery, GetFavoriteMediaResponse>
{
	private readonly IAppDbContext _context;
	private readonly IMediaUrlsProvider _urlsProvider;
	private readonly MediaFormatOptions _mediaFormatOptions;

	public GetFavoriteMediaQueryHandler(IAppDbContext context, IMediaUrlsProvider urlsProvider, IOptions<MediaFormatOptions> mediaFormatOptions)
	{
		_context = context;
		_urlsProvider = urlsProvider;
		_mediaFormatOptions = mediaFormatOptions.Value;
	}

	public async Task<GetFavoriteMediaResponse> Handle(GetFavoriteMediaQuery request, CancellationToken cancellationToken)
	{
		var mediaFiles = await _context.MediaFiles
			.AsNoTracking()
			.IncludeFiltered(m => m.Translations, request.AcceptLanguage.LocalizedMedia())
			.Include(m => m.Tags)
				.ThenIncludeFiltered(t => t.Translations, request.AcceptLanguage.LocalizedTags())
			.Where(m => (request.IsAuthenticated || !m.Private) && m.Favorite)
			.OrderBy(_ => EF.Functions.Random())
			.Take(_mediaFormatOptions.FavoritesReturnCount)
			.ToListAsync(cancellationToken);

		var results = mediaFiles.Select(m => m.ToDto(request.IsAuthenticated).AddUrls(m, _urlsProvider, request.ClientDisplay));

		return new GetFavoriteMediaResponse
		{
			Items = results.ToList()
		};
	}
}


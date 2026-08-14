using FluentValidation;
using HaruyasumiRyokouki.Backend.Common.Abstractions;
using HaruyasumiRyokouki.Backend.Common.Options;
using HaruyasumiRyokouki.Backend.DbContexts;
using HaruyasumiRyokouki.Backend.Extensions;
using HaruyasumiRyokouki.Backend.Models.Dtos.Media;
using HaruyasumiRyokouki.Backend.Models.InternalDtos;
using HaruyasumiRyokouki.Backend.Services.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json.Serialization;

namespace HaruyasumiRyokouki.Backend.Features.Media;

public record GetFavoriteMediaQuery : IRequest<GetFavoriteMediaResponse>, ILocalizableRequest, IDisplayAwareRequest, IAuthentificatedRequest
{
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

public class GetFavoriteMediaResponse
{
	public required ICollection<MediaFileDto> Items { get; init; }
}

internal class GetFavoriteMediaQueryHandler : IRequestHandler<GetFavoriteMediaQuery, GetFavoriteMediaResponse>
{
	private readonly IAppDbContext _context;
	private readonly IMediaPreviewService _previewService;
	private readonly MediaFormatOptions _mediaFormatOptions;
	private const bool IncludeFavorites = true;

	public GetFavoriteMediaQueryHandler(IAppDbContext context, IMediaPreviewService previewService, IOptions<MediaFormatOptions> mediaFormatOptions)
	{
		_context = context;
		_previewService = previewService;
		_mediaFormatOptions = mediaFormatOptions.Value;
	}

	public async Task<GetFavoriteMediaResponse> Handle(GetFavoriteMediaQuery request, CancellationToken cancellationToken)
	{
		var mediaFileDtos = await _context.MediaFiles
			.AsNoTracking()
			.IncludeFiltered(m => m.Translations, request.AcceptLanguage.LocalizedMedia())
			.Include(m => m.Tags).ThenIncludeFiltered(t => t.Translations, request.AcceptLanguage.LocalizedTags())
			.Where(m => (request.IsAuthenticated || !m.Private) && m.Favorite)
			.OrderBy(_ => EF.Functions.Random())
			.Take(_mediaFormatOptions.FavoritesReturnCount)
			.Select(m => m.ToDto(IncludeFavorites, request.IsAuthenticated))
			.ToListAsync(cancellationToken);

		var results = mediaFileDtos.Select(dto => dto.AddUrls(_previewService, request.ClientDisplay));

		return new GetFavoriteMediaResponse
		{
			Items = results.ToList()
		};
	}
}


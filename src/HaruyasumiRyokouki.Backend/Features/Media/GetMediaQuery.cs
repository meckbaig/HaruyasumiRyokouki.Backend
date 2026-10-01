using FluentValidation;
using HaruyasumiRyokouki.Backend.Common.Abstractions;
using HaruyasumiRyokouki.Backend.DbContexts;
using HaruyasumiRyokouki.Backend.Extensions;
using HaruyasumiRyokouki.Backend.Extensions.TypeExtensions;
using HaruyasumiRyokouki.Backend.Models.Dtos.Media;
using HaruyasumiRyokouki.Backend.Models.InternalDtos;
using HaruyasumiRyokouki.Backend.Services.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json.Serialization;

namespace HaruyasumiRyokouki.Backend.Features.Media;

public record GetMediaQuery : IRequest<GetMediaResponse>, ILocalizableRequest, IDisplayAwareRequest, IAuthentificatedRequest
{
	[FromQuery]
	public required DateOnly From { get; set; }

	[FromQuery]
	public required DateOnly To { get; set; }

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

internal class GetMediaQueryValidator : AbstractValidator<GetMediaQuery>
{
	public GetMediaQueryValidator()
	{
		/// TODO: must have valid AcceptLanguage
	}
}

public class GetMediaResponse
{
	public required ICollection<MediaFileDto> Items { get; init; }
}

internal class GetMediaQueryHandler : IRequestHandler<GetMediaQuery, GetMediaResponse>
{
	private readonly IAppDbContext _context;
	private readonly IMediaUrlsProvider _urlsProvider;

	public GetMediaQueryHandler(IAppDbContext context, IMediaUrlsProvider urlsProvider)
	{
		_context = context;
		_urlsProvider = urlsProvider;
	}

	public async Task<GetMediaResponse> Handle(GetMediaQuery request, CancellationToken cancellationToken)
	{
		var fromDate = request.From.ToLocalDateTime(TimeOnly.MinValue);
		var toDate = request.To.ToLocalDateTime(TimeOnly.MaxValue);

		var mediaFiles = await _context.MediaFiles
			.AsNoTracking()
			.IncludeFiltered(m => m.Translations, request.AcceptLanguage!.LocalizedMedia())
			.Include(m => m.Tags)
				.ThenIncludeFiltered(t => t.Translations, request.AcceptLanguage.LocalizedTags())
			.Where(m =>
				(request.IsAuthenticated || (m.IsApproved && !m.Private)) &&
				m.Created >= fromDate &&
				m.Created <= toDate &&
				m.Latitude != null &&
				m.Longitude != null)
			.OrderBy(m => m.Created)
			.ToListAsync(cancellationToken);

		var results = mediaFiles.Select(m => m.ToDto(request.IsAuthenticated).AddUrls(m, _urlsProvider, request.ClientDisplay));

		return new GetMediaResponse
		{
			Items = results.ToList()
		};
	}
}


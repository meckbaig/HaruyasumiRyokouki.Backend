using FluentValidation;
using HaruyasumiRyokouki.Backend.Common.Abstractions;
using HaruyasumiRyokouki.Backend.DbContexts;
using HaruyasumiRyokouki.Backend.Extensions;
using HaruyasumiRyokouki.Backend.Extensions.TypeExtensions;
using HaruyasumiRyokouki.Backend.Models.Db;
using HaruyasumiRyokouki.Backend.Models.Dtos.Media;
using HaruyasumiRyokouki.Backend.Models.InternalDtos;
using HaruyasumiRyokouki.Backend.Services.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json.Serialization;

namespace HaruyasumiRyokouki.Backend.Features.Media;

public record GetMediaLocationsQuery : IRequest<GetMediaLocationsResponse>, ILocalizableRequest, IDisplayAwareRequest, IAuthentificatedRequest
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

internal class GetMediaLocationsQueryValidator : AbstractValidator<GetMediaLocationsQuery>
{
	public GetMediaLocationsQueryValidator()
	{
		/// TODO: must have valid AcceptLanguage
	}
}

public class GetMediaLocationsResponse
{
	public required ICollection<MediaFileLocationDto> Items { get; init; }
}

internal class GetMediaLocationsQueryHandler : IRequestHandler<GetMediaLocationsQuery, GetMediaLocationsResponse>
{
	private readonly IAppDbContext _context;
	private readonly IMediaPreviewService _previewService;

	public GetMediaLocationsQueryHandler(IAppDbContext context, IMediaPreviewService previewService)
	{
		_context = context;
		_previewService = previewService;
	}

	public async Task<GetMediaLocationsResponse> Handle(GetMediaLocationsQuery request, CancellationToken cancellationToken)
	{
		var fromDate = request.From.ToLocalDateTime(TimeOnly.MinValue);
		var toDate = request.To.ToLocalDateTime(TimeOnly.MaxValue);

		var mediaFiles = await _context.MediaFiles
			.AsNoTracking()
			.IncludeFiltered(m => m.Translations, request.AcceptLanguage!.LocalizedMedia())
			.Where(m =>
				(request.IsAuthenticated || (m.IsApproved && !m.Private)) &&
				m.Created >= fromDate &&
				m.Created <= toDate &&
				m.Latitude != null &&
				m.Longitude != null)
			.OrderBy(m => m.Created)
			.ToListAsync(cancellationToken);

		var results = mediaFiles.Select(m => ToLocationDto(m).AddUrls(m.AdditionalFiles, _previewService, request.ClientDisplay));

		return new GetMediaLocationsResponse
		{
			Items = results.ToList()
		};
	}

	private static MediaFileLocationDto ToLocationDto(MediaFile source)
	{
		return new MediaFileLocationDto
		{
			Id = source.Id,
			Created = source.Created,
			FileName = source.FileName,
			AspectRatio = source.AspectRatio,
			Latitude = source.Latitude ?? 0,
			Longitude = source.Longitude ?? 0,
			Miniature = source.Miniature,
			Type = source.Type.ToString(),
			LanguageCode = source.Translations.Select(t => t.LanguageCode).FirstOrDefault(),
			Title = source.Translations.Select(t => t.Title).FirstOrDefault()
		};
	}
}


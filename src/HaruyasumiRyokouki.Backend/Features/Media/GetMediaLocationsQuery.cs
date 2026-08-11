using FluentValidation;
using HaruyasumiRyokouki.Backend.Common.Abstractions;
using HaruyasumiRyokouki.Backend.DbContexts;
using HaruyasumiRyokouki.Backend.Extensions;
using HaruyasumiRyokouki.Backend.Extensions.TypeExtensions;
using HaruyasumiRyokouki.Backend.Models.Dtos;
using HaruyasumiRyokouki.Backend.Models.InternalDtos;
using HaruyasumiRyokouki.Backend.Services.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json.Serialization;

namespace HaruyasumiRyokouki.Backend.Features.Media;

public record GetMediaLocationsQuery : IRequest<GetMediaLocationsResponse>, ILocalizableRequest, IDisplayAwareRequest
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

		var mediaFileLocationDtos = await _context.MediaFiles
			.AsNoTracking()
			.Where(m =>
				m.Created >= fromDate &&
				m.Created <= toDate &&
				m.Latitude != null &&
				m.Longitude != null)
			.OrderBy(m => m.Created)
			.Select(m => ToLocationDto(m, request.AcceptLanguage))
			.ToListAsync(cancellationToken);

		var results = mediaFileLocationDtos.Select(dto => dto.AddUrls(_previewService, request.ClientDisplay));

		return new GetMediaLocationsResponse
		{
			Items = results.ToList()
		};
	}

	private static MediaFileLocationDto ToLocationDto(Models.Db.MediaFile source, string languageCode)
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
			LanguageCode = languageCode,
			Title = source.Translations
							.Where(t => t.LanguageCode == languageCode)
							.Select(t => t.Title)
							.FirstOrDefault()
		};
	}
}


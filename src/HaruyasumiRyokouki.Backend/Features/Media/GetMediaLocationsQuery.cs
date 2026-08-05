using FluentValidation;
using HaruyasumiRyokouki.Backend.Common.Abstractions;
using HaruyasumiRyokouki.Backend.DbContexts;
using HaruyasumiRyokouki.Backend.Extensions.TypeExtensions;
using HaruyasumiRyokouki.Backend.Models.Dtos;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json.Serialization;

namespace HaruyasumiRyokouki.Backend.Features.Media;

public record GetMediaLocationsQuery : IRequest<GetMediaLocationsResponse>, ILocalizableRequest
{
	[FromQuery]
	public required DateOnly From { get; set; }

	[FromQuery]
	public required DateOnly To { get; set; }

	[SwaggerIgnore]
	[JsonIgnore]
	public string? AcceptLanguage { get; set; }
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

	public GetMediaLocationsQueryHandler(IAppDbContext context)
	{
		_context = context;
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
			.Select(m => new MediaFileLocationDto
			{
				Id = m.Id,
				Created = m.Created,
				FileName = m.FileName,
				Latitude = m.Latitude ?? 0,
				Longitude = m.Longitude ?? 0,
				Miniature = m.Miniature,
				LanguageCode = request.AcceptLanguage,
				Title = m.Translations
					.Where(t => t.LanguageCode == request.AcceptLanguage)
					.Select(t => t.Title)
					.FirstOrDefault()
			})
			.ToListAsync(cancellationToken);

		var mediaFileLocationDtos21 = await _context.MediaFiles
			.AsNoTracking()
			.Include(m => m.Translations.Where(t => t.LanguageCode == request.AcceptLanguage))
			.Where(m =>
				m.Created >= fromDate &&
				m.Created <= toDate &&
				m.Latitude != null &&
				m.Longitude != null)
			.OrderBy(m => m.Created)
			.Select(m => new MediaFileLocationDto
			{
				Id = m.Id,
				Created = m.Created,
				FileName = m.FileName,
				Latitude = m.Latitude ?? 0,
				Longitude = m.Longitude ?? 0,
				LanguageCode = request.AcceptLanguage,
				Title = m.Translations
					.Select(t => t.Title)
					.FirstOrDefault()
			})
			.ToListAsync(cancellationToken);

		return new GetMediaLocationsResponse
		{
			Items = mediaFileLocationDtos
		};
	}
}


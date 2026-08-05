using FluentValidation;
using HaruyasumiRyokouki.Backend.Common.Abstractions;
using HaruyasumiRyokouki.Backend.Common.Exceptions;
using HaruyasumiRyokouki.Backend.DbContexts;
using HaruyasumiRyokouki.Backend.Extensions;
using HaruyasumiRyokouki.Backend.Models.Dtos;
using HaruyasumiRyokouki.Backend.Services.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json.Serialization;

namespace HaruyasumiRyokouki.Backend.Features.Days;

public record GetDayQuery : IRequest<GetDayResponse>, ILocalizableRequest
{
	[FromRoute]
	public required DateOnly Date { get; set; }

	[SwaggerIgnore]
	[JsonIgnore]
	public string? AcceptLanguage { get; set; }
}

public class GetDayResponse
{
	public DayDto? Day { get; set; }
}

internal class GetDayQueryValidator : AbstractValidator<GetDayQuery>
{
	public GetDayQueryValidator()
	{

	}
}

internal class GetDayQueryHandler : IRequestHandler<GetDayQuery, GetDayResponse>
{
	private readonly IAppDbContext _context;
	private readonly IMediaPreviewService _previewService;

	public GetDayQueryHandler(IAppDbContext context, IMediaPreviewService previewService)
	{
		_context = context;
		_previewService = previewService;
	}

	public async Task<GetDayResponse> Handle(GetDayQuery request, CancellationToken cancellationToken)
	{
		var searchResults = await _context.Days
			.AsNoTracking()
			.Include(d => d.Media.OrderBy(d => d.Created))
			.ThenInclude(m => m.Translations.Where(t => t.LanguageCode == request.AcceptLanguage))
			.Include(d => d.Translations.Where(t => t.LanguageCode == request.AcceptLanguage))
			.FirstOrDefaultAsync(d => d.Date == request.Date, cancellationToken);

		if (searchResults == null)
			throw new EntityNotFoundException($"Day {request.Date:yyyy-MM-dd} doesn't exist");

		var searchResultsDtos = searchResults.ToDto();
		var result = searchResultsDtos.AddUrls(_previewService);

		return new GetDayResponse
		{
			Day = result,
		};
	}
}


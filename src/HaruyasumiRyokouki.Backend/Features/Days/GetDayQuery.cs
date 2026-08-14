using FluentValidation;
using HaruyasumiRyokouki.Backend.Common.Abstractions;
using HaruyasumiRyokouki.Backend.Common.Exceptions;
using HaruyasumiRyokouki.Backend.DbContexts;
using HaruyasumiRyokouki.Backend.Extensions;
using HaruyasumiRyokouki.Backend.Models.Dtos.Days;
using HaruyasumiRyokouki.Backend.Models.InternalDtos;
using HaruyasumiRyokouki.Backend.Services.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json.Serialization;

namespace HaruyasumiRyokouki.Backend.Features.Days;

public record GetDayQuery : IRequest<GetDayResponse>, ILocalizableRequest, IDisplayAwareRequest, IAuthentificatedRequest
{
	[FromRoute]
	public required DateOnly Date { get; set; }

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
			.Include(d => d.Media.Where(m => request.IsAuthenticated || !m.Private).OrderBy(d => d.Created))
			.ThenIncludeFiltered(m => m.Translations, request.AcceptLanguage!.LocalizedMedia())
			.IncludeFiltered(d => d.Translations, request.AcceptLanguage!.LocalizedDays())
			.FirstOrDefaultAsync(d => d.Date == request.Date, cancellationToken);

		if (searchResults == null)
			throw new EntityNotFoundException($"Day {request.Date:yyyy-MM-dd} doesn't exist");

		var searchResultsDtos = searchResults.ToDto(request.IsAuthenticated);
		var result = searchResultsDtos.AddUrls(_previewService, request.ClientDisplay);

		return new GetDayResponse
		{
			Day = result,
		};
	}
}


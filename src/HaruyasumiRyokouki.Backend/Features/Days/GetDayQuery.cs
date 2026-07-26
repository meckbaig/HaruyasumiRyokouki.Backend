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

	public GetDayQueryHandler(IAppDbContext context)
	{
		_context = context;
	}

	public async Task<GetDayResponse> Handle(GetDayQuery request, CancellationToken cancellationToken)
	{
		var result = await _context.Days
			.AsNoTracking()
			.Include(d => d.Media.OrderBy(d => d.Created))
			.ThenInclude(m => m.Translations.Where(t => t.LanguageCode == request.AcceptLanguage))
			.Include(d => d.Translations.Where(t => t.LanguageCode == request.AcceptLanguage))
			.FirstOrDefaultAsync(d => d.Date == request.Date, cancellationToken);

		return new GetDayResponse
		{
			Day = result?.ToDto()
		};
	}
}


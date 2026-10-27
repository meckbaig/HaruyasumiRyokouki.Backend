using HaruyasumiRyokouki.Backend.Common.Exceptions;
using HaruyasumiRyokouki.Backend.DbContexts;
using HaruyasumiRyokouki.Backend.Extensions;
using HaruyasumiRyokouki.Backend.Models.Dtos.Days;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HaruyasumiRyokouki.Backend.Features.Days;

/// <summary>
/// Request of <c>GET /v1/days/{date}/edit</c>.
/// </summary>
public record GetEditDayQuery : IRequest<GetEditDayResponse>
{
	/// <summary>
	/// Date of the day.
	/// </summary>
	[FromRoute]
	public required DateOnly Date { get; set; }
}

/// <summary>
/// Response of <c>GET /v1/days/{date}/edit</c>.
/// </summary>
public class GetEditDayResponse
{
	/// <summary>
	/// The day with notes in all languages.
	/// </summary>
	public required DayEditDto Day { get; init; }
}

internal class GetEditDayQueryHandler : IRequestHandler<GetEditDayQuery, GetEditDayResponse>
{
	private readonly IAppDbContext _context;

	public GetEditDayQueryHandler(IAppDbContext context)
	{
		_context = context;
	}

	public async Task<GetEditDayResponse> Handle(GetEditDayQuery request, CancellationToken cancellationToken)
	{
		var result = await _context.Days
			.AsNoTracking()
			.Include(d => d.Translations)
			.FirstOrDefaultAsync(d => d.Date == request.Date, cancellationToken)
			?? throw new EntityNotFoundException($"Day {request.Date:yyyy-MM-dd} doesn't exist");

		return new GetEditDayResponse
		{
			Day = result.ToEditDto()
		};
	}
}


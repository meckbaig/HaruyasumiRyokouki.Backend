using HaruyasumiRyokouki.Backend.DbContexts;
using HaruyasumiRyokouki.Backend.Extensions;
using HaruyasumiRyokouki.Backend.Models.Dtos.Days;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HaruyasumiRyokouki.Backend.Features.Days;

public record GetEditDayQuery : IRequest<GetEditDayResponse>
{
	[FromRoute]
	public required DateOnly Date { get; set; }
}

public class GetEditDayResponse
{
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
			.FirstOrDefaultAsync(d => d.Date == request.Date, cancellationToken);

		return new GetEditDayResponse
		{
			Day = result?.ToEditDto()
		};
	}
}


using HaruyasumiRyokouki.Backend.DbContexts;
using HaruyasumiRyokouki.Backend.Models.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HaruyasumiRyokouki.Backend.Features.Days;

public record GetDaysQuery : IRequest<GetDaysResponse>
{
}

public class GetDaysResponse
{
	public List<DayShortDto> Items { get; set; }
}

internal class GetDaysQueryHandler : IRequestHandler<GetDaysQuery, GetDaysResponse>
{
	private readonly IAppDbContext _context;

	public GetDaysQueryHandler(IAppDbContext context)
	{
		_context = context;
	}

	public async Task<GetDaysResponse> Handle(GetDaysQuery request, CancellationToken cancellationToken)
	{
		var days = await _context.Days
			.AsNoTracking()
			.OrderBy(d => d.Date)
			.Select(d => new DayShortDto { Date = d.Date, IsReady = d.IsReady, MediaCount = d.Media.Count })
			.ToListAsync(cancellationToken);

		return new GetDaysResponse
		{
			Items = days,
		};
	}
}


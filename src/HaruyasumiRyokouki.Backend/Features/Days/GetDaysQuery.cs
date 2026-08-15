using HaruyasumiRyokouki.Backend.Common.Abstractions;
using HaruyasumiRyokouki.Backend.DbContexts;
using HaruyasumiRyokouki.Backend.Models.Dtos.Days;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json.Serialization;

namespace HaruyasumiRyokouki.Backend.Features.Days;

public record GetDaysQuery : IRequest<GetDaysResponse>, IAuthentificatedRequest
{
	[SwaggerIgnore]
	[JsonIgnore]
	public bool IsAuthenticated { get; set; }
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
			.Include(d => d.Media.Where(m => request.IsAuthenticated || (m.IsApproved && !m.Private)))
			.OrderBy(d => d.Date)
			.Select(d => new DayShortDto { Date = d.Date, IsReady = d.IsReady, MediaCount = d.Media.Count })
			.ToListAsync(cancellationToken);

		return new GetDaysResponse
		{
			Items = days,
		};
	}
}


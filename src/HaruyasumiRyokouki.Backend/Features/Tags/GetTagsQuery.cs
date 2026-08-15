using HaruyasumiRyokouki.Backend.DbContexts;
using HaruyasumiRyokouki.Backend.Extensions;
using HaruyasumiRyokouki.Backend.Models.Dtos.Tags;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HaruyasumiRyokouki.Backend.Features.Tags;

public record GetTagsQuery : IRequest<GetTagsResponse>
{
}

public class GetTagsResponse
{
	public List<TagDto> Items { get; set; }
}

internal class GetTagsQueryHandler : IRequestHandler<GetTagsQuery, GetTagsResponse>
{
	private readonly IAppDbContext _context;

	public GetTagsQueryHandler(IAppDbContext context)
	{
		_context = context;
	}

	public async Task<GetTagsResponse> Handle(GetTagsQuery request, CancellationToken cancellationToken)
	{
		var tags = await _context.Tags
			.AsNoTracking()
			.Include(t => t.Translations)
			.OrderByDescending(t => t.Media.Count)
			.Select(t => t.ToDto())
			.ToListAsync(cancellationToken);

		return new GetTagsResponse
		{
			Items = tags
		};
	}
}


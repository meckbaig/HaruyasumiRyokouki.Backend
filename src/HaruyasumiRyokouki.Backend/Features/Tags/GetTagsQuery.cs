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
	private readonly ILogger<GetTagsQueryHandler> _logger;

	public GetTagsQueryHandler(IAppDbContext context, ILogger<GetTagsQueryHandler> logger)
	{
		_context = context;
		_logger = logger;
	}

	public async Task<GetTagsResponse> Handle(GetTagsQuery request, CancellationToken cancellationToken)
	{
		var tags = await _context.Tags
			.AsNoTracking()
			.Include(t => t.Translations)
			.Select(t => new { Tag = t, Count = t.MediaTags.Count })
			.ToListAsync(cancellationToken);

		var tagDtos = tags
			.Select(t => t.Tag.ToDto(t.Count))
			.OrderByDescending(t => t.UsageCount)
			.ToList();

		return new GetTagsResponse
		{
			Items = tagDtos
		};
	}
}


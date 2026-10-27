using HaruyasumiRyokouki.Backend.DbContexts;
using HaruyasumiRyokouki.Backend.Extensions;
using HaruyasumiRyokouki.Backend.Models.Dtos.Tags;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HaruyasumiRyokouki.Backend.Features.Tags;

/// <summary>
/// Request of <c>GET /v1/tags</c>.
/// </summary>
public record GetTagsQuery : IRequest<GetTagsResponse>
{
}

/// <summary>
/// Response of <c>GET /v1/tags</c>.
/// </summary>
public class GetTagsResponse
{
	/// <summary>
	/// All tags with all translations and aliases, most used first.
	/// </summary>
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


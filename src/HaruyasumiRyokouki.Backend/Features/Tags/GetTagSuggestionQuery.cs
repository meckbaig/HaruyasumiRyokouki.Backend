using HaruyasumiRyokouki.Backend.Common.Abstractions;
using HaruyasumiRyokouki.Backend.DbContexts;
using HaruyasumiRyokouki.Backend.Extensions;
using HaruyasumiRyokouki.Backend.Models.Dtos.Tags;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json.Serialization;

namespace HaruyasumiRyokouki.Backend.Features.Tags;

public record GetTagSuggestionQuery : IRequest<GetTagSuggestionResponse>, ILocalizableRequest, IAuthentificatedRequest
{
	[FromQuery]
	public required string Text { get; set; }

	[FromQuery]
	public int Take { get; set; } = 8;

	[SwaggerIgnore]
	[JsonIgnore]
	public string? AcceptLanguage { get; set; }

	[SwaggerIgnore]
	[JsonIgnore]
	public bool IsAuthenticated { get; set; }
}

public class GetTagSuggestionResponse
{
	public required ICollection<TagSuggestionDto> Items { get; set; }
}

internal class GetTagSuggestionQueryHandler : IRequestHandler<GetTagSuggestionQuery, GetTagSuggestionResponse>
{
	private readonly IAppDbContext _context;

	public GetTagSuggestionQueryHandler(IAppDbContext context)
	{
		_context = context;
	}

	public async Task<GetTagSuggestionResponse> Handle(GetTagSuggestionQuery request, CancellationToken cancellationToken)
	{
		string likePattern = $"%{request.Text}%";

		var tags = await _context.Tags
			.Include(t => t.MediaTags.Where(mt => request.IsAuthenticated || (mt.Media.IsApproved && !mt.Media.Private)))
			.IncludeFiltered(d => d.Translations, request.AcceptLanguage!.LocalizedTags())
			.Where(t => t.Translations.Any(l => EF.Functions.ILike(l.Text, likePattern)))
			.OrderByDescending(t => t.MediaTags.Count)
			.Select(t => t.ToSuggestionDto(request.AcceptLanguage))
			.Take(request.Take)
			.ToListAsync(cancellationToken);

		return new GetTagSuggestionResponse
		{
			Items = tags.ToList(),
		};
	}
}


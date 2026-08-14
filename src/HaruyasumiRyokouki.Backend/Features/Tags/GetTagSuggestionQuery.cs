using HaruyasumiRyokouki.Backend.Common.Abstractions;
using HaruyasumiRyokouki.Backend.DbContexts;
using HaruyasumiRyokouki.Backend.Extensions;
using HaruyasumiRyokouki.Backend.Models.Dtos.Tags;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json.Serialization;

namespace HaruyasumiRyokouki.Backend.Features.Tags;

public record GetTagSuggestionQuery : IRequest<GetTagSuggestionResponse>, ILocalizableRequest
{
	[FromQuery]
	public required string Text { get; set; }

	[FromQuery]
	public int Take { get; set; } = 8;

	[SwaggerIgnore]
	[JsonIgnore]
	public string? AcceptLanguage { get; set; }
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

		var tags = await _context.Tags.SearchAsync
		(
			likePattern,
			t => t.Media.Count,
			t => t.ToSuggestionDto(request.AcceptLanguage),
			request.Take,
			cancellationToken
		);

		return new GetTagSuggestionResponse
		{
			Items = tags.ToList(),
		};
	}
}


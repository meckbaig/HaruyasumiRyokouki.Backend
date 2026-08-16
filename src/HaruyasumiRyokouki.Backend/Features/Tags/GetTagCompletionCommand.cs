using HaruyasumiRyokouki.Backend.DbContexts;
using HaruyasumiRyokouki.Backend.Extensions;
using HaruyasumiRyokouki.Backend.Models.Db.Enums;
using HaruyasumiRyokouki.Backend.Models.Dtos.Tags;
using HaruyasumiRyokouki.Backend.Services.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace HaruyasumiRyokouki.Backend.Features.Tags;

public record GetTagCompletionCommand : IRequest<GetTagCompletionResponse>
{
	[FromBody]
	public required BodyParameters Body { get; init; }

	public record BodyParameters
	{
		public required string Tag { get; init; }
	}
}

public class GetTagCompletionResponse
{
	public required TagDto Tag { get; set; }
	public required ICollection<TagDto> SimilarExisting { get; set; }
}

internal class GetTagCompletionQueryHandler : IRequestHandler<GetTagCompletionCommand, GetTagCompletionResponse>
{
	private readonly IAiChatService _aiChatService;
	private readonly IAppDbContext _context;
	private readonly ILogger<GetTagCompletionQueryHandler> _logger;
	private readonly static JsonSerializerOptions _options = new JsonSerializerOptions
	{
		PropertyNameCaseInsensitive = true
	};

	private static readonly string _structurePrompt = $$"""
You are a tag localization and search-alias generator.

Given a user-provided tag:

1. Detect its language.
2. Translate it into {{nameof(LanguageCode.English)}}, {{nameof(LanguageCode.Russian)}} and {{nameof(LanguageCode.Japanese)}}.
3. Generate only genuine aliases: synonyms, alternative names, abbreviations, transliterations, spelling variants, or equivalent word-order variants.
4. An alias MUST have the same meaning and semantic scope as the original tag.
5. NEVER broaden, narrow, qualify, describe, or contextualize the original tag.
6. NEVER add modifiers such as country, nationality, region, city, culture, language, cuisine, style, type, category, attribute, ingredient, brand, etc.
7. In particular, never add words meaning "Japanese", "Japan", "日本", "日本の", "японский", "Япония", or equivalent contextual modifiers.
8. Do not generate "[modifier] + [original tag]" variants.
9. Do not generate aliases merely because the concepts are related or commonly associated.
10. Do not include the original tag or duplicates.
11. If no genuine alias exists, return no aliases. Zero aliases is preferable to a low-quality alias.
12. Return only valid JSON.

An alias must be interchangeable with the original tag in search without changing the expected results. These aliases are used as independent search tags on a Japan-focused website, so contextual modifiers must not be used.

Input:
{
  "tag": "user-provided tag"
}

Output:
{
  "translations": [
    {
      "text": "string",
      "languageCode": "language code"
    }
  ],
  "aliases": [
    {
      "text": "string",
      "languageCode": "language code"
    }
  ]
}

Rules:
- `translations`: exactly 3 items.
- `aliases`: maximum 5 per language.
- `languageCode`: one of {{LanguageCode.English}}, {{LanguageCode.Russian}}, {{LanguageCode.Japanese}}.
- Aliases may be empty.
- Never generate an alias just to fill the limit.
""";

	public GetTagCompletionQueryHandler(IAiChatService aiChatService, IAppDbContext context, ILogger<GetTagCompletionQueryHandler> logger)
	{
		_aiChatService = aiChatService;
		_context = context;
		_logger = logger;
	}

	public async Task<GetTagCompletionResponse> Handle(GetTagCompletionCommand request, CancellationToken cancellationToken)
	{
		var suggestionTask = GetAiSuggestionAsync(request, _structurePrompt, cancellationToken);
		var searchTask = FindExistingTagsAsync(request, cancellationToken);
		await Task.WhenAll(suggestionTask, searchTask);

		var result = await suggestionTask;
		var existingTags = await searchTask;

		return new GetTagCompletionResponse
		{
			Tag = result,
			SimilarExisting = existingTags.ToList()
		};
	}

	private async Task<IEnumerable<TagDto>> FindExistingTagsAsync(GetTagCompletionCommand request, CancellationToken cancellationToken)
	{
		string likePattern = $"%{request.Body.Tag}%";

		var existingTags = await _context.Tags
			.Include(t => t.MediaTags)
			.Include(d => d.Translations)
			.Where(t => t.Translations.Any(l => EF.Functions.ILike(l.Text, likePattern)))
			.OrderByDescending(t => t.MediaTags.Count)
			.Select(t => new { Tag = t, Count = t.MediaTags.Count })
			.Take(8)
			.ToListAsync(cancellationToken);

		return existingTags.Select(t => t.Tag.ToDto(t.Count));
	}

	private async Task<TagDto> GetAiSuggestionAsync(GetTagCompletionCommand request, string structurePrompt, CancellationToken cancellationToken)
	{
		var payload = new { tag = request.Body.Tag };
		string userPrompt = JsonSerializer.Serialize(payload);

		var responseString = await _aiChatService.GetChatResponseAsync(userPrompt, structurePrompt, returnJson: true, cancellationToken);
		_logger.LogDebug("Ai response: {ResponseString}", responseString);

		var responseDto = JsonSerializer.Deserialize<TagsSuggestionResponse>(responseString, _options)
			?? throw new Exception($"Failed to deserialize response from AI: {responseString}");

		string slug = (responseDto.Translations.FirstOrDefault(t => t.LanguageCode == LanguageCode.English)?.Text ?? string.Empty).Replace(' ', '_');

		var result = new TagDto
		{
			Slug = slug,
			Translations = responseDto.Translations.DistinctBy(t => t.LanguageCode).ToList(),
			Aliases = responseDto.Aliases.Where(a => !responseDto.Translations.Any(t => t.Text == a.Text)).ToList()
		};
		return result;
	}

	private record TagsSuggestionResponse
	{
		public ICollection<TagTranslationDto> Translations { get; set; }
		public ICollection<TagTranslationDto> Aliases { get; set; }
	}
}


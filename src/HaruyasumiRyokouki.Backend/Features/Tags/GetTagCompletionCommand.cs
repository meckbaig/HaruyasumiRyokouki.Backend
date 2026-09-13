using HaruyasumiRyokouki.Backend.Common.Options;
using HaruyasumiRyokouki.Backend.DbContexts;
using HaruyasumiRyokouki.Backend.Extensions;
using HaruyasumiRyokouki.Backend.Models.Db.Enums;
using HaruyasumiRyokouki.Backend.Models.Dtos.Tags;
using HaruyasumiRyokouki.Backend.Services.Interfaces;
using HaruyasumiRyokouki.Backend.Services.Translation.Factories;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
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
3. Generate only genuine search aliases: synonyms, alternative names, abbreviations, transliterations, spelling variants, or equivalent word-order variants.

Alias rules:
- An alias MUST be semantically equivalent and interchangeable with the original tag in search.
- NEVER broaden, narrow, qualify, describe, contextualize, or otherwise change its meaning or semantic scope.
- NEVER add contextual or descriptive modifiers such as country, nationality, region, city, culture, language, cuisine, style, type, category, attribute, ingredient, brand, etc.
- In particular, never add modifiers meaning "Japanese", "Japan", "日本", "日本の", "японский", "Япония", or their equivalents.
- NEVER generate "[modifier] + [original tag]" variants or aliases based merely on related/common concepts.
- Do not include the original tag or duplicates.
- Generate at most 5 aliases per language. If no genuine aliases exist, return an empty list. Never add aliases just to fill the limit.

These aliases are used as independent search tags on a Japan-focused website, so contextual modifiers must not be used.

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

Output rules:
- Return only valid JSON.
- `translations` MUST contain exactly 3 items.
- `languageCode` MUST be one of {{LanguageCode.English}}, {{LanguageCode.Russian}}, {{LanguageCode.Japanese}}.
- `aliases` may be empty.
""";

	public GetTagCompletionQueryHandler
	(
		IAppDbContext context,
		ILogger<GetTagCompletionQueryHandler> logger,
		ITranslationServiceOptionsAccessor translationAccessor,
		IContentTranslationServiceFactory factory,
		IOptions<TranslationProviderOptions> translationOptions
	)
	{
		_context = context;
		_logger = logger;

		_aiChatService = factory.CreateAiService(translationAccessor.GetAiServiceOptions(translationOptions.Value.Usage.Tags));
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


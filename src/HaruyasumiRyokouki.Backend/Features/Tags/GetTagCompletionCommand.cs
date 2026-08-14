using HaruyasumiRyokouki.Backend.DbContexts;
using HaruyasumiRyokouki.Backend.Extensions;
using HaruyasumiRyokouki.Backend.Models.Db.Enums;
using HaruyasumiRyokouki.Backend.Models.Dtos.Tags;
using HaruyasumiRyokouki.Backend.Services.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
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

Given a user-provided tag.

Tasks:
1. Detect the language of the provided tag.
2. Translate the tag into target languages: {{nameof(LanguageCode.English)}}, {{nameof(LanguageCode.Russian)}} and {{nameof(LanguageCode.Japanese)}}, .
3. Generate useful search aliases for the tag in all target languages.
4. An alias must be a realistic alternative query that a user could type when searching for the same concept.
5. Include synonyms, alternative names, common abbreviations, and spelling variants when appropriate.
6. Do not change or broaden the meaning of the original tag.
7. Do not include the original tag itself as an alias.
8. Do not invent aliases when no meaningful alternative exists.
9. Use the provided language codes exactly.
10. Return only valid JSON.

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

Additional rules:
- `translations` must contain exactly three items.
- `aliases` may contain aliases in the detected source language and other target languages.
- Generate at most 5 aliases per language.
- `code` must be a valid language code ({{LanguageCode.English}}, {{LanguageCode.Russian}}, {{LanguageCode.Japanese}}).
- Do not duplicate identical aliases.
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
		var existingTags = await _context.Tags.SearchAsync
		(
			likePattern,
			t => t.Media.Count,
			t => t.ToDto(),
			cancellationToken: cancellationToken
		);
		return existingTags;
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
			Aliases = responseDto.Aliases
		};
		return result;
	}

	private record TagsSuggestionResponse
	{
		public ICollection<TagTranslationDto> Translations { get; set; }
		public ICollection<TagTranslationDto> Aliases { get; set; }
	}
}


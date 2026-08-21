using HaruyasumiRyokouki.Backend.Services.Interfaces;
using MediatR;
using System.Text.Json;

namespace HaruyasumiRyokouki.Backend.Features.Translation;

public record GenerateTextTranslationCommand : IRequest<GenerateTextTranslationResponse>
{
	public required string InputText { get; init; }
	public required ICollection<string> TargetLanguages { get; init; }
}

public class GenerateTextTranslationResponse
{
	public required Dictionary<string, string> Results { get; init; }
}


internal class GenerateTextTranslationHandler : IRequestHandler<GenerateTextTranslationCommand, GenerateTextTranslationResponse>
{
	private readonly IAiChatService _aiChatService;
	private readonly ILogger<GenerateTextTranslationHandler> _logger;
	private static JsonSerializerOptions _options = new JsonSerializerOptions
	{
		PropertyNameCaseInsensitive = true
	};

	public GenerateTextTranslationHandler(IAiChatService aiChatService, ILogger<GenerateTextTranslationHandler> logger)
	{
		_aiChatService = aiChatService;
		_logger = logger;
	}

	public async Task<GenerateTextTranslationResponse> Handle(
		GenerateTextTranslationCommand request,
		CancellationToken cancellationToken)
	{
		var targetLanguagesJson = JsonSerializer.Serialize(request.TargetLanguages);

		string structurePrompt = $$"""
You are a professional multilingual translator.

Your task is to translate the provided source text into EVERY language specified in the target languages list.

Target languages:
{{targetLanguagesJson}}

IMPORTANT RULES:

1. Translate the source text into EVERY target language.
2. NEVER omit a target language.
3. Each target language MUST have its own entry in the "results" object.
4. The value for each language MUST contain the complete translation of the source text.
5. Do NOT return the original text unless the original text is already written in that target language.
6. The source text may be written in ANY of the target languages. Detect the source language automatically.
7. Do NOT use the source language instead of the requested target language.
8. Japanese MUST be written using Japanese characters (kanji, hiragana and katakana).
9. When translating into Japanese, NEVER output Chinese characters as a substitute for Japanese text. Use natural Japanese grammar and vocabulary.
10. When the source contains Japanese words written in romaji or in other transliteration, convert them to proper Japanese script when translating into Japanese.
11. When translating into a language other than Japanese, preserve romaji exactly as written unless translating it is required by the meaning.
12. Preserve the semantic meaning, tone and style of the original text.
13. Preserve formatting as closely as possible: line breaks, paragraphs, indentation, special characters and structure.
14. Do not add explanations, comments, notes or translations not present in the source text.
15. Do not merge translations.
16. Do not omit any part of the source text.

OUTPUT FORMAT:

Return ONLY valid JSON matching this structure:

{
  "results": {
    "<target language 1>": "<complete translation>",
    "<target language 2>": "<complete translation>",
    "<target language 3>": "<complete translation>"
  }
}

The keys in "results" MUST exactly match the target language names provided above.
""";
		var payload = new { text = request.InputText };
		string userPrompt = JsonSerializer.Serialize(payload);

		var responseString = await _aiChatService.GetChatResponseAsync(
			userPrompt,
			structurePrompt,
			returnJson: true,
			cancellationToken: cancellationToken);

		_logger.LogDebug("Ai response: {ResponseString}", responseString);

		return JsonSerializer.Deserialize<GenerateTextTranslationResponse>(responseString, _options)
			?? throw new Exception($"Failed to deserialize response from AI: {responseString}");
	}

}

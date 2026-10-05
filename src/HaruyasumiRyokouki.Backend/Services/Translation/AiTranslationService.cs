using HaruyasumiRyokouki.Backend.Models.Db.Enums;
using HaruyasumiRyokouki.Backend.Services.Interfaces;

namespace HaruyasumiRyokouki.Backend.Services.Translation;

public class AiTranslationService : IContentTranslationService
{
	private readonly IAiChatService _aiChatService;

	public AiTranslationService(IAiChatService aiChatService)
	{
		_aiChatService = aiChatService;
	}

	private const string PromtCore = """
		
		The text may be a personal travel journal entry, photo title, or short photo description.
		Preserve the original meaning, tone, personal voice, and expressions as faithfully as possible. Keep the translation natural, but do not embellish, simplify, or make the text more literary than the original.
		Preserve the original formatting exactly: line breaks, spacing, indentation, special characters, and structure. Do not modify any formatting elements. 
		Return only the translation. Do not add explanations, quotation marks, markdown, or any other text.
		
		""";

	private const string RomajiConversionRule =
		"IMPORTANT: If present in original text, convert ALL romaji (Japanese words written in Latin alphabet) to proper Japanese script (kanji/kana). The original text in the original language must also be translated.";

	private const string DayEntryEmbedPlacementRule = """
		ADDITIONAL RULES (personal travel journal day entries only):
		If the text contains embedded elements (such as inline media placeholders, emojis, links, or other inline markup), keep each of them in the same position as in the original, attached to the nearest phrase by meaning. Do not relocate them to the beginning or the end of the text.
		
		""";

	private const string DayEntryJapaneseRule = """
		ADDITIONAL RULES (personal travel journal day entries translated to Japanese only):
		Translate the first-person pronouns "I", "me", "my" (and their Russian equivalents "Я", "меня", "мне", "мой") as 私, using 私の for the possessive "my" where appropriate.
		Translate the author's name written as "Рё" or "Ryo" as りょう.
		
		""";

	private string GetSystemPromt(string outputLanguageCode, string? inputLanguageCode = default)
	{
		string inputLanguagePromptSubstring = string.IsNullOrWhiteSpace(inputLanguageCode)
			? string.Empty
			: $" in {TranslationPlanner.LanguageByCode(inputLanguageCode)}";
		var prompt = $"You are a translator. You will receive text{inputLanguagePromptSubstring}, translate the content to {TranslationPlanner.LanguageByCode(outputLanguageCode)}. "
			+ PromtCore;

		if (outputLanguageCode == LanguageCode.Japanese)
		{
			prompt += RomajiConversionRule;
			prompt += DayEntryJapaneseRule;
		}
		prompt += DayEntryEmbedPlacementRule;

		return prompt;
	}

	public async Task<string> TranslateTextAsync(string text, string outputLanguage, string? inputLanguage = null, CancellationToken cancellationToken = default)
	{
		return await _aiChatService.GetChatResponseAsync(text, GetSystemPromt(outputLanguage, inputLanguage), returnJson: false, cancellationToken: cancellationToken);
	}
}

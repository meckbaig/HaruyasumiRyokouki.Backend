using HaruyasumiRyokouki.Backend.Common.Helpers;
using HaruyasumiRyokouki.Backend.Models.Db.Enums;
using HaruyasumiRyokouki.Backend.Services.Interfaces;

namespace HaruyasumiRyokouki.Backend.Services.Translation;

/// <summary>
/// Translation through a chat model with a translation prompt.
/// </summary>
public class AiTranslationService : IContentTranslationService
{
	private readonly IAiChatService _aiChatService;

	/// <summary>
	/// Creates the service on top of a chat model.
	/// </summary>
	public AiTranslationService(IAiChatService aiChatService)
	{
		_aiChatService = aiChatService;
	}

	private const string PromptCore = """
		
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
		First-person pronouns: When first-person references are needed, use 私 (and 私の for possessives). However, follow natural Japanese syntax: omit 私 whenever the subject is obvious from the context to prevent repetitive or unnatural phrasing. Never insert 私 in the middle or end of a clause (e.g., avoid "～を私は...").
		Translate the author's name written as "Рё" or "Ryo" as りょう.
		
		""";

	private string GetSystemPrompt(string outputLanguageCode, string? inputLanguageCode = default)
	{
		string inputLanguagePromptSubstring = string.IsNullOrWhiteSpace(inputLanguageCode)
			? string.Empty
			: $" in {TranslationPlanner.LanguageByCode(inputLanguageCode)}";
		var prompt = $"You are a translator. You will receive text{inputLanguagePromptSubstring}, translate the content to {TranslationPlanner.LanguageByCode(outputLanguageCode)}. "
			+ PromptCore;

		if (outputLanguageCode == LanguageCode.Japanese)
		{
			prompt += RomajiConversionRule;
			prompt += DayEntryJapaneseRule;
		}
		prompt += DayEntryEmbedPlacementRule;

		return prompt;
	}

	/// <inheritdoc/>
	public async Task<string> TranslateTextAsync(string text, string outputLanguage, string? inputLanguage = null, CancellationToken cancellationToken = default)
	{
		return await _aiChatService.GetChatResponseAsync(text, GetSystemPrompt(outputLanguage, inputLanguage), cancellationToken);
	}
}

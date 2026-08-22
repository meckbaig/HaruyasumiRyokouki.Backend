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

	private string GetSystemPromt(string outputLanguageCode, string? inputLanguageCode = default)
	{
		string inputLanguagePromptSubstring = string.IsNullOrWhiteSpace(inputLanguageCode) 
			? string.Empty
			: $" in {TranslationPlanner.LanguageByCode(inputLanguageCode)}";
		return $"You are a translator. You will receive text{inputLanguagePromptSubstring}, translate the content to {TranslationPlanner.LanguageByCode(outputLanguageCode)}. "
			+ PromtCore
			+ (outputLanguageCode == LanguageCode.Japanese
				? "IMPORTANT: If present in original text, convert ALL romaji (Japanese words written in Latin alphabet) to proper Japanese script (kanji/kana)."
				: "IMPORTANT: If the target language is NOT Japanese, leave romaji words unchanged.");
	}

	public async Task<string> TranslateTextAsync(string text, string outputLanguage, string? inputLanguage = null, CancellationToken cancellationToken = default)
	{
		return await _aiChatService.GetChatResponseAsync(text, GetSystemPromt(outputLanguage, inputLanguage), returnJson: false, cancellationToken: cancellationToken);
	}
}

namespace HaruyasumiRyokouki.Backend.Common.Options.Abstractions;

/// <summary>
/// Type of a <c>TranslationProvider:Providers</c> entry.
/// </summary>
public enum TranslationOptionsType
{
	/// <summary>
	/// OpenAI-compatible chat API.
	/// </summary>
	OpenAi,

	/// <summary>
	/// DeepL translation API. Cannot be used for tag completion.
	/// </summary>
	Deepl,

	/// <summary>
	/// Google Gemini.
	/// </summary>
	GeminiAi
}

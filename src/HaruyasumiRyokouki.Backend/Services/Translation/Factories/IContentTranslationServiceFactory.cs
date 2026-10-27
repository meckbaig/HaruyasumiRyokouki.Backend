using HaruyasumiRyokouki.Backend.Common.Options.Abstractions;
using HaruyasumiRyokouki.Backend.Services.Interfaces;

namespace HaruyasumiRyokouki.Backend.Services.Translation.Factories;

/// <summary>
/// Creates translation and AI services from <c>TranslationProvider:Providers</c> entries.
/// </summary>
public interface IContentTranslationServiceFactory
{
	/// <summary>
	/// Creates a chat service for an <see cref="Common.Options.Abstractions.TranslationOptionsType.OpenAi"/> or <see cref="Common.Options.Abstractions.TranslationOptionsType.GeminiAi"/> entry.
	/// </summary>
	IAiChatService CreateAiService(IAiServiceOptions options);

	/// <summary>
	/// Creates a translation service for any entry: DeepL directly, AI providers through a translation prompt.
	/// </summary>
	IContentTranslationService CreateTranslationService(TranslationServiceOptions options);
}

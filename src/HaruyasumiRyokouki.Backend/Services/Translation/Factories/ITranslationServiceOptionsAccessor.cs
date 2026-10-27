using HaruyasumiRyokouki.Backend.Common.Options.Abstractions;

namespace HaruyasumiRyokouki.Backend.Services.Translation.Factories;

/// <summary>
/// Finds <c>TranslationProvider:Providers</c> entries by name, as referenced from <c>TranslationProvider:Usage</c>.
/// </summary>
public interface ITranslationServiceOptionsAccessor
{
	/// <summary>
	/// Returns the entry with the name.
	/// </summary>
	/// <exception cref="InvalidOperationException">No entry with the name.</exception>
	TranslationServiceOptions GetTranslationServiceOptions(string name);

	/// <summary>
	/// Returns the entry with the name, which must be an AI provider.
	/// </summary>
	/// <exception cref="InvalidOperationException">No entry with the name, or it is DeepL.</exception>
	IAiServiceOptions GetAiServiceOptions(string name);
}

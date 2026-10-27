namespace HaruyasumiRyokouki.Backend.Services.Interfaces;

/// <summary>
/// Machine translation of texts by an AI provider or DeepL.
/// </summary>
public interface IContentTranslationService
{
	/// <summary>
	/// Translates a text.
	/// </summary>
	/// <param name="text">Text to translate.</param>
	/// <param name="outputLanguage">Target language code (<c>en</c>, <c>ru</c>, <c>ja</c>).</param>
	/// <param name="inputLanguage">Source language code, or <see langword="null"/> to detect it.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>The translated text.</returns>
	Task<string> TranslateTextAsync(string text, string outputLanguage, string? inputLanguage = default, CancellationToken cancellationToken = default);
}

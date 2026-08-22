namespace HaruyasumiRyokouki.Backend.Services.Interfaces;

public interface IContentTranslationService
{
	Task<string> TranslateTextAsync(string text, string outputLanguage, string? inputLanguage = default, CancellationToken cancellationToken = default);
}

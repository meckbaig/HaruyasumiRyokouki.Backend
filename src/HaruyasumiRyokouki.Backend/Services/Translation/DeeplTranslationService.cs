using DeepL;
using HaruyasumiRyokouki.Backend.Common.Options;
using HaruyasumiRyokouki.Backend.Services.Interfaces;

namespace HaruyasumiRyokouki.Backend.Services.Translation;

/// <summary>
/// Translation through the DeepL API.
/// </summary>
public class DeeplTranslationService : IContentTranslationService
{
	private readonly DeepLClient _client;

	/// <summary>
	/// Creates the service for a <c>Deepl</c> provider entry.
	/// </summary>
	public DeeplTranslationService(DeeplTranslationServiceOptions options)
	{
		_client = new DeepLClient(options.ApiKey);
	}

	/// <inheritdoc/>
	public async Task<string> TranslateTextAsync(string text, string outputLanguage, string? inputLanguage = null, CancellationToken cancellationToken = default)
	{
		var result = await _client.TranslateTextAsync
		(
			text,
			GetDeeplLanguageCode(inputLanguage),
			GetDeeplLanguageCode(outputLanguage)!, 
			cancellationToken: cancellationToken
		);
		return result.Text;
	}

	private static string? GetDeeplLanguageCode(string? original)
	{
		if (original == "en")
			return "en-US";
		return original;
	}
}

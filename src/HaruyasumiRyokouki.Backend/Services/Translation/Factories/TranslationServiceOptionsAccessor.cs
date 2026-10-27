using HaruyasumiRyokouki.Backend.Common.Options;
using HaruyasumiRyokouki.Backend.Common.Options.Abstractions;
using Microsoft.Extensions.Options;

namespace HaruyasumiRyokouki.Backend.Services.Translation.Factories;

/// <inheritdoc/>
public sealed class TranslationServiceOptionsAccessor : ITranslationServiceOptionsAccessor
{
	private readonly TranslationProviderOptions _options;

	/// <summary>
	/// Constructor with parameters for DI.
	/// </summary>
	public TranslationServiceOptionsAccessor(IOptions<TranslationProviderOptions> options)
	{
		_options = options.Value;
	}

	/// <inheritdoc/>
	public TranslationServiceOptions GetTranslationServiceOptions(string name)
	{
		if (!_options.Providers.TryGetValue(name, out var provider))
		{
			throw new InvalidOperationException($"Translation provider '{name}' is not configured.");
		}

		return provider;
	}

	/// <inheritdoc/>
	public IAiServiceOptions GetAiServiceOptions(string name)
	{
		var options = GetTranslationServiceOptions(name);
		if (options is IAiServiceOptions aiServiceOptions)
			return aiServiceOptions;
		throw new InvalidOperationException($"Translation provider '{name}' is not an AI service.");
	}
}

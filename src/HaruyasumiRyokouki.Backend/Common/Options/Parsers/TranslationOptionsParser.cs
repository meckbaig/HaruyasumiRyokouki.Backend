using HaruyasumiRyokouki.Backend.Common.Options.Abstractions;

namespace HaruyasumiRyokouki.Backend.Common.Options.Parsers;

public sealed class TranslationOptionsParser : ITranslationOptionsParser
{
	public TranslationProviderOptions Parse(IConfigurationSection section)
	{
		var usage = section
			.GetSection("Usage")
			.Get<TranslationUsageOptions>()
				?? throw new InvalidOperationException("TranslationApi:Usage is required.");

		var providers = new Dictionary<string, TranslationServiceOptions>();

		foreach (var providerSection in
				 section.GetSection("Providers").GetChildren())
		{
			var type = providerSection
				.GetValue<TranslationOptionsType>("Type");

			TranslationServiceOptions? options = type switch
			{
				TranslationOptionsType.OpenAi =>
					providerSection.Get<OpenAiTranslationServiceOptions>(),

				TranslationOptionsType.Deepl =>
					providerSection.Get<DeeplTranslationServiceOptions>(),

				TranslationOptionsType.GeminiAi =>
					providerSection.Get<GeminiTranslationServiceOptions>(),

				_ => throw new InvalidOperationException(
					$"Unknown translation provider type '{type}'.")
			};

			providers.Add
			(
				providerSection.Key,
				options ?? throw new InvalidOperationException($"Cannot parse provider '{providerSection.Key}'.")
			);
		}

		return new TranslationProviderOptions
		{
			Providers = providers,
			Usage = usage
		};
	}
}

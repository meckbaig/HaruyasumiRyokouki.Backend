using HaruyasumiRyokouki.Backend.Common.Options.Abstractions;
using Microsoft.Extensions.Options;
using System.Text;

namespace HaruyasumiRyokouki.Backend.Common.Options.Validators;

public class TranslationProviderOptionsValidator : IValidateOptions<TranslationProviderOptions>
{
	public ValidateOptionsResult Validate(
		string? name,
		TranslationProviderOptions options)
	{
		if (options == null)
		{
			return ValidateOptionsResult.Fail(
				$"'{TranslationProviderOptions.ConfigurationSectionName}' must not be null.");
		}

		var failures = new StringBuilder();

		ValidateUsage(options, failures);
		ValidateProviders(options, failures);

		return failures.Length > 0
			? ValidateOptionsResult.Fail(failures.ToString())
			: ValidateOptionsResult.Success;
	}

	private static void ValidateUsage(
		TranslationProviderOptions options,
		StringBuilder failures)
	{
		if (options.Usage == null)
		{
			failures.AppendLine(
				$"'{TranslationProviderOptions.ConfigurationSectionName}:" +
				$"{nameof(TranslationProviderOptions.Usage)}' must not be null.");

			return;
		}

		ValidateUsageAiProvider(
			nameof(options.Usage.Tags),
			options.Usage.Tags,
			options,
			failures);

		ValidateUsageProvider(
			nameof(options.Usage.Media),
			options.Usage.Media,
			options,
			failures);

		ValidateUsageProvider(
			nameof(options.Usage.Days),
			options.Usage.Days,
			options,
			failures);
	}

	private static void ValidateUsageProvider(
		string propertyName,
		string? providerName,
		TranslationProviderOptions options,
		StringBuilder failures)
	{
		if (string.IsNullOrWhiteSpace(providerName))
		{
			failures.AppendLine(
				$"'{TranslationProviderOptions.ConfigurationSectionName}:" +
				$"{nameof(TranslationProviderOptions.Usage)}:{propertyName}' " +
				"cannot be null or empty.");

			return;
		}

		if (!options.Providers.ContainsKey(providerName))
		{
			failures.AppendLine(
				$"'{TranslationProviderOptions.ConfigurationSectionName}:" +
				$"{nameof(TranslationProviderOptions.Usage)}:{propertyName}' " +
				$"references unknown provider '{providerName}'.");
		}
	}
	private static void ValidateUsageAiProvider(
		string propertyName,
		string? providerName,
		TranslationProviderOptions options,
		StringBuilder failures)
	{
		ValidateUsageProvider(propertyName,	providerName, options, failures);
		if (options.Providers.TryGetValue(providerName, out var provider))
		{
			if (provider is not IAiServiceOptions)
				failures.AppendLine(
					$"'{TranslationProviderOptions.ConfigurationSectionName}:" +
					$"{nameof(TranslationProviderOptions.Usage)}:{propertyName}' " +
					$"should be an AI provider.");
		}
	}

	private static void ValidateProviders(
		TranslationProviderOptions options,
		StringBuilder failures)
	{
		if (options.Providers == null)
		{
			failures.AppendLine(
				$"'{TranslationProviderOptions.ConfigurationSectionName}:" +
				$"{nameof(TranslationProviderOptions.Providers)}' must not be null.");

			return;
		}

		if (options.Providers.Count == 0)
		{
			failures.AppendLine(
				$"'{TranslationProviderOptions.ConfigurationSectionName}:" +
				$"{nameof(TranslationProviderOptions.Providers)}' " +
				"must contain at least one provider.");

			return;
		}

		foreach (var (name, provider) in options.Providers)
		{
			ValidateProvider(name, provider, failures);
		}
	}

	private static void ValidateProvider(
		string name,
		TranslationServiceOptions? provider,
		StringBuilder failures)
	{
		var path =
			$"{TranslationProviderOptions.ConfigurationSectionName}:" +
			$"{nameof(TranslationProviderOptions.Providers)}:{name}";

		if (provider == null)
		{
			failures.AppendLine(
				$"'{path}' must not be null.");

			return;
		}

		switch (provider)
		{
			case OpenAiTranslationServiceOptions openAi:
				ValidateOpenAi(path, openAi, failures);
				break;

			case DeeplTranslationServiceOptions deepl:
				ValidateDeepl(path, deepl, failures);
				break;

			case GeminiTranslationServiceOptions gemini:
				ValidateGemini(path, gemini, failures);
				break;

			default:
				failures.AppendLine(
					$"'{path}' has unsupported provider options type " +
					$"'{provider.GetType().Name}'.");
				break;
		}
	}

	private static void ValidateOpenAi
	(
		string path,
		OpenAiTranslationServiceOptions options,
		StringBuilder failures
	)
	{
		if (options.Type != TranslationOptionsType.OpenAi)
		{
			failures.AppendLine(
				$"'{path}:{nameof(options.Type)}' must be " +
				$"'{TranslationOptionsType.OpenAi}'.");
		}

		//if (string.IsNullOrWhiteSpace(options.ApiKey))
		//{
		//	failures.AppendLine(
		//		$"'{path}:{nameof(options.ApiKey)}' " +
		//		"cannot be null or empty.");
		//}

		if (string.IsNullOrWhiteSpace(options.Model))
		{
			failures.AppendLine(
				$"'{path}:{nameof(options.Model)}' " +
				"cannot be null or empty.");
		}

		if (options.Temperature < 0)
		{
			failures.AppendLine(
				$"'{path}:{nameof(options.Temperature)}' " +
				"must be greater than 0.");
		}
		else if (options.Temperature > 1)
		{
			failures.AppendLine(
				$"'{path}:{nameof(options.Temperature)}' " +
				"cannot be greater than 1 (100%).");
		}

		if (string.IsNullOrWhiteSpace(options.ReasoningLevel))
		{
			failures.AppendLine(
				$"'{path}:{nameof(options.ReasoningLevel)}' " +
				"cannot be null or empty.");
		}

		if (string.IsNullOrWhiteSpace(options.ApiUrl))
		{
			failures.AppendLine(
				$"'{path}:{nameof(options.ApiUrl)}' " +
				"cannot be null or empty.");
		}
		else if (!Uri.TryCreate(
			options.ApiUrl,
			UriKind.Absolute,
			out _))
		{
			failures.AppendLine(
				$"'{path}:{nameof(options.ApiUrl)}' " +
				"must be a valid absolute URI.");
		}
	}
	private static void ValidateDeepl
	(
		string path,
		DeeplTranslationServiceOptions options,
		StringBuilder failures
	)
	{
		if (options.Type != TranslationOptionsType.Deepl)
		{
			failures.AppendLine(
				$"'{path}:{nameof(options.Type)}' must be " +
				$"'{TranslationOptionsType.Deepl}'.");
		}

		if (string.IsNullOrWhiteSpace(options.ApiKey))
		{
			failures.AppendLine(
				$"'{path}:{nameof(options.ApiKey)}' " +
				"cannot be null or empty.");
		}
	}

	private static void ValidateGemini
	(
		string path,
		GeminiTranslationServiceOptions options,
		StringBuilder failures
	)
	{
		if (options.Type != TranslationOptionsType.GeminiAi)
		{
			failures.AppendLine(
				$"'{path}:{nameof(options.Type)}' must be " +
				$"'{TranslationOptionsType.GeminiAi}'.");
		}

		if (string.IsNullOrWhiteSpace(options.ApiKey))
		{
			failures.AppendLine(
				$"'{path}:{nameof(options.ApiKey)}' " +
				"cannot be null or empty.");
		}

		if (string.IsNullOrWhiteSpace(options.Model))
		{
			failures.AppendLine(
				$"'{path}:{nameof(options.Model)}' " +
				"cannot be null or empty.");
		}

		if (options.Temperature < 0)
		{
			failures.AppendLine(
				$"'{path}:{nameof(options.Temperature)}' " +
				"must be greater than 0.");
		}
		else if (options.Temperature > 1)
		{
			failures.AppendLine(
				$"'{path}:{nameof(options.Temperature)}' " + 
				"cannot be greater than 1 (100%).");
		}

		//if (string.IsNullOrWhiteSpace(options.ReasoningLevel))
		//{
		//	failures.AppendLine(
		//		$"'{path}:{nameof(options.ReasoningLevel)}' " +
		//		"cannot be null or empty.");
		//}
	}
}

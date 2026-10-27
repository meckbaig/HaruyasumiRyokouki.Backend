using HaruyasumiRyokouki.Backend.Common.Options.Abstractions;

namespace HaruyasumiRyokouki.Backend.Common.Options;

/// <summary>
/// Options of the <c>TranslationProvider</c> configuration section; see the Configuration section of README.
/// </summary>
/// <remarks>Bound by <see cref="Parsers.TranslationOptionsParser"/>: the entries of <c>Providers</c> are user-defined.</remarks>
public class TranslationProviderOptions
{
	/// <summary>
	/// Configuration section name.
	/// </summary>
	public const string ConfigurationSectionName = "TranslationProvider";

	/// <summary>
	/// Provider entries by their names from the configuration.
	/// </summary>
	public Dictionary<string, TranslationServiceOptions> Providers { get; set; } = [];

	/// <summary>
	/// Which entry is used for which task.
	/// </summary>
	public TranslationUsageOptions Usage { get; set; }
}

/// <summary>
/// Names of the <see cref="TranslationProviderOptions.Providers"/> entries used for each task.
/// </summary>
public sealed class TranslationUsageOptions
{

	/// <summary>
	/// Tag completion; must be an AI provider.
	/// </summary>
	public required string Tags { get; init; }

	/// <summary>
	/// Translation of media titles and descriptions.
	/// </summary>
	public required string Media { get; init; }

	/// <summary>
	/// Translation of day notes.
	/// </summary>
	public required string Days { get; init; }
}

/// <summary>
/// Entry of type <c>OpenAi</c>: any OpenAI-compatible chat API.
/// </summary>
public sealed class OpenAiTranslationServiceOptions : TranslationServiceOptions, IAiServiceOptions
{
	/// <summary>
	/// API key.
	/// </summary>
	public required string ApiKey { get; set; }

	/// <summary>
	/// Model name.
	/// </summary>
	public required string Model { get; set; }

	/// <summary>
	/// Sampling temperature, 0 to 1.
	/// </summary>
	public float Temperature { get; set; }

	/// <summary>
	/// Reasoning effort passed to the API.
	/// </summary>
	public string ReasoningLevel { get; set; }

	/// <summary>
	/// Base URL of the API, required, e.g. <c>https://api.deepseek.com</c>.
	/// </summary>
	public required string? ApiUrl { get; set; }
}

/// <summary>
/// Entry of type <c>Deepl</c>: the DeepL translation API.
/// </summary>
public sealed class DeeplTranslationServiceOptions : TranslationServiceOptions
{
	/// <summary>
	/// API key.
	/// </summary>
	public required string ApiKey { get; init; }
}

/// <summary>
/// Entry of type <c>GeminiAi</c>: Google Gemini.
/// </summary>
public sealed class GeminiTranslationServiceOptions : TranslationServiceOptions, IAiServiceOptions
{
	/// <summary>
	/// API key.
	/// </summary>
	public required string ApiKey { get; set; }

	/// <summary>
	/// Model name.
	/// </summary>
	public required string Model { get; set; }

	/// <summary>
	/// Sampling temperature, 0 to 1.
	/// </summary>
	public float Temperature { get; set; }

	/// <summary>
	/// Thinking level: <c>none</c>, <c>minimal</c>, <c>low</c>, <c>medium</c> or <c>high</c>.
	/// </summary>
	public string ReasoningLevel { get; set; }
}

using HaruyasumiRyokouki.Backend.Common.Options.Abstractions;

namespace HaruyasumiRyokouki.Backend.Common.Options;

/// <summary>
/// Options of the <c>TranslationProvider</c> configuration section.
/// </summary>
/// <remarks>Bound by <see cref="Parsers.TranslationOptionsParser"/>: the entries of <see cref="Providers"/> are user-defined.</remarks>
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
	public required TranslationUsageOptions Usage { get; set; }
}

/// <summary>
/// Names of the <see cref="TranslationProviderOptions.Providers"/> entries used for each task.
/// </summary>
public sealed class TranslationUsageOptions
{

	/// <summary>
	/// Tag completion. Must be an AI provider.
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
/// Entry of type <see cref="Common.Options.Abstractions.TranslationOptionsType.OpenAi"/>: any OpenAI-compatible chat API.
/// </summary>
public sealed class OpenAiTranslationServiceOptions : TranslationServiceOptions, IAiServiceOptions
{
	/// <summary>
	/// API key. Optional, local servers such as LM Studio do not need one.
	/// </summary>
	public string? ApiKey { get; set; }

	/// <summary>
	/// Model name.
	/// </summary>
	public required string Model { get; set; }

	/// <summary>
	/// Sampling temperature, 0 to 1. Optional, when not set the API default is used.
	/// </summary>
	public float? Temperature { get; set; }

	/// <summary>
	/// Reasoning effort passed to the API.
	/// </summary>
	public required string ReasoningLevel { get; set; }

	/// <summary>
	/// Base URL of the API, required, e.g. <c>https://api.deepseek.com</c>.
	/// </summary>
	public required string? ApiUrl { get; set; }

	/// <summary>
	/// How a JSON answer is requested, <see cref="OpenAiJsonResponseFormat.JsonObject"/> by default.
	/// LM Studio needs <see cref="OpenAiJsonResponseFormat.JsonSchema"/>.
	/// </summary>
	public OpenAiJsonResponseFormat JsonResponseFormat { get; set; } = OpenAiJsonResponseFormat.JsonObject;
}

/// <summary>
/// Entry of type <see cref="Common.Options.Abstractions.TranslationOptionsType.Deepl"/>: the DeepL translation API.
/// </summary>
public sealed class DeeplTranslationServiceOptions : TranslationServiceOptions
{
	/// <summary>
	/// API key.
	/// </summary>
	public required string ApiKey { get; init; }
}

/// <summary>
/// Entry of type <see cref="Common.Options.Abstractions.TranslationOptionsType.GeminiAi"/>: Google Gemini.
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
	/// Sampling temperature, 0 to 1. Optional, when not set the API default is used.
	/// </summary>
	public float? Temperature { get; set; }

	/// <summary>
	/// Thinking level: <c>none</c>, <c>minimal</c>, <c>low</c>, <c>medium</c> or <c>high</c>.
	/// </summary>
	public string? ReasoningLevel { get; set; }
}

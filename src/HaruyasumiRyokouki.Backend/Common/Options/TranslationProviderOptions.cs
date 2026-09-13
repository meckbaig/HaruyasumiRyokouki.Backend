using HaruyasumiRyokouki.Backend.Common.Options.Abstractions;

namespace HaruyasumiRyokouki.Backend.Common.Options;

public class TranslationProviderOptions
{
	public const string ConfigurationSectionName = "TranslationProvider";
	public Dictionary<string, TranslationServiceOptions> Providers { get; set; } = [];
	public TranslationUsageOptions Usage { get; set; }
}

public sealed class TranslationUsageOptions
{

	public required string Tags { get; init; }
	public required string Media { get; init; }
	public required string Days { get; init; }
}

public sealed class OpenAiTranslationServiceOptions : TranslationServiceOptions, IAiServiceOptions
{
	public required string ApiKey { get; set; }
	public required string Model { get; set; }
	public float Temperature { get; set; }
	public string ReasoningLevel { get; set; }
	public required string? ApiUrl { get; set; }
}

public sealed class DeeplTranslationServiceOptions : TranslationServiceOptions
{
	public required string ApiKey { get; init; }
}

public sealed class GeminiTranslationServiceOptions : TranslationServiceOptions, IAiServiceOptions
{
	public required string ApiKey { get; set; }
	public required string Model { get; set; }
	public float Temperature { get; set; }
	public string ReasoningLevel { get; set; }
}

using Google.GenAI;
using Google.GenAI.Types;
using HaruyasumiRyokouki.Backend.Common.Helpers;
using HaruyasumiRyokouki.Backend.Common.Options;
using HaruyasumiRyokouki.Backend.Services.Interfaces;

namespace HaruyasumiRyokouki.Backend.Services;

/// <summary>
/// Chat model of Google Gemini.
/// </summary>
public class GeminiAiChatService : IAiChatService
{
	private readonly Client _client;
	private readonly GeminiTranslationServiceOptions _options;

	/// <summary>
	/// Creates the service for a <see cref="Common.Options.Abstractions.TranslationOptionsType.GeminiAi"/> provider entry.
	/// </summary>
	public GeminiAiChatService(GeminiTranslationServiceOptions options)
	{
		_client = new Client(apiKey: options.ApiKey);

		_options = options;
	}

	/// <inheritdoc/>
	public Task<string> GetChatResponseAsync
	(
		string message,
		string? systemMessage = null,
		CancellationToken cancellationToken = default)
	{
		return GenerateContentAsync(message, CreateConfig(systemMessage), cancellationToken);
	}

	/// <inheritdoc/>
	public async Task<T> GetJsonResponseAsync<T>
	(
		string message,
		string? systemMessage = null,
		CancellationToken cancellationToken = default)
	{
		var config = CreateConfig(systemMessage);
		config.ResponseMimeType = "application/json";
		config.ResponseJsonSchema = AiJsonResponse.GetSchema<T>();

		var text = await GenerateContentAsync(message, config, cancellationToken);
		return AiJsonResponse.Deserialize<T>(text);
	}

	private GenerateContentConfig CreateConfig(string? systemMessage)
	{
		var config = new GenerateContentConfig
		{
			Temperature = _options.Temperature.HasValue ? Math.Round(_options.Temperature.Value, 2) : null,
		};

		if (!string.IsNullOrWhiteSpace(_options.ReasoningLevel) && _options.ReasoningLevel.ToLower() != "none")
		{
			config.ThinkingConfig = new ThinkingConfig
			{
				ThinkingLevel = GetThinkingLevel(_options.ReasoningLevel),
				IncludeThoughts = true
			};
		}
		if (!string.IsNullOrWhiteSpace(systemMessage))
		{
			config.SystemInstruction = new Content
			{
				Parts =
				[
					new Part
					{
						Text = systemMessage
					}
				]
			};
		}

		return config;
	}

	private async Task<string> GenerateContentAsync(string message, GenerateContentConfig config, CancellationToken cancellationToken)
	{
		var response = await _client.Models.GenerateContentAsync
		(
			model: _options.Model,
			contents: message,
			config: config,
			cancellationToken: cancellationToken
		);

		var text = response.Candidates?
			.FirstOrDefault()?
			.Content?
			.Parts?
			.FirstOrDefault(x => !x.Thought.HasValue || !x.Thought.Value)?
			.Text;

		if (string.IsNullOrWhiteSpace(text))
		{
			throw new InvalidOperationException("The response from the AI does not contain text.");
		}

		return text;
	}

	private static ThinkingLevel GetThinkingLevel(string? level)
	{
		return level?.ToLowerInvariant() switch
		{
			"minimal" => ThinkingLevel.Minimal,
			"low" => ThinkingLevel.Low,
			"medium" => ThinkingLevel.Medium,
			"high" => ThinkingLevel.High,
			_ => throw new ArgumentOutOfRangeException(nameof(level))
		};
	}
}

using Google.GenAI;
using Google.GenAI.Types;
using HaruyasumiRyokouki.Backend.Common.Options;
using HaruyasumiRyokouki.Backend.Services.Interfaces;

namespace HaruyasumiRyokouki.Backend.Services;

public class GeminiAiChatService : IAiChatService
{
	private readonly Client _client;
	private readonly GeminiTranslationServiceOptions _options;

	public GeminiAiChatService(GeminiTranslationServiceOptions options)
	{
		_client = new Client(apiKey: options.ApiKey);

		_options = options;
	}

	public async Task<string> GetChatResponseAsync
	(
		string message,
		string? systemMessage = null,
		bool returnJson = false,
		CancellationToken cancellationToken = default)
	{
		var config = new GenerateContentConfig
		{
			Temperature = _options.Temperature,
			//ThinkingConfig = new ThinkingConfig
			//{
			//	ThinkingBudget = GetThinkingBudget(_options.ReasoningLevel)
			//}
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

		if (returnJson)
		{
			config.ResponseMimeType = "application/json";
		}

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

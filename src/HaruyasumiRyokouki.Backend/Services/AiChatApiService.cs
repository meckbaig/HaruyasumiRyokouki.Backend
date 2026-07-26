using HaruyasumiRyokouki.Backend.Common.Options;
using HaruyasumiRyokouki.Backend.Services.Interfaces;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

namespace HaruyasumiRyokouki.Backend.Services;

internal class AiChatApiService : IAiChatService
{
	private readonly AiApiOptions _aiApiOptions;
	private readonly ChatClient _chatClient;
	private readonly ChatCompletionOptions _completionOptions;
	private readonly ChatCompletionOptions _jsonCompletionOptions;

	public AiChatApiService(IOptions<AiApiOptions> options)
	{
		_aiApiOptions = options.Value;
		var aiClientOptions = new OpenAIClientOptions() { Endpoint = new Uri(_aiApiOptions.ApiUrl) }; 
		var apiKey = new ApiKeyCredential(_aiApiOptions.ApiKey);
		_chatClient = new ChatClient(_aiApiOptions.Model, apiKey, aiClientOptions);

		_completionOptions = new ChatCompletionOptions()
		{
			Temperature = _aiApiOptions.Temperature,
		};

		_jsonCompletionOptions = new ChatCompletionOptions()
		{
			Temperature = _aiApiOptions.Temperature,
			ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat()
		};
	}

	public async Task<string> GetChatResponseAsync(string message, bool returnJson = false, CancellationToken cancellationToken = default)
	{
		ChatCompletion response = await _chatClient.CompleteChatAsync
		(
			messages: [new UserChatMessage(message)],
			options: returnJson ? _jsonCompletionOptions : _completionOptions,
			cancellationToken: cancellationToken
		);
		string? summary = response.Content.FirstOrDefault()?.Text;
		if (string.IsNullOrEmpty(summary))
		{
			throw new ArgumentNullException(nameof(summary), "The response from the AI ​​does not contain text.");
		}
		return summary;
	}
}

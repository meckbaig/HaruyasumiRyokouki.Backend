using HaruyasumiRyokouki.Backend.Common.Options;
using HaruyasumiRyokouki.Backend.Services.Interfaces;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

namespace HaruyasumiRyokouki.Backend.Services;

#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
internal class OpenAiChatService : IAiChatService
{
	private readonly ChatClient _chatClient;
	private readonly ChatCompletionOptions _completionOptions;
	private readonly ChatCompletionOptions _jsonCompletionOptions;

	public OpenAiChatService(OpenAiTranslationServiceOptions options)
	{
		var aiClientOptions = new OpenAIClientOptions(); 
		if (!string.IsNullOrWhiteSpace(options.ApiUrl))
		{
			aiClientOptions.Endpoint = new Uri(options.ApiUrl);
		}
		var apiKey = new ApiKeyCredential(options.ApiKey);
		_chatClient = new ChatClient(options.Model, apiKey, aiClientOptions);

		_completionOptions = new ChatCompletionOptions()
		{
			Temperature = options.Temperature,
			ReasoningEffortLevel = new ChatReasoningEffortLevel(options.ReasoningLevel)
		};

		_jsonCompletionOptions = new ChatCompletionOptions()
		{
			Temperature = options.Temperature,
			ReasoningEffortLevel = new ChatReasoningEffortLevel(options.ReasoningLevel),
			ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat()
		};
	}

	public async Task<string> GetChatResponseAsync
	(
		string message,
		string? systemMessage = null,
		bool returnJson = false,
		CancellationToken cancellationToken = default
	)
	{
		ICollection<ChatMessage> messages;
		var userMessage = new UserChatMessage(message);
		if (systemMessage != null)
			messages = [new SystemChatMessage(systemMessage), userMessage];
		else
			messages = [userMessage];

		ChatCompletion response = await _chatClient.CompleteChatAsync
		(
			messages: messages,
			options: returnJson ? _jsonCompletionOptions : _completionOptions,
			cancellationToken: cancellationToken
		);
		string? summary = response.Content.FirstOrDefault()?.Text;
		if (string.IsNullOrEmpty(summary))
		{
			throw new InvalidOperationException("The response from the AI does not contain text.");
		}
		return summary;
	}
}

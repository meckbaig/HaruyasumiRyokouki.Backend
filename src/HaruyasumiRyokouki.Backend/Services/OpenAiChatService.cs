using HaruyasumiRyokouki.Backend.Common.Helpers;
using HaruyasumiRyokouki.Backend.Common.Options;
using HaruyasumiRyokouki.Backend.Common.Options.Abstractions;
using HaruyasumiRyokouki.Backend.Services.Interfaces;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

namespace HaruyasumiRyokouki.Backend.Services;

#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
internal class OpenAiChatService : IAiChatService
{
	// Local servers such as LM Studio need no key, but the client does not accept an empty one.
	private const string NoApiKey = "none";

	private readonly ChatClient _chatClient;
	private readonly OpenAiTranslationServiceOptions _options;
	private readonly ChatCompletionOptions _completionOptions;

	public OpenAiChatService(OpenAiTranslationServiceOptions options)
	{
		var aiClientOptions = new OpenAIClientOptions();
		if (!string.IsNullOrWhiteSpace(options.ApiUrl))
		{
			aiClientOptions.Endpoint = new Uri(options.ApiUrl);
		}
		var apiKey = new ApiKeyCredential(string.IsNullOrWhiteSpace(options.ApiKey) ? NoApiKey : options.ApiKey);
		_chatClient = new ChatClient(options.Model, apiKey, aiClientOptions);

		_options = options;
		_completionOptions = CreateCompletionOptions();
	}

	public Task<string> GetChatResponseAsync
	(
		string message,
		string? systemMessage = null,
		CancellationToken cancellationToken = default
	)
	{
		return CompleteChatAsync(message, systemMessage, _completionOptions, cancellationToken);
	}

	public async Task<T> GetJsonResponseAsync<T>
	(
		string message,
		string? systemMessage = null,
		CancellationToken cancellationToken = default
	)
	{
		var completionOptions = CreateCompletionOptions();
		completionOptions.ResponseFormat = _options.JsonResponseFormat switch
		{
			OpenAiJsonResponseFormat.JsonObject => ChatResponseFormat.CreateJsonObjectFormat(),
			OpenAiJsonResponseFormat.JsonSchema => ChatResponseFormat.CreateJsonSchemaFormat
			(
				jsonSchemaFormatName: typeof(T).Name,
				jsonSchema: BinaryData.FromString(AiJsonResponse.GetSchema<T>().GetRawText())
			),
			_ => null
		};

		var text = await CompleteChatAsync(message, systemMessage, completionOptions, cancellationToken);
		return AiJsonResponse.Deserialize<T>(text);
	}

	private async Task<string> CompleteChatAsync
	(
		string message,
		string? systemMessage,
		ChatCompletionOptions completionOptions,
		CancellationToken cancellationToken
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
			options: completionOptions,
			cancellationToken: cancellationToken
		);
		string? summary = response.Content.FirstOrDefault()?.Text;
		if (string.IsNullOrEmpty(summary))
		{
			throw new InvalidOperationException("The response from the AI does not contain text.");
		}
		return summary;
	}

	private ChatCompletionOptions CreateCompletionOptions()
	{
		return new ChatCompletionOptions()
		{
			Temperature = _options.Temperature,
			ReasoningEffortLevel = new ChatReasoningEffortLevel(_options.ReasoningLevel)
		};
	}
}

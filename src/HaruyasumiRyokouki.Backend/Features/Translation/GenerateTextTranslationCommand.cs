using HaruyasumiRyokouki.Backend.Services.Interfaces;
using MediatR;
using System.Text.Json;

namespace HaruyasumiRyokouki.Backend.Features.Translation;

public record GenerateTextTranslationCommand : IRequest<GenerateTextTranslationResponse>
{
	public required string InputText { get; init; }
	public required string TargetLanguage { get; init; }
}

public class GenerateTextTranslationResponse
{
	public required string Result { get; init; }
}


internal class GenerateTextTranslationHandler : IRequestHandler<GenerateTextTranslationCommand, GenerateTextTranslationResponse>
{
	private readonly IAiChatService _aiChatService;
	private readonly ILogger<GenerateTextTranslationHandler> _logger;
	private static JsonSerializerOptions _options = new JsonSerializerOptions
	{
		PropertyNameCaseInsensitive = true
	};

	public GenerateTextTranslationHandler(IAiChatService aiChatService, ILogger<GenerateTextTranslationHandler> logger)
	{
		_aiChatService = aiChatService;
		_logger = logger;
	}

	public async Task<GenerateTextTranslationResponse> Handle(GenerateTextTranslationCommand request, CancellationToken cancellationToken)
	{
		string structurePrompt = $$"""
You are a translator. Translate the following text to {{request.TargetLanguage}}. 
Preserve the original formatting exactly: line breaks, spacing, indentation, special characters, and structure. Do not modify any formatting elements. 
IMPORTANT: If the target language is Japanese, convert ALL romaji (Japanese words written in Latin alphabet) to proper Japanese script (kanji/kana). 
If the target language is NOT Japanese, leave romaji words unchanged. 
Return the result strictly in the following JSON schema: {\"result\": string}

Text to translate:
{{request.InputText}}
""";

		var responseString = await _aiChatService.GetChatResponseAsync(structurePrompt, returnJson: true, cancellationToken); 
		_logger.LogDebug("Ai response: {ResponseString}", responseString);

		return JsonSerializer.Deserialize<GenerateTextTranslationResponse>(responseString, _options)
			?? throw new Exception($"Failed to deserialize response from AI: {responseString}");
	}
}

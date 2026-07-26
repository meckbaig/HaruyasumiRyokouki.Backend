using HaruyasumiRyokouki.Backend.Services.Interfaces;
using MediatR;
using System.Text.Json;

namespace HaruyasumiRyokouki.Backend.Features.Translation;

public record GenerateMediaDescriptionTranslationCommand : IRequest<GenerateMediaDescriptionTranslationResponse>
{
	public required string Title { get; init; }
	public required string Description { get; init; }
	public required string Tags { get; init; }
	public required string TargetLanguage { get; init; }
}

public class GenerateMediaDescriptionTranslationResponse
{
	public required string Title { get; init; }
	public required string Description { get; init; }
	public required string Tags { get; init; }
}


internal class GenerateMediaDescriptionTranslationHandler : IRequestHandler<GenerateMediaDescriptionTranslationCommand, GenerateMediaDescriptionTranslationResponse>
{
	private readonly IAiChatService _aiChatService;
	private readonly ILogger<GenerateMediaDescriptionTranslationHandler> _logger;
	private static JsonSerializerOptions _options = new JsonSerializerOptions
	{
		PropertyNameCaseInsensitive = true
	};

	public GenerateMediaDescriptionTranslationHandler(IAiChatService aiChatService, ILogger<GenerateMediaDescriptionTranslationHandler> logger)
	{
		_aiChatService = aiChatService;
		_logger = logger;
	}

	public async Task<GenerateMediaDescriptionTranslationResponse> Handle(GenerateMediaDescriptionTranslationCommand request, CancellationToken cancellationToken)
	{
		string structurePrompt = $$"""
You are a translator. You will receive text with three fields labeled 'Title:', 'Description:', and 'Tags:'. These labels are structural markers, NOT content to translate — do NOT translate the words 'Title', 'Description', or 'Tags'. Translate ONLY the content after each label to {{request.TargetLanguage}}. 
Preserve the original formatting exactly: line breaks, spacing, indentation, special characters, and structure. Do not modify any formatting elements. 
IMPORTANT: If the target language is Japanese, convert ALL romaji (Japanese words written in Latin alphabet) to proper Japanese script (kanji/kana). 
If the target language is NOT Japanese, leave romaji words unchanged. 
Return the result strictly in the following JSON schema: {"title": string, "description": string, "tags": string}

Text to translate:
Title:
{{request.Title}}
Description: 
{{request.Description}}
Tags:
{{request.Tags}}
""";

		var responseString = await _aiChatService.GetChatResponseAsync(structurePrompt, returnJson: true, cancellationToken); 
		_logger.LogDebug("Ai response: {ResponseString}", responseString);

		return JsonSerializer.Deserialize<GenerateMediaDescriptionTranslationResponse>(responseString, _options)
			?? throw new Exception($"Failed to deserialize response from AI: {responseString}");
	}
}

using HaruyasumiRyokouki.Backend.Common.Options;
using HaruyasumiRyokouki.Backend.Common.Options.Abstractions;
using HaruyasumiRyokouki.Backend.Services.Interfaces;

namespace HaruyasumiRyokouki.Backend.Services.Translation.Factories;

/// <inheritdoc/>
public sealed class ContentTranslationServiceFactory : IContentTranslationServiceFactory
{
	private readonly IServiceProvider _services;

	/// <summary>
	/// Constructor with parameters for DI.
	/// </summary>
	public ContentTranslationServiceFactory(IServiceProvider services)
	{
		_services = services;
	}

	/// <inheritdoc/>
	public IContentTranslationService CreateTranslationService(TranslationServiceOptions options)
	{
		return options switch
		{
			OpenAiTranslationServiceOptions openAi =>
				CreateOpenAiTranslationService(openAi),

			GeminiTranslationServiceOptions gemini =>
				CreateGeminiAiTranslationService(gemini),

			DeeplTranslationServiceOptions deepl =>
				ActivatorUtilities.CreateInstance<DeeplTranslationService>(_services, deepl),

			_ => throw new ArgumentOutOfRangeException(
				nameof(options))
		};
	}

	/// <inheritdoc/>
	public IAiChatService CreateAiService(IAiServiceOptions options)
	{
		return options switch
		{
			OpenAiTranslationServiceOptions openAi =>
				ActivatorUtilities.CreateInstance<OpenAiChatService>(_services, openAi),

			GeminiTranslationServiceOptions gemini =>
				ActivatorUtilities.CreateInstance<GeminiAiChatService>(_services, gemini),

			_ => throw new ArgumentOutOfRangeException(
				nameof(options))
		};
	}

	private IContentTranslationService CreateOpenAiTranslationService(OpenAiTranslationServiceOptions options)
	{
		var chatService = ActivatorUtilities.CreateInstance<OpenAiChatService>(_services, options);

		return ActivatorUtilities.CreateInstance<AiTranslationService>(_services, chatService);
	}

	private IContentTranslationService CreateGeminiAiTranslationService(GeminiTranslationServiceOptions options)
	{
		var chatService = ActivatorUtilities.CreateInstance<GeminiAiChatService>(_services, options);

		return ActivatorUtilities.CreateInstance<AiTranslationService>(_services, chatService);
	}
}

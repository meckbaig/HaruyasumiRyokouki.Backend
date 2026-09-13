using HaruyasumiRyokouki.Backend.Common.Options.Abstractions;
using HaruyasumiRyokouki.Backend.Services.Interfaces;

namespace HaruyasumiRyokouki.Backend.Services.Translation.Factories;

public interface IContentTranslationServiceFactory
{
	IAiChatService CreateAiService(IAiServiceOptions options);
	IContentTranslationService CreateTranslationService(TranslationServiceOptions options);
}

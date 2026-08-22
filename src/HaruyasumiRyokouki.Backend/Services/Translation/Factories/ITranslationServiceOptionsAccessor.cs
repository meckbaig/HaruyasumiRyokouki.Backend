using HaruyasumiRyokouki.Backend.Common.Options.Abstractions;

namespace HaruyasumiRyokouki.Backend.Services.Translation.Factories;

public interface ITranslationServiceOptionsAccessor
{
	TranslationServiceOptions GetTranslationServiceOptions(string name);
	IAiServiceOptions GetAiServiceOptions(string name);
}

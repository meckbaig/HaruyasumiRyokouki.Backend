namespace HaruyasumiRyokouki.Backend.Common.Options.Abstractions;

public interface ITranslationOptionsParser
{
	TranslationProviderOptions Parse(IConfigurationSection section);
}

namespace HaruyasumiRyokouki.Backend.Common.Options.Abstractions;

/// <summary>
/// Reads the <c>TranslationProvider</c> section with its user-defined provider entries.
/// </summary>
public interface ITranslationOptionsParser
{
	/// <summary>
	/// Creates the options object of each provider entry by its <c>Type</c>.
	/// </summary>
	TranslationProviderOptions Parse(IConfigurationSection section);
}

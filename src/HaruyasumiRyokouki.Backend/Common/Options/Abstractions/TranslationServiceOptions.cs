namespace HaruyasumiRyokouki.Backend.Common.Options.Abstractions;

/// <summary>
/// Base of a <c>TranslationProvider:Providers</c> entry.
/// </summary>
public abstract class TranslationServiceOptions
{
	/// <summary>
	/// Provider type; defines the other fields of the entry.
	/// </summary>
	public required TranslationOptionsType Type { get; init; }
}

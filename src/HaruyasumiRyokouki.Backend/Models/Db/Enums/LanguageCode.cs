namespace HaruyasumiRyokouki.Backend.Models.Db.Enums;

/// <summary>
/// Supported content languages (ISO 639-1 codes).
/// </summary>
internal static class LanguageCode
{
	public const string Russian = "ru";
	public const string English = "en";
	public const string Japanese = "ja";

	/// <summary>
	/// Language used when the client does not request any supported language.
	/// </summary>
	public const string Default = English;

	/// <summary>
	/// All supported language codes.
	/// </summary>
	public static readonly IReadOnlySet<string> Supported = new HashSet<string> { English, Russian, Japanese };

	/// <summary>
	/// Returns <see langword="true"/> if <paramref name="code"/> is a supported language code.
	/// </summary>
	public static bool IsSupported(string? code) => code != null && Supported.Contains(code);
}

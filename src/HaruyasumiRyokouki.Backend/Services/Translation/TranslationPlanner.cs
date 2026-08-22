using HaruyasumiRyokouki.Backend.Models.Db.Enums;

namespace HaruyasumiRyokouki.Backend.Services.Translation;

internal sealed class TranslationPlanner
{
	private static readonly ICollection<LanguagePriority> _priorities =
	[
		new LanguagePriority(LanguageCode.English, nameof(LanguageCode.English), 1),
		new LanguagePriority(LanguageCode.Russian, nameof(LanguageCode.Russian), 2),
		new LanguagePriority(LanguageCode.Japanese, nameof(LanguageCode.Japanese), 3),
	];

	private record LanguagePriority(string LanguageCode, string LanguageName, int Priority);

	public static IReadOnlyCollection<string> Plan(IReadOnlyDictionary<string, string> translations, out string sourceText, out string sourceLanguageCode)
	{
		var existingTranslations = translations
			.Where(x => !string.IsNullOrWhiteSpace(x.Value))
			.ToDictionary();

		var source = _priorities
			.OrderBy(x => x.Priority)
			.FirstOrDefault(x => existingTranslations.ContainsKey(x.LanguageCode));

		if (source == null)
		{
			sourceText = string.Empty;
			sourceLanguageCode = string.Empty;
			return [];
		}

		sourceLanguageCode = source.LanguageCode;
		sourceText = existingTranslations[source.LanguageCode];

		return _priorities
			.Where(x => !existingTranslations.ContainsKey(x.LanguageCode))
			.Select(x => x.LanguageCode)
			.ToArray();
	}

	public static string? LanguageByCode(string languageCode)
	{
		return _priorities.FirstOrDefault(x => x.LanguageCode == languageCode)?.LanguageName;
	}
}

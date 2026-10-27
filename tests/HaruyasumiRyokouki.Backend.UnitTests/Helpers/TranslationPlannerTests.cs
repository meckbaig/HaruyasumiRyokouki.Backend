using HaruyasumiRyokouki.Backend.Common.Helpers;

namespace HaruyasumiRyokouki.Backend.UnitTests.Helpers;

public class TranslationPlannerTests
{
	[Fact]
	public void Plan_EnglishPresent_IsSourceForOtherLanguages()
	{
		var targets = TranslationPlanner.Plan(
			new Dictionary<string, string> { ["en"] = "Temple", ["ru"] = "Храм" },
			out var sourceText, out var sourceLanguage);

		Assert.Equal("en", sourceLanguage);
		Assert.Equal("Temple", sourceText);
		Assert.Equal(["ja"], targets);
	}

	[Fact]
	public void Plan_NoEnglish_RussianIsPreferredOverJapanese()
	{
		var targets = TranslationPlanner.Plan(
			new Dictionary<string, string> { ["ja"] = "寺", ["ru"] = "Храм" },
			out var sourceText, out var sourceLanguage);

		Assert.Equal("ru", sourceLanguage);
		Assert.Equal("Храм", sourceText);
		Assert.Equal(["en"], targets);
	}

	[Fact]
	public void Plan_OnlyJapanese_TranslatesToEnglishAndRussianInPriorityOrder()
	{
		var targets = TranslationPlanner.Plan(
			new Dictionary<string, string> { ["ja"] = "寺" },
			out _, out var sourceLanguage);

		Assert.Equal("ja", sourceLanguage);
		Assert.Equal(["en", "ru"], targets);
	}

	[Theory]
	[InlineData("")]
	[InlineData("   ")]
	public void Plan_BlankTranslation_IsTreatedAsMissing(string blank)
	{
		var targets = TranslationPlanner.Plan(
			new Dictionary<string, string> { ["en"] = blank, ["ru"] = "Храм" },
			out _, out var sourceLanguage);

		Assert.Equal("ru", sourceLanguage);
		Assert.Equal(["en", "ja"], targets);
	}

	[Fact]
	public void Plan_AllLanguagesPresent_NothingToTranslate()
	{
		var targets = TranslationPlanner.Plan(
			new Dictionary<string, string> { ["en"] = "Temple", ["ru"] = "Храм", ["ja"] = "寺" },
			out _, out var sourceLanguage);

		Assert.Equal("en", sourceLanguage);
		Assert.Empty(targets);
	}

	[Fact]
	public void Plan_NoSource_ReturnsEmptyPlan()
	{
		var targets = TranslationPlanner.Plan(
			new Dictionary<string, string> { ["en"] = " ", ["fr"] = "Temple" },
			out var sourceText, out var sourceLanguage);

		Assert.Empty(targets);
		Assert.Empty(sourceText);
		Assert.Empty(sourceLanguage);
	}

	[Theory]
	[InlineData("en", "English")]
	[InlineData("ru", "Russian")]
	[InlineData("ja", "Japanese")]
	[InlineData("fr", null)]
	public void LanguageByCode_ReturnsLanguageName(string code, string? expected)
	{
		Assert.Equal(expected, TranslationPlanner.LanguageByCode(code));
	}
}

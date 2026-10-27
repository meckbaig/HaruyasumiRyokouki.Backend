using HaruyasumiRyokouki.Backend.Common.Helpers;

namespace HaruyasumiRyokouki.Backend.UnitTests.Helpers;

public class AcceptLanguageParserTests
{
	[Theory]
	[InlineData("ru", "ru")]
	[InlineData("RU", "ru")]
	[InlineData("ja-JP", "ja")]
	[InlineData("en-US,en;q=0.9,ru;q=0.8", "en")]
	[InlineData("ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7", "ru")]
	public void Resolve_SupportedLanguage_ReturnsLanguageCode(string header, string expected)
	{
		Assert.Equal(expected, AcceptLanguageParser.Resolve(header));
	}

	[Theory]
	[InlineData("en;q=0.5,ja;q=0.9", "ja")]
	[InlineData("fr,de;q=0.9,ru;q=0.1", "ru")]
	public void Resolve_PicksHighestQualitySupportedLanguage(string header, string expected)
	{
		Assert.Equal(expected, AcceptLanguageParser.Resolve(header));
	}

	[Fact]
	public void Resolve_EqualQuality_KeepsHeaderOrder()
	{
		Assert.Equal("ja", AcceptLanguageParser.Resolve("ja,ru"));
	}

	[Fact]
	public void Resolve_ZeroQuality_IsExcluded()
	{
		Assert.Equal("en", AcceptLanguageParser.Resolve("ru;q=0,en;q=0.1"));
	}

	[Theory]
	[InlineData("fr")]
	[InlineData("fr-FR,de;q=0.9")]
	[InlineData("*")]
	[InlineData(";;;q=abc")]
	[InlineData("")]
	[InlineData("   ")]
	[InlineData(null)]
	public void Resolve_NoSupportedLanguage_ReturnsDefault(string? header)
	{
		Assert.Equal("en", AcceptLanguageParser.Resolve(header));
	}
}

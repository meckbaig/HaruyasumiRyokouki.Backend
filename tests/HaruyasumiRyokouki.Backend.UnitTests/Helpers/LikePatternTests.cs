using HaruyasumiRyokouki.Backend.Common.Helpers;

namespace HaruyasumiRyokouki.Backend.UnitTests.Helpers;

/// <summary>
/// Only the pattern text is checked here. Whether <c>ILIKE</c> is called with the escape character
/// is visible only in the generated SQL.
/// </summary>
public class LikePatternTests
{
	[Theory]
	[InlineData("temple", "temple")]
	[InlineData("100%", @"100\%")]
	[InlineData("snake_case", @"snake\_case")]
	[InlineData(@"C:\path", @"C:\\path")]
	[InlineData(@"\%_", @"\\\%\_")]
	public void Exact_EscapesWildcardsAndEscapeCharacter(string text, string expected)
	{
		Assert.Equal(expected, LikePattern.Exact(text));
	}

	[Theory]
	[InlineData("temple", "%temple%")]
	[InlineData("50%_off", @"%50\%\_off%")]
	[InlineData("", "%%")]
	public void Contains_WrapsEscapedTextInWildcards(string text, string expected)
	{
		Assert.Equal(expected, LikePattern.Contains(text));
	}

	[Fact]
	public void EscapeCharacter_IsBackslash()
	{
		Assert.Equal(@"\", LikePattern.EscapeCharacter);
	}
}

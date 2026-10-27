using HaruyasumiRyokouki.Backend.Common.Helpers;
using System.Text;

namespace HaruyasumiRyokouki.Backend.UnitTests.Helpers;

public class BasicCredentialsTests
{
	private static string Header(string credentials, string scheme = "Basic")
	{
		return $"{scheme} {Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials))}";
	}

	[Fact]
	public void TryParse_ValidHeader_ReturnsCredentials()
	{
		Assert.True(BasicCredentials.TryParse(Header("admin:secret"), out var username, out var password));
		Assert.Equal("admin", username);
		Assert.Equal("secret", password);
	}

	[Fact]
	public void TryParse_PasswordWithColon_KeepsWholePassword()
	{
		Assert.True(BasicCredentials.TryParse(Header("admin:se:cr:et"), out var username, out var password));
		Assert.Equal("admin", username);
		Assert.Equal("se:cr:et", password);
	}

	[Fact]
	public void TryParse_SchemeIsCaseInsensitive()
	{
		Assert.True(BasicCredentials.TryParse(Header("admin:secret", "basic"), out _, out _));
	}

	[Fact]
	public void TryParse_NonAsciiCredentials_DecodedAsUtf8()
	{
		Assert.True(BasicCredentials.TryParse(Header("админ:пароль"), out var username, out var password));
		Assert.Equal("админ", username);
		Assert.Equal("пароль", password);
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("Basic")]
	[InlineData("Basic !!!not-base64!!!")]
	[InlineData("Bearer YWRtaW46c2VjcmV0")]
	public void TryParse_MalformedHeader_ReturnsFalse(string? header)
	{
		Assert.False(BasicCredentials.TryParse(header, out var username, out var password));
		Assert.Empty(username);
		Assert.Empty(password);
	}

	[Fact]
	public void TryParse_NoColon_ReturnsFalse()
	{
		Assert.False(BasicCredentials.TryParse(Header("adminsecret"), out _, out _));
	}

	[Theory]
	[InlineData("admin", "secret", true)]
	[InlineData("admin", "wrong", false)]
	[InlineData("other", "secret", false)]
	[InlineData("Admin", "secret", false)]
	[InlineData("admin", "", false)]
	public void AreEqual_ComparesBothParts(string username, string password, bool expected)
	{
		Assert.Equal(expected, BasicCredentials.AreEqual(username, password, "admin", "secret"));
	}
}

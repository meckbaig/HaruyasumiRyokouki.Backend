using HaruyasumiRyokouki.Backend.Extensions;
using HaruyasumiRyokouki.Backend.Models.Dtos.Media;
using HaruyasumiRyokouki.Backend.Models.Dtos.Tags;
using System.Text.Json;

namespace HaruyasumiRyokouki.Backend.UnitTests.Json;

/// <summary>
/// PATCH requests rely on telling "field not sent" from "field sent as null":
/// a missing field must not overwrite the stored value.
/// </summary>
public class OptionalJsonTests
{
	private static T Deserialize<T>(string json)
	{
		return JsonSerializer.Deserialize<T>(json, JsonResponseExtensions.SerializerOptions)!;
	}

	[Fact]
	public void MissingFields_HaveNoValue()
	{
		var changes = Deserialize<EditMediaChanges>("{}");

		Assert.False(changes.Latitude.HasValue);
		Assert.False(changes.Longitude.HasValue);
		Assert.False(changes.IsApproved.HasValue);
		Assert.False(changes.Private.HasValue);
		Assert.False(changes.Favorite.HasValue);
		Assert.False(changes.TagIds.HasValue);
		Assert.False(changes.Translations.HasValue);
	}

	[Fact]
	public void NullField_HasNullValue()
	{
		var changes = Deserialize<EditMediaChanges>("""{"latitude":null,"tagIds":null}""");

		Assert.True(changes.Latitude.HasValue);
		Assert.Null(changes.Latitude.Value);
		Assert.True(changes.TagIds.HasValue);
		Assert.Null(changes.TagIds.Value);
		Assert.False(changes.Longitude.HasValue);
	}

	[Fact]
	public void SentFields_HaveTheirValues()
	{
		var changes = Deserialize<EditMediaChanges>(
			"""{"latitude":35.0116,"isApproved":false,"tagIds":[1,2],"translations":[{"languageCode":"en","title":"Kyoto"}]}""");

		Assert.Equal(35.0116, changes.Latitude.Value);
		Assert.True(changes.IsApproved.HasValue);
		Assert.False(changes.IsApproved.Value);
		Assert.Equal([1, 2], changes.TagIds.Value);
		Assert.Equal("Kyoto", Assert.Single(changes.Translations.Value!).Title);
	}

	[Fact]
	public void PropertyNames_AreCaseInsensitive()
	{
		var changes = Deserialize<EditMediaChanges>("""{"Latitude":10}""");

		Assert.Equal(10, changes.Latitude.Value);
	}

	[Fact]
	public void EditTagDto_DistinguishesMissingAndNull()
	{
		var tag = Deserialize<EditTagDto>("""{"slug":"temple","aliases":null}""");

		Assert.Equal("temple", tag.Slug.Value);
		Assert.True(tag.Aliases.HasValue);
		Assert.Null(tag.Aliases.Value);
		Assert.False(tag.Translations.HasValue);
	}
}

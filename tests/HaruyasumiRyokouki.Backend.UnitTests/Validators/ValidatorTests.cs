using FluentValidation.TestHelper;
using HaruyasumiRyokouki.Backend.Features.Days;
using HaruyasumiRyokouki.Backend.Features.Media;
using HaruyasumiRyokouki.Backend.Features.Search;
using HaruyasumiRyokouki.Backend.Models.Dtos.Days;
using HaruyasumiRyokouki.Backend.Models.Dtos.Media;
using HaruyasumiRyokouki.Backend.Models.Dtos.Tags;

namespace HaruyasumiRyokouki.Backend.UnitTests.Validators;

public class EditMediaValidatorTests
{
	private readonly EditMediaValidator _validator = new();

	private static EditMediaCommand Command(EditMediaChanges? changes, params int[] ids)
	{
		return new EditMediaCommand { Body = new EditMediaCommand.BodyParameters { Ids = ids, Changes = changes! } };
	}

	[Fact]
	public void ValidChanges_Pass()
	{
		var result = _validator.TestValidate(Command(new EditMediaChanges
		{
			Latitude = 35.0,
			Longitude = 135.7,
			TagIds = new List<int> { 1 },
			Translations = new List<MediaTranslationEditDto> { new() { LanguageCode = "en", Title = "Kyoto" } }
		}, 1, 2));

		result.ShouldNotHaveAnyValidationErrors();
	}

	[Fact]
	public void NullCoordinates_Pass()
	{
		_validator.TestValidate(Command(new EditMediaChanges { Latitude = null, Longitude = null }, 1))
			.ShouldNotHaveAnyValidationErrors();
	}

	[Fact]
	public void MissingBody_Fails()
	{
		_validator.TestValidate(new EditMediaCommand { Body = null! }).ShouldHaveValidationErrorFor(x => x.Body);
	}

	[Fact]
	public void EmptyIdsAndMissingChanges_Fail()
	{
		var result = _validator.TestValidate(Command(null));

		result.ShouldHaveValidationErrorFor("Body.Ids");
		result.ShouldHaveValidationErrorFor("Body.Changes");
	}

	[Theory]
	[InlineData(90.1, 0)]
	[InlineData(-90.1, 0)]
	[InlineData(0, 180.1)]
	[InlineData(0, -180.1)]
	public void CoordinatesOutOfRange_Fail(double latitude, double longitude)
	{
		var result = _validator.TestValidate(Command(new EditMediaChanges { Latitude = latitude, Longitude = longitude }, 1));

		Assert.False(result.IsValid);
	}

	[Fact]
	public void NullTagIdsAndTranslations_Fail()
	{
		var result = _validator.TestValidate(Command(new EditMediaChanges
		{
			TagIds = new Common.OptionalType.Optional<List<int>>(null),
			Translations = new Common.OptionalType.Optional<List<MediaTranslationEditDto>>(null)
		}, 1));

		result.ShouldHaveValidationErrorFor("Body.Changes.TagIds");
		result.ShouldHaveValidationErrorFor("Body.Changes.Translations");
	}

	[Fact]
	public void DuplicateLanguages_Fail()
	{
		var result = _validator.TestValidate(Command(new EditMediaChanges
		{
			Translations = new List<MediaTranslationEditDto> { new() { LanguageCode = "en" }, new() { LanguageCode = "en" } }
		}, 1));

		result.ShouldHaveValidationErrorFor("Body.Changes.Translations");
	}

	[Fact]
	public void UnsupportedLanguage_Fails()
	{
		var result = _validator.TestValidate(Command(new EditMediaChanges
		{
			Translations = new List<MediaTranslationEditDto> { new() { LanguageCode = "fr" } }
		}, 1));

		result.ShouldHaveValidationErrorFor("Body.Changes.Translations");
	}
}

public class EditDayValidatorTests
{
	private readonly EditDayValidator _validator = new();

	private static EditDayCommand Command(params DayTranslationEditDto[] translations)
	{
		return new EditDayCommand
		{
			Date = new DateOnly(2026, 3, 1),
			Body = new EditDayCommand.BodyParameters { Day = new DayEditDto { Translations = translations } }
		};
	}

	[Fact]
	public void ValidDay_Passes()
	{
		_validator.TestValidate(Command(new DayTranslationEditDto { LanguageCode = "ru", Note = "" }))
			.ShouldNotHaveAnyValidationErrors();
	}

	[Fact]
	public void MissingBodyOrDay_Fails()
	{
		_validator.TestValidate(new EditDayCommand { Date = default, Body = null! }).ShouldHaveValidationErrorFor(x => x.Body);
		_validator.TestValidate(new EditDayCommand { Date = default, Body = new EditDayCommand.BodyParameters { Day = null! } })
			.ShouldHaveValidationErrorFor("Body.Day");
	}

	[Fact]
	public void NullTranslations_Fail()
	{
		var command = Command();
		command.Body.Day.Translations = null!;

		_validator.TestValidate(command).ShouldHaveValidationErrorFor("Body.Day.Translations");
	}

	[Fact]
	public void DuplicateLanguages_Fail()
	{
		_validator.TestValidate(Command(
				new DayTranslationEditDto { LanguageCode = "en", Note = "a" },
				new DayTranslationEditDto { LanguageCode = "en", Note = "b" }))
			.ShouldHaveValidationErrorFor("Body.Day.Translations");
	}

	[Fact]
	public void UnsupportedLanguageAndNullNote_Fail()
	{
		var result = _validator.TestValidate(Command(new DayTranslationEditDto { LanguageCode = "fr", Note = null! }));

		result.ShouldHaveValidationErrorFor("Body.Day.Translations[0].LanguageCode");
		result.ShouldHaveValidationErrorFor("Body.Day.Translations[0].Note");
	}
}

public class TagDtoValidatorTests
{
	private static TagTranslationDto T(string languageCode, string text) => new() { LanguageCode = languageCode, Text = text };

	private static CreateTagDto Tag(TagTranslationDto[] translations, params TagTranslationDto[] aliases)
	{
		return new CreateTagDto { Slug = "temple", Translations = translations, Aliases = aliases };
	}

	[Fact]
	public void ValidTag_Passes()
	{
		new CreateTagDto.Validator()
			.TestValidate(Tag([T("en", "temple"), T("ru", "храм"), T("ja", "寺")], T("en", "shrine")))
			.ShouldNotHaveAnyValidationErrors();
	}

	[Fact]
	public void SameTextInDifferentLanguages_Fails()
	{
		// Texts are unique per tag in the database regardless of language.
		var result = new CreateTagDto.Validator().TestValidate(Tag([T("en", "sushi"), T("ja", "sushi")]));

		Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("unique within a tag") && e.ErrorMessage.Contains("sushi"));
	}

	[Fact]
	public void AliasEqualToTranslation_IgnoringCase_Fails()
	{
		var result = new CreateTagDto.Validator().TestValidate(Tag([T("en", "Temple")], T("en", "temple")));

		Assert.Contains(result.Errors, e => e.ErrorMessage == "Aliases must not duplicate Translations.");
	}

	[Fact]
	public void DuplicateLanguagesInTranslations_Fail()
	{
		new CreateTagDto.Validator().TestValidate(Tag([T("en", "temple"), T("en", "shrine")]))
			.ShouldHaveValidationErrorFor(x => x.Translations);
	}

	[Fact]
	public void MissingSlugAndLists_Fail()
	{
		var result = new CreateTagDto.Validator().TestValidate(new CreateTagDto { Slug = " ", Translations = null!, Aliases = null! });

		result.ShouldHaveValidationErrorFor(x => x.Slug);
		result.ShouldHaveValidationErrorFor(x => x.Translations);
		result.ShouldHaveValidationErrorFor(x => x.Aliases);
	}

	[Fact]
	public void UnsupportedLanguageAndEmptyText_Fail()
	{
		var result = new CreateTagDto.Validator().TestValidate(Tag([T("fr", "")], T("xx", "alias")));

		result.ShouldHaveValidationErrorFor("Translations[0].LanguageCode");
		result.ShouldHaveValidationErrorFor("Translations[0].Text");
		result.ShouldHaveValidationErrorFor("Aliases[0].LanguageCode");
	}

	[Fact]
	public void EditTag_OnlySentFieldsAreValidated()
	{
		new EditTagDto.Validator().TestValidate(new EditTagDto()).ShouldNotHaveAnyValidationErrors();
	}

	[Fact]
	public void EditTag_NullListsAndEmptySlug_Fail()
	{
		var result = new EditTagDto.Validator().TestValidate(new EditTagDto
		{
			Slug = "",
			Translations = new Common.OptionalType.Optional<ICollection<TagTranslationDto>>(null),
			Aliases = new Common.OptionalType.Optional<ICollection<TagTranslationDto>>(null)
		});

		Assert.Equal(3, result.Errors.Count);
		result.ShouldHaveValidationErrorFor("Translations");
		result.ShouldHaveValidationErrorFor("Aliases");
	}
}

public class QueryValidatorTests
{
	[Theory]
	[InlineData("temple", null, null)]
	[InlineData(null, "temple", null)]
	[InlineData(null, null, 1)]
	public void Search_AnyCriterion_Passes(string? text, string? tag, int? tagId)
	{
		new SearchQueryValidator().TestValidate(new SearchQuery { Text = text, Tag = tag, TagId = tagId })
			.ShouldNotHaveAnyValidationErrors();
	}

	[Fact]
	public void Search_OnlyWhitespace_Fails()
	{
		Assert.False(new SearchQueryValidator().TestValidate(new SearchQuery { Text = "  ", Tag = " " }).IsValid);
	}

	[Fact]
	public void GetMedia_ToBeforeFrom_Fails()
	{
		new GetMediaQueryValidator().TestValidate(new GetMediaQuery { From = new DateOnly(2026, 3, 2), To = new DateOnly(2026, 3, 1) })
			.ShouldHaveValidationErrorFor(x => x.To);
	}

	[Fact]
	public void GetMedia_SingleDay_Passes()
	{
		new GetMediaQueryValidator().TestValidate(new GetMediaQuery { From = new DateOnly(2026, 3, 1), To = new DateOnly(2026, 3, 1) })
			.ShouldNotHaveAnyValidationErrors();
	}
}

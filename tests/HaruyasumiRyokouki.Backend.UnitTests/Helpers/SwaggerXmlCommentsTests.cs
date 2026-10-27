using HaruyasumiRyokouki.Backend.Common.Helpers;

namespace HaruyasumiRyokouki.Backend.UnitTests.Helpers;

public class SwaggerXmlCommentsTests
{
	[Theory]
	[InlineData("T:HaruyasumiRyokouki.Backend.Models.Dtos.Days.DayEditDto", "DayEditDto")]
	[InlineData("P:HaruyasumiRyokouki.Backend.Models.Dtos.Days.DayEditDto.Translations", "DayEditDto.Translations")]
	[InlineData("F:HaruyasumiRyokouki.Backend.Models.Db.Enums.MediaSource.Local", "MediaSource.Local")]
	public void ToCodeName_TypeAndMember_WithoutNamespace(string cref, string expected)
	{
		Assert.Equal(expected, SwaggerXmlComments.ToCodeName(cref));
	}

	[Theory]
	[InlineData("T:HaruyasumiRyokouki.Backend.Features.Days.EditDayCommand.BodyParameters", "EditDayCommand.BodyParameters")]
	[InlineData("P:HaruyasumiRyokouki.Backend.Features.Days.EditDayCommand.BodyParameters.AutoTranslate", "EditDayCommand.BodyParameters.AutoTranslate")]
	public void ToCodeName_NestedType_KeepsDeclaringType(string cref, string expected)
	{
		Assert.Equal(expected, SwaggerXmlComments.ToCodeName(cref));
	}

	[Theory]
	[InlineData("M:HaruyasumiRyokouki.Backend.Services.Interfaces.IMediaPreviewService.GetImageUrl(System.String,HaruyasumiRyokouki.Backend.Models.InternalDtos.Enums.ImageUrlType,HaruyasumiRyokouki.Backend.Models.InternalDtos.ClientDisplay,System.Nullable{System.Single})", "IMediaPreviewService.GetImageUrl")]
	[InlineData("M:HaruyasumiRyokouki.Backend.Models.Db.MediaTranslation.Clone", "MediaTranslation.Clone")]
	public void ToCodeName_Method_WithoutParameters(string cref, string expected)
	{
		Assert.Equal(expected, SwaggerXmlComments.ToCodeName(cref));
	}

	[Fact]
	public void ToCodeName_GenericType_ShowsTypeParameters()
	{
		Assert.Equal("Optional<T>", SwaggerXmlComments.ToCodeName("T:HaruyasumiRyokouki.Backend.Common.OptionalType.Optional`1"));
	}

	[Fact]
	public void ToCodeName_TypeFromAnotherAssembly()
	{
		Assert.Equal("StatusCodes.Status503ServiceUnavailable",
			SwaggerXmlComments.ToCodeName("F:Microsoft.AspNetCore.Http.StatusCodes.Status503ServiceUnavailable"));
	}

	[Theory]
	[InlineData("T:Unknown.Namespace.Missing", "Missing")]
	[InlineData("P:Unknown.Namespace.Missing.Value", "Missing.Value")]
	[InlineData("N:Microsoft", "Microsoft")]
	[InlineData("Plain.Text", "Text")]
	public void ToCodeName_Unresolved_FallsBackToLastSegments(string cref, string expected)
	{
		Assert.Equal(expected, SwaggerXmlComments.ToCodeName(cref));
	}
}

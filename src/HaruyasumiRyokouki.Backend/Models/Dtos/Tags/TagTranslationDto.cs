using FluentValidation;

namespace HaruyasumiRyokouki.Backend.Models.Dtos.Tags;

public record TagTranslationDto
{
	public string LanguageCode { get; set; }
	public string Text { get; set; }

	internal class Validator : AbstractValidator<TagTranslationDto>
	{
		public Validator()
		{
			RuleFor(x => x.LanguageCode)
				.Must(Db.Enums.LanguageCode.IsSupported)
				.WithMessage($"Language code must be one of: {string.Join(", ", Db.Enums.LanguageCode.Supported)}.");
			RuleFor(x => x.Text)
				.NotEmpty();
		}
	}
}

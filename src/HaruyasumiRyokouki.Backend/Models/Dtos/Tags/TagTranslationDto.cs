using FluentValidation;

namespace HaruyasumiRyokouki.Backend.Models.Dtos.Tags;

/// <summary>
/// Text of a tag in one language: a display name or a search alias.
/// </summary>
public record TagTranslationDto
{
	/// <summary>
	/// Language code: <c>en</c>, <c>ru</c> or <c>ja</c>.
	/// </summary>
	public string LanguageCode { get; set; }

	/// <summary>
	/// The text; unique within the tag.
	/// </summary>
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

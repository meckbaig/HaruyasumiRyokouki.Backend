using FluentValidation;

namespace HaruyasumiRyokouki.Backend.Models.Dtos.Tags;

/// <summary>
/// Text of a tag in one language: a display name or a search alias.
/// </summary>
public record TagTranslationDto
{
	/// <summary>
	/// Language of the text: <c>en</c>, <c>ru</c> or <c>ja</c>.
	/// </summary>
	/// <remarks>
	/// See <see cref="Db.Enums.LanguageCode"/>.
	/// </remarks>
	public string LanguageCode { get; set; } = null!;

	/// <summary>
	/// The text. Unique within the tag.
	/// </summary>
	public string Text { get; set; } = null!;

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

using FluentValidation;
using HaruyasumiRyokouki.Backend.Common.OptionalType;
using HaruyasumiRyokouki.Backend.Models.Db;
using Meckbaig.Cqrs.Dto.Abstractions;

namespace HaruyasumiRyokouki.Backend.Models.Dtos.Tags;

/// <summary>
/// Changes of a tag. An absent field is not modified.
/// </summary>
public record EditTagDto : IEditDto
{
	/// <summary>
	/// Unique tag key, used in links (<c>/v1/search?tag=</c>).
	/// </summary>
	public Optional<string> Slug { get; set; }

	/// <summary>
	/// Replaces all display names, at most one per language.
	/// </summary>
	public Optional<ICollection<TagTranslationDto>> Translations { get; set; }

	/// <summary>
	/// Replaces all search aliases.
	/// </summary>
	public Optional<ICollection<TagTranslationDto>> Aliases { get; set; }

	/// <summary>
	/// Entity type the DTO edits.
	/// </summary>
	public static Type GetOriginType() => typeof(Tag);

	/// <summary>
	/// Validator of the DTO.
	/// </summary>
	public static Type GetValidatorType() => typeof(Validator);

	internal class Validator : AbstractValidator<EditTagDto>
	{
		public Validator()
		{
			RuleFor(x => x.Slug.Value)
				.NotEmpty()
				.When(x => x.Slug.HasValue);
			RuleFor(x => x.Translations.Value)
				.Cascade(CascadeMode.Stop)
				.NotNull()
				.Must(BeDistinct)
				.WithMessage("Translations must have exactly one variation for each language.")
				.OverridePropertyName(nameof(Translations))
				.When(x => x.Translations.HasValue);
			RuleForEach(x => x.Translations.Value)
				.SetValidator(new TagTranslationDto.Validator())
				.OverridePropertyName(nameof(Translations))
				.When(x => x.Translations.HasValue);
			RuleFor(x => x.Aliases.Value)
				.NotNull()
				.OverridePropertyName(nameof(Aliases))
				.When(x => x.Aliases.HasValue);
			RuleForEach(x => x.Aliases.Value)
				.SetValidator(new TagTranslationDto.Validator())
				.OverridePropertyName(nameof(Aliases))
				.When(x => x.Aliases.HasValue);
			RuleFor(x => x)
				.Must(NoAliasDuplicatesWithTranslations)
				.WithMessage("Aliases must not duplicate Translations.");
		}

		private bool BeDistinct(ICollection<TagTranslationDto> collection)
		{
			return collection.DistinctBy(c => c.LanguageCode).Count() == collection.Count;
		}

		private bool NoAliasDuplicatesWithTranslations(EditTagDto dto)
		{
			if (!dto.Aliases.HasValue || !dto.Translations.HasValue)
				return true;

			if (dto.Aliases.Value is null || dto.Translations.Value is null)
				return true;

			var translations = dto.Translations.Value
				.Select(x => x.Text)
				.ToHashSet(StringComparer.OrdinalIgnoreCase);

			return !dto.Aliases.Value.Any(alias => translations.Contains(alias.Text));
		}
	}
}

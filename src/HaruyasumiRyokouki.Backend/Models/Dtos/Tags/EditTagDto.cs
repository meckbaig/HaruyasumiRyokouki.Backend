using FluentValidation;
using HaruyasumiRyokouki.Backend.Common.OptionalType;
using HaruyasumiRyokouki.Backend.Models.Db;
using Meckbaig.Cqrs.Dto.Abstractions;

namespace HaruyasumiRyokouki.Backend.Models.Dtos.Tags;

public record EditTagDto : IEditDto
{
	public Optional<string> Slug { get; set; }
	public Optional<ICollection<TagTranslationDto>> Translations { get; set; }
	public Optional<ICollection<TagTranslationDto>> Aliases { get; set; }

	public static Type GetOriginType() => typeof(Tag);
	public static Type GetValidatorType() => typeof(Validator);

	internal class Validator : AbstractValidator<EditTagDto>
	{
		public Validator()
		{
			RuleFor(x => x.Slug.Value)
				.NotEmpty()
				.When(x => x.Slug.HasValue);
			RuleFor(x => x.Translations.Value)
				.Must(BeDistinct)
				.WithMessage("Translations must have exactly one variation for each language.")
				.When(x => x.Translations.HasValue);
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

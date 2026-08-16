using FluentValidation;
using HaruyasumiRyokouki.Backend.Models.Db;
using Meckbaig.Cqrs.Dto.Abstractions;

namespace HaruyasumiRyokouki.Backend.Models.Dtos.Tags;

public record CreateTagDto : IEditDto
{
	public string Slug { get; set; }
	public ICollection<TagTranslationDto> Translations { get; set; }
	public ICollection<TagTranslationDto> Aliases { get; set; }

	public static Type GetOriginType() => typeof(Tag);
	public static Type GetValidatorType() => typeof(Validator);

	internal class Validator : AbstractValidator<CreateTagDto>
	{
		public Validator()
		{
			RuleFor(x => x.Slug)
				.NotEmpty();
			RuleFor(x => x.Translations)
				.Must(BeDistinct)
				.WithMessage("Translations must have exactly one variation for each language.");
			RuleFor(x => x)
				.Must(NoAliasDuplicatesWithTranslations)
				.WithMessage("Aliases must not duplicate Translations.");
		}

		private bool BeDistinct(ICollection<TagTranslationDto> collection)
		{
			return collection.DistinctBy(c => c.LanguageCode).Count() == collection.Count;
		}

		private bool NoAliasDuplicatesWithTranslations(CreateTagDto dto)
		{
			var translations = dto.Translations
				.Select(x => x.Text)
				.ToHashSet(StringComparer.OrdinalIgnoreCase);

			return !dto.Aliases.Any(alias => translations.Contains(alias.Text));
		}
	}
}

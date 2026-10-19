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
			RuleFor(x => x)
				.Must(x => !FindDuplicateTexts(x).Any())
				.WithMessage(x => $"Texts must be unique within a tag, duplicated: {string.Join(", ", FindDuplicateTexts(x))}.");
		}

		/// <summary>
		/// Texts used more than once among all translations and aliases (unique per tag in the database, regardless of language).
		/// </summary>
		private static IEnumerable<string> FindDuplicateTexts(CreateTagDto dto)
		{
			return (dto.Translations ?? []).Concat(dto.Aliases ?? [])
				.GroupBy(t => t.Text)
				.Where(g => g.Count() > 1)
				.Select(g => g.Key);
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

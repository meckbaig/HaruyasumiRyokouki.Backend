using FluentValidation;
using HaruyasumiRyokouki.Backend.Models.Db;
using Meckbaig.Cqrs.Dto.Abstractions;

namespace HaruyasumiRyokouki.Backend.Models.Dtos.Tags;

/// <summary>
/// New tag.
/// </summary>
/// <remarks>
/// All texts of the tag, display names and aliases in all languages, must be different.
/// </remarks>
public record CreateTagDto : IEditDto
{
	/// <summary>
	/// Unique key of the tag in English, e.g. <c>cherry_blossom</c>.
	/// </summary>
	/// <remarks>
	/// Used in links to the tag search: <c>GET /v1/search?tag=cherry_blossom</c>.
	/// </remarks>
	public string Slug { get; set; }

	/// <summary>
	/// Display names of the tag, at most one per language.
	/// </summary>
	public ICollection<TagTranslationDto> Translations { get; set; }

	/// <summary>
	/// Search aliases of the tag: synonyms and spelling variants in any supported language.
	/// </summary>
	/// <remarks>
	/// Aliases are never displayed. They let the tag be found by other words.
	/// </remarks>
	public ICollection<TagTranslationDto> Aliases { get; set; }

	/// <summary>
	/// Entity created from the DTO.
	/// </summary>
	public static Type GetOriginType() => typeof(Tag);

	/// <summary>
	/// Validator of the DTO.
	/// </summary>
	public static Type GetValidatorType() => typeof(Validator);

	internal class Validator : AbstractValidator<CreateTagDto>
	{
		public Validator()
		{
			RuleFor(x => x.Slug)
				.NotEmpty();
			RuleFor(x => x.Translations)
				.Cascade(CascadeMode.Stop)
				.NotNull()
				.Must(BeDistinct)
				.WithMessage("Translations must have exactly one variation for each language.");
			RuleForEach(x => x.Translations)
				.SetValidator(new TagTranslationDto.Validator());
			RuleFor(x => x.Aliases)
				.NotNull();
			RuleForEach(x => x.Aliases)
				.SetValidator(new TagTranslationDto.Validator());
			RuleFor(x => x)
				.Must(NoAliasDuplicatesWithTranslations)
				.WithMessage("Aliases must not duplicate Translations.")
				.When(x => x.Translations != null && x.Aliases != null);
			RuleFor(x => x)
				.Must(x => !FindDuplicateTexts(x).Any())
				.WithMessage(x => $"Texts must be unique within a tag, duplicated: {string.Join(", ", FindDuplicateTexts(x))}.");
		}

		/// <summary>
		/// Returns the texts used more than once among all display names and aliases.
		/// </summary>
		/// <remarks>
		/// Texts are unique per tag in the database, regardless of the language.
		/// </remarks>
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

using FluentValidation;
using HaruyasumiRyokouki.Backend.Common.OptionalType;
using HaruyasumiRyokouki.Backend.Models.Db;
using Meckbaig.Cqrs.Dto.Abstractions;

namespace HaruyasumiRyokouki.Backend.Models.Dtos.Tags;

/// <summary>
/// Changes of a tag.
/// </summary>
/// <remarks>
/// Every field is an <see cref="Common.OptionalType.Optional{T}"/>: a field absent from the request is not modified.
/// </remarks>
public record EditTagDto : IEditDto
{
	/// <summary>
	/// Unique key of the tag in English, e.g. <c>cherry_blossom</c>.
	/// </summary>
	/// <remarks>
	/// Used in links to the tag search: <c>GET /v1/search?tag=cherry_blossom</c>.
	/// </remarks>
	public Optional<string> Slug { get; set; }

	/// <summary>
	/// New display names, at most one per language.
	/// </summary>
	/// <remarks>
	/// Replace all current display names.
	/// </remarks>
	public Optional<ICollection<TagTranslationDto>> Translations { get; set; }

	/// <summary>
	/// New search aliases.
	/// </summary>
	/// <remarks>
	/// Replace all current aliases.
	/// </remarks>
	public Optional<ICollection<TagTranslationDto>> Aliases { get; set; }

	/// <summary>
	/// Entity edited with the DTO.
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
				.Must(t => BeDistinct(t!))
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

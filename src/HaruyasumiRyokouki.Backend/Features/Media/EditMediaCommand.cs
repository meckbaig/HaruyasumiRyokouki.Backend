using FluentValidation;
using HaruyasumiRyokouki.Backend.Common.Abstractions;
using HaruyasumiRyokouki.Backend.Common.Exceptions;
using HaruyasumiRyokouki.Backend.Common.Helpers;
using HaruyasumiRyokouki.Backend.Common.Options;
using HaruyasumiRyokouki.Backend.DbContexts;
using HaruyasumiRyokouki.Backend.Extensions;
using HaruyasumiRyokouki.Backend.Models.Db;
using HaruyasumiRyokouki.Backend.Models.Db.Enums;
using HaruyasumiRyokouki.Backend.Models.Dtos.Media;
using HaruyasumiRyokouki.Backend.Services.Interfaces;
using HaruyasumiRyokouki.Backend.Services.Translation.Factories;
using Meckbaig.Cqrs.Abstractons;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json.Serialization;

namespace HaruyasumiRyokouki.Backend.Features.Media;

/// <summary>
/// Request of <c>PATCH /v1/media</c>.
/// </summary>
public record EditMediaCommand : IRequest<EditMediaResponse>, ILocalizableRequest
{
	/// <summary>
	/// Request body.
	/// </summary>
	[FromBody]
	public required BodyParameters Body { get; init; }

	/// <inheritdoc/>
	[SwaggerIgnore]
	[JsonIgnore]
	public string AcceptLanguage { get; set; } = LanguageCode.Default;

	/// <summary>
	/// Body of <see cref="EditMediaCommand"/>.
	/// </summary>
	public record BodyParameters
	{
		/// <summary>
		/// Media files to edit.
		/// </summary>
		/// <remarks>
		/// The same changes are applied to all of them.
		/// </remarks>
		public ICollection<int> Ids { get; set; } = [];

		/// <summary>
		/// Changes to apply.
		/// </summary>
		/// <remarks>
		/// Fields absent from the request are not modified.
		/// </remarks>
		public EditMediaChanges Changes { get; set; }

		/// <summary>
		/// Translate titles and descriptions sent in <see cref="EditMediaChanges.Translations"/> into the missing languages and return them without saving.
		/// </summary>
		public bool AutoTranslate { get; init; }
	}
}

/// <summary>
/// Response of <c>PATCH /v1/media</c>.
/// </summary>
public class EditMediaResponse : BaseResponse
{
	/// <summary>
	/// The edited media with texts in all languages and tags in the requested language.
	/// </summary>
	public required IEnumerable<MediaFileEditDto> Items { get; init; }
}

internal class EditMediaValidator : AbstractValidator<EditMediaCommand>
{
	public EditMediaValidator()
	{
		RuleFor(x => x.Body)
			.NotNull()
			.SetValidator(new BodyParametersValidator());
	}

	internal class BodyParametersValidator : AbstractValidator<EditMediaCommand.BodyParameters>
	{
		public BodyParametersValidator()
		{
			RuleFor(x => x.Ids)
				.NotEmpty();
			RuleFor(x => x.Changes)
				.NotNull()
				.SetValidator(new EditMediaChangesValidator());
		}
	}

	internal class EditMediaChangesValidator : AbstractValidator<EditMediaChanges>
	{
		public EditMediaChangesValidator()
		{
			RuleFor(x => x.Latitude.Value)
				.InclusiveBetween(-90, 90)
				.OverridePropertyName(nameof(EditMediaChanges.Latitude))
				.When(x => x.Latitude.HasValue && x.Latitude.Value.HasValue);
			RuleFor(x => x.Longitude.Value)
				.InclusiveBetween(-180, 180)
				.OverridePropertyName(nameof(EditMediaChanges.Longitude))
				.When(x => x.Longitude.HasValue && x.Longitude.Value.HasValue);
			RuleFor(x => x.TagIds.Value)
				.NotNull()
				.OverridePropertyName(nameof(EditMediaChanges.TagIds))
				.When(x => x.TagIds.HasValue);
			RuleFor(x => x.Translations.Value)
				.Cascade(CascadeMode.Stop)
				.NotNull()
				.Must(t => t!.DistinctBy(x => x.LanguageCode).Count() == t!.Count)
				.WithMessage("Translations must have at most one variation for each language.")
				.Must(t => t!.All(x => LanguageCode.IsSupported(x.LanguageCode)))
				.WithMessage($"Language code must be one of: {string.Join(", ", LanguageCode.Supported)}.")
				.OverridePropertyName(nameof(EditMediaChanges.Translations))
				.When(x => x.Translations.HasValue);
		}
	}
}

internal class EditMediaHandler : IRequestHandler<EditMediaCommand, EditMediaResponse>
{
	private readonly IAppDbContext _context;
	private readonly IContentTranslationService _translationService;

	public EditMediaHandler
	(
		IAppDbContext context,
		ITranslationServiceOptionsAccessor translationAccessor,
		IContentTranslationServiceFactory factory,
		IOptions<TranslationProviderOptions> translationOptions
	)
	{
		_context = context;
		_translationService = factory.CreateTranslationService(translationAccessor.GetTranslationServiceOptions(translationOptions.Value.Usage.Media));
	}

	public async Task<EditMediaResponse> Handle(EditMediaCommand request, CancellationToken cancellationToken)
	{
		var mediaToEdit = await _context.MediaFiles
			.Include(m => m.MediaTags)
			.Include(m => m.Translations)
			.Where(m => request.Body.Ids.Contains(m.Id))
			.ToListAsync(cancellationToken);

		EntityNotFoundException.ThrowIfAnyMissing(request.Body.Ids, mediaToEdit.Select(m => m.Id), "Media file");

		if (request.Body.Changes.Latitude.HasValue)
			mediaToEdit.ForEach(m => m.Latitude = request.Body.Changes.Latitude);
		if (request.Body.Changes.Longitude.HasValue)
			mediaToEdit.ForEach(m => m.Longitude = request.Body.Changes.Longitude);
		if (request.Body.Changes.IsApproved.HasValue)
			mediaToEdit.ForEach(m => m.IsApproved = request.Body.Changes.IsApproved);
		if (request.Body.Changes.Private.HasValue)
			mediaToEdit.ForEach(m => m.Private = request.Body.Changes.Private);
		if (request.Body.Changes.Favorite.HasValue)
			mediaToEdit.ForEach(m => m.Favorite = request.Body.Changes.Favorite);
		if (request.Body.Changes.TagIds.HasValue)
		{
			var tagChanges = request.Body.Changes.TagIds.Value;
			foreach (var media in mediaToEdit)
			{
				var toAdd = tagChanges.Where(mt => !media.MediaTags.Any(m => m.TagId == mt));
				media.MediaTags = media.MediaTags.Where(mt => tagChanges.Contains(mt.TagId)).ToList();
				media.MediaTags = media.MediaTags.Concat(toAdd.Select(x => new MediaFileTag { TagId = x })).ToList();
			}
		}
		if (request.Body.Changes.Translations.HasValue)
		{
			var newTranslations = request.Body.Changes.Translations.Value!.FromEditDtos().ToList();
			mediaToEdit.ForEach(m => UpdateTranslations(m.Translations, newTranslations));
		}

		await _context.SaveChangesAsync(cancellationToken);

		if (request.Body.AutoTranslate)
		{
			var newTranslationsDtos = await TranslateMedia(request.Body.Changes.Translations.Value ?? [], cancellationToken);
			var newTranslations = newTranslationsDtos.FromEditDtos().ToList();
			mediaToEdit.ForEach(m => UpdateTranslations(m.Translations, newTranslations));
		}

		await EnrichWithTags(request, mediaToEdit, cancellationToken);
		return new EditMediaResponse { Items = mediaToEdit.ToEditDtos() };
	}

	private async Task EnrichWithTags(EditMediaCommand request, List<MediaFile> mediaToEdit, CancellationToken cancellationToken)
	{
		var newTags = await _context.MediaFileTags
			.Include(mt => mt.Tag)
				.ThenInclude(t => t.Translations.Where(tt => tt.LanguageCode == request.AcceptLanguage && tt.IsPrimary))
			.Where(mt => mediaToEdit.Select(m => m.Id).Contains(mt.MediaId))
			.ToListAsync(cancellationToken);

		mediaToEdit.ForEach(m => m.Tags = newTags.Where(nt => nt.MediaId == m.Id).Select(mt => mt.Tag).ToList());
	}

	private async Task<ICollection<MediaTranslationEditDto>> TranslateMedia(ICollection<MediaTranslationEditDto> mediaTranslations, CancellationToken cancellationToken)
	{
		var titleTargets = TranslationPlanner.Plan
		(
			mediaTranslations.ToDictionary(x => x.LanguageCode, x => x.Title),
			out string titleSourceText,
			out string titleSourceLanguageCode
		);
		var descriptionTargets = TranslationPlanner.Plan
		(
			mediaTranslations.ToDictionary(x => x.LanguageCode, x => x.Description),
			out string descriptionSourceText,
			out string descriptionSourceLanguageCode
		);

		var translationTasks = mediaTranslations
			.Select(x => x.LanguageCode)
			.Concat(titleTargets)
			.Concat(descriptionTargets)
			.Distinct()
			.Select(async languageCode =>
			{
				var existing = mediaTranslations
					.FirstOrDefault(x => x.LanguageCode == languageCode);

				var title = existing?.Title;
				var description = existing?.Description;

				if (titleTargets.Contains(languageCode))
				{
					title = await _translationService.TranslateTextAsync
					(
						titleSourceText,
						languageCode,
						titleSourceLanguageCode,
						cancellationToken
					);
				}

				if (descriptionTargets.Contains(languageCode))
				{
					description = await _translationService.TranslateTextAsync
					(
						descriptionSourceText,
						languageCode,
						descriptionSourceLanguageCode,
						cancellationToken
					);
				}

				return new MediaTranslationEditDto
				{
					LanguageCode = languageCode,
					Title = title,
					Description = description
				};
			});

		return (await Task.WhenAll(translationTasks)).ToList();
	}

	private void UpdateTranslations(ICollection<MediaTranslation> source, ICollection<MediaTranslation> newTranslations)
	{
		foreach (var newTranslation in newTranslations)
		{
			if (source.FirstOrDefault(s => s.LanguageCode == newTranslation.LanguageCode) is MediaTranslation sourceTranslation)
			{
				sourceTranslation.Title = newTranslation.Title;
				sourceTranslation.Description = newTranslation.Description;
			}
			else
			{
				source.Add(newTranslation.Clone());
			}
		}
	}
}

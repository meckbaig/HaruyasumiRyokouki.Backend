using HaruyasumiRyokouki.Backend.Common.Abstractions;
using HaruyasumiRyokouki.Backend.Common.Options;
using HaruyasumiRyokouki.Backend.DbContexts;
using HaruyasumiRyokouki.Backend.Extensions;
using HaruyasumiRyokouki.Backend.Models.Db;
using HaruyasumiRyokouki.Backend.Models.Dtos.Media;
using HaruyasumiRyokouki.Backend.Services.Interfaces;
using HaruyasumiRyokouki.Backend.Services.Translation;
using HaruyasumiRyokouki.Backend.Services.Translation.Factories;
using Meckbaig.Cqrs.Abstractons;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json.Serialization;

namespace HaruyasumiRyokouki.Backend.Features.Media;

public record EditMediaCommand : IRequest<EditMediaResponse>, ILocalizableRequest
{
	[FromBody]
	public required BodyParameters Body { get; init; }

	[SwaggerIgnore]
	[JsonIgnore]
	public string? AcceptLanguage { get; set; }

	public record BodyParameters
	{
		public ICollection<int> Ids { get; set; } = [];
		public EditMediaChanges Changes { get; set; }
		public bool AutoTranslate { get; init; }
	}
}

public class EditMediaResponse : BaseResponse
{
	public required IEnumerable<MediaFileEditDto> Items { get; init; }
}

/// TODO
//internal class EditMediaValidator : AbstractValidator<EditMediaCommand>
//{
//	public EditMediaValidator()
//	{
//		RuleFor(x => x.Body)
//			.NotNull()
//			.SetValidator(new BodyParametersValidator());
//	}

//	internal class BodyParametersValidator : AbstractValidator<BodyParameters>
//	{
//		public BodyParametersValidator()
//		{
//			RuleFor(x => x.MediaFile)
//				.NotNull()
//				.SetValidator(new DayEditDto.Validator());
//		}
//	}
//}

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

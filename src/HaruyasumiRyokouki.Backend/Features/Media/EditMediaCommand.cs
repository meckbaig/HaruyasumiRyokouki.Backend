using HaruyasumiRyokouki.Backend.DbContexts;
using HaruyasumiRyokouki.Backend.Extensions;
using HaruyasumiRyokouki.Backend.Features.Translation;
using HaruyasumiRyokouki.Backend.Models.Db;
using HaruyasumiRyokouki.Backend.Models.Db.Enums;
using HaruyasumiRyokouki.Backend.Models.Dtos;
using Meckbaig.Cqrs.Abstractons;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HaruyasumiRyokouki.Backend.Features.Media;

public record EditMediaCommand : IRequest<EditMediaResponse>
{
	[FromBody]
	public required BodyParameters Body { get; init; }

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
	private readonly IMediator _mediator;

	public EditMediaHandler(IAppDbContext context, IMediator mediator)
	{
		_context = context;
		_mediator = mediator;
	}

	public async Task<EditMediaResponse> Handle(EditMediaCommand request, CancellationToken cancellationToken)
	{
		var mediaToEdit = await _context.MediaFiles
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

		return new EditMediaResponse { Items = mediaToEdit.ToEditDtos() };
	}

	private static readonly ICollection<LanguagePriority> _priorities =
	[
		new LanguagePriority(LanguageCode.English, nameof(LanguageCode.English), 1),
		new LanguagePriority(LanguageCode.Russian, nameof(LanguageCode.Russian), 2),
		new LanguagePriority(LanguageCode.Japanese, nameof(LanguageCode.Japanese), 3),
	];

	private async Task<ICollection<MediaTranslationEditDto>> TranslateMedia(ICollection<MediaTranslationEditDto> mediaTranslations, CancellationToken cancellationToken)
	{
		var existingTranslations = mediaTranslations
			.Where(t => !string.IsNullOrWhiteSpace(t.Title) && t.Tags.Any())
			.ToList();

		var source = _priorities
			.OrderBy(x => x.Priority)
			.Join(
				existingTranslations,
				p => p.LanguageCode,
				n => n.LanguageCode,
				(p, n) => n)
			.FirstOrDefault();

		if (source == null)
			return mediaTranslations;

		var missingLanguages = _priorities
			.Where(p => !existingTranslations.Any(t => t.LanguageCode == p.LanguageCode))
			.ToList();

		var translationTasks = missingLanguages.Select(async missingLanguage =>
		{
			var command = new GenerateMediaDescriptionTranslationCommand
			{
				Title = source.Title,
				Description = source.Description,
				Tags = string.Join(',', source.Tags),
				TargetLanguage = missingLanguage.LanguageName
			};
			var translationResponse = await _mediator.Send(command, cancellationToken);
			return new MediaTranslationEditDto
			{
				Title = translationResponse.Title,
				Description = translationResponse.Description,
				Tags = translationResponse.Tags.Split(',').Select(s => s.Trim()).ToList(),
				LanguageCode = missingLanguage.LanguageCode
			};
		});

		var translatedMedia = await Task.WhenAll(translationTasks);
		return translatedMedia.ToList();
	}

	private record LanguagePriority(string LanguageCode, string LanguageName, int Priority);

	private void UpdateTranslations(ICollection<MediaTranslation> source, ICollection<MediaTranslation> newTranslations)
	{
		foreach (var newTranslation in newTranslations)
		{
			if (source.FirstOrDefault(s => s.LanguageCode == newTranslation.LanguageCode) is MediaTranslation sourceTranslation)
			{
				sourceTranslation.Title = newTranslation.Title;
				sourceTranslation.Description = newTranslation.Description;
				sourceTranslation.Tags = newTranslation.Tags;
			}
			else
			{
				source.Add(newTranslation.Clone());
			}
		}
	}
}

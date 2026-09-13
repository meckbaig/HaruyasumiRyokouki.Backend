using HaruyasumiRyokouki.Backend.Common.Exceptions;
using HaruyasumiRyokouki.Backend.Common.Options;
using HaruyasumiRyokouki.Backend.DbContexts;
using HaruyasumiRyokouki.Backend.Extensions;
using HaruyasumiRyokouki.Backend.Models.Db;
using HaruyasumiRyokouki.Backend.Models.Dtos.Days;
using HaruyasumiRyokouki.Backend.Services.Interfaces;
using HaruyasumiRyokouki.Backend.Services.Translation;
using HaruyasumiRyokouki.Backend.Services.Translation.Factories;
using Meckbaig.Cqrs.Abstractons;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HaruyasumiRyokouki.Backend.Features.Days;

public record EditDayCommand : IRequest<EditDayResponse>
{
	[FromRoute]
	public required DateOnly Date { get; set; }

	[FromBody]
	public required BodyParameters Body { get; init; }

	public record BodyParameters
	{
		public required DayEditDto Day { get; init; }
		public bool AutoTranslate { get; init; }
	}
}

public class EditDayResponse : BaseResponse
{
	public required DayEditDto Day { get; init; }
}

/// TODO
//internal class EditDayValidator : AbstractValidator<EditDayCommand>
//{
//	public EditDayValidator()
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

internal class EditDayHandler : IRequestHandler<EditDayCommand, EditDayResponse>
{
	private readonly IAppDbContext _context;
	private readonly IContentTranslationService _translationService;

	public EditDayHandler
	(
		IAppDbContext context,
		ITranslationServiceOptionsAccessor translationAccessor,
		IContentTranslationServiceFactory factory,
		IOptions<TranslationProviderOptions> translationOptions
	)
	{
		_context = context;
		_translationService = factory.CreateTranslationService(translationAccessor.GetTranslationServiceOptions(translationOptions.Value.Usage.Days));
	}

	public async Task<EditDayResponse> Handle(EditDayCommand request, CancellationToken cancellationToken)
	{
		var day = await _context.Days
			.Include(d => d.Translations)
			.FirstOrDefaultAsync(d => d.Date == request.Date, cancellationToken);
		if (day == null)
			throw new EntityNotFoundException($"{request.Date} not found in the system.");

		day = day.FromEditDto(request.Body.Day);
		await _context.SaveChangesAsync(cancellationToken);

		if (request.Body.AutoTranslate)
			day.Translations = await TranslateNoteAsync(day, cancellationToken);

		return new EditDayResponse { Day = day.ToEditDto() };
	}

	private async Task<ICollection<DayTranslation>> TranslateNoteAsync(Day day, CancellationToken cancellationToken)
	{
		var translationTargets = TranslationPlanner.Plan
		(
			day.Translations.ToDictionary(x => x.LanguageCode, x => x.Note),
			out string sourceText,
			out string sourceLanguageCode
		);

		var translationTasks = translationTargets.Select(async languageCode =>
		{
			var translation = await _translationService.TranslateTextAsync
			(
				sourceText,
				languageCode,
				sourceLanguageCode,
				cancellationToken
			);
			return new DayTranslation
			{
				Note = translation,
				LanguageCode = languageCode
			};
		});

		var translatedNotes = await Task.WhenAll(translationTasks);
		return day.Translations
			.Where(x => !translationTargets.Contains(x.LanguageCode))
			.Concat(translatedNotes)
			.ToList();
	}

	private record LanguagePriority(string LanguageCode, string LanguageName, int Priority);
}

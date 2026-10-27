using FluentValidation;
using HaruyasumiRyokouki.Backend.Common.Exceptions;
using HaruyasumiRyokouki.Backend.Common.Helpers;
using HaruyasumiRyokouki.Backend.Common.Options;
using HaruyasumiRyokouki.Backend.DbContexts;
using HaruyasumiRyokouki.Backend.Extensions;
using HaruyasumiRyokouki.Backend.Models.Db;
using HaruyasumiRyokouki.Backend.Models.Db.Enums;
using HaruyasumiRyokouki.Backend.Models.Dtos.Days;
using HaruyasumiRyokouki.Backend.Services.Interfaces;
using HaruyasumiRyokouki.Backend.Services.Translation.Factories;
using Meckbaig.Cqrs.Abstractons;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HaruyasumiRyokouki.Backend.Features.Days;

/// <summary>
/// Request of <c>PUT /v1/days/{date}</c>.
/// </summary>
public record EditDayCommand : IRequest<EditDayResponse>
{
	/// <summary>
	/// Date of the day to edit.
	/// </summary>
	[FromRoute]
	public required DateOnly Date { get; set; }

	/// <summary>
	/// Request body.
	/// </summary>
	[FromBody]
	public required BodyParameters Body { get; init; }

	/// <summary>
	/// Body of <see cref="EditDayCommand"/>.
	/// </summary>
	public record BodyParameters
	{
		/// <summary>
		/// New ready flag and notes. Translations replace the stored ones; <c>Date</c> is ignored.
		/// </summary>
		public required DayEditDto Day { get; init; }

		/// <summary>
		/// Translate the notes into the missing languages and return them without saving.
		/// </summary>
		public bool AutoTranslate { get; init; }
	}
}

/// <summary>
/// Response of <c>PUT /v1/days/{date}</c>.
/// </summary>
public class EditDayResponse : BaseResponse
{
	/// <summary>
	/// The saved day; with auto-translation, the translated notes are included but not saved.
	/// </summary>
	public required DayEditDto Day { get; init; }
}

internal class EditDayValidator : AbstractValidator<EditDayCommand>
{
	public EditDayValidator()
	{
		RuleFor(x => x.Body)
			.NotNull()
			.SetValidator(new BodyParametersValidator());
	}

	internal class BodyParametersValidator : AbstractValidator<EditDayCommand.BodyParameters>
	{
		public BodyParametersValidator()
		{
			RuleFor(x => x.Day)
				.NotNull()
				.SetValidator(new DayEditDtoValidator());
		}
	}

	internal class DayEditDtoValidator : AbstractValidator<DayEditDto>
	{
		public DayEditDtoValidator()
		{
			RuleFor(x => x.Translations)
				.Cascade(CascadeMode.Stop)
				.NotNull()
				.Must(t => t.DistinctBy(x => x.LanguageCode).Count() == t.Count)
				.WithMessage("Translations must have at most one variation for each language.");
			RuleForEach(x => x.Translations)
				.ChildRules(translation =>
				{
					translation.RuleFor(t => t.LanguageCode)
						.Must(LanguageCode.IsSupported)
						.WithMessage($"Language code must be one of: {string.Join(", ", LanguageCode.Supported)}.");
					translation.RuleFor(t => t.Note)
						.NotNull();
				});
		}
	}
}

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

}

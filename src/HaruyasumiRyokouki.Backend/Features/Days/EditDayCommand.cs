using HaruyasumiRyokouki.Backend.Common.Exceptions;
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
	private readonly IMediator _mediator;

	public EditDayHandler(IAppDbContext context, IMediator mediator)
	{
		_context = context;
		_mediator = mediator;
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
			day.Translations = await TranslateNote(day);

		return new EditDayResponse { Day = day.ToEditDto() };
	}

	private static readonly ICollection<LanguagePriority> _priorities =
	[
		new LanguagePriority(LanguageCode.English, nameof(LanguageCode.English), 1),
		new LanguagePriority(LanguageCode.Russian, nameof(LanguageCode.Russian), 2),
		new LanguagePriority(LanguageCode.Japanese, nameof(LanguageCode.Japanese), 3),
	];

	private async Task<ICollection<DayTranslation>> TranslateNote(Day day)
	{
		var existingTranslations = day.Translations
			.Where(x => !string.IsNullOrWhiteSpace(x.Note))
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
			return day.Translations;

		var missingLanguages = _priorities
			.Where(p => !existingTranslations.Any(t => t.LanguageCode == p.LanguageCode))
			.ToList();

		var translationTasks = missingLanguages.Select(async missingLanguage =>
		{
			var command = new GenerateTextTranslationCommand
			{
				InputText = source.Note,
				TargetLanguage = missingLanguage.LanguageName
			};
			var translationResponse = await _mediator.Send(command);
			return new DayTranslation
			{
				Note = translationResponse.Result,
				LanguageCode = missingLanguage.LanguageCode
			};
		});

		var translatedNotes = await Task.WhenAll(translationTasks);
		return existingTranslations.Concat(translatedNotes).ToList();
	}

	private record LanguagePriority(string LanguageCode, string LanguageName, int Priority);
}

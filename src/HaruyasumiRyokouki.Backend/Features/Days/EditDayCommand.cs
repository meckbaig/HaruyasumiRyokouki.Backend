using HaruyasumiRyokouki.Backend.Common.Exceptions;
using HaruyasumiRyokouki.Backend.DbContexts;
using HaruyasumiRyokouki.Backend.Extensions;
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
	}
}

public class EditDayResponse : BaseResponse
{
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

	public EditDayHandler(IAppDbContext context)
	{
		_context = context;
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

		return new EditDayResponse();
	}
}

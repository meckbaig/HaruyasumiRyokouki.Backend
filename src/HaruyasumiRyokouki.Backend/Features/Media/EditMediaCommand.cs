using HaruyasumiRyokouki.Backend.DbContexts;
using HaruyasumiRyokouki.Backend.Extensions;
using HaruyasumiRyokouki.Backend.Models.Dtos;
using Meckbaig.Cqrs.Abstractons;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HaruyasumiRyokouki.Backend.Features.Days;

public record EditMediaCommand : IRequest<EditMediaResponse>
{
	[FromBody]
	public required BodyParameters Body { get; init; }

	public record BodyParameters
	{
		public ICollection<Guid> Ids { get; set; } = [];
		public EditMediaChanges Changes { get; set; }
	}
}

public class EditMediaResponse : BaseResponse
{
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

	public EditMediaHandler(IAppDbContext context)
	{
		_context = context;
	}

	public async Task<EditMediaResponse> Handle(EditMediaCommand request, CancellationToken cancellationToken)
	{
		var mediaToEdit = await _context.MediaFiles
			.Where(m => request.Body.Ids.Contains(m.Id))
			.ToListAsync(cancellationToken);

		if (request.Body.Changes.Latitude.HasValue)
			mediaToEdit.ForEach(m => m.Latitude = request.Body.Changes.Latitude);
		if (request.Body.Changes.Longitude.HasValue)
			mediaToEdit.ForEach(m => m.Longitude = request.Body.Changes.Longitude);
		if (request.Body.Changes.IsApproved.HasValue)
			mediaToEdit.ForEach(m => m.IsApproved = request.Body.Changes.IsApproved);
		if (request.Body.Changes.Translations.HasValue)
			mediaToEdit.ForEach(m => m.Translations = request.Body.Changes.Translations.Value!.FromEditDtos().ToList());

		await _context.SaveChangesAsync(cancellationToken);

		return new EditMediaResponse();
	}
}

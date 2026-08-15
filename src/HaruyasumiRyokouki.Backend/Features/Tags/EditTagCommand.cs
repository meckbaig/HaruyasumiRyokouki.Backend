using FluentValidation;
using HaruyasumiRyokouki.Backend.Common.Exceptions;
using HaruyasumiRyokouki.Backend.DbContexts;
using HaruyasumiRyokouki.Backend.Extensions;
using HaruyasumiRyokouki.Backend.Models.Db;
using HaruyasumiRyokouki.Backend.Models.Dtos.Tags;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static HaruyasumiRyokouki.Backend.Features.Tags.EditTagCommand;

namespace HaruyasumiRyokouki.Backend.Features.Tags;

public record EditTagCommand : IRequest<EditTagResponse>
{
	[FromRoute]
	public required int Id { get; set; }

	[FromBody]
	public required BodyParameters Body { get; init; }

	public record BodyParameters
	{
		public required EditTagDto Tag { get; init; }
	}
}

internal class EditTagValidator : AbstractValidator<EditTagCommand>
{
	public EditTagValidator()
	{
		RuleFor(x => x.Id)
			.GreaterThan(0);
		RuleFor(x => x.Body)
			.NotNull()
			.SetValidator(new BodyParametersValidator());
	}

	internal class BodyParametersValidator : AbstractValidator<BodyParameters>
	{
		public BodyParametersValidator()
		{
			RuleFor(x => x.Tag)
				.NotNull()
				.SetValidator(new EditTagDto.Validator());
		}
	}
}

public class EditTagResponse
{
	public required TagDto Tag { get; set; }
}

internal class EditTagQueryHandler : IRequestHandler<EditTagCommand, EditTagResponse>
{
	private readonly IAppDbContext _context;

	public EditTagQueryHandler(IAppDbContext context)
	{
		_context = context;
	}

	public async Task<EditTagResponse> Handle(EditTagCommand request, CancellationToken cancellationToken)
	{
		var tagToEdit = await _context.Tags
			.Include(t => t.Translations)
			.Include(t => t.MediaTags)
			.FirstOrDefaultAsync(x => x.Id  == request.Id, cancellationToken)
			?? throw new EntityNotFoundException($"Tag with Id {request.Id} not found.");

		if (request.Body.Tag.Slug.HasValue)
			tagToEdit.Slug = request.Body.Tag.Slug.Value;
		if (request.Body.Tag.Translations.HasValue)
		{
			tagToEdit.Translations = tagToEdit.Translations.Except(tagToEdit.Translations.Primary()).ToList();
			tagToEdit.Translations = tagToEdit.Translations.Concat(request.Body.Tag.Translations.Value.Select(tt => new TagTranslation
			{
				IsPrimary = true,
				Text = tt.Text,
				LanguageCode = tt.LanguageCode
			})).ToList();
		}
		if (request.Body.Tag.Aliases.HasValue)
		{
			tagToEdit.Translations = tagToEdit.Translations.Except(tagToEdit.Translations.Aliases()).ToList();
			tagToEdit.Translations = tagToEdit.Translations.Concat(request.Body.Tag.Aliases.Value.Select(tt => new TagTranslation
			{
				IsPrimary = false,
				Text = tt.Text,
				LanguageCode = tt.LanguageCode
			})).ToList();
		}

		await _context.SaveChangesAsync(cancellationToken);

		return new EditTagResponse
		{
			Tag = tagToEdit.ToDto()
		};
	}
}


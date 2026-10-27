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

/// <summary>
/// Request of <c>PATCH /v1/tags/{id}</c>.
/// </summary>
public record EditTagCommand : IRequest<EditTagResponse>
{
	/// <summary>
	/// Id of the tag to edit.
	/// </summary>
	[FromRoute]
	public required int Id { get; set; }

	/// <summary>
	/// Request body.
	/// </summary>
	[FromBody]
	public required BodyParameters Body { get; init; }

	/// <summary>
	/// Body of <see cref="EditTagCommand"/>.
	/// </summary>
	public record BodyParameters
	{
		/// <summary>
		/// Changes of the tag. Absent fields are not modified.
		/// </summary>
		public required EditTagDto Tag { get; init; }
	}
}

internal class EditTagValidator : AbstractValidator<EditTagCommand>
{
	public EditTagValidator(IAppDbContext context)
	{
		RuleFor(x => x.Id)
			.GreaterThan(0);
		RuleFor(x => x.Body)
			.NotNull()
			.SetValidator(new BodyParametersValidator());
		RuleFor(x => x.Body.Tag.Slug.Value)
			.MustAsync(async (command, slug, cancellationToken) =>
				!await context.Tags.AnyAsync(t => t.Slug == slug && t.Id != command.Id, cancellationToken))
			.WithMessage("Tag with this slug already exists.")
			.OverridePropertyName("Body.Tag.Slug")
			.When(x => x.Body?.Tag != null && x.Body.Tag.Slug.HasValue && !string.IsNullOrWhiteSpace(x.Body.Tag.Slug.Value));
		RuleFor(x => x)
			.CustomAsync(async (command, validationContext, cancellationToken) =>
			{
				var duplicates = await FindDuplicateTextsAsync(context, command, cancellationToken);
				if (duplicates.Count > 0)
					validationContext.AddFailure("Body.Tag", $"Texts must be unique within a tag, duplicated: {string.Join(", ", duplicates)}.");
			})
			.When(x => x.Body?.Tag != null && (x.Body.Tag.Translations.HasValue || x.Body.Tag.Aliases.HasValue));
	}

	/// <summary>
	/// Texts used more than once among the tag translations and aliases after the edit
	/// (unique per tag in the database, regardless of language). A list that is not edited is taken from the database.
	/// </summary>
	private static async Task<List<string>> FindDuplicateTextsAsync(IAppDbContext context, EditTagCommand command, CancellationToken cancellationToken)
	{
		var tag = command.Body.Tag;
		var existing = await context.Tags
			.AsNoTracking()
			.Where(t => t.Id == command.Id)
			.SelectMany(t => t.Translations)
			.Select(t => new { t.Text, t.IsPrimary })
			.ToListAsync(cancellationToken);

		var translations = tag.Translations.HasValue
			? (tag.Translations.Value ?? []).Select(t => t.Text)
			: existing.Where(t => t.IsPrimary).Select(t => t.Text);
		var aliases = tag.Aliases.HasValue
			? (tag.Aliases.Value ?? []).Select(t => t.Text)
			: existing.Where(t => !t.IsPrimary).Select(t => t.Text);

		return translations.Concat(aliases)
			.GroupBy(t => t)
			.Where(g => g.Count() > 1)
			.Select(g => g.Key)
			.ToList();
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

/// <summary>
/// Response of <c>PATCH /v1/tags/{id}</c>.
/// </summary>
public class EditTagResponse
{
	/// <summary>
	/// The edited tag.
	/// </summary>
	public required TagDto Tag { get; set; }
}

internal class EditTagHandler : IRequestHandler<EditTagCommand, EditTagResponse>
{
	private readonly IAppDbContext _context;

	public EditTagHandler(IAppDbContext context)
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


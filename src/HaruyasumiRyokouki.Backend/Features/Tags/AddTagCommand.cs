using FluentValidation;
using HaruyasumiRyokouki.Backend.DbContexts;
using HaruyasumiRyokouki.Backend.Extensions;
using HaruyasumiRyokouki.Backend.Models.Dtos.Tags;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static HaruyasumiRyokouki.Backend.Features.Tags.AddTagCommand;

namespace HaruyasumiRyokouki.Backend.Features.Tags;

/// <summary>
/// Request of <c>POST /v1/tags</c>.
/// </summary>
public record AddTagCommand : IRequest<AddTagResponse>
{
	/// <summary>
	/// Request body.
	/// </summary>
	[FromBody]
	public required BodyParameters Body { get; init; }

	/// <summary>
	/// Body of <see cref="AddTagCommand"/>.
	/// </summary>
	public record BodyParameters
	{
		/// <summary>
		/// The tag to create.
		/// </summary>
		public required CreateTagDto Tag { get; init; }
	}
}

internal class AddTagValidator : AbstractValidator<AddTagCommand>
{
	public AddTagValidator(IAppDbContext context)
	{
		RuleFor(x => x.Body)
			.NotNull()
			.SetValidator(new BodyParametersValidator());
		RuleFor(x => x.Body.Tag.Slug)
			.MustAsync(async (slug, cancellationToken) => !await context.Tags.AnyAsync(t => t.Slug == slug, cancellationToken))
			.WithMessage("Tag with this slug already exists.")
			.When(x => !string.IsNullOrWhiteSpace(x.Body?.Tag?.Slug));
	}

	internal class BodyParametersValidator : AbstractValidator<BodyParameters>
	{
		public BodyParametersValidator()
		{
			RuleFor(x => x.Tag)
				.NotNull()
				.SetValidator(new CreateTagDto.Validator());
		}
	}
}

/// <summary>
/// Response of <c>POST /v1/tags</c>.
/// </summary>
public class AddTagResponse
{
	/// <summary>
	/// The created tag.
	/// </summary>
	public required TagDto Tag { get; set; }
}

internal class AddTagHandler : IRequestHandler<AddTagCommand, AddTagResponse>
{
	private readonly IAppDbContext _context;

	public AddTagHandler(IAppDbContext context)
	{
		_context = context;
	}

	public async Task<AddTagResponse> Handle(AddTagCommand request, CancellationToken cancellationToken)
	{
		var newTag = request.Body.Tag.FromDto();
		_context.Tags.Add(newTag);
		await _context.SaveChangesAsync(cancellationToken);

		return new AddTagResponse
		{
			Tag = newTag.ToDto()
		};
	}
}


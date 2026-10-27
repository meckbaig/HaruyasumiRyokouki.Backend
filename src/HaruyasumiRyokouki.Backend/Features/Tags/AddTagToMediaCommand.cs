using FluentValidation;
using HaruyasumiRyokouki.Backend.Common.Exceptions;
using HaruyasumiRyokouki.Backend.DbContexts;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HaruyasumiRyokouki.Backend.Features.Tags;

/// <summary>
/// Request of <c>POST /v1/tags/{id}/media</c>.
/// </summary>
public record AddTagToMediaCommand : IRequest<AddTagToMediaResponse>
{
	/// <summary>
	/// Identifier of the tag to add.
	/// </summary>
	[FromRoute]
	public required int Id { get; init; }

	/// <summary>
	/// Request body.
	/// </summary>
	[FromBody]
	public required BodyParameters Body { get; init; }

	/// <summary>
	/// Body of <see cref="AddTagToMediaCommand"/>.
	/// </summary>
	public record BodyParameters
	{
		/// <summary>
		/// Media files to tag.
		/// </summary>
		/// <remarks>
		/// Their other tags are kept.
		/// </remarks>
		public required ICollection<int> MediaFileIds { get; init; }
	}
}

/// <summary>
/// Response of <c>POST /v1/tags/{id}/media</c>.
/// </summary>
public class AddTagToMediaResponse
{
	/// <summary>
	/// Number of media files that did not have the tag before.
	/// </summary>
	public int Affected { get; set; }
}

internal class AddTagToMediaValidator : AbstractValidator<AddTagToMediaCommand>
{
	public AddTagToMediaValidator()
	{
		RuleFor(x => x.Id)
			.GreaterThan(0);
		RuleFor(x => x.Body)
			.NotNull()
			.SetValidator(new BodyParametersValidator());
	}

	internal class BodyParametersValidator : AbstractValidator<AddTagToMediaCommand.BodyParameters>
	{
		public BodyParametersValidator()
		{
			RuleFor(x => x.MediaFileIds)
				.NotEmpty();
		}
	}
}

internal class AddTagToMediaHandler : IRequestHandler<AddTagToMediaCommand, AddTagToMediaResponse>
{
	private readonly IAppDbContext _context;

	public AddTagToMediaHandler(IAppDbContext context)
	{
		_context = context;
	}

	public async Task<AddTagToMediaResponse> Handle(AddTagToMediaCommand request, CancellationToken cancellationToken)
	{
		var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken)
			?? throw new EntityNotFoundException($"Tag with Id {request.Id} not found.");

		var media = await _context.MediaFiles
			.Include(m => m.Tags)
			.Where(m => request.Body.MediaFileIds.Contains(m.Id))
			.ToListAsync(cancellationToken);

		EntityNotFoundException.ThrowIfAnyMissing(request.Body.MediaFileIds, media.Select(m => m.Id), "Media file");

		int affected = 0;
		foreach (var file in media)
		{
			if (file.Tags.Any(t => t.Id == tag.Id))
				continue;

			file.Tags.Add(tag);
			affected++;
		}

		await _context.SaveChangesAsync(cancellationToken);

		return new AddTagToMediaResponse { Affected = affected };
	}
}

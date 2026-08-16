using HaruyasumiRyokouki.Backend.Common.Exceptions;
using HaruyasumiRyokouki.Backend.DbContexts;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HaruyasumiRyokouki.Backend.Features.Tags;

public record AddTagToMediaCommand : IRequest<AddTagToMediaResponse>
{
	[FromRoute]
	public required int Id { get; init; }

	[FromBody]
	public required BodyParameters Body { get; init; }

	public record BodyParameters
	{
		public required ICollection<int> MediaFileIds { get; init; }
	}
}

public class AddTagToMediaResponse
{
	public int Affected { get; set; }
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

using HaruyasumiRyokouki.Backend.Common.Exceptions;
using HaruyasumiRyokouki.Backend.DbContexts;
using HaruyasumiRyokouki.Backend.Services.Interfaces;
using Meckbaig.Cqrs.Abstractons;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HaruyasumiRyokouki.Backend.Features.Media;

public record DeleteMediaCommand : IRequest<DeleteMediaResponse>
{
	[FromRoute]
	public required int MediaId { get; init; }
}

public class DeleteMediaResponse : BaseResponse
{
}

internal class DeleteMediaHandler : IRequestHandler<DeleteMediaCommand, DeleteMediaResponse>
{
	private readonly IAppDbContext _context;
	private readonly IRemovalQueueService _removalQueue;

	public DeleteMediaHandler(IAppDbContext context, IRemovalQueueService removalQueue)
	{
		_context = context;
		_removalQueue = removalQueue;
	}

	public async Task<DeleteMediaResponse> Handle(DeleteMediaCommand request, CancellationToken cancellationToken)
	{
		var mediaToDelete = await _context.MediaFiles
			.FirstOrDefaultAsync(m => m.Id == request.MediaId, cancellationToken)
				?? throw new EntityNotFoundException($"Media file with Id {request.MediaId} not found.");

		_context.MediaFiles.Remove(mediaToDelete);
		await _context.SaveChangesAsync(cancellationToken);

		await CreateRemovalQueueAsync(mediaToDelete, cancellationToken);

		return new DeleteMediaResponse();
	}

	private async Task CreateRemovalQueueAsync(Models.Db.MediaFile mediaToDelete, CancellationToken cancellationToken)
	{
		await _removalQueue.AddAsync(mediaToDelete.FileName);
		if (mediaToDelete.AdditionalFiles.Count > 0)
		{
			foreach (string fileName in mediaToDelete.AdditionalFiles)
			{
				await _removalQueue.AddAsync(fileName);
			}
		}
	}
}

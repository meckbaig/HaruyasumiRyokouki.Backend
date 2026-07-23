using HaruyasumiRyokouki.Backend.Common.Exceptions;
using HaruyasumiRyokouki.Backend.DbContexts;
using HaruyasumiRyokouki.Backend.Services.Interfaces;
using Meckbaig.Cqrs.Abstractons;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HaruyasumiRyokouki.Backend.Features.Days;

public record DeleteMediaCommand : IRequest<DeleteMediaResponse>
{
	[FromRoute]
	public required Guid MediaId { get; init; }
}

public class DeleteMediaResponse : BaseResponse
{
}

internal class DeleteMediaHandler : IRequestHandler<DeleteMediaCommand, DeleteMediaResponse>
{
	private readonly IAppDbContext _context;
	private readonly IFileStorage _fileStorage;

	public DeleteMediaHandler(IAppDbContext context)
	{
		_context = context;
	}

	public async Task<DeleteMediaResponse> Handle(DeleteMediaCommand request, CancellationToken cancellationToken)
	{
		var mediaToDelete = await _context.MediaFiles
			.FirstOrDefaultAsync(m => m.Id == request.MediaId, cancellationToken);

		if (mediaToDelete == null)
			throw new EntityNotFoundException($"Media file with Id {request.MediaId} not found.");

		_context.MediaFiles.Remove(mediaToDelete);

		await _fileStorage.DeleteAsync(mediaToDelete.FileName, cancellationToken);

		await _context.SaveChangesAsync(cancellationToken);

		return new DeleteMediaResponse();
	}
}

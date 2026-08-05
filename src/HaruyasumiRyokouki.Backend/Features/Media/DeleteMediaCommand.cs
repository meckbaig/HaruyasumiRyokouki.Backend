using HaruyasumiRyokouki.Backend.Common.Exceptions;
using HaruyasumiRyokouki.Backend.DbContexts;
using HaruyasumiRyokouki.Backend.Models.Db.Enums;
using HaruyasumiRyokouki.Backend.Services.Interfaces;
using Meckbaig.Cqrs.Abstractons;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HaruyasumiRyokouki.Backend.Features.Days;

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
	private readonly IFileStorage _fileStorage;
	private readonly IMediaProcessorService _mediaProcessor;

	public DeleteMediaHandler(IAppDbContext context, IFileStorage fileStorage, IMediaProcessorService mediaProcessor)
	{
		_context = context;
		_fileStorage = fileStorage;
		_mediaProcessor = mediaProcessor;
	}

	public async Task<DeleteMediaResponse> Handle(DeleteMediaCommand request, CancellationToken cancellationToken)
	{
		var mediaToDelete = await _context.MediaFiles
			.FirstOrDefaultAsync(m => m.Id == request.MediaId, cancellationToken);

		if (mediaToDelete == null)
			throw new EntityNotFoundException($"Media file with Id {request.MediaId} not found.");

		_context.MediaFiles.Remove(mediaToDelete);

		await _fileStorage.DeleteAsync(mediaToDelete.FileName, cancellationToken);
		if (mediaToDelete.Type == MediaType.Video)
		{
			await _fileStorage.DeleteAsync(_mediaProcessor.GetVideoWebName(mediaToDelete.FileName), cancellationToken);
			await _fileStorage.DeleteAsync(_mediaProcessor.GetVideoPreviewName(mediaToDelete.FileName), cancellationToken);
		}

		await _context.SaveChangesAsync(cancellationToken);

		return new DeleteMediaResponse();
	}
}

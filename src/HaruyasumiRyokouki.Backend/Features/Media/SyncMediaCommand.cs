using HaruyasumiRyokouki.Backend.Services.Interfaces;
using Meckbaig.Cqrs.Abstractons;
using MediatR;

namespace HaruyasumiRyokouki.Backend.Features.Media;

public record SyncMediaCommand : IRequest<SyncMediaResponse>
{
}

public class SyncMediaResponse : BaseResponse
{
}

internal class SyncMediaHandler : IRequestHandler<SyncMediaCommand, SyncMediaResponse>
{
	private readonly IMediaSyncService _mediaSyncService;

	public SyncMediaHandler(IMediaSyncService mediaSyncService)
	{
		_mediaSyncService = mediaSyncService;
	}

	public async Task<SyncMediaResponse> Handle(SyncMediaCommand request, CancellationToken cancellationToken)
	{
		await _mediaSyncService.SyncAsync(cancellationToken);
		return new SyncMediaResponse();
	}
}

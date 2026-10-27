using HaruyasumiRyokouki.Backend.Services.Interfaces;
using Meckbaig.Cqrs.Abstractons;
using MediatR;

namespace HaruyasumiRyokouki.Backend.Features.Media;

/// <summary>
/// Request of <c>PUT /v1/media/sync</c>.
/// </summary>
public record SyncMediaCommand : IRequest<SyncMediaResponse>
{
}

/// <summary>
/// Empty response; the endpoint returns <c>200</c> without a body.
/// </summary>
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

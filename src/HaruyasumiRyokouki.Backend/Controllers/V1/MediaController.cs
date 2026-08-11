using Asp.Versioning;
using HaruyasumiRyokouki.Backend.Extensions;
using HaruyasumiRyokouki.Backend.Features.Media;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;

namespace HaruyasumiRyokouki.Backend.Controllers.V1;

[ApiController]
[Route("v{version:ApiVersion}/[controller]")]
[ApiVersion(1)]
[SwaggerTag("Manages media parameters.")]
[Produces(MediaTypeNames.Application.Json)]
public class MediaController : ControllerBase
{
	private readonly IMediator _mediator;

	/// <summary>
	/// Constructor with parameters for DI.
	/// </summary>
	public MediaController(IMediator mediator)
	{
		_mediator = mediator;
	}

	[Authorize]
	[HttpDelete("{mediaId}")]
	public async Task<IActionResult> DeleteMedia(DeleteMediaCommand command, CancellationToken cancellationToken)
	{
		var result = await _mediator.Send(command, cancellationToken);
		return Ok();
	}

	[Authorize]
	[HttpPatch]
	public async Task<ActionResult<EditMediaResponse>> EditMedia(EditMediaCommand command, CancellationToken cancellationToken)
	{
		var result = await _mediator.Send(command, cancellationToken);
		return result.ToJsonResponse();
	}

	[Authorize]
	[HttpPut("sync")]
	public async Task<IActionResult> SyncMedia([FromQuery] SyncMediaCommand command, CancellationToken cancellationToken)
	{
		var result = await _mediator.Send(command, cancellationToken);
		return Ok();
	}

	[Authorize]
	[HttpGet("edit")]
	public async Task<ActionResult<GetEditMediaResponse>> GetEditMedia(GetEditMediaQuery query, CancellationToken cancellationToken)
	{
		var result = await _mediator.Send(query, cancellationToken);
		return result.ToJsonResponse();
	}

	[HttpGet("locations")]
	public async Task<ActionResult<GetMediaLocationsResponse>> GetMediaLocations(GetMediaLocationsQuery query, CancellationToken cancellationToken)
	{
		var result = await _mediator.Send(query, cancellationToken);
		return result.ToJsonResponse();
	}	
}

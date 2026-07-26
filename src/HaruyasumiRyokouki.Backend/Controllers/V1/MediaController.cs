using Asp.Versioning;
using HaruyasumiRyokouki.Backend.Features.Days;
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
	public async Task<IActionResult> DeleteMedia(DeleteMediaCommand command)
	{
		var result = await _mediator.Send(command);
		return Ok();
	}

	[Authorize]
	[HttpPatch]
	public async Task<IActionResult> EditMedia(EditMediaCommand command)
	{
		var result = await _mediator.Send(command);
		return Ok();
	}

	[Authorize]
	[HttpPut("sync")]
	public async Task<IActionResult> SyncMedia()
	{
		SyncMediaCommand command = new();
		var result = await _mediator.Send(command);
		return Ok();
	}
}

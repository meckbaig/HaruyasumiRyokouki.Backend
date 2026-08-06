using Asp.Versioning;
using HaruyasumiRyokouki.Backend.Extensions;
using HaruyasumiRyokouki.Backend.Features.Admin;
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
public class AdminController : ControllerBase
{
	private readonly IMediator _mediator;

	/// <summary>
	/// Constructor with parameters for DI.
	/// </summary>
	public AdminController(IMediator mediator)
	{
		_mediator = mediator;
	}

	[Authorize]
	[HttpGet("pending")]
	public async Task<ActionResult<GetPendingResponse>> GetList([FromQuery] GetPendingQuery query, CancellationToken cancellationToken)
	{
		var result = await _mediator.Send(query, cancellationToken);
		return result.ToJsonResponse();
	}

	[Authorize]
	[HttpGet("login")]
	public async Task<IActionResult> Login()
	{
		return Ok();
	}
}


using Asp.Versioning;
using HaruyasumiRyokouki.Backend.Extensions;
using HaruyasumiRyokouki.Backend.Features.Admin;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;

namespace HaruyasumiRyokouki.Backend.Controllers.V1;

/// <summary>
/// Administrative operations.
/// </summary>
[ApiController]
[Route("v{version:ApiVersion}/[controller]")]
[ApiVersion(1)]
[SwaggerTag("Administrative operations: pending content and login check.")]
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

	/// <summary>
	/// Returns days that are not ready and media that are not approved.
	/// </summary>
	/// <response code="200">Pending days and media.</response>
	/// <response code="401">No or invalid admin credentials.</response>
	[Authorize]
	[HttpGet("pending")]
	public async Task<ActionResult<GetPendingResponse>> GetList([FromQuery] GetPendingQuery query, CancellationToken cancellationToken)
	{
		var result = await _mediator.Send(query, cancellationToken);
		return result.ToJsonResponse();
	}

	/// <summary>
	/// Checks the admin credentials.
	/// </summary>
	/// <response code="200">Credentials are valid.</response>
	/// <response code="401">No or invalid admin credentials.</response>
	/// <response code="429">Too many failed attempts from this IP address.</response>
	[Authorize]
	[HttpGet("login")]
	public async Task<IActionResult> Login()
	{
		return Ok();
	}
}


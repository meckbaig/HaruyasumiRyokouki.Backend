using Asp.Versioning;
using HaruyasumiRyokouki.Backend.Extensions;
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
public class DaysController : ControllerBase
{
	private readonly IMediator _mediator;

	/// <summary>
	/// Constructor with parameters for DI.
	/// </summary>
	public DaysController(IMediator mediator)
	{
		_mediator = mediator;
	}

	[Authorize]
	[HttpPut("{date}")]
	public async Task<ActionResult<EditDayResponse>> EditDay(EditDayCommand command, CancellationToken cancellationToken)
	{
		var result = await _mediator.Send(command, cancellationToken);
		return result.ToJsonResponse();
	}

	[HttpGet("{date}")]
	public async Task<ActionResult<GetDayResponse>> GetDay(GetDayQuery query, CancellationToken cancellationToken)
	{
		var result = await _mediator.Send(query, cancellationToken);
		return result.ToJsonResponse();
	}

	[HttpGet]
	public async Task<ActionResult<GetDaysResponse>> GetList(CancellationToken cancellationToken)
	{
		GetDaysQuery query = new();
		var result = await _mediator.Send(query, cancellationToken);
		return result.ToJsonResponse();
	}

	[Authorize]
	[HttpGet("{date}/edit")]
	public async Task<ActionResult<GetEditDayResponse>> GetEditDay(GetEditDayQuery query, CancellationToken cancellationToken)
	{
		var result = await _mediator.Send(query, cancellationToken);
		return result.ToJsonResponse();
	}
}

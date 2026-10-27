using Asp.Versioning;
using HaruyasumiRyokouki.Backend.Extensions;
using HaruyasumiRyokouki.Backend.Features.Days;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;

namespace HaruyasumiRyokouki.Backend.Controllers.V1;

/// <summary>
/// Travel days and their notes.
/// </summary>
[ApiController]
[Route("v{version:ApiVersion}/[controller]")]
[ApiVersion(1)]
[SwaggerTag("Manages travel days and their notes.")]
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

	/// <summary>
	/// Replaces the notes and the ready flag of a day.
	/// </summary>
	/// <remarks>
	/// The day is taken from the route. A language missing from <see cref="Models.Dtos.Days.DayEditDto.Translations"/> is deleted.
	/// With <see cref="EditDayCommand.BodyParameters.AutoTranslate"/>
	/// the missing languages are translated and returned without being saved.
	/// </remarks>
	/// <response code="200">The day with all translations.</response>
	/// <response code="400">Validation error.</response>
	/// <response code="401">No or invalid admin credentials.</response>
	/// <response code="404">Day not found.</response>
	[Authorize]
	[HttpPut("{date}")]
	public async Task<ActionResult<EditDayResponse>> EditDay(EditDayCommand command, CancellationToken cancellationToken)
	{
		var result = await _mediator.Send(command, cancellationToken);
		return result.ToJsonResponse();
	}

	/// <summary>
	/// Returns a day with its note and media in the requested language.
	/// </summary>
	/// <remarks>Media that are private or not approved are returned only to the admin.</remarks>
	/// <response code="200">The day.</response>
	/// <response code="404">Day not found.</response>
	[HttpGet("{date}")]
	public async Task<ActionResult<GetDayResponse>> GetDay(GetDayQuery query, CancellationToken cancellationToken)
	{
		var result = await _mediator.Send(query, cancellationToken);
		return result.ToJsonResponse();
	}

	/// <summary>
	/// Returns all days with the number of media.
	/// </summary>
	/// <remarks>For visitors only public approved media are counted.</remarks>
	/// <response code="200">Days ordered by date.</response>
	[HttpGet]
	public async Task<ActionResult<GetDaysResponse>> GetList([FromQuery] GetDaysQuery query, CancellationToken cancellationToken)
	{
		var result = await _mediator.Send(query, cancellationToken);
		return result.ToJsonResponse();
	}

	/// <summary>
	/// Returns a day with notes in all languages, for the editor.
	/// </summary>
	/// <response code="200">The day.</response>
	/// <response code="401">No or invalid admin credentials.</response>
	/// <response code="404">Day not found.</response>
	[Authorize]
	[HttpGet("{date}/edit")]
	public async Task<ActionResult<GetEditDayResponse>> GetEditDay(GetEditDayQuery query, CancellationToken cancellationToken)
	{
		var result = await _mediator.Send(query, cancellationToken);
		return result.ToJsonResponse();
	}
}

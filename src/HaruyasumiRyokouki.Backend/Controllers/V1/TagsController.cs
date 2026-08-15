using Asp.Versioning;
using HaruyasumiRyokouki.Backend.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;
using MediatR;
using HaruyasumiRyokouki.Backend.Features.Tags;

namespace HaruyasumiRyokouki.Backend.Controllers.V1;

[ApiController]
[Route("v{version:ApiVersion}/[controller]")]
[ApiVersion(1)]
[SwaggerTag("Manages media parameters.")]
[Produces(MediaTypeNames.Application.Json)]
public class TagsController : ControllerBase
{
	private readonly IMediator _mediator;

	/// <summary>
	/// Constructor with parameters for DI.
	/// </summary>
	public TagsController(IMediator mediator)
	{
		_mediator = mediator;
	}

	[HttpGet("suggestion")]
	public async Task<ActionResult<GetTagSuggestionResponse>> GetEditTags(GetTagSuggestionQuery query, CancellationToken cancellationToken)
	{
		var result = await _mediator.Send(query, cancellationToken);
		return result.ToJsonResponse();
	}

	[Authorize]
	[HttpGet]
	public async Task<ActionResult<GetTagsResponse>> GetTags([FromQuery] GetTagsQuery query, CancellationToken cancellationToken)
	{
		var result = await _mediator.Send(query, cancellationToken);
		return result.ToJsonResponse();
	}

	[Authorize]
	[HttpPost("completion")]
	public async Task<ActionResult<GetTagCompletionResponse>> GetTags(GetTagCompletionCommand command, CancellationToken cancellationToken)
	{
		var result = await _mediator.Send(command, cancellationToken);
		return result.ToJsonResponse();
	}

	[Authorize]
	[HttpPost]
	public async Task<ActionResult<AddTagResponse>> AddTag(AddTagCommand command, CancellationToken cancellationToken)
	{
		var result = await _mediator.Send(command, cancellationToken);
		return result.ToJsonResponse();
	}

	[Authorize]
	[HttpPatch("{id}")]
	public async Task<ActionResult<EditTagResponse>> EditTag(EditTagCommand command, CancellationToken cancellationToken)
	{
		var result = await _mediator.Send(command, cancellationToken);
		return result.ToJsonResponse();
	}	
}

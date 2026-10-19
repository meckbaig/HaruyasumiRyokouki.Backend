using Asp.Versioning;
using HaruyasumiRyokouki.Backend.Extensions;
using HaruyasumiRyokouki.Backend.Features.Tags;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;

namespace HaruyasumiRyokouki.Backend.Controllers.V1;

[ApiController]
[Route("v{version:ApiVersion}/[controller]")]
[ApiVersion(1)]
[SwaggerTag("Manages tags, tag suggestions and tagging of media.")]
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
	public async Task<ActionResult<GetTagSuggestionResponse>> GetTagSuggestion(GetTagSuggestionQuery query, CancellationToken cancellationToken)
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
	public async Task<ActionResult<GetTagCompletionResponse>> CompleteTag(GetTagCompletionCommand command, CancellationToken cancellationToken)
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

	[Authorize]
	[HttpGet("{id}/suggest")]
	public async Task<ActionResult<SuggestMediaForTagResponse>> GetSimilarTags(SuggestMediaForTagQuery query, CancellationToken cancellationToken)
	{
		var result = await _mediator.Send(query, cancellationToken);
		return result.ToJsonResponse();
	}

	[Authorize]
	[HttpPost("{id}/media")]
	public async Task<ActionResult<AddTagToMediaResponse>> AddTagToMedia(AddTagToMediaCommand command, CancellationToken cancellationToken)
	{
		var result = await _mediator.Send(command, cancellationToken);
		return result.ToJsonResponse();
	}	
}

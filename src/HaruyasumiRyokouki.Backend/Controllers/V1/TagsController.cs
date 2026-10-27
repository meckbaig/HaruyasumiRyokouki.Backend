using Asp.Versioning;
using HaruyasumiRyokouki.Backend.Extensions;
using HaruyasumiRyokouki.Backend.Features.Tags;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;

namespace HaruyasumiRyokouki.Backend.Controllers.V1;

/// <summary>
/// Tags, tag suggestions and tagging of media.
/// </summary>
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

	/// <summary>
	/// Returns tags having a translation or alias that contains the text, most used first.
	/// </summary>
	/// <response code="200">Tags in the requested language.</response>
	/// <response code="400">Validation error.</response>
	[HttpGet("suggestion")]
	public async Task<ActionResult<GetTagSuggestionResponse>> GetTagSuggestion(GetTagSuggestionQuery query, CancellationToken cancellationToken)
	{
		var result = await _mediator.Send(query, cancellationToken);
		return result.ToJsonResponse();
	}

	/// <summary>
	/// Returns all tags with all translations and aliases, most used first.
	/// </summary>
	/// <response code="200">Tags.</response>
	/// <response code="401">No or invalid admin credentials.</response>
	[Authorize]
	[HttpGet]
	public async Task<ActionResult<GetTagsResponse>> GetTags([FromQuery] GetTagsQuery query, CancellationToken cancellationToken)
	{
		var result = await _mediator.Send(query, cancellationToken);
		return result.ToJsonResponse();
	}

	/// <summary>
	/// Drafts a new tag with AI: translations and search aliases, plus existing tags containing the text.
	/// </summary>
	/// <remarks>Nothing is saved. The editor reviews the draft and creates the tag with <c>POST /v1/tags</c>.</remarks>
	/// <response code="200">Draft tag and similar existing tags.</response>
	/// <response code="400">Validation error.</response>
	/// <response code="401">No or invalid admin credentials.</response>
	[Authorize]
	[HttpPost("completion")]
	public async Task<ActionResult<GetTagCompletionResponse>> CompleteTag(GetTagCompletionCommand command, CancellationToken cancellationToken)
	{
		var result = await _mediator.Send(command, cancellationToken);
		return result.ToJsonResponse();
	}

	/// <summary>
	/// Creates a tag.
	/// </summary>
	/// <response code="200">Created tag.</response>
	/// <response code="400">Validation error, including a duplicate slug or a text used twice in the tag.</response>
	/// <response code="401">No or invalid admin credentials.</response>
	[Authorize]
	[HttpPost]
	public async Task<ActionResult<AddTagResponse>> AddTag(AddTagCommand command, CancellationToken cancellationToken)
	{
		var result = await _mediator.Send(command, cancellationToken);
		return result.ToJsonResponse();
	}

	/// <summary>
	/// Edits a tag. Fields absent from the body are not modified.
	/// </summary>
	/// <remarks><see cref="Models.Dtos.Tags.EditTagDto.Translations"/> replaces all display names, <see cref="Models.Dtos.Tags.EditTagDto.Aliases"/> replaces all aliases.</remarks>
	/// <response code="200">Edited tag.</response>
	/// <response code="400">Validation error, including a duplicate slug or a text used twice in the tag.</response>
	/// <response code="401">No or invalid admin credentials.</response>
	/// <response code="404">Tag not found.</response>
	[Authorize]
	[HttpPatch("{id}")]
	public async Task<ActionResult<EditTagResponse>> EditTag(EditTagCommand command, CancellationToken cancellationToken)
	{
		var result = await _mediator.Send(command, cancellationToken);
		return result.ToJsonResponse();
	}

	/// <summary>
	/// Suggests media for a tag: media similar to those already tagged, most similar first.
	/// </summary>
	/// <response code="200">Suggested media and the number of tagged media used as the sample. No media when fewer than 3 are tagged.</response>
	/// <response code="400">Validation error.</response>
	/// <response code="401">No or invalid admin credentials.</response>
	/// <response code="404">Tag not found.</response>
	/// <response code="503">Similarity search is disabled.</response>
	[Authorize]
	[HttpGet("{id}/suggest")]
	public async Task<ActionResult<SuggestMediaForTagResponse>> GetSimilarTags(SuggestMediaForTagQuery query, CancellationToken cancellationToken)
	{
		var result = await _mediator.Send(query, cancellationToken);
		return result.ToJsonResponse();
	}

	/// <summary>
	/// Adds the tag to media files, keeping their other tags.
	/// </summary>
	/// <response code="200">Number of media that did not have the tag.</response>
	/// <response code="400">Validation error.</response>
	/// <response code="401">No or invalid admin credentials.</response>
	/// <response code="404">Tag or some of the media files not found.</response>
	[Authorize]
	[HttpPost("{id}/media")]
	public async Task<ActionResult<AddTagToMediaResponse>> AddTagToMedia(AddTagToMediaCommand command, CancellationToken cancellationToken)
	{
		var result = await _mediator.Send(command, cancellationToken);
		return result.ToJsonResponse();
	}	
}

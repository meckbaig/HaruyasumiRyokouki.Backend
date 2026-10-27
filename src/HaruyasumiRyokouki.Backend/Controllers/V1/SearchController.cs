using Asp.Versioning;
using HaruyasumiRyokouki.Backend.Extensions;
using HaruyasumiRyokouki.Backend.Features.Search;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;

namespace HaruyasumiRyokouki.Backend.Controllers.V1;

/// <summary>
/// Search of days and media.
/// </summary>
[ApiController]
[Route("v{version:ApiVersion}/[controller]")]
[ApiVersion(1)]
[SwaggerTag("Searches days and media by text or tag.")]
[Produces(MediaTypeNames.Application.Json)]
public class SearchController : ControllerBase
{
	private readonly IMediator _mediator;

	/// <summary>
	/// Constructor with parameters for DI.
	/// </summary>
	public SearchController(IMediator mediator)
	{
		_mediator = mediator;
	}

	/// <summary>
	/// Searches days and media by text or by tag.
	/// </summary>
	/// <remarks>
	/// Text has priority: when <see cref="SearchQuery.Text"/> is given, tags are ignored. Text matches day notes, media titles
	/// and descriptions partially and tag texts exactly, in any language. A tag is found by <see cref="SearchQuery.Tag"/> (slug)
	/// or <see cref="SearchQuery.TagId"/>.
	/// </remarks>
	/// <response code="200">Days with the matching media, newest first.</response>
	/// <response code="400">None of text, tag and tagId is given.</response>
	[HttpGet]
	public async Task<ActionResult<SearchResponse>> GetList(SearchQuery query, CancellationToken cancellationToken)
	{
		var result = await _mediator.Send(query, cancellationToken);
		return result.ToJsonResponse();
	}
}

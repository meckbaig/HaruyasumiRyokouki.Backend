using Asp.Versioning;
using HaruyasumiRyokouki.Backend.Extensions;
using HaruyasumiRyokouki.Backend.Features.Search;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;

namespace HaruyasumiRyokouki.Backend.Controllers.V1;

[ApiController]
[Route("v{version:ApiVersion}/[controller]")]
[ApiVersion(1)]
[SwaggerTag("Manages media parameters.")]
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

	[HttpGet]
	public async Task<ActionResult<SearchResponse>> GetList(SearchQuery query, CancellationToken cancellationToken)
	{
		var result = await _mediator.Send(query, cancellationToken);
		return result.ToJsonResponse();
	}
}

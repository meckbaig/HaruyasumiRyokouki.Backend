using Asp.Versioning;
using HaruyasumiRyokouki.Backend.Extensions;
using HaruyasumiRyokouki.Backend.Features.Media;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;

namespace HaruyasumiRyokouki.Backend.Controllers.V1;

/// <summary>
/// Media files: listing, editing, sync and YouTube import.
/// </summary>
[ApiController]
[Route("v{version:ApiVersion}/[controller]")]
[ApiVersion(1)]
[SwaggerTag("Manages media files: listing, editing, sync and YouTube import.")]
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

	/// <summary>
	/// Deletes a media file. Its files are removed from the file storage in the background.
	/// </summary>
	/// <response code="200">Deleted.</response>
	/// <response code="400">Validation error.</response>
	/// <response code="401">No or invalid admin credentials.</response>
	/// <response code="404">Media file not found.</response>
	[Authorize]
	[HttpDelete("{mediaId}")]
	public async Task<IActionResult> DeleteMedia(DeleteMediaCommand command, CancellationToken cancellationToken)
	{
		var result = await _mediator.Send(command, cancellationToken);
		return Ok();
	}

	/// <summary>
	/// Applies the same changes to several media files.
	/// </summary>
	/// <remarks>
	/// Fields absent from <see cref="EditMediaCommand.BodyParameters.Changes"/> are not modified, <see langword="null"/> clears a field.
	/// <see cref="Models.Dtos.Media.EditMediaChanges.TagIds"/> replaces the tags, <see cref="Models.Dtos.Media.EditMediaChanges.Translations"/>
	/// are updated by language. With <see cref="EditMediaCommand.BodyParameters.AutoTranslate"/> the missing languages are translated
	/// and returned without being saved.
	/// </remarks>
	/// <response code="200">Edited media with all translations.</response>
	/// <response code="400">Validation error.</response>
	/// <response code="401">No or invalid admin credentials.</response>
	/// <response code="404">Some of the media files not found. Nothing is changed.</response>
	[Authorize]
	[HttpPatch]
	public async Task<ActionResult<EditMediaResponse>> EditMedia(EditMediaCommand command, CancellationToken cancellationToken)
	{
		var result = await _mediator.Send(command, cancellationToken);
		return result.ToJsonResponse();
	}

	/// <summary>
	/// Imports new files from the file storage, converts them and creates missing miniatures.
	/// </summary>
	/// <remarks>
	/// Runs until all new files are processed, which can take minutes. Files that cannot be imported are skipped and logged.
	/// </remarks>
	/// <response code="200">Sync finished.</response>
	/// <response code="401">No or invalid admin credentials.</response>
	/// <response code="409">Another sync is running.</response>
	[Authorize]
	[HttpPut("sync")]
	public async Task<IActionResult> SyncMedia([FromQuery] SyncMediaCommand command, CancellationToken cancellationToken)
	{
		var result = await _mediator.Send(command, cancellationToken);
		return Ok();
	}

	/// <summary>
	/// Imports a YouTube video as a media file of the day it was published.
	/// </summary>
	/// <response code="200">Imported media.</response>
	/// <response code="400">Invalid link, video already imported, or published outside the trip dates.</response>
	/// <response code="401">No or invalid admin credentials.</response>
	/// <response code="502">yt-dlp, the thumbnail download or ffmpeg failed.</response>
	[Authorize]
	[HttpPost("youtube")]
	public async Task<ActionResult<ImportYoutubeMediaResponse>> ImportYoutubeMedia(ImportYoutubeMediaCommand command, CancellationToken cancellationToken)
	{
		var result = await _mediator.Send(command, cancellationToken);
		return result.ToJsonResponse();
	}

	/// <summary>
	/// Returns media files with all translations, for the editor.
	/// </summary>
	/// <response code="200">Media files.</response>
	/// <response code="400">Validation error.</response>
	/// <response code="401">No or invalid admin credentials.</response>
	/// <response code="404">Some of the media files not found.</response>
	[Authorize]
	[HttpGet("edit")]
	public async Task<ActionResult<GetEditMediaResponse>> GetEditMedia(GetEditMediaQuery query, CancellationToken cancellationToken)
	{
		var result = await _mediator.Send(query, cancellationToken);
		return result.ToJsonResponse();
	}

	/// <summary>
	/// Returns media with coordinates created between two dates, both inclusive, for the map.
	/// </summary>
	/// <remarks>Media that are private or not approved are returned only to the admin.</remarks>
	/// <response code="200">Media ordered by creation time.</response>
	/// <response code="400">Validation error.</response>
	[HttpGet]
	public async Task<ActionResult<GetMediaResponse>> GetMedia(GetMediaQuery query, CancellationToken cancellationToken)
	{
		var result = await _mediator.Send(query, cancellationToken);
		return result.ToJsonResponse();
	}

	/// <summary>
	/// Returns random favorite media for the front page.
	/// </summary>
	/// <remarks>Private media are returned only to the admin.</remarks>
	/// <response code="200">Favorite media.</response>
	[HttpGet("favorites")]
	public async Task<ActionResult<GetFavoriteMediaResponse>> GetFavoriteMedia([FromQuery] GetFavoriteMediaQuery query, CancellationToken cancellationToken)
	{
		var result = await _mediator.Send(query, cancellationToken);
		return result.ToJsonResponse();
	}

	/// <summary>
	/// Returns media that look similar to the given one, most similar first.
	/// </summary>
	/// <response code="200">Similar media with scores. Empty when the media has no embedding.</response>
	/// <response code="400">Validation error.</response>
	/// <response code="401">No or invalid admin credentials.</response>
	/// <response code="404">Media file not found.</response>
	/// <response code="503">Similarity search is disabled.</response>
	[Authorize]
	[HttpGet("{id}/similar")]
	public async Task<ActionResult<GetSimilarMediaResponse>> GetSimilarMedia(GetSimilarMediaQuery query, CancellationToken cancellationToken)
	{
		var result = await _mediator.Send(query, cancellationToken);
		return result.ToJsonResponse();
	}
}

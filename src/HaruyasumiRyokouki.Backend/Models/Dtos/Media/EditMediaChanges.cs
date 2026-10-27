using HaruyasumiRyokouki.Backend.Common.OptionalType;

namespace HaruyasumiRyokouki.Backend.Models.Dtos.Media;

/// <summary>
/// Changes of media files applied by <c>PATCH /v1/media</c>.
/// </summary>
/// <remarks>
/// Every field is an <see cref="Common.OptionalType.Optional{T}"/>: a field absent from the request is not modified, a field sent as <see langword="null"/> is cleared.
/// </remarks>
public record EditMediaChanges
{
	/// <summary>
	/// New latitude from -90 to 90, or <see langword="null"/> to remove the location.
	/// </summary>
	public Optional<double?> Latitude { get; set; }

	/// <summary>
	/// New longitude from -180 to 180, or <see langword="null"/> to remove the location.
	/// </summary>
	public Optional<double?> Longitude { get; set; }

	/// <summary>
	/// Approve the media or take the approval back.
	/// </summary>
	public Optional<bool> IsApproved { get; set; }

	/// <summary>
	/// Hide the media from visitors or show it.
	/// </summary>
	public Optional<bool> Private { get; set; }

	/// <summary>
	/// Add the media to the favorites shown on the home page or remove it.
	/// </summary>
	public Optional<bool> Favorite { get; set; }

	/// <summary>
	/// Complete new set of tags of the media.
	/// </summary>
	/// <remarks>
	/// Replaces the current tags. An empty list removes all tags, <see langword="null"/> is not allowed. To add one tag without touching the others, use <c>POST /v1/tags/{id}/media</c>.
	/// </remarks>
	public Optional<List<int>> TagIds { get; set; }

	/// <summary>
	/// Titles and descriptions to add or replace, at most one per language.
	/// </summary>
	/// <remarks>
	/// Languages that are not sent keep their texts.
	/// </remarks>
	public Optional<List<MediaTranslationEditDto>> Translations { get; set; }
}

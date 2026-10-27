using HaruyasumiRyokouki.Backend.Common.OptionalType;

namespace HaruyasumiRyokouki.Backend.Models.Dtos.Media;

/// <summary>
/// Changes of <c>PATCH /v1/media</c>. An absent field is not modified, <c>null</c> clears it.
/// </summary>
public record EditMediaChanges
{
	/// <summary>
	/// New latitude, -90 to 90, or <c>null</c> to remove the coordinates.
	/// </summary>
	public Optional<double?> Latitude { get; set; }

	/// <summary>
	/// New longitude, -180 to 180, or <c>null</c> to remove the coordinates.
	/// </summary>
	public Optional<double?> Longitude { get; set; }

	/// <summary>
	/// Approve or unapprove.
	/// </summary>
	public Optional<bool> IsApproved { get; set; }

	/// <summary>
	/// Hide from visitors or show.
	/// </summary>
	public Optional<bool> Private { get; set; }

	/// <summary>
	/// Show on the front page or not.
	/// </summary>
	public Optional<bool> Favorite { get; set; }

	/// <summary>
	/// Complete new set of tags; replaces the current tags. <c>null</c> is not allowed, <c>[]</c> removes all.
	/// </summary>
	public Optional<List<int>> TagIds { get; set; }

	/// <summary>
	/// Translations to add or replace, at most one per language; languages not sent are kept.
	/// </summary>
	public Optional<List<MediaTranslationEditDto>> Translations { get; set; }
}

using HaruyasumiRyokouki.Backend.Common.OptionalType;

namespace HaruyasumiRyokouki.Backend.Models.Dtos.Media;

public record EditMediaChanges
{
	public Optional<double?> Latitude { get; set; }
	public Optional<double?> Longitude { get; set; }
	public Optional<bool> IsApproved { get; set; }
	public Optional<bool> Private { get; set; }
	public Optional<bool> Favorite { get; set; }

	public Optional<List<int>> TagIds { get; set; }
	public Optional<List<MediaTranslationEditDto>> Translations { get; set; }
}

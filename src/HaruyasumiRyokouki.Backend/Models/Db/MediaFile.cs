using HaruyasumiRyokouki.Backend.Models.Db.Enums;

namespace HaruyasumiRyokouki.Backend.Models.Db;

public class MediaFile
{
	public int Id { get; set; }
	public int DayId { get; set; }
	public required DateTime Created { get; set; }
	public required string FileName { get; set; } = null!;
	public required float AspectRatio { get; set; }
	public required MediaType Type { get; set; } = MediaType.Unknown;
	public MediaSource Source { get; set; } = MediaSource.Local;
	public string? ExternalUrl { get; set; }
	public double? Latitude { get; set; }
	public double? Longitude { get; set; }
	public string? Miniature { get; set; }
	public bool IsApproved { get; set; }
	public bool Private { get; set; } = false;
	public bool Favorite { get; set; } = false;

	public ICollection<string> AdditionalFiles = [];

	public ICollection<MediaTranslation> Translations { get; set; } = [];
	public ICollection<MediaFileTag> MediaTags { get; set; } = [];
	public ICollection<Tag> Tags { get; set; } = [];
	public Day Day { get; set; }
}

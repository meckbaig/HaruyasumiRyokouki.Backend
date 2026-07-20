using HaruyasumiRyokouki.Backend.Models.Db.Enums;

namespace HaruyasumiRyokouki.Backend.Models.Db;

public class MediaFile
{
	public Guid Id { get; set; }
	public DateOnly DayDate { get; set; }
	public required string FileName { get; set; } = null!;
	public MediaType Type { get; set; } = MediaType.Unknown; 
	public double? Latitude { get; set; }
	public double? Longitude { get; set; }
	public bool IsApproved { get; set; }

	public ICollection<MediaTranslation> Translations { get; set; } = [];
	public Day Day { get; set; }
}

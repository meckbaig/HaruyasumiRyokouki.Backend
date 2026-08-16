namespace HaruyasumiRyokouki.Backend.Models.Db;

public class MediaFileTag
{
	public int MediaId { get; set; }
	public MediaFile Media { get; set; } = null!;

	public int TagId { get; set; }
	public Tag Tag { get; set; } = null!;
}

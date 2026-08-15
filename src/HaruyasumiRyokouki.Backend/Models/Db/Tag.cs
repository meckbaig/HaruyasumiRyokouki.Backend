namespace HaruyasumiRyokouki.Backend.Models.Db;

public class Tag
{
	public int Id { get; set; }
	public string Slug { get; set; }

	public ICollection<TagTranslation> Translations { get; set; } = [];
	public ICollection<MediaFileTag> MediaTags { get; set; } = [];
	public ICollection<MediaFile> Media { get; set; } = [];
}

namespace HaruyasumiRyokouki.Backend.Models.Db;

public class MediaTranslation
{
	public Guid Id { get; set; }
	public Guid MediaFileId { get; set; }
	public string LanguageCode { get; set; } = null!;
	public string? Title { get; set; }
	public string? Description { get; set; }

	public ICollection<string> Tags { get; set; } = [];
	public MediaFile MediaFile { get; set; }
}

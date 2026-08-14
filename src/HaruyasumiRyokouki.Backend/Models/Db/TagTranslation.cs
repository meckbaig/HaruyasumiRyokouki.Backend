namespace HaruyasumiRyokouki.Backend.Models.Db;

public class TagTranslation
{
	public int Id { get; set; }
	public int TagId { get; set; }
	public string LanguageCode { get; set; }
	public string Text { get; set; }
	public bool IsPrimary { get; set; }

	public Tag Tag { get; set; }
}

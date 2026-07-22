namespace HaruyasumiRyokouki.Backend.Models.Db;

public class DayTranslation
{
	public Guid Id { get; set; }
	public int DayId { get; set; }
	public string LanguageCode { get; set; } = null!;
	public string Note { get; set; } = null!;

	public Day Day { get; set; }
}

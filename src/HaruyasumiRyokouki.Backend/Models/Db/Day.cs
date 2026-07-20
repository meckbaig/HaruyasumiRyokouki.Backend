namespace HaruyasumiRyokouki.Backend.Models.Db;

public class Day
{
	public DateOnly Date { get; set; }
	public bool IsReady { get; set; }

	public ICollection<DayTranslation> Translations { get; set; } = []; 
	public ICollection<MediaFile> Media { get; set; } = [];
}

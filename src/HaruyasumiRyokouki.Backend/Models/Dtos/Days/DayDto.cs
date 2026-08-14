using HaruyasumiRyokouki.Backend.Models.Db;
using HaruyasumiRyokouki.Backend.Models.Dtos.Media;

namespace HaruyasumiRyokouki.Backend.Models.Dtos.Days;

public record DayDto
{
	public DateOnly Date { get; set; }
	public bool IsReady { get; set; }
	public string LanguageCode { get; set; } = null!;
	public string Note { get; set; } = null!;

	public ICollection<MediaFileDto> Media { get; set; } = [];
}

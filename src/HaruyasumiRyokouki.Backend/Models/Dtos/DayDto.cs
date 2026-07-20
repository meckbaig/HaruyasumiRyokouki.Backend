using HaruyasumiRyokouki.Backend.Models.Db;

namespace HaruyasumiRyokouki.Backend.Models.Dtos;

public record DayDto
{
	public DateOnly Date { get; set; }
	public bool IsReady { get; set; }
	public string LanguageCode { get; set; } = null!;
	public string Note { get; set; } = null!;

	public ICollection<MediaFileDto> Media { get; set; } = [];
}

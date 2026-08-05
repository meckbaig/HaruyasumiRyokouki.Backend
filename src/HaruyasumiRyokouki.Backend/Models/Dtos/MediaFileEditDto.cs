using HaruyasumiRyokouki.Backend.Models.Db.Enums;

namespace HaruyasumiRyokouki.Backend.Models.Dtos;

public record MediaFileEditDto
{
	public int Id { get; set; }
	public DateTime Created { get; set; }
	public required string FileName { get; set; } = null!;
	public string Type { get; set; }
	public double? Latitude { get; set; }
	public double? Longitude { get; set; }
	public string Miniature { get; set; }

	public List<MediaTranslationEditDto> Translations { get; set; } = [];
}

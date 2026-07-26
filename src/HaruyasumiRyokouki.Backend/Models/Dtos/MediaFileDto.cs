using HaruyasumiRyokouki.Backend.Models.Db.Enums;

namespace HaruyasumiRyokouki.Backend.Models.Dtos;

public record MediaFileDto
{
	public Guid Id { get; set; }
	public DateTime Created { get; set; }
	public required string FileName { get; set; } = null!;
	public string Type { get; set; }
	public double? Latitude { get; set; }
	public double? Longitude { get; set; }
	public bool IsApproved { get; set; }
	public string LanguageCode { get; set; } = null!;
	public string? Title { get; set; }
	public string? Description { get; set; }

	public ICollection<string> Tags { get; set; } = [];
}

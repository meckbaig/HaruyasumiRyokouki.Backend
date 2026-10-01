using System.Text.Json.Serialization;

namespace HaruyasumiRyokouki.Backend.Models.Dtos.Media;

public record VideoUrlsDto
{
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? Download { get; set; }
	public string? Stream { get; set; }
	public string Preview { get; set; } = null!;
}

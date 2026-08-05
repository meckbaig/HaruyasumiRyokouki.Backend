namespace HaruyasumiRyokouki.Backend.Models.Dtos;

public record MediaFileLocationDto
{
	public int Id { get; set; }
	public DateTime Created { get; set; }
	public double Latitude { get; set; }
	public double Longitude { get; set; }
	public string FileName { get; set; }
	public string? Title { get; set; }
	public string? LanguageCode { get; set; }
	public string Miniature { get; set; }
}

namespace HaruyasumiRyokouki.Backend.Models.Dtos;

internal interface IPreviewDto
{
	public string FileName { get; set; }
	public float AspectRatio { get; set; }
	internal string Type { get; set; }
	public ImageUrlsDto? ImageUrls { get; set; }
	public VideoUrlsDto? VideoUrls { get; set; }
}

namespace HaruyasumiRyokouki.Backend.Models.InternalDtos;

internal record ConvertionsResponseDto
{
	public ConvertionsResponseDto(string newFileName, string miniature, float aspectRatio)
	{
		NewFileName = newFileName;
		Miniature = miniature;
		AspectRatio = aspectRatio;
	}

	public string NewFileName { get; set; }
	public string Miniature { get; set; }
	public float AspectRatio { get; set; }
}

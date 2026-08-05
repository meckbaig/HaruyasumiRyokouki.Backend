namespace HaruyasumiRyokouki.Backend.Models.InternalDtos;

internal record ConvertionsResponseDto
{
	public ConvertionsResponseDto(string newFileName, string miniature)
	{
		NewFileName = newFileName;
		Miniature = miniature;
	}

	public string NewFileName { get; set; }
	public string Miniature { get; set; }
}

namespace HaruyasumiRyokouki.Backend.Models.InternalDtos;

internal record ConvertionsResponseDto
{
	public ConvertionsResponseDto(string newFileName, string miniature, float aspectRatio, ICollection<string> additionalFileNames)
	{
		NewFileName = newFileName;
		Miniature = miniature;
		AspectRatio = aspectRatio;
		AdditionalFileNames = additionalFileNames;
	}

	public string NewFileName { get; set; }
	public string Miniature { get; set; }
	public float AspectRatio { get; set; }
	public ICollection<string> AdditionalFileNames { get; set; }
}

namespace HaruyasumiRyokouki.Backend.Models.InternalDtos;

/// <summary>
/// Result of converting a media file in the file storage.
/// </summary>
internal record ConversionResponseDto
{
	/// <summary>
	/// Creates the result.
	/// </summary>
	public ConversionResponseDto(string newFileName, string miniature, float aspectRatio, ICollection<string> additionalFileNames)
	{
		NewFileName = newFileName;
		Miniature = miniature;
		AspectRatio = aspectRatio;
		AdditionalFileNames = additionalFileNames;
	}

	/// <summary>
	/// Name of the media file after the conversion.
	/// </summary>
	/// <remarks>
	/// Differs from the original name when an image was converted to another format.
	/// </remarks>
	public string NewFileName { get; set; }

	/// <summary>
	/// Tiny blurred placeholder of the image, base64-encoded, shown while the real image loads.
	/// </summary>
	public string Miniature { get; set; }

	/// <summary>
	/// Width divided by height, with the rotation of the image taken into account.
	/// </summary>
	public float AspectRatio { get; set; }

	/// <summary>
	/// Files created next to the media file, see <see cref="Db.MediaFile.AdditionalFiles"/>.
	/// </summary>
	public ICollection<string> AdditionalFileNames { get; set; }
}

namespace HaruyasumiRyokouki.Backend.Models.Dtos.Media;

/// <summary>
/// Media DTO that gets image or video URLs from <see cref="Extensions.MappingExtensions"/>.
/// </summary>
internal interface IPreviewDto
{
	/// <summary>
	/// File name in the file storage.
	/// </summary>
	/// <remarks>
	/// For YouTube media it is the video id. The only stored file is the preview from <see cref="Db.MediaFile.AdditionalFiles"/>.
	/// </remarks>
	public string FileName { get; set; }

	/// <summary>
	/// Width divided by height, with the rotation of the image taken into account.
	/// </summary>
	public float AspectRatio { get; set; }

	/// <summary>
	/// <see cref="Db.Enums.MediaType.Image"/> or <see cref="Db.Enums.MediaType.Video"/> as a string.
	/// </summary>
	internal string Type { get; set; }

	/// <summary>
	/// URLs of an image, or <see langword="null"/> for a video.
	/// </summary>
	/// <remarks>
	/// Omitted from the response when <see langword="null"/>.
	/// </remarks>
	public ImageUrlsDto? ImageUrls { get; set; }

	/// <summary>
	/// URLs of a video, or <see langword="null"/> for an image.
	/// </summary>
	/// <remarks>
	/// Omitted from the response when <see langword="null"/>.
	/// </remarks>
	public VideoUrlsDto? VideoUrls { get; set; }
}

using HaruyasumiRyokouki.Backend.Models.InternalDtos;
using HaruyasumiRyokouki.Backend.Models.InternalDtos.Enums;

namespace HaruyasumiRyokouki.Backend.Services.Interfaces;

/// <summary>
/// Builds media URLs for one preview provider (Nextcloud or imgproxy), selected by <c>MediaPreview:Provider</c>.
/// </summary>
public interface IMediaPreviewService
{
	/// <summary>
	/// Builds a URL of an image file.
	/// </summary>
	/// <param name="fileName">File name in the storage.</param>
	/// <param name="linkType">Kind of URL; the resized kinds depend on <paramref name="clientDisplay"/> and <paramref name="aspectRatio"/>.</param>
	/// <param name="clientDisplay">Client screen, <see langword="null"/> for defaults.</param>
	/// <param name="aspectRatio">Aspect ratio of the image, <see langword="null"/> for the default.</param>
	string GetImageUrl(string fileName, ImageUrlType linkType, ClientDisplay? clientDisplay = null, float? aspectRatio = default);

	/// <summary>
	/// Builds a URL for a local video.
	/// </summary>
	/// <param name="fileName">Video file name in the storage.</param>
	/// <param name="linkType">Kind of URL.</param>
	/// <param name="additionalFiles">Additional files of the media; the stream URL uses the <c>_web</c> file when it is listed.</param>
	/// <param name="clientDisplay">Client screen, for the preview size.</param>
	/// <param name="aspectRatio">Aspect ratio of the video, for the preview size.</param>
	string GetVideoUrl(string fileName, VideoUrlType linkType, ICollection<string> additionalFiles, ClientDisplay? clientDisplay = null, float? aspectRatio = default);
}

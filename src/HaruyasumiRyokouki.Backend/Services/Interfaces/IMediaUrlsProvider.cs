using HaruyasumiRyokouki.Backend.Models.Db.Enums;
using HaruyasumiRyokouki.Backend.Models.Dtos.Media;
using HaruyasumiRyokouki.Backend.Models.InternalDtos;
using HaruyasumiRyokouki.Backend.Models.InternalDtos.Enums;

namespace HaruyasumiRyokouki.Backend.Services.Interfaces;

/// <summary>
/// URLs of media for DTOs: delegates to the preview service and handles YouTube media.
/// </summary>
internal interface IMediaUrlsProvider
{
	/// <summary>
	/// Builds a URL of an image file, see <see cref="IMediaPreviewService.GetImageUrl"/>.
	/// </summary>
	string GetImageUrl(string fileName, ImageUrlType linkType, ClientDisplay? clientDisplay = null, float? aspectRatio = default);

	/// <summary>
	/// Builds all URLs of a video. For YouTube the stream is <paramref name="externalUrl"/> and there is no download URL.
	/// </summary>
	VideoUrlsDto GetVideoUrls(string fileName, MediaSource source, string? externalUrl, ICollection<string> additionalFiles, ClientDisplay? clientDisplay = null, float? aspectRatio = default);
}

using HaruyasumiRyokouki.Backend.Models.InternalDtos.Enums;

namespace HaruyasumiRyokouki.Backend.Services.Interfaces;

/// <summary>
/// Image size requested from the preview service, rounded to <c>MediaSizes:SizeBuckets</c>.
/// </summary>
internal interface IMediaResolutionCalculationService
{
	/// <summary>
	/// Calculating the size in which the image will fit.
	/// </summary>
	int GetResolution(ImageUrlType linkType, float? dpr, int? resolution, float? aspectRatio = default);
}

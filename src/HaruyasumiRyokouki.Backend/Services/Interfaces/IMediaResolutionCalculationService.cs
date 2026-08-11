using HaruyasumiRyokouki.Backend.Models.InternalDtos.Enums;

namespace HaruyasumiRyokouki.Backend.Services.Interfaces;

internal interface IMediaResolutionCalculationService
{
	/// <summary>
	/// Calculating the size in which the image will fit.
	/// </summary>
	int GetResolution(ImageUrlType linkType, float? dpr, int? resolution, float? aspectRatio = default);
}

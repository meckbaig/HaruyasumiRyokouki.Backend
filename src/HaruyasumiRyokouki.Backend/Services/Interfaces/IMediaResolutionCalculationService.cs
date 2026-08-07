using HaruyasumiRyokouki.Backend.Models.InternalDtos.Enums;

namespace HaruyasumiRyokouki.Backend.Services.Interfaces;

internal interface IMediaResolutionCalculationService
{
	int CalculateSize(ImageUrlType linkType, float? dpr, int? resolution);
	int GetNearestResolution(int calculatedSize);
	int GetResolution(ImageUrlType linkType, float? dpr, int? resolution);
}

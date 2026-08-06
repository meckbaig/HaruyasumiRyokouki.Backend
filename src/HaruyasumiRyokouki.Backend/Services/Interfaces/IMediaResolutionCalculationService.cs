using HaruyasumiRyokouki.Backend.Models.InternalDtos.Enums;

namespace HaruyasumiRyokouki.Backend.Services.Interfaces;

internal interface IMediaResolutionCalculationService
{
	int CalculateSize(ImageUrlType linkType, double? dpr, int? resolution);
	int GetNearestResolution(int calculatedSize);
	int GetResolution(ImageUrlType linkType, double? dpr, int? resolution);
}

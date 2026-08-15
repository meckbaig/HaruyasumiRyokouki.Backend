using HaruyasumiRyokouki.Backend.Models.InternalDtos;
using HaruyasumiRyokouki.Backend.Models.InternalDtos.Enums;

namespace HaruyasumiRyokouki.Backend.Services.Interfaces;

public interface IMediaPreviewService
{
	string GetImageUrl(string fileName, ImageUrlType linkType, ClientDisplay? clientDisplay = null, float? aspectRatio = default);
	string GetVideoUrl(string fileName, VideoUrlType linkType, ICollection<string> additionalFiles, ClientDisplay? clientDisplay = null, float? aspectRatio = default);
}

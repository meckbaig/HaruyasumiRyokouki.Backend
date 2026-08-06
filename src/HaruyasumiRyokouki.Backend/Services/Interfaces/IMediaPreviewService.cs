using HaruyasumiRyokouki.Backend.Models.InternalDtos;
using HaruyasumiRyokouki.Backend.Models.InternalDtos.Enums;

namespace HaruyasumiRyokouki.Backend.Services.Interfaces;

public interface IMediaPreviewService
{
	string GetImageUrl(string fileName, ImageUrlType linkType, ClientDisplay? clientDisplay = null);
	string GetVideoUrl(string fileName, VideoUrlType linkType, ClientDisplay? clientDisplay = null);
}

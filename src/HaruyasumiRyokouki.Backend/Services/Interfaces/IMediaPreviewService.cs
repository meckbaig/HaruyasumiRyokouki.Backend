using HaruyasumiRyokouki.Backend.Models.InternalDtos.Enums;

namespace HaruyasumiRyokouki.Backend.Services.Interfaces;

public interface IMediaPreviewService
{
	string GetImageUrl(string fileName, ImageUrlType linkType);
	string GetVideoUrl(string fileName, VideoUrlType linkType);
}

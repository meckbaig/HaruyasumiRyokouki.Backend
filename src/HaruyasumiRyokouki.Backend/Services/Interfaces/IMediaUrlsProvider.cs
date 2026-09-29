using HaruyasumiRyokouki.Backend.Models.Db.Enums;
using HaruyasumiRyokouki.Backend.Models.Dtos.Media;
using HaruyasumiRyokouki.Backend.Models.InternalDtos;
using HaruyasumiRyokouki.Backend.Models.InternalDtos.Enums;

namespace HaruyasumiRyokouki.Backend.Services.Interfaces;

internal interface IMediaUrlsProvider
{
	string GetImageUrl(string fileName, ImageUrlType linkType, ClientDisplay? clientDisplay = null, float? aspectRatio = default);
	VideoUrlsDto GetVideoUrls(string fileName, MediaSource source, string? externalUrl, ICollection<string> additionalFiles, ClientDisplay? clientDisplay = null, float? aspectRatio = default);
}

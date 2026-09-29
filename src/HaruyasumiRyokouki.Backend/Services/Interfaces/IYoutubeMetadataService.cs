using HaruyasumiRyokouki.Backend.Common.ResultType;
using HaruyasumiRyokouki.Backend.Models.InternalDtos;

namespace HaruyasumiRyokouki.Backend.Services.Interfaces;

internal interface IYoutubeMetadataService
{
	Task<Result<YoutubeMediaMetadata>> GetMetadataAsync(string videoId, CancellationToken cancellationToken);
}

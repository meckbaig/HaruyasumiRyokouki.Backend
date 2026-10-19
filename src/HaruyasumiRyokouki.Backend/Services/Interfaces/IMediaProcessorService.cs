using HaruyasumiRyokouki.Backend.Common.ResultType;
using HaruyasumiRyokouki.Backend.Models.InternalDtos;

namespace HaruyasumiRyokouki.Backend.Services.Interfaces
{
	internal interface IMediaProcessorService
	{
		Task<Result<ConversionResponseDto>> ConvertImageAsync(string fileName, CancellationToken cancellationToken);
		Task<Result<ConversionResponseDto>> ConvertVideoAsync(string fileName, CancellationToken cancellationToken);
		Task<Result<string>> CreateMiniatureAsync(string fileName, CancellationToken cancellationToken);
		Task<Result<string>> CreateYoutubePreviewAsync(string videoId, string sourceExtension, Stream thumbnail, CancellationToken cancellationToken);
	}
}

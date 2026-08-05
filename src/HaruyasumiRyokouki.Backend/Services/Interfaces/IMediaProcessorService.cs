using HaruyasumiRyokouki.Backend.Common.ResultType;
using HaruyasumiRyokouki.Backend.Models.InternalDtos;

namespace HaruyasumiRyokouki.Backend.Services.Interfaces
{
	internal interface IMediaProcessorService
	{
		Task<Result<ConvertionsResponseDto>> ConvertImageAsync(string fileName, CancellationToken cancellationToken);
		Task<Result<ConvertionsResponseDto>> ConvertVideoAsync(string fileName, CancellationToken cancellationToken);
		Task<Result<string>> CreateMiniatureAsync(string fileName, CancellationToken cancellationToken);
		string GetVideoPreviewName(string fileName);
		string GetVideoWebName(string fileName);
		bool IsAnImage(string fileName);
		bool IsAVideo(string fileName);
	}
}

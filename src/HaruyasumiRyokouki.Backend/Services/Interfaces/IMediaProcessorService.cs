using HaruyasumiRyokouki.Backend.Common.ResultType;

namespace HaruyasumiRyokouki.Backend.Services.Interfaces
{
	internal interface IMediaProcessorService
	{
		Task<Result<string>> ConvertImageAsync(string fileName, CancellationToken cancellationToken);
		Task<Result<string>> ConvertVideoAsync(string fileName, CancellationToken cancellationToken);
		string GetWebName(string fileName);
		bool IsAnImage(string fileName);
		bool IsAVideo(string fileName);
	}
}
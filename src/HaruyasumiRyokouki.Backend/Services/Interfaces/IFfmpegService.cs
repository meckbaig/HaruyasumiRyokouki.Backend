using HaruyasumiRyokouki.Backend.Models.InternalDtos;

namespace HaruyasumiRyokouki.Backend.Services.Interfaces;

internal interface IFfmpegService
{
	Task ConvertImageAsync(string input, string output, CancellationToken cancellationToken = default);
	Task CreateVideoPreviewAsync(string input, string output, CancellationToken cancellationToken = default);
	Task ConvertVideoAsync(string input, string output, CancellationToken cancellationToken = default);
	Task<VideoFileInfo> GetVideoInfoAsync(MediaInput mediaInput, CancellationToken cancellationToken = default);
	Task<byte[]> GetImageMiniatureBytesAsync(string input, CancellationToken cancellationToken);
}

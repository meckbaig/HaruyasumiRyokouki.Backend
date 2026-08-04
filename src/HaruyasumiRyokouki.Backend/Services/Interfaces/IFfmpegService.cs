using HaruyasumiRyokouki.Backend.Models.InternalDtos;
using HaruyasumiRyokouki.Backend.Models.InternalDtos.Enums;

namespace HaruyasumiRyokouki.Backend.Services.Interfaces;

internal interface IFfmpegService
{
	Task ConvertImageAsync(string input, string output, FfmpegImagePreset preset, CancellationToken cancellationToken = default);
	Task ConvertVideoAsync(string input, string output, FfmpegVideoPreset preset, CancellationToken cancellationToken = default);
	Task<VideoFileInfo> GetVideoInfoAsync(MediaInput mediaInput, CancellationToken cancellationToken = default);
}

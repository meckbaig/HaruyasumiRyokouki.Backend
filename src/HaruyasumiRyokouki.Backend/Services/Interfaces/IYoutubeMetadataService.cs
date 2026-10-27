using HaruyasumiRyokouki.Backend.Common.ResultType;
using HaruyasumiRyokouki.Backend.Models.InternalDtos;

namespace HaruyasumiRyokouki.Backend.Services.Interfaces;

/// <summary>
/// Metadata of YouTube videos, read with <c>yt-dlp</c> (through <c>YouTube:Proxy</c> when enabled).
/// </summary>
internal interface IYoutubeMetadataService
{
	/// <summary>
	/// Reads publish time, aspect ratio and thumbnail URL of a video.
	/// </summary>
	/// <returns>Metadata, or a failure when yt-dlp is missing, fails, or its output lacks the date or resolution.</returns>
	Task<Result<YoutubeMediaMetadata>> GetMetadataAsync(string videoId, CancellationToken cancellationToken);
}

using HaruyasumiRyokouki.Backend.Common.ResultType;
using HaruyasumiRyokouki.Backend.Models.InternalDtos;
using HaruyasumiRyokouki.Backend.Services.Interfaces;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Videos.Streams;

namespace HaruyasumiRyokouki.Backend.Services;

internal class YoutubeMetadataService : IYoutubeMetadataService
{
	private readonly YoutubeClient _client = new();

	public async Task<Result<YoutubeMediaMetadata>> GetMetadataAsync(string videoId, CancellationToken cancellationToken)
	{
		try
		{
			var video = await _client.Videos.GetAsync(videoId, cancellationToken);
			var thumbnail = video.Thumbnails.GetWithHighestResolution();

			var manifest = await _client.Videos.Streams.GetManifestAsync(videoId, cancellationToken);
			var stream = manifest
				.GetVideoStreams()
				.MaxBy(s => s.VideoResolution.Height);

			if (stream is null || stream.VideoResolution.Height == 0)
				return Result<YoutubeMediaMetadata>.Failure($"Unable to determine video resolution for {videoId}.");

			var resolution = stream.VideoResolution;

			return Result<YoutubeMediaMetadata>.Success(new YoutubeMediaMetadata
			{
				VideoId = video.Id,
				ExternalUrl = video.Url,
				PublishedAtUtc = video.UploadDate.UtcDateTime,
				ThumbnailUrl = thumbnail.Url,
				AspectRatio = (float)resolution.Width / resolution.Height
			});
		}
		catch (Exception ex)
		{
			return Result<YoutubeMediaMetadata>.Failure($"Failed to fetch YouTube metadata for {videoId}: {ex.Message}");
		}
	}
}

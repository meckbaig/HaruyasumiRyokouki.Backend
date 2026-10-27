namespace HaruyasumiRyokouki.Backend.Models.InternalDtos;

/// <summary>
/// Metadata of a YouTube video read with yt-dlp.
/// </summary>
internal sealed record YoutubeMediaMetadata
{
	/// <summary>
	/// YouTube video id.
	/// </summary>
	public required string VideoId { get; init; }

	/// <summary>
	/// Page URL of the video.
	/// </summary>
	public required string ExternalUrl { get; init; }

	/// <summary>
	/// Publish time of the video in UTC.
	/// </summary>
	public required DateTime PublishedAtUtc { get; init; }

	/// <summary>
	/// URL of the largest thumbnail.
	/// </summary>
	public required string ThumbnailUrl { get; init; }

	/// <summary>
	/// Width divided by height of the video.
	/// </summary>
	public required float AspectRatio { get; init; }
}

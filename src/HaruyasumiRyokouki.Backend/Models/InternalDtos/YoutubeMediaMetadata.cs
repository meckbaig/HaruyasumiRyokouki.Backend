namespace HaruyasumiRyokouki.Backend.Models.InternalDtos;

internal sealed record YoutubeMediaMetadata
{
	public required string VideoId { get; init; }
	public required string ExternalUrl { get; init; }
	public required DateTime PublishedAtUtc { get; init; }
	public required string ThumbnailUrl { get; init; }
	public required float AspectRatio { get; init; }
}

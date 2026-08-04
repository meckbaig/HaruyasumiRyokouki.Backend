namespace HaruyasumiRyokouki.Backend.Models.InternalDtos;

public record VideoFileInfo
{
	public long Bitrate { get; init; }
	public double Duration { get; init; }
	public long Size { get; init; }
}

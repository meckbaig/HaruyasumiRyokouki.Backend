namespace HaruyasumiRyokouki.Backend.Models.InternalDtos;

/// <summary>
/// Video properties read by ffprobe.
/// </summary>
public record VideoFileInfo
{
	/// <summary>
	/// Overall bitrate in bits per second.
	/// </summary>
	public long Bitrate { get; init; }

	/// <summary>
	/// Duration in seconds.
	/// </summary>
	public double Duration { get; init; }

	/// <summary>
	/// File size in bytes.
	/// </summary>
	public long Size { get; init; }
}

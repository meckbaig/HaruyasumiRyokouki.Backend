namespace HaruyasumiRyokouki.Backend.Models.InternalDtos;

/// <summary>
/// How ffmpeg and ffprobe can read a file from the file storage directly.
/// </summary>
internal sealed record MediaInput
{
	/// <summary>
	/// Local path or URL of the file.
	/// </summary>
	public required string Input { get; init; }

	/// <summary>
	/// HTTP headers needed to read the URL, e.g. <c>Authorization</c>.
	/// </summary>
	/// <remarks>
	/// Values may contain credentials and must not be logged.
	/// </remarks>
	public Dictionary<string, string> Headers { get; init; } = [];
}

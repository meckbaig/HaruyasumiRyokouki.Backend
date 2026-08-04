namespace HaruyasumiRyokouki.Backend.Models.InternalDtos;

/// <summary>
/// Data transfer model representing means to access a file,
/// </summary>
internal sealed record MediaInput
{
	/// <summary>
	/// Full path to file.
	/// </summary>
	public required string Input { get; init; }

	/// <summary>
	/// Request headers if applicable.
	/// </summary>
	public Dictionary<string, string> Headers { get; init; } = [];
}

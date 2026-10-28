namespace HaruyasumiRyokouki.Backend.Common.Options;

/// <summary>
/// Options of the <c>FfmpegPresets</c> configuration section.
/// </summary>
/// <remarks>Every subsection holds user-defined named presets. <see cref="MediaFormatOptions"/> selects one of each by name.</remarks>
sealed class FfmpegPresetsOptions
{
	public const string ConfigurationSectionName = "FfmpegPresets";

	/// <summary>
	/// Video presets: each is a list of ffmpeg runs (e.g. two passes). Tokens <c>{input}</c>, <c>{output}</c>.
	/// </summary>
	public required Dictionary<string, string[]> Video { get; set; }

	/// <summary>
	/// Image presets. The name is also the output extension. Tokens <c>{input}</c>, <c>{output}</c>, <c>{videoThumbnailPrefix}</c>.
	/// </summary>
	public required Dictionary<string, string> Image { get; set; }

	/// <summary>
	/// Arguments inserted into an image preset to pick the frame of a video preview.
	/// </summary>
	public required Dictionary<string, string> VideoThumbnailPrefix { get; set; }

	/// <summary>
	/// Placeholder image presets writing to stdout. Tokens <c>{input}</c>, <c>{miniatureSize}</c>.
	/// </summary>
	public required Dictionary<string, string> Miniature { get; set; }
}


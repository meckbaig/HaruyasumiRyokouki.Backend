namespace HaruyasumiRyokouki.Backend.Common.Options;

sealed class FfmpegPresetsOptions
{
	public const string ConfigurationSectionName = "FfmpegPresets";

	public Dictionary<string, string[]> Video { get; set; }

	public Dictionary<string, string> Image { get; set; }

	public Dictionary<string, string> VideoThumbnailPrefix { get; set; }

	public Dictionary<string, string> Miniature { get; set; }
}


namespace HaruyasumiRyokouki.Backend.Common.Options;

sealed class MediaPresetsOptions
{
	public const string ConfigurationSectionName = "MediaPresets";

	public Dictionary<string, string[]> Video { get; set; }

	public Dictionary<string, string> Image { get; set; }

	public Dictionary<string, string> VideoThumbnailPrefix { get; set; }

	public Dictionary<string, string> Miniature { get; set; }
}

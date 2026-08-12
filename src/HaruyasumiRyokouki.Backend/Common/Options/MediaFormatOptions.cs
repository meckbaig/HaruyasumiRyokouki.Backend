namespace HaruyasumiRyokouki.Backend.Common.Options;

sealed class MediaFormatOptions
{
	public const string ConfigurationSectionName = "MediaFormat";

	public required string VideoPreset { get; set; }

	public required string VideoThumbnailPreset { get; set; }

	public required string ImagePreset { get; set; }

	public required string MiniaturePreset { get; set; }

	public required int MiniatureSize { get; set; }

	public required int FavoritesReturnCount { get; set; }
}

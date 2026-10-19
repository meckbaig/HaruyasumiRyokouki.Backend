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

	/// <summary>
	/// Video bitrate (bits per second) above which a lower bitrate version for streaming
	/// (<c>*_web</c>) is created during sync. Default is 25 * 1024 * 1024.
	/// </summary>
	public long VideoConversionBitrateThreshold { get; set; } = 25 * 1024 * 1024;

	public IReadOnlyCollection<TravelDateOffset> TravelDateOffsets { get; set; } = [];

	public sealed class TravelDateOffset
	{
		public DateTime? From { get; set; }
		public DateTime? To { get; set; }
		public double Offset { get; set; }
	}
}

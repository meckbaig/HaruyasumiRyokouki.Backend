namespace HaruyasumiRyokouki.Backend.Common.Options;

/// <summary>
/// Options of the <c>MediaFormat</c> configuration section; see the Configuration section of README.
/// </summary>
sealed class MediaFormatOptions
{
	public const string ConfigurationSectionName = "MediaFormat";

	/// <summary>
	/// Name of the <see cref="FfmpegPresetsOptions.Video"/> preset for the lower bitrate versions of videos.
	/// </summary>
	public required string VideoPreset { get; set; }

	/// <summary>
	/// Name of the <see cref="FfmpegPresetsOptions.VideoThumbnailPrefix"/> preset for video previews.
	/// </summary>
	public required string VideoThumbnailPreset { get; set; }

	/// <summary>
	/// Name of the <see cref="FfmpegPresetsOptions.Image"/> preset; also the target image format and extension.
	/// </summary>
	public required string ImagePreset { get; set; }

	/// <summary>
	/// Name of the <see cref="FfmpegPresetsOptions.Miniature"/> preset.
	/// </summary>
	public required string MiniaturePreset { get; set; }

	/// <summary>
	/// Size of the placeholder image in pixels.
	/// </summary>
	public required int MiniatureSize { get; set; }

	/// <summary>
	/// Number of random favorites returned by <c>GET /v1/media/favorites</c>.
	/// </summary>
	public required int FavoritesReturnCount { get; set; }

	/// <summary>
	/// Video bitrate (bits per second) above which a lower bitrate version for streaming
	/// (<c>*_web</c>) is created during sync. Default is 25 * 1024 * 1024.
	/// </summary>
	public long VideoConversionBitrateThreshold { get; set; } = 25 * 1024 * 1024;

	/// <summary>
	/// Time zone offsets of the trip by UTC ranges; converts file modification and YouTube publish times to local time.
	/// </summary>
	public IReadOnlyCollection<TravelDateOffset> TravelDateOffsets { get; set; } = [];

	/// <summary>
	/// UTC range with the time zone offset of the trip in it.
	/// </summary>
	public sealed class TravelDateOffset
	{
		/// <summary>
		/// Start of the range in UTC, inclusive; <see langword="null"/> for no start.
		/// </summary>
		public DateTime? From { get; set; }

		/// <summary>
		/// End of the range in UTC, inclusive; <see langword="null"/> for no end.
		/// </summary>
		public DateTime? To { get; set; }

		/// <summary>
		/// Offset of the local time from UTC in hours.
		/// </summary>
		public double Offset { get; set; }
	}
}

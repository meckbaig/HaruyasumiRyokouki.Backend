using HaruyasumiRyokouki.Backend.Models.InternalDtos;

namespace HaruyasumiRyokouki.Backend.Services.Interfaces;

/// <summary>
/// Runs <c>ffmpeg</c> and <c>ffprobe</c> with the presets selected in <c>MediaFormat</c>.
/// Failures of the tools are thrown as exceptions with their error output.
/// </summary>
internal interface IFfmpegService
{
	/// <summary>
	/// Converts an image (or a frame) with the <c>MediaFormat:ImagePreset</c> preset.
	/// </summary>
	/// <param name="input">Local input file.</param>
	/// <param name="output">Local output file. Its extension should match the preset.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	Task ConvertImageAsync(string input, string output, CancellationToken cancellationToken = default);

	/// <summary>
	/// Saves one frame of a video as an image: the <c>MediaFormat:ImagePreset</c> preset with the <c>MediaFormat:VideoThumbnailPreset</c> frame selection.
	/// </summary>
	/// <param name="input">Local video file.</param>
	/// <param name="output">Local output image.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	Task CreateVideoPreviewAsync(string input, string output, CancellationToken cancellationToken = default);

	/// <summary>
	/// Converts a video with all passes of the <c>MediaFormat:VideoPreset</c> preset.
	/// </summary>
	/// <param name="input">Local input video.</param>
	/// <param name="output">Local output video.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	Task ConvertVideoAsync(string input, string output, CancellationToken cancellationToken = default);

	/// <summary>
	/// Reads bitrate, duration and size of a video with ffprobe.
	/// </summary>
	/// <param name="mediaInput">Local path or URL with headers, from <see cref="IFileStorage.GetMediaInputAsync"/>.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	Task<VideoFileInfo> GetVideoInfoAsync(MediaInput mediaInput, CancellationToken cancellationToken = default);

	/// <summary>
	/// Creates the tiny placeholder image with the <c>MediaFormat:MiniaturePreset</c> preset.
	/// </summary>
	/// <param name="input">Local image file.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>Bytes of the image, read from ffmpeg output.</returns>
	Task<byte[]> GetImageMiniatureBytesAsync(string input, CancellationToken cancellationToken);

	/// <summary>
	/// Returns width divided by height of an image, taking its rotation metadata into account.
	/// </summary>
	/// <param name="input">Local image file.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	Task<float> GetImageAspectRatioAsync(string input, CancellationToken cancellationToken);
}

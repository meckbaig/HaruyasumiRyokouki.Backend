using HaruyasumiRyokouki.Backend.Common.ResultType;
using HaruyasumiRyokouki.Backend.Models.InternalDtos;

namespace HaruyasumiRyokouki.Backend.Services.Interfaces
{
	/// <summary>
	/// Conversion of media files in the storage. Files are copied to a temporary folder, processed with ffmpeg
	/// and the results are uploaded back.
	/// </summary>
	internal interface IMediaProcessorService
	{
		/// <summary>
		/// Converts an image to the <c>MediaFormat:ImagePreset</c> format, replacing the original in the storage
		/// (skipped when it already has that format), and creates its miniature.
		/// </summary>
		/// <returns>New file name, miniature and aspect ratio, without additional files.</returns>
		Task<Result<ConversionResponseDto>> ConvertImageAsync(string fileName, CancellationToken cancellationToken);

		/// <summary>
		/// Creates the preview image of a video and, when the bitrate is above the threshold, a lower bitrate <c>_web</c> version.
		/// The original video is kept.
		/// </summary>
		/// <returns>The original file name, miniature, aspect ratio of the preview and the names of the created files.</returns>
		Task<Result<ConversionResponseDto>> ConvertVideoAsync(string fileName, CancellationToken cancellationToken);

		/// <summary>
		/// Creates the miniature of an image already in the storage.
		/// </summary>
		/// <returns>Base64-encoded miniature, or a failure when the file is not an image.</returns>
		Task<Result<string>> CreateMiniatureAsync(string fileName, CancellationToken cancellationToken);

		/// <summary>
		/// Converts a downloaded YouTube thumbnail to <c>&lt;videoId&gt;_preview.&lt;image preset&gt;</c> and uploads it.
		/// </summary>
		/// <param name="videoId">YouTube video id.</param>
		/// <param name="sourceExtension">Extension of the thumbnail, for ffmpeg to detect the format.</param>
		/// <param name="thumbnail">Thumbnail content.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>Name of the uploaded preview.</returns>
		Task<Result<string>> CreateYoutubePreviewAsync(string videoId, string sourceExtension, Stream thumbnail, CancellationToken cancellationToken);
	}
}

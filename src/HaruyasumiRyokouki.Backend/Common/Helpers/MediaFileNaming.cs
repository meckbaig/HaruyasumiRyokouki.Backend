using HaruyasumiRyokouki.Backend.Models.Db.Enums;

namespace HaruyasumiRyokouki.Backend.Common.Helpers;

/// <summary>
/// Media type detection by file extension and naming of the additional files created for videos.
/// </summary>
internal static class MediaFileNaming
{
	private const string VideoWebSuffix = "_web";
	private const string VideoPreviewSuffix = "_preview";

	private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
	{
		".jpg", ".jpeg", ".png", ".bmp", ".webp", ".tif", ".tiff", ".heic", ".heif", ".avif"
	};

	private static readonly HashSet<string> VideoExtensions = new(StringComparer.OrdinalIgnoreCase)
	{
		".mp4", ".mov", ".avi", ".mkv", ".wmv", ".flv", ".webm", ".m4v", ".3gp", ".mpeg", ".mpg"
	};

	/// <summary>
	/// Returns <see langword="true"/> if the file has an image extension (case-insensitive).
	/// </summary>
	public static bool IsImage(string fileName)
	{
		return ImageExtensions.Contains(Path.GetExtension(fileName));
	}

	/// <summary>
	/// Returns <see langword="true"/> if the file has a video extension (case-insensitive).
	/// </summary>
	public static bool IsVideo(string fileName)
	{
		return VideoExtensions.Contains(Path.GetExtension(fileName));
	}

	/// <summary>
	/// Detects the media type by the file extension.
	/// </summary>
	public static MediaType GetMediaType(string fileName)
	{
		if (IsImage(fileName))
			return MediaType.Image;

		if (IsVideo(fileName))
			return MediaType.Video;

		return MediaType.Unknown;
	}

	/// <summary>
	/// Returns the name of the streaming version of a video: <c>clip.mp4</c> → <c>clip_web.mp4</c>.
	/// A name that already has the suffix is returned as is.
	/// </summary>
	public static string GetVideoWebName(string fileName)
	{
		if (Path.GetFileNameWithoutExtension(fileName).EndsWith(VideoWebSuffix))
			return fileName;
		return Path.GetFileNameWithoutExtension(fileName) + VideoWebSuffix + Path.GetExtension(fileName);
	}

	/// <summary>
	/// Returns the name of the preview image of a video: <c>clip.mp4</c> → <c>clip_preview.webp</c>.
	/// A name that already has the suffix is returned as is.
	/// </summary>
	/// <param name="fileName">Video file name (or YouTube video id).</param>
	/// <param name="imagePreset">Image preset name from <c>MediaFormat:ImagePreset</c>, used as the extension.</param>
	public static string GetVideoPreviewName(string fileName, string imagePreset)
	{
		if (Path.GetFileNameWithoutExtension(fileName).EndsWith(VideoPreviewSuffix))
			return fileName;
		return Path.GetFileNameWithoutExtension(fileName) + VideoPreviewSuffix + "." + imagePreset.ToLower();
	}
}

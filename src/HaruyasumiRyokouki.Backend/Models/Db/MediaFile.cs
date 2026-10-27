using HaruyasumiRyokouki.Backend.Models.Db.Enums;

namespace HaruyasumiRyokouki.Backend.Models.Db;

/// <summary>
/// Photo or video of the trip.
/// </summary>
/// <remarks>
/// Media files are not created directly: they come from sync with the file storage or from YouTube import.
/// </remarks>
public class MediaFile
{
	/// <summary>
	/// Identifier of the media file.
	/// </summary>
	public int Id { get; set; }

	/// <summary>
	/// Identifier of the <see cref="Db.Day"/>.
	/// </summary>
	public int DayId { get; set; }

	/// <summary>
	/// Time the photo or video was taken, in the local time of the trip.
	/// </summary>
	/// <remarks>
	/// Not UTC: taken from the file name or converted with <c>MediaFormat:TravelDateOffsets</c>. Defines the day of the media and its order within the day.
	/// </remarks>
	public required DateTime Created { get; set; }

	/// <summary>
	/// File name in the file storage.
	/// </summary>
	/// <remarks>
	/// For YouTube media it is the video id. The only stored file is the preview from <see cref="AdditionalFiles"/>.
	/// </remarks>
	public required string FileName { get; set; } = null!;

	/// <summary>
	/// Width divided by height, with the rotation of the image taken into account.
	/// </summary>
	public required float AspectRatio { get; set; }

	/// <summary>
	/// Image or video.
	/// </summary>
	public required MediaType Type { get; set; } = MediaType.Unknown;

	/// <summary>
	/// Where the media comes from: the file storage or YouTube.
	/// </summary>
	public MediaSource Source { get; set; } = MediaSource.Local;

	/// <summary>
	/// Embed URL of a YouTube video, or <see langword="null"/> for media from the file storage.
	/// </summary>
	public string? ExternalUrl { get; set; }

	/// <summary>
	/// Latitude in degrees, or <see langword="null"/> when the place is not set.
	/// </summary>
	public double? Latitude { get; set; }

	/// <summary>
	/// Longitude in degrees, or <see langword="null"/> when the place is not set.
	/// </summary>
	public double? Longitude { get; set; }

	/// <summary>
	/// Tiny blurred placeholder of the image, base64-encoded, shown while the real image loads.
	/// </summary>
	/// <remarks>
	/// Re-created by sync when missing.
	/// </remarks>
	public string? Miniature { get; set; }

	/// <summary>
	/// The media has been reviewed by the editor: its title, description, tags and location are final.
	/// </summary>
	/// <remarks>
	/// New media are not approved. Visitors see only approved media.
	/// </remarks>
	public bool IsApproved { get; set; }

	/// <summary>
	/// Hidden media: interesting, but not for visitors. Visible only to the admin, even when approved.
	/// </summary>
	public bool Private { get; set; } = false;

	/// <summary>
	/// Favorite media, shown at random on the home page.
	/// </summary>
	public bool Favorite { get; set; } = false;

	/// <summary>
	/// Other files of the media in the file storage.
	/// </summary>
	/// <remarks>
	/// For videos: the preview <c>&lt;name&gt;_preview.&lt;image preset&gt;</c> and, for a high bitrate, the streaming version <c>&lt;name&gt;_web.&lt;ext&gt;</c>. They are deleted together with the media.
	/// </remarks>
	public ICollection<string> AdditionalFiles { get; set; } = [];

	/// <summary>
	/// Title and description, one translation per language.
	/// </summary>
	public ICollection<MediaTranslation> Translations { get; set; } = [];

	/// <summary>
	/// Rows of the media-tag join table.
	/// </summary>
	/// <remarks>
	/// Used to change the tags of the media. To read them, use <see cref="Tags"/>.
	/// </remarks>
	public ICollection<MediaFileTag> MediaTags { get; set; } = [];

	/// <summary>
	/// Tags of the media.
	/// </summary>
	/// <remarks>
	/// Many-to-many through <see cref="MediaTags"/>.
	/// </remarks>
	public ICollection<Tag> Tags { get; set; } = [];

	/// <summary>
	/// The day the media was taken.
	/// </summary>
	public Day Day { get; set; }
}

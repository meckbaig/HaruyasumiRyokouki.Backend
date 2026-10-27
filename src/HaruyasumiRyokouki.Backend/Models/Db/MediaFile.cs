using HaruyasumiRyokouki.Backend.Models.Db.Enums;

namespace HaruyasumiRyokouki.Backend.Models.Db;

/// <summary>
/// Photo or video. Created by sync or YouTube import.
/// </summary>
public class MediaFile
{
	/// <summary>
	/// Id.
	/// </summary>
	public int Id { get; set; }

	/// <summary>
	/// Day id.
	/// </summary>
	public int DayId { get; set; }

	/// <summary>
	/// Creation time in the local time of the trip (not UTC); defines the day and the order within it.
	/// </summary>
	public required DateTime Created { get; set; }

	/// <summary>
	/// File name in the storage; the video id for YouTube media.
	/// </summary>
	public required string FileName { get; set; } = null!;

	/// <summary>
	/// Width divided by height.
	/// </summary>
	public required float AspectRatio { get; set; }

	/// <summary>
	/// Image or video.
	/// </summary>
	public required MediaType Type { get; set; } = MediaType.Unknown;

	/// <summary>
	/// Local file or YouTube.
	/// </summary>
	public MediaSource Source { get; set; } = MediaSource.Local;

	/// <summary>
	/// Embed URL of a YouTube video; <see langword="null"/> for local media.
	/// </summary>
	public string? ExternalUrl { get; set; }

	/// <summary>
	/// Latitude in degrees, <see langword="null"/> when unknown.
	/// </summary>
	public double? Latitude { get; set; }

	/// <summary>
	/// Longitude in degrees, <see langword="null"/> when unknown.
	/// </summary>
	public double? Longitude { get; set; }

	/// <summary>
	/// Tiny placeholder image, base64-encoded. Re-created by sync when missing.
	/// </summary>
	public string? Miniature { get; set; }

	/// <summary>
	/// Approved by the admin; visitors see only approved media.
	/// </summary>
	public bool IsApproved { get; set; }

	/// <summary>
	/// Hidden from visitors even when approved.
	/// </summary>
	public bool Private { get; set; } = false;

	/// <summary>
	/// Shown on the front page.
	/// </summary>
	public bool Favorite { get; set; } = false;

	/// <summary>
	/// Other files of the media in the storage: <c>&lt;name&gt;_preview.&lt;image preset&gt;</c> and <c>&lt;name&gt;_web.&lt;ext&gt;</c> of videos.
	/// Deleted together with the media.
	/// </summary>
	public ICollection<string> AdditionalFiles { get; set; } = [];

	/// <summary>
	/// Titles and descriptions, one per language.
	/// </summary>
	public ICollection<MediaTranslation> Translations { get; set; } = [];

	/// <summary>
	/// Rows of the media-tag join table; used to change the tags.
	/// </summary>
	public ICollection<MediaFileTag> MediaTags { get; set; } = [];

	/// <summary>
	/// Tags of the media (many-to-many through <see cref="MediaTags"/>); used to read them.
	/// </summary>
	public ICollection<Tag> Tags { get; set; } = [];

	/// <summary>
	/// The day.
	/// </summary>
	public Day Day { get; set; }
}

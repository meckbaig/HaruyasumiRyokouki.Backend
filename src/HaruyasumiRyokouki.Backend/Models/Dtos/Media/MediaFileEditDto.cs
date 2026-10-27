using HaruyasumiRyokouki.Backend.Models.Dtos.Tags;

namespace HaruyasumiRyokouki.Backend.Models.Dtos.Media;

/// <summary>
/// Photo or video with texts in all languages, for the editor.
/// </summary>
public record MediaFileEditDto : IPreviewDto
{
	/// <summary>
	/// Identifier of the media file.
	/// </summary>
	public int Id { get; set; }

	/// <summary>
	/// Time the photo or video was taken, in the local time of the trip.
	/// </summary>
	/// <remarks>
	/// Not UTC: taken from the file name or converted with <c>MediaFormat:TravelDateOffsets</c>. Defines the day of the media and its order within the day.
	/// </remarks>
	public DateTime Created { get; set; }

	/// <summary>
	/// File name in the file storage.
	/// </summary>
	/// <remarks>
	/// For YouTube media it is the video id. The only stored file is the preview from <see cref="Db.MediaFile.AdditionalFiles"/>.
	/// </remarks>
	public required string FileName { get; set; } = null!;

	/// <summary>
	/// Width divided by height, with the rotation of the image taken into account.
	/// </summary>
	public float AspectRatio { get; set; }

	/// <summary>
	/// <see cref="Db.Enums.MediaType.Image"/> or <see cref="Db.Enums.MediaType.Video"/> as a string.
	/// </summary>
	public string Type { get; set; }

	/// <summary>
	/// <see cref="Db.Enums.MediaSource.Local"/> or <see cref="Db.Enums.MediaSource.YouTube"/> as a string.
	/// </summary>
	public string Source { get; set; }

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
	public string Miniature { get; set; }

	/// <summary>
	/// Hidden media: interesting, but not for visitors. Visible only to the admin, even when approved.
	/// </summary>
	public bool Private { get; set; }

	/// <summary>
	/// Favorite media, shown at random on the home page.
	/// </summary>
	public bool Favorite { get; set; }

	/// <summary>
	/// The media has been reviewed by the editor: its title, description, tags and location are final.
	/// </summary>
	/// <remarks>
	/// New media are not approved.
	/// </remarks>
	public bool IsApproved { get; set; }

	/// <summary>
	/// Titles and descriptions, at most one translation per language.
	/// </summary>
	public ICollection<MediaTranslationEditDto> Translations { get; set; } = [];

	/// <summary>
	/// Tags of the media with display names in the requested language.
	/// </summary>
	public ICollection<TagPublicDto> Tags { get; set; } = [];

	/// <summary>
	/// URLs of an image, or <see langword="null"/> for a video.
	/// </summary>
	/// <remarks>
	/// Omitted from the response when <see langword="null"/>.
	/// </remarks>
	public ImageUrlsDto? ImageUrls { get; set; } = null;

	/// <summary>
	/// URLs of a video, or <see langword="null"/> for an image.
	/// </summary>
	/// <remarks>
	/// Omitted from the response when <see langword="null"/>.
	/// </remarks>
	public VideoUrlsDto? VideoUrls { get; set; } = null;
}

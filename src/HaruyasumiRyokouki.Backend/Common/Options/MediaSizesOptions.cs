namespace HaruyasumiRyokouki.Backend.Common.Options;

/// <summary>
/// Options of the <c>MediaSizes</c> configuration section.
/// </summary>
sealed class MediaSizesOptions
{
	public const string ConfigurationSectionName = "MediaSizes";

	private IReadOnlyCollection<int> _sizeBuckets;

	/// <summary>
	/// Allowed image sizes in pixels, sorted on assignment.
	/// </summary>
	public IReadOnlyCollection<int> SizeBuckets { get => _sizeBuckets; set => _sizeBuckets = value.Order().ToArray(); }

	/// <summary>
	/// Thumbnail size in CSS pixels.
	/// </summary>
	public int PreviewTargetCss { get; set; }

	/// <summary>
	/// Smaller side of the screen in pixels when the client sends no <c>X-Display</c>.
	/// </summary>
	public int DefaultScreenResolution { get; set; }

	/// <summary>
	/// Aspect ratio used when it is unknown.
	/// </summary>
	public float DefaultAspectRatio { get; set; } = 4f / 3f;
}

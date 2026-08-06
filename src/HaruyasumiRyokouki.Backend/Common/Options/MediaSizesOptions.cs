namespace HaruyasumiRyokouki.Backend.Common.Options;

sealed class MediaSizesOptions
{
	public const string ConfigurationSectionName = "MediaSizes";

	private IReadOnlyCollection<int> _sizeBuckets;

	public IReadOnlyCollection<int> SizeBuckets { get => _sizeBuckets; set => _sizeBuckets = value.Order().ToArray(); }
	public int PreviewTargetCss { get; set; }
	public int DefaultScreenResolution { get; set; }
}

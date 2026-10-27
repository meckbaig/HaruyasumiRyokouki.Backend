
namespace HaruyasumiRyokouki.Backend.Services.Builders;

/// <summary>
/// URLs of original files. The template needs <c>{fileName}</c>.
/// </summary>
public class OriginStorageUrlBuilder : AbstractUrlBuilder
{
	/// <inheritdoc/>
	public override HashSet<string> RequiredTokens =>
	[
		"fileName"
	];

	/// <inheritdoc/>
	public OriginStorageUrlBuilder(string endpoint, string? cdnEndpoint, string payloadStringBase, bool useCdnForDownloads = false)
		: base(endpoint, cdnEndpoint, payloadStringBase, useCdnForDownloads)
	{

	}

	/// <summary>
	/// URL for viewing or streaming, through the CDN when configured. The file name is escaped.
	/// </summary>
	public string BuildMedia(string fileName)
		=> Build(MediaBase, fileName);

	/// <summary>
	/// URL for downloading. The file name is escaped.
	/// </summary>
	public string BuildDownload(string fileName)
		=> Build(DownloadBase, fileName);

	private string Build(string urlBase, string fileName)
	{
		return urlBase.Replace("{fileName}", Uri.EscapeDataString(fileName));
	}
}

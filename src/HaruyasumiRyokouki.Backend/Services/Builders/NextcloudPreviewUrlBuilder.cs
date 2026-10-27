namespace HaruyasumiRyokouki.Backend.Services.Builders;

/// <summary>
/// URLs of Nextcloud previews; the template needs <c>{fileName}</c>, <c>{xAxis}</c> and <c>{yAxis}</c>.
/// </summary>
public class NextcloudPreviewUrlBuilder : AbstractUrlBuilder
{
	/// <inheritdoc/>
	public override HashSet<string> RequiredTokens =>
	[
		"fileName",
		"xAxis",
		"yAxis"
	];

	/// <inheritdoc/>
	public NextcloudPreviewUrlBuilder(string endpoint, string? cdnEndpoint, string payloadStringBase, bool useCdnForDownloads = false)
		: base(endpoint, cdnEndpoint, payloadStringBase, useCdnForDownloads)
	{
	}

	/// <summary>
	/// Preview URL of the size, through the CDN when configured.
	/// </summary>
	public string BuildMedia(string fileName, int xAxis, int yAxis)
		=> Build(MediaBase, fileName, xAxis, yAxis);

	/// <summary>
	/// Preview URL of the size for downloading.
	/// </summary>
	public string BuildDownload(string fileName, int xAxis, int yAxis)
		=> Build(DownloadBase, fileName, xAxis, yAxis);

	private string Build(string urlBase, string fileName, int xAxis, int yAxis)
	{
		return urlBase
			.Replace("{fileName}", Uri.EscapeDataString(fileName))
			.Replace("{xAxis}", xAxis.ToString())
			.Replace("{yAxis}", yAxis.ToString());
	}
}

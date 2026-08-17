namespace HaruyasumiRyokouki.Backend.Services.Builders;

public class NextcloudPreviewUrlBuilder : AbstractUrlBuilder
{
	public override HashSet<string> RequiredTokens =>
	[
		"fileName",
		"xAxis",
		"yAxis"
	];

	public NextcloudPreviewUrlBuilder(string endpoint, string? cdnEndpoint, string payloadStringBase, bool useCdnForDownloads = false)
		: base(endpoint, cdnEndpoint, payloadStringBase, useCdnForDownloads)
	{
	}

	public string BuildMedia(string fileName, int xAxis, int yAxis)
		=> Build(MediaBase, fileName, xAxis, yAxis);
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

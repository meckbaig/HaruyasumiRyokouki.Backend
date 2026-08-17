
namespace HaruyasumiRyokouki.Backend.Services.Builders;

public class OriginStorageUrlBuilder : AbstractUrlBuilder
{
	public override HashSet<string> RequiredTokens =>
	[
		"fileName"
	];

	public OriginStorageUrlBuilder(string endpoint, string? cdnEndpoint, string payloadStringBase, bool useCdnForDownloads = false)
		: base(endpoint, cdnEndpoint, payloadStringBase, useCdnForDownloads)
	{

	}

	public override bool Validate(out IReadOnlyCollection<string> missingTokens)
	{
		return base.Validate(out missingTokens);
	}

	public string BuildMedia(string fileName)
		=> Build(MediaBase, fileName);

	public string BuildDownload(string fileName)
		=> Build(DownloadBase, fileName);

	private string Build(string urlBase, string fileName)
	{
		return urlBase.Replace("{fileName}", Uri.EscapeDataString(fileName));
	}
}

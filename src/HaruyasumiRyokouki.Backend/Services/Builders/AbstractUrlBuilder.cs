using System.Text.RegularExpressions;

namespace HaruyasumiRyokouki.Backend.Services.Builders;

public abstract class AbstractUrlBuilder
{
	protected static readonly Regex TokenRegex = new(@"\{(?<token>[^\}]+)\}", RegexOptions.Compiled);
	public abstract HashSet<string> RequiredTokens { get; }

	protected readonly string _mediaEndpoint;
	protected readonly string _downloadEndpoint;
	protected readonly string _payloadStringBase;

	protected string MediaBase => CombineUrl(_mediaEndpoint, _payloadStringBase);
	protected string DownloadBase => CombineUrl(_downloadEndpoint, _payloadStringBase);

	protected AbstractUrlBuilder(string endpoint, string? cdnEndpoint, string payloadStringBase, bool useCdnForDownloads = false)
	{
		_mediaEndpoint = !string.IsNullOrEmpty(cdnEndpoint)
			? cdnEndpoint
			: endpoint;
		_downloadEndpoint = !string.IsNullOrEmpty(cdnEndpoint) && useCdnForDownloads
			? cdnEndpoint
			: endpoint;
		_payloadStringBase = payloadStringBase;
	}

	public virtual bool Validate(out IReadOnlyCollection<string> missingTokens)
	{
		var tokensMedia = ExtractTokens(_payloadStringBase);
		missingTokens = RequiredTokens.Where(x => !tokensMedia.Contains(x)).ToArray();
		return missingTokens.Count == 0;
	}

	private HashSet<string> ExtractTokens(string urlBase)
	{
		return TokenRegex
			.Matches(urlBase)
			.Select(x => x.Groups["token"].Value)
			.ToHashSet();
	}

	static string CombineUrl(params string[] parts) 
		=> string.Join('/',	parts.Select(p => p.Trim('/')));	
}

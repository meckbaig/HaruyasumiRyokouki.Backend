using System.Text.RegularExpressions;

namespace HaruyasumiRyokouki.Backend.Services.Builders;

/// <summary>
/// Builds URLs from an endpoint and a path template with <c>{token}</c> placeholders.
/// </summary>
public abstract class AbstractUrlBuilder
{
	/// <summary>
	/// Matches <c>{token}</c> placeholders.
	/// </summary>
	protected static readonly Regex TokenRegex = new(@"\{(?<token>[^\}]+)\}", RegexOptions.Compiled);

	/// <summary>
	/// Tokens the template must contain.
	/// </summary>
	public abstract HashSet<string> RequiredTokens { get; }

	/// <summary>
	/// Endpoint for media URLs: the CDN when configured.
	/// </summary>
	protected readonly string _mediaEndpoint;

	/// <summary>
	/// Endpoint for download URLs: the CDN only with <c>useCdnForDownloads</c>.
	/// </summary>
	protected readonly string _downloadEndpoint;

	/// <summary>
	/// Path template.
	/// </summary>
	protected readonly string _payloadStringBase;

	/// <summary>
	/// Media endpoint joined with the template.
	/// </summary>
	protected string MediaBase => CombineUrl(_mediaEndpoint, _payloadStringBase);

	/// <summary>
	/// Download endpoint joined with the template.
	/// </summary>
	protected string DownloadBase => CombineUrl(_downloadEndpoint, _payloadStringBase);

	/// <summary>
	/// Creates a builder.
	/// </summary>
	/// <param name="endpoint">Base URL of the server.</param>
	/// <param name="cdnEndpoint">Base URL of a CDN, or empty.</param>
	/// <param name="payloadStringBase">Path template with tokens.</param>
	/// <param name="useCdnForDownloads">Use the CDN for download URLs too.</param>
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

	/// <summary>
	/// Checks that the template contains all <see cref="RequiredTokens"/>.
	/// </summary>
	/// <param name="missingTokens">Required tokens absent from the template.</param>
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

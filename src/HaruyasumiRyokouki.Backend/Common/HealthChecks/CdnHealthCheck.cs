using HaruyasumiRyokouki.Backend.Common.Options;
using HaruyasumiRyokouki.Backend.DbContexts;
using HaruyasumiRyokouki.Backend.Models.Db.Enums;
using HaruyasumiRyokouki.Backend.Models.InternalDtos;
using HaruyasumiRyokouki.Backend.Models.InternalDtos.Enums;
using HaruyasumiRyokouki.Backend.Services.Builders;
using HaruyasumiRyokouki.Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using static HaruyasumiRyokouki.Backend.Common.Options.MediaPreviewOptions;

namespace HaruyasumiRyokouki.Backend.Common.HealthChecks;

/// <summary>
/// Checks that every configured CDN returns a real media file.
/// </summary>
/// <remarks>
/// The first local image from the database is requested through the CDN of <see cref="MediaPreviewOptions.OriginStorage"/>
/// as an original, and through the CDN of the active preview provider as the smallest preview.
/// Media URLs are built with the CDN once at startup, so a CDN that does not respond breaks the main part of the service.
/// </remarks>
internal class CdnHealthCheck : IHealthCheck
{
	private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

	/// <summary>
	/// Screen parameters that give the smallest preview size bucket.
	/// </summary>
	private static readonly ClientDisplay SmallestDisplay = new() { Dpr = 0.01f, MinSide = 1 };

	private readonly IAppDbContext _context;
	private readonly IMediaPreviewService _previewService;
	private readonly IHttpClientFactory _httpClientFactory;
	private readonly MediaPreviewOptions _options;

	public CdnHealthCheck(IAppDbContext context, IMediaPreviewService previewService, IHttpClientFactory httpClientFactory, IOptions<MediaPreviewOptions> options)
	{
		_context = context;
		_previewService = previewService;
		_httpClientFactory = httpClientFactory;
		_options = options.Value;
	}

	public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
	{
		bool originCdn = !string.IsNullOrEmpty(_options.OriginStorage.CdnEndpoint);
		string? previewCdnEndpoint = _options.Provider switch
		{
			MediaPreviewProvider.Imgproxy => _options.Imgproxy?.CdnEndpoint,
			MediaPreviewProvider.Nextcloud => _options.Nextcloud?.CdnEndpoint,
			_ => null
		};
		bool previewCdn = !string.IsNullOrEmpty(previewCdnEndpoint);

		if (!originCdn && !previewCdn)
			return HealthCheckResult.Healthy("No CDN configured.");

		string? fileName = await _context.MediaFiles
			.AsNoTracking()
			.Where(m => m.Source == MediaSource.Local && m.Type == MediaType.Image)
			.OrderBy(m => m.Id)
			.Select(m => m.FileName)
			.FirstOrDefaultAsync(cancellationToken);

		if (fileName == null)
			return HealthCheckResult.Healthy("No media to check.");

		var failures = new List<string>();
		using var client = _httpClientFactory.CreateClient();
		client.Timeout = Timeout;

		if (originCdn)
		{
			var originBuilder = new OriginStorageUrlBuilder(
				_options.OriginStorage.Endpoint,
				_options.OriginStorage.CdnEndpoint,
				_options.OriginStorage.PayloadStringBase);
			await CheckUrlAsync(client, "Origin storage CDN", originBuilder.BuildMedia(fileName), failures, cancellationToken);
		}

		if (previewCdn)
		{
			string previewUrl = _previewService.GetImageUrl(fileName, ImageUrlType.Preview, SmallestDisplay, aspectRatio: 1);
			await CheckUrlAsync(client, $"{_options.Provider} CDN", previewUrl, failures, cancellationToken);
		}

		return failures.Count == 0
			? HealthCheckResult.Healthy($"CDN returned {fileName}.")
			: HealthCheckResult.Unhealthy(string.Join(" ", failures));
	}

	private static async Task CheckUrlAsync(HttpClient client, string name, string url, List<string> failures, CancellationToken cancellationToken)
	{
		try
		{
			using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
			if (!response.IsSuccessStatusCode)
				failures.Add($"{name} returned {(int)response.StatusCode} {response.ReasonPhrase}.");
		}
		catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException && !cancellationToken.IsCancellationRequested)
		{
			failures.Add($"{name} is not reachable: {ex.Message}");
		}
	}
}

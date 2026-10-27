using HaruyasumiRyokouki.Backend.Common.Options;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using static HaruyasumiRyokouki.Backend.Common.Options.MediaPreviewOptions;

namespace HaruyasumiRyokouki.Backend.Common.HealthChecks;

/// <summary>
/// Checks that the configured preview service answers on its status URL:
/// <c>/health</c> of imgproxy or <c>/status.php</c> of Nextcloud, relative to the provider <c>Endpoint</c>.
/// </summary>
internal class PreviewServiceHealthCheck : IHealthCheck
{
	private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

	private readonly IHttpClientFactory _httpClientFactory;
	private readonly MediaPreviewOptions _options;

	public PreviewServiceHealthCheck(IHttpClientFactory httpClientFactory, IOptions<MediaPreviewOptions> options)
	{
		_httpClientFactory = httpClientFactory;
		_options = options.Value;
	}

	public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
	{
		var (endpoint, statusPath) = _options.Provider switch
		{
			MediaPreviewProvider.Imgproxy => (_options.Imgproxy!.Endpoint, "health"),
			MediaPreviewProvider.Nextcloud => (_options.Nextcloud!.Endpoint, "status.php"),
			_ => throw new NotSupportedException($"Unsupported media preview provider: {_options.Provider}")
		};
		var url = new Uri(new Uri(endpoint.TrimEnd('/') + "/"), statusPath);

		using var client = _httpClientFactory.CreateClient();
		client.Timeout = Timeout;
		try
		{
			using var response = await client.GetAsync(url, cancellationToken);
			return response.IsSuccessStatusCode
				? HealthCheckResult.Healthy($"{_options.Provider} is reachable.")
				: HealthCheckResult.Unhealthy($"{_options.Provider} returned {(int)response.StatusCode} {response.ReasonPhrase}.");
		}
		catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException && !cancellationToken.IsCancellationRequested)
		{
			return HealthCheckResult.Unhealthy($"{_options.Provider} is not reachable.", ex);
		}
	}
}

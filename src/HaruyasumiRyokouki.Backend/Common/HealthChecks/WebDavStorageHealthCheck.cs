using Microsoft.Extensions.Diagnostics.HealthChecks;
using WebDav;

namespace HaruyasumiRyokouki.Backend.Common.HealthChecks;

/// <summary>
/// Checks that the media folder of the <c>WebDav</c> file storage is reachable with the configured credentials
/// (<c>PROPFIND</c> of the folder itself, without listing the files).
/// </summary>
internal class WebDavStorageHealthCheck : IHealthCheck
{
	private readonly WebDavClient _client;

	public WebDavStorageHealthCheck(WebDavClient client)
	{
		_client = client;
	}

	public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
	{
		var response = await _client.Propfind("", new PropfindParameters
		{
			ApplyTo = ApplyTo.Propfind.ResourceOnly,
			CancellationToken = cancellationToken
		});

		return response.IsSuccessful
			? HealthCheckResult.Healthy("WebDAV storage is reachable.")
			: HealthCheckResult.Unhealthy($"WebDAV storage returned {response.StatusCode} {response.Description}.");
	}
}

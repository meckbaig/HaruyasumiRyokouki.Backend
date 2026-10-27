using HaruyasumiRyokouki.Backend.Common.Options;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace HaruyasumiRyokouki.Backend.Common.HealthChecks;

/// <summary>
/// Checks that the media folder of the <see cref="Options.MediaStorageOptions.FileStorageProvider.Local"/> file storage exists.
/// </summary>
internal class LocalStorageHealthCheck : IHealthCheck
{
	private readonly LocalStorageOptions _options;

	public LocalStorageHealthCheck(IOptions<LocalStorageOptions> options)
	{
		_options = options.Value;
	}

	public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
	{
		var result = Directory.Exists(_options.Path)
			? HealthCheckResult.Healthy("Local storage folder exists.")
			: HealthCheckResult.Unhealthy("Local storage folder does not exist.");

		return Task.FromResult(result);
	}
}

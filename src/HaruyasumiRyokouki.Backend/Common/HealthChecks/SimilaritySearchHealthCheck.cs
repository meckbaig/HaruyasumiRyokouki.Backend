using HaruyasumiRyokouki.Backend.Services.Interfaces;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HaruyasumiRyokouki.Backend.Common.HealthChecks;

/// <summary>
/// Reports the state of the in-memory similarity index. Disabled search is healthy:
/// it is a configuration choice, not a failure. Enabled search without loaded vectors is unhealthy.
/// </summary>
internal class SimilaritySearchHealthCheck : IHealthCheck
{
	private readonly IMediaSimilarityIndexService _similarityService;

	public SimilaritySearchHealthCheck(IMediaSimilarityIndexService similarityService)
	{
		_similarityService = similarityService;
	}

	public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
	{
		var result = !_similarityService.IsEnabled
			? HealthCheckResult.Healthy("Disabled.")
			: _similarityService.IsLoaded
				? HealthCheckResult.Healthy($"{_similarityService.Count} vectors loaded.")
				: HealthCheckResult.Unhealthy("Enabled, but no vectors are loaded.");

		return Task.FromResult(result);
	}
}

using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HaruyasumiRyokouki.Backend.Common.HealthChecks;

/// <summary>
/// Tags of health check registrations.
/// </summary>
internal static class HealthCheckTags
{
	/// <summary>
	/// A component the main features do not depend on (similarity search, ffmpeg, yt-dlp).
	/// Its failure makes the overall status <see cref="HealthStatus.Degraded"/> instead of <see cref="HealthStatus.Unhealthy"/>.
	/// </summary>
	public const string Optional = "optional";

	/// <summary>
	/// Overall status: the worst status of the required checks, and at most <see cref="HealthStatus.Degraded"/>
	/// when only optional checks fail.
	/// </summary>
	public static HealthStatus GetOverallStatus(HealthReport report)
	{
		var overall = HealthStatus.Healthy;
		foreach (var entry in report.Entries.Values)
		{
			var status = entry.Tags.Contains(Optional) && entry.Status < HealthStatus.Healthy
				? HealthStatus.Degraded
				: entry.Status;
			if (status < overall)
				overall = status;
		}
		return overall;
	}
}

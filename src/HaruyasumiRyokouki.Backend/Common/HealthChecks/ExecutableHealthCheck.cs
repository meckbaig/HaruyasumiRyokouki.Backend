using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.ComponentModel;
using System.Diagnostics;

namespace HaruyasumiRyokouki.Backend.Common.HealthChecks;

/// <summary>
/// Checks that an external tool can be started: runs it with a version argument
/// and reports the first line of its output as the description.
/// </summary>
internal class ExecutableHealthCheck : IHealthCheck
{
	private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

	private readonly string _fileName;
	private readonly string _versionArgument;

	/// <param name="fileName">Executable name, resolved the same way as when the application runs the tool.</param>
	/// <param name="versionArgument">Argument that prints the version and exits, e.g. <c>-version</c>.</param>
	public ExecutableHealthCheck(string fileName, string versionArgument)
	{
		_fileName = fileName;
		_versionArgument = versionArgument;
	}

	public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
	{
		using var process = new Process();
		process.StartInfo.FileName = _fileName;
		process.StartInfo.ArgumentList.Add(_versionArgument);
		process.StartInfo.RedirectStandardOutput = true;
		process.StartInfo.RedirectStandardError = true;
		process.StartInfo.UseShellExecute = false;
		process.StartInfo.CreateNoWindow = true;

		try
		{
			process.Start();
		}
		catch (Win32Exception)
		{
			return HealthCheckResult.Unhealthy($"{_fileName} not found.");
		}

		using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		timeout.CancelAfter(Timeout);
		try
		{
			var outputTask = process.StandardOutput.ReadToEndAsync(timeout.Token);
			var errorTask = process.StandardError.ReadToEndAsync(timeout.Token);
			await Task.WhenAll(outputTask, errorTask);
			await process.WaitForExitAsync(timeout.Token);

			if (process.ExitCode != 0)
				return HealthCheckResult.Unhealthy($"{_fileName} exited with code {process.ExitCode}.");

			string version = outputTask.Result.Split('\n', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.Trim() ?? string.Empty;
			return HealthCheckResult.Healthy(version);
		}
		catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
		{
			process.Kill(entireProcessTree: true);
			return HealthCheckResult.Unhealthy($"{_fileName} did not respond in {Timeout.TotalSeconds} seconds.");
		}
	}
}

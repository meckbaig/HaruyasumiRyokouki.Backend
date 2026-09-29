using HaruyasumiRyokouki.Backend.Common.Options;
using HaruyasumiRyokouki.Backend.Common.ResultType;
using HaruyasumiRyokouki.Backend.Models.InternalDtos;
using HaruyasumiRyokouki.Backend.Services.Interfaces;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HaruyasumiRyokouki.Backend.Services;

internal class YoutubeMetadataService : IYoutubeMetadataService
{
	private const string YtDlpExecutable = "yt-dlp";

	private readonly ILogger<YoutubeMetadataService> _logger;
	private readonly YoutubeOptions _options;
	private readonly JsonSerializerOptions _serializerOptions = new() { PropertyNameCaseInsensitive = true };

	public YoutubeMetadataService(ILogger<YoutubeMetadataService> logger, IOptions<YoutubeOptions> options)
	{
		_logger = logger;
		_options = options.Value;
	}

	public async Task<Result<YoutubeMediaMetadata>> GetMetadataAsync(string videoId, CancellationToken cancellationToken)
	{
		var url = $"https://www.youtube.com/watch?v={videoId}";

		try
		{
			var json = await RunYtDlpAsync(url, cancellationToken);
			var response = JsonSerializer.Deserialize<YtDlpResponse>(json, _serializerOptions);

			if (response is null)
				return Result<YoutubeMediaMetadata>.Failure($"Unable to parse yt-dlp metadata for {videoId}.");

			var publishedAt = ResolvePublishedAt(response);
			if (publishedAt is null)
				return Result<YoutubeMediaMetadata>.Failure($"Unable to determine publish date for {videoId}.");

			var aspectRatio = ResolveAspectRatio(response);
			if (aspectRatio is null)
				return Result<YoutubeMediaMetadata>.Failure($"Unable to determine video resolution for {videoId}.");

			return Result<YoutubeMediaMetadata>.Success(new YoutubeMediaMetadata
			{
				VideoId = videoId,
				ExternalUrl = response.WebpageUrl ?? url,
				PublishedAtUtc = publishedAt.Value,
				ThumbnailUrl = response.Thumbnail ?? string.Empty,
				AspectRatio = aspectRatio.Value
			});
		}
		catch (Exception ex)
		{
			return Result<YoutubeMediaMetadata>.Failure($"Failed to fetch YouTube metadata for {videoId}: {ex.Message}");
		}
	}

	private static DateTime? ResolvePublishedAt(YtDlpResponse response)
	{
		if (response.Timestamp is long timestamp)
			return DateTimeOffset.FromUnixTimeSeconds(timestamp).UtcDateTime;

		if (!string.IsNullOrEmpty(response.UploadDate)
			&& DateTime.TryParseExact(response.UploadDate, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
		{
			return DateTime.SpecifyKind(date, DateTimeKind.Utc);
		}

		return null;
	}

	private static float? ResolveAspectRatio(YtDlpResponse response)
	{
		if (response.Width is int width && response.Height is int height && width > 0 && height > 0)
			return (float)width / height;

		var bestFormat = response.Formats?
			.Where(f => f.Width is > 0 && f.Height is > 0)
			.OrderByDescending(f => f.Height)
			.FirstOrDefault();

		if (bestFormat is not null)
			return (float)bestFormat.Width!.Value / bestFormat.Height!.Value;

		if (response.AspectRatio is double aspectRatio && aspectRatio > 0)
			return (float)aspectRatio;

		return null;
	}

	private async Task<string> RunYtDlpAsync(string url, CancellationToken cancellationToken)
	{
		using var process = new Process();

		process.StartInfo.FileName = YtDlpExecutable;
		process.StartInfo.ArgumentList.Add("--dump-single-json");
		process.StartInfo.ArgumentList.Add("--no-warnings");
		process.StartInfo.ArgumentList.Add("--no-playlist");
		process.StartInfo.ArgumentList.Add("--ignore-no-formats-error");

		if ((_options.Proxy?.Enabled ?? false) && !string.IsNullOrWhiteSpace(_options.Proxy.Address))
		{
			process.StartInfo.ArgumentList.Add("--proxy");
			process.StartInfo.ArgumentList.Add(_options.Proxy.Address);
		}

		process.StartInfo.ArgumentList.Add(url);
		process.StartInfo.RedirectStandardOutput = true;
		process.StartInfo.RedirectStandardError = true;
		process.StartInfo.UseShellExecute = false;
		process.StartInfo.CreateNoWindow = true;
		_logger.LogDebug("Run command: {Command}", $"{YtDlpExecutable} {string.Join(' ', process.StartInfo.ArgumentList)}");
		process.Start();

		var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
		var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);

		await Task.WhenAll(outputTask, errorTask);
		await process.WaitForExitAsync(cancellationToken);

		if (process.ExitCode != 0)
			throw new Exception($"yt-dlp failed: {errorTask.Result}");

		return outputTask.Result;
	}

	private sealed class YtDlpResponse
	{
		[JsonPropertyName("id")] public string? Id { get; set; }
		[JsonPropertyName("webpage_url")] public string? WebpageUrl { get; set; }
		[JsonPropertyName("upload_date")] public string? UploadDate { get; set; }
		[JsonPropertyName("timestamp")] public long? Timestamp { get; set; }
		[JsonPropertyName("thumbnail")] public string? Thumbnail { get; set; }
		[JsonPropertyName("width")] public int? Width { get; set; }
		[JsonPropertyName("height")] public int? Height { get; set; }
		[JsonPropertyName("aspect_ratio")] public double? AspectRatio { get; set; }
		[JsonPropertyName("formats")] public List<YtDlpFormat>? Formats { get; set; }
	}

	private sealed class YtDlpFormat
	{
		[JsonPropertyName("width")] public int? Width { get; set; }
		[JsonPropertyName("height")] public int? Height { get; set; }
	}
}

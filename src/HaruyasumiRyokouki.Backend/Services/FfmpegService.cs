using HaruyasumiRyokouki.Backend.Common.Options;
using HaruyasumiRyokouki.Backend.Models.InternalDtos;
using HaruyasumiRyokouki.Backend.Services.Interfaces;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace HaruyasumiRyokouki.Backend.Services;

internal class FfmpegService : IFfmpegService
{
	private readonly ILogger<FfmpegService> _logger;
	private readonly MediaFormatOptions _mediaFormatOptions;
	private readonly FfmpegPresetsOptions _ffmpegPresetsOptions;

	private const string Input = "{input}";
	private const string Output = "{output}";
	private const string MiniatureSize = "{miniatureSize}";
	private const string VideoThumbnailPrefix = "{videoThumbnailPrefix}";

	public FfmpegService(ILogger<FfmpegService> logger, IOptions<MediaFormatOptions> mediaFormatOptions, IOptions<FfmpegPresetsOptions> mediaPresetsOptions)
	{
		_logger = logger;
		_mediaFormatOptions = mediaFormatOptions.Value;
		_ffmpegPresetsOptions = mediaPresetsOptions.Value;
	}

	private readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions()
	{
		PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
	};

	public async Task ConvertImageAsync(string input, string output, CancellationToken cancellationToken)
	{
		string arguments = _ffmpegPresetsOptions.Image
			.GetValueOrDefault(_mediaFormatOptions.ImagePreset)!
			.Replace(Input, input)
			.Replace(Output, output)
			.Replace(VideoThumbnailPrefix, "")
			.Trim();
		await RunFfmpegAsync(arguments, cancellationToken: cancellationToken);
	}

	public async Task CreateVideoPreviewAsync(string input, string output, CancellationToken cancellationToken)
	{
		string videoThumbnailPrefix = _ffmpegPresetsOptions.VideoThumbnailPrefix
			.GetValueOrDefault(_mediaFormatOptions.VideoThumbnailPreset)!
			.Trim();
		string arguments = _ffmpegPresetsOptions.Image
			.GetValueOrDefault(_mediaFormatOptions.ImagePreset)!
			.Replace(Input, input)
			.Replace(Output, output)
			.Replace(VideoThumbnailPrefix, videoThumbnailPrefix)
			.Trim();
		await RunFfmpegAsync(arguments, cancellationToken: cancellationToken);
	}

	public async Task ConvertVideoAsync(string input, string output, CancellationToken cancellationToken)
	{
		string workDir = Path.GetDirectoryName(input);
		string[] passes = _ffmpegPresetsOptions.Video.GetValueOrDefault(_mediaFormatOptions.VideoPreset)!;
		foreach (var pass in passes)
		{
			string arguments = pass
				.Replace(Input, input)
				.Replace(Output, output)
				.Trim();
			await RunFfmpegAsync(arguments, workDir, cancellationToken);
		}
	}

	public async Task<VideoFileInfo> GetVideoInfoAsync(MediaInput mediaInput, CancellationToken cancellationToken)
	{
		var arguments = new StringBuilder();
		foreach (var header in mediaInput.Headers)
		{
			arguments.Append($"-headers \"{header.Key}: {header.Value}\" ");
		}
		arguments.Append($"-v quiet -print_format json -show_format -show_streams -i \"{mediaInput.Input}\"");
		var jsonData = await RunFfprobeAsync(arguments.ToString(), cancellationToken);

		var result = JsonSerializer.Deserialize<FfprobeResponse>(jsonData, _serializerOptions);

		return new VideoFileInfo
		{
			Bitrate = long.Parse(result.Format.BitRate ?? "0"),
			Duration = double.Parse(result.Format.Duration ?? "0", CultureInfo.InvariantCulture),
			Size = long.Parse(result.Format.Size ?? "0")
		};
	}

	public async Task<byte[]> GetImageMiniatureBytesAsync(string input, CancellationToken cancellationToken)
	{
		int miniatureSize = _mediaFormatOptions.MiniatureSize;
		string arguments = _ffmpegPresetsOptions.Miniature
			.GetValueOrDefault(_mediaFormatOptions.MiniaturePreset)!
			.Replace(Input, input)
			.Replace(MiniatureSize, miniatureSize.ToString())
			.Trim();
		return await RunFfmpegBytesAsync(arguments, cancellationToken);
	}

	private async Task RunFfmpegAsync(string arguments, string? workingDirectory = null, CancellationToken cancellationToken = default)
	{
		using var process = new Process();

		process.StartInfo.FileName = "ffmpeg";
		process.StartInfo.Arguments = arguments;
		if (workingDirectory != null)
			process.StartInfo.WorkingDirectory = workingDirectory;
		process.StartInfo.RedirectStandardError = true;
		process.StartInfo.UseShellExecute = false;
		process.StartInfo.CreateNoWindow = true;
		_logger.LogDebug("Run command: {Command}", $"{process.StartInfo.FileName} {arguments}");
		process.Start();

		var error = await process.StandardError.ReadToEndAsync(cancellationToken);

		await process.WaitForExitAsync(cancellationToken);

		if (process.ExitCode != 0)
		{
			throw new Exception($"FFmpeg failed: {error}");
		}
	}

	private async Task<byte[]> RunFfmpegBytesAsync(string arguments, CancellationToken cancellationToken = default)
	{
		using var process = new Process();

		process.StartInfo.FileName = "ffmpeg";
		process.StartInfo.Arguments = arguments;
		process.StartInfo.RedirectStandardOutput = true;
		process.StartInfo.RedirectStandardError = true;
		process.StartInfo.UseShellExecute = false;
		process.StartInfo.CreateNoWindow = true;
		_logger.LogDebug("Run command: {Command}", $"{process.StartInfo.FileName} {arguments}");
		process.Start();

		using var output = new MemoryStream();
		var copyTask = process.StandardOutput.BaseStream.CopyToAsync(output, cancellationToken);
		var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);

		await Task.WhenAll(copyTask, errorTask);
		await process.WaitForExitAsync(cancellationToken);

		if (process.ExitCode != 0)
		{
			throw new Exception($"FFmpeg failed: {errorTask.Result}");
		}

		return output.ToArray();
	}

	private async Task<string> RunFfprobeAsync(string arguments, CancellationToken cancellationToken = default)
	{
		using var process = new Process();

		process.StartInfo.FileName = "ffprobe";
		process.StartInfo.Arguments = arguments;
		process.StartInfo.RedirectStandardOutput = true;
		process.StartInfo.RedirectStandardError = true;
		process.StartInfo.UseShellExecute = false;
		process.StartInfo.CreateNoWindow = true;
		_logger.LogDebug("Run command: {Command}", $"{process.StartInfo.FileName} {arguments}");
		process.Start();

		string output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
		string error = await process.StandardError.ReadToEndAsync(cancellationToken);

		await process.WaitForExitAsync(cancellationToken);

		if (process.ExitCode != 0)
		{
			throw new Exception($"FFmpeg failed: {error}");
		}

		return output;
	}

	private sealed class FfprobeResponse
	{
		public FfprobeFormat? Format { get; set; }
	}

	private sealed class FfprobeFormat
	{
		public string? BitRate { get; set; }
		public string? Duration { get; set; }
		public string? Size { get; set; }
	}
}

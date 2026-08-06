using HaruyasumiRyokouki.Backend.Models.InternalDtos;
using HaruyasumiRyokouki.Backend.Models.InternalDtos.Enums;
using HaruyasumiRyokouki.Backend.Services.Interfaces;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace HaruyasumiRyokouki.Backend.Services;

internal class FfmpegService : IFfmpegService
{
	private readonly ILogger<FfmpegService> _logger;

	public FfmpegService(ILogger<FfmpegService> logger)
	{
		_logger = logger;
	}

	private readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions()
	{
		PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
	};

	public async Task ConvertImageAsync(string input, string output, FfmpegImagePreset preset, CancellationToken cancellationToken)
	{
		string arguments = $"-i \"{input}\" {GetImagePreset(preset)} \"{output}\"";
		await RunFfmpegAsync(arguments, cancellationToken: cancellationToken);
	}

	public async Task CreateVideoPreviewAsync(string input, string output, FfmpegImagePreset preset, CancellationToken cancellationToken)
	{
		string videoPreviewArguments = "-ss 00:00:01 -vframes 1";
		string arguments = $"-i \"{input}\" {videoPreviewArguments} {GetImagePreset(preset)} \"{output}\"";
		await RunFfmpegAsync(arguments, cancellationToken: cancellationToken);
	}

	public async Task ConvertVideoAsync(string input, string output, FfmpegVideoPreset preset, CancellationToken cancellationToken)
	{
		string workDir = Path.GetDirectoryName(input);
		string pass1Arguments = $"-i \"{input}\" {GetVideoPreset(preset, pass: 1)}";
		string pass2Arguments = $"-i \"{input}\" {GetVideoPreset(preset, pass: 2)} \"{output}\"";

		await RunFfmpegAsync(pass1Arguments, workDir, cancellationToken);
		await RunFfmpegAsync(pass2Arguments, workDir, cancellationToken);
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

	public async Task<byte[]> GetImageMiniatureBytesAsync(string input, int sideSize, CancellationToken cancellationToken)
	{
		string arguments =
			$"-i \"{input}\" " +
			$"-vf \"scale={sideSize}:{sideSize}:force_original_aspect_ratio=increase,crop={sideSize}:{sideSize}\" " +
			$"-c:v libwebp -quality 75 -compression_level 6 -preset picture -f webp pipe:1";
		return await RunFfmpegBytesAsync(arguments, cancellationToken);
	}


	private string GetImagePreset(FfmpegImagePreset preset)
	{
		switch (preset)
		{
			case FfmpegImagePreset.Webp:
				return "-map_metadata 0 -c:v libwebp -quality 88 -compression_level 6 -preset picture";
			case FfmpegImagePreset.Avif:
				return "-map_metadata 0 -c:v libaom-av1 -still-picture 1 -crf 20";
			default:
				throw new NotImplementedException("Image preset does not exist.");
		}
	}
	
	private string GetVideoPreset(FfmpegVideoPreset preset, int pass = 1)
	{
		const string errorMessage = "Video preset does not exist.";
		return preset switch
		{
			FfmpegVideoPreset.h254_1440p_2pass_20M => pass switch
			{
				1 => "-map_metadata 0 -vf \"scale='if(gt(iw,ih),-2,1440)':'if(gt(iw,ih),1440,-2)'\" -c:v libx264 -preset slow -aq-mode 3 -profile:v high -level 5.1 -b:v 20M -pass 1 -an -f null -",
				2 => "-map_metadata 0 -vf \"scale='if(gt(iw,ih),-2,1440)':'if(gt(iw,ih),1440,-2)'\" -c:v libx264 -preset slow -aq-mode 3 -profile:v high -level 5.1 -b:v 20M -pass 2 -c:a copy -movflags +faststart",
				_ => throw new NotImplementedException(errorMessage)
			},
			FfmpegVideoPreset.h254_1080p_2pass_10M => pass switch
			{
				1 => "-map_metadata 0 -vf \"scale='if(gt(iw,ih),-2,1080)':'if(gt(iw,ih),1080,-2)'\" -c:v libx264 -preset slow -aq-mode 3 -profile:v high -level 5.1 -b:v 10M -pass 1 -an -f null -",
				2 => "-map_metadata 0 -vf \"scale='if(gt(iw,ih),-2,1080)':'if(gt(iw,ih),1080,-2)'\" -c:v libx264 -preset slow -aq-mode 3 -profile:v high -level 5.1 -b:v 10M -pass 2 -c:a copy -movflags +faststart",
				_ => throw new NotImplementedException(errorMessage)
			},
			FfmpegVideoPreset.h254_720p_2pass_5M => pass switch
			{
				1 => "-map_metadata 0 -vf \"scale='if(gt(iw,ih),-2,720)':'if(gt(iw,ih),720,-2)'\" -c:v libx264 -preset slow -aq-mode 3 -profile:v high -level 5.1 -b:v 5M -pass 1 -an -f null -",
				2 => "-map_metadata 0 -vf \"scale='if(gt(iw,ih),-2,720)':'if(gt(iw,ih),720,-2)'\" -c:v libx264 -preset slow -aq-mode 3 -profile:v high -level 5.1 -b:v 5M -pass 2 -c:a copy -movflags +faststart",
				_ => throw new NotImplementedException(errorMessage)
			},
			_ => throw new NotImplementedException(errorMessage),
		};
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

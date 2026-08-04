using HaruyasumiRyokouki.Backend.Common.Options;
using HaruyasumiRyokouki.Backend.Common.ResultType;
using HaruyasumiRyokouki.Backend.Models.Db.Enums;
using HaruyasumiRyokouki.Backend.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace HaruyasumiRyokouki.Backend.Services;

internal class MediaProcessorService : IMediaProcessorService
{
	private readonly ILogger<MediaProcessorService> _logger;
	private readonly IFileStorage _fileStorage;
	private readonly IFfmpegService _ffmpegService;
	private readonly MediaFormatOptions _mediaFormatOptions;

	private static readonly HashSet<string> ImageExtensions =
	[
		".jpg", ".jpeg", ".png", ".bmp", ".webp", ".tif", ".tiff", ".heic", ".heif", ".avif"
	];

	private static readonly HashSet<string> VideoExtensions =
	[
		".mp4", ".mov", ".avi", ".mkv", ".wmv", ".flv", ".webm", ".m4v", ".3gp", ".mpeg", ".mpg"
	];

	private const string _videoSuffix = "_web";

	public MediaProcessorService(ILogger<MediaProcessorService> logger, IFileStorage fileStorage, IFfmpegService ffmpegService, IOptions<MediaFormatOptions> mediaFormatOptions)
	{
		_logger = logger;
		_fileStorage = fileStorage;
		_ffmpegService = ffmpegService;
		_mediaFormatOptions = mediaFormatOptions.Value;
	}

	public string GetWebName(string fileName)
	{
		if (Path.GetFileNameWithoutExtension(fileName).EndsWith(_videoSuffix))
			return fileName;
		return Path.GetFileNameWithoutExtension(fileName) + _videoSuffix + Path.GetExtension(fileName);
	}

	public bool IsAnImage(string fileName)
	{
		return ImageExtensions.Contains(Path.GetExtension(fileName));
	}

	public bool IsAVideo(string fileName)
	{
		return VideoExtensions.Contains(Path.GetExtension(fileName));
	}

	private async Task<bool> ShouldBeConverted(string fileName, MediaType mediaType)
	{
		if (mediaType == MediaType.Image)
		{
			string fileExtension = Path.GetExtension(fileName).TrimStart('.').ToLower();
			string targetExtension = _mediaFormatOptions.TargetImagePreset.ToString().ToLower();
			return fileExtension != targetExtension;
		}
		if (mediaType == MediaType.Video)
		{
			var mediaInput = await _fileStorage.GetMediaInputAsync(fileName);
			var info = await _ffmpegService.GetVideoInfoAsync(mediaInput);
			return info.Bitrate > (25 * 1024 * 1024);
		}
		throw new NotImplementedException();
	}

	public async Task<Result<string>> ConvertImageAsync(string fileName, CancellationToken cancellationToken)
	{
		if (!IsAnImage(fileName))
			return Result<string>.Failure("File is not an image");
		if (!await ShouldBeConverted(fileName, MediaType.Image))
			return Result<string>.Success(fileName);

		string resultImageFileName = Path.GetFileNameWithoutExtension(fileName) + "." + _mediaFormatOptions.TargetImagePreset.ToString().ToLower();

		await using var input = await _fileStorage.OpenReadAsync(fileName, cancellationToken);
		await using var workspace = new TempStorageService(MediaType.Image);

		var inputFile = Path.Combine(workspace.TempFolder, fileName);
		var outputFile = Path.Combine(workspace.TempFolder, resultImageFileName);

		Directory.CreateDirectory(workspace.TempFolder);
		await using (var file = File.Create(inputFile))
		{
			await input.CopyToAsync(file, cancellationToken);
		}

		_logger.LogDebug("File convertion started ({OriginalFileName} -> {NewFileName})", fileName, resultImageFileName);
		await _ffmpegService.ConvertImageAsync(inputFile, outputFile, _mediaFormatOptions.TargetImagePreset, cancellationToken);
		_logger.LogDebug("File convertion ended ({OriginalFileName} -> {NewFileName})", fileName, resultImageFileName);

		await using var result = File.OpenRead(outputFile);
		await _fileStorage.SaveFileAsync(resultImageFileName, result, cancellationToken);
		await _fileStorage.DeleteAsync(fileName, cancellationToken);

		_logger.LogInformation("File {OriginalFileName} was replaced with {NewFileName}", fileName, resultImageFileName);

		return Result<string>.Success(resultImageFileName);
	}

	public async Task<Result<string>> ConvertVideoAsync(string fileName, CancellationToken cancellationToken)
	{
		if (!IsAVideo(fileName))
			return Result<string>.Failure("File is not a video");
		if (!await ShouldBeConverted(fileName, MediaType.Video))
			return Result<string>.Success(fileName);

		string resultVideoFileName = Path.GetFileNameWithoutExtension(fileName) + _videoSuffix + Path.GetExtension(fileName);

		await using var input = await _fileStorage.OpenReadAsync(fileName, cancellationToken);
		await using var workspace = new TempStorageService(MediaType.Video);

		var inputFile = Path.Combine(workspace.TempFolder, fileName);
		var outputFile = Path.Combine(workspace.TempFolder, resultVideoFileName);

		Directory.CreateDirectory(workspace.TempFolder);
		await using (var file = File.Create(inputFile))
		{
			await input.CopyToAsync(file, cancellationToken);
		}

		_logger.LogDebug("File convertion started ({OriginalFileName} -> {NewFileName})", fileName, resultVideoFileName);
		await _ffmpegService.ConvertVideoAsync(inputFile, outputFile, _mediaFormatOptions.TargetVideoPreset, cancellationToken);
		_logger.LogDebug("File convertion ended ({OriginalFileName} -> {NewFileName})", fileName, resultVideoFileName);

		await using var result = File.OpenRead(outputFile);
		await _fileStorage.SaveFileAsync(resultVideoFileName, result, cancellationToken);

		_logger.LogInformation("File {NewFileName} was created", resultVideoFileName);

		return Result<string>.Success(resultVideoFileName);
	}
}

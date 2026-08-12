using HaruyasumiRyokouki.Backend.Common.Options;
using HaruyasumiRyokouki.Backend.Common.ResultType;
using HaruyasumiRyokouki.Backend.Models.Db.Enums;
using HaruyasumiRyokouki.Backend.Models.InternalDtos;
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

	private const string VideoWebSuffix = "_web";
	private const string VideoPreviewSuffix = "_preview";

	public MediaProcessorService(ILogger<MediaProcessorService> logger, IFileStorage fileStorage, IFfmpegService ffmpegService, IOptions<MediaFormatOptions> mediaFormatOptions)
	{
		_logger = logger;
		_fileStorage = fileStorage;
		_ffmpegService = ffmpegService;
		_mediaFormatOptions = mediaFormatOptions.Value;
	}

	public string GetVideoWebName(string fileName)
	{
		if (Path.GetFileNameWithoutExtension(fileName).EndsWith(VideoWebSuffix))
			return fileName;
		return Path.GetFileNameWithoutExtension(fileName) + VideoWebSuffix + Path.GetExtension(fileName);
	}

	public string GetVideoPreviewName(string fileName)
	{
		if (Path.GetFileNameWithoutExtension(fileName).EndsWith(VideoPreviewSuffix))
			return fileName;
		return Path.GetFileNameWithoutExtension(fileName) + VideoPreviewSuffix + "." + _mediaFormatOptions.ImagePreset.ToString().ToLower();
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
			string targetExtension = _mediaFormatOptions.ImagePreset.ToString().ToLower();
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

	public async Task<Result<ConvertionsResponseDto>> ConvertImageAsync(string fileName, CancellationToken cancellationToken)
	{
		if (!IsAnImage(fileName))
			return Result<ConvertionsResponseDto>.Failure("File is not an image");

		await using var input = await _fileStorage.OpenReadAsync(fileName, cancellationToken);
		await using var workspace = new TempStorageService(MediaType.Image);

		string resultImageFileName = fileName;
		var inputFile = Path.Combine(workspace.TempFolder, fileName);

		Directory.CreateDirectory(workspace.TempFolder);
		await using (var file = File.Create(inputFile))
		{
			await input.CopyToAsync(file, cancellationToken);
		}

		if (await ShouldBeConverted(fileName, MediaType.Image))
		{
			resultImageFileName = Path.GetFileNameWithoutExtension(fileName) + "." + _mediaFormatOptions.ImagePreset.ToString().ToLower();
			var outputFile = Path.Combine(workspace.TempFolder, resultImageFileName);

			_logger.LogDebug("File convertion started ({OriginalFileName} -> {NewFileName})", fileName, resultImageFileName);
			await _ffmpegService.ConvertImageAsync(inputFile, outputFile, cancellationToken);
			_logger.LogDebug("File convertion ended ({OriginalFileName} -> {NewFileName})", fileName, resultImageFileName);
			SetOriginalTimeInfo(inputFile, outputFile);

			await using var result = File.OpenRead(outputFile);
			await _fileStorage.SaveFileAsync(resultImageFileName, result, cancellationToken);
			await _fileStorage.DeleteAsync(fileName, cancellationToken);
			_logger.LogInformation("File {OriginalFileName} was replaced with {NewFileName}", fileName, resultImageFileName);
		}

		string miniature = await CreateMiniatureAsync(inputFile, workspace, cancellationToken);
		float aspectRatio = await _ffmpegService.GetImageAspectRatioAsync(inputFile, cancellationToken);

		return Result<ConvertionsResponseDto>.Success(new(resultImageFileName, miniature, aspectRatio));
	}

	public async Task<Result<ConvertionsResponseDto>> ConvertVideoAsync(string fileName, CancellationToken cancellationToken)
	{
		if (!IsAVideo(fileName))
			return Result<ConvertionsResponseDto>.Failure("File is not a video");

		await using var input = await _fileStorage.OpenReadAsync(fileName, cancellationToken);
		await using var workspace = new TempStorageService(MediaType.Video);

		string resultVideoFileName = fileName;
		var inputFile = Path.Combine(workspace.TempFolder, fileName);

		Directory.CreateDirectory(workspace.TempFolder);
		await using (var file = File.Create(inputFile))
		{
			await input.CopyToAsync(file, cancellationToken);
		}

		if (await ShouldBeConverted(fileName, MediaType.Video))
		{
			resultVideoFileName = GetVideoWebName(fileName);
			var outputFile = Path.Combine(workspace.TempFolder, resultVideoFileName);

			_logger.LogDebug("File convertion started ({OriginalFileName} -> {NewFileName})", fileName, resultVideoFileName);
			await _ffmpegService.ConvertVideoAsync(inputFile, outputFile, cancellationToken);
			_logger.LogDebug("File convertion ended ({OriginalFileName} -> {NewFileName})", fileName, resultVideoFileName);
			SetOriginalTimeInfo(inputFile, outputFile);

			await using var result = File.OpenRead(outputFile);
			await _fileStorage.SaveFileAsync(resultVideoFileName, result, cancellationToken);
			_logger.LogInformation("File {NewFileName} was created", resultVideoFileName);
		}

		string previewOutputFile = await CreateVideoPreviewAsync(inputFile, workspace, cancellationToken);
		string miniature = await CreateMiniatureAsync(previewOutputFile, workspace, cancellationToken);
		float aspectRatio = await _ffmpegService.GetImageAspectRatioAsync(previewOutputFile, cancellationToken);

		return Result<ConvertionsResponseDto>.Success(new(resultVideoFileName, miniature, aspectRatio));
	}

	public async Task<Result<string>> CreateMiniatureAsync(string fileName, CancellationToken cancellationToken)
	{
		if (!IsAnImage(fileName))
			return Result<string>.Failure("File is not an image");

		await using var input = await _fileStorage.OpenReadAsync(fileName, cancellationToken);
		await using var workspace = new TempStorageService(MediaType.Image);

		var inputFile = Path.Combine(workspace.TempFolder, fileName);

		Directory.CreateDirectory(workspace.TempFolder);
		await using (var file = File.Create(inputFile))
		{
			await input.CopyToAsync(file, cancellationToken);
		}

		string miniature = await CreateMiniatureAsync(inputFile, workspace, cancellationToken);

		_logger.LogInformation("Miniature for {FileName} was created", fileName);
		return miniature;
	}

	private async Task<string> CreateMiniatureAsync(string fileName, TempStorageService workspace, CancellationToken cancellationToken)
	{
		var miniatureBytes = await _ffmpegService.GetImageMiniatureBytesAsync
		(
			fileName,
			cancellationToken
		);
		return Convert.ToBase64String(miniatureBytes);
	}

	private async Task<string> CreateVideoPreviewAsync(string fileName, TempStorageService workspace, CancellationToken cancellationToken)
	{
		string resultVideoPreviewFileName = GetVideoPreviewName(fileName);
		var inputFile = Path.Combine(workspace.TempFolder, fileName);
		var outputFile = Path.Combine(workspace.TempFolder, resultVideoPreviewFileName);

		_logger.LogDebug("Video preview creation started ({OriginalFileName} -> {NewFileName})", fileName, resultVideoPreviewFileName);
		await _ffmpegService.CreateVideoPreviewAsync(inputFile, outputFile, cancellationToken);

		SetOriginalTimeInfo(inputFile, outputFile);

		await using var result = File.OpenRead(outputFile);
		await _fileStorage.SaveFileAsync(resultVideoPreviewFileName, result, cancellationToken);
		_logger.LogInformation("Video preview {NewFileName} was created", resultVideoPreviewFileName);

		return outputFile;
	}

	private void SetOriginalTimeInfo(string sourceFile, string targetFile)
	{
		var source = new FileInfo(sourceFile);
		var target = new FileInfo(targetFile);

		File.SetCreationTime(target.FullName, source.CreationTime);
		File.SetLastWriteTime(target.FullName, source.LastWriteTime);
		File.SetLastAccessTime(target.FullName, source.LastAccessTime);
	}
}

using HaruyasumiRyokouki.Backend.Common.Options;
using HaruyasumiRyokouki.Backend.Models.InternalDtos;
using HaruyasumiRyokouki.Backend.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace HaruyasumiRyokouki.Backend.Services;

internal class LocalFileStorageService : IFileStorage
{
	private readonly string _path;
	private readonly ILogger<LocalFileStorageService> _logger;

	public LocalFileStorageService(IOptions<LocalStorageOptions> options, ILogger<LocalFileStorageService> logger)
	{
		_path = options.Value.Path;
		_logger = logger;
	}

	public Task<MediaInput> GetMediaInputAsync(string fileName, CancellationToken cancellationToken = default)
	{
		return Task.FromResult(new MediaInput
		{
			Input = Path.Combine(_path, fileName)
		});
	}

	public Task<Stream> OpenReadAsync(string fileName, CancellationToken cancellationToken)
	{
		var fullPath = Path.Combine(_path, fileName);

		Stream stream = new FileStream(
			fullPath,
			FileMode.Open,
			FileAccess.Read,
			FileShare.Read,
			81920,
			FileOptions.Asynchronous);

		return Task.FromResult(stream);
	}

	public async Task SaveFileAsync(string fileName, Stream content, CancellationToken cancellationToken)
	{
		var fullPath = Path.Combine(_path, fileName);

		Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

		await using var file = new FileStream(
			fullPath,
			FileMode.Create,
			FileAccess.Write,
			FileShare.None,
			81920,
			FileOptions.Asynchronous);

		content.Position = 0;
		await content.CopyToAsync(file, cancellationToken);
	}

	public Task<IReadOnlyCollection<StorageFile>> GetFilesAsync(CancellationToken cancellationToken)
	{
		var files = Directory
			.EnumerateFiles(_path)
			.Select(path =>
			{
				var info = new FileInfo(path);

				return new StorageFile
				{
					FileName = info.Name,
					Created = DateTime.SpecifyKind(info.LastWriteTime, DateTimeKind.Unspecified)
				};
			})
			.ToList();

		return Task.FromResult<IReadOnlyCollection<StorageFile>>(files);
	}

	public Task DeleteAsync(string fileName, CancellationToken cancellationToken = default)
	{
		string filePath = Path.Combine(_path, fileName);

		if (File.Exists(filePath))
		{
			File.Delete(filePath);
		}

		_logger.LogInformation("Deleted {FileName}", fileName);

		return Task.CompletedTask;
	}
}

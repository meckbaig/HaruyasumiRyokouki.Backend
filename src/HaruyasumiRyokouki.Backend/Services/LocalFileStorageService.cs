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

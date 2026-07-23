using HaruyasumiRyokouki.Backend.Models.InternalDtos;
using HaruyasumiRyokouki.Backend.Services.Interfaces;
using WebDav;

namespace HaruyasumiRyokouki.Backend.Services;

internal class WebDavFileStorageService : IFileStorage
{
	private readonly WebDavClient _client;
	private readonly ILogger<WebDavFileStorageService> _logger;

	public WebDavFileStorageService(WebDavClient client, ILogger<WebDavFileStorageService> logger)
	{
		_client = client;
		_logger = logger;
	}

	public async Task<IReadOnlyCollection<StorageFile>> GetFilesAsync(CancellationToken cancellationToken)
	{
		var result = await _client.Propfind("");

		return result.Resources
			.Where(r => !r.IsCollection && r.LastModifiedDate != null)
			.Select(r => new StorageFile
			{
				FileName = Path.GetFileName(r.Uri),
				Created = DateTime.SpecifyKind((DateTime)r.LastModifiedDate!, DateTimeKind.Unspecified)
			})
			.ToList();
	}

	public async Task DeleteAsync(string fileName, CancellationToken cancellationToken = default)
	{
		var result = await _client.Delete(fileName);

		if (!result.IsSuccessful)
		{
			_logger.LogError("Failed to delete {FileName}", fileName);
			throw new Exception($"Delete failed: {result.StatusCode}");
		}

		_logger.LogInformation("Deleted {FileName}", fileName);
	}
}

using HaruyasumiRyokouki.Backend.Services.Interfaces;
using System.Collections.Concurrent;

namespace HaruyasumiRyokouki.Backend.Services.BackgroundServices;

public class FileRemovalBackgroundService : BackgroundService, IRemovalQueueService
{
	private readonly ILogger<FileRemovalBackgroundService> _logger;
	private readonly IServiceProvider _services;

	private CancellationToken _appStoppingToken;
	private readonly ConcurrentQueue<string> _filesRemovalQueue = new();
	private readonly SemaphoreSlim _removalLock = new(1, 1);

	public FileRemovalBackgroundService
	(
		ILogger<FileRemovalBackgroundService> logger,
		IServiceProvider services
	)
	{
		_logger = logger;
		_services = services;
	}

	protected override Task ExecuteAsync(CancellationToken stoppingToken)
	{
		_appStoppingToken = stoppingToken;
		return Task.CompletedTask;
	}

	public async Task AddAsync(string fileName)
	{
		_filesRemovalQueue.Enqueue(fileName);
		_ = ProcessQueueAsync();
	}

	private async Task ProcessQueueAsync()
	{
		if (!await _removalLock.WaitAsync(0, _appStoppingToken))
			return;

		try
		{
			using var scope = _services.CreateScope();
			var fileStorage = scope.ServiceProvider.GetRequiredService<IFileStorage>();

			while (_filesRemovalQueue.TryDequeue(out var fileName))
			{
				await fileStorage.DeleteAsync(fileName, _appStoppingToken);
			}
		}
		catch (Exception ex)
		{
			_logger.LogError("An unhandled exception occurred while deleting the file: {Error}", ex.Message);
		}
		finally
		{
			_removalLock.Release();
		}
	}
}

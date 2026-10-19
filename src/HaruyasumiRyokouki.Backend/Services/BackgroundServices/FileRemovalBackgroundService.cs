using HaruyasumiRyokouki.Backend.Services.Interfaces;
using System.Threading.Channels;

namespace HaruyasumiRyokouki.Backend.Services.BackgroundServices;

/// <summary>
/// Background queue that deletes files from the file storage one by one.
/// </summary>
public class FileRemovalBackgroundService : BackgroundService, IRemovalQueueService
{
	private readonly ILogger<FileRemovalBackgroundService> _logger;
	private readonly IServiceProvider _services;

	/// <summary>
	/// Unbounded queue with many writers (request handlers) and a single reader (<see cref="ExecuteAsync"/>).
	/// </summary>
	private readonly Channel<string> _queue = Channel.CreateUnbounded<string>(
		new UnboundedChannelOptions { SingleReader = true });

	/// <summary>
	/// Constructor with parameters for DI.
	/// </summary>
	public FileRemovalBackgroundService
	(
		ILogger<FileRemovalBackgroundService> logger,
		IServiceProvider services
	)
	{
		_logger = logger;
		_services = services;
	}

	/// <inheritdoc/>
	public Task AddAsync(string fileName)
	{
		// Writing to an unbounded channel always succeeds immediately.
		_queue.Writer.TryWrite(fileName);
		return Task.CompletedTask;
	}

	/// <summary>
	/// Waits for queued files and deletes them until the application stops.
	/// </summary>
	/// <param name="stoppingToken">Triggered when the application is stopping.</param>
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		using var scope = _services.CreateScope();
		var fileStorage = scope.ServiceProvider.GetRequiredService<IFileStorage>();

		await foreach (var fileName in _queue.Reader.ReadAllAsync(stoppingToken))
		{
			try
			{
				await fileStorage.DeleteAsync(fileName, stoppingToken);
			}
			catch (Exception ex) when (ex is not OperationCanceledException)
			{
				// A failure on one file must not stop deletion of the others.
				_logger.LogError(ex, "Failed to delete {FileName}", fileName);
			}
		}
	}
}

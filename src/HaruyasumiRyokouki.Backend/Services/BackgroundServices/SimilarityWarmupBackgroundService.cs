
using HaruyasumiRyokouki.Backend.DbContexts;
using HaruyasumiRyokouki.Backend.Services.Interfaces;

namespace HaruyasumiRyokouki.Backend.Services.BackgroundServices;

internal class SimilarityWarmupBackgroundService : BackgroundService
{
	private readonly IServiceScopeFactory _scopeFactory;
	private readonly IMediaSimilarityIndexService _similarityService;
	private readonly ILogger<SimilarityWarmupBackgroundService> _logger;

	public SimilarityWarmupBackgroundService(IServiceScopeFactory scopeFactory, IMediaSimilarityIndexService similarityService, ILogger<SimilarityWarmupBackgroundService> logger)
	{
		_scopeFactory = scopeFactory;
		_similarityService = similarityService;
		_logger = logger;
	}

	protected override async Task ExecuteAsync(CancellationToken cancellationToken)
	{
		try
		{
			using var scope = _scopeFactory.CreateScope();
			var context = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
			await _similarityService.ReloadAsync(context, cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, $"Failed to warm up {nameof(SimilarityWarmupBackgroundService)}");
		}
	}
}

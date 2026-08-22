namespace HaruyasumiRyokouki.Backend.Services.Interfaces;

public interface IRemovalQueueService
{
	Task AddAsync(string fileName);
}

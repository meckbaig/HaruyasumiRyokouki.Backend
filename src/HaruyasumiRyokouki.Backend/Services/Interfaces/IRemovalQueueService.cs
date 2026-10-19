namespace HaruyasumiRyokouki.Backend.Services.Interfaces;

/// <summary>
/// Queue of files to be deleted from the file storage in the background.
/// </summary>
public interface IRemovalQueueService
{
	/// <summary>
	/// Queues a file for deletion. Returns immediately, the file is deleted later in the background.
	/// </summary>
	/// <param name="fileName">Name of the file in the file storage.</param>
	Task AddAsync(string fileName);
}

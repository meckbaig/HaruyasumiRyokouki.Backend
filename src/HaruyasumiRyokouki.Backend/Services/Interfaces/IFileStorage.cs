using HaruyasumiRyokouki.Backend.Models.InternalDtos;

namespace HaruyasumiRyokouki.Backend.Services.Interfaces;

public interface IFileStorage
{
	/// <summary>
	/// Deletes the specified file asynchronously.
	/// </summary>
	/// <param name="fileName">The name of the file to delete.</param>
	/// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
	/// <returns>A task that represents the asynchronous delete operation.</returns>
	Task DeleteAsync(string fileName, CancellationToken cancellationToken = default);

	/// <summary>
	/// Returns all the files from file storage provider and their local creation dates.
	/// </summary>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>Collection of files and their local creation dates.</returns>
	Task<IReadOnlyCollection<StorageFile>> GetFilesAsync(CancellationToken cancellationToken);
}

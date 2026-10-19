namespace HaruyasumiRyokouki.Backend.Services.Interfaces;

/// <summary>
/// Synchronizes the database with the file storage.
/// </summary>
internal interface IMediaSyncService
{
	/// <summary>
	/// Imports new files from the file storage, creates missing miniatures and logs files
	/// that are in the database but missing from the storage.
	/// </summary>
	/// <exception cref="Common.Exceptions.ConflictException">Another sync is already running.</exception>
	Task SyncAsync(CancellationToken cancellationToken = default);
}

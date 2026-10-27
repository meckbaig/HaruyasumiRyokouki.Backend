using HaruyasumiRyokouki.Backend.Models.InternalDtos;

namespace HaruyasumiRyokouki.Backend.Services.Interfaces;

/// <summary>
/// Storage of media files: a local folder or a WebDAV folder, selected by <c>MediaStorage:Provider</c>.
/// File names are relative to the media folder; there are no subfolders.
/// </summary>
internal interface IFileStorage
{
	/// <summary>
	/// Returns how ffmpeg/ffprobe can read the file directly: a local path, or a URL with an authorization header.
	/// The header value must not be logged.
	/// </summary>
	Task<MediaInput> GetMediaInputAsync(string filePath, CancellationToken cancellationToken = default);

	/// <summary>
	/// Opens the file for reading.
	/// </summary>
	Task<Stream> OpenReadAsync(string fileName, CancellationToken cancellationToken = default);

	/// <summary>
	/// Creates or overwrites the file.
	/// </summary>
	Task SaveFileAsync(string fileName, Stream stream, CancellationToken cancellationToken = default);

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
	/// <returns>Collection of files and their modification time in UTC.</returns>
	Task<IReadOnlyCollection<StorageFile>> GetFilesAsync(CancellationToken cancellationToken = default);
}

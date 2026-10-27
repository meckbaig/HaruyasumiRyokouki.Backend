using HaruyasumiRyokouki.Backend.Models.Db.Enums;
using HaruyasumiRyokouki.Backend.Services.Interfaces;

namespace HaruyasumiRyokouki.Backend.Services;

/// <summary>
/// Temporary folder for processing one media file: <c>temp/&lt;media type&gt;/&lt;guid&gt;</c> next to the application binaries.
/// Deleted with its content on dispose.
/// </summary>
public class TempStorageService : IAsyncDisposable
{
	private const string TempWorkspace = "temp";

	/// <summary>
	/// Path of the folder; not created by the constructor.
	/// </summary>
	public string TempFolder { get; init; }

	/// <summary>
	/// Chooses a new folder path for the media type.
	/// </summary>
	public TempStorageService(MediaType mediaType)
	{
		TempFolder = Path.Combine
		(
			AppDomain.CurrentDomain.BaseDirectory,
			TempWorkspace, 
			mediaType.ToString(),
			Guid.NewGuid().ToString()
		);
	}

	/// <summary>
	/// Deletes the folder if it exists.
	/// </summary>
	public ValueTask DisposeAsync()
	{
		if (Directory.Exists(TempFolder))
		{
			Directory.Delete(TempFolder, true);
		}
		return ValueTask.CompletedTask;
	}
}

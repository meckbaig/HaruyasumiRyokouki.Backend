using HaruyasumiRyokouki.Backend.Models.Db.Enums;
using HaruyasumiRyokouki.Backend.Services.Interfaces;

namespace HaruyasumiRyokouki.Backend.Services;

public class TempStorageService : ITempStorageService
{
	private const string TempWorkspace = "temp";
	public string TempFolder { get; init; }

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

	public ValueTask DisposeAsync()
	{
		if (Directory.Exists(TempFolder))
		{
			Directory.Delete(TempFolder, true);
		}
		return ValueTask.CompletedTask;
	}
}

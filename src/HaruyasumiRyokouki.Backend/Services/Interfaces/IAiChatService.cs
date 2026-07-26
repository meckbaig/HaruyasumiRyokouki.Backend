
namespace HaruyasumiRyokouki.Backend.Services.Interfaces;

public interface IAiChatService
{
	Task<string> GetChatResponseAsync(string message, bool returnJson = false, CancellationToken cancellationToken = default);
}

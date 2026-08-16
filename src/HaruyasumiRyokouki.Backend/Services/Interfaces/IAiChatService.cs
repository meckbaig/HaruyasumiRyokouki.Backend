
namespace HaruyasumiRyokouki.Backend.Services.Interfaces;

public interface IAiChatService
{
	Task<string> GetChatResponseAsync(string message, string? systemMessage = null, bool returnJson = false, CancellationToken cancellationToken = default);
}

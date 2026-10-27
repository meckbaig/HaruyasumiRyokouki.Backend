
namespace HaruyasumiRyokouki.Backend.Services.Interfaces;

/// <summary>
/// Chat model of an AI provider (OpenAI-compatible API or Gemini), configured by a <c>TranslationProvider:Providers</c> entry.
/// </summary>
public interface IAiChatService
{
	/// <summary>
	/// Sends one message and returns the text of the answer.
	/// </summary>
	/// <param name="message">User message.</param>
	/// <param name="systemMessage">Instructions for the model, if any.</param>
	/// <param name="returnJson">Ask the model for a JSON object. The answer is then a JSON string.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	Task<string> GetChatResponseAsync(string message, string? systemMessage = null, bool returnJson = false, CancellationToken cancellationToken = default);
}

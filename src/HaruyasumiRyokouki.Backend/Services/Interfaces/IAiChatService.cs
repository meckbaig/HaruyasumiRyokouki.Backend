
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
	/// <param name="cancellationToken">Cancellation token.</param>
	Task<string> GetChatResponseAsync(string message, string? systemMessage = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// Sends one message and returns the answer as a <typeparamref name="T"/> object.
	/// </summary>
	/// <remarks>
	/// The JSON schema of <typeparamref name="T"/> is sent when the provider supports it
	/// (see <see cref="Common.Options.Abstractions.OpenAiJsonResponseFormat"/>), so the prompt should still describe the structure.
	/// Property names are camelCase.
	/// </remarks>
	/// <param name="message">User message.</param>
	/// <param name="systemMessage">Instructions for the model, if any.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <exception cref="InvalidOperationException">The answer is empty or not a JSON of <typeparamref name="T"/>.</exception>
	Task<T> GetJsonResponseAsync<T>(string message, string? systemMessage = null, CancellationToken cancellationToken = default);
}

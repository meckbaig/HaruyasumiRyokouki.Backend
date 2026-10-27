namespace HaruyasumiRyokouki.Backend.Common.Abstractions;

/// <summary>
/// A request whose response is in one language.
/// </summary>
public interface ILocalizableRequest
{
	/// <summary>
	/// Supported language code chosen from the <c>Accept-Language</c> header, <c>en</c> by default.
	/// Set by <see cref="Behaviors.LocalizationBehavior{TRequest, TResponse}"/> after validation. Not bound from the request.
	/// </summary>
	string AcceptLanguage { get; set; }
}

using HaruyasumiRyokouki.Backend.Models.InternalDtos;

namespace HaruyasumiRyokouki.Backend.Common.Abstractions;

/// <summary>
/// A request whose response contains media URLs sized for the client screen.
/// </summary>
public interface IDisplayAwareRequest
{
	/// <summary>
	/// Screen parameters from the <c>X-Display</c> header, <see langword="null"/> without the header.
	/// Set by <see cref="Behaviors.ClientDisplayBehavior{TRequest, TResponse}"/> before validation. Not bound from the request.
	/// </summary>
	ClientDisplay? ClientDisplay { get; set; }
}

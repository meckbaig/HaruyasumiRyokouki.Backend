using FluentValidation;

namespace HaruyasumiRyokouki.Backend.Models.InternalDtos;

/// <summary>
/// Client screen from the <c>X-Display</c> header.
/// </summary>
public record ClientDisplay
{
	/// <summary>
	/// Device pixel ratio; 1 when unknown.
	/// </summary>
	public float? Dpr { get; set; }

	/// <summary>
	/// Smaller side of the screen in CSS pixels; <c>MediaSizes:DefaultScreenResolution</c> when unknown.
	/// </summary>
	public int? MinSide { get; set; }
}

internal class ClientDisplayValidator : AbstractValidator<ClientDisplay>
{
	public ClientDisplayValidator()
	{
		RuleFor(x => x.Dpr)
			.GreaterThan(0);

		RuleFor(x => x.MinSide)
			.GreaterThan(0);
	}
}

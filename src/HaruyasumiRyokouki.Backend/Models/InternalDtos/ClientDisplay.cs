using FluentValidation;

namespace HaruyasumiRyokouki.Backend.Models.InternalDtos;

/// <summary>
/// Screen of the client from the <c>X-Display</c> header, used to choose image sizes.
/// </summary>
public record ClientDisplay
{
	/// <summary>
	/// Device pixel ratio.
	/// </summary>
	/// <remarks>
	/// When unknown, 1 is used.
	/// </remarks>
	public float? Dpr { get; set; }

	/// <summary>
	/// Smaller side of the screen in CSS pixels.
	/// </summary>
	/// <remarks>
	/// When unknown, <see cref="Common.Options.MediaSizesOptions.DefaultScreenResolution"/> is used.
	/// </remarks>
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

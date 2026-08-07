using FluentValidation;

namespace HaruyasumiRyokouki.Backend.Models.InternalDtos;

public record ClientDisplay
{
	public float? Dpr { get; set; }
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

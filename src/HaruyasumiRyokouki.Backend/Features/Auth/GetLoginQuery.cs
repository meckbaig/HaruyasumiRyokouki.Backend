using FluentValidation;
using HaruyasumiRyokouki.Backend.Common.Exceptions;
using HaruyasumiRyokouki.Backend.Common.Options;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HaruyasumiRyokouki.Backend.Features.Auth;

public record GetLoginQuery : IRequest<GetLoginResponse>
{
	[FromQuery]
	public required string Login { get; set; }

	[FromQuery]
	public required string Password { get; set; }
}

/// <summary>
/// Response for the GetLoginCommand indicating that the user exists.
/// </summary>
public class GetLoginResponse { }

internal class GetLoginValidator : AbstractValidator<GetLoginQuery>
{
	public GetLoginValidator()
	{
		RuleFor(x => x.Login)
			.NotEmpty();
		RuleFor(x => x.Password)
			.NotEmpty();
	}
}

internal class GetLoginHandler : IRequestHandler<GetLoginQuery, GetLoginResponse>
{
	private readonly ApplicationOptions _options;

	public GetLoginHandler(IOptions<ApplicationOptions> options)
	{
		_options = options.Value;
	}

	public async Task<GetLoginResponse> Handle(GetLoginQuery request, CancellationToken cancellationToken)
	{
		if (_options.AdminLogin != request.Login || _options.AdminPassword != request.Password)
			throw new EntityNotFoundException("User not found.");

		return new GetLoginResponse();
	}
}

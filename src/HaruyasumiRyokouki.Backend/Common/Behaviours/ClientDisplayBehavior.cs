using FluentValidation;
using HaruyasumiRyokouki.Backend.Common.Abstractions;
using HaruyasumiRyokouki.Backend.Models.InternalDtos;
using MediatR;
using System.Globalization;

namespace HaruyasumiRyokouki.Backend.Common.Behaviours;

internal class ClientDisplayBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>, IHeaderAwareBehavior
	where TRequest : IDisplayAwareRequest
{
	private readonly IHttpContextAccessor _httpContextAccessor;
	private readonly IValidator<ClientDisplay> _validator;

	public static string HeaderName => "X-Display";
	public static string Description => "Client display information to determine media sizes in response.";
	public static string Example => "dpr=1.25; min-side=864";
	public static bool Required => false;

	public ClientDisplayBehavior(IHttpContextAccessor httpContextAccessor, IValidator<ClientDisplay> validator)
	{
		_httpContextAccessor = httpContextAccessor;
		_validator = validator;
	}

	public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
	{
		var httpContext = _httpContextAccessor.HttpContext;
		if (httpContext == null)
			return await next(cancellationToken);

		var clientDisplayHeader = httpContext.Request.Headers[HeaderName];
		var headerValue = clientDisplayHeader.FirstOrDefault();
		if (headerValue == null)
			return await next(cancellationToken);

		request.ClientDisplay = new();

		foreach (var part in headerValue.Split(';', StringSplitOptions.TrimEntries))
		{
			var pair = part.Split('=', 2);

			if (pair.Length != 2)
				continue;

			switch (pair[0].ToLowerInvariant())
			{
				case "dpr":
					if (float.TryParse(pair[1],
						NumberStyles.Float,
						CultureInfo.InvariantCulture,
						out var dpr))
					{
						request.ClientDisplay.Dpr = dpr;
					}
					break;

				case "min-side":
					if (int.TryParse(pair[1], out var side))
					{
						request.ClientDisplay.MinSide = side;
					}
					break;
			}
		}

		var result = await _validator.ValidateAsync(request.ClientDisplay, cancellationToken);

		if (!result.IsValid)
			throw new ValidationException(result.Errors);

		return await next(cancellationToken);
	}
}

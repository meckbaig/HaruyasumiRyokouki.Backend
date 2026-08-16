using HaruyasumiRyokouki.Backend.Common.Abstractions;
using MediatR;

namespace HaruyasumiRyokouki.Backend.Common.Behaviours;

internal class ClientAuthentificationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
	where TRequest : IAuthentificatedRequest
{
	private readonly IHttpContextAccessor _httpContextAccessor;

	public ClientAuthentificationBehavior(IHttpContextAccessor httpContextAccessor)
	{
		_httpContextAccessor = httpContextAccessor;
	}

	public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
	{
		var user = _httpContextAccessor.HttpContext?.User;

		request.IsAuthenticated = user?.Identity?.IsAuthenticated ?? false;

		return await next(cancellationToken);
	}
}

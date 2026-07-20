using HaruyasumiRyokouki.Backend.Common.Abstractions;
using MediatR;

namespace HaruyasumiRyokouki.Backend.Common.Behaviours;

public class LocalizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
	where TRequest : ILocalizableRequest
{
	private readonly IHttpContextAccessor _httpContextAccessor;

	public LocalizationBehavior(IHttpContextAccessor httpContextAccessor)
	{
		_httpContextAccessor = httpContextAccessor;
	}

	public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
	{
		var httpContext = _httpContextAccessor.HttpContext;
		if (httpContext != null)
		{
			request.AcceptLanguage = httpContext.Request.Headers.AcceptLanguage.ToString();
		}

		return await next();
	}
}

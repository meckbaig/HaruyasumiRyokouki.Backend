using HaruyasumiRyokouki.Backend.Common.Abstractions;
using MediatR;

namespace HaruyasumiRyokouki.Backend.Common.Behaviours;

internal class LocalizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>, IHeaderAwareBehavior
	where TRequest : ILocalizableRequest
{
	private readonly IHttpContextAccessor _httpContextAccessor;

	public static string HeaderName => "Accept-Language";
	public static string Description => "Localization of response contents.";
	public static string Example => "en";
	public static bool Required => true;

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
		
		/// TODO: add validation and default???

		return await next(cancellationToken);
	}
}

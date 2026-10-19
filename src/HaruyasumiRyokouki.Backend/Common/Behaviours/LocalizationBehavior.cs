using HaruyasumiRyokouki.Backend.Common.Abstractions;
using HaruyasumiRyokouki.Backend.Common.Helpers;
using HaruyasumiRyokouki.Backend.Models.Db.Enums;
using MediatR;

namespace HaruyasumiRyokouki.Backend.Common.Behaviours;

internal class LocalizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>, IHeaderAwareBehavior
	where TRequest : ILocalizableRequest
{
	private readonly IHttpContextAccessor _httpContextAccessor;

	public static string HeaderName => "Accept-Language";
	public static string Description => $"Localization of response contents. Supported: {string.Join(", ", LanguageCode.Supported)}; defaults to '{LanguageCode.Default}'.";
	public static string Example => "en";
	public static bool Required => false;

	public LocalizationBehavior(IHttpContextAccessor httpContextAccessor)
	{
		_httpContextAccessor = httpContextAccessor;
	}

	public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
	{
		var header = _httpContextAccessor.HttpContext?.Request.Headers.AcceptLanguage.ToString();
		request.AcceptLanguage = AcceptLanguageParser.Resolve(header);

		return await next(cancellationToken);
	}
}

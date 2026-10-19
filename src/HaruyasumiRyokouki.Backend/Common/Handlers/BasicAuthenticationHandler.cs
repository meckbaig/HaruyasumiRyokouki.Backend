using HaruyasumiRyokouki.Backend.Common.Helpers;
using HaruyasumiRyokouki.Backend.Common.Options;
using HaruyasumiRyokouki.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace HaruyasumiRyokouki.Backend.Common.Handlers;

internal class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
	private const string LimiterScope = "api:";

	private readonly ApplicationOptions _appOptions;
	private readonly ILoginAttemptLimiter _loginAttemptLimiter;
	private bool _isBlocked;

	public BasicAuthenticationHandler(
		IOptions<ApplicationOptions> appOptions,
		ILoginAttemptLimiter loginAttemptLimiter,
		IOptionsMonitor<AuthenticationSchemeOptions> options,
		ILoggerFactory logger,
		UrlEncoder encoder) : base(options, logger, encoder)
	{
		_appOptions = appOptions.Value;
		_loginAttemptLimiter = loginAttemptLimiter;
	}

	protected override Task<AuthenticateResult> HandleAuthenticateAsync()
	{
		if (!Request.Headers.ContainsKey("Authorization"))
			return Task.FromResult(AuthenticateResult.NoResult());

		string clientKey = LimiterScope + Context.Connection.RemoteIpAddress;
		if (_loginAttemptLimiter.IsBlocked(clientKey))
		{
			_isBlocked = true;
			return Task.FromResult(AuthenticateResult.Fail("Too many failed authentication attempts"));
		}

		if (!BasicCredentials.TryParse(Request.Headers.Authorization, out var username, out var password))
		{
			_loginAttemptLimiter.RegisterFailure(clientKey);
			return Task.FromResult(AuthenticateResult.Fail("Invalid Basic Authentication header"));
		}

		if (!BasicCredentials.AreEqual(username, password, _appOptions.AdminLogin, _appOptions.AdminPassword))
		{
			_loginAttemptLimiter.RegisterFailure(clientKey);
			return Task.FromResult(AuthenticateResult.Fail("Invalid Username or Password"));
		}

		_loginAttemptLimiter.Reset(clientKey);

		var claims = new[] {
			new Claim(ClaimTypes.NameIdentifier, username),
			new Claim(ClaimTypes.Name, username),
		};

		var identity = new ClaimsIdentity(claims, Scheme.Name);
		var principal = new ClaimsPrincipal(identity);
		var ticket = new AuthenticationTicket(principal, Scheme.Name);

		return Task.FromResult(AuthenticateResult.Success(ticket));
	}

	protected override Task HandleChallengeAsync(AuthenticationProperties properties)
	{
		if (!_isBlocked)
			return base.HandleChallengeAsync(properties);

		Response.StatusCode = StatusCodes.Status429TooManyRequests;
		return Task.CompletedTask;
	}
}

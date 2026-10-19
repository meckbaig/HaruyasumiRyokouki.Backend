using HaruyasumiRyokouki.Backend.Common.Helpers;
using HaruyasumiRyokouki.Backend.Common.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace HaruyasumiRyokouki.Backend.Common.Handlers;

internal class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
	private readonly ApplicationOptions _appOptions;

	public BasicAuthenticationHandler(
		IOptions<ApplicationOptions> appOptions,
		IOptionsMonitor<AuthenticationSchemeOptions> options,
		ILoggerFactory logger,
		UrlEncoder encoder) : base(options, logger, encoder)
	{
		_appOptions = appOptions.Value;
	}

	protected override Task<AuthenticateResult> HandleAuthenticateAsync()
	{
		if (!Request.Headers.ContainsKey("Authorization"))
			return Task.FromResult(AuthenticateResult.NoResult());

		if (!BasicCredentials.TryParse(Request.Headers.Authorization, out var username, out var password))
			return Task.FromResult(AuthenticateResult.Fail("Invalid Basic Authentication header"));

		if (!BasicCredentials.AreEqual(username, password, _appOptions.AdminLogin, _appOptions.AdminPassword))
			return Task.FromResult(AuthenticateResult.Fail("Invalid Username or Password"));

		var claims = new[] {
			new Claim(ClaimTypes.NameIdentifier, username),
			new Claim(ClaimTypes.Name, username),
		};

		var identity = new ClaimsIdentity(claims, Scheme.Name);
		var principal = new ClaimsPrincipal(identity);
		var ticket = new AuthenticationTicket(principal, Scheme.Name);

		return Task.FromResult(AuthenticateResult.Success(ticket));
	}
}

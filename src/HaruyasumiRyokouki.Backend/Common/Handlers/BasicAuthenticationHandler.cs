using HaruyasumiRyokouki.Backend.Common.Options;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
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

	protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
	{
		if (!Request.Headers.ContainsKey("Authorization"))
			return AuthenticateResult.Fail("Missing Authorization Header");

		try
		{
			var authHeader = AuthenticationHeaderValue.Parse(Request.Headers.Authorization!);

			if (!authHeader.Scheme.Equals("Basic", StringComparison.OrdinalIgnoreCase))
				return AuthenticateResult.Fail("Not a Basic Authentication Scheme");

			if (string.IsNullOrEmpty(authHeader.Parameter))
				return AuthenticateResult.Fail("Invalid Basic Authentication Parameter");

			var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
			var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':', 2);

			if (credentials.Length != 2)
				return AuthenticateResult.Fail("Invalid Basic Authentication Format");

			var username = credentials[0];
			var password = credentials[1];

			bool isValidUser = (username == _appOptions.AdminLogin && password == _appOptions.AdminPassword);

			if (!isValidUser)
				return AuthenticateResult.Fail("Invalid Username or Password");

			var claims = new[] {
				new Claim(ClaimTypes.NameIdentifier, username),
				new Claim(ClaimTypes.Name, username),
            };

			var identity = new ClaimsIdentity(claims, Scheme.Name);
			var principal = new ClaimsPrincipal(identity);
			var ticket = new AuthenticationTicket(principal, Scheme.Name);

			return AuthenticateResult.Success(ticket);
		}
		catch (Exception ex)
		{
			return AuthenticateResult.Fail($"Authentication failed: {ex.Message}");
		}
	}
}

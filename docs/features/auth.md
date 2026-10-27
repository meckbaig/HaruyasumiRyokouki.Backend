# Admin authentication

There is one administrator. Admin endpoints require HTTP Basic authentication with
`Application:AdminLogin` / `Application:AdminPassword`; public endpoints accept the same header
and show more to the admin. Swagger has its own, separate credentials.

## Files

| File | Role |
| --- | --- |
| `Common/Handlers/BasicAuthenticationHandler.cs` | ASP.NET authentication scheme for the API. |
| `Common/Helpers/BasicCredentials.cs` | Parsing of the `Authorization: Basic` header and constant-time comparison. |
| `Services/LoginAttemptLimiter.cs` | Failed attempt counter per client key (`IMemoryCache`). |
| `Extensions/ApplicationBuilderExtensions.cs` | `RequireSwaggerAuthorization` - Basic auth middleware for `/swagger`. |
| `Common/Behaviors/ClientAuthenticationBehavior.cs` | Copies "is admin" into `IAuthenticatedRequest.IsAuthenticated`. |
| `Controllers/V1/AdminController.cs` | `GET /v1/admin/login` - returns 200 for valid credentials, used by the frontend login form. |

## API authentication

`BasicAuthenticationHandler` runs for every request that has an `Authorization` header:

1. If the client IP is blocked by the limiter - fail; the challenge answers **429** instead of 401.
2. Parse the header. A malformed header (not Basic, bad base64, no `:`) is a failed attempt.
3. Compare login and password with `BasicCredentials.AreEqual` - SHA-256 of both sides and `FixedTimeEquals`, both parts always compared, so timing reveals neither which part matched nor the length.
4. Success resets the counter for the IP.

A password may contain `:` - only the first `:` separates the login.

Endpoints with `[Authorize]` answer 401 (or 429) without valid credentials. Other endpoints work
anonymously; for requests implementing `IAuthenticatedRequest` the handler gets
`IsAuthenticated = true` only for valid admin credentials. What the admin sees in addition is
described in [reading-and-visibility.md](reading-and-visibility.md).

## Brute-force protection

`LoginAttemptLimiter` counts failures per key in memory:

- Key: `api:<ip>` for the API, `swagger:<ip>` for Swagger - the two are counted separately.
- After `Application:MaxFailedLoginAttempts` failures (default 10) the key is blocked.
- The window is `Application:FailedLoginLockoutMinutes` (default 15) **from the first failure**: the cache entry expires then, which both ends the counting window and lifts the block.
- A successful login removes the key.

## Swagger authentication

With `SwaggerAuth:RequireLogin = true` every request under `/swagger` needs Basic credentials
from `SwaggerAuth:Username` / `Password`; otherwise 401 with `WWW-Authenticate: Basic` (the
browser shows its login dialog). Swagger has no access to the API by itself - "Authorize" in the
Swagger UI takes the admin credentials.

## Invariants

- **A wrong `Authorization` header on a public endpoint counts as a failed attempt**, even though the request itself succeeds anonymously. A client that keeps sending stale credentials gets blocked and then receives 429 on public endpoints too.
- **A Swagger request without the header is not a failure** - it is the browser asking for credentials. Only a request with wrong credentials counts.
- **The limiter state is in memory**: a restart clears all blocks, and several instances would count separately. The application is deployed as a single instance.
- **The real client IP depends on `Application:KnownProxies`.** Behind a reverse proxy that is not listed, every client has the proxy IP and one attacker blocks everyone.
- Credentials are compared on every request; there are no sessions or tokens.

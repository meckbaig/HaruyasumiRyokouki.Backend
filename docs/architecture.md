# Architecture

ASP.NET Core 8 Web API with MediatR (one request type per endpoint), FluentValidation,
Entity Framework Core on PostgreSQL. One project: `src/HaruyasumiRyokouki.Backend`. Unit tests
live in `tests/HaruyasumiRyokouki.Backend.UnitTests`. Read this page before changing anything;
the feature pages in [features/](features/) assume it.

## Layers

```
Controllers/V1 ── MediatR ── Features/<Area>/<Request>.cs ── Services/ ── IFileStorage, ffmpeg, AI, ...
                                 │                              │
                                 └──────── IAppDbContext ───────┘
```

| Directory | Rule |
| --- | --- |
| `Controllers/V1/` | Routing, `[Authorize]` and nothing else. Every action sends its request to MediatR and returns `ToJsonResponse()`. |
| `Features/<Area>/` | One file per endpoint: the request record, its response, its validator and its handler. The handler is the use case. |
| `Services/` | Everything registered in DI. Grouped by domain in subfolders (`BackgroundServices/`, `Translation/`, `Builders/`). Interfaces in `Services/Interfaces/`. |
| `Common/` | Cross-cutting infrastructure: behaviors, exceptions, options and their validators, health checks, `Optional<T>`. |
| `Common/Helpers/` | Stateless static helpers - pure functions, no DI. Most of them are unit tested. |
| `Extensions/` | Extension methods only: DI registration, middleware, mapping (`MappingExtensions`), EF query helpers (`LinqExtensions`). |
| `Models/Db/` | EF entities. `Models/Dtos/` - API contracts. `Models/InternalDtos/` - values passed between services. |

Code conventions are in [conventions.md](conventions.md).

## The request pipeline

ASP.NET middleware, in `Program.cs` order:

1. `UseForwardedHeadersFromKnownProxies` - real client IP and scheme behind a reverse proxy. It runs first because the login limiter counts failures per IP.
2. `RequireSwaggerAuthorization` - Basic auth for `/swagger` when `SwaggerAuth:RequireLogin` is on.
3. Swagger, exception handler, CORS (any origin), controllers, `/health`, `/metrics`, authentication, authorization.

MediatR behaviors, outermost first:

| Behavior | Applies to | Does |
| --- | --- | --- |
| `UnhandledExceptionBehavior` | all | Logs validation errors and cancellations at Information, rethrows everything unchanged. |
| `ClientDisplayBehavior` | `IDisplayAwareRequest` | Parses `X-Display` into `request.ClientDisplay`. See [media-urls.md](features/media-urls.md). |
| `ValidationBehavior` | all | Runs every FluentValidation validator of the request; failures become `ValidationException`. |
| `LocalizationBehavior` | `ILocalizableRequest` | Sets `request.AcceptLanguage` from the header. See [localization-and-translation.md](features/localization-and-translation.md). |
| `ClientAuthenticationBehavior` | `IAuthenticatedRequest` | Sets `request.IsAuthenticated` - whether a valid admin `Authorization` header came with the request. |

Properties filled by behaviors are marked `[JsonIgnore]` and `[SwaggerIgnore]` on the request,
so a client cannot set them.

## Errors

`CustomExceptionHandler` maps exception types to status codes, matching by inheritance:

| Exception | Status | Used for |
| --- | --- | --- |
| `ValidationException` (FluentValidation) | 400 | Validators, and handlers for checks that need data (e.g. already imported YouTube video). |
| `UnauthorizedException` | 401 | Not used now, kept as infrastructure. |
| `ForbiddenAccessException` | 403 | Not used now, kept as infrastructure. |
| `EntityNotFoundException` | 404 | Missing day, media or tag. `ThrowIfAnyMissing` lists all missing ids of a batch. |
| `ConflictException` | 409 | A second media sync while one is running. |
| `ExternalServiceException` | 502 | yt-dlp, thumbnail download, ffmpeg in YouTube import. |
| `ServiceUnavailableException` | 503 | Similarity search disabled in the configuration. |
| anything else | 500 | Logged with the stack trace. |

The response is `ProblemDetails`; the 500 response carries the exception message (the inner
one if present) **on purpose** - the only clients are the author's own frontend and admin.

## Configuration

Every options section is validated on startup (`IValidateOptions<T>` in
`Common/Options/Validators/`, `ValidateOnStart`). An invalid value stops the application with exit
code `1` and a message naming the setting; several invalid sections are all reported. Sections
that decide which services are registered (storage provider, preview provider, connection string)
are read directly from configuration during registration (`GetOptions<T>` in
`ServiceCollectionExtensions`) and validated the same way later. The keys are described in the
[README](../README.md#configuration).

## Data

- Tables and columns are snake_case (`EFCore.NamingConventions`).
- `Day` 1 - * `MediaFile`, `MediaFile` * - * `Tag` through `media_file_tag`. Days, media and tags have translation tables (`*_translations`) with one row per language; tags also store search aliases there (`is_primary = false`).
- `media_embeddings` (one vector per media file) is filled by an external tool, see [similarity-search.md](features/similarity-search.md).
- Media and days are created by sync and YouTube import, never by a plain "create" endpoint.
- Migrations are in `Migrations/`; `dotnet ef database update` applies them (see README).

## Invariants

- **Validators run before `AcceptLanguage` and `IsAuthenticated` are set.** `ValidationBehavior` is registered before `LocalizationBehavior` and `ClientAuthenticationBehavior`, so a validator sees their default values. A rule that depends on the language or on the admin must live in the handler.
- **A malformed request body arrives as `Body = null`, not as a 400.** `SuppressModelStateInvalidFilter` is on, so model binding errors are not answered automatically; every validator of a request with a body starts with `RuleFor(x => x.Body).NotNull()` and validates the body through a nested validator. Rules written as `RuleFor(x => x.Body.Field)` throw on a null body (500).
- **`Must` after `NotNull` needs `Cascade(CascadeMode.Stop)`.** FluentValidation runs the rest of the chain after a failed `NotNull`, and a lambda over a null collection throws.
- **Mapping is manual** (`MappingExtensions`). A new DTO field is not filled until the mapping is updated.

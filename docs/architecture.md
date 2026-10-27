# Architecture

The backend is one ASP.NET Core 8 project (`src/HaruyasumiRyokouki.Backend`) on top of PostgreSQL. Every endpoint is a MediatR request: the controller only receives it and sends it on, and everything else - validation, language, screen size, the admin check and the work itself - happens in the request pipeline and the handler. Unit tests are in `tests/HaruyasumiRyokouki.Backend.UnitTests`. Folder structure and code style are described in [conventions.md](conventions.md).

## How a request is processed

A request first passes the ASP.NET middleware. Forwarded headers come first, so that behind a reverse proxy the application sees the real client IP - the login brute-force protection counts failures per IP, and with the proxy IP it would block everyone at once. Then Swagger authentication, the exception handler, CORS and authentication.

The controller sends the request object to MediatR, where it passes several behaviors before the handler:

1. Unhandled exception logging.
2. `X-Display` parsing - the screen size of the client, for image URLs.
3. Validation with FluentValidation.
4. `Accept-Language` parsing - the response language.
5. The admin check - whether valid admin credentials came with the request.

The order matters in one place: validation runs **before** the language and the admin flag are set. A validator always sees the default language and "not admin", so a rule that depends on them belongs in the handler.

A malformed request body does not stop the request with a 400 automatically (`SuppressModelStateInvalidFilter` is on). The body simply arrives as `null`, and the validators of every request with a body check it first. A rule written as `RuleFor(x => x.Body.Field)` would throw on such a request and return 500, so body fields are validated through a nested validator of the body.

## Errors

Handlers report problems with exceptions, and one exception handler turns them into responses with `ProblemDetails`:

| Exception | Status |
|---|---|
| `ValidationException` | 400 |
| `UnauthorizedException`, `ForbiddenAccessException` | 401, 403 (not used now) |
| `EntityNotFoundException` | 404 |
| `ConflictException` | 409, e.g. a second media sync while one is running |
| `ExternalServiceException` | 502, an external tool (yt-dlp, ffmpeg) failed |
| `ServiceUnavailableException` | 503, a feature is turned off in the configuration |
| anything else | 500, logged with the stack trace |

The message of a 500 error is returned to the client on purpose: the only clients are the author's own site and admin tools.

## Configuration

Every options section is validated on startup. An invalid value stops the application with exit code `1` and a message naming the setting. The settings that decide which services exist at all - the storage provider, the preview provider, the connection string - are read directly from the configuration while services are registered. The keys are described in the [README](../README.md#configuration).

## Data

Days, media files and tags each have a table of translations, one row per language. Tags also keep their search aliases there. Days and media are never created by a "create" endpoint: they appear when media are imported from the file storage or from YouTube (see [media-sync.md](features/media-sync.md)). The `media_embeddings` table is filled by a separate tool (see [similarity-search.md](features/similarity-search.md)).

Mapping between entities and DTOs is written by hand in `MappingExtensions`, so a new field reaches the API only after the mapping is updated.

## State kept in memory

Some state lives only in the memory of the process:

- the lock that allows one media sync at a time;
- the failed login counters;
- the queue of files to delete from the storage;
- the similarity index with all image embeddings.

That is why the application must run as **a single instance**. A restart clears the login counters and loses files that were queued for deletion but not deleted yet (see [issues.md](issues.md)).

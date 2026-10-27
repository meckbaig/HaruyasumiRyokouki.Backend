# HaruyasumiRyokouki - Backend

Backend of a personal travelogue: a website with notes, photos and videos from a spring trip to Japan, organized by days. The frontend lives in a separate repository: [HaruyasumiRyokouki.Frontend](https://github.com/meckbaig/HaruyasumiRyokouki.Frontend).

What the backend does:

- Stores days of the trip with notes, and media (photos, videos, YouTube videos) with titles, descriptions, coordinates and tags.
- Serves all texts in three languages - English, Russian and Japanese - picked by the `Accept-Language` header.
- Imports media from a file storage (a local folder or WebDAV, e.g. Nextcloud): converts images to an optimized format, creates video previews, lower bitrate versions for streaming and tiny placeholder images.
- Builds media URLs for the client screen size through a preview service (Nextcloud previews or imgproxy).
- Searches days and media by text and by tags, suggests tags while typing.
- Provides an admin API (HTTP Basic authentication): approving and editing media and days, private and favorite media, tags with aliases, AI translation and AI tag completion, YouTube import.

Built with .NET 8, ASP.NET Core, Entity Framework Core with PostgreSQL, MediatR and FluentValidation. Media processing uses ffmpeg, YouTube metadata uses yt-dlp.

## Requirements

- .NET 8 SDK.
- PostgreSQL. The database user must be allowed to create the `pg_trgm` extension (a migration creates it), or the extension must be created in advance.
- `ffmpeg` and `ffprobe` available in `PATH` - for media sync.
- `yt-dlp` available in `PATH` - only for YouTube import.
- The `Meckbaig.*` NuGet packages in the `nuget/` folder of the repository. They are not published on nuget.org; `nuget.config` points to this folder as a local package source, and `*.nupkg` files are not committed.

## Getting started

1. Put the `Meckbaig.*` packages into `nuget/` and restore:

   ```
   dotnet restore
   ```

2. Configure the application. [`appsettings.json`](src/HaruyasumiRyokouki.Backend/appsettings.json) is an example config for development and the first run. Keep real values (passwords, API keys, connection strings) out of it: in development use [user secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) (the project already has a `UserSecretsId`), elsewhere use environment variables (`Section__Key`, e.g. `ConnectionStrings__Default`) or a separate config file. See [First run checklist](#first-run-checklist) for what has to be changed.

3. Create the database schema:

   ```
   dotnet tool install --global dotnet-ef
   dotnet ef database update --project src/HaruyasumiRyokouki.Backend
   ```

   The command applies migrations to the database from `ConnectionStrings:Default` of the `Development` environment, including user secrets. Make sure it points to the intended database.

4. Run:

   ```
   dotnet run --project src/HaruyasumiRyokouki.Backend
   ```

   The API listens on `http://localhost:5101`:

   - `/swagger` - API documentation (behind `SwaggerAuth` credentials when `SwaggerAuth:RequireLogin` is `true`);
   - `/health` - health check of the database and the other components, see [Health check](#health-check);
   - `/metrics` - Prometheus metrics.

5. Fill the database with media: put files into the file storage and call `PUT /v1/media/sync` as admin. Imported media are not approved and are visible only to the admin until they are approved.

The application validates all configuration sections on startup. An invalid value stops it with a message naming the setting and exit code `1`.

### First run checklist

| Setting | What to set |
|---|---|
| `ConnectionStrings:Default` | Connection string to your PostgreSQL database. |
| `Application:AdminLogin`, `Application:AdminPassword` | Admin credentials for the API. Do not keep the example `admin` / `password`. |
| `SwaggerAuth:Username`, `SwaggerAuth:Password` | Credentials for Swagger, or set `SwaggerAuth:RequireLogin` to `false` locally. |
| `Application:KnownProxies` | IP addresses of reverse proxies in front of the application, if any. |
| `MediaStorage:Provider` | `Local` or `WebDav`, then `LocalStorageProvider:Path` or the `WebDavStorageProvider` section. |
| `MediaPreview` | Provider and URLs of the original files and of the preview service. With the example values the API works, but media URLs point to placeholder hosts. |
| `MediaFormat:TravelDateOffsets` | Date ranges of the trip and their time zone offsets. Files with a modification time outside every range are skipped by sync. |
| `TranslationProvider` | API keys of the AI and translation providers. Only needed for auto-translation and tag completion. |
| `Seq:Enabled` | `false` if there is no Seq server. |

## Configuration

All sections are in [`appsettings.json`](src/HaruyasumiRyokouki.Backend/appsettings.json). The application validates every section on startup.

Keys marked with `*` are dynamic: you choose their names (they are referred to by name from other settings) or the number of list items. All other keys are fixed: only their values can be changed.

```jsonc
{
  "Application": {
    // Number of retries of database operations after transient errors.
    "CheckDbRetryCount": 10,
    // Maximum delay between retries, in seconds.
    "CheckDbRetryDelay": 60,
    // * IP addresses of reverse proxies whose X-Forwarded-For and X-Forwarded-Proto headers are trusted.
    //   Needed behind a proxy to get the real client IP (used by the brute-force protection) and scheme.
    "KnownProxies": [ "127.0.0.1" ],
    // Credentials of the single admin (HTTP Basic authentication).
    "AdminLogin": "admin",
    "AdminPassword": "password",
    // Failed authentication attempts from one IP after which further attempts get 429. Default 10.
    "MaxFailedLoginAttempts": 10,
    // Minutes from the first failed attempt during which failures are counted. Also the lockout duration. Default 15.
    "FailedLoginLockoutMinutes": 15,
    // Search of similar media by image embeddings from the media_embeddings table (filled by an external tool).
    // false: the vectors are not loaded into memory and the similarity endpoints return 503. Default true.
    "SimilaritySearchEnabled": true
  },

  "SwaggerAuth": {
    // Protect /swagger with HTTP Basic authentication.
    "RequireLogin": true,
    // Swagger credentials, separate from the admin ones. Required only with RequireLogin.
    "Username": "youruser",
    "Password": "yourpassword"
  },

  "ConnectionStrings": {
    // PostgreSQL connection string. The user must be allowed to create the pg_trgm extension.
    "Default": "Server=localhost;Port=5440;Database=ryokouki;User ID=user;Password=test;"
  },

  "MediaStorage": {
    // Where the original media files are: Local (a folder on the server) or WebDav (e.g. a Nextcloud folder).
    "Provider": "Local"
  },
  "LocalStorageProvider": {
    // Folder with the media files, for the Local provider.
    "Path": "D:\\Media"
  },
  "WebDavStorageProvider": {
    // URL of the media folder on the WebDAV server, with a trailing slash, for the WebDav provider.
    "Endpoint": "https://yoursite.com/remote.php/dav/files/youruser/files/yourfolder/",
    "Username": "youruser",
    "Password": "yourpassword"
  },

  // How media URLs are built. The backend does not serve files: clients load originals
  // from the storage and resized images from the preview service.
  "MediaPreview": {
    // Preview service for resized images: Nextcloud or Imgproxy.
    "Provider": "Nextcloud",
    // URLs of the original files, used for downloads and video streams.
    "OriginStorage": {
      // Path after the endpoint. Must contain {fileName}.
      "PayloadStringBase": "public.php/dav/files/yourfolderid/{fileName}",
      // Base URL of the server.
      "Endpoint": "https://yoursite.com",
      // Optional CDN in front of the server. When set, it is used for media URLs and checked by /health.
      "CdnEndpoint": "https://origin-yoursite.cdn.com",
      // Use the CDN for download URLs too.
      "UseCdnForDownloads": false
    },
    // Nextcloud public previews, used with the Nextcloud provider.
    "Nextcloud": {
      // Path after the endpoint. Must contain {fileName}, {xAxis} and {yAxis} (preview size in pixels).
      "PayloadStringBase": "apps/files_sharing/publicpreview/yourfolderid?file={fileName}&x={xAxis}&y={yAxis}&a=1",
      "Endpoint": "https://yoursite.com",
      "CdnEndpoint": "https://nextcloud-yoursite.cdn.com",
      "UseCdnForDownloads": false
    },
    // imgproxy, used with the Imgproxy provider.
    "Imgproxy": {
      "Endpoint": "https://yoursite.com",
      // Source prefix imgproxy reads the original files from, e.g. local:/// or s3://bucket/.
      "FilePath": "",
      // Hex-encoded key and salt for signed URLs. May be empty only with Insecure.
      "Key": "",
      "Salt": "",
      // Allow unsigned URLs.
      "Insecure": true,
      "CdnEndpoint": "https://imgproxy-yoursite.cdn.com",
      "UseCdnForDownloads": false
    }
  },

  // Image sizes requested from the preview service. Sizes are rounded up to a fixed set,
  // so the preview service caches a limited number of variants.
  "MediaSizes": {
    // * Allowed image sizes in pixels. A size above the largest one gets the largest.
    "SizeBuckets": [ 256, 1024, 4096 ],
    // Thumbnail size in CSS pixels, multiplied by the device pixel ratio of the client.
    "PreviewTargetCss": 200,
    // Smaller side of the screen when the client does not send X-Display.
    "DefaultScreenResolution": 768,
    // Aspect ratio used when it is unknown. Default 4/3.
    "DefaultAspectRatio": 1.3333
  },

  "MediaFormat": {
    // Name of the FfmpegPresets:Image preset. Also the target image format and extension.
    "ImagePreset": "webp",
    // Name of the FfmpegPresets:Video preset for the streaming versions of videos.
    "VideoPreset": "h264_1440p_2pass_20M",
    // Name of the FfmpegPresets:VideoThumbnailPrefix preset: how the frame for a video preview is chosen.
    "VideoThumbnailPreset": "first_frame",
    // Name of the FfmpegPresets:Miniature preset.
    "MiniaturePreset": "webp",
    // Size in pixels of the tiny placeholder image stored in the database and shown while media load.
    "MiniatureSize": 16,
    // Number of random favorites returned for the home page.
    "FavoritesReturnCount": 15,
    // Video bitrate in bits per second above which a streaming version is created. Default 26214400 (25 Mbit).
    "VideoConversionBitrateThreshold": 26214400,
    // * Time zone of the trip by UTC ranges. Converts file modification times (for files without
    //   a date in the name) and YouTube publish times to the local time of the trip.
    //   From and To are optional and inclusive, Offset is in hours. At least one range is required.
    //   Files outside every range are skipped by sync.
    "TravelDateOffsets": [
      { "From": "2026-02-26T00:00:00", "To": "2026-05-26T04:59:59", "Offset": 9 },
      { "From": "2026-05-26T05:00:00", "To": "2026-05-27T23:59:59", "Offset": 8 }
    ]
  },

  // Named sets of ffmpeg arguments. Add any number of presets, MediaFormat selects one of each kind by name.
  "FfmpegPresets": {
    "Image": {
      // * Image conversion and video preview. The name is also the output extension.
      //   Tokens: {input}, {output}, {videoThumbnailPrefix}.
      "webp": "-i \"{input}\" {videoThumbnailPrefix} -c:v libwebp -quality 88 -compression_level 6 -preset picture \"{output}\""
    },
    "Video": {
      // * Video conversion as a list of ffmpeg runs, e.g. two passes. Tokens: {input}, {output}.
      "h264_1440p_2pass_20M": [ "... -pass 1 -an -f null -", "... -pass 2 -c:a copy -movflags +faststart \"{output}\"" ]
    },
    "VideoThumbnailPrefix": {
      // * Arguments inserted into the image preset to pick the frame of a video preview.
      "first_frame": "-ss 00:00:01 -frames:v 1"
    },
    "Miniature": {
      // * Placeholder image. The output must go to stdout (pipe:1). Tokens: {input}, {miniatureSize}.
      "webp": "-i \"{input}\" -vf \"scale=...\" -c:v libwebp -quality 75 -f webp pipe:1"
    }
  },

  // AI and translation services for auto-translation of days and media and for tag completion.
  "TranslationProvider": {
    "Providers": {
      // * Provider entries. The fields depend on Type:
      //   OpenAi - any OpenAI-compatible chat API: ApiKey, Model, Temperature (0 to 1), ReasoningLevel, ApiUrl.
      //   GeminiAi - Google Gemini: ApiKey, Model, Temperature, ReasoningLevel (none, minimal, low, medium, high).
      //   Deepl - DeepL translation API: ApiKey.
      "DeepseekFlash": {
        "Type": "OpenAi",
        "ApiKey": "XXX",
        "Model": "deepseek-v4-flash",
        "Temperature": 0.2,
        "ReasoningLevel": "low",
        "ApiUrl": "https://api.deepseek.com"
      },
      "GeminiFlash": { "Type": "GeminiAi", "ApiKey": "XXX", "Model": "gemini-3.7-flash", "Temperature": 0.2, "ReasoningLevel": "none" },
      "Deepl": { "Type": "Deepl", "ApiKey": "XXX" }
    },
    // Which entry of Providers is used for what.
    "Usage": {
      // Tag completion: translations and search aliases of a new tag. Needs JSON output, so OpenAi or GeminiAi only.
      "Tags": "DeepseekFlash",
      // Translation of media titles and descriptions.
      "Media": "GeminiFlash",
      // Translation of day notes.
      "Days": "Deepl"
    }
  },

  "YouTube": {
    // Proxy passed to yt-dlp, for servers where YouTube is blocked.
    "Proxy": { "Enabled": false, "Address": "" }
  },

  // Logging. Each sink has Enabled and MinimumLevel.
  "ConsoleLog": { "Enabled": true, "MinimumLevel": "Information" },
  "DebugLog": { "Enabled": false, "MinimumLevel": "Debug" },
  // Daily files in FileLogs/ next to the application binaries.
  "FileLog": { "Enabled": true, "MinimumLevel": "Information", "RetainedDaysCountLimit": 30 },
  "Seq": {
    "Enabled": true,
    "ServerUrl": "http://localhost:5341",
    "ApiKey": "",
    "MinimumLevel": "Information",
    // * Optional log levels by source name.
    "LevelOverride": { "Microsoft.EntityFrameworkCore": "Warning" }
  },
  // Standard ASP.NET Core log level overrides.
  "Logging": { "LogLevel": { "Default": "Information" } },

  // Listening address. Takes precedence over ASPNETCORE_URLS.
  "Kestrel": { "Endpoints": { "Http": { "Url": "http://*:5101" } } }
}
```

## Using the API

- Routes are versioned: `/v1/...`. Swagger lists all endpoints with request and response schemas.
- `Accept-Language` - response language: `en`, `ru` or `ja`. Browser-style values such as `ru-RU,ru;q=0.9` are supported. Optional, defaults to `en`.
- `X-Display` - client screen, used to choose image sizes: `dpr=1.25; min-side=864`. Optional.
- `Authorization: Basic ...` - admin requests. Without it, private media and media that are not approved yet are hidden.
- Errors are returned as RFC 9457 problem details: `400` - validation, `404` - not found, `409` - conflict (e.g. a sync is already running), `429` - too many failed logins, `502` - an external service failed (yt-dlp, preview download), `503` - the feature is disabled (similarity search), `500` - other errors.

## Health check

`GET /health` (no authentication) checks the components and returns the overall status and one entry per check.

| Check | Required | What is checked |
|---|---|---|
| `database` | Yes | Connection to PostgreSQL. |
| `storage` | Yes | `Local`: the media folder exists. `WebDav`: `PROPFIND` of the media folder succeeds with the configured credentials. |
| `preview` | Yes | The preview service answers on its status URL: `<Endpoint>/health` for imgproxy, `<Endpoint>/status.php` for Nextcloud. |
| `cdn` | Yes | Every configured `CdnEndpoint` (origin storage and the active preview provider) returns the first local image from the database: the original and the smallest preview. Healthy when no CDN is configured. |
| `similarity-search` | No | Number of loaded vectors. `Disabled` when `Application:SimilaritySearchEnabled` is `false`; unhealthy when the search is enabled but no vectors are loaded. |
| `ffmpeg`, `ffprobe` | No | The tool starts; its version is shown. Needed for media sync. |
| `yt-dlp` | No | The tool starts; its version is shown. Needed only for YouTube import. |

Overall status: `Unhealthy` (HTTP `503`) when a required check fails; `Degraded` (HTTP `200`) when only optional checks fail; otherwise `Healthy`.

## Where things live

All code is in `src/HaruyasumiRyokouki.Backend`, unit tests in `tests/HaruyasumiRyokouki.Backend.UnitTests`.

| Directory | Contents |
|---|---|
| `Controllers/V1/` | Routes and `[Authorize]` only; every action sends a MediatR request. |
| `Features/<Area>/` | One file per endpoint: request, response, validator, handler. |
| `Services/` | DI services: storage, ffmpeg, preview URLs, translation, sync, background services. |
| `Common/` | Behaviors, exceptions, options and their validators, health checks, static helpers. |
| `Extensions/` | DI registration, middleware, mapping, EF query helpers. |
| `Models/` | EF entities (`Db/`), API contracts (`Dtos/`), internal values (`InternalDtos/`). |
| `Migrations/` | EF Core migrations. |

### Finding a feature

| Feature | Start here | Documentation |
|---|---|---|
| Layers, request pipeline, errors, configuration | `Program.cs` | [architecture.md](docs/architecture.md) |
| Admin login, brute-force protection, Swagger login | `Common/Handlers/BasicAuthenticationHandler.cs` | [auth.md](docs/features/auth.md) |
| Days, map, favorites, pending, what visitors see | `Features/Days/GetDayQuery.cs` | [reading-and-visibility.md](docs/features/reading-and-visibility.md) |
| Editing media and days, auto-translation, deleting | `Features/Media/EditMediaCommand.cs` | [editing.md](docs/features/editing.md) |
| Media import from the storage, conversion, dates | `Services/MediaSyncService.cs` | [media-sync.md](docs/features/media-sync.md) |
| YouTube videos | `Features/Media/ImportYoutubeMediaCommand.cs` | [youtube-import.md](docs/features/youtube-import.md) |
| Media URLs, preview services, image sizes, `X-Display` | `Services/MediaUrlsProvider.cs` | [media-urls.md](docs/features/media-urls.md) |
| Languages, `Accept-Language`, translation providers | `Common/Helpers/TranslationPlanner.cs` | [localization-and-translation.md](docs/features/localization-and-translation.md) |
| Tags, aliases, tag completion, search | `Features/Search/SearchQuery.cs` | [tags-and-search.md](docs/features/tags-and-search.md) |
| Similar media, media suggestions for a tag | `Services/MediaSimilarityIndexService.cs` | [similarity-search.md](docs/features/similarity-search.md) |
| Health, metrics, logs, in-memory state | `Common/HealthChecks/` | [operations.md](docs/features/operations.md) |

## Running tests

```
dotnet test
```

Unit tests cover code that needs no database, files or network: helpers, validators, JSON handling of `Optional<T>`, URL building, the login limiter. Handlers that use the database are not unit tested.

## Deploying

Docker is optional: the application is a regular .NET program and can run directly on a server.

In both cases:

- Publish into an empty folder. Files left from previous builds can break startup.
- Provide the production configuration through environment variables or replace `appsettings.json` in the published folder. User secrets are used only in the `Development` environment.
- Migrations are not applied on startup. Apply them with `dotnet ef database update` (with the production connection string) or with an SQL script from `dotnet ef migrations script --idempotent`.
- The database, the file storage and the preview service are external.

### Without Docker

1. Put the `Meckbaig.*` packages into `nuget/` and publish:

   ```
   dotnet publish src/HaruyasumiRyokouki.Backend -c Release -o <output folder>
   ```

2. Make `ffmpeg`, `ffprobe` and `yt-dlp` available to the application: put their executables next to `HaruyasumiRyokouki.Backend.exe` in the output folder or add them to `PATH`. `yt-dlp` is needed only for YouTube import.
3. Configure the application (see [First run checklist](#first-run-checklist)) and run `HaruyasumiRyokouki.Backend.exe` (or `dotnet HaruyasumiRyokouki.Backend.dll`). The listening address is set by `Kestrel:Endpoints:Http:Url`.

### With Docker

The [`Dockerfile`](src/HaruyasumiRyokouki.Backend/Dockerfile) builds an image from an already published application and installs ffmpeg and yt-dlp. [`docker-compose.yml`](src/HaruyasumiRyokouki.Backend/docker-compose.yml) runs it on port `5104` (container port `8080`).

```
cd src/HaruyasumiRyokouki.Backend
dotnet publish -c Release -o publish
docker compose up -d --build
```

Set `Kestrel:Endpoints:Http:Url` to `http://+:8080` (or remove the `Kestrel` section), otherwise the example value overrides `ASPNETCORE_URLS` from the compose file.

## Documentation

Technical detail lives in [docs/](docs/), one file per feature. Each feature page lists its files first and ends with **Invariants** - rules that look like bugs and are not, or that are easy to break.

- [docs/architecture.md](docs/architecture.md) - read before changing anything.
- [docs/issues.md](docs/issues.md) - known problems left for later.
- [docs/conventions.md](docs/conventions.md) - code structure conventions.
- Swagger (`/swagger`) - API reference generated from the code.
- [HaruyasumiRyokouki.Frontend](https://github.com/meckbaig/HaruyasumiRyokouki.Frontend) - the client application.

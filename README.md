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
   - `/health` - health check, including the database connection;
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

All sections are in [`appsettings.json`](src/HaruyasumiRyokouki.Backend/appsettings.json).

### Fixed and user-defined settings

Most keys are fixed: their names are read by the application and only values can be changed. Some sections are user-defined: you add entries with your own names or any number of list items, and other settings refer to them by name.

| Setting | Kind | How it is used |
|---|---|---|
| `FfmpegPresets:Image:<name>` | Named entries, any names | Image conversion presets. One is selected by `MediaFormat:ImagePreset`. The name is also the output file extension, so it must be an image format ffmpeg can write (e.g. `webp`, `avif`). |
| `FfmpegPresets:Video:<name>` | Named entries, any names | Video conversion presets, each a list of ffmpeg runs. One is selected by `MediaFormat:VideoPreset`. |
| `FfmpegPresets:VideoThumbnailPrefix:<name>` | Named entries, any names | Arguments that pick a frame for the video preview. One is selected by `MediaFormat:VideoThumbnailPreset`. |
| `FfmpegPresets:Miniature:<name>` | Named entries, any names | Placeholder image presets. One is selected by `MediaFormat:MiniaturePreset`. The output must go to stdout (`pipe:1`). |
| `TranslationProvider:Providers:<name>` | Named entries, any names | AI and translation providers. `TranslationProvider:Usage` selects which provider is used for what. The fields of an entry depend on its `Type`, see [TranslationProvider](#translationprovider). |
| `MediaFormat:TravelDateOffsets` | List | Date ranges of the trip with time zone offsets, any number of items, at least one. |
| `MediaSizes:SizeBuckets` | List | Allowed image sizes, any number of items. |
| `Application:KnownProxies` | List | Trusted reverse proxy IP addresses, can be empty. |
| `Seq:LevelOverride` | Named entries | Optional log level overrides by source name. |

Everything else is fixed.

### Application

| Key | Description |
|---|---|
| `CheckDbRetryCount` | Number of retries of database operations after transient errors. |
| `CheckDbRetryDelay` | Maximum delay between retries, in seconds. |
| `KnownProxies` | IP addresses whose `X-Forwarded-For` and `X-Forwarded-Proto` headers are trusted. Needed behind a reverse proxy to get the real client IP and scheme. |
| `AdminLogin`, `AdminPassword` | Credentials of the single administrator (HTTP Basic authentication). |
| `MaxFailedLoginAttempts` | Failed authentication attempts from one IP after which further attempts get `429`. Default `10`. |
| `FailedLoginLockoutMinutes` | Window in minutes in which failed attempts are counted; also the lockout duration. Default `15`. |

### SwaggerAuth

| Key | Description |
|---|---|
| `RequireLogin` | Whether Swagger (`/swagger`) requires HTTP Basic authentication. |
| `Username`, `Password` | Swagger credentials, separate from the admin ones. Required only when `RequireLogin` is `true`. |

### ConnectionStrings

| Key | Description |
|---|---|
| `Default` | PostgreSQL connection string. |

### MediaStorage, LocalStorageProvider, WebDavStorageProvider

Where the original media files are stored.

| Key | Description |
|---|---|
| `MediaStorage:Provider` | `Local` - a folder on the server, `WebDav` - a WebDAV server (e.g. a Nextcloud folder). |
| `LocalStorageProvider:Path` | Folder with media files for the `Local` provider. |
| `WebDavStorageProvider:Endpoint` | URL of the media folder on the WebDAV server, with a trailing slash. |
| `WebDavStorageProvider:Username`, `Password` | WebDAV credentials. |

### MediaPreview

How URLs to media are built. The backend does not serve files itself: clients load originals from the storage and resized images from the preview service.

| Key | Description |
|---|---|
| `Provider` | Image preview service: `Nextcloud` or `Imgproxy`. |
| `OriginStorage` | URLs of original files, used for downloads and video streams. |
| `Nextcloud` | Nextcloud public preview URLs, used when `Provider` is `Nextcloud`. |
| `Imgproxy` | imgproxy settings, used when `Provider` is `Imgproxy`. |

`OriginStorage` and `Nextcloud`:

| Key | Description |
|---|---|
| `Endpoint` | Base URL of the server. |
| `CdnEndpoint` | Optional base URL of a CDN in front of the server. Used for media when set. |
| `UseCdnForDownloads` | Whether download URLs also use `CdnEndpoint`. |
| `PayloadStringBase` | Path template appended to the base URL. `OriginStorage` requires the `{fileName}` token, `Nextcloud` requires `{fileName}`, `{xAxis}` and `{yAxis}` (preview size in pixels). |

`Imgproxy`:

| Key | Description |
|---|---|
| `Endpoint`, `CdnEndpoint`, `UseCdnForDownloads` | Same as above. |
| `FilePath` | Source prefix imgproxy uses to read original files, e.g. `local:///` or `s3://bucket/`. |
| `Key`, `Salt` | Hex-encoded imgproxy key and salt for signed URLs. |
| `Insecure` | `true` allows empty `Key` and `Salt` (unsigned URLs). |

### MediaSizes

Image sizes requested from the preview service. Sizes are rounded up to a fixed set of buckets so the preview service caches a limited number of variants.

| Key | Description |
|---|---|
| `SizeBuckets` | Allowed image sizes in pixels. A requested size is rounded up to the nearest bucket, sizes above the largest one use the largest. |
| `PreviewTargetCss` | Size of a preview thumbnail in CSS pixels; multiplied by the client device pixel ratio. |
| `DefaultScreenResolution` | Smaller side of the client screen in pixels, used for full screen images when the client does not send `X-Display`. |
| `DefaultAspectRatio` | Aspect ratio used when it is unknown. Default `4/3`. |

### MediaFormat

| Key | Description |
|---|---|
| `ImagePreset` | Name of the preset to use from `FfmpegPresets:Image`, which you define yourself. The name is also the target image format and file extension (e.g. `webp`). |
| `VideoPreset` | Name of the preset to use from `FfmpegPresets:Video` for streaming versions of videos. |
| `VideoThumbnailPreset` | Name of the preset to use from `FfmpegPresets:VideoThumbnailPrefix` - how a frame for the video preview is chosen. |
| `MiniaturePreset` | Name of the preset to use from `FfmpegPresets:Miniature`. |
| `MiniatureSize` | Size in pixels of the tiny placeholder image stored in the database and shown while media is loading. |
| `FavoritesReturnCount` | Number of random favorite media returned by `GET /v1/media/favorites`. |
| `VideoConversionBitrateThreshold` | Video bitrate in bits per second above which a streaming version is created. Default `26214400` (25 Mbit). |
| `TravelDateOffsets` | List of `{ From, To, Offset }`: UTC ranges (both ends optional and inclusive) and the trip time zone offset in hours for each. Converts the file modification time to the local trip time when the file name does not contain a date. At least one range is required. |

### FfmpegPresets

Named sets of ffmpeg arguments defined by you. Add as many presets as you like under each subsection; the ones actually used are selected by name in `MediaFormat`. The example config contains working presets for WebP and AVIF images and for H.264 video at 1440p, 1080p and 720p.

| Key | Description |
|---|---|
| `Image` | Image conversion and video preview arguments. Tokens: `{input}`, `{output}`, `{videoThumbnailPrefix}`. |
| `VideoThumbnailPrefix` | Arguments inserted into the image preset when a preview is made from a video (e.g. take the frame at 1 second). |
| `Video` | Video conversion as a list of ffmpeg runs, e.g. two passes. Tokens: `{input}`, `{output}`. |
| `Miniature` | Placeholder image arguments; the output is read from stdout. Tokens: `{input}`, `{miniatureSize}`. |

### TranslationProvider

AI and translation services used for auto-translation of days and media and for tag completion. `Providers` contains your own named entries (any number, any names); `Usage` selects an entry by name for each task.

`Usage` - which provider is used for what:

| Key | Task | Allowed provider types |
|---|---|---|
| `Usage:Days` | Translating day notes (`AutoTranslate` in `PUT /v1/days/{date}`). | `OpenAi`, `GeminiAi`, `Deepl` |
| `Usage:Media` | Translating media titles and descriptions (`AutoTranslate` in `PATCH /v1/media`). | `OpenAi`, `GeminiAi`, `Deepl` |
| `Usage:Tags` | Tag completion: translations and search aliases for a new tag (`POST /v1/tags/completion`). Needs structured JSON output from a chat model. | `OpenAi`, `GeminiAi` |

`Providers:<name>` - fields by `Type`:

| Type | Fields | Notes |
|---|---|---|
| `OpenAi` | `ApiKey`, `Model`, `Temperature`, `ReasoningLevel`, `ApiUrl` | Any OpenAI-compatible chat API, e.g. `https://api.deepseek.com`. `ReasoningLevel` is passed to the API as the reasoning effort. |
| `GeminiAi` | `ApiKey`, `Model`, `Temperature`, `ReasoningLevel` | Google Gemini. `ReasoningLevel`: `none`, `minimal`, `low`, `medium` or `high`. |
| `Deepl` | `ApiKey` | DeepL translation API. Cannot be used for tags. |

`Temperature` is a value from `0` to `1`. Several entries can use the same type, e.g. two Gemini models for different tasks.

### YouTube

| Key | Description |
|---|---|
| `Proxy:Enabled`, `Proxy:Address` | Proxy passed to yt-dlp, for servers where YouTube is blocked. |

### Logging

| Section | Description |
|---|---|
| `ConsoleLog`, `DebugLog` | Console and debugger output: `Enabled`, `MinimumLevel`. |
| `FileLog` | Daily log files in the `FileLogs` folder next to the application binaries: `Enabled`, `MinimumLevel`, `RetainedDaysCountLimit`. |
| `Seq` | [Seq](https://datalust.co/seq) server: `Enabled`, `ServerUrl`, `ApiKey`, `MinimumLevel`. |
| `Logging` | Standard ASP.NET Core log level overrides. |

### Kestrel

`Kestrel:Endpoints:Http:Url` sets the address the application listens on (`http://*:5101` in the example config). Kestrel endpoints from configuration take precedence over `ASPNETCORE_URLS`.

## Using the API

- Routes are versioned: `/v1/...`. Swagger lists all endpoints with request and response schemas.
- `Accept-Language` - response language: `en`, `ru` or `ja`. Browser-style values such as `ru-RU,ru;q=0.9` are supported. Optional, defaults to `en`.
- `X-Display` - client screen, used to choose image sizes: `dpr=1.25; min-side=864`. Optional.
- `Authorization: Basic ...` - admin requests. Without it, private media and media that are not approved yet are hidden.
- Errors are returned as RFC 9457 problem details: `400` - validation, `404` - not found, `409` - conflict (e.g. a sync is already running), `429` - too many failed logins, `502` - an external service failed (yt-dlp, preview download), `500` - other errors.

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

- [docs/documentation-notes.md](docs/documentation-notes.md) - how media sync, conversion, YouTube import, file removal and auto-translation work.
- [docs/conventions.md](docs/conventions.md) - code structure conventions.
- Swagger (`/swagger`) - API reference generated from the code.
- [HaruyasumiRyokouki.Frontend](https://github.com/meckbaig/HaruyasumiRyokouki.Frontend) - the client application.

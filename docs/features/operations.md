# Running the service

What matters when the application runs on a server: health, metrics, logs, and the state it keeps
in memory. Installation and configuration keys are in the [README](../../README.md).

## Files

| File | Role |
| --- | --- |
| `Program.cs` | Startup order, exit codes. |
| `Common/HealthChecks/` | `/health` checks and the overall status rule. |
| `Extensions/ApplicationBuilderExtensions.cs` | `/health` response, `/metrics`, Swagger, forwarded headers. |
| `Common/Options/Loggers/` | Serilog sinks built from `ConsoleLog`, `DebugLog`, `FileLog`, `Seq`. |
| `Services/TempStorageService.cs` | Temporary folders for ffmpeg. |

## Health

`GET /health` - checks and the status rule are listed in the README, section Health check. In short:
database, file storage and preview service are required (`Unhealthy`, HTTP 503); similarity search,
`ffmpeg`, `ffprobe` and `yt-dlp` are optional (at worst `Degraded`, HTTP 200). The response lists
every check with its description, e.g. the ffmpeg version or the number of loaded vectors.

The checks run in parallel on every call and start the tools as processes - do not poll `/health`
more often than every few seconds.

## Metrics and logs

- `/metrics` - Prometheus format: ASP.NET Core request metrics and .NET runtime metrics.
- Logs go to the sinks enabled in configuration: console, debugger, daily files in `FileLogs/` next to the binaries, Seq. Unhandled exceptions are logged with the method and path; aborted requests are logged at Information.
- Sync logs every imported, skipped and missing file - the log is the report of a sync.

## State in memory

| State | Lost on restart | Consequence |
| --- | --- | --- |
| Sync lock | - | Two instances could sync at the same time. |
| Failed login counters | Yes | Blocks are lifted. |
| File removal queue | Yes | Queued files stay in the storage as orphans. |
| Similarity index | Reloaded on startup | Startup reads all vectors. |

**Run a single instance.** Every row above assumes it.

## Startup and shutdown

- Exit code `1` - invalid configuration; the log names every invalid setting.
- Migrations are not applied on startup.
- The similarity index loads in the background; the API answers before it is ready, and a similarity request that comes first loads the index itself.
- Publish into an empty folder: Serilog loads sink assemblies it finds next to the executable, and a leftover assembly of a removed sink stops the startup.

## Invariants

- **`/health` is anonymous** and shows tool versions and component errors. The information is not secret, but restrict the path at the proxy if that matters.
- **Temporary media files** are written to `temp/` next to the application binaries during conversion and deleted afterwards (`TempStorageService`). Large videos need free space there, and the application needs write access to its own folder.

# Media sync and conversion

Media get into the database only through sync: the admin puts files into the file storage and
calls `PUT /v1/media/sync`. Sync imports new files, converts them, creates days and placeholder
images.

## Files

| File | Role |
| --- | --- |
| `Features/Media/SyncMediaCommand.cs` | `PUT /v1/media/sync` - thin handler. |
| `Services/MediaSyncService.cs` | The sync itself. |
| `Services/MediaProcessorService.cs` | Conversion of one image or video, previews, miniatures. |
| `Services/FfmpegService.cs` | Runs `ffmpeg` / `ffprobe` with the presets from `FfmpegPresets`. |
| `Services/MediaDateResolver.cs` | Creation time from the file name or the modification time. |
| `Common/Helpers/MediaFileNaming.cs` | Media type by extension, `_web` / `_preview` names. |
| `Common/Helpers/DayResolver.cs` | Finds or creates the day of a date in the loaded list. |
| `Services/LocalFileStorageService.cs`, `Services/WebDavFileStorageService.cs` | `IFileStorage` implementations. |
| `Services/BackgroundServices/FileRemovalBackgroundService.cs` | Background removal of files (`IRemovalQueueService`). |

## What sync does

1. Takes a process-wide lock. A second sync while one is running gets **409** at once; the frontend waits for the first request.
2. Loads all days and all media file names from the database.
3. **New files.** Lists the storage and takes files that are not in the database, skipping additional files of videos (`*_web.*`, `*_preview.<image preset>`). For each new file:
   1. Detects the type by extension (case-insensitive). Unknown types are skipped.
   2. Resolves the creation time (see below). A file outside every configured date range is skipped.
   3. Converts it (see below). A failed conversion skips the file.
   4. Finds or creates the day of the creation date and saves the media file, not approved.
4. Logs files that are in the database but missing from the storage, including additional files. Nothing is deleted.
5. **Incomplete files.** Creates the missing miniature of local media that have none (for example after a failed attempt).

A failure of one file is logged and the sync goes on with the next one.

## Conversion

| | Image | Video |
| --- | --- | --- |
| Main file | Converted to `MediaFormat:ImagePreset` (e.g. WebP) when the extension differs. The converted file **replaces the original** in the storage; the media gets the new name. | **Kept as is.** |
| Additional files | none | `<name>_preview.<image preset>` - a frame chosen by `MediaFormat:VideoThumbnailPreset`, always created. `<name>_web.<ext>` - a lower bitrate version from `MediaFormat:VideoPreset`, only when the bitrate (from `ffprobe`) is above `MediaFormat:VideoConversionBitrateThreshold`. |
| Miniature | A tiny image (`MediaFormat:MiniatureSize` px, `MiniaturePreset`) stored in the database as base64 and shown while the real image loads. | Same, made from the preview. |
| Aspect ratio | From the image. | From the preview. |

Files are copied to a temporary folder for ffmpeg (`TempStorageService`) and the results are
uploaded back to the storage. Additional file names are stored in `MediaFile.AdditionalFiles`; the
stream URL uses `_web` when it is there, see [media-urls.md](media-urls.md).

## Creation time

`MediaFile.Created` is the **local time of the trip**, not UTC:

1. If the file name contains a timestamp - `20260301_120000`, `IMG_20260301_120000`, `PXL_20260301_120000123`, `2026-03-01 12.00.00` and similar - it is taken as the local time. Phones name files by local time.
2. Otherwise the modification time of the file (UTC) is converted with `MediaFormat:TravelDateOffsets`: the first range `From <= time <= To` (both optional) gives the offset in hours. No matching range - the file is skipped with an error in the log.

The day of the media is the date of this local time.

## Storage providers

| Provider | Listing | Reading for ffmpeg |
| --- | --- | --- |
| `Local` | Files in `LocalStorageProvider:Path`, modification time in UTC. | Path on disk. |
| `WebDav` | `PROPFIND` of the folder; the library converts the server time to local, the service converts it back to UTC. | URL with the credentials in a header; the header value is masked in logs. |

## Invariants

- **One sync at a time per process.** The lock is a static `SemaphoreSlim`; it does not work across several instances.
- **Images are replaced, videos are kept.** After sync the storage contains `photo.webp` instead of `photo.jpg`. The original is deleted only after the converted file is uploaded.
- **A file name without a timestamp gets its date from the modification time.** Copying files in a way that resets the modification time (some upload tools do) moves them to the upload date or out of all ranges.
- **A day is created only after a successful conversion**, so a skipped file never leaves an empty day.
- **Missing files are only logged.** Sync never deletes database rows.
- **One failed database save can break the rest of the sync**: the failed entities stay in the context and the next `SaveChanges` fails on them again. Rare with the lock in place; see [issues.md](../issues.md).
- `ffmpeg` and `ffprobe` must be in `PATH` or next to the executable. `/health` shows whether they start.

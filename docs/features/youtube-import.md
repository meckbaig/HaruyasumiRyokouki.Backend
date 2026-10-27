# YouTube import

The admin can add a YouTube video to a day: `POST /v1/media/youtube` with `{ "url": "..." }`.
The video is played from YouTube; only a preview image is stored.

## Files

| File | Role |
| --- | --- |
| `Features/Media/ImportYoutubeMediaCommand.cs` | The endpoint and the whole import. |
| `Common/Helpers/YoutubeUrlParser.cs` | Video id from `watch?v=`, `youtu.be/`, `shorts/`, `embed/`, `v/` links or a bare 11-character id. |
| `Services/YoutubeMetadataService.cs` | Runs `yt-dlp --dump-single-json` for the publish date, aspect ratio and thumbnail URL. The title is not imported; the admin writes titles in the editor. |
| `Services/MediaProcessorService.cs` | `CreateYoutubePreviewAsync` - converts the thumbnail to the image preset and uploads it. |

## Flow

1. Extract the video id; an invalid link is 400.
2. A video already imported (same id) is 400.
3. Read metadata with yt-dlp (through `YouTube:Proxy` when enabled). Failure - 502.
4. Convert the publish time (UTC) to local trip time with `MediaFormat:TravelDateOffsets`. Outside all ranges - 400.
5. Download the thumbnail, convert it to `<id>_preview.<image preset>`, upload it to the storage. Failure - 502.
6. Create the miniature from the preview and save the media file (`Source = YouTube`, not approved) in the day of the publish date, creating the day if needed.

If anything fails after the preview was uploaded, the preview is queued for removal.

## The stored media

| Field | Value |
| --- | --- |
| `FileName` | The video id - there is no video file. |
| `Source` | `YouTube` |
| `ExternalUrl` | `https://www.youtube-nocookie.com/embed/<id>` - the stream URL returned to clients. |
| `AdditionalFiles` | `[<id>_preview.<image preset>]` - the only file in the storage. |
| `Created` | Publish time converted to local trip time. |

## Invariants

- **The video must be published within the trip dates.** The day comes from the publish time; a video published later than the last range of `TravelDateOffsets` cannot be imported until the ranges are extended.
- **YouTube media have no download URL** and sync ignores them: they are not checked for missing files and do not get miniatures re-created.
- **502 means an external tool failed, 400 means the request is wrong** - the frontend shows them differently.
- `yt-dlp` is needed only here. Without it the rest of the application works and `/health` is `Degraded`.

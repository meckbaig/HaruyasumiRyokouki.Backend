# Media import and conversion

Media get into the site in two ways: files from the file storage are imported by sync, and YouTube videos are imported one by one by a link. Either way a new media file is not approved and is visible only to the editor until it is reviewed.

## Sync with the file storage

The editor uploads photos and videos into the storage folder (a local folder or a WebDAV folder, e.g. in Nextcloud) and starts sync. For a whole trip both steps can be done by [HaruyasumiRyokouki.DataTool](https://github.com/meckbaig/HaruyasumiRyokouki.DataTool): it copies the files with their modification time, so their dates are determined the same way, and then starts sync. Sync looks for files that are not in the database yet and imports them one after another:

1. The type is detected by the extension. Files of other types are skipped.
2. The time the file was taken is determined (see [dates.md](dates.md)). A file whose time does not fall into the trip is skipped.
3. The file is converted (see below).
4. The day of that date is found, or created if it does not exist yet, and the media file is saved.

If one file fails, the error is written to the log and sync goes on with the next one. The log is the report of a sync: it lists imported, skipped and failed files.

After the import sync does two more things. It creates the tiny placeholder images (miniatures) that are missing, for example after an earlier failure. And it logs files that are in the database but no longer in the storage. It never deletes anything from the database.

Only one sync can run at a time. A second request gets 409 at once, while the first one goes on until all files are processed - which can take minutes for videos.

## Conversion

Images and videos are treated differently:

- **Images are replaced.** An image in another format than the configured one (e.g. JPEG when the preset is WebP) is converted, the converted file is uploaded to the storage, and only then the original is deleted. After sync the storage contains `photo.webp` instead of `photo.jpg`, and the media file has the new name.
- **Videos are kept.** The original stays as it is. A preview image `<name>_preview.<image format>` is always created from one frame. If the bitrate of the video is above the configured threshold, a lighter version for streaming `<name>_web.<ext>` is created too. Both are additional files of the media: the stream URL uses the `_web` version when it exists, and both are deleted together with the media.

For every media a miniature of a few pixels is stored right in the database. The site shows it blurred while the real image is loading.

ffmpeg works with local files, so every file is first copied into a temporary folder `temp/` next to the application and the results are uploaded back. For WebDAV the bitrate of a video is read by ffprobe directly from the server, with the credentials passed in a header (they are masked in the logs).

## YouTube videos

The editor can add a YouTube video by its link. Only metadata are imported: the video itself is played from YouTube.

1. yt-dlp reads the publish time, the aspect ratio and the thumbnail of the video. The title is not imported, the editor writes it.
2. The publish time is converted to the local time of the trip, and the video gets into the day it was published. A video published outside the trip cannot be imported.
3. The thumbnail is converted into `<video id>_preview.<image format>` - the only file of a YouTube media in the storage.

Errors of yt-dlp, the thumbnail download or ffmpeg return 502, a wrong link or an already imported video return 400, so the site can tell "the link is wrong" from "YouTube is not available".

## Deleting media

When a media file is deleted, its database row is removed at once, and its files - the main one and the additional ones - are put into a queue. A background service deletes them from the storage one by one, so the response does not wait for the storage. The queue is kept in memory: files that were queued but not deleted before a restart stay in the storage.

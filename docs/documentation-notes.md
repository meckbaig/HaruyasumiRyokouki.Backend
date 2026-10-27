# Documentation notes

Details about how the system works that do not belong in code comments (they are not needed to work with a specific class). Collected during refactoring for the README (stage 7); what does not fit into the README stays here.

## Media sync and conversion
- Sync (`PUT /v1/media/sync`) imports files from the file storage that are not in the database yet, creates missing miniatures and logs files that are in the database but missing from the storage. Only one sync can run at a time (a parallel request gets 409); the frontend waits for the request to finish.
- Images are replaced with an optimized version in the target format (`MediaFormat:ImagePreset`); the original file is deleted from the storage.
- Videos keep the original. A preview image `<name>_preview.<image preset>` is always created; a lower bitrate version for streaming `<name>_web.<ext>` is created only when the video bitrate is above `MediaFormat:VideoConversionBitrateThreshold`. Both are stored in `MediaFile.AdditionalFiles`; the stream URL points to `_web` when it exists.
- Creation time: taken from the file name if it contains a timestamp (`yyyyMMdd_HHmmss` and similar), otherwise from the file modification time (UTC) converted to the local trip time by `MediaFormat:TravelDateOffsets`. Files outside every configured range are skipped.
- Files of unknown type are skipped.

## YouTube import
- `FileName` of a YouTube media is the video id; the preview `<id>_preview.<image preset>` is the only file in the storage.
- Errors of yt-dlp, preview download or ffmpeg return 502; an invalid link or an already imported video returns 400.

## File removal
- Deleted media files are removed from the storage in the background (`IRemovalQueueService`). The queue is kept in memory: files queued but not yet deleted are lost on restart.

## Auto-translation
- `AutoTranslate` in day and media editing returns translations as a preview: nothing is saved until the admin reviews and saves the result.

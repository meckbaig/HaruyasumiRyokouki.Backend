# Media URLs and image sizes

The backend never serves media bytes. Every media DTO carries URLs: originals point to the file
storage (or a CDN in front of it), resized images point to a preview service - Nextcloud previews
or imgproxy. Sizes are chosen per request from the client screen.

## Files

| File | Role |
| --- | --- |
| `Extensions/MappingExtensions.cs` | `AddUrls` - fills `imageUrls` or `videoUrls` of a DTO. |
| `Services/MediaUrlsProvider.cs` | Chooses image or video URLs, local or YouTube. |
| `Services/NextcloudMediaPreviewService.cs`, `Services/ImgproxyMediaPreviewService.cs` | `IMediaPreviewService` - one is registered by `MediaPreview:Provider`. |
| `Services/Builders/` | `OriginStorageUrlBuilder` and `NextcloudPreviewUrlBuilder` - fill `{fileName}`, `{xAxis}`, `{yAxis}` in `PayloadStringBase`. |
| `Services/MediaResolutionCalculationService.cs` | Requested image size in pixels. |
| `Common/Behaviors/ClientDisplayBehavior.cs` | Parses the `X-Display` header. |

## URLs in a DTO

Images - `imageUrls`:

| Field | Nextcloud | imgproxy |
| --- | --- | --- |
| `preview` | Nextcloud preview of the preview size | `rs:fit` of the preview size |
| `fullScreen` | Nextcloud preview of the screen size | `rs:fit` of the screen size |
| `download` | Original file (`OriginStorage`) | Original through imgproxy as an attachment |

Videos - `videoUrls`:

| Field | Local video | YouTube |
| --- | --- | --- |
| `preview` | Resized `<name>_preview.<image preset>` | Resized `<id>_preview.<image preset>` |
| `stream` | `<name>_web.<ext>` if it exists, otherwise the original (`OriginStorage`) | `ExternalUrl` (YouTube embed) |
| `download` | Original (`OriginStorage`) | `null` |

`CdnEndpoint` replaces `Endpoint` for media URLs when set; for downloads only with
`UseCdnForDownloads`. File names are escaped (`Uri.EscapeDataString`).

## Image size

1. `X-Display: dpr=1.25; min-side=864` gives the device pixel ratio and the smaller side of the screen in CSS pixels. Both are optional; without the header `dpr = 1` and the screen is `MediaSizes:DefaultScreenResolution`.
2. Target size: full screen = `dpr * min-side`, preview = `dpr * MediaSizes:PreviewTargetCss`.
3. Rounded **up** to the nearest value of `MediaSizes:SizeBuckets`; above the largest bucket - the largest. Buckets keep the number of variants the preview service has to cache small.
4. Multiplied by the aspect ratio of the media (`max(r, 1/r)`, `DefaultAspectRatio` when unknown) and used for **both** width and height of a "fit" box. The box is square, so the smaller side of the image gets the bucket size whatever its orientation.

`X-Display` parsing: keys are case-insensitive, unknown keys and malformed parts are ignored, a
value that does not parse is skipped, `dpr` uses a dot as the decimal separator. Zero or negative
values are a 400.

## Invariants

- **Responses depend on `X-Display`.** A cache in front of the API must vary on it (and on `Accept-Language`).
- **Only file names are stored.** Changing `MediaPreview` changes every URL at once, no migration needed; moving files to another storage needs only `OriginStorage` / `FilePath` changes.
- **The stream URL depends on `AdditionalFiles`.** A `_web` file that exists in the storage but is not listed in the database is not used.
- **Nextcloud has no "original through the preview service"**: `ImageUrlType.Original` is supported only by imgproxy and is not used in DTOs.
- Bucket sizes are sorted when the options are read, so their order in the config does not matter.

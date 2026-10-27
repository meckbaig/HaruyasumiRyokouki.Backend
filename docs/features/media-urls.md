# Media URLs and image sizes

The backend never sends media files itself. Every media in a response carries URLs, and the client loads the files directly: originals and videos from the file storage, resized images from a preview service - Nextcloud previews or imgproxy. The database stores only file names, so moving the files or switching the preview service changes only the configuration.

## Which URLs a media file gets

An image has three URLs: a thumbnail for grids, a version for the full screen and the original for downloading. A video has a preview image, a URL to play it and the original for downloading. The play URL points to the lighter `_web` version when sync created one, otherwise to the original. A YouTube video is played by its YouTube embed URL and has no download URL.

When a CDN is configured in front of the storage or the preview service, media URLs use it. Download URLs use it only when that is enabled separately. The URLs are built with the CDN without any fallback, so a CDN that does not respond breaks the site, which is why `/health` checks every configured CDN.

## Choosing the image size

The client tells the server about its screen in the `X-Display` header, e.g. `dpr=1.25; min-side=864`: the device pixel ratio and the smaller side of the screen in CSS pixels. Without the header the server assumes a pixel ratio of 1 and a default screen size from the configuration.

The size is calculated in three steps:

1. The needed size: the screen side times the pixel ratio for a full-screen image, a fixed thumbnail size times the pixel ratio for a thumbnail.
2. Rounded **up** to the nearest of a few allowed sizes (`MediaSizes:SizeBuckets`). Thanks to that, the preview service makes and caches only a handful of variants of every image instead of one per device. A size above the largest allowed one gets the largest.
3. Multiplied by the aspect ratio of the image (always the long side to the short one). The result is used as both the width and the height of a square box the image has to fit in. In a square box the long side of the image reaches the box size, and the short side gets exactly the size from step 2 - whether the photo is landscape or portrait.

Because the URLs depend on `X-Display` and texts depend on `Accept-Language`, a cache in front of the API must take both headers into account.

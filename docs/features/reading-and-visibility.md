# Reading days and media, visibility

Public endpoints return the trip by days. What an anonymous visitor sees is limited by two flags
on each media file; the admin sees everything.

## Files

| File | Role |
| --- | --- |
| `Features/Days/GetDaysQuery.cs` | `GET /v1/days` - all days with media counts. |
| `Features/Days/GetDayQuery.cs` | `GET /v1/days/{date}` - one day with notes and media. |
| `Features/Media/GetMediaQuery.cs` | `GET /v1/media?from=&to=` - media with coordinates in a date range (the map). |
| `Features/Media/GetFavoriteMediaQuery.cs` | `GET /v1/media/favorites` - random favorite media for the front page. |
| `Features/Admin/GetPendingQuery.cs` | `GET /v1/admin/pending` - days not ready and media not approved. |
| `Features/Days/GetEditDayQuery.cs`, `Features/Media/GetEditMediaQuery.cs` | Admin variants with all translations, for the editors. |
| `Extensions/MappingExtensions.cs` | `ToDto(admin)`, `ToEditDto`, `AddUrls`. |

## Visibility

| Flag | Meaning |
| --- | --- |
| `MediaFile.IsApproved` | Reviewed by the admin. New media from sync are not approved. |
| `MediaFile.Private` | Never shown publicly, even when approved. |
| `Day.IsReady` | The day notes are finished. Returned to everyone; the frontend decides what to do with unfinished days. |

Anonymous requests see media with `IsApproved && !Private`. The rule is repeated in each
handler as `request.IsAuthenticated || (m.IsApproved && !m.Private)`. For the admin, public DTOs
also carry `isApproved`, `private` and `favorite`; for others these fields are `null`.

| Endpoint | Anonymous | Admin |
| --- | --- | --- |
| `GET /v1/days` | All days, `mediaCount` counts visible media only. | All days, all media counted. |
| `GET /v1/days/{date}` | Day notes and visible media ordered by time. 404 if the day does not exist. | All media. |
| `GET /v1/media?from=&to=` | Visible media **with coordinates**, ordered by time. `to` is inclusive. | All media with coordinates. |
| `GET /v1/media/favorites` | `MediaFormat:FavoritesReturnCount` random favorites that are not private. | Random favorites including private. |
| `GET /v1/search` | Visible media only. See [tags-and-search.md](tags-and-search.md). | All media. |
| `GET /v1/tags/suggestion` | Tags ranked by the number of visible media. | Ranked by all media. |

## Localized DTOs

Public DTOs are flattened to one language: the handler loads only the translations in
`AcceptLanguage` (`IncludeFiltered(..., LocalizedMedia())` and similar from `LinqExtensions`),
and the mapping takes the first one. Edit DTOs (`ToEditDto`) contain all languages and are used by
the admin editors.

## Invariants

- **There is no fallback language.** A media file without a translation in the requested language has `title`, `description` and `languageCode` set to `null`; it is not replaced by English.
- **`GET /v1/media/favorites` does not check `IsApproved`** on purpose: a favorite is shown to visitors unless it is private.
- **Media without coordinates are not on the map.** `GET /v1/media` exists for the map and filters them out.
- **Media of a day are ordered by `Created`** - the local trip time, see [media-sync.md](media-sync.md).
- **Image and video URLs depend on `X-Display`** and are built per request; the database stores only file names. See [media-urls.md](media-urls.md).

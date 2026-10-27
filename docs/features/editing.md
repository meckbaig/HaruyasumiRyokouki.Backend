# Editing media and days

The admin edits media in batches with `PATCH /v1/media` and day notes with `PUT /v1/days/{date}`.
Both can ask the server to translate the texts, which is returned as a preview and not saved.

## Files

| File | Role |
| --- | --- |
| `Features/Media/EditMediaCommand.cs` | `PATCH /v1/media` - bulk edit of media. |
| `Models/Dtos/Media/EditMediaChanges.cs` | The changes, every field `Optional<T>`. |
| `Common/OptionalType/` | `Optional<T>`, its JSON converter and model binder. |
| `Features/Days/EditDayCommand.cs` | `PUT /v1/days/{date}` - day notes and the "ready" flag. |
| `Features/Media/DeleteMediaCommand.cs` | `DELETE /v1/media/{mediaId}`. |
| `Features/Tags/AddTagToMediaCommand.cs` | `POST /v1/tags/{id}/media` - add one tag to many media. |
| `Common/Helpers/TranslationPlanner.cs` | Which language is the source and which are translated. |

## `PATCH /v1/media`

```json
{ "ids": [1, 2, 3], "changes": { "isApproved": true, "latitude": null }, "autoTranslate": false }
```

The same changes are applied to every media in `ids`; any missing id gives 404 and nothing is
changed. Every field of `changes` is an `Optional<T>`:

| JSON | `HasValue` | Effect |
| --- | --- | --- |
| field absent | `false` | Not changed. |
| `"latitude": null` | `true`, value `null` | Set to null (for coordinates: removes the media from the map). |
| `"latitude": 35.0` | `true` | Set. |

Per field:

| Field | Effect |
| --- | --- |
| `latitude`, `longitude` | Set; validated to ±90 / ±180. |
| `isApproved`, `private`, `favorite` | Set. |
| `tagIds` | **Replaces** the whole tag set of every selected media. `null` is rejected; `[]` removes all tags. |
| `translations` | **Upserts** by language: the sent languages are replaced, the others stay. At most one item per language, languages `en`, `ru`, `ja`. |

The response contains the edited media as edit DTOs (all languages, tags in the request language).

## `PUT /v1/days/{date}`

The day is found by the date in the route. `isReady` and the whole list of translations are
replaced; a language missing from the list is deleted. The date inside the body is ignored - a day
cannot be moved to another date.

## Auto-translation

With `autoTranslate: true` the handler first saves the request as usual, then translates the
texts and returns the translations **without saving them**. The admin reviews them in the editor
and saves with a second request. Providers are configured in `TranslationProvider`; see
[localization-and-translation.md](localization-and-translation.md).

What is translated:

- Source language: the first of English, Russian, Japanese that has a non-blank text. Blank or whitespace texts count as missing.
- Targets: every language without a text. Existing texts are never overwritten.
- Media: titles and descriptions are planned separately, so a title can be translated from Russian and a description from English.
- Media are translated from the `translations` **sent in the same request**. Without `translations` in the request there is nothing to translate.
- With several media selected, one translation is made and applied to all of them.

## Deleting media

`DELETE /v1/media/{mediaId}` deletes the database row and queues the file and its additional files
(`_web`, `_preview`) for removal from the storage. The removal runs in the background
(`FileRemovalBackgroundService`), so the response does not wait for the storage.

## Adding a tag to many media

`POST /v1/tags/{id}/media` with `{ "mediaFileIds": [...] }` adds one tag to the media and leaves
their other tags alone - unlike `tagIds` in the PATCH, which replaces the set. It returns the
number of media that did not have the tag yet. Used by the similarity screens of the frontend.

## Invariants

- **Absent and `null` are different in PATCH.** Removing `Optional<T>` or deserializing the changes into plain nullable fields would make every PATCH clear the fields it does not mention. `OptionalJsonTests` guards this.
- **Translations of media are upserted, translations of days are replaced.** Sending one language in a PATCH keeps the others; sending one language in a PUT deletes the others.
- **The auto-translation preview is not saved** - by design, a person checks machine translation before it is published.
- **Removal queue is in memory.** Files queued and not yet deleted are lost on restart and stay in the storage; sync then logs nothing about them because they are not in the database.
- `DELETE` and `PUT /v1/media/sync` return an empty `200`, not `204` - the frontend expects that.

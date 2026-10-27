# Tags and search

Media are tagged with multilingual tags. Each tag has one translation per language and any number
of search aliases. Search finds days and media by text or by tag.

## Files

| File | Role |
| --- | --- |
| `Features/Tags/GetTagsQuery.cs` | `GET /v1/tags` (admin) - all tags with all texts, most used first. |
| `Features/Tags/AddTagCommand.cs`, `Features/Tags/EditTagCommand.cs` | `POST /v1/tags`, `PATCH /v1/tags/{id}`. |
| `Models/Dtos/Tags/CreateTagDto.cs`, `EditTagDto.cs`, `TagTranslationDto.cs` | Tag contracts and their validators. |
| `Features/Tags/GetTagSuggestionQuery.cs` | `GET /v1/tags/suggestion?text=` - public autocomplete. |
| `Features/Tags/GetTagCompletionCommand.cs` | `POST /v1/tags/completion` (admin) - AI drafts a new tag. |
| `Features/Search/SearchQuery.cs` | `GET /v1/search`. |
| `Common/Helpers/LikePattern.cs` | Escaping of user input for `ILIKE`. |

## Tag model

| Part | Storage | Rules |
| --- | --- | --- |
| `slug` | `tags.slug` | Not empty, unique (checked by the validators, 400 on a duplicate). Used in links: `GET /v1/search?tag=<slug>`. |
| `translations` | `tag_translations`, `is_primary = true` | At most one per language (unique partial index). Shown to users in their language. |
| `aliases` | `tag_translations`, `is_primary = false` | Any number, any supported language. Only used for search and suggestions. |

All texts of one tag - translations and aliases, in all languages - must be different
(unique index `(tag_id, text)`). The validators check it before saving; `PATCH` checks the texts
after the edit, taking the lists that are not edited from the database. An alias equal to a
translation of the same tag is rejected regardless of case.

`PATCH /v1/tags/{id}` uses `Optional<T>` like the media PATCH: `translations` replaces all primary
texts, `aliases` replaces all aliases, an absent field is not changed.

Media get tags through `tagIds` of `PATCH /v1/media` (replaces the set) or
`POST /v1/tags/{id}/media` (adds one tag), see [editing.md](editing.md).

## Suggestions and completion

`GET /v1/tags/suggestion?text=&take=8` returns tags having **any** text (translation or alias,
any language) that contains `text`, case-insensitive, most used first, shown with the primary
translation in the request language. For anonymous visitors usage counts only visible media.

`POST /v1/tags/completion` with `{ "tag": "..." }` is the admin's "new tag" helper. In parallel it:

- asks the AI (`TranslationProvider:Usage:Tags`) for the three translations and up to five genuine aliases per language - the prompt forbids widening or narrowing the meaning;
- finds up to 8 existing tags containing the text, so the admin can reuse one instead of creating a duplicate.

The draft slug is the English translation with spaces replaced by `_`. Nothing is saved; the admin
edits the draft and creates the tag with `POST /v1/tags`. Missing lists in the AI answer are treated
as empty, duplicate languages and aliases equal to a translation are dropped.

## Search

`GET /v1/search` takes `text`, `tag` (slug) or `tagId`; at least one is required.

| Mode | When | Finds |
| --- | --- | --- |
| Text | `text` is not blank (wins over tags) | Days whose note in any language contains the text; media whose title or description in any language contains it; media with a tag having a text (translation or alias, any language) **equal** to it. |
| Tag | no text | Media with the tag: by `tag` slug (exact, case-insensitive) when given, otherwise by `tagId`. |

The result is a list of days, **newest first**, each with only the matching media, ordered by time,
in the request language. A day found by its note is returned with the media that match too, which
may be none. Visibility rules apply to media.

User input is matched literally: `%`, `_` and `\` are escaped by `LikePattern`, and the queries pass
the escape character to `ILIKE`.

## Invariants

- **`ILIKE` must get `LikePattern.EscapeCharacter`.** Npgsql generates `ESCAPE ''` otherwise, which disables escaping, and `%`/`_` in the input become wildcards again. A unit test cannot see this - check the SQL with `ToQueryString()`.
- **Text search matches tags exactly but notes and titles partially.** Searching "cat" finds a note mentioning "category" but not the tag "cats".
- **`tag` beats `tagId`** when both are sent.
- **Suggestions are public**, completion is admin-only (it costs AI calls).
- Trigram GIN index on `tag_translations.text` (`pg_trgm`) serves the `ILIKE '%...%'` lookups.

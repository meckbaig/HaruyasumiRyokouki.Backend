# Languages and machine translation

All texts exist in three languages: English (`en`), Russian (`ru`) and Japanese (`ja`). Public
endpoints return one language chosen by `Accept-Language`; the admin edits all three and can ask
an AI or DeepL to fill the missing ones.

## Files

| File | Role |
| --- | --- |
| `Models/Db/Enums/LanguageCode.cs` | Supported codes and `IsSupported`. |
| `Common/Helpers/AcceptLanguageParser.cs` | Header → language code. |
| `Common/Behaviors/LocalizationBehavior.cs` | Sets `ILocalizableRequest.AcceptLanguage`. |
| `Extensions/LinqExtensions.cs` | `LocalizedDays`, `LocalizedMedia`, `LocalizedTags` - filters for `IncludeFiltered`. |
| `Common/Helpers/TranslationPlanner.cs` | Source and target languages of a translation. |
| `Services/Translation/` | `AiTranslationService`, `DeeplTranslationService` and the factory that builds them from `TranslationProvider`. |
| `Services/OpenAiChatService.cs`, `Services/GeminiAiChatService.cs` | `IAiChatService` for OpenAI-compatible APIs and Gemini. |
| `Common/Options/Parsers/TranslationOptionsParser.cs` | Reads the user-defined `TranslationProvider:Providers` entries. |

## Choosing the response language

`AcceptLanguageParser.Resolve`:

- Parses the header as a list with quality values: `ru-RU,ru;q=0.9,en;q=0.8`.
- Drops entries with `q=0`, orders by quality (stable, so equal qualities keep the header order).
- Reduces regional variants to the language (`ja-JP` → `ja`), ignores case.
- Takes the first supported language; nothing supported, empty or malformed header → `en`.

## Translation providers

`TranslationProvider:Providers` holds named entries of type `OpenAi`, `GeminiAi` or `Deepl`;
`TranslationProvider:Usage` picks one entry for days, media and tags (README, section
TranslationProvider). Tag completion needs a chat model with JSON output, so it cannot use DeepL.

Planning (`TranslationPlanner.Plan`), shared by days and media:

| Step | Rule |
| --- | --- |
| Present texts | Non-blank values only. |
| Source | First present of `en`, `ru`, `ja`. None - nothing is translated. |
| Targets | Every language without a present text, in the same order. |

Each target is a separate call to the provider, run in parallel. How the results are used is in
[editing.md](editing.md#auto-translation).

## Invariants

- **Supported languages are fixed in code.** Validators reject other codes (`LanguageCode.IsSupported`); adding a language means changing `LanguageCode`, the planner priorities and the tag completion prompt.
- **No fallback language** in public DTOs - see [reading-and-visibility.md](reading-and-visibility.md).
- **The source language order `en`, `ru`, `ja` is fixed** in `TranslationPlanner`; an existing text in a less preferred language is never used as a source while a more preferred one exists.
- **Machine translation is never saved directly** - it is returned for review.

# Languages and machine translation

All texts of the site - day notes, titles and descriptions of media, tag names - exist in three languages: English, Russian and Japanese. The editor writes a text in one of them and can ask the server to translate it into the others.

## Choosing the response language

The response language comes from the `Accept-Language` header, the way browsers send it: `ru-RU,ru;q=0.9,en;q=0.8`. The server orders the languages by their weight, reduces regional variants to the language (`ja-JP` becomes `ja`) and takes the first supported one. When there is none, or the header is missing or malformed, the response is in English.

## Auto-translation

When the editor saves a day or media with auto-translation turned on, the server saves what was sent and then translates the missing texts. **The translations are returned, not saved.** The editor reads them, corrects them if needed and saves them with a second request. Machine translation never reaches the site without a human look.

Which text is translated from where:

- The source is the first language that has a text, in the order English, Russian, Japanese. A text of spaces only counts as missing.
- The targets are all languages without a text. Existing texts are never overwritten.
- For media, titles and descriptions are planned separately: a title can be translated from Russian while the description is translated from English.
- Media are translated from the texts sent in the same request. With several media edited at once, one translation is made and applied to all of them.

## Providers

The configuration lists named providers: any OpenAI-compatible chat API, Google Gemini or DeepL. Separate settings say which provider translates day notes, which translates media and which drafts new tags. Different tasks can use different models. Tag drafts need a chat model that returns JSON, so DeepL cannot be used for them.

# Tags and search

Tags describe what is on a photo or video: a temple, cherry blossoms, a train. Visitors use them to find media by subject, the editor uses them to organize the archive.

## What a tag consists of

A tag has a unique key (slug) in English, e.g. `cherry_blossom`, used in links to the tag search. Besides it, a tag has two kinds of texts:

- **Display names** - at most one per language. The site shows the name in the language of the visitor.
- **Search aliases** - any number, in any language: synonyms, other spellings, transliterations (`sakura` for cherry blossoms). They are never shown, they only let the tag be found by other words.

All texts of one tag must be different, regardless of the language: the same word cannot be both a display name and an alias, or a name in two languages.

## Creating tags with AI

Making three translations and a set of aliases by hand for every tag is tedious, so the editor types a word in any language and asks for a draft. In parallel the server:

- asks the AI for the display names in all three languages and genuine aliases - the prompt forbids words that widen or narrow the meaning (no "Japanese temple" for "temple");
- finds existing tags that contain the word, so the editor can reuse one instead of creating a duplicate.

Nothing is saved. The editor corrects the draft and creates the tag.

There are two ways to tag media. Editing media sets the complete list of their tags, so tags that are not in the list are removed. Adding a tag to a selection of media only adds it and keeps the other tags - this is what the similarity screens use (see [similarity-search.md](similarity-search.md)).

## Search

Search finds days and media either by text or by a tag. When a text is given, the tag is ignored.

**By text**, the server looks in all languages at once:

- day notes, media titles and descriptions that **contain** the text;
- tags with a display name or alias **equal** to the text.

Notes and titles match partially and tags match exactly on purpose: a search for "cat" should find a note that mentions a cat, but not every media tagged "category".

**By tag**, media with the tag are found by its slug or its id.

The result is a list of days, newest first, each with only the matching media in the order they were taken. A day found by its note is returned even when none of its media match. The visibility rules of [visibility.md](visibility.md) apply.

What the user types is matched literally. Characters that are wildcards in SQL `LIKE` (`%`, `_`) are escaped, so a search for `100%` finds "100%" and not everything that starts with "100". Npgsql disables escaping unless the escape character is passed explicitly, so every `ILIKE` in the queries passes it.

## Tag suggestions

While a visitor types in the search box, the site suggests tags that contain the typed text in any of their texts, most used first. For visitors a tag is ranked by the number of media they can see.

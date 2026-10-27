# Visibility and the admin

The site has one user with special rights: the author, who is also the editor and the admin. Visitors see the trip without logging in. The editor sees everything and can edit it.

## Who sees what

Each media file has two flags that decide whether visitors see it:

- **Approved** - the editor has reviewed the media: its title, description, tags and location are final. Media just imported from the storage or YouTube are not approved, so a fresh upload never appears on the site by accident.
- **Private** - interesting media that are not for the public. They stay hidden from visitors even after approval and are visible only to the admin.

A visitor sees media that are approved and not private. Everything public follows this rule: the list of days counts only such media, a day page shows only them, as do the map, search and tag suggestions (tags are ranked by the number of media a visitor can see).

Favorites on the home page are the one exception on purpose: a favorite is shown to visitors unless it is private, whether it is approved or not.

Days have their own flag, **ready**: the day note and all media of the day have been reviewed. It is returned to everyone, and the site decides how to show unfinished days.

For the admin, public responses also contain the approved, private and favorite flags of every media. For visitors these fields are omitted.

## Languages in responses

Public responses are in one language, chosen by the `Accept-Language` header (see [translations.md](translations.md)). There is no fallback: if a media file has no title in the requested language, the title is empty rather than replaced by another language. The editor works with separate "edit" responses that contain the texts in all languages.

## How the admin is recognized

There are no sessions or tokens. Every request of the admin carries HTTP Basic credentials, and they are checked against the configured login and password on every request. Endpoints for editing require them and answer 401 without them. Public endpoints work without them, but show more when valid credentials come along.

Swagger has its own login and password, separate from the admin ones, so that the API description can be opened without giving away the admin password.

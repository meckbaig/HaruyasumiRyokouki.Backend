# Code conventions

## Project structure

- `Features/` - CQRS request handlers, one folder per feature. A request, its validator and its handler live in one file.
- `Services/` - DI-only. Anything placed here participates in dependency injection and is registered in `Extensions/ServiceCollectionExtensions.cs` or `Program.cs`. Domain-specific service groups get their own subfolder (e.g. `Services/BackgroundServices/`, `Services/Translation/`).
- `Extensions/` - extension methods only (`this`-parameter). `Extensions/TypeExtensions/` holds type-specific extensions.
- `Common/Helpers/` - stateless static helper classes that are neither DI services nor extension methods (pure functions with no dependencies and no mutable state).
- `Common/` - cross-cutting infrastructure (options, behaviors, filters, exceptions, result types, etc.).
- New settings go into an existing options section when they belong to it (e.g. switches of application features go to `Application`), not into a new section.

## Validation

- Standard FluentValidation rules (`NotEmpty`, `GreaterThan`, ...) keep their default messages. `WithMessage` is used only for custom rules (`Must`, `Custom`).

## XML documentation

What to write:

- Every public type and member has a description. Internal code is documented where its purpose is not obvious from the name.
- The description explains what the member means in the domain, not its name in other words. `DayDto.Note` is "the author's diary note for the day", not "the note".
- Only what is needed to use the code. How a part of the system works as a whole goes to [docs/](.), not to class remarks.
- `<summary>` is short. Details, special cases and reasons go to `<remarks>`.
- Details that only matter for a contract are documented where the contract is special, e.g. a method returning EF entities with specific navigations loaded.

How to write it:

- C# entities are referenced with `<see>`: `<see langword="null"/>`, `<see langword="true"/>`, `<see cref="MediaFile.IsApproved"/>` for types, members and parameters (`<paramref name="..."/>`).
- `<c>` is used only for what `<see>` cannot reference: configuration keys (`<c>MediaFormat:ImagePreset</c>`), routes (`<c>GET /v1/media</c>`), headers, literal values (`<c>en</c>`), file names, SQL.
- A semicolon is used only in the form `<see langword="true"/> when ...; otherwise <see langword="false"/>`. Elsewhere a sentence ends with a period, or the details move to `<remarks>`.
- Roles: "editor" is the person who reviews and edits content, "admin" is used only for authentication and the admin account. On this site it is the same single user, the author of the trip.

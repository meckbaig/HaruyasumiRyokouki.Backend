# Code conventions

- `Features/` - CQRS request handlers, one folder per feature. A request, its validator and its handler live in one file.
- `Services/` - DI-only. Anything placed here participates in dependency injection and is registered in `Extensions/ServiceCollectionExtensions.cs` or `Program.cs`. Domain-specific service groups get their own subfolder (e.g. `Services/BackgroundServices/`, `Services/Translation/`).
- `Extensions/` - extension methods only (`this`-parameter). `Extensions/TypeExtensions/` holds type-specific extensions.
- `Common/Helpers/` - stateless static helper classes that are neither DI services nor extension methods (pure functions with no dependencies and no mutable state).
- `Common/` - cross-cutting infrastructure (options, behaviors, filters, exceptions, result types, etc.).
- XML documentation describes what is needed to use the code. How the system works as a whole is described in the README and in [documentation-notes.md](documentation-notes.md).

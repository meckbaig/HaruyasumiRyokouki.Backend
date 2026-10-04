# HaruyasumiRyokouki.Backend

Basically my spring vacation travelogue from Japan

## Project structure conventions

- `Services/` - DI-only. Anything placed here participates in dependency injection and is registered in `Extensions/ServiceCollectionExtensions.cs`. Domain-specific service groups get their own subfolder (e.g. `Services/Builders/`, `Services/BackgroundServices/`, `Services/Translation/`).
- `Extensions/` - extension methods only (`this`-parameter). `Extensions/TypeExtensions/` holds type-specific extensions.
- `Common/Helpers/` - stateless static helper classes that are neither DI services nor extension methods (pure functions with no dependencies and no mutable state).
- `Common/` - cross-cutting infrastructure (options, behaviours, filters, result types, etc.).
- `Features/` - CQRS request handlers, one folder per feature.

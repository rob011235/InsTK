# Agent Notes

## Entity Framework migrations

- Do not edit existing migration files directly.
- Treat generated migration files as append-only history once created.
- If a schema change needs correction, create a new migration instead of modifying an older one.
- If migration drift is suspected, inspect the current model, migration history, and database state before applying fixes.
- Prefer normal EF workflows:
  `dotnet ef migrations add ...`
  `dotnet ef database update`

## Why

Editing old migration files can put the code model, migration history, and actual database schema out of sync. Even in local-only development, that creates misleading "up to date" states and makes debugging schema issues harder.

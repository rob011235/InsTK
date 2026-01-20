## Purpose
This file gives concise, repo-specific guidance for AI coding agents working on InsTK so they can be immediately productive.

## Quick summary / Big picture
- **Monorepo with 3 main apps**: `InsTK.Server` (ASP.NET Core + Identity, hosts APIs and Razor components), `InsTK.Client` (Blazor WebAssembly UI), and `InsTK.MauiClient` (MAUI front-end). Shared types and interfaces live in `InsTK.Shared`.
- The Server is the authoritative backend (EF Core + SQL Server). Client code uses `HttpClient` with DI to call server APIs.

## Where to look (examples)
- Server startup & DB migration: [InsTK.Server/Program.cs](../InsTK.Server/Program.cs)
- Server API controllers: [InsTK.Server/Controllers/CoursesController.cs](../InsTK.Server/Controllers/CoursesController.cs) and [InsTK.Server/Controllers/ObjectivesController.cs](../InsTK.Server/Controllers/ObjectivesController.cs)
- EF DbContext and user type: [InsTK.Server/Data/ApplicationDbContext.cs](../InsTK.Server/Data/ApplicationDbContext.cs), [InsTK.Server/Data/ApplicationUser.cs](../InsTK.Server/Data/ApplicationUser.cs)
- Blazor WebAssembly client startup: [InsTK.Client/Program.cs](../InsTK.Client/Program.cs)
- Shared interfaces & models: [InsTK.Shared](../InsTK.Shared)
- CI/Deploy workflow (production build + FTP to Plesk): [deploy workflow](workflows/deploy-plesk.yml)

## Architecture & integration notes (actionable)
- The Server both hosts server-side Razor components and exposes Web API endpoints. It registers Blazor rendering modes via `MapRazorComponents` and also calls `app.MapControllers()` for `api/*` routes.
- Data flow: `InsTK.Client` calls REST endpoints implemented by `InsTK.Server` (controllers use interfaces from `InsTK.Shared.Interfaces`). Example: `ICoursesDataService` is implemented server-side and has a client-side counterpart `CoursesClientDataService`.
- Authentication: Identity is configured in `InsTK.Server/Program.cs`. Role creation and an admin seed run on app startup using appsettings keys `Admin:Email` and `Admin:Password`.
- Database: EF Core migrations are applied automatically on startup (`RunMigrations` in `Program.cs`). The app expects a SQL Server connection string at `ConnectionStrings:DefaultConnection`.

## Developer workflows & commands
- Build entire solution: `dotnet build InsTK.sln`
- Run server locally (serves UI + APIs): `dotnet run --project InsTK.Server` (ensure `DefaultConnection` set in appsettings.Development.json)
- Run tests: `dotnet test InsTK.Test`
- Production build (what CI does): `dotnet publish InsTK.Server/InsTK.Server.csproj --configuration Release --runtime win-x64 --self-contained true` (see workflow at [deploy workflow](workflows/deploy-plesk.yml)).
- MAUI app: open `InsTK.MauiClient` in Visual Studio with MAUI workloads; CI does not build MAUI by default.

## Project-specific conventions
- DI pattern: services are registered in the project Program.cs files. Data services are registered as `AddTransient<ICoursesDataService, CoursesDataService>()` (server) and `CoursesClientDataService` (client).
- API routing: controllers use `[Route("api/[controller]")]` and map to `api/<ControllerName>`; mutations usually require `[Authorize(Roles = "Admin")]`.
- Shared code: domain models and interfaces live in `InsTK.Shared` and are the canonical contract between client and server.
- Config-driven seeds: admin user/email/password come from configuration keys under `Admin`—CI injects them into `appsettings.Production.json`.

## Pitfalls & what an AI agent should NOT assume
- Do not assume the server is stateless: identity, EF migrations, and role/user seeds run at startup. Always preserve startup steps when refactoring.
- Do not change connection/secret keys inline — they are provided via configuration and CI secrets (see deploy workflow usage of `PROD_SQL_CONNECTION` and admin secrets).

## Typical tasks and where to implement them
- Add new API endpoints: add controller methods under `InsTK.Server/Controllers` and a matching interface/implementation under `InsTK.Shared.Interfaces` / `InsTK.Server.Data.Services`.
- Add client pages/components: put Blazor pages in `InsTK.Client/Pages` and shared UI models in `InsTK.Shared/ViewModels`.
- Add EF entities: update `ApplicationDbContext` and add migrations via `dotnet ef migrations add <Name> --project InsTK.Server` and `dotnet ef database update` (CI runs `db.Database.Migrate()` on startup too).

## Quick examples (code pointers)
- To find where API URLs are set on the client, see `HttpClient` BaseAddress in [InsTK.Client/Program.cs](../InsTK.Client/Program.cs) (reads `FrontendUrl`).
- Server creates roles on startup (Admin, Instructor) and seeds an admin when `Admin:Email` and `Admin:Password` are set: see [InsTK.Server/Program.cs](../InsTK.Server/Program.cs).

## If you need more info
- Ask for local `appsettings.Development.json` values for database or for an exported Postman collection if API examples are needed.

---
Please review and tell me if you'd like more details (examples for auth flows, tests, or local env setup).

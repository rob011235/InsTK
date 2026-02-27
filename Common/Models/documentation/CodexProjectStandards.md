# Codex Project Standards (InsTK)

This document captures implementation standards established during SME questionnaire work so future Codex sessions stay consistent.

## Architecture and Data Access

- Do not inject `ApplicationDbContext` directly into `.razor` pages.
- Use interface-based data services in `Common/Interfaces`.
- Implement those interfaces in `Server/Data/Services`.
- Register data services in `Server/Program.cs` with DI.
- Razor pages should inject service interfaces only.

## Razor Page Standards (Server Project)

- Server-side interactive pages should explicitly set:
  - `@rendermode InteractiveServer`
- For forms using `<EditForm>`, always set a unique `FormName`.
- For route parameters (for example `@page "/x/{Token}"`), define matching `[Parameter]` properties in `@code`.
- Keep each questionnaire step page's privacy footer link:
  - Text: `How we use your data (Privacy Notice)`
  - Target: `/sme-questionnaire/privacy`

## Routing and Access Conventions

- SME questionnaire user flow routes:
  - `/sme-questionnaire/{token}`
  - `/sme-questionnaire/{token}/profile`
  - `/sme-questionnaire/{token}/skills`
  - `/sme-questionnaire/{token}/equipment`
  - `/sme-questionnaire/privacy`
- Admin pages should require:
  - `@attribute [Authorize(Roles = "Admin")]`
- SME-facing questionnaire pages are currently anonymous:
  - `@attribute [AllowAnonymous]`

## Current Invite Token Behavior

- Current implementation uses `SmeQuestionnaireResponse.Id` (`Guid`) as the invite token.
- Admin invite generation creates a draft `SmeQuestionnaireResponse` and shares link `/sme-questionnaire/{responseId}`.
- This is a working interim model, not a hardened opaque-token system yet.

## Questionnaire Workflow Rules Implemented

- Start page requires clicking `I have read the Privacy Notice`.
- Clicking acknowledgment stores first-click timestamp in:
  - `SmeQuestionnaireResponse.PrivacyAcknowledgedOn`
- Begin button is disabled until privacy acknowledgment exists.
- Profile and skills steps support save-draft behavior.

## Data Model and Migration Notes

- `SmeQuestionnaireResponse` includes:
  - `PrivacyAcknowledgedOn`
  - `FinalOpenEndedAnswer`
- Migrations were rolled up into:
  - `20260227194920_AddSmeQuestionnaireModels`
- Before rebuilding while app runs, stop the server process to avoid locked DLL copy failures.

## SQLite/EF Core Caution

- SQLite cannot translate `DateTimeOffset` ordering in some LINQ `OrderBy` queries.
- If this appears, fetch records first and order in memory (`ToListAsync()` then `OrderBy(...)`).

## Current Admin Pages (Implemented)

- Curriculums:
  - `/admin/curriculums`
  - `/admin/curriculums/create`
- SME Questionnaires:
  - `/admin/sme-questionnaires`
  - `/admin/sme-questionnaires/create`
- SMEs:
  - `/admin/smes/create`
- Invites:
  - `/admin/sme-questionnaire-invites`
  - `/admin/sme-questionnaire-invites/list`

## Style and Consistency Guidance

- Prefer incremental feature delivery with placeholder pages for next steps when needed for navigation continuity.
- Keep page logic thin and push persistence logic into data services.
- Build after changes to validate compile and route/form wiring.

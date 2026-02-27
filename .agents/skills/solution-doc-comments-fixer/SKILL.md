---
name: solution-doc-comments-fixer
description: Fix missing or invalid C# XML documentation comments across an entire .NET solution with minimal behavioral risk. Use when a user asks to clean documentation-comment warnings/errors solution-wide (for example CS1591, CS1570, CS1587, SA16xx, and related XML-doc diagnostics) or to enforce documentation comment quality before CI/release.
---

When the user invokes this skill, they will provide:
- solution path (`.sln`) or project path (`.csproj`)
Optional:
- scope limits (specific projects/folders)
- strictness target (warnings only vs warnings + suggestions)
- exclusions (generated code, migrations, designer files, test projects)

Rules:
1) Ask ZERO follow-up questions unless required to avoid editing the wrong solution/project.
2) Resolve target context:
   - If a `.sln` path is provided, use it.
   - If only a `.csproj` path is provided, use it.
   - If neither is provided, find the nearest `.sln` from the current directory upward; otherwise pick the primary `.sln` in workspace root.
3) Build a diagnostic baseline before edits:
   - Run `dotnet build <target>` and capture documentation-related diagnostics only (CS1591, CS1570, CS1587, SA16xx, IDE XML-doc diagnostics).
   - If strictness includes suggestions, include analyzer info-level diagnostics during formatting and validation.
4) Apply safe automated fixes first:
   - Run `dotnet format analyzers <target>` with a scope matching user limits.
   - Rebuild and re-check doc-comment diagnostics.
5) Fix remaining diagnostics manually with minimal risk:
   - Add missing XML docs for public/protected API surface first.
   - Keep summaries factual and concise; do not invent behavior not present in code.
   - Add/repair `<param>`, `<returns>`, `<exception>`, and `<typeparam>` tags only when applicable.
   - Prefer `<inheritdoc/>` when an override/interface implementation clearly inherits contract text and project conventions allow it.
   - Correct malformed XML, invalid cref references, and misplaced doc tags.
6) Exclude or skip unsafe/non-source targets unless user explicitly asks:
   - Skip generated files (`*.g.cs`, `*.designer.cs`, migrations snapshots, tool-generated code) unless they are the explicit target.
7) Iterate until clean or blocked:
   - Rebuild after each batch of edits.
   - Continue until documentation diagnostics are resolved for in-scope files or blocked by external issues.
8) Report:
   - Provide concise summary of files changed and diagnostics resolved.
   - If diagnostics remain, list each remaining code and why it was not fixed safely.

Output should be only code changes and concise verification notes; no long explanations.

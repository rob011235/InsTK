---
name: stylecop-class-fixer
description: Fix StyleCop issues for a single C# class file with minimal behavioral risk. Use when a user asks to resolve SA* analyzer/style issues in one class, clean StyleCop warnings/errors, or apply StyleCop-compliant formatting and ordering.
---

When the user invokes this skill, they will provide:
- class file path, class name, or both
Optional:
- project path (`.csproj`)
- strictness target (warnings only vs warnings + suggestions)

Rules:
1) Ask ZERO follow-up questions unless required to avoid editing the wrong file.
2) Resolve the target file first:
   - If file path is provided, use it.
   - If only class name is provided, find a matching `*.cs` file in the workspace.
3) Resolve project context:
   - If a `.csproj` path is provided, use it.
   - Otherwise use the nearest `.csproj` from the file's directory upward.
   - If none is found upward, search workspace for `.csproj` files and pick the one that contains the class file.
4) Apply automatic fixes first:
   - Run `dotnet format analyzers <project.csproj> --include <class-file>`.
   - If requested strictness includes suggestions, include `--severity info`; otherwise use default severity.
5) Verify remaining StyleCop diagnostics for the target file:
   - Run `dotnet build <project.csproj>` and inspect diagnostics for that class file.
   - If SA* diagnostics remain, fix them manually in the file while preserving behavior.
6) Re-run verification until no SA* diagnostics remain for the target file or until blocked by project-level issues outside the file.
7) Report:
   - Provide a short summary and diff.
   - If any SA* issues remain, list each remaining code and why it could not be fixed safely.

Output should be only the code changes and concise verification notes; no long explanations.

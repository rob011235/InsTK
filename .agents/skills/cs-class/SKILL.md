---
name: cs-class
description: Generate a C# class file from a simple parameter list (type name pairs), optionally with namespace and constructor.
---

When the user invokes this skill, they will provide:
- class name
- a list of properties as "Type Name" pairs (comma-separated)
Optional:
- namespace
- file path
- whether to generate constructor
- whether to use record/class
- whether properties are init-only

Rules:
1) Ask ZERO follow-up questions unless required to avoid overwriting an existing file.
2) Parse the user’s input list. Accept formats like:
   - "string FirstName, int Age"
   - "string firstName, int age" (PascalCase property names)
   - "FirstName:string, Age:int" (support this too if given)
3) Generate idiomatic C#:
   - Namespace if provided
   - public class {Name}
   - public {Type} {PropertyName} { get; init; }  (default init-only)
4) If constructor requested:
   - Create a constructor with parameters camelCase
   - Assign to properties
5) File placement:
   - If a path is provided, write there.
   - Otherwise, look for a project folder (a directory containing a `.csproj`) in the current workspace.
   - If a project folder is found, default to `{ProjectFolder}/Common/Models/{ClassName}.cs`.
   - If no project folder is found, default to `Common/Models/{ClassName}.cs` from the current directory.
6) Show a short summary plus a diff.

Output should be only the code changes; do not include long explanations.

# Grading App v1 – Architecture & Design Spec

## Overview

This system consists of two primary applications:

### 1. Blazor Web App (Authoring System)
Used by instructors to:
- Create tutorials
- Create assignments (Tutorial or Programming)
- Link tutorials to assignments
- Generate WordPress-ready markup
- Export assignment definition packages

### 2. MAUI App (Grading Workstation)
Used by instructors to:
- Import assignment definitions
- Import student repo submissions (CSV/template)
- Clone repositories
- Normalize and analyze code
- Run grading locally using Ollama
- Review and finalize grades
- Export results

---

## Core Design Principles

- Only **two assignment types**:
  - Tutorial
  - Programming
- Default to simple grading structures
- Task-based grading
- Equal weighting for tutorials
- Local-first grading with Ollama
- Human review required

---

## Assignment Types

### Tutorial Assignment
- Completion-based
- Equal-weight steps
- Output: percent complete + missed steps

### Programming Assignment
- Task-based grading
- Output: completed/missed tasks + feedback + tutorials to review

---

## Core Models

### AssignmentType
```csharp
public enum AssignmentType
{
    Tutorial,
    Programming
}
```

### TutorialDefinition
```csharp
public class TutorialDefinition
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string Summary { get; set; } = "";
    public string Technology { get; set; } = "";
    public List<TutorialBlock> Blocks { get; set; } = new();
    public List<TutorialStep> Steps { get; set; } = new();
}
```

### TutorialStep
```csharp
public class TutorialStep
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string Instruction { get; set; } = "";
}
```

### AssignmentDefinition
```csharp
public class AssignmentDefinition
{
    public string Id { get; set; } = "";
    public AssignmentType Type { get; set; }
    public string Title { get; set; } = "";
    public string Summary { get; set; } = "";
}
```

---

## Submission Model

```csharp
public class RepoSubmissionReference
{
    public string StudentId { get; set; } = "";
    public string StudentName { get; set; } = "";
    public string RepositoryUrl { get; set; } = "";
    public string? BranchName { get; set; }
}
```

---

## v1 Scope

### Included
- Tutorial authoring
- Assignment authoring
- Markdown generation
- CSV repo import
- GitHub cloning
- Ollama grading

### Not Included
- LMS APIs
- Grade pushback
- Multi-course dashboards

---

## Next Step

Build Blazor Web App:
- Tutorial CRUD
- Step editor
- Markdown renderer

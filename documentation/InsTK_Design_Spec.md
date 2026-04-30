# InsTK v1 Architecture and Design Spec (Revised)

## Overview

InsTK v1 is designed to standardize how tutorials and programming assignments are authored and graded. The goal is to reduce ambiguity for AI-assisted grading by aligning authored content, grading expectations, and evaluation workflows.

InsTK is the **authoring source of truth**. Instructors create tutorials and assignments in InsTK, then publish to external systems.

The local AI runtime for grading in v1 is Ollama.

---

## Primary Applications

### 1. Blazor Web App (Authoring System)

Used by instructors to:

* create tutorials
* create assignments (`Tutorial` or `Programming`)
* link tutorials to assignments
* preview tutorials as student-facing web pages
* export tutorials to WordPress-ready content
* define programming assignment criteria
* publish assignments to downstream systems

---

### 2. MAUI App (Grading Workstation)

Used by instructors to:

* import submissions from Brightspace
* extract repository URLs from submission comments
* clone repositories
* run grading using Ollama
* review proposed grades
* override grading decisions
* publish final grades to Brightspace

---

## Core Design Principles

* Only two assignment types: `Tutorial` and `Programming`
* Minimize inference required by AI grader
* Prefer artifact-based grading over heuristic interpretation
* Local-first grading using Ollama
* AI generates **proposed grades only**
* Instructor is always the final authority
* Keep authoring lightweight and fast
* Prioritize deterministic and explainable grading

---

## Assignment Types

### Tutorial Assignment

Tutorial assignments are **completion-based** and derived directly from tutorial steps.

#### Key Rules

* Only steps with **reference code** are graded
* Each graded step evaluates **one file**
* All graded steps are **equally weighted**
* Steps without reference code are **instruction-only**

#### Grading Behavior

For each graded step:

1. Find the best matching student file using `ReferenceFileName`
2. If no file is found → `fail`
3. If found → compare student file to reference code using Ollama
4. Return:

   * `pass`
   * `fail`
   * `uncertain`
   * rationale

#### Evaluation Criteria (Global)

Ollama evaluates based on:

* Core constructs present
* Key methods present
* Framework usage patterns
* Ignore superficial differences (naming, formatting, ordering)

#### Output

* Step results (`pass / fail / uncertain`)
* Proposed score = % of steps passed
* If any step is `uncertain` → overall status = **Uncertain**

---

### Programming Assignment

Programming assignments are **rubric-driven**.

#### Criterion Model

Each criterion:

* has a description
* may define:

  * expected files
  * expected concepts
  * grading hints

#### Evaluation Rules

* All expectations must be met to pass
* Each criterion returns:

  * `pass`
  * `fail`
  * `uncertain`

#### Scoring

* Equal weight per criterion
* Proposed score = (# passed) / (total criteria)
* If any criterion is `uncertain` → overall status = **Uncertain**

---

## Tutorial Authoring Model

```csharp
public class TutorialDefinition
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = "";
    public string Summary { get; set; } = "";
    public string Technology { get; set; } = "";

    public List<TutorialStep> Steps { get; set; } = new();

    public int DraftRevisionNumber { get; set; }
    public Guid? CurrentPublishedRevisionId { get; set; }
}
```

```csharp
public class TutorialStep
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = "";
    public string InstructionMarkdown { get; set; } = "";

    public string? ReferenceFileName { get; set; }
    public string? ReferenceCode { get; set; }

    public string GradingHints { get; set; } = "";
}
```

---

## Programming Assignment Model

```csharp
public class AssignmentDefinition
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public AssignmentType Type { get; set; }

    public string Title { get; set; } = "";
    public string Summary { get; set; } = "";

    public List<Criterion> Criteria { get; set; } = new();

    public int DraftRevisionNumber { get; set; }
    public Guid? CurrentPublishedRevisionId { get; set; }
}
```

```csharp
public class Criterion
{
    public string Description { get; set; } = "";

    public List<string> ExpectedFiles { get; set; } = new();
    public List<string> ExpectedConcepts { get; set; } = new();

    public string GradingHints { get; set; } = "";
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

    public string? SubmissionComments { get; set; }
}
```

### Submission Rules

* Only the **first URL** in submission comments is used
* If URL includes a branch → use it
* Otherwise → default to `main`
* Full submission comments are shown to instructor

---

## Grading Workflow

1. Instructor selects course and assignment
2. App imports submissions
3. App extracts first repository URL
4. App determines branch (`main` or from URL)
5. App clones repository
6. App runs grading immediately
7. Results displayed in list view

---

## Grade Results Model

```csharp
public enum GradingResultStatus
{
    Pass,
    Fail,
    Uncertain
}
```

```csharp
public class AssignmentGradeResult
{
    public double? ProposedScore { get; set; }
    public double? FinalScore { get; set; }

    public bool InstructorOverrodeScore { get; set; }

    public string? InstructorFeedbackToStudent { get; set; }
    public string? InstructorOverrideNotes { get; set; }
}
```

---

## Instructor Review Workflow

Instructor can:

* review all grading results
* override individual items
* override final score directly
* add student-facing feedback
* add private notes
* change branch and regrade

---

## UI Behavior

### Default List View

* Student Name
* Proposed Score
* Status (`Complete`, `Uncertain`, `Error`)
* Repo URL
* Branch

### Expanded View

* Submission comments
* Detailed grading results
* Rationales
* Error messages (if any)

---

## Error Handling

If repository clone fails:

* Status = `Error`
* Store error message
* Instructor can fix and retry

---

## Publishing

### Tutorial Publishing

* Render tutorial as web page
* Export WordPress-ready content (HTML/Markdown)

### Programming Assignment Publishing

* Create assignment in Brightspace
* Create rubric structure

---

## Revision Model

* Drafts are editable
* Publishing creates immutable revision
* Grading links to revision snapshot

---

## Storage Direction

* Relational core database
* Snapshot storage for published revisions

---

## v1 Scope

### Included

* tutorial authoring
* programming assignment authoring
* tutorial preview
* WordPress export (content generation)
* Brightspace submission import
* repository cloning
* Ollama grading
* instructor review and override
* grade publishing
* revision tracking

### Not Included

* full LMS API integrations
* automated WordPress publishing (initially)
* multi-course dashboards

---

## Open Design Question

Should each grading run store:

* exact Ollama model
* prompt version
* inputs used

to allow full grading reproducibility?

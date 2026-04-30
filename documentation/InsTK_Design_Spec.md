# InsTK v1 Architecture and Design Spec

## Overview

InsTK v1 is intended to restart the grading approach from the ground up so that
tutorials and assignments are defined in a consistent, grading-friendly format from
the start. The objective is to reduce the ambiguity an AI grader has to resolve later
by making authored learning materials, grading hints, and assignment structures more
consistent.

InsTK is intended to act as the authoring source of truth. Instructors draft tutorials
and assignments in InsTK first, then publish outward to delivery systems.

The intended local AI runtime for grading in v1 is Ollama.

## Primary Applications

### 1. Blazor Web App (Authoring System)

Used by instructors to:

- create tutorials
- create assignments of type `Tutorial` or `Programming`
- link tutorials to assignments
- author assignment descriptions and tutorial-linked assignment instructions
- generate WordPress-ready tutorial content
- export assignment definition packages
- publish tutorials and assignments to downstream systems

### 2. MAUI App (Grading Workstation)

Used by instructors to:

- import assignment definitions
- import student repository submissions
- clone repositories
- normalize and analyze code
- run grading locally using Ollama
- review and edit draft grades and feedback
- compare multiple grading runs
- publish approved grades and comments to Brightspace
- export grading results

## Product Goal

The system is designed around two kinds of evaluation:

- Tutorial grading:
  determine whether a student substantially completed a guided workflow and produced
  an outcome reasonably close to the finished tutorial.
- Programming assignment grading:
  evaluate a programming solution against a specification using knowledge drawn from
  multiple tutorials.

Assignment descriptions, tutorial-linked assignment instructions, grading hints,
reference repository settings, and rubric structures should all remain connected inside
InsTK so published content and grading behavior stay aligned.

## Core Design Principles

- Only two assignment types in v1: `Tutorial` and `Programming`
- Tutorials and assignments should be authored in a consistent structure that supports
  later AI-assisted grading
- Prefer structures that minimize inference work for Ollama
- Local-first grading with Ollama
- Human review is required before grade publication
- AI-generated grades are always draft grades until approved by an instructor
- Tutorials should stay lightweight
- Programming assignments may use richer rubric criteria when needed

## Assignment Types

### Tutorial Assignment

- completion-based
- equal-weight steps
- represents a guided workflow the student follows step by step
- grading question:
  did the student substantially complete the workflow, and is the final result
  reasonably close to the expected finished tutorial?
- primary evidence comes from the existence and quality of appropriate files in the
  student's submitted repository
- file names do not need to match exactly
- file contents do not need to match exactly
- implementations should be substantively similar and function in a similar manner
- tutorials should remain lightweight and should not require a formal multi-criteria
  rubric in v1
- tutorial steps should include author-only grading hints to reduce AI inference work
- output:
  completed steps, missed steps, percent complete, and an auto-computed draft score
  from `0` to `100`

### Programming Assignment

- task-based grading
- represents a programming assignment that asks the student to build a solution that
  meets a specification
- expected to draw on knowledge from multiple tutorials
- default grading standard:
  substantively similar and functionally similar to the expected solution
- assignments may define explicit rubric criteria for required features,
  architectural elements, and other requirements that must be evaluated directly
- rubric structure should align closely with a Brightspace rubric
- programming assignments may include author-only grading guidance to reduce AI
  inference work
- output:
  completed criteria, missed criteria, feedback, and a draft score from `0` to `100`

## Tutorial Authoring Guidance

Tutorials should be authored to minimize how much reconstruction the AI grader has to
do. In addition to student-facing instructions, each step should support author-only
grading hints such as:

- expected files or file patterns
- concepts or implementation elements the grader should look for
- brief notes about what a substantially complete result looks like

These hints are intended to guide grading and do not need to be shown to students.

## Programming Assignment Authoring Guidance

Programming assignments should support author-only grading guidance such as:

- expected files or file patterns
- required concepts or implementation elements
- likely mistakes or anti-patterns to check
- notes that help the grader distinguish acceptable variation from missing requirements

## Reference Repositories

### Tutorials

Tutorials should normally provide a reference solution repository so grading can
compare the student submission to concrete artifacts instead of relying only on prose.

Reference solution defaults:

- branch: `main`
- optional subpath when only part of the repository is relevant

The reference repository may be omitted for tutorials that do not involve code or do
not rely on repository artifacts.

### Programming Assignments

Programming assignments may optionally provide a reference solution repository to
support artifact-based comparison during grading.

Reference solution defaults:

- branch: `main`
- optional subpath when only part of the repository is relevant

## Rubric Direction

Programming assignment rubrics should be authored in a structure that maps cleanly to a
Brightspace rubric. The long-term intent is for rubric definitions created in this
system to be transferable into Brightspace through browser automation tooling such as
Puppeteer or a similar package.

For v1, the default rubric model should be criterion-based pass/fail scoring with
optional point values per criterion. This keeps grading decisions narrow and reduces
ambiguity for the AI grader while still allowing a final numeric score from `0` to
`100` to be calculated from the rubric results.

Richer Brightspace performance levels may be supported later, but they should not be
the default grading model in v1.

## Submission Model

Student submissions are repository-based. Students are expected to submit a repository
for grading.

Supported repository sources in v1:

- GitHub
- Diversion, when large files are anticipated

Submission rules:

- default branch to grade: `main`
- if a student wants a branch other than `main` graded, they should provide a
  repository URL that points to that branch
- student notes should be captured separately from the repository URL
- student notes should be shown to the instructor during grade review

## Publishing Workflow

Publishing should support the following authoring flows:

- Tutorial publishing:
  the instructor drafts the tutorial in InsTK and clicks publish. Publish should
  create the WordPress tutorial page and create a Brightspace assignment that
  references the tutorial.
- Programming assignment publishing:
  the instructor drafts the assignment in InsTK and clicks publish. Publish should
  create the assignment in Brightspace using content authored in InsTK.

When rubric data is defined for a programming assignment, publishing should also create
or synchronize the related Brightspace rubric.

For published tutorial assignments, the Brightspace assignment should include:

- a link to the published WordPress tutorial
- instructions to follow along with the tutorial
- instructions to save the work in a repository in the course's GitHub organization
- instructions to include the repository URL in the Brightspace submission
- a statement that the default branch to be graded is `main`
- instructions for how to specify a different branch by submitting a repository URL
  that points to that branch
- a separate field or captured notes area for any student comments beyond the
  repository URL

## Grade Review Workflow

The grading workstation should produce draft grades and draft feedback comments, not
final grades. Instructors should be able to review all proposed grades and comments in
the user interface, edit them as needed, and then explicitly direct the system to
publish the approved grades and comments to Brightspace.

The grading score model for proposed grades should support a numeric score from `0` to
`100`.

If the AI grader is uncertain about whether a tutorial step or programming criterion
passes, it should mark that item as uncertain and continue grading the rest of the
submission. Uncertainty should not block the grading run from completing.

The full grading pass should run to completion and then present the results to the
instructor. The instructor can then inspect the repository or project directly and
adjust grades or comments before publishing.

Each graded item should store reviewable result data so the instructor can understand
why the AI proposed its decision. At minimum, each graded item should capture:

- result: `pass`, `fail`, or `uncertain`
- score impact
- short rationale
- uncertain flag

## Grading Runs

The system should allow multiple grading runs for the same submission against the same
assignment revision. This supports reruns after prompt changes, grading-hint changes,
or operational failures.

The v1 grading-run model should support:

- retaining all grading runs for audit and comparison
- marking one grading run as the current review target
- allowing only one approved published outcome to be active for a submission at a time

## Revision Model

To preserve traceability without introducing a heavy version-control workflow, v1
should use a lightweight publish-revision model.

The revision model should support:

- a stable identity for each tutorial and assignment
- editable drafts before publish
- an immutable published revision created at publish time
- a link from grading records to the published revision used for that grading run
- full snapshot storage for each published revision rather than only references back to
  current draft records

## Storage Direction

The recommended storage approach for v1 is a relational primary database with
document-style snapshot storage for immutable published revisions.

The operational core of the system is relational. Published revision payloads should
preserve a full snapshot of the authored content, rubric data, reference repository
settings, and grading hints used at publish time.

## Core Models

### AssignmentType

```csharp
public enum AssignmentType
{
    Tutorial,
    Programming
}
```

### Identifier Convention

```csharp
// v1 uses Guid identifiers for persisted domain records.
```

### TutorialDefinition

```csharp
public class TutorialDefinition
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = "";
    public string Summary { get; set; } = "";
    public string Technology { get; set; } = "";
    public List<TutorialStep> Steps { get; set; } = new();
    public string? ReferenceRepoUrl { get; set; }
    public string? ReferenceBranchName { get; set; }
    public string? ReferenceSubPath { get; set; }
    public int DraftRevisionNumber { get; set; }
    public Guid? CurrentPublishedRevisionId { get; set; }
}
```

### TutorialStep

```csharp
public class TutorialStep
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = "";
    public string Instruction { get; set; } = "";
    public List<string> ExpectedFiles { get; set; } = new();
    public List<string> ExpectedConcepts { get; set; } = new();
    public string GradingHints { get; set; } = "";
}
```

### AssignmentDefinition

```csharp
public class AssignmentDefinition
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public AssignmentType Type { get; set; }
    public string Title { get; set; } = "";
    public string Summary { get; set; } = "";
    public string? ReferenceRepoUrl { get; set; }
    public string? ReferenceBranchName { get; set; }
    public string? ReferenceSubPath { get; set; }
    public List<string> ExpectedFiles { get; set; } = new();
    public List<string> ExpectedConcepts { get; set; } = new();
    public List<string> CommonIssuesToCheck { get; set; } = new();
    public string GradingHints { get; set; } = "";
    public int DraftRevisionNumber { get; set; }
    public Guid? CurrentPublishedRevisionId { get; set; }
}
```

### RepoSubmissionReference

```csharp
public class RepoSubmissionReference
{
    public string StudentId { get; set; } = "";
    public string StudentName { get; set; } = "";
    public string RepositoryUrl { get; set; } = "";
    public string? SubmissionNotes { get; set; }
    public string? BranchName { get; set; }
}
```

## v1 Scope

### Included

- tutorial authoring
- programming assignment authoring
- markdown generation
- WordPress tutorial publishing
- Brightspace assignment creation
- Brightspace rubric creation
- CSV repository import
- GitHub cloning
- Ollama grading
- instructor review and editing of proposed grades and comments
- publishing approved grades and comments to Brightspace
- lightweight publish revisions
- multiple grading runs with audit history

### Not Included

- broad LMS API integrations beyond the targeted Brightspace publishing workflow
- multi-course dashboards

## Open Design Question

Should each grading run store the exact Ollama model and prompt or revision inputs used
so grading differences across runs can be traced precisely?

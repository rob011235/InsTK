# SME Questionnaire Models

This model set follows a common .NET questionnaire pattern:

-   Questionnaire definition (what is asked).
-   Questionnaire response (what each SME answers).
-   Domain lists for curriculum planning (skills, equipment, software).

## Curriculum.cs

-   Guid: Id
-   string: Title
-   string?: Description
-   DateTimeOffset?: TargetStartDate
-   DateTimeOffset?: TargetEndDate
-   List: SubjectMatterExperts
-   List: Questionnaires

## SubjectMatterExpert.cs

-   Guid: Id
-   string: FullName
-   string?: Company
-   string?: JobTitle
-   string?: Email
-   string?: Phone
-   string?: Website
-   string?: Location
-   List: QuestionnaireResponses

## SmeQuestionnaire.cs

Represents the questionnaire template sent to SMEs.

-   Guid: Id
-   Guid: CurriculumId
-   string: Title
-   string?: Description
-   bool: IsActive
-   DateTimeOffset: CreatedOn
-   string?: FinalOpenEndedPrompt
-   bool: IsFinalOpenEndedRequired
-   List: Sections

## QuestionnaireSection.cs

Groups questions by category.

-   Guid: Id
-   Guid: QuestionnaireId
-   QuestionCategory: Category (Skills, Equipment, Software, General)
-   string: Title
-   string?: Instructions
-   int: DisplayOrder
-   List: Questions

## QuestionnaireQuestion.cs

Definition of a single question in a section.

-   Guid: Id
-   Guid: SectionId
-   string: Prompt
-   QuestionInputType: InputType (Text, MultiSelect, Number, YesNo, Rating)
-   bool: IsRequired
-   int: DisplayOrder
-   string?: HelpText
-   List: Options

## QuestionOption.cs

Selectable option for select-style questions.

-   Guid: Id
-   Guid: QuestionId
-   string: Value
-   string: Label
-   int: DisplayOrder

## SmeQuestionnaireResponse.cs

A single SME submission for a questionnaire.

-   Guid: Id
-   Guid: QuestionnaireId
-   Guid: SubjectMatterExpertId
-   ResponseStatus: Status (Draft, Submitted, Reviewed)
-   DateTimeOffset: StartedOn
-   DateTimeOffset?: SubmittedOn
-   DateTimeOffset?: PrivacyAcknowledgedOn
-   string?: ReviewerNotes
-   string?: FinalOpenEndedAnswer
-   List: Answers
-   List: Skills
-   List: Equipment
-   List: Software

## QuestionnaireAnswer.cs

Raw answers tied to questionnaire questions.

-   Guid: Id
-   Guid: ResponseId
-   Guid: QuestionId
-   string?: ValueText
-   decimal?: ValueNumber
-   bool?: ValueBool
-   int?: ValueRating

## SmeSkill.cs

Normalized skill captured from response.

-   Guid: Id
-   Guid: ResponseId
-   string: Name
-   ProficiencyLevel: Proficiency (Beginner, Intermediate, Advanced, Expert)
-   int?: YearsExperience
-   bool: IsPrimary
-   string?: Notes

## SmeEquipment.cs

Normalized equipment/tooling used by SME.

-   Guid: Id
-   Guid: ResponseId
-   string: Name
-   EquipmentType: Type (Hardware, Instrument, Facility, Safety)
-   string?: Manufacturer
-   string?: Model
-   string?: VersionOrSpec
-   UsageFrequency: Frequency (Daily, Weekly, Monthly, Rarely)
-   bool: RequiredForJob
-   string?: Notes

## SmeSoftware.cs

Normalized software platforms used by SME.

-   Guid: Id
-   Guid: ResponseId
-   string: Name
-   string?: Vendor
-   string?: Version
-   SoftwareCategory: Category (CAD, IDE, Office, Analysis, LMS, Other)
-   UsageFrequency: Frequency (Daily, Weekly, Monthly, Rarely)
-   bool: RequiredForJob
-   string?: Notes

## Shared Enums

### QuestionCategory

-   General
-   Skills
-   Equipment
-   Software

### QuestionInputType

-   Text
-   MultiSelect
-   Number
-   YesNo
-   Rating

### ResponseStatus

-   Draft
-   Submitted
-   Reviewed

### ProficiencyLevel

-   Beginner
-   Intermediate
-   Advanced
-   Expert

### EquipmentType

-   Hardware
-   Instrument
-   Facility
-   Safety

### SoftwareCategory

-   CAD
-   IDE
-   Office
-   Analysis
-   LMS
-   Other

### UsageFrequency

-   Daily
-   Weekly
-   Monthly
-   Rarely

## Optional Validation Guidance

-   Use `[Required]` for `Title`, `Prompt`, and normalized item `Name` fields.
-   Use `[MaxLength]` for free text fields.
-   Use unique indexes where appropriate (for example one response per SME per questionnaire if needed).
-   Use `DateTimeOffset` for audit/date fields instead of `DateTime`.

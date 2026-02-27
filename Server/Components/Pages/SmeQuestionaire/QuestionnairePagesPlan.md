# SME Questionnaire Pages Plan

## Goal
Build a questionnaire flow for subject matter experts (SMEs) that does **not require a login account**.

## Core Approach (No Login)
- Use an invite-style link with a unique token per SME request (for example: `/sme-questionnaire/{token}`).
- Validate token server-side before showing questionnaire pages.
- Allow save-as-draft and resume using the same token.
- Expire tokens after a configurable duration.
- Mark responses as submitted and allow SMEs to edit after final submit.

## Why Token Access Instead of Anonymous Open Form
- Prevents random or spam submissions.
- Lets each SME submit one intended response.
- Preserves a no-account user experience.

## Suggested Page Structure
- `SmeQuestionnaireStart.razor`
Purpose: Welcome, instructions, estimated time, begin button, and visible privacy summary.

- `SmeQuestionnaireProfile.razor`
Purpose: Collect SME profile/contact details.

- `SmeQuestionnaireSkills.razor`
Purpose: Collect skills, proficiency, years experience.

- `SmeQuestionnaireEquipment.razor`
Purpose: Collect equipment used (replaces "Materiel").

- `SmeQuestionnaireSoftware.razor`
Purpose: Collect software used and usage frequency.

- `SmeQuestionnaireFinalQuestion.razor`
Purpose: Capture final open-ended response before review/submit.

- `SmeQuestionnaireReview.razor`
Purpose: Show all entered data, allow edits before submit.

- `SmeQuestionnaireSubmitted.razor`
Purpose: Confirmation page after final submission.

- `SmeQuestionnairePrivacy.razor`
Purpose: Explain how data is used and explicitly state we do not sell personal information.

## Routing Pattern
- Start route: `/sme-questionnaire/{token}`
- Child steps route examples:
  - `/sme-questionnaire/{token}/profile`
  - `/sme-questionnaire/{token}/skills`
  - `/sme-questionnaire/{token}/equipment`
  - `/sme-questionnaire/{token}/software`
  - `/sme-questionnaire/{token}/final-question`
  - `/sme-questionnaire/{token}/review`
  - `/sme-questionnaire/{token}/submitted`
- Privacy route: `/sme-questionnaire/privacy`

## UX Requirements
- Show progress indicator (step X of Y).
- Save on step change and on explicit Save Draft click.
- Validate required fields per step.
- Keep page navigation simple: Back, Next, Save Draft.
- Keep clear submit confirmation before final post.
- Show privacy summary on welcome/start page and link to full notice.
- Require an acknowledgment/read button before users can begin.

## Required Footer Link
Every questionnaire step page should include a bottom link:
- Link text: `How we use your data (Privacy Notice)`
- Target page: `/sme-questionnaire/privacy`

## Welcome Page Privacy Requirement
The start page should show a short privacy block before users begin:
- We collect this information to support curriculum development.
- Access is limited to authorized team members.
- We do not sell personal information.
- Full details are available at `/sme-questionnaire/privacy`.
- Include a required acknowledgment/read button before the Begin action is enabled.

## Final Open-Ended Question Requirement
- Add one final free-text question near the end of the flow.
- Prompt should come from questionnaire configuration (`FinalOpenEndedPrompt`).
- Response should save to `FinalOpenEndedAnswer` on the response model.
- Support required/optional behavior using `IsFinalOpenEndedRequired`.

## Privacy Notice Content Requirements
The privacy page should include at minimum:
- What data is collected.
- Why data is collected (curriculum development and planning).
- Who can access it (authorized staff only).
- Data retention expectations.
- Security statement (reasonable safeguards).
- Explicit statement: **"We do not sell your personal information."**
- Contact method for questions/removal requests.

## Data + Backend Notes
- Store token with response metadata (`CreatedOn`, `ExpiresOn`, `SubmittedOn`, `Status`).
- Keep token hashed in storage if possible.
- Add throttling/rate limiting on submit endpoints.
- Log minimal audit events (opened, saved, submitted).
- Persist final prompt/answer fields:
  - `SmeQuestionnaire.FinalOpenEndedPrompt`
  - `SmeQuestionnaire.IsFinalOpenEndedRequired`
  - `SmeQuestionnaireResponse.FinalOpenEndedAnswer`

## Implementation Phases
1. Build privacy page + route.
2. Build token validation and start page (with privacy summary).
3. Build step pages (profile, skills, equipment, software).
4. Build final open-ended question page and persistence.
5. Build review + submit pages.
6. Add save draft/resume behavior.
7. Add final validation and anti-abuse/rate limit.

## Decisions (Finalized)
- Token lifetime (for example 14 days vs 30 days). 90 days.
- Whether SMEs can edit after submission. Yes.
- Whether questionnaire is one long page or wizard-only. Wizard with forward back buttons.
- Require a privacy acknowledgment/read button before start. Yes.
- Max length for final open-ended response. 4000 characters.

## Deliverables for First Pass
- Working no-login wizard routes.
- Privacy notice page.
- Privacy summary on welcome/start page.
- Final open-ended question step.
- Footer link on all questionnaire pages.
- Save draft + final submit.

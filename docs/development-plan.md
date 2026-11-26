# Development Plan and Increment Log

## Selected process

Group 10 will use the Incremental Development Model, as selected in the project overview. Each increment passes through requirements, design, implementation, verification, and review before the next increment is treated as complete. Core interfaces and the database schema are kept stable where possible so separate team members can work on modules without guessing each other's method names or data fields.

## Planned increments

| Increment | Scope | Completion evidence |
| --- | --- | --- |
| 0. Planning and design baseline | Confirm scope, SRS, architecture, data model, UML, roles, schedule, and risks before accepting feature work. | Reviewed requirements and diagrams; agreed interfaces and schema. |
| 1. Project foundation | Solution structure, SQLite initialization, first staff access, and dashboard. | Clean build; first-run database initialization; login/setup and dashboard walkthrough. |
| 2. Room management | Seed and display 30 rooms across four confirmed categories; edit rates, capacities, and operational status. | Room CRUD and status test records; room list screenshot. |
| 3. Guest management | Register, search, view, and update guests. | Guest validation and retrieval test records; guest screen screenshot. |
| 4. Reservations | Search availability; create, retrieve, modify, and cancel reservations; prevent date conflicts. | Boundary and overlap tests; reservation walkthrough. |
| 5. Accommodation billing | Calculate nights and room charges from the assigned rate. | Billing calculation cases and summary screenshot. |
| 6. Check-in and check-out | Validate active bookings, record arrivals and departures, and integrate the complete reservation lifecycle. | End-to-end lifecycle and invalid-state test records. |
| 7. Release evidence | Run acceptance checks; complete report, screenshots, demo recording, and hand-in package. | Final build, test log, accessible repository/video links, and one report PDF. |

## Phase update convention

Each completed increment is recorded as a separate, reviewable Git change set with a phase-specific commit message, such as `phase-1: project foundation`. A phase is not marked complete until its implementation and verification evidence are present. Git authorship must reflect the actual contributors; shared work is not assigned to teammates who did not make the change.

## Current status

| Increment | Status | Notes |
| --- | --- | --- |
| 0. Planning and design baseline | In progress | Project overview and feasibility study exist; SRS and diagram set are being assembled from the assignment and agreed scope. |
| 1. Project foundation | Implementation built; verification pending | Three-project solution, SQLite initialization, first-run staff setup, salted password hashing, sign-in, dashboard, and early room/availability groundwork are implemented. Full solution build succeeds; first-run and sign-in walkthroughs remain to be recorded. |

## Team responsibilities

| Member | Primary role | Current responsibility from the project overview |
| --- | --- | --- |
| Ruth Ben-Osagie | C# Developer and System Modeller | UML, layered architecture, booking rules, billing logic, integration. |
| Julius-Imoisi Obehi | Project Manager and Requirements Analyst | Requirements, checkpoints, schedule, scope, final report. |
| Evbuomwan Osasogie (Osaso) | QA Tester | Test cases, boundary and double-booking checks, defect tracking and release checks. |

Matriculation numbers are not present in the supplied project overview and must be added by the group before final submission.

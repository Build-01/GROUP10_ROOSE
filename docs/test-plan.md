# Verification and Validation Plan

## Test levels

| Level | Purpose | Planned evidence |
| --- | --- | --- |
| Unit | Verify individual date, overlap, status, and charge rules. | Test ID, input, expected, actual, result. |
| Integration | Verify UI-to-service-to-SQLite behavior, constraints, and persistence. | Database-backed test results and captured defects. |
| System | Verify full front-desk workflows against FR-01 through FR-11. | End-to-end execution log and screenshots. |
| Acceptance | Confirm the delivered application meets the group's agreed scenarios and submission needs. | Group walkthrough/sign-off before release. |

## Planned test cases

| ID | Requirement | Scenario / input | Expected result | Status |
| --- | --- | --- | --- | --- |
| IT-00 | FR-01 | First application run with no staff account. | Setup form creates initial account; password is stored as a salted hash. | Pending execution |
| IT-00b | FR-01 | Sign in with correct then incorrect password. | Correct credentials open staff workspace; incorrect credentials are rejected. | Pending execution |
| UT-01 | FR-07 | Check-out is one day after check-in. | Date range is valid; one night. | Pending execution |
| UT-02 | FR-07 | Check-out equals check-in. | Date range is rejected. | Pending execution |
| UT-03 | FR-07 | Check-out is before check-in. | Date range is rejected. | Pending execution |
| UT-04 | FR-07 | Existing 10–12 Nov; requested 12–14 Nov. | No overlap. | Pending execution |
| UT-05 | FR-07 | Existing 10–12 Nov; requested 11–13 Nov. | Overlap detected. | Pending execution |
| UT-06 | FR-09 | Rate ₦120,000; three nights. | Charge is ₦360,000. | Passed (26 Nov 2026) |
| IT-01 | FR-11 | Initialize an empty database. | Schema and expected 30 seed rooms are created once. | Pending execution |
| IT-02 | FR-05 | Search dates conflicting with a confirmed reservation. | The reserved room is omitted. | Passed (26 Nov 2026) |
| IT-03 | FR-06/07 | Insert overlapping reservation for the same room. | Operation is rejected and no duplicate row is saved. | Passed (26 Nov 2026) |
| IT-04 | FR-08 | Cancel a reservation and search its dates. | The cancelled reservation no longer blocks the room. | Passed (26 Nov 2026) |
| IT-05 | FR-11 | Save records, close connection, reopen database. | Saved data is still present. | Pending execution |
| IT-06 | FR-03 | Add a room with valid details. | Room is stored and appears in the room list. | Pending execution |
| IT-07 | FR-03 | Edit a room's category, rate, capacity, and status. | Updated values persist and appear in the room list. | Pending execution |
| IT-08 | FR-03 | Add or rename a room to a duplicate number. | Database rejects the duplicate and UI explains the conflict. | Pending execution |
| IT-09 | FR-03/05 | Filter the room list by number, category, and operational status. | Only rooms matching all selected filters are shown. | Pending execution |
| IT-10 | FR-04 | Register a guest with name, phone, and email. | Guest record is saved and searchable by name, phone, and email. | Passed (26 Nov 2026) |
| IT-11 | FR-04 | Update a guest's contact details. | Updated name, phone, and email persist. | Passed (26 Nov 2026) |
| IT-12 | FR-06/07/08 | Create overlapping and adjacent stays, update a confirmed reservation, cancel it, then search the released dates. | Overlap is rejected, adjacent stay is allowed, update persists, and cancellation frees availability. | Passed (26 Nov 2026) |
| IT-13 | FR-09 | Load a reservation summary for a ₦120,000 room and three nights. | Summary reports the saved rate, three nights, and ₦360,000 charge. | Passed (26 Nov 2026) |
| ST-01 | FR-01/02 | Sign in and open dashboard. | Authorized staff sees current summary. | Pending execution |
| ST-02 | FR-04–09 | Register a guest and complete a valid booking. | Reservation and charge summary are correct. | Pending execution |
| ST-03 | FR-10 | Check in then check out an eligible reservation. | State changes are recorded in order. | Pending execution |

## Execution log

### 2026-11-26 — Phase 2 verification

Command: `dotnet test RooseHotel.Tests/RooseHotel.Tests.csproj --no-restore --verbosity minimal --logger "console;verbosity=normal"`.

Environment: .NET SDK 9.0.305, .NET runtime 9.0.9, VSTest 17.14.1, Windows x64. Result: **6 passed, 0 failed**. Covered room seeding (30 rooms, idempotent initialization), room create/update persistence, duplicate room rejection, reservation date/charge rules, availability overlap and adjacent dates, and password hashing. Defect fixed during verification: SQLite connection pooling kept temporary test databases open during teardown; the test helper now clears SQLite pools before deleting test folders.

System walkthroughs and screenshots remain planned acceptance/release evidence.

### 2026-11-26 — Phase 4 verification

Command: `dotnet test RooseHotel.Tests/RooseHotel.Tests.csproj --no-restore --verbosity minimal --logger "console;verbosity=minimal"` — **8 passed, 0 failed, 0 skipped**. The reservation lifecycle check verifies overlap rejection, adjacent dates, update persistence, cancellation status, and released-room availability. App build: `dotnet build RooseHotel.App/RooseHotel.App.csproj --no-restore --verbosity minimal -p:BaseOutputPath=bin/phase4/ -p:UseSharedCompilation=false` — **0 warnings, 0 errors**. Manual reservation UI walkthrough/screenshot remains for release evidence.

### 2026-11-26 — Phase 5 verification

Command: `dotnet test RooseHotel.Tests/RooseHotel.Tests.csproj --no-restore --verbosity minimal --logger "console;verbosity=minimal"` — **9 passed, 0 failed, 0 skipped**. Billing check: a three-night reservation in a ₦120,000 room returned a ₦360,000 accommodation total; test cases UT-06 and IT-13 passed. App build: `dotnet build RooseHotel.App/RooseHotel.App.csproj --no-restore --verbosity minimal -p:BaseOutputPath=bin/phase5/ -p:UseSharedCompilation=false` — **0 warnings, 0 errors**. Manual billing screen walkthrough/screenshot remains for release evidence.

### 2026-11-26 — Phase 3 verification

Command: `dotnet test RooseHotel.Tests/RooseHotel.Tests.csproj --no-restore --verbosity minimal --logger "console;verbosity=minimal"`.

Result: **7 passed, 0 failed, 0 skipped** (including guest create/update and search by name and phone). Full application build: `dotnet build RooseHotel.App/RooseHotel.App.csproj --no-restore --verbosity minimal -p:BaseOutputPath=bin/phase3/ -p:UseSharedCompilation=false` — **0 warnings, 0 errors**. Separate output was used because the already-running app holds its default build files open. Guest screen manual walkthrough and screenshot remain for release evidence.

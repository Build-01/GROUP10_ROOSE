# Verification and Validation Plan

## Test levels

| Level | Purpose | Evidence / limitation |
| --- | --- | --- |
| Unit | Verify date, overlap, status, password, and charge rules. | Automated xUnit tests. |
| Integration | Verify repository behavior against SQLite, including constraints and persistence. | Automated database-backed xUnit tests. |
| System | Verify the complete front-desk workflow in the Windows Forms application. | Requires a live staff setup/login and UI walkthrough; not represented by repository tests. |
| Acceptance | Confirm the delivered behavior against group scenarios. | Group walkthrough and sign-off remain to be recorded. |

## Current test coverage

| ID | Requirement | Input / action | Expected result | Latest evidence | Status |
| --- | --- | --- | --- | --- | --- |
| UT-01–05 | FR-07 | Valid, zero-length, reversed, adjacent, and overlapping date ranges. | Valid dates accepted; invalid dates rejected; adjacent stays do not overlap. | Automated reservation rule tests; phase runs on 26 Nov 2026. | Pass |
| UT-06 | FR-09 | ₦120,000 nightly rate for three nights. | ₦360,000 accommodation charge. | Automated billing test; phase run on 26 Nov 2026. | Pass |
| IT-00 | FR-01 | Empty database; create first staff account. | First account is stored with salted password verifier; duplicate username is rejected. | `StaffSetup_CreatesFirstAccountAndValidatesCredentials` passed 28 Nov 2026. | Pass |
| IT-00b | FR-01 | Verify correct and incorrect password against stored hash. | Correct succeeds; incorrect fails. | Password and staff setup tests passed 28 Nov 2026. | Pass |
| IT-01 | FR-11 | Initialize empty database twice. | Schema and 30 rooms created once; four categories present. | Automated seed/idempotency test; phase runs on 26 Nov 2026. | Pass |
| IT-02–04 | FR-05–08 | Search overlapping and adjacent stays; cancel a reservation. | Conflicting room omitted, adjacent stay allowed, cancelled stay released. | Automated availability and reservation lifecycle tests; phase runs on 26 Nov 2026. | Pass |
| IT-05 | FR-11 | Save guest and reservation, close/reopen repository. | Data remains after reopen. | `Repository_PreservesRecordsAfterReopen` passed 28 Nov 2026. | Pass |
| IT-06–07 | FR-03 | Add room; change category, rate, capacity, and operational status. | Values persist and are returned by repository. | Automated room persistence test; phase runs on 26 Nov 2026. | Pass |
| IT-08 | FR-03 | Add duplicate room number. | SQLite rejects duplicate number. | Automated duplicate constraint test. UI message not exercised. | Pass (data layer); UI pending |
| IT-09 | FR-03/05 | Filter visible room list by number, category, and status. | Only matching rows shown. | No UI automation or manual evidence recorded. | Pending manual check |
| IT-10–11 | FR-04 | Create, update, and search guest contact details. | Saved values returned by name and phone search. | Automated guest repository test; phase runs on 26 Nov 2026. | Pass |
| IT-12 | FR-06–08 | Create, update, and cancel reservations, including conflict. | Conflict rejected; update persists; cancellation releases dates. | Automated reservation lifecycle test; phase run on 26 Nov 2026. | Pass |
| IT-13 | FR-09 | Summarize a three-night reservation at ₦120,000/night. | Saved rate, night count, and ₦360,000 total returned. | Automated billing summary test; phase run on 26 Nov 2026. | Pass |
| IT-14 | FR-10/11 | Migrate prior schema, check in active booking, then check out. | Timestamps saved; room becomes occupied/unavailable, then released. | Automated lifecycle/migration test; phase run on 26 Nov 2026. | Pass |
| IT-15 | FR-02/05/10/11 | Create in-house, arriving, and upcoming stays; place a room under maintenance; reopen the database and rebuild dashboard summary. | Counts, availability, occupancy, arrivals, upcoming stays, and attention indicators reflect the saved records. | `DashboardSummary_RefreshesFromPersistedRecordsAfterRepositoryReopens` passed 28 Nov 2026. | Pass |
| ST-01 | FR-01/02 | Create staff account, sign in, open dashboard. | Authorized staff reaches dashboard. | No credentialed UI walkthrough recorded. | Pending manual check |
| ST-02 | FR-04–09 | Register a guest and complete a booking. | Reservation and accommodation summary match booking. | Repository modules tested separately; full UI flow not recorded. | Pending manual check |
| ST-03 | FR-10 | Check in and check out an eligible booking. | State and room availability change in order. | Repository lifecycle integration test passed 26 Nov 2026; UI walkthrough not recorded. | Pass (integration); UI pending |

## Execution log

### Phase 2 — 26 Nov 2026

`dotnet test RooseHotel.Tests/RooseHotel.Tests.csproj --no-restore --verbosity minimal --logger "console;verbosity=normal"` — **6 passed, 0 failed**. Covered 30-room idempotent seed, room create/update, duplicate room rejection, reservation date/charge rules, availability overlap and adjacent dates, and password hashing. Fixed SQLite connection-pool cleanup for temporary test databases.

### Phase 3 — 26 Nov 2026

Automated suite — **7 passed, 0 failed, 0 skipped**; guest create/update and search included. Application build — **0 warnings, 0 errors**. Guest UI walkthrough not recorded.

### Phase 4 — 26 Nov 2026

Automated suite — **8 passed, 0 failed, 0 skipped**; overlap rejection, adjacent dates, update, cancellation, and released-room availability included. Application build — **0 warnings, 0 errors**. Reservation UI walkthrough not recorded.

### Phase 5 — 26 Nov 2026

Automated suite — **9 passed, 0 failed, 0 skipped**; three nights at ₦120,000/night returned ₦360,000. Application build — **0 warnings, 0 errors**. Billing UI walkthrough not recorded.

### Phase 6 — 26 Nov 2026

Automated suite — **10 passed, 0 failed, 0 skipped**; migration, early check-in rejection, timestamps, occupancy, availability block, and release after checkout included. Application build — **0 warnings, 0 errors**. UI walkthrough not recorded.

### Release test rerun — 28 Nov 2026

Two additional tests were added for first-staff setup/credential verification and data persistence after reopening SQLite. The complete test assembly was run with `dotnet test RooseHotel.Tests/RooseHotel.Tests.csproj --no-restore --no-build --verbosity normal --logger "console;verbosity=normal"` — **12 passed, 0 failed** (.NET 9.0.9, VSTest 17.14.1). Both added cases passed. The full solution was then built with `dotnet build RooseHotel.sln --no-restore --verbosity minimal -p:BaseOutputPath=bin/release/ -p:UseSharedCompilation=false` — **0 warnings, 0 errors**.

System and acceptance checks still need an actual group walkthrough and sign-off. UI room filtering, live login, printing, and full visual workflow have not been manually recorded.

### Dashboard increment — 28 Nov 2026

Added an operations summary built from persisted rooms and reservations. The automated test saves in-house, arriving, and upcoming stays plus a maintenance status, clears SQLite connection pools, reopens the repository, and verifies the dashboard metrics. Release test command — **13 passed, 0 failed**. Release application build — `dotnet build RooseHotel.App/RooseHotel.App.csproj --configuration Release --no-restore --verbosity minimal -p:BaseOutputPath=bin/dashboard/ -p:UseSharedCompilation=false` — **0 warnings, 0 errors**. Returning to the dashboard and the Refresh button both reload current repository data; a manual visual walkthrough remains useful for final UI review.

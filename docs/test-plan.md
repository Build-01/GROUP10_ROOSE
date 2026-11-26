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
| UT-06 | FR-09 | Rate ₦120,000; three nights. | Charge is ₦360,000. | Pending execution |
| IT-01 | FR-11 | Initialize an empty database. | Schema and expected 30 seed rooms are created once. | Pending execution |
| IT-02 | FR-05 | Search dates conflicting with a confirmed reservation. | The reserved room is omitted. | Pending execution |
| IT-03 | FR-06/07 | Insert overlapping reservation for the same room. | Operation is rejected and no duplicate row is saved. | Pending execution |
| IT-04 | FR-08 | Cancel a reservation and search its dates. | The cancelled reservation no longer blocks the room. | Pending execution |
| IT-05 | FR-11 | Save records, close connection, reopen database. | Saved data is still present. | Pending execution |
| ST-01 | FR-01/02 | Sign in and open dashboard. | Authorized staff sees current summary. | Pending execution |
| ST-02 | FR-04–09 | Register a guest and complete a valid booking. | Reservation and charge summary are correct. | Pending execution |
| ST-03 | FR-10 | Check in then check out an eligible reservation. | State changes are recorded in order. | Pending execution |

## Execution log

No application test results are claimed yet. Record the build/runtime version, environment, actual result, pass/fail, defect ID, and retest result when each case is run. A test is not marked passed until the observed output is recorded.

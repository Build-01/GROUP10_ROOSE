# Software Requirements Specification

## 1. Purpose and scope

The ROOSE Hotel Room Reservation System is a Windows desktop application for authorized hotel reception staff. It supports the front-desk reservation lifecycle for one 30-room hotel using a local SQLite database. The first release focuses on staff access, room and guest records, date-based room reservations, accommodation charges, and check-in/check-out.

The first release excludes a public booking website, guest self-service accounts, online card payments, restaurant/POS, housekeeping, payroll, multiple hotel branches, and loyalty programs.

## 2. Users and operating assumptions

The primary actor is a hotel receptionist or other authorized staff member. Staff use the application on a Windows workstation. The database is stored locally and persists across application restarts. Dates represent hotel-local calendar dates; a stay occupies the interval from check-in up to, but not including, check-out.

Room category names, room assignments, nightly rates, staff accounts, and any required login policy must be confirmed by the group. The supplied mockup identifies rates for Standard, Deluxe, and Executive Suite rooms but does not specify the fourth category.

## 3. Functional requirements

| ID | Requirement | Acceptance condition |
| --- | --- | --- |
| FR-01 | The system shall allow authorized staff to enter the staff workspace. On first run, it shall allow creation of the initial staff account. | The first account requires a username, display name, and password; later sign-ins require valid credentials; invalid credentials are rejected with a clear message. Passwords are salted and hashed before storage. |
| FR-02 | The system shall display a dashboard with room and daily-operation summaries. | Summary values are derived from stored records and refresh after relevant changes. |
| FR-03 | The system shall store and display room number, category, nightly rate, capacity, and operational status. | Staff can find a room and see all five fields. |
| FR-04 | The system shall register guests and store searchable guest details. | A guest with required fields can be saved and retrieved by name or contact detail. |
| FR-05 | The system shall search rooms for a check-in and check-out date range. | Results exclude rooms under maintenance and rooms with overlapping active reservations. |
| FR-06 | The system shall create a reservation for a registered guest and eligible room. | A valid request is saved with a unique reservation ID and confirmed status. |
| FR-07 | The system shall reject invalid stay dates and overlapping reservations for the same room. | Check-out must be later than check-in; an overlapping active stay is rejected. |
| FR-08 | The system shall retrieve, modify, and cancel reservations. | Changes persist, and cancelled stays no longer block availability. |
| FR-09 | The system shall calculate accommodation charges. | Total equals nightly rate multiplied by the number of nights. |
| FR-10 | The system shall record check-in and check-out only for an eligible active reservation. | The reservation state and room occupancy reflect the recorded action. |
| FR-11 | The system shall preserve records across application restarts. | Saved rooms, guests, and reservations are still present after closing and reopening. |

## 4. Non-functional requirements

| ID | Requirement | Acceptance condition |
| --- | --- | --- |
| NFR-01 Usability | Common front-desk workflows use clearly named forms, tables, and actions. | A user can follow the task flow without needing to edit the database directly. |
| NFR-02 Reliability | Invalid input and conflicting reservations are handled without corrupting stored data. | Failed operations show an understandable error and leave records unchanged. |
| NFR-03 Data integrity | Related guest, room, and reservation records use database constraints and consistent status rules. | Orphaned reservations and invalid date ranges cannot be saved. |
| NFR-04 Persistence | Application records are stored locally using SQLite. | Data remains available after restart without a separate database server. |
| NFR-05 Maintainability | Presentation, business rules, and data access are separated. | UI event handlers do not contain SQL or define booking rules. |
| NFR-06 Compatibility | The application targets Windows desktop computers with the agreed .NET runtime. | The release package starts on the team's target Windows setup. |

## 5. Key scenarios

### SC-01 Find and reserve a room

Staff selects check-in and check-out dates. The system validates that check-out is later, returns rooms with an available operational status and no overlapping confirmed or checked-in reservation, then accepts a selected registered guest and room. Immediately before saving, the system repeats the availability check so two attempts cannot both reserve the same room for overlapping dates.

### SC-02 Guest departs and another guest arrives on the same date

A reservation ending on 30 November and another beginning on 30 November do not overlap. The checkout date is not an occupied night.

### SC-03 Cancel a reservation

Staff cancels a confirmed reservation. Its status changes to Cancelled and it no longer blocks availability for its former dates.

### SC-04 Check in a guest

Staff retrieves the reservation. The system permits check-in only when the reservation is confirmed and active, then records the arrival and updates the stay state.

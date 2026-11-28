# ROOSE Hotel — Staff Quick Guide

## First use

1. Start the app on Windows with .NET 9 installed.
2. On first launch, create the initial authorized staff account. Choose a private password and keep it with the staff member; no default password is shipped or printed in this guide.
3. Sign in to open the dashboard. Close the app when finished, especially on a shared workstation.

The local SQLite file is `%LOCALAPPDATA%\RooseHotel\roosehotel.db`. Treat this as local project/demo data; it is not a shared multi-computer deployment.

## Front-desk tasks

| Task | Navigation and steps |
| --- | --- |
| Review operations | Open **Dashboard** for total/in-service/available rooms, occupancy, today's arrivals and departures, in-house stays, the next seven days of arrivals, and rooms needing attention. The saved data reloads whenever Dashboard opens; select **Refresh** to reload it while the page is open. |
| Manage rooms | Open **Rooms** to search, filter, view details, add, or edit room details and operational status. |
| Register a guest | Open **Guests**, enter the guest's name, phone, and email, then register. Search to find or edit an existing guest. |
| Check availability | Open **Availability**, enter check-in and check-out dates and guest count, then search. Check-out is the exclusive end date. |
| Create a reservation | Open **Reservations**, select a registered guest, available room, and stay dates; review the estimate and confirm. The system rechecks availability before saving. |
| Review accommodation charge | Open **Billing**, select a reservation and review its saved nightly rate, number of nights, and total. Use print preview to print/save the summary. |
| Check in/out | Open **Check-in/Check-out**, find an eligible confirmed reservation, and record arrival. At departure, select the checked-in reservation and record checkout. The room shows **Occupied** after arrival and is released after checkout. |

## Suggested demonstration (about 5–7 minutes)

Use synthetic records only. Do not show a real guest's personal information or reveal the staff password in a recording.

1. Show first-run setup or sign in with a demo account created for the presentation; open the dashboard.
2. Search the room list and show category, rate, and room status.
3. Register a sample guest (for example, “Demo Guest”, `08000000000`, `demo@example.test`).
4. Search a future date range, select a room, and create a reservation.
5. Try an overlapping stay for that room and show that the system rejects it.
6. Open Billing and show a three-night Deluxe stay at ₦120,000/night totals ₦360,000.
7. On a date eligible for check-in, record arrival and show the room as Occupied/unavailable; then check out and show it released.

For a deterministic recording, seed dates around the actual demo date and confirm room availability before recording. A screenshot/mockup is design reference only; it does not substitute for a live application demonstration.

## Project assumptions to confirm

The supplied overview names 30 rooms and four categories but specifies only Standard (₦80,000), Deluxe (₦120,000), and Executive Suite (₦180,000). The current Family Suite category at ₦220,000 is a development assumption and must be confirmed by the group before presenting it as an agreed hotel rate.

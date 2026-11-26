# ROOSE Hotel Reservation System

Group 10's C# Windows Forms project for managing hotel rooms, guests, reservations, accommodation charges, and arrivals/departures.

## Current increment

The initial application foundation includes:

- `RooseHotel.Core`: room, guest, and reservation models, plus shared date-overlap and charge rules.
- `RooseHotel.Data`: SQLite schema setup, sample room seeding, salted staff-password storage, guest search/registration, date-based availability, and guarded reservation creation.
- `RooseHotel.App`: first-run staff account setup, sign-in, a Windows Forms dashboard, room list, and availability search.

The guest, reservation, billing, and check-in/check-out screens are planned for later increments. The first-run account is the initial local staff account; additional staff-management screens are not part of this increment.

## Architecture

```text
RooseHotel.App (Windows Forms)
        ↓
RooseHotel.Core (models and business rules)
        ↑
RooseHotel.Data (SQLite repository)
```

The app stores its local database at `%LOCALAPPDATA%\RooseHotel\roosehotel.db`.

The UI uses white backgrounds, fine gold borders, and coffee-brown active states; see [docs/visual-style.md](docs/visual-style.md).

## Build and run

Requirements: Windows, .NET 9 SDK, and access to NuGet for the `Microsoft.Data.Sqlite` package.

```powershell
dotnet restore RooseHotel.sln
dotnet run --project RooseHotel.App
```

## Seed-data assumptions to confirm

The overview says there are 30 rooms in four categories. The image shows Standard at ₦80,000, Deluxe at ₦120,000, and Executive Suite at ₦180,000, but does not identify the fourth category or its rate. The initial seed data uses Family Suite at ₦220,000 for development; confirm these room assignments and rates with the group before treating them as final hotel data.

## Assignment deliverables still to prepare

Requirements specification, UML and architecture diagrams, complete module workflows, test plan and results, screenshots, demo video, and final PDF report.

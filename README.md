# ROOSE Hotel Reservation System

Group 10's C# Windows Forms project for managing hotel rooms, guests, reservations, accommodation charges, and arrivals/departures.

## Current release

The application implements the planned front-desk workflow in incremental phases:

- Staff first-run setup and sign-in with salted password hashes.
- Dashboard and 30 seeded rooms across four development-assumption categories.
- Room management, guest registration/search/edit, and date-based availability.
- Reservation create, search, edit, cancellation, and overlap protection.
- Accommodation charge summaries with print preview.
- Check-in/check-out timestamps and room occupancy derived from checked-in reservations.

The initial release excludes public booking, card payments, POS, housekeeping, payroll, and multi-branch management. The Family Suite category and its ₦220,000 rate are development assumptions because the source overview did not define the fourth category; confirm them before operational use.

## Architecture

```text
RooseHotel.App (Windows Forms)
       ├── RooseHotel.Core (models and business rules)
       └── RooseHotel.Data (SQLite repository)
                  └── RooseHotel.Core
```

The app stores its local database at `%LOCALAPPDATA%\RooseHotel\roosehotel.db`.

The UI uses white backgrounds, fine gold borders, and coffee-brown active states; see [docs/visual-style.md](docs/visual-style.md).

## Build and run

Requirements: Windows, .NET 9 SDK, and access to NuGet for the `Microsoft.Data.Sqlite` package.

```powershell
dotnet restore RooseHotel.sln
dotnet run --project RooseHotel.App
dotnet test RooseHotel.sln
```

## Download for Windows

[Download the Windows x64 app ZIP](output/download/ROOSE_Hotel_Reservation_System_Windows_x64.zip). Extract the ZIP and run `RooseHotel.App.exe`. The package requires the **.NET 9 Desktop Runtime for Windows x64**. On first launch, create the initial staff account; no default credentials are provided. Local data is stored at `%LOCALAPPDATA%\RooseHotel\roosehotel.db`.

## Seed-data assumptions to confirm

The overview says there are 30 rooms in four categories. The image shows Standard at ₦80,000, Deluxe at ₦120,000, and Executive Suite at ₦180,000, but does not identify the fourth category or its rate. The initial seed data uses Family Suite at ₦220,000 for development; confirm these room assignments and rates with the group before treating them as final hotel data.

## Verification and hand-in

The incremental development log and test execution evidence are in `docs/development-plan.md` and `docs/test-plan.md`. The final report PDF is in `output/pdf/`. Before submission, the group must add matriculation numbers and a shareable demonstration video link, and complete a live staff login and front-desk walkthrough. See `docs/user-guide.md` for the demo flow.

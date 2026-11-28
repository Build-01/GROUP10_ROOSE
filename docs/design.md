# System Models and Software Design

## 1. Architecture

The application follows a three-layer architecture. Windows Forms handles display and user input. Core contains the entities and validation rules. Data implements persistence using ADO.NET and SQLite. Each layer has a narrow responsibility so UI changes do not require rewriting the database logic.

```mermaid
flowchart LR
    Staff[Receptionist / authorized staff]
    UI[RooseHotel.App<br/>Windows Forms]
    Core[RooseHotel.Core<br/>Models and business rules]
    Data[RooseHotel.Data<br/>Repository and SQL]
    DB[(SQLite database<br/>Rooms, Guests, Reservations, Staff)]
    Staff --> UI
    UI --> Core
    UI --> Data
    Data --> Core
    Data --> DB
```

## 2. Use-case model

```mermaid
flowchart LR
    Staff[Reception staff]
    subgraph System[ROOSE Hotel Reservation System]
      Login((Sign in))
      Dashboard((View dashboard))
      Rooms((Manage rooms))
      Guests((Register and search guests))
      Availability((Search availability))
      Reservations((Manage reservations))
      Billing((View accommodation charges))
      Stay((Check in / check out))
    end
    Staff --> Login
    Staff --> Dashboard
    Staff --> Rooms
    Staff --> Guests
    Staff --> Availability
    Staff --> Reservations
    Staff --> Billing
    Staff --> Stay
    Reservations -. requires .-> Guests
    Reservations -. selects .-> Availability
    Stay -. retrieves .-> Reservations
    Billing -. summarizes .-> Reservations
```

## 3. Core class model

```mermaid
classDiagram
    class Room {
      +int Id
      +string Number
      +RoomCategory Category
      +decimal NightlyRate
      +int Capacity
      +RoomOperationalStatus OperationalStatus
      +bool IsOccupied
      +string DisplayStatus
    }
    class Guest {
      +int Id
      +string FullName
      +string Phone
      +string Email
    }
    class StaffAccount {
      +int Id
      +string Username
      +string DisplayName
      +string PasswordSalt
      +string PasswordHash
    }
    class Reservation {
      +int Id
      +int GuestId
      +int RoomId
      +DateOnly CheckIn
      +DateOnly CheckOut
      +ReservationStatus Status
      +int Nights
      +DateTimeOffset ActualCheckIn
      +DateTimeOffset ActualCheckOut
    }
    class ReservationSummary {
      +int Id
      +string GuestName
      +string RoomNumber
      +DateOnly CheckIn
      +DateOnly CheckOut
      +ReservationStatus Status
    }
    class DashboardSummary {
      +int TotalRooms
      +int InServiceRooms
      +int AvailableToday
      +int OccupiedRooms
      +decimal OccupancyPercent
      +int ArrivalsToday
      +int DeparturesToday
      +int GuestsInHouse
      +int UpcomingArrivals
      +int RoomsNeedingAttention
      +Build(rooms, reservations, today) DashboardSummary
    }
    class ReservationRules {
      +HasValidDates(checkIn, checkOut) bool
      +Overlaps(start, end, otherStart, otherEnd) bool
      +BlocksAvailability(status) bool
      +CalculateCharge(rate, checkIn, checkOut) decimal
    }
    class IHotelRepository {
      +InitializeAsync()
      +GetStaffCountAsync()
      +FindStaffAsync(username)
      +CreateStaffAsync(username, displayName, salt, hash)
      +GetRoomsAsync()
      +FindAvailableRoomsAsync(checkIn, checkOut)
      +SearchGuestsAsync(query)
      +CreateGuestAsync(guest)
      +CreateReservationAsync(request)
      +GetReservationsAsync(query)
      +UpdateReservationAsync(request)
      +CancelReservationAsync(id)
      +CheckInReservationAsync(id, time)
      +CheckOutReservationAsync(id, time)
    }
    Guest "1" --> "0..*" Reservation : books
    Room "1" --> "0..*" Reservation : assigned to
    Reservation --> ReservationSummary : read as
    DashboardSummary ..> Room : aggregates
    DashboardSummary ..> ReservationSummary : aggregates
    Reservation ..> ReservationRules : validated by
    IHotelRepository ..> Room : stores
    IHotelRepository ..> Guest : stores
    IHotelRepository ..> Reservation : stores
```

## 4. Reservation sequence

```mermaid
sequenceDiagram
    actor Staff
    participant UI as Windows Forms
    participant Rules as Core rules
    participant Repo as SQLite repository
    participant DB as SQLite
    Staff->>UI: Enter guest and stay dates
    UI->>Rules: Validate check-out after check-in
    Rules-->>UI: Valid date range
    UI->>Repo: Find rooms for date range
    Repo->>DB: Select operational rooms without overlap
    DB-->>Repo: Available room rows
    Repo-->>UI: Available rooms
    Staff->>UI: Confirm selected guest and room
    UI->>Repo: Create reservation request
    Repo->>DB: Begin transaction and recheck overlap
    alt Room still available and guest exists
      Repo->>DB: Insert confirmed reservation
      DB-->>Repo: Reservation ID
      Repo-->>UI: Success
      UI-->>Staff: Show confirmation
    else Room unavailable or data invalid
      Repo-->>UI: Validation error
      UI-->>Staff: Explain why booking was rejected
    end
```

## 5. Booking activity

```mermaid
flowchart TD
    A([Start]) --> B[Choose check-in and check-out]
    B --> C{Check-out after check-in?}
    C -- No --> D[Show date validation message]
    D --> B
    C -- Yes --> E[Search operational rooms]
    E --> F{Any room available for full date range?}
    F -- No --> G[Show no rooms available]
    G --> Z([End])
    F -- Yes --> H[Select registered guest and room]
    H --> I[Recheck overlap inside database transaction]
    I --> J{Still available?}
    J -- No --> K[Reject and refresh search]
    K --> E
    J -- Yes --> L[Save confirmed reservation]
    L --> M[Calculate nights and accommodation charge]
    M --> Z
```

## 6. Data design notes

`Rooms` stores room identity, category, nightly rate, capacity, and operational status. The UI derives the `Occupied` display state from a reservation in `CheckedIn`; occupancy remains separate from operational state. `Guests` stores contact records. `Reservations` references one guest and one room, stores ISO `yyyy-MM-dd` booked dates, a status, and nullable actual check-in/check-out timestamps. `Staff` stores a unique username, display name, and password salt/hash; it never stores the entered password. Foreign keys are enabled. The date overlap rule is `existing.CheckIn < requested.CheckOut AND requested.CheckIn < existing.CheckOut`; adjacent stays therefore do not conflict.

`DashboardSummary` is a read-time aggregate of persisted rooms and reservations. It is rebuilt whenever staff open or refresh the dashboard; there is no separate cached dashboard record to become stale.

The first run creates the initial staff account. Passwords are hashed with PBKDF2-SHA256 and a random per-account salt. Room assignments and the fourth category's name/rate remain group-confirmation items.

## 7. Check-in and check-out activity

```mermaid
flowchart TD
    A([Start]) --> B[Find a confirmed reservation]
    B --> C{Is today within the booked stay?}
    C -- No --> D[Reject check-in and explain eligibility]
    C -- Yes --> E[Record actual arrival timestamp]
    E --> F[Set reservation to CheckedIn]
    F --> G[Show room as Occupied and exclude it from availability]
    G --> H[Staff checks out the guest]
    H --> I{Is reservation CheckedIn?}
    I -- No --> J[Reject invalid state]
    I -- Yes --> K[Record departure timestamp]
    K --> L[Set reservation to CheckedOut]
    L --> M[Release room occupancy]
    M --> N([End])
```

## 8. Dashboard refresh sequence

```mermaid
sequenceDiagram
    actor Staff
    participant UI as Dashboard
    participant Repo as SQLite repository
    participant Core as DashboardSummary
    participant DB as SQLite
    Staff->>UI: Open dashboard or select Refresh
    UI->>Repo: Get rooms and reservations
    Repo->>DB: Read current saved records
    DB-->>Repo: Current room and reservation rows
    Repo-->>UI: Room and reservation lists
    UI->>Core: Build summary for today's date
    Core-->>UI: Counts and arrival/departure lists
    UI-->>Staff: Render current operations overview
```

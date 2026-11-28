using Microsoft.Data.Sqlite;
using RooseHotel.Core;
using RooseHotel.Data;

namespace RooseHotel.Tests;

public sealed class HotelTests
{
    [Fact]
    public void ReservationRules_UseCheckoutAsExclusiveEnd()
    {
        var start = new DateOnly(2026, 11, 27);
        var end = new DateOnly(2026, 11, 30);

        Assert.True(ReservationRules.HasValidDates(start, end));
        Assert.False(ReservationRules.HasValidDates(start, start));
        Assert.False(ReservationRules.Overlaps(start, end, end, end.AddDays(2)));
        Assert.True(ReservationRules.Overlaps(start, end, end.AddDays(-1), end.AddDays(1)));
        Assert.Equal(360_000m, ReservationRules.CalculateCharge(120_000m, start, end));
    }

    [Fact]
    public void PasswordSecurity_StoresVerifierAndChecksPassword()
    {
        var (salt, hash) = PasswordSecurity.CreateHash("RooseTestPass2026");

        Assert.NotEqual("RooseTestPass2026", hash);
        Assert.True(PasswordSecurity.Verify("RooseTestPass2026", salt, hash));
        Assert.False(PasswordSecurity.Verify("wrong-password", salt, hash));
    }

    [Fact]
    public async Task StaffSetup_CreatesFirstAccountAndValidatesCredentials()
    {
        var (repository, folder) = CreateRepository();
        try
        {
            await repository.InitializeAsync();
            Assert.Equal(0, await repository.GetStaffCountAsync());
            const string testOnlyPassword = "RooseTestPass2026";
            var (salt, hash) = PasswordSecurity.CreateHash(testOnlyPassword);
            await repository.CreateStaffAsync("frontdesk", "Demo Staff", salt, hash);

            var staff = await repository.FindStaffAsync("FRONTDESK");
            Assert.Equal(1, await repository.GetStaffCountAsync());
            Assert.NotNull(staff);
            Assert.True(PasswordSecurity.Verify(testOnlyPassword, staff!.PasswordSalt, staff.PasswordHash));
            Assert.False(PasswordSecurity.Verify("wrong-password", staff.PasswordSalt, staff.PasswordHash));
            var duplicate = await Assert.ThrowsAsync<SqliteException>(() => repository.CreateStaffAsync("frontdesk", "Other Staff", salt, hash));
            Assert.Equal(19, duplicate.SqliteErrorCode);
        }
        finally { DeleteTestFolder(folder); }
    }

    [Fact]
    public async Task Repository_PreservesRecordsAfterReopen()
    {
        var (repository, folder) = CreateRepository();
        try
        {
            await repository.InitializeAsync();
            var guestId = await repository.CreateGuestAsync(new Guest(0, "Persistent Guest", "08000000000", "persist@example.test"));
            var room = (await repository.GetRoomsAsync()).First();
            await repository.CreateReservationAsync(new ReservationRequest(guestId, room.Id,
                new DateOnly(2026, 12, 10), new DateOnly(2026, 12, 11)));
            SqliteConnection.ClearAllPools();

            var reopened = new SqliteHotelRepository(Path.Combine(folder, "test.db"));
            await reopened.InitializeAsync();
            Assert.Equal(30, (await reopened.GetRoomsAsync()).Count);
            Assert.Single(await reopened.SearchGuestsAsync("Persistent Guest"));
            Assert.Single(await reopened.GetReservationsAsync("Persistent Guest"));
        }
        finally { DeleteTestFolder(folder); }
    }

    [Fact]
    public async Task InitializeAsync_SeedsThirtyRoomsOnlyOnce()
    {
        var (repository, folder) = CreateRepository();
        try
        {
            await repository.InitializeAsync();
            await repository.InitializeAsync();

            var rooms = await repository.GetRoomsAsync();
            Assert.Equal(30, rooms.Count);
            Assert.Equal(4, rooms.Select(room => room.Category).Distinct().Count());
        }
        finally { DeleteTestFolder(folder); }
    }

    [Fact]
    public async Task AddAndUpdateRoom_PersistsRoomFields()
    {
        var (repository, folder) = CreateRepository();
        try
        {
            await repository.InitializeAsync();
            var id = await repository.AddRoomAsync(new Room(0, "401", RoomCategory.Deluxe, 145_000m, 2, RoomOperationalStatus.Available));

            var added = (await repository.GetRoomsAsync()).Single(room => room.Id == id);
            Assert.Equal("401", added.Number);
            Assert.Equal(145_000m, added.NightlyRate);
            Assert.Equal(2, added.Capacity);

            await repository.UpdateRoomAsync(added with { Category = RoomCategory.ExecutiveSuite, NightlyRate = 180_000m, Capacity = 3, OperationalStatus = RoomOperationalStatus.Maintenance });
            var updated = (await repository.GetRoomsAsync()).Single(room => room.Id == id);

            Assert.Equal(RoomCategory.ExecutiveSuite, updated.Category);
            Assert.Equal(180_000m, updated.NightlyRate);
            Assert.Equal(3, updated.Capacity);
            Assert.Equal(RoomOperationalStatus.Maintenance, updated.OperationalStatus);
        }
        finally { DeleteTestFolder(folder); }
    }

    [Fact]
    public async Task AddRoom_RejectsDuplicateRoomNumber()
    {
        var (repository, folder) = CreateRepository();
        try
        {
            await repository.InitializeAsync();
            var room = new Room(0, "401", RoomCategory.Standard, 80_000m, 1, RoomOperationalStatus.Available);
            await repository.AddRoomAsync(room);

            var error = await Assert.ThrowsAsync<SqliteException>(() => repository.AddRoomAsync(room));
            Assert.Equal(19, error.SqliteErrorCode);
        }
        finally { DeleteTestFolder(folder); }
    }

    [Fact]
    public async Task Guest_CreateUpdateAndSearch_PersistsContactDetails()
    {
        var (repository, folder) = CreateRepository();
        try
        {
            await repository.InitializeAsync();
            var id = await repository.CreateGuestAsync(new Guest(0, "Ada Guest", "08000000000", "ada@example.test"));
            await repository.UpdateGuestAsync(new Guest(id, "Ada Updated", "08000000001", "updated@example.test"));

            var byName = await repository.SearchGuestsAsync("Updated");
            var byPhone = await repository.SearchGuestsAsync("8000000001");
            Assert.Single(byName);
            Assert.Equal("updated@example.test", byName[0].Email);
            Assert.Contains(byPhone, guest => guest.Id == id);
        }
        finally { DeleteTestFolder(folder); }
    }

    [Fact]
    public async Task AvailabilitySearch_RejectsOverlapsAndAllowsAdjacentStay()
    {
        var (repository, folder) = CreateRepository();
        try
        {
            await repository.InitializeAsync();
            var room = (await repository.GetRoomsAsync()).First();
            var guestId = await repository.CreateGuestAsync(new Guest(0, "Test Guest", "08000000000", "guest@example.test"));
            await repository.CreateReservationAsync(new ReservationRequest(guestId, room.Id,
                new DateOnly(2026, 11, 27), new DateOnly(2026, 11, 30)));

            var overlapping = await repository.FindAvailableRoomsAsync(new DateOnly(2026, 11, 29), new DateOnly(2026, 12, 1));
            var adjacent = await repository.FindAvailableRoomsAsync(new DateOnly(2026, 11, 30), new DateOnly(2026, 12, 2));

            Assert.DoesNotContain(overlapping, candidate => candidate.Id == room.Id);
            Assert.Contains(adjacent, candidate => candidate.Id == room.Id);
        }
        finally { DeleteTestFolder(folder); }
    }

    [Fact]
    public async Task Reservation_CreateUpdateCancel_EnforcesConflictsAndFreesRoom()
    {
        var (repository, folder) = CreateRepository();
        try
        {
            await repository.InitializeAsync();
            var rooms = await repository.GetRoomsAsync();
            var guestId = await repository.CreateGuestAsync(new Guest(0, "Reservation Guest", "08000000000", "book@example.test"));
            var firstRequest = new ReservationRequest(guestId, rooms[0].Id, new DateOnly(2026, 12, 10), new DateOnly(2026, 12, 12));
            var id = await repository.CreateReservationAsync(firstRequest);

            await Assert.ThrowsAsync<InvalidOperationException>(() => repository.CreateReservationAsync(
                new ReservationRequest(guestId, rooms[0].Id, new DateOnly(2026, 12, 11), new DateOnly(2026, 12, 13))));
            var adjacentId = await repository.CreateReservationAsync(
                new ReservationRequest(guestId, rooms[0].Id, new DateOnly(2026, 12, 12), new DateOnly(2026, 12, 14)));
            await repository.UpdateReservationAsync(new ReservationUpdateRequest(id, guestId, rooms[1].Id,
                new DateOnly(2026, 12, 10), new DateOnly(2026, 12, 12)));
            await repository.CancelReservationAsync(id);

            var reservations = await repository.GetReservationsAsync("Reservation Guest");
            Assert.Equal(ReservationStatus.Cancelled, reservations.Single(r => r.Id == id).Status);
            Assert.Equal(rooms[1].Number, reservations.Single(r => r.Id == id).RoomNumber);
            Assert.Equal(ReservationStatus.Confirmed, reservations.Single(r => r.Id == adjacentId).Status);
            var available = await repository.FindAvailableRoomsAsync(new DateOnly(2026, 12, 10), new DateOnly(2026, 12, 12));
            Assert.Contains(available, room => room.Id == rooms[0].Id);
        }
        finally { DeleteTestFolder(folder); }
    }

    [Fact]
    public async Task BillingSummary_UsesSavedRoomRateAndReservationNights()
    {
        var (repository, folder) = CreateRepository();
        try
        {
            await repository.InitializeAsync();
            var room = (await repository.GetRoomsAsync()).First(item => item.NightlyRate == 120_000m);
            var guestId = await repository.CreateGuestAsync(new Guest(0, "Billing Guest", "08000000000", "billing@example.test"));
            var id = await repository.CreateReservationAsync(new ReservationRequest(guestId, room.Id,
                new DateOnly(2026, 12, 10), new DateOnly(2026, 12, 13)));

            var summary = (await repository.GetReservationsAsync("Billing Guest")).Single();
            Assert.Equal(id, summary.Id);
            Assert.Equal(3, summary.Nights);
            Assert.Equal(120_000m, summary.NightlyRate);
            Assert.Equal(360_000m, ReservationRules.CalculateCharge(summary.NightlyRate, summary.CheckIn, summary.CheckOut));
        }
        finally { DeleteTestFolder(folder); }
    }

    [Fact]
    public async Task CheckInAndOut_UpdatesRoomAvailabilityAndMigratesExistingDatabase()
    {
        var (repository, folder) = CreateRepository();
        try
        {
            var databasePath = Path.Combine(folder, "test.db");
            await using (var connection = new SqliteConnection($"Data Source={databasePath}"))
            {
                await connection.OpenAsync();
                var legacySchema = connection.CreateCommand();
                legacySchema.CommandText = "CREATE TABLE Reservations (Id INTEGER PRIMARY KEY AUTOINCREMENT, GuestId INTEGER NOT NULL, RoomId INTEGER NOT NULL, CheckIn TEXT NOT NULL, CheckOut TEXT NOT NULL, Status TEXT NOT NULL DEFAULT 'Confirmed', CHECK (CheckOut > CheckIn));";
                await legacySchema.ExecuteNonQueryAsync();
            }

            await repository.InitializeAsync();
            var room = (await repository.GetRoomsAsync()).First();
            var guestId = await repository.CreateGuestAsync(new Guest(0, "Arrival Guest", "08000000000", "arrival@example.test"));
            var today = DateOnly.FromDateTime(DateTime.Now);
            var checkInAt = DateTimeOffset.Now;
            var reservationId = await repository.CreateReservationAsync(new ReservationRequest(guestId, room.Id, today, today.AddDays(2)));
            await Assert.ThrowsAsync<InvalidOperationException>(() => repository.CheckInReservationAsync(reservationId, checkInAt.AddDays(-1)));
            await repository.CheckInReservationAsync(reservationId, checkInAt);

            var occupiedRoom = (await repository.GetRoomsAsync()).Single(item => item.Id == room.Id);
            Assert.True(occupiedRoom.IsOccupied);
            Assert.Equal("Occupied", occupiedRoom.DisplayStatus);
            Assert.DoesNotContain(await repository.FindAvailableRoomsAsync(today, today.AddDays(1)), item => item.Id == room.Id);
            var checkedIn = (await repository.GetReservationsAsync("Arrival Guest")).Single();
            Assert.Equal(ReservationStatus.CheckedIn, checkedIn.Status);
            Assert.NotNull(checkedIn.ActualCheckIn);

            await repository.CheckOutReservationAsync(reservationId, checkInAt.AddMinutes(30));
            var availableRoom = (await repository.GetRoomsAsync()).Single(item => item.Id == room.Id);
            Assert.False(availableRoom.IsOccupied);
            Assert.Equal("Available", availableRoom.DisplayStatus);
            Assert.Contains(await repository.FindAvailableRoomsAsync(today, today.AddDays(1)), item => item.Id == room.Id);
            var checkedOut = (await repository.GetReservationsAsync("Arrival Guest")).Single();
            Assert.Equal(ReservationStatus.CheckedOut, checkedOut.Status);
            Assert.NotNull(checkedOut.ActualCheckOut);
        }
        finally { DeleteTestFolder(folder); }
    }

    private static (SqliteHotelRepository Repository, string Folder) CreateRepository()
    {
        var folder = Path.Combine(Path.GetTempPath(), $"RooseHotel.Tests.{Guid.NewGuid():N}");
        Directory.CreateDirectory(folder);
        return (new SqliteHotelRepository(Path.Combine(folder, "test.db")), folder);
    }

    private static void DeleteTestFolder(string folder)
    {
        // Microsoft.Data.Sqlite pools connections by connection string. Clear the
        // pool before deleting the temporary database on Windows.
        SqliteConnection.ClearAllPools();
        if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
    }
}

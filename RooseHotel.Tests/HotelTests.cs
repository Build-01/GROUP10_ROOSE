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

    private static (SqliteHotelRepository Repository, string Folder) CreateRepository()
    {
        var folder = Path.Combine(Path.GetTempPath(), $"RooseHotel.Tests.{Guid.NewGuid():N}");
        Directory.CreateDirectory(folder);
        return (new SqliteHotelRepository(Path.Combine(folder, "test.db")), folder);
    }

    private static void DeleteTestFolder(string folder)
    {
        if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
    }
}

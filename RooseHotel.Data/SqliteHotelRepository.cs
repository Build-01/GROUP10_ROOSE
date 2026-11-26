using Microsoft.Data.Sqlite;
using RooseHotel.Core;

namespace RooseHotel.Data;

public sealed class SqliteHotelRepository(string databasePath) : IHotelRepository
{
    private readonly string _connectionString = new SqliteConnectionStringBuilder
    {
        DataSource = databasePath,
        ForeignKeys = true
    }.ToString();

    private SqliteConnection OpenConnection() => new(_connectionString);

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var directory = Path.GetDirectoryName(Path.GetFullPath(new SqliteConnectionStringBuilder(_connectionString).DataSource));
        if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);

        await using var connection = OpenConnection();
        await connection.OpenAsync(cancellationToken);
        var command = connection.CreateCommand();
        command.CommandText = """
            PRAGMA foreign_keys = ON;
            CREATE TABLE IF NOT EXISTS Rooms (
                Id INTEGER PRIMARY KEY,
                Number TEXT NOT NULL UNIQUE,
                Category TEXT NOT NULL,
                NightlyRate INTEGER NOT NULL CHECK (NightlyRate >= 0),
                Capacity INTEGER NOT NULL CHECK (Capacity > 0),
                OperationalStatus TEXT NOT NULL DEFAULT 'Available'
            );
            CREATE TABLE IF NOT EXISTS Guests (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                FullName TEXT NOT NULL,
                Phone TEXT NOT NULL,
                Email TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS Staff (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Username TEXT NOT NULL UNIQUE COLLATE NOCASE,
                DisplayName TEXT NOT NULL,
                PasswordSalt TEXT NOT NULL,
                PasswordHash TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS Reservations (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                GuestId INTEGER NOT NULL REFERENCES Guests(Id),
                RoomId INTEGER NOT NULL REFERENCES Rooms(Id),
                CheckIn TEXT NOT NULL,
                CheckOut TEXT NOT NULL,
                Status TEXT NOT NULL DEFAULT 'Confirmed',
                CHECK (CheckOut > CheckIn)
            );
            CREATE INDEX IF NOT EXISTS IX_Reservations_Room_Dates
                ON Reservations(RoomId, CheckIn, CheckOut, Status);
            """;
        await command.ExecuteNonQueryAsync(cancellationToken);

        var countCommand = connection.CreateCommand();
        countCommand.CommandText = "SELECT COUNT(*) FROM Rooms;";
        if (Convert.ToInt64(await countCommand.ExecuteScalarAsync(cancellationToken)) == 0)
            await SeedRoomsAsync(connection, cancellationToken);
    }

    public async Task<int> GetStaffCountAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = OpenConnection();
        await connection.OpenAsync(cancellationToken);
        var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Staff;";
        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
    }

    public async Task<StaffAccount?> FindStaffAsync(string username, CancellationToken cancellationToken = default)
    {
        await using var connection = OpenConnection();
        await connection.OpenAsync(cancellationToken);
        var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Username, DisplayName, PasswordSalt, PasswordHash FROM Staff WHERE Username = $username COLLATE NOCASE;";
        command.Parameters.AddWithValue("$username", username.Trim());
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return null;
        return new StaffAccount(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4));
    }

    public async Task CreateStaffAsync(string username, string displayName, string salt, string hash, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Username and display name are required.");
        await using var connection = OpenConnection();
        await connection.OpenAsync(cancellationToken);
        var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO Staff(Username, DisplayName, PasswordSalt, PasswordHash) VALUES($username, $displayName, $salt, $hash);";
        command.Parameters.AddWithValue("$username", username.Trim());
        command.Parameters.AddWithValue("$displayName", displayName.Trim());
        command.Parameters.AddWithValue("$salt", salt);
        command.Parameters.AddWithValue("$hash", hash);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Room>> GetRoomsAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = OpenConnection();
        await connection.OpenAsync(cancellationToken);
        var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Number, Category, NightlyRate, Capacity, OperationalStatus FROM Rooms ORDER BY Number;";
        var rooms = new List<Room>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken)) rooms.Add(ReadRoom(reader));
        return rooms;
    }

    public async Task<int> AddRoomAsync(Room room, CancellationToken cancellationToken = default)
    {
        ValidateRoom(room);
        await using var connection = OpenConnection();
        await connection.OpenAsync(cancellationToken);
        var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO Rooms(Number, Category, NightlyRate, Capacity, OperationalStatus) VALUES($number, $category, $rate, $capacity, $status); SELECT last_insert_rowid();";
        AddRoomParameters(command, room);
        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
    }

    public async Task UpdateRoomAsync(Room room, CancellationToken cancellationToken = default)
    {
        ValidateRoom(room);
        if (room.Id <= 0) throw new ArgumentOutOfRangeException(nameof(room), "A saved room ID is required for updates.");
        await using var connection = OpenConnection();
        await connection.OpenAsync(cancellationToken);
        var command = connection.CreateCommand();
        command.CommandText = "UPDATE Rooms SET Number=$number, Category=$category, NightlyRate=$rate, Capacity=$capacity, OperationalStatus=$status WHERE Id=$id;";
        AddRoomParameters(command, room);
        command.Parameters.AddWithValue("$id", room.Id);
        if (await command.ExecuteNonQueryAsync(cancellationToken) != 1)
            throw new KeyNotFoundException($"Room {room.Number} was not found.");
    }

    private static void ValidateRoom(Room room)
    {
        if (string.IsNullOrWhiteSpace(room.Number)) throw new ArgumentException("Room number is required.");
        if (room.NightlyRate < 0) throw new ArgumentOutOfRangeException(nameof(room), "Nightly rate cannot be negative.");
        if (room.Capacity < 1) throw new ArgumentOutOfRangeException(nameof(room), "Room capacity must be at least one.");
        if (!Enum.IsDefined(room.Category)) throw new ArgumentOutOfRangeException(nameof(room), "Choose a valid room category.");
        if (!Enum.IsDefined(room.OperationalStatus)) throw new ArgumentOutOfRangeException(nameof(room), "Choose a valid room status.");
    }

    private static void AddRoomParameters(SqliteCommand command, Room room)
    {
        command.Parameters.AddWithValue("$number", room.Number.Trim());
        command.Parameters.AddWithValue("$category", room.Category.ToString());
        command.Parameters.AddWithValue("$rate", room.NightlyRate);
        command.Parameters.AddWithValue("$capacity", room.Capacity);
        command.Parameters.AddWithValue("$status", room.OperationalStatus.ToString());
    }

    public async Task<IReadOnlyList<Room>> FindAvailableRoomsAsync(DateOnly checkIn, DateOnly checkOut,
        CancellationToken cancellationToken = default)
    {
        if (!ReservationRules.HasValidDates(checkIn, checkOut))
            throw new ArgumentException("Check-out must be later than check-in.");

        await using var connection = OpenConnection();
        await connection.OpenAsync(cancellationToken);
        var command = connection.CreateCommand();
        command.CommandText = """
            SELECT r.Id, r.Number, r.Category, r.NightlyRate, r.Capacity, r.OperationalStatus
            FROM Rooms r
            WHERE r.OperationalStatus = 'Available'
              AND NOT EXISTS (
                SELECT 1 FROM Reservations b
                WHERE b.RoomId = r.Id AND b.Status IN ('Confirmed', 'CheckedIn')
                  AND b.CheckIn < $checkOut AND $checkIn < b.CheckOut
              )
            ORDER BY r.Number;
            """;
        command.Parameters.AddWithValue("$checkIn", checkIn.ToString("yyyy-MM-dd"));
        command.Parameters.AddWithValue("$checkOut", checkOut.ToString("yyyy-MM-dd"));
        var rooms = new List<Room>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken)) rooms.Add(ReadRoom(reader));
        return rooms;
    }

    public async Task<IReadOnlyList<Guest>> SearchGuestsAsync(string query, CancellationToken cancellationToken = default)
    {
        await using var connection = OpenConnection();
        await connection.OpenAsync(cancellationToken);
        var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, FullName, Phone, Email FROM Guests WHERE FullName LIKE $query OR Phone LIKE $query OR Email LIKE $query ORDER BY FullName LIMIT 100;";
        command.Parameters.AddWithValue("$query", $"%{query.Trim()}%");
        var guests = new List<Guest>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            guests.Add(new Guest(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3)));
        return guests;
    }

    public async Task<int> CreateGuestAsync(Guest guest, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(guest.FullName) || string.IsNullOrWhiteSpace(guest.Phone))
            throw new ArgumentException("Guest name and phone are required.");
        await using var connection = OpenConnection();
        await connection.OpenAsync(cancellationToken);
        var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO Guests(FullName, Phone, Email) VALUES($name, $phone, $email); SELECT last_insert_rowid();";
        command.Parameters.AddWithValue("$name", guest.FullName.Trim());
        command.Parameters.AddWithValue("$phone", guest.Phone.Trim());
        command.Parameters.AddWithValue("$email", guest.Email.Trim());
        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
    }

    public async Task UpdateGuestAsync(Guest guest, CancellationToken cancellationToken = default)
    {
        if (guest.Id <= 0 || string.IsNullOrWhiteSpace(guest.FullName) || string.IsNullOrWhiteSpace(guest.Phone))
            throw new ArgumentException("A saved guest, name, and phone are required.");
        await using var connection = OpenConnection();
        await connection.OpenAsync(cancellationToken);
        var command = connection.CreateCommand();
        command.CommandText = "UPDATE Guests SET FullName = $name, Phone = $phone, Email = $email WHERE Id = $id;";
        command.Parameters.AddWithValue("$id", guest.Id);
        command.Parameters.AddWithValue("$name", guest.FullName.Trim());
        command.Parameters.AddWithValue("$phone", guest.Phone.Trim());
        command.Parameters.AddWithValue("$email", guest.Email.Trim());
        if (await command.ExecuteNonQueryAsync(cancellationToken) == 0)
            throw new InvalidOperationException("The guest record no longer exists.");
    }

    public async Task<int> CreateReservationAsync(ReservationRequest request, CancellationToken cancellationToken = default)
    {
        if (!ReservationRules.HasValidDates(request.CheckIn, request.CheckOut))
            throw new ArgumentException("Check-out must be later than check-in.");

        await using var connection = OpenConnection();
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        var check = connection.CreateCommand();
        check.Transaction = (SqliteTransaction)transaction;
        check.CommandText = """
            SELECT COUNT(*) FROM Rooms r
            WHERE r.Id = $roomId AND r.OperationalStatus = 'Available'
              AND EXISTS (SELECT 1 FROM Guests g WHERE g.Id = $guestId)
              AND NOT EXISTS (
                SELECT 1 FROM Reservations b WHERE b.RoomId = r.Id
                  AND b.Status IN ('Confirmed', 'CheckedIn')
                  AND b.CheckIn < $checkOut AND $checkIn < b.CheckOut
              );
            """;
        check.Parameters.AddWithValue("$roomId", request.RoomId);
        check.Parameters.AddWithValue("$guestId", request.GuestId);
        check.Parameters.AddWithValue("$checkIn", request.CheckIn.ToString("yyyy-MM-dd"));
        check.Parameters.AddWithValue("$checkOut", request.CheckOut.ToString("yyyy-MM-dd"));
        if (Convert.ToInt64(await check.ExecuteScalarAsync(cancellationToken)) != 1)
            throw new InvalidOperationException("The guest or room is unavailable, or the room is already booked for those dates.");

        var insert = connection.CreateCommand();
        insert.Transaction = (SqliteTransaction)transaction;
        insert.CommandText = "INSERT INTO Reservations(GuestId, RoomId, CheckIn, CheckOut, Status) VALUES($guestId, $roomId, $checkIn, $checkOut, 'Confirmed'); SELECT last_insert_rowid();";
        insert.Parameters.AddWithValue("$guestId", request.GuestId);
        insert.Parameters.AddWithValue("$roomId", request.RoomId);
        insert.Parameters.AddWithValue("$checkIn", request.CheckIn.ToString("yyyy-MM-dd"));
        insert.Parameters.AddWithValue("$checkOut", request.CheckOut.ToString("yyyy-MM-dd"));
        var id = Convert.ToInt32(await insert.ExecuteScalarAsync(cancellationToken));
        await transaction.CommitAsync(cancellationToken);
        return id;
    }

    private static Room ReadRoom(SqliteDataReader reader) => new(reader.GetInt32(0), reader.GetString(1),
        Enum.Parse<RoomCategory>(reader.GetString(2)), reader.GetDecimal(3), reader.GetInt32(4),
        Enum.Parse<RoomOperationalStatus>(reader.GetString(5)));

    private static async Task SeedRoomsAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        var command = connection.CreateCommand();
        command.Transaction = (SqliteTransaction)transaction;
        command.CommandText = "INSERT INTO Rooms(Number, Category, NightlyRate, Capacity) VALUES($number, $category, $rate, $capacity);";
        var number = command.Parameters.Add("$number", SqliteType.Text);
        var category = command.Parameters.Add("$category", SqliteType.Text);
        var rate = command.Parameters.Add("$rate", SqliteType.Integer);
        var capacity = command.Parameters.Add("$capacity", SqliteType.Integer);
        for (var floor = 1; floor <= 3; floor++)
        for (var room = 1; room <= 10; room++)
        {
            var categoryValue = room switch
            {
                <= 4 => RoomCategory.Standard,
                <= 7 => RoomCategory.Deluxe,
                <= 9 => RoomCategory.ExecutiveSuite,
                _ => RoomCategory.FamilySuite
            };
            number.Value = $"{floor}{room:00}";
            category.Value = categoryValue.ToString();
            rate.Value = categoryValue switch
            {
                RoomCategory.Standard => 80000,
                RoomCategory.Deluxe => 120000,
                RoomCategory.ExecutiveSuite => 180000,
                _ => 220000
            };
            capacity.Value = categoryValue is RoomCategory.FamilySuite ? 4 : categoryValue is RoomCategory.Standard ? 1 : 2;
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        await transaction.CommitAsync(cancellationToken);
    }
}

namespace RooseHotel.Core;

public enum RoomCategory { Standard, Deluxe, ExecutiveSuite, FamilySuite }
public enum RoomOperationalStatus { Available, Maintenance, OutOfService }
public enum ReservationStatus { Confirmed, CheckedIn, CheckedOut, Cancelled }

public sealed record Room(int Id, string Number, RoomCategory Category, decimal NightlyRate,
    int Capacity, RoomOperationalStatus OperationalStatus);

public sealed record Guest(int Id, string FullName, string Phone, string Email);
public sealed record StaffAccount(int Id, string Username, string DisplayName, string PasswordSalt, string PasswordHash);

public static class PasswordSecurity
{
    private const int Iterations = 210_000;

    public static (string Salt, string Hash) CreateHash(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            throw new ArgumentException("Password must contain at least 8 characters.", nameof(password));
        var salt = System.Security.Cryptography.RandomNumberGenerator.GetBytes(16);
        var hash = System.Security.Cryptography.Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations,
            System.Security.Cryptography.HashAlgorithmName.SHA256, 32);
        return (Convert.ToBase64String(salt), Convert.ToBase64String(hash));
    }

    public static bool Verify(string password, string saltBase64, string expectedHashBase64)
    {
        try
        {
            var salt = Convert.FromBase64String(saltBase64);
            var expected = Convert.FromBase64String(expectedHashBase64);
            var actual = System.Security.Cryptography.Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations,
                System.Security.Cryptography.HashAlgorithmName.SHA256, expected.Length);
            return System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(actual, expected);
        }
        catch (FormatException) { return false; }
    }
}

public sealed record Reservation(int Id, int GuestId, int RoomId, DateOnly CheckIn,
    DateOnly CheckOut, ReservationStatus Status)
{
    public int Nights => CheckOut.DayNumber - CheckIn.DayNumber;
}

public sealed record ReservationRequest(int GuestId, int RoomId, DateOnly CheckIn, DateOnly CheckOut);

public static class ReservationRules
{
    public static bool HasValidDates(DateOnly checkIn, DateOnly checkOut) => checkOut > checkIn;

    // Stays use [check-in, check-out): a guest leaving on a day another guest arrives
    // does not create an overlap.
    public static bool Overlaps(DateOnly start, DateOnly end, DateOnly otherStart, DateOnly otherEnd) =>
        start < otherEnd && otherStart < end;

    public static bool BlocksAvailability(ReservationStatus status) =>
        status is ReservationStatus.Confirmed or ReservationStatus.CheckedIn;

    public static decimal CalculateCharge(decimal nightlyRate, DateOnly checkIn, DateOnly checkOut)
    {
        if (!HasValidDates(checkIn, checkOut))
            throw new ArgumentException("Check-out must be later than check-in.");
        if (nightlyRate < 0)
            throw new ArgumentOutOfRangeException(nameof(nightlyRate));
        return nightlyRate * (checkOut.DayNumber - checkIn.DayNumber);
    }
}

public interface IHotelRepository
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
    Task<int> GetStaffCountAsync(CancellationToken cancellationToken = default);
    Task<StaffAccount?> FindStaffAsync(string username, CancellationToken cancellationToken = default);
    Task CreateStaffAsync(string username, string displayName, string salt, string hash, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Room>> GetRoomsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Room>> FindAvailableRoomsAsync(DateOnly checkIn, DateOnly checkOut,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Guest>> SearchGuestsAsync(string query, CancellationToken cancellationToken = default);
    Task<int> CreateGuestAsync(Guest guest, CancellationToken cancellationToken = default);
    Task<int> CreateReservationAsync(ReservationRequest request, CancellationToken cancellationToken = default);
}

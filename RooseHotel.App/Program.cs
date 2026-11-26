using RooseHotel.Core;
using RooseHotel.Data;

namespace RooseHotel.App;

static class Program
{
    [STAThread]
    static async Task Main()
    {
        ApplicationConfiguration.Initialize();
        var dataDirectoryOverride = Environment.GetEnvironmentVariable("ROOSE_HOTEL_DATA_DIR");
        var databaseDirectory = string.IsNullOrWhiteSpace(dataDirectoryOverride)
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RooseHotel")
            : Path.GetFullPath(dataDirectoryOverride);
        IHotelRepository repository = new SqliteHotelRepository(Path.Combine(databaseDirectory, "roosehotel.db"));
        try
        {
            await repository.InitializeAsync();
            using var login = new LoginForm(repository);
            if (login.ShowDialog() == DialogResult.OK)
                Application.Run(new MainForm(repository, login.StaffDisplayName));
        }
        catch (Exception ex)
        {
            MessageBox.Show($"ROOSE Hotel could not start.\n\n{ex.Message}", "Startup error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}

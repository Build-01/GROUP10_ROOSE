using System.Globalization;
using RooseHotel.Core;
using RooseHotel.Data;

namespace RooseHotel.App;

public sealed class MainForm : Form
{
    private static readonly Color Coffee = Color.FromArgb(87, 58, 41);
    private static readonly Color Gold = Color.FromArgb(190, 139, 65);
    private readonly IHotelRepository _repository;
    private readonly string _staffName;
    private readonly Panel _content = new() { Dock = DockStyle.Fill, Padding = new Padding(28), BackColor = Color.White, AutoScroll = true };
    private readonly Label _status = new() { Dock = DockStyle.Bottom, Height = 30, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(16, 0, 0, 0), ForeColor = Coffee, BackColor = Color.White };
    private Button? _activeNavigation;
    private Button? _dashboardNavigation;

    public MainForm(IHotelRepository repository, string staffName)
    {
        _repository = repository;
        _staffName = staffName;
        Text = "ROOSE Hotel | Hotel Room Reservation System";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1050, 680);
        Size = new Size(1280, 820);
        BackColor = Color.White;
        Font = new Font("Segoe UI", 10F);

        var shell = new Panel { Dock = DockStyle.Fill };
        shell.Controls.Add(_content);
        shell.Controls.Add(BuildSidebar());
        Controls.Add(shell);
        _status.Paint += (_, e) => { using var pen = new Pen(Gold, 1F); e.Graphics.DrawLine(pen, 0, 0, _status.Width, 0); };
        Controls.Add(_status);
        ShowDashboard();
        SetActiveNavigation(_dashboardNavigation);
    }

    private Control BuildSidebar()
    {
        var sidebar = new Panel { Dock = DockStyle.Left, Width = 225, BackColor = Color.White, Padding = new Padding(12, 20, 12, 16) };
        sidebar.Paint += (_, e) => { using var pen = new Pen(Gold, 1F); e.Graphics.DrawLine(pen, sidebar.Width - 1, 0, sidebar.Width - 1, sidebar.Height); };
        var brand = new Label { Text = "ROOSE\nHOTEL", Dock = DockStyle.Top, Height = 105, ForeColor = Coffee, Font = new Font("Georgia", 20F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter };
        sidebar.Controls.Add(brand);
        var brandRule = new Panel { Dock = DockStyle.Top, Height = 2, BackColor = Gold };
        sidebar.Controls.Add(brandRule);
        var nav = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 360, FlowDirection = FlowDirection.TopDown, WrapContents = false, BackColor = Color.White, Padding = new Padding(0, 18, 0, 0) };
        _dashboardNavigation = NavButton("Dashboard", ShowDashboard);
        nav.Controls.Add(_dashboardNavigation);
        nav.Controls.Add(NavButton("Rooms", ShowRooms));
        nav.Controls.Add(NavButton("Availability", ShowAvailability));
        nav.Controls.Add(NavButton("Guests", () => ShowComingSoon("Guest Management")));
        nav.Controls.Add(NavButton("Reservations", () => ShowComingSoon("Reservation Management")));
        nav.Controls.Add(NavButton("Check-in / Check-out", () => ShowComingSoon("Check-in / Check-out")));
        sidebar.Controls.Add(nav);
        sidebar.Controls.Add(new Label { Text = "STAFF WORKSPACE", Dock = DockStyle.Bottom, Height = 35, ForeColor = Coffee, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 8F, FontStyle.Bold) });
        return sidebar;
    }

    private Button NavButton(string text, Action action)
    {
        var button = new Button { Text = "   " + text, Width = 195, Height = 44, FlatStyle = FlatStyle.Flat, ForeColor = Coffee, BackColor = Color.White, TextAlign = ContentAlignment.MiddleLeft, Cursor = Cursors.Hand, Margin = new Padding(0, 2, 0, 2) };
        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.MouseOverBackColor = Color.FromArgb(250, 247, 242);
        button.Click += (_, _) => { SetActiveNavigation(button); action(); };
        return button;
    }

    private void SetActiveNavigation(Button? selected)
    {
        if (_activeNavigation is not null)
        {
            _activeNavigation.BackColor = Color.White;
            _activeNavigation.ForeColor = Coffee;
            _activeNavigation.FlatAppearance.BorderSize = 0;
            _activeNavigation.FlatAppearance.MouseOverBackColor = Color.FromArgb(250, 247, 242);
        }
        _activeNavigation = selected;
        if (_activeNavigation is null) return;
        _activeNavigation.BackColor = Coffee;
        _activeNavigation.ForeColor = Color.White;
        _activeNavigation.FlatAppearance.BorderColor = Gold;
        _activeNavigation.FlatAppearance.BorderSize = 1;
        _activeNavigation.FlatAppearance.MouseOverBackColor = Color.FromArgb(110, 74, 51);
    }

    private void PreparePage(string title, string subtitle)
    {
        _content.Controls.Clear();
        _content.Controls.Add(new Label { Text = title, Left = 0, Top = 0, Width = 900, Height = 48, ForeColor = Coffee, Font = new Font("Georgia", 23F, FontStyle.Bold) });
        _content.Controls.Add(new Label { Text = subtitle, Left = 0, Top = 54, Width = 900, Height = 30, ForeColor = Color.FromArgb(105, 96, 88) });
        _content.Controls.Add(new Panel { Left = 0, Top = 92, Width = 68, Height = 2, BackColor = Gold });
    }

    private void ShowDashboard()
    {
        PreparePage($"Welcome, {_staffName}", "Hotel room reservation system · Staff workspace");
        _content.Controls.Add(Card("Plan stays, manage rooms, and keep guest records in one place.", 0, 110, 830, 75));
        var action = new Button { Text = "Check room availability", Left = 0, Top = 205, Width = 235, Height = 46 };
        StylePrimaryAction(action);
        action.Click += (_, _) => ShowAvailability();
        _content.Controls.Add(action);
        _ = LoadDashboardStatsAsync();
    }

    private async Task LoadDashboardStatsAsync()
    {
        try
        {
            var rooms = await _repository.GetRoomsAsync();
            if (IsDisposed || _content.Controls.Count < 3) return;
            var stats = new FlowLayoutPanel { Left = 0, Top = 290, Width = 850, Height = 125, WrapContents = false, BackColor = Color.White };
            stats.Controls.Add(StatCard("Total Rooms", rooms.Count.ToString(CultureInfo.InvariantCulture)));
            stats.Controls.Add(StatCard("Operational", rooms.Count(r => r.OperationalStatus == RoomOperationalStatus.Available).ToString(CultureInfo.InvariantCulture)));
            stats.Controls.Add(StatCard("Categories", Enum.GetValues<RoomCategory>().Length.ToString(CultureInfo.InvariantCulture)));
            _content.Controls.Add(stats);
        }
        catch (Exception ex) { _status.Text = $"Could not load dashboard information: {ex.Message}"; }
    }

    private void ShowRooms()
    {
        PreparePage("Room Management", "Browse the 30 rooms configured for ROOSE Hotel.");
        _ = LoadRoomsAsync(_content);
    }

    private async Task LoadRoomsAsync(Control host)
    {
        try
        {
            var rooms = await _repository.GetRoomsAsync();
            if (host.IsDisposed || host.Controls.Count == 0) return;
            var grid = CreateRoomGrid();
            foreach (var room in rooms) AddRoomRow(grid, room, room.OperationalStatus.ToString());
            grid.Left = 0;
            grid.Top = 105;
            grid.Width = 900;
            grid.Height = 520;
            host.Controls.Add(grid);
        }
        catch (Exception ex) { _status.Text = $"Could not load rooms: {ex.Message}"; }
    }

    private void ShowAvailability()
    {
        PreparePage("Room Availability", "Check-out day is not counted as an occupied night.");
        var checkIn = new DateTimePicker { Left = 0, Top = 115, Width = 165, Format = DateTimePickerFormat.Short, MinDate = DateTime.Today };
        var checkOut = new DateTimePicker { Left = 185, Top = 115, Width = 165, Format = DateTimePickerFormat.Short, MinDate = DateTime.Today.AddDays(1), Value = DateTime.Today.AddDays(1) };
        checkIn.ValueChanged += (_, _) => checkOut.MinDate = checkIn.Value.Date.AddDays(1);
        _content.Controls.Add(Field("Check-in", checkIn, 0));
        _content.Controls.Add(Field("Check-out", checkOut, 180));
        var find = new Button { Text = "Find available rooms", Left = 370, Top = 137, Width = 200, Height = 34 };
        StylePrimaryAction(find);
        _content.Controls.Add(find);
        var results = CreateRoomGrid();
        results.Left = 0;
        results.Top = 200;
        results.Width = 900;
        results.Height = 440;
        _content.Controls.Add(results);
        find.Click += async (_, _) =>
        {
            results.Rows.Clear();
            find.Enabled = false;
            try
            {
                var available = await _repository.FindAvailableRoomsAsync(DateOnly.FromDateTime(checkIn.Value), DateOnly.FromDateTime(checkOut.Value));
                foreach (var room in available) AddRoomRow(results, room, "Available");
                _status.Text = $"{available.Count} room(s) available for the selected dates.";
            }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, "Availability search", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
            finally { find.Enabled = true; }
        };
    }

    private static DataGridView CreateRoomGrid()
    {
        var grid = new DataGridView { ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, BackgroundColor = Color.White, BorderStyle = BorderStyle.None, CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal, GridColor = Gold, RowHeadersVisible = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, Font = new Font("Segoe UI", 10F), ColumnHeadersHeight = 42, RowTemplate = { Height = 38 } };
        grid.Columns.Add("number", "Room");
        grid.Columns.Add("category", "Category");
        grid.Columns.Add("capacity", "Capacity");
        grid.Columns.Add("rate", "Nightly Rate");
        grid.Columns.Add("status", "Status");
        grid.EnableHeadersVisualStyles = false;
        grid.ColumnHeadersDefaultCellStyle.BackColor = Color.White;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Coffee;
        grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.White;
        grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = Coffee;
        grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
        grid.DefaultCellStyle.SelectionBackColor = Coffee;
        grid.DefaultCellStyle.SelectionForeColor = Color.White;
        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.White;
        return grid;
    }

    private static void AddRoomRow(DataGridView grid, Room room, string status) => grid.Rows.Add(room.Number,
        FormatCategory(room.Category), room.Capacity, room.NightlyRate.ToString("₦#,##0", CultureInfo.GetCultureInfo("en-NG")), status);

    private void ShowComingSoon(string module)
    {
        PreparePage(module, "This module is scheduled for a later development increment.");
        _content.Controls.Add(Card("The application foundation and shared reservation rules are in place. This workflow will be connected in a later increment.", 0, 110, 830, 100));
    }

    private static Panel Card(string text, int left, int top, int width, int height)
    {
        var panel = new Panel { Left = left, Top = top, Width = width, Height = height, BackColor = Color.White, Padding = new Padding(18) };
        panel.Paint += (_, e) => { using var pen = new Pen(Gold, 1F); e.Graphics.DrawRectangle(pen, 0, 0, panel.Width - 2, panel.Height - 2); };
        panel.Controls.Add(new Label { Text = text, Dock = DockStyle.Fill, ForeColor = Coffee, Font = new Font("Segoe UI", 12F), TextAlign = ContentAlignment.MiddleLeft });
        return panel;
    }

    private static Panel StatCard(string label, string value)
    {
        var panel = new Panel { Width = 205, Height = 110, BackColor = Color.White, Margin = new Padding(0, 0, 16, 0), Padding = new Padding(16) };
        panel.Paint += (_, e) => { using var pen = new Pen(Gold, 1F); e.Graphics.DrawRectangle(pen, 0, 0, panel.Width - 2, panel.Height - 2); };
        panel.Controls.Add(new Label { Text = value, Dock = DockStyle.Top, Height = 48, ForeColor = Coffee, Font = new Font("Georgia", 23F, FontStyle.Bold) });
        panel.Controls.Add(new Label { Text = label, Dock = DockStyle.Fill, ForeColor = Color.FromArgb(105, 96, 88) });
        return panel;
    }

    private static Panel Field(string caption, Control control, int left)
    {
        var panel = new Panel { Left = left, Top = 98, Width = control.Width, Height = 55 };
        panel.Controls.Add(new Label { Text = caption, Dock = DockStyle.Top, Height = 20, ForeColor = Coffee, Font = new Font("Segoe UI", 9F, FontStyle.Bold) });
        control.Top = 22;
        panel.Controls.Add(control);
        return panel;
    }

    private static string FormatCategory(RoomCategory category) => category switch
    {
        RoomCategory.ExecutiveSuite => "Executive Suite",
        RoomCategory.FamilySuite => "Family Suite",
        _ => category.ToString()
    };

    private static void StylePrimaryAction(Button button)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.BackColor = Coffee;
        button.ForeColor = Color.White;
        button.FlatAppearance.BorderColor = Gold;
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.MouseOverBackColor = Color.FromArgb(110, 74, 51);
        button.Cursor = Cursors.Hand;
    }
}

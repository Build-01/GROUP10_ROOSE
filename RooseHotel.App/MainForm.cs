using System.Globalization;
using System.Drawing.Printing;
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
    private int _dashboardRequestVersion;

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
        nav.Controls.Add(NavButton("Guests", ShowGuests));
        nav.Controls.Add(NavButton("Reservations", ShowReservations));
        nav.Controls.Add(NavButton("Billing", ShowBilling));
        nav.Controls.Add(NavButton("Check-in / Check-out", ShowCheckInOut));
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
        _dashboardRequestVersion++;
        _content.Controls.Clear();
        _content.Controls.Add(new Label { Text = title, Left = 0, Top = 0, Width = 900, Height = 48, ForeColor = Coffee, Font = new Font("Georgia", 23F, FontStyle.Bold) });
        _content.Controls.Add(new Label { Text = subtitle, Left = 0, Top = 54, Width = 900, Height = 30, ForeColor = Color.FromArgb(105, 96, 88) });
        _content.Controls.Add(new Panel { Left = 0, Top = 92, Width = 68, Height = 2, BackColor = Gold });
    }

    private void ShowDashboard()
    {
        var greeting = DateTime.Now.Hour switch
        {
            < 12 => "Good morning",
            < 17 => "Good afternoon",
            _ => "Good evening"
        };
        PreparePage($"{greeting}, {_staffName}", $"Hotel operations · {DateTime.Today:dddd, d MMMM yyyy}");

        var width = Math.Max(700, _content.ClientSize.Width - 60);
        var root = new Panel
        {
            Left = 0,
            Top = 108,
            Width = width,
            Height = 785,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            BackColor = Color.White
        };
        _content.Controls.Add(root);

        var actions = new Panel { Left = 0, Top = 0, Width = width, Height = 42, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
        var reservations = new Button { Text = "Reservations", Left = 0, Top = 0, Width = 145, Height = 38 };
        StylePrimaryAction(reservations);
        reservations.Click += (_, _) => ShowReservations();
        var guests = new Button { Text = "Register guest", Left = 156, Top = 0, Width = 145, Height = 38 };
        StyleSecondaryAction(guests);
        guests.Click += (_, _) => ShowGuests();
        var availability = new Button { Text = "Check availability", Left = 312, Top = 0, Width = 170, Height = 38 };
        StyleSecondaryAction(availability);
        availability.Click += (_, _) => ShowAvailability();
        var refreshed = new Label
        {
            Text = "Loading saved data…",
            Left = Math.Max(490, width - 264),
            Top = 0,
            Width = 150,
            Height = 38,
            TextAlign = ContentAlignment.MiddleRight,
            ForeColor = Color.FromArgb(105, 96, 88),
            Font = new Font("Segoe UI", 8.5F)
        };
        refreshed.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        var refresh = new Button { Text = "Refresh", Left = width - 104, Top = 0, Width = 104, Height = 38, Anchor = AnchorStyles.Top | AnchorStyles.Right };
        StyleSecondaryAction(refresh);
        actions.Controls.AddRange([reservations, guests, availability, refreshed, refresh]);
        root.Controls.Add(actions);

        var metrics = new TableLayoutPanel
        {
            Left = 0,
            Top = 52,
            Width = width,
            Height = 112,
            ColumnCount = 5,
            RowCount = 1,
            BackColor = Color.White,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            Margin = Padding.Empty
        };
        metrics.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        for (var i = 0; i < 5; i++) metrics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
        var metricCards = new[]
        {
            DashboardMetricCard("Total rooms", "—", "hotel inventory"),
            DashboardMetricCard("Available today", "—", "free for a stay"),
            DashboardMetricCard("Occupied now", "—", "of total rooms"),
            DashboardMetricCard("Arrivals today", "—", "confirmed stays"),
            DashboardMetricCard("Departures today", "—", "checked-in stays")
        };
        for (var i = 0; i < metricCards.Length; i++) metrics.Controls.Add(metricCards[i].Panel, i, 0);
        root.Controls.Add(metrics);

        var operations = new TableLayoutPanel
        {
            Left = 0,
            Top = 174,
            Width = width,
            Height = 78,
            ColumnCount = 3,
            RowCount = 1,
            BackColor = Color.White,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            Margin = Padding.Empty
        };
        operations.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        for (var i = 0; i < 3; i++) operations.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / 3));
        var operationCards = new[]
        {
            DashboardMetricCard("In-house stays", "—", "currently checked in", compact: true),
            DashboardMetricCard("Next 7 days", "—", "confirmed arrivals", compact: true),
            DashboardMetricCard("Rooms needing attention", "—", "maintenance or out of service", compact: true)
        };
        for (var i = 0; i < operationCards.Length; i++) operations.Controls.Add(operationCards[i].Panel, i, 0);
        root.Controls.Add(operations);

        var arrivalsTable = CreateDashboardTable("Today's arrivals", "Expected guest arrivals", "No arrivals scheduled today.", ["Reservation", "Guest", "Room"]);
        var departuresTable = CreateDashboardTable("Today's departures", "Guests currently due to leave", "No departures scheduled today.", ["Reservation", "Guest", "Room"]);
        var dailyTables = new TableLayoutPanel
        {
            Left = 0,
            Top = 262,
            Width = width,
            Height = 244,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Color.White,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            Margin = Padding.Empty
        };
        dailyTables.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        dailyTables.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        dailyTables.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        arrivalsTable.Panel.Margin = new Padding(0, 0, 8, 0);
        departuresTable.Panel.Margin = new Padding(8, 0, 0, 0);
        dailyTables.Controls.Add(arrivalsTable.Panel, 0, 0);
        dailyTables.Controls.Add(departuresTable.Panel, 1, 0);
        root.Controls.Add(dailyTables);

        var upcomingTable = CreateDashboardTable("Upcoming reservations", "Confirmed arrivals over the next 7 days", "No confirmed arrivals in the next 7 days.",
            ["Reservation", "Guest", "Room", "Check-in", "Check-out"]);
        upcomingTable.Panel.SetBounds(0, 516, width, 245);
        upcomingTable.Panel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        root.Controls.Add(upcomingTable.Panel);

        var view = new DashboardView(root, metricCards, operationCards, refreshed,
            arrivalsTable, departuresTable, upcomingTable);
        refresh.Click += async (_, _) => await RefreshDashboardAsync(view);
        _ = RefreshDashboardAsync(view);
    }

    private async Task RefreshDashboardAsync(DashboardView view)
    {
        var requestVersion = ++_dashboardRequestVersion;
        view.LastUpdated.Text = "Refreshing…";
        try
        {
            var roomsTask = _repository.GetRoomsAsync();
            var reservationsTask = _repository.GetReservationsAsync();
            await Task.WhenAll(roomsTask, reservationsTask);
            if (IsDisposed || view.Root.IsDisposed || view.Root.Parent != _content || requestVersion != _dashboardRequestVersion) return;

            var today = DateOnly.FromDateTime(DateTime.Today);
            var summary = DashboardSummary.Build(await roomsTask, await reservationsTask, today);
            SetDashboardMetric(view.Metrics[0], summary.TotalRooms.ToString(CultureInfo.InvariantCulture), $"{summary.InServiceRooms} in service");
            SetDashboardMetric(view.Metrics[1], summary.AvailableToday.ToString(CultureInfo.InvariantCulture), "available for a stay today");
            SetDashboardMetric(view.Metrics[2], summary.OccupiedRooms.ToString(CultureInfo.InvariantCulture), $"{summary.OccupancyPercent:N1}% of total inventory");
            SetDashboardMetric(view.Metrics[3], summary.ArrivalsToday.ToString(CultureInfo.InvariantCulture), "confirmed arrivals today");
            SetDashboardMetric(view.Metrics[4], summary.DeparturesToday.ToString(CultureInfo.InvariantCulture), "checked-in departures today");
            SetDashboardMetric(view.Operations[0], summary.GuestsInHouse.ToString(CultureInfo.InvariantCulture), "currently checked in");
            SetDashboardMetric(view.Operations[1], summary.UpcomingArrivals.ToString(CultureInfo.InvariantCulture), "confirmed arrivals in 7 days");
            SetDashboardMetric(view.Operations[2], summary.RoomsNeedingAttention.ToString(CultureInfo.InvariantCulture), "maintenance or out of service");
            PopulateDashboardTable(view.Arrivals, summary.ArrivalReservations, includeDates: false);
            PopulateDashboardTable(view.Departures, summary.DepartureReservations, includeDates: false);
            PopulateDashboardTable(view.Upcoming, summary.UpcomingReservations, includeDates: true);
            view.LastUpdated.Text = $"Updated {DateTime.Now:h:mm tt}";
            _status.Text = "Dashboard refreshed from saved hotel records.";
        }
        catch (Exception ex)
        {
            if (!IsDisposed && !view.Root.IsDisposed && view.Root.Parent == _content && requestVersion == _dashboardRequestVersion)
            {
                view.LastUpdated.Text = "Refresh failed";
                _status.Text = $"Could not refresh the dashboard: {ex.Message}";
            }
        }
    }

    private static DashboardMetric DashboardMetricCard(string title, string value, string detail, bool compact = false)
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 10, 0),
            BackColor = Color.White,
            Padding = compact ? new Padding(12, 7, 12, 6) : new Padding(12, 9, 12, 8)
        };
        panel.Paint += (_, e) => { using var pen = new Pen(Gold, 1F); e.Graphics.DrawRectangle(pen, 0, 0, panel.Width - 2, panel.Height - 2); };
        var titleLabel = new Label
        {
            Text = title,
            Dock = DockStyle.Top,
            Height = compact ? 19 : 21,
            ForeColor = Color.FromArgb(105, 96, 88),
            Font = new Font("Segoe UI", compact ? 8F : 8.5F, FontStyle.Bold),
            AutoEllipsis = true
        };
        var valueLabel = new Label
        {
            Text = value,
            Dock = DockStyle.Fill,
            ForeColor = Coffee,
            Font = new Font("Georgia", compact ? 17F : 22F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft,
            AutoEllipsis = true
        };
        var detailLabel = new Label
        {
            Text = detail,
            Dock = DockStyle.Bottom,
            Height = compact ? 18 : 20,
            ForeColor = Color.FromArgb(105, 96, 88),
            Font = new Font("Segoe UI", compact ? 7F : 7.5F),
            AutoEllipsis = true
        };
        panel.Controls.Add(valueLabel);
        panel.Controls.Add(detailLabel);
        panel.Controls.Add(titleLabel);
        return new DashboardMetric(panel, valueLabel, detailLabel);
    }

    private static void SetDashboardMetric(DashboardMetric metric, string value, string detail)
    {
        metric.Value.Text = value;
        metric.Detail.Text = detail;
    }

    private static DashboardTable CreateDashboardTable(string title, string subtitle, string emptyText, string[] columns)
    {
        var panel = new Panel { BackColor = Color.White, Padding = new Padding(12) };
        panel.Paint += (_, e) => { using var pen = new Pen(Gold, 1F); e.Graphics.DrawRectangle(pen, 0, 0, panel.Width - 2, panel.Height - 2); };
        var heading = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = Color.White };
        heading.Controls.Add(new Label
        {
            Text = title,
            Dock = DockStyle.Top,
            Height = 23,
            ForeColor = Coffee,
            Font = new Font("Georgia", 11F, FontStyle.Bold),
            AutoEllipsis = true
        });
        heading.Controls.Add(new Label
        {
            Text = subtitle,
            Dock = DockStyle.Fill,
            ForeColor = Color.FromArgb(105, 96, 88),
            Font = new Font("Segoe UI", 7.5F),
            AutoEllipsis = true
        });
        var grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            MultiSelect = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            RowHeadersVisible = false,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
            GridColor = Gold,
            Font = new Font("Segoe UI", 8.5F),
            ColumnHeadersHeight = 32,
            RowTemplate = { Height = 30 }
        };
        foreach (var column in columns) grid.Columns.Add(column.Replace(" ", ""), column);
        grid.EnableHeadersVisualStyles = false;
        grid.ColumnHeadersDefaultCellStyle.BackColor = Color.White;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Coffee;
        grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.White;
        grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = Coffee;
        grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
        grid.DefaultCellStyle.SelectionBackColor = Coffee;
        grid.DefaultCellStyle.SelectionForeColor = Color.White;
        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 247, 242);
        var empty = new Label
        {
            Text = emptyText,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Color.FromArgb(105, 96, 88),
            Font = new Font("Segoe UI", 9F)
        };
        var body = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(0, 5, 0, 0) };
        body.Controls.Add(grid);
        body.Controls.Add(empty);
        panel.Controls.Add(body);
        panel.Controls.Add(heading);
        return new DashboardTable(panel, grid, empty);
    }

    private static void PopulateDashboardTable(DashboardTable table, IReadOnlyList<ReservationSummary> items, bool includeDates)
    {
        table.Grid.Rows.Clear();
        foreach (var item in items)
        {
            object[] values = includeDates
                ? [$"R{item.Id:000}", item.GuestName, item.RoomNumber, item.CheckIn.ToString("dd MMM"), item.CheckOut.ToString("dd MMM")]
                : [$"R{item.Id:000}", item.GuestName, item.RoomNumber];
            var row = table.Grid.Rows.Add(values);
            table.Grid.Rows[row].Tag = item;
        }
        table.Grid.Visible = items.Count > 0;
        table.EmptyState.Visible = items.Count == 0;
    }

    private sealed record DashboardMetric(Panel Panel, Label Value, Label Detail);
    private sealed record DashboardTable(Panel Panel, DataGridView Grid, Label EmptyState);
    private sealed record DashboardView(Panel Root, DashboardMetric[] Metrics, DashboardMetric[] Operations,
        Label LastUpdated, DashboardTable Arrivals, DashboardTable Departures, DashboardTable Upcoming);

    private void ShowRooms()
    {
        PreparePage("Room Management", "Search and maintain room details, rates, capacity, and operating status.");
        var search = new TextBox { PlaceholderText = "Search room number...", BorderStyle = BorderStyle.None, BackColor = Color.White, ForeColor = Coffee };
        var searchFrame = GoldInput(search, 0, 112, 245, 34);
        var category = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = Coffee };
        category.DataSource = new[] { "All categories" }.Concat(Enum.GetValues<RoomCategory>().Select(FormatCategory)).ToList();
        var categoryFrame = GoldInput(category, 260, 112, 180, 34);
        var status = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = Coffee };
        status.DataSource = new[] { "All statuses", "Occupied" }.Concat(Enum.GetNames<RoomOperationalStatus>()).ToList();
        var statusFrame = GoldInput(status, 455, 112, 165, 34);
        var add = new Button { Text = "Add room", Left = 790, Top = 108, Width = 155, Height = 38 };
        StylePrimaryAction(add);
        var grid = CreateRoomGrid();
        grid.Left = 0;
        grid.Top = 163;
        grid.Width = 970;
        grid.Height = 430;
        var edit = new Button { Text = "Edit selected", Left = 0, Top = 610, Width = 160, Height = 38 };
        StylePrimaryAction(edit);
        var details = new Button { Text = "View details", Left = 175, Top = 610, Width = 150, Height = 38 };
        StyleSecondaryAction(details);
        _content.Controls.AddRange([searchFrame, categoryFrame, statusFrame, add, grid, edit, details]);

        async Task RefreshAsync() => await RefreshRoomGridAsync(grid, search.Text, category.SelectedItem?.ToString(), status.SelectedItem?.ToString());
        search.TextChanged += async (_, _) => await RefreshAsync();
        category.SelectedIndexChanged += async (_, _) => await RefreshAsync();
        status.SelectedIndexChanged += async (_, _) => await RefreshAsync();
        add.Click += async (_, _) =>
        {
            using var editor = new RoomEditorForm();
            if (editor.ShowDialog(this) != DialogResult.OK) return;
            try
            {
                await _repository.AddRoomAsync(editor.RoomToSave);
                await RefreshAsync();
                _status.Text = $"Room {editor.RoomToSave.Number} added.";
            }
            catch (Microsoft.Data.Sqlite.SqliteException ex) when (ex.SqliteErrorCode == 19)
            {
                MessageBox.Show(this, "That room number is already in use.", "Room management", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, "Room management", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        };
        edit.Click += async (_, _) =>
        {
            if (SelectedRoom(grid) is not { } room)
            {
                MessageBox.Show(this, "Select a room to edit.", "Room management", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            using var editor = new RoomEditorForm(room);
            if (editor.ShowDialog(this) != DialogResult.OK) return;
            try
            {
                await _repository.UpdateRoomAsync(editor.RoomToSave);
                await RefreshAsync();
                _status.Text = $"Room {editor.RoomToSave.Number} updated.";
            }
            catch (Microsoft.Data.Sqlite.SqliteException ex) when (ex.SqliteErrorCode == 19)
            {
                MessageBox.Show(this, "That room number is already in use.", "Room management", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, "Room management", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        };
        details.Click += (_, _) =>
        {
            if (SelectedRoom(grid) is not { } room)
            {
                MessageBox.Show(this, "Select a room to view its details.", "Room management", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            MessageBox.Show(this,
                $"Room: {room.Number}\nCategory: {FormatCategory(room.Category)}\nNightly rate: {room.NightlyRate:N0} NGN\nCapacity: {room.Capacity}\nStatus: {room.DisplayStatus}",
                $"Room {room.Number}", MessageBoxButtons.OK, MessageBoxIcon.Information);
        };
        _ = RefreshAsync();
    }

    private async Task RefreshRoomGridAsync(DataGridView grid, string query, string? category, string? status)
    {
        try
        {
            var rooms = await _repository.GetRoomsAsync();
            if (grid.IsDisposed) return;
            grid.Rows.Clear();
            foreach (var room in rooms.Where(r =>
                (string.IsNullOrWhiteSpace(query) || r.Number.Contains(query.Trim(), StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrWhiteSpace(category) || category == "All categories" || FormatCategory(r.Category) == category) &&
                (string.IsNullOrWhiteSpace(status) || status == "All statuses" || r.DisplayStatus == status)))
            {
                var index = grid.Rows.Add(room.Number, FormatCategory(room.Category), room.Capacity,
                    room.NightlyRate.ToString("₦#,##0", CultureInfo.GetCultureInfo("en-NG")), room.DisplayStatus);
                grid.Rows[index].Tag = room;
            }
        }
        catch (Exception ex) { _status.Text = $"Could not load rooms: {ex.Message}"; }
    }

    private static Room? SelectedRoom(DataGridView grid) => grid.CurrentRow?.Tag as Room;

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

    private void ShowGuests()
    {
        PreparePage("Guest Management", "Register guests and keep their contact details current.");
        var search = new TextBox { BorderStyle = BorderStyle.None, BackColor = Color.White, ForeColor = Coffee, PlaceholderText = "Search name, phone, or email..." };
        var searchFrame = GoldInput(search, 0, 112, 390, 36);
        var grid = new DataGridView { Left = 0, Top = 165, Width = 900, Height = 390, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false, MultiSelect = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, RowHeadersVisible = false, BackgroundColor = Color.White, BorderStyle = BorderStyle.None };
        grid.Columns.Add("id", "Guest ID"); grid.Columns.Add("name", "Full Name"); grid.Columns.Add("phone", "Phone"); grid.Columns.Add("email", "Email");
        grid.Columns[0].FillWeight = 55;
        grid.EnableHeadersVisualStyles = false;
        grid.ColumnHeadersDefaultCellStyle.BackColor = Color.White; grid.ColumnHeadersDefaultCellStyle.ForeColor = Coffee;
        grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
        grid.DefaultCellStyle.SelectionBackColor = Coffee; grid.DefaultCellStyle.SelectionForeColor = Color.White;
        var add = new Button { Text = "Register guest", Left = 0, Top = 575, Width = 165, Height = 40 }; StylePrimaryAction(add);
        var edit = new Button { Text = "Edit selected", Left = 180, Top = 575, Width = 155, Height = 40 }; StyleSecondaryAction(edit);
        _content.Controls.AddRange([searchFrame, grid, add, edit]);
        async Task RefreshAsync()
        {
            grid.Rows.Clear();
            foreach (var guest in await _repository.SearchGuestsAsync(search.Text))
                grid.Rows.Add($"G{guest.Id:000}", guest.FullName, guest.Phone, guest.Email);
        }
        search.TextChanged += async (_, _) => await RefreshAsync();
        add.Click += async (_, _) =>
        {
            using var form = new GuestEditorForm();
            if (form.ShowDialog(this) != DialogResult.OK) return;
            try { await _repository.CreateGuestAsync(form.GuestToSave); await RefreshAsync(); _status.Text = "Guest registered."; }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, "Guest management", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        };
        edit.Click += async (_, _) =>
        {
            if (grid.CurrentRow is null) { MessageBox.Show(this, "Select a guest to edit.", "Guest management", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            var idText = grid.CurrentRow.Cells[0].Value?.ToString()?.TrimStart('G');
            if (!int.TryParse(idText, out var id)) return;
            var guest = (await _repository.SearchGuestsAsync("" )).FirstOrDefault(g => g.Id == id);
            if (guest is null) return;
            using var form = new GuestEditorForm(guest);
            if (form.ShowDialog(this) != DialogResult.OK) return;
            try { await _repository.UpdateGuestAsync(form.GuestToSave); await RefreshAsync(); _status.Text = "Guest details updated."; }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, "Guest management", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        };
        _ = RefreshAsync();
    }

    private void ShowReservations()
    {
        PreparePage("Reservation Management", "Create and maintain confirmed stays. Dates use check-in inclusive and check-out exclusive.");
        var search = new TextBox { BorderStyle = BorderStyle.None, BackColor = Color.White, ForeColor = Coffee, PlaceholderText = "Search reservation, guest, room, or status..." };
        var searchFrame = GoldInput(search, 0, 112, 390, 36);
        var grid = new DataGridView { Left = 0, Top = 165, Width = 940, Height = 385, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false, MultiSelect = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, RowHeadersVisible = false, BackgroundColor = Color.White, BorderStyle = BorderStyle.None };
        grid.Columns.Add("id", "Reservation"); grid.Columns.Add("guest", "Guest"); grid.Columns.Add("room", "Room"); grid.Columns.Add("checkin", "Check-in"); grid.Columns.Add("checkout", "Check-out"); grid.Columns.Add("status", "Status");
        grid.EnableHeadersVisualStyles = false; grid.ColumnHeadersDefaultCellStyle.BackColor = Color.White; grid.ColumnHeadersDefaultCellStyle.ForeColor = Coffee; grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single; grid.DefaultCellStyle.SelectionBackColor = Coffee; grid.DefaultCellStyle.SelectionForeColor = Color.White;
        var add = new Button { Text = "New reservation", Left = 0, Top = 570, Width = 170, Height = 40 }; StylePrimaryAction(add);
        var edit = new Button { Text = "Edit selected", Left = 185, Top = 570, Width = 145, Height = 40 }; StyleSecondaryAction(edit);
        var cancel = new Button { Text = "Cancel selected", Left = 345, Top = 570, Width = 155, Height = 40 }; StyleSecondaryAction(cancel);
        var details = new Button { Text = "View details", Left = 515, Top = 570, Width = 135, Height = 40 }; StyleSecondaryAction(details);
        _content.Controls.AddRange([searchFrame, grid, add, edit, cancel, details]);
        async Task RefreshAsync()
        {
            grid.Rows.Clear();
            foreach (var item in await _repository.GetReservationsAsync(search.Text))
            {
                var row = grid.Rows.Add($"R{item.Id:000}", item.GuestName, item.RoomNumber, item.CheckIn.ToString("dd MMM yyyy"), item.CheckOut.ToString("dd MMM yyyy"), item.Status);
                grid.Rows[row].Tag = item;
            }
        }
        async Task OpenEditorAsync(ReservationSummary? selected)
        {
            var guests = await _repository.SearchGuestsAsync("");
            var rooms = await _repository.GetRoomsAsync();
            if (guests.Count == 0) { MessageBox.Show(this, "Register a guest before creating a reservation.", "Reservations", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            using var form = new ReservationEditorForm(guests, rooms, selected);
            if (form.ShowDialog(this) != DialogResult.OK) return;
            try
            {
                if (selected is null) await _repository.CreateReservationAsync(form.Request);
                else await _repository.UpdateReservationAsync(new ReservationUpdateRequest(selected.Id, form.Request.GuestId, form.Request.RoomId, form.Request.CheckIn, form.Request.CheckOut));
                await RefreshAsync(); _status.Text = selected is null ? "Reservation created." : "Reservation updated.";
            }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, "Reservations", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }
        search.TextChanged += async (_, _) => await RefreshAsync();
        add.Click += async (_, _) => await OpenEditorAsync(null);
        edit.Click += async (_, _) =>
        {
            if (grid.CurrentRow?.Tag is not ReservationSummary selected) { MessageBox.Show(this, "Select a reservation first.", "Reservations", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            if (selected.Status != ReservationStatus.Confirmed) { MessageBox.Show(this, "Only confirmed reservations can be edited.", "Reservations", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            await OpenEditorAsync(selected);
        };
        cancel.Click += async (_, _) =>
        {
            if (grid.CurrentRow?.Tag is not ReservationSummary selected) { MessageBox.Show(this, "Select a reservation first.", "Reservations", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            if (MessageBox.Show(this, $"Cancel reservation R{selected.Id:000} for {selected.GuestName}?", "Confirm cancellation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            try { await _repository.CancelReservationAsync(selected.Id); await RefreshAsync(); _status.Text = "Reservation cancelled."; }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, "Reservations", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        };
        details.Click += (_, _) =>
        {
            if (grid.CurrentRow?.Tag is not ReservationSummary selected) { MessageBox.Show(this, "Select a reservation first.", "Reservations", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            MessageBox.Show(this, $"Guest: {selected.GuestName}\nRoom: {selected.RoomNumber}\nDates: {selected.CheckIn:dd MMM yyyy} – {selected.CheckOut:dd MMM yyyy}\nNights: {selected.Nights}\nAccommodation: ₦{selected.NightlyRate * selected.Nights:N0}\nStatus: {selected.Status}", $"Reservation R{selected.Id:000}", MessageBoxButtons.OK, MessageBoxIcon.Information);
        };
        _ = RefreshAsync();
    }

    private void ShowBilling()
    {
        PreparePage("Billing Summary", "Review accommodation charges using the room's saved nightly rate.");
        var selector = new ComboBox { Left = 0, Top = 116, Width = 500, DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = Coffee };
        var selectorFrame = GoldInput(selector, 0, 112, 520, 38);
        var summary = Card("Select a reservation to view its accommodation charge.", 0, 176, 850, 285);
        var print = new Button { Text = "Print summary", Left = 0, Top = 485, Width = 165, Height = 40, Enabled = false };
        StylePrimaryAction(print);
        _content.Controls.AddRange([selectorFrame, summary, print]);
        ReservationSummary? selected = null;

        void ShowSelected()
        {
            if (selector.SelectedItem is not BillingChoice choice) return;
            selected = choice.Reservation;
            var total = ReservationRules.CalculateCharge(selected.NightlyRate, selected.CheckIn, selected.CheckOut);
            summary.Controls.Clear();
            summary.Controls.Add(new Label { Text = $"Reservation R{selected.Id:000}  ·  {selected.Status}", Left = 20, Top = 16, Width = 790, Height = 30, ForeColor = Coffee, Font = new Font("Georgia", 17F, FontStyle.Bold) });
            summary.Controls.Add(new Label { Text = $"Guest: {selected.GuestName}\nRoom: {selected.RoomNumber}\nStay: {selected.CheckIn:dd MMM yyyy} to {selected.CheckOut:dd MMM yyyy}", Left = 20, Top = 65, Width = 430, Height = 105, ForeColor = Coffee, Font = new Font("Segoe UI", 11F) });
            summary.Controls.Add(new Label { Text = "Accommodation calculation", Left = 20, Top = 184, Width = 450, Height = 24, ForeColor = Coffee, Font = new Font("Segoe UI", 10F, FontStyle.Bold) });
            summary.Controls.Add(new Label { Text = $"{selected.Nights} night(s)  ×  ₦{selected.NightlyRate:N0}", Left = 20, Top = 215, Width = 520, Height = 28, ForeColor = Coffee });
            summary.Controls.Add(new Label { Text = $"₦{total:N0}", Left = 575, Top = 198, Width = 235, Height = 48, ForeColor = Coffee, Font = new Font("Georgia", 24F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleRight });
            summary.Invalidate();
            print.Enabled = true;
        }

        selector.SelectedIndexChanged += (_, _) => ShowSelected();
        print.Click += (_, _) =>
        {
            if (selected is null) return;
            var printable = selected;
            using var document = new PrintDocument();
            document.DocumentName = $"ROOSE Hotel Reservation R{printable.Id:000}";
            document.PrintPage += (_, e) =>
            {
                if (e.Graphics is not { } graphics) return;
                using var heading = new Font("Georgia", 22F, FontStyle.Bold);
                using var regular = new Font("Segoe UI", 12F);
                using var bold = new Font("Segoe UI", 13F, FontStyle.Bold);
                graphics.DrawString("ROOSE HOTEL", heading, Brushes.SaddleBrown, 70, 70);
                graphics.DrawString("Accommodation Billing Summary", bold, Brushes.SaddleBrown, 70, 125);
                graphics.DrawString($"Reservation: R{printable.Id:000}\nGuest: {printable.GuestName}\nRoom: {printable.RoomNumber}\nCheck-in: {printable.CheckIn:dd MMM yyyy}\nCheck-out: {printable.CheckOut:dd MMM yyyy}\nNights: {printable.Nights}\nNightly rate: ₦{printable.NightlyRate:N0}", regular, Brushes.Black, 70, 190);
                graphics.DrawString($"Total accommodation charge: ₦{ReservationRules.CalculateCharge(printable.NightlyRate, printable.CheckIn, printable.CheckOut):N0}", bold, Brushes.SaddleBrown, 70, 380);
                e.HasMorePages = false;
            };
            using var preview = new PrintPreviewDialog { Document = document, Width = 900, Height = 700 };
            preview.ShowDialog(this);
        };

        _ = LoadReservationsAsync();
        async Task LoadReservationsAsync()
        {
            try
            {
                var items = (await _repository.GetReservationsAsync(""))
                    .Where(item => item.Status != ReservationStatus.Cancelled).ToList();
                selector.DisplayMember = nameof(BillingChoice.Label);
                selector.DataSource = items.Select(item => new BillingChoice(item, $"R{item.Id:000} · {item.GuestName} · Room {item.RoomNumber}")).ToList();
                if (items.Count == 0) _status.Text = "No billable reservations are available yet.";
            }
            catch (Exception ex) { _status.Text = $"Could not load billing data: {ex.Message}"; }
        }
    }

    private sealed record BillingChoice(ReservationSummary Reservation, string Label);

    private void ShowCheckInOut()
    {
        PreparePage("Check-in / Check-out", "Record arrivals and departures; checked-in rooms remain unavailable until checkout.");
        var search = new TextBox { BorderStyle = BorderStyle.None, BackColor = Color.White, ForeColor = Coffee, PlaceholderText = "Search reservation, guest, room, or status..." };
        var searchFrame = GoldInput(search, 0, 112, 390, 36);
        var grid = new DataGridView { Left = 0, Top = 165, Width = 955, Height = 380, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false, MultiSelect = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, RowHeadersVisible = false, BackgroundColor = Color.White, BorderStyle = BorderStyle.None };
        grid.Columns.Add("id", "Reservation"); grid.Columns.Add("guest", "Guest"); grid.Columns.Add("room", "Room"); grid.Columns.Add("checkin", "Booked Check-in"); grid.Columns.Add("checkout", "Booked Check-out"); grid.Columns.Add("status", "Status");
        grid.EnableHeadersVisualStyles = false; grid.ColumnHeadersDefaultCellStyle.BackColor = Color.White; grid.ColumnHeadersDefaultCellStyle.ForeColor = Coffee; grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single; grid.DefaultCellStyle.SelectionBackColor = Coffee; grid.DefaultCellStyle.SelectionForeColor = Color.White;
        var checkIn = new Button { Text = "Check in guest", Left = 0, Top = 565, Width = 160, Height = 40 }; StylePrimaryAction(checkIn);
        var checkOut = new Button { Text = "Check out guest", Left = 175, Top = 565, Width = 165, Height = 40 }; StyleSecondaryAction(checkOut);
        var details = new Button { Text = "View stay details", Left = 355, Top = 565, Width = 160, Height = 40 }; StyleSecondaryAction(details);
        _content.Controls.AddRange([searchFrame, grid, checkIn, checkOut, details]);
        async Task RefreshAsync()
        {
            grid.Rows.Clear();
            foreach (var item in await _repository.GetReservationsAsync(search.Text))
            {
                var row = grid.Rows.Add($"R{item.Id:000}", item.GuestName, item.RoomNumber, item.CheckIn.ToString("dd MMM yyyy"), item.CheckOut.ToString("dd MMM yyyy"), item.Status);
                grid.Rows[row].Tag = item;
            }
        }
        ReservationSummary? Selected() => grid.CurrentRow?.Tag as ReservationSummary;
        search.TextChanged += async (_, _) => await RefreshAsync();
        checkIn.Click += async (_, _) =>
        {
            var item = Selected();
            if (item is null) { MessageBox.Show(this, "Select a reservation first.", "Check-in", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            try { await _repository.CheckInReservationAsync(item.Id, DateTimeOffset.Now); await RefreshAsync(); _status.Text = $"{item.GuestName} checked in; room {item.RoomNumber} is now occupied."; }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, "Check-in", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        };
        checkOut.Click += async (_, _) =>
        {
            var item = Selected();
            if (item is null) { MessageBox.Show(this, "Select a reservation first.", "Check-out", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            try { await _repository.CheckOutReservationAsync(item.Id, DateTimeOffset.Now); await RefreshAsync(); _status.Text = $"{item.GuestName} checked out; room {item.RoomNumber} is available again."; }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, "Check-out", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        };
        details.Click += (_, _) =>
        {
            var item = Selected();
            if (item is null) { MessageBox.Show(this, "Select a reservation first.", "Stay details", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            var arrival = item.ActualCheckIn?.ToLocalTime().ToString("dd MMM yyyy, h:mm tt") ?? "Not checked in";
            var departure = item.ActualCheckOut?.ToLocalTime().ToString("dd MMM yyyy, h:mm tt") ?? "Not checked out";
            MessageBox.Show(this, $"Guest: {item.GuestName}\nRoom: {item.RoomNumber}\nBooked stay: {item.CheckIn:dd MMM yyyy} – {item.CheckOut:dd MMM yyyy}\nActual arrival: {arrival}\nActual departure: {departure}\nStatus: {item.Status}", $"Reservation R{item.Id:000}", MessageBoxButtons.OK, MessageBoxIcon.Information);
        };
        _ = RefreshAsync();
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

    private static void StyleSecondaryAction(Button button)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.BackColor = Color.White;
        button.ForeColor = Coffee;
        button.FlatAppearance.BorderColor = Gold;
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.MouseOverBackColor = Color.FromArgb(250, 247, 242);
        button.Cursor = Cursors.Hand;
    }

    private static Panel GoldInput(Control input, int left, int top, int width, int height)
    {
        var frame = new Panel { Left = left, Top = top, Width = width, Height = height, BackColor = Color.White };
        frame.Paint += (_, e) => { using var pen = new Pen(input.Focused ? Coffee : Gold, 1F); e.Graphics.DrawRectangle(pen, 0, 0, frame.Width - 2, frame.Height - 2); };
        input.SetBounds(6, 5, width - 12, height - 10);
        input.GotFocus += (_, _) => frame.Invalidate();
        input.LostFocus += (_, _) => frame.Invalidate();
        frame.Controls.Add(input);
        return frame;
    }
}

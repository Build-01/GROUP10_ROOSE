using RooseHotel.Core;

namespace RooseHotel.App;

public sealed class ReservationEditorForm : Form
{
    private static readonly Color Coffee = Color.FromArgb(87, 58, 41);
    private static readonly Color Gold = Color.FromArgb(190, 139, 65);
    private readonly ComboBox _guest = new() { DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat };
    private readonly ComboBox _room = new() { DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat };
    private readonly DateTimePicker _checkIn = new() { Format = DateTimePickerFormat.Short };
    private readonly DateTimePicker _checkOut = new() { Format = DateTimePickerFormat.Short };

    public ReservationRequest Request => new(((GuestChoice)_guest.SelectedItem!).Id, ((RoomChoice)_room.SelectedItem!).Id,
        DateOnly.FromDateTime(_checkIn.Value), DateOnly.FromDateTime(_checkOut.Value));

    public ReservationEditorForm(IReadOnlyList<Guest> guests, IReadOnlyList<Room> rooms, ReservationSummary? reservation = null)
    {
        Text = reservation is null ? "New Reservation" : "Edit Reservation";
        StartPosition = FormStartPosition.CenterParent; FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false; MinimizeBox = false; ShowInTaskbar = false;
        ClientSize = new Size(470, 390); BackColor = Color.White; Font = new Font("Segoe UI", 10F);
        Controls.Add(new Label { Text = reservation is null ? "Create reservation" : "Edit reservation", Left = 26, Top = 20, Width = 420, Height = 36, ForeColor = Coffee, Font = new Font("Georgia", 19F, FontStyle.Bold) });
        _guest.DisplayMember = nameof(GuestChoice.Label); _guest.DataSource = guests.Select(g => new GuestChoice(g.Id, $"{g.FullName} · {g.Phone}")).ToList();
        _room.DisplayMember = nameof(RoomChoice.Label); _room.DataSource = rooms.Where(r => r.OperationalStatus == RoomOperationalStatus.Available).Select(r => new RoomChoice(r.Id, $"{r.Number} · {r.Category} · ₦{r.NightlyRate:N0}/night")).ToList();
        if (reservation is not null)
        {
            _checkIn.MinDate = DateTimePicker.MinimumDateTime; _checkOut.MinDate = DateTimePicker.MinimumDateTime;
            var guest = guests.FirstOrDefault(g => g.Id == reservation.GuestId);
            if (guest is not null) _guest.SelectedItem = _guest.Items.Cast<GuestChoice>().FirstOrDefault(g => g.Id == guest.Id);
            _room.SelectedItem = _room.Items.Cast<RoomChoice>().FirstOrDefault(r => r.Label.StartsWith(reservation.RoomNumber + " ·", StringComparison.Ordinal));
            _checkIn.Value = reservation.CheckIn.ToDateTime(TimeOnly.MinValue);
            _checkOut.Value = reservation.CheckOut.ToDateTime(TimeOnly.MinValue);
        }
        else
        {
            _checkIn.MinDate = DateTime.Today; _checkOut.MinDate = DateTime.Today.AddDays(1);
            _checkIn.Value = DateTime.Today; _checkOut.Value = DateTime.Today.AddDays(1);
        }
        _checkIn.ValueChanged += (_, _) => { var minimum = _checkIn.Value.Date.AddDays(1); _checkOut.MinDate = minimum; if (_checkOut.Value.Date < minimum) _checkOut.Value = minimum; };
        AddField("Guest", _guest, 76); AddField("Room", _room, 142); AddField("Check-in", _checkIn, 208); AddField("Check-out", _checkOut, 274);
        var save = new Button { Text = reservation is null ? "Create reservation" : "Save changes", Left = 255, Top = 333, Width = 180, Height = 38, FlatStyle = FlatStyle.Flat, BackColor = Coffee, ForeColor = Color.White, Cursor = Cursors.Hand };
        save.FlatAppearance.BorderColor = Gold; save.FlatAppearance.BorderSize = 1;
        save.Click += (_, _) =>
        {
            if (_guest.SelectedItem is null || _room.SelectedItem is null) { MessageBox.Show(this, "Select a guest and an available room.", "Reservation details", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            if (_checkOut.Value.Date <= _checkIn.Value.Date) { MessageBox.Show(this, "Check-out must be later than check-in.", "Reservation dates", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            DialogResult = DialogResult.OK; Close();
        };
        var cancel = new Button { Text = "Cancel", Left = 160, Top = 333, Width = 85, Height = 38, DialogResult = DialogResult.Cancel, FlatStyle = FlatStyle.Flat, ForeColor = Coffee, BackColor = Color.White };
        cancel.FlatAppearance.BorderColor = Gold;
        Controls.Add(save); Controls.Add(cancel); AcceptButton = save; CancelButton = cancel;
    }

    private void AddField(string caption, Control input, int top)
    {
        Controls.Add(new Label { Text = caption, Left = 28, Top = top, Width = 410, Height = 19, ForeColor = Coffee, Font = new Font("Segoe UI", 9F, FontStyle.Bold) });
        input.SetBounds(28, top + 22, 410, 32); input.BackColor = Color.White; input.ForeColor = Coffee;
        Controls.Add(input);
    }

    private sealed record GuestChoice(int Id, string Label);
    private sealed record RoomChoice(int Id, string Label);
}

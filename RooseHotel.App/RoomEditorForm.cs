using RooseHotel.Core;

namespace RooseHotel.App;

public sealed class RoomEditorForm : Form
{
    private sealed record LabeledValue<T>(T Value, string Label);
    private static readonly Color Coffee = Color.FromArgb(87, 58, 41);
    private static readonly Color Gold = Color.FromArgb(190, 139, 65);
    private readonly TextBox _number = new();
    private readonly ComboBox _category = new();
    private readonly NumericUpDown _rate = new();
    private readonly NumericUpDown _capacity = new();
    private readonly ComboBox _status = new();
    private readonly int _roomId;

    public Room RoomToSave => new(_roomId, _number.Text.Trim(), (RoomCategory)_category.SelectedValue!,
        _rate.Value, decimal.ToInt32(_capacity.Value), (RoomOperationalStatus)_status.SelectedValue!);

    public RoomEditorForm(Room? room = null)
    {
        _roomId = room?.Id ?? 0;
        Text = room is null ? "ROOSE Hotel | Add Room" : "ROOSE Hotel | Edit Room";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(540, 500);
        BackColor = Color.White;
        Font = new Font("Segoe UI", 10F);

        var frame = new Panel { Left = 22, Top = 18, Width = 496, Height = 460, BackColor = Color.White };
        frame.Paint += (_, e) => { using var pen = new Pen(Gold, 1.2F); e.Graphics.DrawRectangle(pen, 0, 0, frame.Width - 2, frame.Height - 2); };
        frame.Controls.Add(new Label { Text = room is null ? "Add a room" : "Edit room details", Left = 28, Top = 24, Width = 430, Height = 38, ForeColor = Coffee, Font = new Font("Georgia", 20F, FontStyle.Bold) });
        frame.Controls.Add(new Label { Text = "Room number, category, rate, capacity, and operational status", Left = 30, Top = 64, Width = 432, Height = 26, ForeColor = Color.FromArgb(105, 96, 88), Font = new Font("Segoe UI", 9F) });

        ConfigureInputs();
        AddField(frame, "Room number", _number, 105, 432);
        AddField(frame, "Category", _category, 171, 207);
        AddField(frame, "Nightly rate (₦)", _rate, 171, 207, rightColumn: true);
        AddField(frame, "Capacity", _capacity, 237, 207);
        AddField(frame, "Operational status", _status, 237, 207, rightColumn: true);

        var save = new Button { Text = room is null ? "ADD ROOM" : "SAVE CHANGES", Left = 246, Top = 365, Width = 216, Height = 42, DialogResult = DialogResult.OK };
        StyleAction(save);
        var cancel = new Button { Text = "CANCEL", Left = 30, Top = 365, Width = 190, Height = 42, DialogResult = DialogResult.Cancel, FlatStyle = FlatStyle.Flat, ForeColor = Coffee, BackColor = Color.White };
        cancel.FlatAppearance.BorderColor = Gold;
        cancel.FlatAppearance.BorderSize = 1;
        frame.Controls.AddRange([save, cancel]);
        Controls.Add(frame);
        AcceptButton = save;
        CancelButton = cancel;

        if (room is not null)
        {
            _number.Text = room.Number;
            _category.SelectedValue = room.Category;
            _rate.Value = Math.Clamp(room.NightlyRate, _rate.Minimum, _rate.Maximum);
            _capacity.Value = Math.Clamp(room.Capacity, decimal.ToInt32(_capacity.Minimum), decimal.ToInt32(_capacity.Maximum));
            _status.SelectedValue = room.OperationalStatus;
        }
        else
        {
            _category.SelectedIndex = 0;
            _status.SelectedItem = RoomOperationalStatus.Available;
            _capacity.Value = 1;
            _number.Focus();
        }
    }

    private void ConfigureInputs()
    {
        _number.BorderStyle = BorderStyle.None;
        _category.DropDownStyle = ComboBoxStyle.DropDownList;
        _category.FlatStyle = FlatStyle.Flat;
        _category.DisplayMember = nameof(LabeledValue<RoomCategory>.Label);
        _category.ValueMember = nameof(LabeledValue<RoomCategory>.Value);
        _category.DataSource = Enum.GetValues<RoomCategory>().Select(c => new LabeledValue<RoomCategory>(c, FormatCategory(c))).ToList();
        _rate.Minimum = 0;
        _rate.Maximum = 999_999_999;
        _rate.Increment = 1_000;
        _rate.DecimalPlaces = 0;
        _rate.ThousandsSeparator = true;
        _rate.BorderStyle = BorderStyle.None;
        _capacity.Minimum = 1;
        _capacity.Maximum = 20;
        _capacity.BorderStyle = BorderStyle.None;
        _status.DropDownStyle = ComboBoxStyle.DropDownList;
        _status.FlatStyle = FlatStyle.Flat;
        _status.DisplayMember = nameof(LabeledValue<RoomOperationalStatus>.Label);
        _status.ValueMember = nameof(LabeledValue<RoomOperationalStatus>.Value);
        _status.DataSource = Enum.GetValues<RoomOperationalStatus>().Select(s => new LabeledValue<RoomOperationalStatus>(s, FormatStatus(s))).ToList();
        foreach (var control in new Control[] { _number, _category, _rate, _capacity, _status })
        {
            control.BackColor = Color.White;
            control.ForeColor = Coffee;
            control.Font = new Font("Segoe UI", 10F);
        }
    }

    private static string FormatCategory(RoomCategory category) => category switch
    {
        RoomCategory.ExecutiveSuite => "Executive Suite",
        RoomCategory.FamilySuite => "Family Suite",
        _ => category.ToString()
    };

    private static string FormatStatus(RoomOperationalStatus status) => status switch
    {
        RoomOperationalStatus.OutOfService => "Out of service",
        _ => status.ToString()
    };

    private static void AddField(Control parent, string title, Control input, int top, int width, bool rightColumn = false)
    {
        var left = rightColumn ? 255 : 30;
        var label = new Label { Text = title, Left = left, Top = top, Width = width, Height = 19, ForeColor = Coffee, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
        var border = new Panel { Left = left, Top = top + 22, Width = width, Height = 32, BackColor = Color.White };
        border.Paint += (_, e) =>
        {
            using var pen = new Pen(input.Focused ? Coffee : Gold, input.Focused ? 1.4F : 1F);
            e.Graphics.DrawRectangle(pen, 0, 0, border.Width - 2, border.Height - 2);
        };
        input.SetBounds(6, 5, width - 12, 22);
        input.GotFocus += (_, _) => border.Invalidate();
        input.LostFocus += (_, _) => border.Invalidate();
        border.Controls.Add(input);
        parent.Controls.Add(label);
        parent.Controls.Add(border);
    }

    private static void StyleAction(Button button)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.BackColor = Coffee;
        button.ForeColor = Color.White;
        button.FlatAppearance.BorderColor = Gold;
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.MouseOverBackColor = Color.FromArgb(110, 74, 51);
        button.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
    }
}

using RooseHotel.Core;

namespace RooseHotel.App;

public sealed class GuestEditorForm : Form
{
    private static readonly Color Coffee = Color.FromArgb(87, 58, 41);
    private static readonly Color Gold = Color.FromArgb(190, 139, 65);
    private readonly TextBox _name = new();
    private readonly TextBox _phone = new();
    private readonly TextBox _email = new();
    private readonly int _guestId;

    public Guest GuestToSave => new(_guestId, _name.Text.Trim(), _phone.Text.Trim(), _email.Text.Trim());

    public GuestEditorForm(Guest? guest = null)
    {
        _guestId = guest?.Id ?? 0;
        Text = guest is null ? "Register Guest" : "Edit Guest";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false; MinimizeBox = false; ShowInTaskbar = false;
        ClientSize = new Size(420, 340); BackColor = Color.White; Font = new Font("Segoe UI", 10F);
        var title = new Label { Text = guest is null ? "Register guest" : "Update guest details", Left = 26, Top = 20, Width = 360, Height = 36, ForeColor = Coffee, Font = new Font("Georgia", 19F, FontStyle.Bold) };
        Controls.Add(title);
        AddField("Full name *", _name, 75); AddField("Phone *", _phone, 145); AddField("Email", _email, 215);
        _name.Text = guest?.FullName ?? ""; _phone.Text = guest?.Phone ?? ""; _email.Text = guest?.Email ?? "";
        var save = new Button { Text = guest is null ? "Register" : "Save changes", Left = 250, Top = 285, Width = 140, Height = 38, DialogResult = DialogResult.None, FlatStyle = FlatStyle.Flat, BackColor = Coffee, ForeColor = Color.White, Cursor = Cursors.Hand };
        save.FlatAppearance.BorderColor = Gold; save.FlatAppearance.BorderSize = 1;
        save.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(_name.Text) || string.IsNullOrWhiteSpace(_phone.Text))
            { MessageBox.Show(this, "Full name and phone are required.", "Guest details", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            DialogResult = DialogResult.OK; Close();
        };
        var cancel = new Button { Text = "Cancel", Left = 155, Top = 285, Width = 85, Height = 38, DialogResult = DialogResult.Cancel, FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = Coffee };
        cancel.FlatAppearance.BorderColor = Gold;
        Controls.Add(save); Controls.Add(cancel); AcceptButton = save; CancelButton = cancel;
    }

    private void AddField(string caption, TextBox input, int top)
    {
        Controls.Add(new Label { Text = caption, Left = 28, Top = top, Width = 360, Height = 20, ForeColor = Coffee, Font = new Font("Segoe UI", 9F, FontStyle.Bold) });
        input.SetBounds(28, top + 23, 360, 34); input.BorderStyle = BorderStyle.FixedSingle; input.BackColor = Color.White; input.ForeColor = Coffee;
        Controls.Add(input);
    }
}

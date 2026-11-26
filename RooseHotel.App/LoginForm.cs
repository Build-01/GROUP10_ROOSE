using RooseHotel.Core;

namespace RooseHotel.App;

public sealed class LoginForm : Form
{
    private static readonly Color Coffee = Color.FromArgb(87, 58, 41);
    private static readonly Color Gold = Color.FromArgb(190, 139, 65);
    private readonly IHotelRepository _repository;
    private readonly TextBox _username = new();
    private readonly TextBox _displayName = new();
    private readonly TextBox _password = new();
    private readonly TextBox _confirmPassword = new();
    private Label _displayNameCaption = new();
    private Label _passwordCaption = new();
    private Label _confirmPasswordCaption = new();
    private readonly Label _title = new();
    private readonly Label _note = new();
    private readonly Label _footer = new();
    private readonly Button _submit = new();
    private bool _firstRun;

    public string StaffDisplayName { get; private set; } = "";

    public LoginForm(IHotelRepository repository)
    {
        _repository = repository;
        Text = "ROOSE Hotel | Staff Access";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(820, 660);
        BackColor = Color.White;
        Font = new Font("Segoe UI", 10F);

        var frame = new Panel { Left = 30, Top = 20, Width = 760, Height = 600, BackColor = Color.White };
        frame.Paint += (_, e) =>
        {
            using var pen = new Pen(Gold, 1.2F);
            e.Graphics.DrawRectangle(pen, 0, 0, frame.Width - 2, frame.Height - 2);
        };

        using var sourceLogo = Image.FromFile(Path.Combine(AppContext.BaseDirectory, "assets", "roose-brand-mark.png"));
        var logo = new PictureBox { Left = 93, Top = 37, Width = 130, Height = 80, BackColor = Color.White, SizeMode = PictureBoxSizeMode.Zoom, Image = new Bitmap(sourceLogo) };
        var wordmark = new Label { Text = "ROOSE HOTEL", Left = 236, Top = 47, Width = 420, Height = 40, ForeColor = Coffee, Font = new Font("Georgia", 25F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft };
        var tagline = new Label { Text = "Luxury. Comfort. Excellence.", Left = 239, Top = 88, Width = 410, Height = 28, ForeColor = Coffee, Font = new Font("Georgia", 11F, FontStyle.Italic) };
        var divider = new Panel { Left = 55, Top = 132, Width = 650, Height = 2, BackColor = Gold };

        _title.SetBounds(130, 154, 500, 38);
        _title.Font = new Font("Georgia", 21F, FontStyle.Bold);
        _title.ForeColor = Coffee;
        _title.TextAlign = ContentAlignment.MiddleCenter;
        _note.SetBounds(130, 195, 500, 28);
        _note.ForeColor = Color.FromArgb(105, 96, 88);
        _note.TextAlign = ContentAlignment.MiddleCenter;

        AddField(frame, "Username", _username, 240);
        _displayNameCaption = AddField(frame, "Display name", _displayName, 300);
        _passwordCaption = AddField(frame, "Password", _password, 360, password: true);
        _confirmPasswordCaption = AddField(frame, "Confirm password", _confirmPassword, 420, password: true);

        _submit.SetBounds(130, 493, 500, 46);
        _submit.FlatStyle = FlatStyle.Flat;
        _submit.FlatAppearance.BorderColor = Gold;
        _submit.FlatAppearance.BorderSize = 1;
        _submit.BackColor = Coffee;
        _submit.ForeColor = Color.White;
        _submit.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        _submit.Cursor = Cursors.Hand;
        _submit.FlatAppearance.MouseOverBackColor = Color.FromArgb(110, 74, 51);
        _submit.Click += async (_, _) => await SubmitAsync();

        _footer.SetBounds(95, 548, 570, 30);
        _footer.ForeColor = Color.FromArgb(105, 96, 88);
        _footer.Font = new Font("Segoe UI", 9F);
        _footer.TextAlign = ContentAlignment.MiddleCenter;

        frame.Controls.AddRange([logo, wordmark, tagline, divider, _title, _note, _submit, _footer]);
        Controls.Add(frame);
        AcceptButton = _submit;
        Load += async (_, _) => await SetModeAsync();
    }

    private Label AddField(Control parent, string caption, TextBox input, int top, bool password = false)
    {
        var label = new Label { Text = caption, Left = 130, Top = top, Width = 500, Height = 19, ForeColor = Coffee, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
        var border = new Panel { Left = 130, Top = top + 22, Width = 500, Height = 32, BackColor = Color.White };
        border.Paint += (_, e) =>
        {
            using var pen = new Pen(input.Focused ? Coffee : Gold, input.Focused ? 1.4F : 1F);
            e.Graphics.DrawRectangle(pen, 0, 0, border.Width - 2, border.Height - 2);
        };
        input.BorderStyle = BorderStyle.None;
        input.SetBounds(7, 6, 484, 20);
        input.BackColor = Color.White;
        input.ForeColor = Coffee;
        input.Font = new Font("Segoe UI", 10F);
        if (password) input.UseSystemPasswordChar = true;
        input.GotFocus += (_, _) => border.Invalidate();
        input.LostFocus += (_, _) => border.Invalidate();
        border.Controls.Add(input);
        parent.Controls.Add(label);
        parent.Controls.Add(border);
        return label;
    }

    private async Task SetModeAsync()
    {
        try
        {
            _firstRun = await _repository.GetStaffCountAsync() == 0;
            _title.Text = _firstRun ? "First-time staff setup" : "Staff Login";
            _note.Text = _firstRun
                ? "Create the initial account for authorized ROOSE Hotel staff."
                : "Sign in to continue to the ROOSE Hotel staff workspace.";
            _displayName.Visible = _displayNameCaption.Visible = _firstRun;
            _confirmPassword.Visible = _confirmPasswordCaption.Visible = _firstRun;
            _displayName.Parent!.Visible = _firstRun;
            _confirmPassword.Parent!.Visible = _firstRun;
            _submit.Text = _firstRun ? "CREATE FIRST STAFF ACCOUNT" : "SIGN IN";
            _footer.Text = _firstRun
                ? "The first account is for authorized staff. Future account creation is restricted."
                : "New staff accounts are created by authorized management.";
            if (_firstRun)
            {
                _submit.Top = 493;
                _username.Focus();
            }
            else
            {
                _passwordCaption.Top = 300;
                _password.Parent!.Top = 322;
                _submit.Top = 390;
                _footer.Top = 448;
                _username.Focus();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Staff access could not be loaded.\n\n{ex.Message}", "ROOSE Hotel", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Close();
        }
    }

    private async Task SubmitAsync()
    {
        if (string.IsNullOrWhiteSpace(_username.Text) || string.IsNullOrWhiteSpace(_password.Text))
        {
            MessageBox.Show(this, "Enter a username and password.", "Staff access", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _submit.Enabled = false;
        try
        {
            if (_firstRun)
            {
                if (string.IsNullOrWhiteSpace(_displayName.Text))
                    throw new ArgumentException("Enter the staff member's display name.");
                if (_password.Text != _confirmPassword.Text)
                    throw new ArgumentException("The passwords do not match.");
                var (salt, hash) = PasswordSecurity.CreateHash(_password.Text);
                await _repository.CreateStaffAsync(_username.Text, _displayName.Text, salt, hash);
                StaffDisplayName = _displayName.Text.Trim();
                DialogResult = DialogResult.OK;
            }
            else
            {
                var staff = await _repository.FindStaffAsync(_username.Text);
                if (staff is null || !PasswordSecurity.Verify(_password.Text, staff.PasswordSalt, staff.PasswordHash))
                    throw new ArgumentException("The username or password is incorrect.");
                StaffDisplayName = staff.DisplayName;
                DialogResult = DialogResult.OK;
            }
        }
        catch (ArgumentException ex)
        {
            MessageBox.Show(this, ex.Message, "Staff access", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _password.Clear();
            _confirmPassword.Clear();
            _password.Focus();
        }
        catch (Microsoft.Data.Sqlite.SqliteException ex) when (ex.SqliteErrorCode == 19)
        {
            MessageBox.Show(this, "That username is already in use. Choose a different username.", "Staff access", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Could not complete staff access.\n\n{ex.Message}", "Staff access", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally { _submit.Enabled = true; }
    }
}

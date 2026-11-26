using RooseHotel.Core;

namespace RooseHotel.App;

public sealed class LoginForm : Form
{
    private readonly IHotelRepository _repository;
    private readonly TextBox _username = new();
    private readonly TextBox _displayName = new();
    private readonly TextBox _password = new();
    private readonly TextBox _confirmPassword = new();
    private readonly Label _title = new();
    private readonly Label _note = new();
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
        ClientSize = new Size(480, 550);
        BackColor = Color.FromArgb(250, 246, 237);
        Font = new Font("Segoe UI", 10F);

        var brand = new Label { Text = "ROOSE HOTEL", Left = 44, Top = 28, Width = 390, Height = 52, Font = new Font("Georgia", 24F, FontStyle.Bold), ForeColor = Color.FromArgb(105, 60, 28), TextAlign = ContentAlignment.MiddleCenter };
        _title.SetBounds(55, 100, 370, 34);
        _title.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
        _title.ForeColor = Color.FromArgb(58, 33, 18);
        _note.SetBounds(55, 137, 370, 42);
        _note.ForeColor = Color.DimGray;
        _note.TextAlign = ContentAlignment.MiddleLeft;

        AddField("Username", _username, 190);
        AddField("Display name", _displayName, 253);
        AddField("Password", _password, 316, password: true);
        AddField("Confirm password", _confirmPassword, 379, password: true);

        _submit.SetBounds(55, 455, 370, 44);
        _submit.FlatStyle = FlatStyle.Flat;
        _submit.FlatAppearance.BorderSize = 0;
        _submit.BackColor = Color.FromArgb(105, 60, 28);
        _submit.ForeColor = Color.White;
        _submit.Click += async (_, _) => await SubmitAsync();
        AcceptButton = _submit;
        Controls.AddRange([brand, _title, _note, _submit]);
        Load += async (_, _) => await SetModeAsync();
    }

    private void AddField(string label, TextBox input, int top, bool password = false)
    {
        var caption = new Label { Text = label, Left = 55, Top = top, Width = 370, Height = 20, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(58, 33, 18) };
        input.SetBounds(55, top + 22, 370, 32);
        if (password) input.UseSystemPasswordChar = true;
        Controls.Add(caption);
        Controls.Add(input);
    }

    private async Task SetModeAsync()
    {
        try
        {
            _firstRun = await _repository.GetStaffCountAsync() == 0;
            _title.Text = _firstRun ? "Create the first staff account" : "Staff sign in";
            _note.Text = _firstRun
                ? "First run: create an account for authorized hotel staff."
                : "Sign in with your hotel staff account.";
            _displayName.Visible = _firstRun;
            _confirmPassword.Visible = _firstRun;
            Controls.OfType<Label>().FirstOrDefault(x => x.Text == "Display name")!.Visible = _firstRun;
            Controls.OfType<Label>().FirstOrDefault(x => x.Text == "Confirm password")!.Visible = _firstRun;
            if (!_firstRun)
            {
                _password.Top = 275;
                _submit.Top = 360;
                Controls.OfType<Label>().First(x => x.Text == "Password").Top = 253;
                _username.Focus();
            }
            else _username.Focus();
            _submit.Text = _firstRun ? "Create account and continue" : "Sign in";
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

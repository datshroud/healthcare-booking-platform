using System;
using System.Drawing;
using System.Net.Http;
using System.Net.Http.Json;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using BookingCareManagement.WinForms.Shared.Models;
using BookingCareManagement.WinForms.Shared.State;

namespace BookingCareManagement.WinForms.Areas.Account.Forms
{
    public class AccountEditorForm : Form
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly SessionState _session;

        private TextBox txtFirstName = null!;
        private TextBox txtLastName = null!;
        private TextBox txtFullName = null!;
        private TextBox txtEmail = null!;
        private DateTimePicker dtpDob = null!;
        private TextBox txtCurrentPassword = null!;
        private TextBox txtPassword = null!;
        private TextBox txtConfirmPassword = null!;
        private Button btnSave = null!;
        private Button btnCancel = null!;

        public AccountEditorForm(IHttpClientFactory httpFactory, SessionState session)
        {
            _httpFactory = httpFactory;
            _session = session;

            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Text = "Cài đặt tài khoản";
            this.Width = 520;
            this.Height = 420;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;

            var tabs = new TabControl
            {
                Dock = DockStyle.Fill,
                Padding = new Point(12, 8)
            };

            // Thông tin cá nhân
            var profilePage = new TabPage("Thông tin cá nhân");
            var profileLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 6,
                Padding = new Padding(12),
                AutoSize = false,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };

            profileLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
            profileLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            var lblFirst = new Label { Text = "Tên", Anchor = AnchorStyles.Left, AutoSize = true };
            txtFirstName = new TextBox { Dock = DockStyle.Fill, Margin = new Padding(0, 6, 0, 6) };

            var lblLast = new Label { Text = "Họ", Anchor = AnchorStyles.Left, AutoSize = true };
            txtLastName = new TextBox { Dock = DockStyle.Fill, Margin = new Padding(0, 6, 0, 6) };

            var lblFull = new Label { Text = "Tên hiển thị", Anchor = AnchorStyles.Left, AutoSize = true };
            txtFullName = new TextBox { Dock = DockStyle.Fill, Margin = new Padding(0, 6, 0, 6) };

            var lblEmail = new Label { Text = "Email", Anchor = AnchorStyles.Left, AutoSize = true };
            txtEmail = new TextBox { Dock = DockStyle.Fill, Margin = new Padding(0, 6, 0, 6) };

            var lblDob = new Label { Text = "Ngày sinh", Anchor = AnchorStyles.Left, AutoSize = true };
            dtpDob = new DateTimePicker { Dock = DockStyle.Fill, Format = DateTimePickerFormat.Short, ShowCheckBox = true, Margin = new Padding(0, 6, 0, 6) };

            profileLayout.Controls.Add(lblFirst, 0, 0);
            profileLayout.Controls.Add(txtFirstName, 1, 0);
            profileLayout.Controls.Add(lblLast, 0, 1);
            profileLayout.Controls.Add(txtLastName, 1, 1);
            profileLayout.Controls.Add(lblFull, 0, 2);
            profileLayout.Controls.Add(txtFullName, 1, 2);
            profileLayout.Controls.Add(lblEmail, 0, 3);
            profileLayout.Controls.Add(txtEmail, 1, 3);
            profileLayout.Controls.Add(lblDob, 0, 4);
            profileLayout.Controls.Add(dtpDob, 1, 4);

            profileLayout.RowStyles.Clear();
            for (int i = 0; i < profileLayout.RowCount - 1; i++)
            {
                profileLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            }
            profileLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            profilePage.Controls.Add(profileLayout);

            // Bảo mật
            var securityPage = new TabPage("Bảo mật");
            var securityLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 4,
                Padding = new Padding(12),
                AutoSize = false,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };

            securityLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
            securityLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            var lblCurrentPassword = new Label { Text = "Mật khẩu hiện tại", Anchor = AnchorStyles.Left, AutoSize = true };
            txtCurrentPassword = new TextBox { Dock = DockStyle.Fill, UseSystemPasswordChar = true, Margin = new Padding(0, 6, 0, 6) };

            var lblPassword = new Label { Text = "Mật khẩu mới", Anchor = AnchorStyles.Left, AutoSize = true };
            txtPassword = new TextBox { Dock = DockStyle.Fill, UseSystemPasswordChar = true, Margin = new Padding(0, 6, 0, 6) };

            var lblConfirm = new Label { Text = "Xác nhận", Anchor = AnchorStyles.Left, AutoSize = true };
            txtConfirmPassword = new TextBox { Dock = DockStyle.Fill, UseSystemPasswordChar = true, Margin = new Padding(0, 6, 0, 6) };

            securityLayout.Controls.Add(lblCurrentPassword, 0, 0);
            securityLayout.Controls.Add(txtCurrentPassword, 1, 0);
            securityLayout.Controls.Add(lblPassword, 0, 1);
            securityLayout.Controls.Add(txtPassword, 1, 1);
            securityLayout.Controls.Add(lblConfirm, 0, 2);
            securityLayout.Controls.Add(txtConfirmPassword, 1, 2);

            securityLayout.RowStyles.Clear();
            for (int i = 0; i < securityLayout.RowCount - 1; i++)
            {
                securityLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            }
            securityLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            securityPage.Controls.Add(securityLayout);

            tabs.TabPages.Add(profilePage);
            tabs.TabPages.Add(securityPage);

            btnSave = new Button { Text = "Lưu", AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink };
            btnCancel = new Button { Text = "Hủy", AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink };

            btnSave.Click += async (s, e) => await SaveAsync();
            btnCancel.Click += (s, e) => this.Close();

            var actions = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(12),
                AutoSize = true
            };
            actions.Controls.Add(btnCancel);
            actions.Controls.Add(btnSave);

            this.Controls.Add(tabs);
            this.Controls.Add(actions);

            this.Load += AccountEditorForm_Load;
        }

        private void AccountEditorForm_Load(object? sender, EventArgs e)
        {
            // Pre-fill fields from session
            var firstName = _session.FirstName;
            var lastName = _session.LastName;
            if (string.IsNullOrWhiteSpace(firstName) && string.IsNullOrWhiteSpace(lastName) && !string.IsNullOrWhiteSpace(_session.DisplayName))
            {
                var parts = _session.DisplayName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 0)
                {
                    firstName = parts[0];
                }

                if (parts.Length > 1)
                {
                    lastName = string.Join(" ", parts.Skip(1));
                }
            }

            var preferredFullName = string.Join(" ", new[] { firstName, lastName }.Where(p => !string.IsNullOrWhiteSpace(p))).Trim();
            txtFullName.Text = string.IsNullOrWhiteSpace(preferredFullName) ? _session.DisplayName : preferredFullName;
            txtEmail.Text = _session.Email;
            txtFirstName.Text = firstName;
            txtLastName.Text = lastName;
            if (_session.DateOfBirth.HasValue)
            {
                dtpDob.Value = _session.DateOfBirth.Value;
                dtpDob.Checked = true;
            }
        }

        private async Task SaveAsync()
        {
            btnSave.Enabled = false;
            try
            {
                UserProfileDto? returned = null;
                var client = _httpFactory.CreateClient("BookingCareApi");
                // Build payload matching web API expectations (split first/last)
                var payload = new UserProfileDto
                {
                    UserId = _session.CurrentUserId ?? string.Empty,
                    FirstName = txtFirstName.Text ?? string.Empty,
                    LastName = txtLastName.Text ?? string.Empty,
                    FullName = txtFullName.Text ?? string.Empty,
                    Email = txtEmail.Text ?? string.Empty,
                    DateOfBirth = dtpDob.Checked ? dtpDob.Value.Date : null,
                    IsAdmin = _session.IsAdmin,
                    IsDoctor = _session.IsDoctor,
                    Roles = _session.Roles as string[] ?? Array.Empty<string>()
                };

                try
                {
                        var resp = await client.PutAsJsonAsync("api/account/auth/profile", payload);
                        var respText = await resp.Content.ReadAsStringAsync();
                        if (!resp.IsSuccessStatusCode)
                        {
                            MessageBox.Show($"Lưu không thành công: {(int)resp.StatusCode}\n{respText}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        // Try to parse returned updated profile so we get AvatarUrl / First/Last names from server
                        try
                        {
                            returned = System.Text.Json.JsonSerializer.Deserialize<UserProfileDto>(respText, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        }
                        catch { }

                    // If password change requested, call endpoint
                    var anyPasswordInput = !string.IsNullOrWhiteSpace(txtCurrentPassword.Text)
                        || !string.IsNullOrWhiteSpace(txtPassword.Text)
                        || !string.IsNullOrWhiteSpace(txtConfirmPassword.Text);

                    if (anyPasswordInput)
                    {
                        if (string.IsNullOrWhiteSpace(txtCurrentPassword.Text))
                        {
                            MessageBox.Show("Vui lòng nhập mật khẩu hiện tại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        if (string.IsNullOrWhiteSpace(txtPassword.Text))
                        {
                            MessageBox.Show("Vui lòng nhập mật khẩu mới.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        if (txtPassword.Text != txtConfirmPassword.Text)
                        {
                            MessageBox.Show("Mật khẩu xác nhận không khớp.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        var pwdResp = await client.PostAsJsonAsync("api/account/auth/change-password", new { currentPassword = txtCurrentPassword.Text, newPassword = txtPassword.Text });
                        if (!pwdResp.IsSuccessStatusCode)
                        {
                            var ptxt = await pwdResp.Content.ReadAsStringAsync();
                            MessageBox.Show($"Đổi mật khẩu thất bại: {(int)pwdResp.StatusCode}\n{ptxt}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lưu thất bại: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Update local session with server-returned profile when possible
                if (returned is not null)
                {
                    _session.ApplyProfile(returned);
                }
                else
                {
                    _session.ApplyProfile(payload);
                }
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            finally
            {
                btnSave.Enabled = true;
            }
        }
    }
}

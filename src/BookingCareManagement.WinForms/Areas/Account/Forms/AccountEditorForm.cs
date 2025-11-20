using System;
using System.Net.Http;
using System.Net.Http.Json;
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
            this.Width = 420;
            this.Height = 360;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;

            var lblFirst = new Label { Text = "Tên", Left = 12, Top = 18, Width = 80 };
            txtFirstName = new TextBox { Left = 100, Top = 15, Width = 120 };

            var lblLast = new Label { Text = "Họ", Left = 230, Top = 18, Width = 40 };
            txtLastName = new TextBox { Left = 270, Top = 15, Width = 90 };

            var lblFull = new Label { Text = "Tên hiển thị", Left = 12, Top = 58, Width = 80 };
            txtFullName = new TextBox { Left = 100, Top = 55, Width = 260 };

            var lblEmail = new Label { Text = "Email", Left = 12, Top = 98, Width = 80 };
            txtEmail = new TextBox { Left = 100, Top = 95, Width = 260 };

            var lblDob = new Label { Text = "Ngày sinh", Left = 12, Top = 138, Width = 80 };
            dtpDob = new DateTimePicker { Left = 100, Top = 135, Width = 200, Format = DateTimePickerFormat.Short, ShowCheckBox = true };

            var lblPassword = new Label { Text = "Mật khẩu mới", Left = 12, Top = 178, Width = 80 };
            txtPassword = new TextBox { Left = 100, Top = 175, Width = 260, UseSystemPasswordChar = true };

            var lblConfirm = new Label { Text = "Xác nhận", Left = 12, Top = 218, Width = 80 };
            txtConfirmPassword = new TextBox { Left = 100, Top = 215, Width = 260, UseSystemPasswordChar = true };

            btnSave = new Button { Text = "Lưu", Left = 200, Top = 255, Width = 80 };
            btnCancel = new Button { Text = "Hủy", Left = 290, Top = 255, Width = 80 };

            btnSave.Click += async (s, e) => await SaveAsync();
            btnCancel.Click += (s, e) => this.Close();

            this.Controls.Add(lblFirst);
            this.Controls.Add(txtFirstName);
            this.Controls.Add(lblLast);
            this.Controls.Add(txtLastName);
            this.Controls.Add(lblFull);
            this.Controls.Add(txtFullName);
            this.Controls.Add(lblEmail);
            this.Controls.Add(txtEmail);
            this.Controls.Add(lblDob);
            this.Controls.Add(dtpDob);
            this.Controls.Add(lblPassword);
            this.Controls.Add(txtPassword);
            this.Controls.Add(lblConfirm);
            this.Controls.Add(txtConfirmPassword);
            this.Controls.Add(btnSave);
            this.Controls.Add(btnCancel);

            this.Load += AccountEditorForm_Load;
        }

        private void AccountEditorForm_Load(object? sender, EventArgs e)
        {
            // Pre-fill fields from session
            txtFullName.Text = _session.DisplayName;
            txtEmail.Text = _session.Email;
            txtFirstName.Text = _session.FirstName;
            txtLastName.Text = _session.LastName;
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
                    if (!string.IsNullOrWhiteSpace(txtPassword.Text))
                    {
                        if (txtPassword.Text != txtConfirmPassword.Text)
                        {
                            MessageBox.Show("Mật khẩu xác nhận không khớp.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        var pwdResp = await client.PostAsJsonAsync("api/account/auth/change-password", new { newPassword = txtPassword.Text });
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

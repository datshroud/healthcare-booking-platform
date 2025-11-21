using System;
using System;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;

namespace BookingCareManagement.WinForms.Areas.Account.Forms
{
    public partial class Login : Form
    {
        private readonly IServiceProvider _services;

        public Login(IServiceProvider services)
        {
            _services = services;
            InitializeComponent();

            void PreventNewLines(TextBox textBox)
            {
                textBox.Multiline = false;
                textBox.AcceptsReturn = false;
                textBox.AcceptsTab = false;
                textBox.TextChanged += (_, _) =>
                {
                    var current = textBox.Text;
                    if (string.IsNullOrEmpty(current))
                    {
                        return;
                    }

                    var sanitized = current
                        .Replace("\r", string.Empty)
                        .Replace("\n", string.Empty);

                    if (sanitized != current)
                    {
                        var caret = textBox.SelectionStart;
                        textBox.Text = sanitized;
                        textBox.SelectionStart = Math.Min(caret, textBox.Text.Length);
                    }
                };
            }

            PreventNewLines(textBoxUsername);
            PreventNewLines(textBoxPassword);

            // Open register form when user clicks the link. Hide this login form
            // while register dialog is shown, then show it again when register closes.
            linkLabelRegister.LinkClicked += (_, _) =>
            {
                try
                {
                    this.Hide();
                    using var r = _services.GetRequiredService<Register>();
                    r.ShowDialog();
                }
                catch
                {
                    // swallow in case DI not configured during tests
                }
                finally
                {
                    // When register dialog closes, restore this login form.
                    try { this.Show(); } catch { }
                }
            };

            // Wire login button to call AuthService
            buttonLogin.Click += async (_, _) =>
            {
                try { System.IO.File.AppendAllText("debug_winforms.log", $"[{DateTime.Now:O}] Login: button clicked\n"); } catch {}
                var dialogs = _services.GetRequiredService<BookingCareManagement.WinForms.Shared.Services.DialogService>();
                var auth = _services.GetRequiredService<BookingCareManagement.WinForms.Shared.Services.AuthService>();
                try
                {
                    buttonLogin.Enabled = false;
                    var req = new BookingCareManagement.WinForms.Shared.Models.LoginRequestDto
                    {
                        Email = textBoxUsername.Text?.Trim(),
                        Password = textBoxPassword.Text
                    };

                    var ok = await auth.LoginAsync(req);
                    if (ok)
                    {
                        try { System.IO.File.AppendAllText("debug_winforms.log", $"[{DateTime.Now:O}] Login: succeeded\n"); } catch {}
                        Console.WriteLine("[Login] Login succeeded, closing dialog.");
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    else
                    {
                        dialogs.ShowError("Đăng nhập thất bại. Vui lòng kiểm tra email và mật khẩu.");
                    }
                }
                catch (Exception ex)
                {
                    try { _services.GetRequiredService<BookingCareManagement.WinForms.Shared.Services.DialogService>().ShowError(ex.Message); } catch { }
                }
                finally
                {
                    try { buttonLogin.Enabled = true; } catch { }
                }
            };
        }
    }
}

using System;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;

namespace BookingCareManagement.WinForms.Areas.Account.Forms
{
    public partial class Register : Form
    {
        private readonly IServiceProvider _services;

        public Register(IServiceProvider services)
        {
            _services = services;
            InitializeComponent();

            // When user clicks "Back to Login", close this dialog so
            // the previously-hidden login form can be restored.
            try
            {
                linkLabelBackToLogin.LinkClicked += (_, _) =>
                {
                    this.Close();
                };
            }
            catch
            {
                // ignore if designer control missing in some tests
            }

            // Wire register button to call AuthService
            try
            {
                buttonRegister.Click += async (_, _) =>
                {
                    var auth = _services.GetRequiredService<BookingCareManagement.WinForms.Shared.Services.AuthService>();
                    var dialogs = _services.GetRequiredService<BookingCareManagement.WinForms.Shared.Services.DialogService>();
                    try
                    {
                        buttonRegister.Enabled = false;

                        if (textBoxPassword.Text != textBoxConfirmPassword.Text)
                        {
                            dialogs.ShowError("Mật khẩu xác nhận không khớp.");
                            return;
                        }

                        var req = new BookingCareManagement.WinForms.Shared.Models.RegisterRequestDto
                        {
                            FirstName = textBoxFirstName.Text?.Trim(),
                            LastName = textBoxLastName.Text?.Trim(),
                            Email = textBoxEmail.Text?.Trim(),
                            Password = textBoxPassword.Text,
                            PhoneNumber = textBoxPhone.Text?.Trim(),
                            DateOfBirth = dateTimePickerBirthDate.Value
                        };

                        var ok = await auth.RegisterAsync(req);
                        if (ok)
                        {
                            dialogs.ShowInfo("Đăng ký thành công.");
                            this.DialogResult = DialogResult.OK;
                            this.Close();
                        }
                        else
                        {
                            dialogs.ShowError("Đăng ký thất bại.");
                        }
                    }
                    catch (Exception ex)
                    {
                        dialogs.ShowError(ex.Message);
                    }
                    finally
                    {
                        try { buttonRegister.Enabled = true; } catch { }
                    }
                };
            }
            catch
            {
            }
        }
    }
}

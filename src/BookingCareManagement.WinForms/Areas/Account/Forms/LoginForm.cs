using System.Threading.Tasks;
using System.Windows.Forms;
using BookingCareManagement.WinForms.Areas.Account.Controllers;
using BookingCareManagement.WinForms.Areas.Account.ViewModels;

namespace BookingCareManagement.WinForms.Areas.Account.Forms;

public sealed class LoginForm : Form
{
    private readonly AuthController _authController;
    private readonly LoginViewModel _viewModel;
    private readonly TextBox _userNameInput = new() { PlaceholderText = "Tên đăng nhập" };
    private readonly TextBox _passwordInput = new() { PlaceholderText = "Mật khẩu", UseSystemPasswordChar = true };
    private readonly Button _loginButton = new() { Text = "Đăng nhập" };

    public LoginForm(AuthController authController, LoginViewModel viewModel)
    {
        _authController = authController;
        _viewModel = viewModel;

        Text = "Đăng nhập hệ thống";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        Width = 400;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(24),
            RowCount = 4,
            ColumnCount = 1
        };

        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        layout.Controls.Add(_userNameInput, 0, 0);
        layout.Controls.Add(_passwordInput, 0, 1);
        layout.Controls.Add(_loginButton, 0, 2);
        Controls.Add(layout);

        _loginButton.Click += async (_, _) => await AttemptLoginAsync();
    }

    private async Task AttemptLoginAsync()
    {
        _viewModel.UserName = _userNameInput.Text;
        _viewModel.Password = _passwordInput.Text;

        _loginButton.Enabled = false;
        try
        {
            if (await _authController.LoginAsync())
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }
        finally
        {
            _loginButton.Enabled = true;
        }
    }
}

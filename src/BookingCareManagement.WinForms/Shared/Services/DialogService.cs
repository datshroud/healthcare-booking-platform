using System.Windows.Forms;

namespace BookingCareManagement.WinForms.Shared.Services;

public sealed class DialogService
{
    public void ShowError(string message, string title = "Có lỗi xảy ra")
    {
        MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    public void ShowInfo(string message, string title = "Thông báo")
    {
        MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    public bool Confirm(string message, string title = "Xác nhận")
    {
        return MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
    }
}

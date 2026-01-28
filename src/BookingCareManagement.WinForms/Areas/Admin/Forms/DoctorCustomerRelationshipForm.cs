using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using BookingCareManagement.WinForms.Areas.Admin.Controls;

namespace BookingCareManagement.WinForms.Areas.Admin.Forms;

public sealed class DoctorCustomerRelationshipForm : Form
{
    private readonly DoctorCustomerRelationshipControl _control;

    public DoctorCustomerRelationshipForm(DoctorCustomerRelationshipControl control)
    {
        _control = control;

        Text = "Quan hệ bác sĩ - khách hàng";
        BackColor = Color.FromArgb(243, 244, 246);
        FormBorderStyle = FormBorderStyle.None;
        TopLevel = false;

        _control.Dock = DockStyle.Fill;
        Controls.Add(_control);

        Shown += async (_, _) => await InitializeAsync();
    }

    private Task InitializeAsync()
    {
        return _control.InitializeAsync();
    }
}

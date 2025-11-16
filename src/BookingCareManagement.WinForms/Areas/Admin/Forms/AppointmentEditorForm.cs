using System;
using System.Drawing;
using System.Windows.Forms;

namespace BookingCareManagement.WinForms.Areas.Admin.Forms
{
    public sealed partial class AppointmentEditorForm : Form
    {
        public AppointmentEditorForm()
        {
            InitializeComponent();
            InitializeGridColumns();
            ApplyGridStyling();
            LoadSampleData();
        }

        private void InitializeGridColumns()
        {
            appointmentGrid.Columns.Clear();

            // Checkbox column
            appointmentGrid.Columns.Add(new DataGridViewCheckBoxColumn
            {
                Name = "Select",
                HeaderText = "",
                FillWeight = 5
            });

            // Other columns
            appointmentGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Time", HeaderText = "Time", FillWeight = 10 });
            appointmentGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Service", HeaderText = "Service", FillWeight = 15 });
            appointmentGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Customer", HeaderText = "Customers", FillWeight = 25 });
            appointmentGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Duration", HeaderText = "Duration", FillWeight = 10 });
            appointmentGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", FillWeight = 15 });
            appointmentGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Employee", HeaderText = "Employee", FillWeight = 10 });
            appointmentGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Note", HeaderText = "Note", FillWeight = 10 });
            appointmentGrid.Columns.Add(new DataGridViewButtonColumn
            {
                Name = "Action",
                HeaderText = "",
                Text = "...",
                UseColumnTextForButtonValue = true,
                FillWeight = 5
            });
        }

        private void ApplyGridStyling()
        {
            // Column header style
            appointmentGrid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.White,
                ForeColor = Color.FromArgb(107, 114, 128),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 0, 0)
            };

            // Default cell style
            appointmentGrid.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.White,
                ForeColor = Color.FromArgb(17, 24, 39),
                SelectionBackColor = Color.FromArgb(243, 244, 246),
                SelectionForeColor = Color.FromArgb(17, 24, 39),
                Padding = new Padding(15, 10, 0, 10),
                Font = new Font("Segoe UI", 10F)
            };

            // Alternating row style
            appointmentGrid.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(249, 250, 251)
            };
        }

        private void LoadSampleData()
        {
            // Load demo data giống web
            appointmentGrid.Rows.Clear();
            appointmentGrid.Rows.Add(false, "4:00 pm", "Check Up", ". Le Ngoc Bao Chan", "1h", "Approved", "C", "", "...");
            appointmentGrid.Rows.Add(false, "2:00 pm", "Check Up", ". Le Ngoc Bao Chan", "1h", "Approved", "C", "", "...");

            // Update title với số lượng
            lblTitle.Text = $"Appointments ({appointmentGrid.Rows.Count})";
        }
    }
}

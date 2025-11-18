using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace BookingCareManagement.WinForms.Areas.Customer.Forms
{
    // Đã đổi tên class từ Appointment thành Bookings
    public partial class Bookings : Form
    {
        private string selectedSpecialty = "";
        private string selectedDoctor = "";
        private decimal selectedPrice = 0;
        private string selectedDate = "";
        private string selectedTime = "";

        private List<string> specialties;
        private Dictionary<string, List<DoctorInfo>> doctorsBySpecialty;
        private List<string> timeSlots;

        // Đã đổi tên Constructor từ Appointment thành Bookings
        public Bookings()
        {
            InitializeComponent();
            InitializeData();

            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = true;
            this.MinimumSize = new Size(800, 600);

            // Cập nhật tên sự kiện Load và Resize
            this.Load += Bookings_Load;
            this.Resize += Bookings_Resize;

            this.buttonBookAppointment.Click += ButtonBookAppointment_Click;
            this.dateTimePickerAppointment.ValueChanged += DateTimePickerAppointment_ValueChanged;
            this.textBoxPhone.KeyPress += TextBoxPhone_KeyPress;
            this.textBoxFullName.TextChanged += TextBoxCustomerInfo_Changed;
            this.textBoxPhone.TextChanged += TextBoxCustomerInfo_Changed;
            this.textBoxEmail.TextChanged += TextBoxCustomerInfo_Changed;

            dateTimePickerAppointment.MinDate = DateTime.Now;
            dateTimePickerAppointment.MaxDate = DateTime.Now.AddMonths(3);
            selectedDate = dateTimePickerAppointment.Value.ToString("dd/MM/yyyy");

            LoadSpecialties();
        }

        // Đã đổi tên hàm xử lý sự kiện Load
        private void Bookings_Load(object sender, EventArgs e)
        {
            ResponsiveLayout();
        }

        // Đã đổi tên hàm xử lý sự kiện Resize
        private void Bookings_Resize(object sender, EventArgs e)
        {
            ResponsiveLayout();
        }

        private void ResponsiveLayout()
        {
            int maxContentWidth = 1000;
            int currentWidth = panelMain.Width;

            if (currentWidth > maxContentWidth)
            {
                int padding = (currentWidth - maxContentWidth) / 2;
                panelMain.Padding = new Padding(padding, 20, padding, 20);
            }
            else
            {
                panelMain.Padding = new Padding(20);
            }
        }

        private void InitializeData()
        {
            specialties = new List<string>
            {
                "Nha khoa", "Tim mạch", "Nội khoa", "Ngoại khoa",
                "Sản phụ khoa", "Nhi khoa", "Da liễu", "Mắt"
            };

            doctorsBySpecialty = new Dictionary<string, List<DoctorInfo>>
            {
                { "Nha khoa", new List<DoctorInfo> {
                    new DoctorInfo { Name = "BS. Nguyễn Văn A", Specialty = "Nha khoa", Experience = "10 năm", Price = 200000 },
                    new DoctorInfo { Name = "BS. Trần Thị B", Specialty = "Nha khoa", Experience = "8 năm", Price = 150000 },
                    new DoctorInfo { Name = "BS. Lê Văn C", Specialty = "Nha khoa", Experience = "12 năm", Price = 250000 }
                }},
                { "Tim mạch", new List<DoctorInfo> {
                    new DoctorInfo { Name = "BS. Phạm Văn D", Specialty = "Tim mạch", Experience = "15 năm", Price = 500000 },
                    new DoctorInfo { Name = "BS. Hoàng Thị E", Specialty = "Tim mạch", Experience = "11 năm", Price = 450000 }
                }},
                { "Nội khoa", new List<DoctorInfo> {
                    new DoctorInfo { Name = "BS. Đỗ Văn F", Specialty = "Nội khoa", Experience = "9 năm", Price = 200000 },
                    new DoctorInfo { Name = "BS. Vũ Thị G", Specialty = "Nội khoa", Experience = "14 năm", Price = 300000 }
                }},
                { "Ngoại khoa", new List<DoctorInfo> {
                    new DoctorInfo { Name = "BS. Bùi Văn H", Specialty = "Ngoại khoa", Experience = "16 năm", Price = 350000 }
                }},
                { "Sản phụ khoa", new List<DoctorInfo> {
                    new DoctorInfo { Name = "BS. Đinh Thị I", Specialty = "Sản phụ khoa", Experience = "13 năm", Price = 300000 }
                }},
                { "Nhi khoa", new List<DoctorInfo> {
                    new DoctorInfo { Name = "BS. Ngô Văn K", Specialty = "Nhi khoa", Experience = "10 năm", Price = 200000 }
                }},
                { "Da liễu", new List<DoctorInfo> {
                    new DoctorInfo { Name = "BS. Phan Thị L", Specialty = "Da liễu", Experience = "7 năm", Price = 250000 }
                }},
                { "Mắt", new List<DoctorInfo> {
                    new DoctorInfo { Name = "BS. Mai Văn M", Specialty = "Mắt", Experience = "11 năm", Price = 220000 }
                }}
            };

            timeSlots = new List<string>
            {
                "08:00", "08:30", "09:00", "09:30", "10:00", "10:30", "11:00", "11:30",
                "14:00", "14:30", "15:00", "15:30", "16:00", "16:30", "17:00", "17:30"
            };
        }

        private void LoadSpecialties()
        {
            flowLayoutPanelSpecialties.Controls.Clear();

            foreach (var specialty in specialties)
            {
                Button btnSpecialty = new Button
                {
                    Text = specialty,
                    Size = new Size(200, 50),
                    Font = new Font("Segoe UI", 11),
                    ForeColor = Color.FromArgb(66, 66, 66),
                    BackColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand,
                    Margin = new Padding(10),
                    Tag = specialty
                };
                btnSpecialty.FlatAppearance.BorderColor = Color.FromArgb(220, 220, 220);
                btnSpecialty.Click += BtnSpecialty_Click;

                flowLayoutPanelSpecialties.Controls.Add(btnSpecialty);
            }
        }

        private void BtnSpecialty_Click(object sender, EventArgs e)
        {
            ResetStep2AndAfter();

            Button clickedButton = (Button)sender;
            selectedSpecialty = clickedButton.Tag.ToString();

            foreach (Button btn in flowLayoutPanelSpecialties.Controls)
            {
                btn.BackColor = Color.White;
                btn.ForeColor = Color.FromArgb(66, 66, 66);
            }

            clickedButton.BackColor = Color.FromArgb(23, 162, 184);
            clickedButton.ForeColor = Color.White;

            groupBoxStep2.Enabled = true;
            LoadDoctors(selectedSpecialty);
        }

        private void LoadDoctors(string specialty)
        {
            flowLayoutPanelDoctors.Controls.Clear();
            flowLayoutPanelDoctors.SuspendLayout();

            if (doctorsBySpecialty.ContainsKey(specialty))
            {
                foreach (var doctor in doctorsBySpecialty[specialty])
                {
                    Panel doctorPanel = CreateDoctorCard(doctor);
                    flowLayoutPanelDoctors.Controls.Add(doctorPanel);
                }
            }

            flowLayoutPanelDoctors.ResumeLayout();
        }

        private Panel CreateDoctorCard(DoctorInfo doctor)
        {
            Panel panel = new Panel
            {
                Size = new Size(280, 100),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Cursor = Cursors.Hand,
                Margin = new Padding(10),
                Tag = doctor
            };

            // Tên Bác sĩ
            Label lblName = new Label
            {
                Text = doctor.Name,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 33, 33),
                Location = new Point(10, 10),
                AutoSize = true
            };

            // Chuyên khoa
            Label lblSpecialty = new Label
            {
                Text = $"Chuyên khoa: {doctor.Specialty}",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                Location = new Point(10, 40),
                AutoSize = true
            };

            // Kinh nghiệm
            Label lblExperience = new Label
            {
                Text = $"Kinh nghiệm: {doctor.Experience}",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                Location = new Point(10, 65),
                AutoSize = true
            };


            Label lblPrice = new Label
            {
                Text = $"{doctor.Price:N0}đ",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 33, 33),
                Location = new Point(160, 65),
                AutoSize = false,
                Size = new Size(110, 25),
                TextAlign = ContentAlignment.MiddleRight
            };

            panel.Controls.Add(lblName);
            panel.Controls.Add(lblSpecialty);
            panel.Controls.Add(lblExperience);
            panel.Controls.Add(lblPrice);

            panel.Click += DoctorPanel_Click;
            lblName.Click += DoctorPanel_Click;
            lblSpecialty.Click += DoctorPanel_Click;
            lblExperience.Click += DoctorPanel_Click;
            lblPrice.Click += DoctorPanel_Click;

            return panel;
        }

        private void DoctorPanel_Click(object sender, EventArgs e)
        {
            Control control = (Control)sender;
            Panel panel = control as Panel ?? control.Parent as Panel;

            if (panel != null && panel.Tag is DoctorInfo)
            {
                DoctorInfo doctor = (DoctorInfo)panel.Tag;
                selectedDoctor = doctor.Name;
                selectedPrice = doctor.Price;

                foreach (Control c in flowLayoutPanelDoctors.Controls)
                {
                    if (c is Panel p) p.BackColor = Color.White;
                }

                panel.BackColor = Color.FromArgb(230, 247, 255);

                ResetStep3AndAfter();
                groupBoxStep3.Enabled = true;
                LoadTimeSlots();
            }
        }

        private void DateTimePickerAppointment_ValueChanged(object sender, EventArgs e)
        {
            selectedDate = dateTimePickerAppointment.Value.ToString("dd/MM/yyyy");
            LoadTimeSlots();
            UpdateSummary();
        }

        private void LoadTimeSlots()
        {
            flowLayoutPanelTimeSlots.Controls.Clear();
            if (timeSlots == null) return;

            foreach (var time in timeSlots)
            {
                Button btnTime = new Button
                {
                    Text = time,
                    Size = new Size(110, 45),
                    Font = new Font("Segoe UI", 10),
                    ForeColor = Color.FromArgb(66, 66, 66),
                    BackColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand,
                    Margin = new Padding(8),
                    Tag = time
                };
                btnTime.FlatAppearance.BorderColor = Color.FromArgb(220, 220, 220);
                btnTime.Click += BtnTime_Click;

                flowLayoutPanelTimeSlots.Controls.Add(btnTime);
            }
        }

        private void BtnTime_Click(object sender, EventArgs e)
        {
            Button clickedButton = (Button)sender;
            selectedTime = clickedButton.Tag.ToString();

            foreach (Button btn in flowLayoutPanelTimeSlots.Controls)
            {
                btn.BackColor = Color.White;
                btn.ForeColor = Color.FromArgb(66, 66, 66);
            }

            clickedButton.BackColor = Color.FromArgb(23, 162, 184);
            clickedButton.ForeColor = Color.White;

            groupBoxStep4.Enabled = true;
            textBoxFullName.Focus();

            UpdateSummary();
            CheckEnableBookButton();
        }

        private void TextBoxPhone_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void TextBoxCustomerInfo_Changed(object sender, EventArgs e)
        {
            UpdateSummary();
            CheckEnableBookButton();
        }

        private void CheckEnableBookButton()
        {
            bool allFieldsFilled = !string.IsNullOrWhiteSpace(selectedSpecialty) &&
                                  !string.IsNullOrWhiteSpace(selectedDoctor) &&
                                  !string.IsNullOrWhiteSpace(selectedTime) &&
                                  !string.IsNullOrWhiteSpace(textBoxFullName.Text) &&
                                  !string.IsNullOrWhiteSpace(textBoxPhone.Text) &&
                                  !string.IsNullOrWhiteSpace(textBoxEmail.Text);

            buttonBookAppointment.Enabled = allFieldsFilled;
            buttonBookAppointment.BackColor = allFieldsFilled ?
                Color.FromArgb(23, 162, 184) : Color.Gray;
        }

        private void UpdateSummary()
        {
            if (!string.IsNullOrEmpty(selectedSpecialty) && !string.IsNullOrEmpty(selectedDoctor))
            {
                string summary = $"Chuyên khoa: {selectedSpecialty} | Bác sĩ: {selectedDoctor}";
                if (!string.IsNullOrEmpty(selectedTime))
                {
                    summary += $" | Ngày: {dateTimePickerAppointment.Value:dd/MM/yyyy} - Giờ: {selectedTime}";
                }
                labelSummary.Text = summary;
            }
            else
            {
                labelSummary.Text = "Vui lòng hoàn thành các bước để đặt lịch";
            }
        }

        private void ButtonBookAppointment_Click(object sender, EventArgs e)
        {
            if (!ValidateCustomerInfo()) return;

            string appointmentInfo = $"THÔNG TIN LỊCH HẸN\n\n" +
                                    $"Chuyên khoa: {selectedSpecialty}\n" +
                                    $"Bác sĩ: {selectedDoctor}\n" +
                                    $"Phí khám: {selectedPrice:N0} VNĐ\n" +
                                    $"Ngày khám: {dateTimePickerAppointment.Value:dd/MM/yyyy}\n" +
                                    $"Giờ khám: {selectedTime}\n\n" +
                                    $"THÔNG TIN KHÁCH HÀNG\n\n" +
                                    $"Họ tên: {textBoxFullName.Text}\n" +
                                    $"Số điện thoại: {textBoxPhone.Text}\n" +
                                    $"Email: {textBoxEmail.Text}";

            DialogResult result = MessageBox.Show(
                appointmentInfo + "\n\nXác nhận đặt lịch hẹn?", "Xác nhận đặt lịch",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                if (SaveAppointment())
                {
                    MessageBox.Show("Đặt lịch hẹn thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Có lỗi xảy ra!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private bool ValidateCustomerInfo()
        {
            if (string.IsNullOrWhiteSpace(textBoxFullName.Text))
            {
                MessageBox.Show("Vui lòng nhập họ tên!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxFullName.Focus(); return false;
            }
            if (string.IsNullOrWhiteSpace(textBoxPhone.Text))
            {
                MessageBox.Show("Vui lòng nhập số điện thoại!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxPhone.Focus(); return false;
            }
            if (!Regex.IsMatch(textBoxPhone.Text, @"^0\d{9,10}$"))
            {
                MessageBox.Show("Số điện thoại không hợp lệ!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxPhone.Focus(); return false;
            }
            if (string.IsNullOrWhiteSpace(textBoxEmail.Text))
            {
                MessageBox.Show("Vui lòng nhập email!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxEmail.Focus(); return false;
            }
            if (!Regex.IsMatch(textBoxEmail.Text, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MessageBox.Show("Email không hợp lệ!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxEmail.Focus(); return false;
            }
            return true;
        }

        private bool SaveAppointment()
        {
            // Logic lưu vào database sẽ nằm ở đây
            return true;
        }

        private void ResetStep2AndAfter()
        {
            selectedDoctor = "";
            selectedPrice = 0;
            flowLayoutPanelDoctors.Controls.Clear();
            groupBoxStep2.Enabled = false;
            ResetStep3AndAfter();
        }

        private void ResetStep3AndAfter()
        {
            selectedTime = "";
            groupBoxStep3.Enabled = false;
            groupBoxStep4.Enabled = false;
            flowLayoutPanelTimeSlots.Controls.Clear();
            textBoxFullName.Clear();
            textBoxPhone.Clear();
            textBoxEmail.Clear();
            buttonBookAppointment.Enabled = false;
            buttonBookAppointment.BackColor = Color.Gray;
            UpdateSummary();
        }
    }

    public class DoctorInfo
    {
        public string Name { get; set; }
        public string Specialty { get; set; }
        public string Experience { get; set; }
        public decimal Price { get; set; }
    }
}
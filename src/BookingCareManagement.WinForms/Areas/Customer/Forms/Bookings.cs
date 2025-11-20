using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace BookingCareManagement.WinForms.Areas.Customer.Forms
{
    public partial class Bookings : Form
    {
        private string selectedSpecialty = "";
        private string selectedEmployee = "";
        private string selectedDate = "";
        private string selectedTime = "";
        private decimal totalPrice = 0;

        private List<SpecialtyData> specialties;
        private List<EmployeeData> employees;
        private List<string> timeSlots;

        private enum BookingStep
        {
            Specialty,
            Employee,
            DateTime,
            Payment,
            ThankYou
        }

        private BookingStep currentStep = BookingStep.Specialty;

        public Bookings()
        {
            InitializeComponent();

            // Thiết lập sự kiện
            this.Load += Bookings_Load;
            this.textBoxSearch.Enter += TextBoxSearch_Enter;
            this.textBoxSearch.Leave += TextBoxSearch_Leave;
            this.textBoxSearch.TextChanged += TextBoxSearch_TextChanged;
            this.buttonBack.Click += ButtonBack_Click;
            this.dateTimePickerAppointment.ValueChanged += DateTimePickerAppointment_ValueChanged;
            this.buttonConfirmDateTime.Click += ButtonConfirmDateTime_Click;
            this.buttonApplyPromo.Click += ButtonApplyPromo_Click;
            this.buttonConfirmBooking.Click += ButtonConfirmBooking_Click;
        }

        private void Bookings_Load(object sender, EventArgs e)
        {
            InitializeData();
            LoadSpecialties();
            dateTimePickerAppointment.MinDate = DateTime.Now;
            dateTimePickerAppointment.MaxDate = DateTime.Now.AddMonths(3);
            dateTimePickerAppointment.Value = DateTime.Now.AddDays(1);
        }

        private void InitializeData()
        {
            // Dữ liệu chuyên khoa
            specialties = new List<SpecialtyData>
            {
                new SpecialtyData { Name = "Nha Khoa", Price = 200000 },
                new SpecialtyData { Name = "Tim Mạch", Price = 500000 },
                new SpecialtyData { Name = "Nội Khoa", Price = 300000 },
                new SpecialtyData { Name = "Ngoại Khoa", Price = 800000 },
                new SpecialtyData { Name = "Sản Phụ Khoa", Price = 400000 },
                new SpecialtyData { Name = "Nhi Khoa", Price = 250000 },
                new SpecialtyData { Name = "Da Liễu", Price = 350000 },
                new SpecialtyData { Name = "Mắt", Price = 280000 }
            };

            // Dữ liệu nhân viên
            employees = new List<EmployeeData>
            {
                new EmployeeData { Name = "BS. Nguyễn Văn A", Specialty = "Nha Khoa" },
                new EmployeeData { Name = "BS. Trần Thị B", Specialty = "Nha Khoa" },
                new EmployeeData { Name = "BS. Phạm Văn C", Specialty = "Tim Mạch" },
                new EmployeeData { Name = "BS. Hoàng Thị D", Specialty = "Nội Khoa" },
                new EmployeeData { Name = "BS. Lê Văn E", Specialty = "Ngoại Khoa" }
            };

            // Khung giờ
            timeSlots = new List<string>
            {
                "08:00", "08:30", "09:00", "09:30", "10:00", "10:30",
                "11:00", "11:30", "14:00", "14:30", "15:00", "15:30",
                "16:00", "16:30", "17:00", "17:30"
            };
        }

        private void LoadSpecialties()
        {
            flowLayoutPanelSpecialties.Controls.Clear();

            foreach (var specialty in specialties)
            {
                Panel card = CreateSpecialtyCard(specialty);
                flowLayoutPanelSpecialties.Controls.Add(card);
            }
        }

        private Panel CreateSpecialtyCard(SpecialtyData specialty)
        {
            Panel panel = new Panel
            {
                Size = new Size(570, 80),
                BackColor = Color.White,
                Margin = new Padding(10),
                Cursor = Cursors.Hand,
                BorderStyle = BorderStyle.FixedSingle,
                Tag = specialty
            };

            Label lblName = new Label
            {
                Text = specialty.Name,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 33, 33),
                Location = new Point(15, 15),
                AutoSize = true
            };

            Label lblPrice = new Label
            {
                Text = $"{specialty.Price:N0} VNĐ",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.FromArgb(255, 165, 0),
                Location = new Point(15, 45),
                AutoSize = true
            };

            panel.Controls.Add(lblName);
            panel.Controls.Add(lblPrice);

            panel.Click += (s, e) => SelectSpecialty(specialty);
            lblName.Click += (s, e) => SelectSpecialty(specialty);
            lblPrice.Click += (s, e) => SelectSpecialty(specialty);

            return panel;
        }

        private void SelectSpecialty(SpecialtyData specialty)
        {
            selectedSpecialty = specialty.Name;
            totalPrice = specialty.Price;

            labelSelectedSpecialtyValue.Text = specialty.Name;
            labelTotalPrice.Text = $"{totalPrice:N0} VNĐ";
            labelCheckoutTotal.Text = $"{totalPrice:N0} VNĐ";
            panelTotalSection.Visible = true;

            // Chuyển sang bước chọn nhân viên
            ShowStep(BookingStep.Employee);
            LoadEmployees(specialty.Name);
        }

        private void LoadEmployees(string specialtyName)
        {
            flowLayoutPanelEmployees.Controls.Clear();

            var filteredEmployees = employees.Where(e => e.Specialty == specialtyName).ToList();

            foreach (var employee in filteredEmployees)
            {
                Panel card = CreateEmployeeCard(employee);
                flowLayoutPanelEmployees.Controls.Add(card);
            }
        }

        private Panel CreateEmployeeCard(EmployeeData employee)
        {
            Panel panel = new Panel
            {
                Size = new Size(570, 80),
                BackColor = Color.White,
                Margin = new Padding(10),
                Cursor = Cursors.Hand,
                BorderStyle = BorderStyle.FixedSingle,
                Tag = employee
            };

            Label lblName = new Label
            {
                Text = employee.Name,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 33, 33),
                Location = new Point(15, 15),
                AutoSize = true
            };

            Label lblSpecialty = new Label
            {
                Text = $"Chuyên khoa: {employee.Specialty}",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gray,
                Location = new Point(15, 45),
                AutoSize = true
            };

            panel.Controls.Add(lblName);
            panel.Controls.Add(lblSpecialty);

            panel.Click += (s, e) => SelectEmployee(employee);
            lblName.Click += (s, e) => SelectEmployee(employee);
            lblSpecialty.Click += (s, e) => SelectEmployee(employee);

            return panel;
        }

        private void SelectEmployee(EmployeeData employee)
        {
            selectedEmployee = employee.Name;
            labelSelectedEmployeeValue.Text = employee.Name;

            // Chuyển sang bước chọn ngày giờ
            ShowStep(BookingStep.DateTime);
            LoadTimeSlots();
        }

        private void DateTimePickerAppointment_ValueChanged(object sender, EventArgs e)
        {
            selectedDate = dateTimePickerAppointment.Value.ToString("dd/MM/yyyy");
            LoadTimeSlots();
        }

        private void LoadTimeSlots()
        {
            comboBoxTimeSlot.Items.Clear();
            comboBoxTimeSlot.Enabled = true;

            foreach (var time in timeSlots)
            {
                comboBoxTimeSlot.Items.Add(time);
            }

            if (comboBoxTimeSlot.Items.Count > 0)
            {
                comboBoxTimeSlot.SelectedIndex = 0;
            }
        }

        private void ButtonConfirmDateTime_Click(object sender, EventArgs e)
        {
            if (comboBoxTimeSlot.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn giờ!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            selectedTime = comboBoxTimeSlot.SelectedItem.ToString();
            selectedDate = dateTimePickerAppointment.Value.ToString("dd/MM/yyyy");

            labelSelectedDateTimeValue.Text = $"{selectedDate} - {selectedTime}";

            // Chuyển sang bước thanh toán
            ShowStep(BookingStep.Payment);
        }

        private void ButtonApplyPromo_Click(object sender, EventArgs e)
        {
            string promoCode = textBoxPromoCode.Text.Trim().ToUpper();

            if (string.IsNullOrEmpty(promoCode))
            {
                MessageBox.Show("Vui lòng nhập mã khuyến mãi!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Giả lập kiểm tra mã
            if (promoCode == "GIAM10")
            {
                decimal discount = totalPrice * 0.1m;
                totalPrice -= discount;
                labelCheckoutTotal.Text = $"{totalPrice:N0} VNĐ";
                labelTotalPrice.Text = $"{totalPrice:N0} VNĐ";

                MessageBox.Show($"Áp dụng mã thành công!\nGiảm giá: {discount:N0} VNĐ",
                    "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Mã khuyến mãi không hợp lệ!",
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ButtonConfirmBooking_Click(object sender, EventArgs e)
        {
            // Validate
            if (string.IsNullOrWhiteSpace(textBoxName.Text))
            {
                MessageBox.Show("Vui lòng nhập họ tên!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(textBoxPhone.Text))
            {
                MessageBox.Show("Vui lòng nhập số điện thoại!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxPhone.Focus();
                return;
            }

            // Lưu đặt lịch (TODO: Kết nối database)
            bool success = SaveBooking();

            if (success)
            {
                ShowStep(BookingStep.ThankYou);
            }
        }

        private bool SaveBooking()
        {
            // TODO: Lưu vào database
            return true;
        }

        private void ShowStep(BookingStep step)
        {
            currentStep = step;

            // Ẩn tất cả panels
            panelSpecialty.Visible = false;
            panelEmployee.Visible = false;
            panelDateTime.Visible = false;
            panelPayment.Visible = false;
            panelThankYou.Visible = false;

            // Hiển thị panel tương ứng
            switch (step)
            {
                case BookingStep.Specialty:
                    panelSpecialty.Visible = true;
                    labelLeftTitle.Text = "Chọn chuyên khoa";
                    buttonBack.Visible = false;
                    textBoxSearch.Visible = true;
                    break;

                case BookingStep.Employee:
                    panelEmployee.Visible = true;
                    labelLeftTitle.Text = "Chọn nhân viên";
                    buttonBack.Visible = true;
                    textBoxSearch.Visible = false;
                    break;

                case BookingStep.DateTime:
                    panelDateTime.Visible = true;
                    labelLeftTitle.Text = "Chọn ngày & giờ";
                    buttonBack.Visible = true;
                    textBoxSearch.Visible = false;
                    break;

                case BookingStep.Payment:
                    panelPayment.Visible = true;
                    labelLeftTitle.Text = "Thanh toán";
                    buttonBack.Visible = true;
                    textBoxSearch.Visible = false;
                    break;

                case BookingStep.ThankYou:
                    panelThankYou.Visible = true;
                    labelLeftTitle.Text = "Hoàn tất";
                    buttonBack.Visible = false;
                    textBoxSearch.Visible = false;
                    break;
            }
        }

        private void ButtonBack_Click(object sender, EventArgs e)
        {
            switch (currentStep)
            {
                case BookingStep.Employee:
                    ShowStep(BookingStep.Specialty);
                    break;
                case BookingStep.DateTime:
                    ShowStep(BookingStep.Employee);
                    break;
                case BookingStep.Payment:
                    ShowStep(BookingStep.DateTime);
                    break;
            }
        }

        private void TextBoxSearch_Enter(object sender, EventArgs e)
        {
            if (textBoxSearch.Text == "Tìm kiếm chuyên khoa")
            {
                textBoxSearch.Text = "";
                textBoxSearch.ForeColor = Color.Black;
            }
        }

        private void TextBoxSearch_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxSearch.Text))
            {
                textBoxSearch.Text = "Tìm kiếm chuyên khoa";
                textBoxSearch.ForeColor = Color.Gray;
            }
        }

        private void TextBoxSearch_TextChanged(object sender, EventArgs e)
        {
            if (textBoxSearch.Text == "Tìm kiếm chuyên khoa" || string.IsNullOrWhiteSpace(textBoxSearch.Text))
            {
                LoadSpecialties();
                return;
            }

            string searchText = textBoxSearch.Text.ToLower();
            var filtered = specialties.Where(s => s.Name.ToLower().Contains(searchText)).ToList();

            flowLayoutPanelSpecialties.Controls.Clear();
            foreach (var specialty in filtered)
            {
                Panel card = CreateSpecialtyCard(specialty);
                flowLayoutPanelSpecialties.Controls.Add(card);
            }
        }
    }

    // Classes dữ liệu
    public class SpecialtyData
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
    }

    public class EmployeeData
    {
        public string Name { get; set; }
        public string Specialty { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using BookingCareManagement.WinForms.Areas.Customer.Services;
using BookingCareManagement.WinForms.Areas.Customer.Services.Models;
using BookingCareManagement.WinForms.Shared.Models.Dtos;

namespace BookingCareManagement.WinForms.Areas.Customer.Forms
{
    public partial class Bookings : Form
    {
        private readonly CustomerBookingApiClient _apiClient;

        private Guid selectedSpecialtyId = Guid.Empty;
        private Guid selectedEmployeeId = Guid.Empty;
        private string selectedSpecialty = "";
        private string selectedEmployee = "";
        private string selectedDate = "";
        private string selectedTime = "";
        private decimal totalPrice =0;

        private List<SpecialtyData> specialties = new();
        private List<EmployeeData> employees = new();
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

        public Bookings(CustomerBookingApiClient apiClient)
        {
            _apiClient = apiClient;
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

        private async void Bookings_Load(object sender, EventArgs e)
        {
            // Load initial data from API, fallback to local static data if API not reachable
            await InitializeDataAsync();

            LoadSpecialties();
            // Server requires min lead of2 days
            dateTimePickerAppointment.MinDate = DateTime.Now.AddDays(2);
            dateTimePickerAppointment.MaxDate = DateTime.Now.AddMonths(3);
            dateTimePickerAppointment.Value = DateTime.Now.AddDays(2);
        }

        private async Task InitializeDataAsync()
        {
            // Default time slots (fallback)
            timeSlots = new List<string>
            {
                "08:00", "08:30", "09:00", "09:30", "10:00", "10:30",
                "11:00", "11:30", "14:00", "14:30", "15:00", "15:30",
                "16:00", "16:30", "17:00", "17:30"
            };

            if (_apiClient is null)
            {
                // fallback to local sample specialties
                specialties = new List<SpecialtyData>
                {
                    new SpecialtyData { Id = Guid.NewGuid(), Name = "Nha Khoa", Price =200000 },
                    new SpecialtyData { Id = Guid.NewGuid(), Name = "Tim Mạch", Price =500000 },
                    new SpecialtyData { Id = Guid.NewGuid(), Name = "Nội Khoa", Price =300000 }
                };
                return;
            }

            try
            {
                var dtos = await _apiClient.GetSpecialtiesAsync();
                specialties = dtos.Select(d => new SpecialtyData
                {
                    Id = d.Id,
                    Name = d.Name,
                    Price = d.Price
                }).ToList();
            }
            catch
            {
                // ignore and fallback to sample data
                specialties = new List<SpecialtyData>
                {
                    new SpecialtyData { Id = Guid.NewGuid(), Name = "Nha Khoa", Price =200000 },
                    new SpecialtyData { Id = Guid.NewGuid(), Name = "Tim Mạch", Price =500000 },
                    new SpecialtyData { Id = Guid.NewGuid(), Name = "Nội Khoa", Price =300000 }
                };
            }
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
                Size = new Size(570,80),
                BackColor = Color.White,
                Margin = new Padding(10),
                Cursor = Cursors.Hand,
                BorderStyle = BorderStyle.FixedSingle,
                Tag = specialty
            };

            Label lblName = new Label
            {
                Text = specialty.Name,
                Font = new Font("Segoe UI",12, FontStyle.Bold),
                ForeColor = Color.FromArgb(33,33,33),
                Location = new Point(15,15),
                AutoSize = true
            };

            Label lblPrice = new Label
            {
                Text = $"{specialty.Price:N0} VNĐ",
                Font = new Font("Segoe UI",11),
                ForeColor = Color.FromArgb(255,165,0),
                Location = new Point(15,45),
                AutoSize = true
            };

            panel.Controls.Add(lblName);
            panel.Controls.Add(lblPrice);

            panel.Click += async (s, e) => await SelectSpecialtyAsync(specialty);
            lblName.Click += async (s, e) => await SelectSpecialtyAsync(specialty);
            lblPrice.Click += async (s, e) => await SelectSpecialtyAsync(specialty);

            return panel;
        }

        private async Task SelectSpecialtyAsync(SpecialtyData specialty)
        {
            selectedSpecialtyId = specialty.Id;
            selectedSpecialty = specialty.Name;
            totalPrice = specialty.Price;

            labelSelectedSpecialtyValue.Text = specialty.Name;
            labelTotalPrice.Text = $"{totalPrice:N0} VNĐ";
            labelCheckoutTotal.Text = $"{totalPrice:N0} VNĐ";
            panelTotalSection.Visible = true;

            // Chuyển sang bước chọn nhân viên
            ShowStep(BookingStep.Employee);
            await LoadEmployeesAsync(specialty.Id);
        }

        private async Task LoadEmployeesAsync(Guid specialtyId)
        {
            flowLayoutPanelEmployees.Controls.Clear();

            employees.Clear();

            if (_apiClient != null)
            {
                try
                {
                    var docs = await _apiClient.GetDoctorsBySpecialtyAsync(specialtyId);
                    employees = docs.Select(d => new EmployeeData
                    {
                        Id = d.Id,
                        Name = d.FullName,
                        Specialty = selectedSpecialty
                    }).ToList();
                }
                catch
                {
                    // ignore and fallback
                }
            }

            // If still empty, show placeholder employees
            if (employees.Count ==0)
            {
                employees = new List<EmployeeData>
                {
                    new EmployeeData { Id = Guid.NewGuid(), Name = "BS. A", Specialty = selectedSpecialty },
                    new EmployeeData { Id = Guid.NewGuid(), Name = "BS. B", Specialty = selectedSpecialty }
                };
            }

            foreach (var employee in employees)
            {
                Panel card = CreateEmployeeCard(employee);
                flowLayoutPanelEmployees.Controls.Add(card);
            }
        }

        private Panel CreateEmployeeCard(EmployeeData employee)
        {
            Panel panel = new Panel
            {
                Size = new Size(570,80),
                BackColor = Color.White,
                Margin = new Padding(10),
                Cursor = Cursors.Hand,
                BorderStyle = BorderStyle.FixedSingle,
                Tag = employee
            };

            Label lblName = new Label
            {
                Text = employee.Name,
                Font = new Font("Segoe UI",12, FontStyle.Bold),
                ForeColor = Color.FromArgb(33,33,33),
                Location = new Point(15,15),
                AutoSize = true
            };

            Label lblSpecialty = new Label
            {
                Text = $"Chuyên khoa: {employee.Specialty}",
                Font = new Font("Segoe UI",10),
                ForeColor = Color.Gray,
                Location = new Point(15,45),
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
            selectedEmployeeId = employee.Id;
            selectedEmployee = employee.Name;
            labelSelectedEmployeeValue.Text = employee.Name;

            // Chuyển sang bước chọn ngày giờ
            ShowStep(BookingStep.DateTime);
            _ = LoadTimeSlotsAsync();
        }

        private void DateTimePickerAppointment_ValueChanged(object sender, EventArgs e)
        {
            selectedDate = dateTimePickerAppointment.Value.ToString("dd/MM/yyyy");
            _ = LoadTimeSlotsAsync();
        }

        private async Task LoadTimeSlotsAsync()
        {
            comboBoxTimeSlot.Items.Clear();
            comboBoxTimeSlot.Enabled = false;

            // Prefer API slots for selected doctor and date
            if (_apiClient != null && selectedEmployeeId != Guid.Empty)
            {
                try
                {
                    var date = DateOnly.FromDateTime(dateTimePickerAppointment.Value);
                    var slots = await _apiClient.GetDoctorSlotsAsync(selectedEmployeeId, date);
                    var available = slots.Where(s => s.IsAvailable).OrderBy(s => s.StartLocal).ToList();
                    foreach (var s in available)
                    {
                        comboBoxTimeSlot.Items.Add(s.StartLocal.ToString("HH:mm"));
                    }
                }
                catch
                {
                    // fallback to static list below
                }
            }

            if (comboBoxTimeSlot.Items.Count ==0)
            {
                // fallback static slots
                foreach (var time in timeSlots)
                {
                    comboBoxTimeSlot.Items.Add(time);
                }
            }

            if (comboBoxTimeSlot.Items.Count >0)
            {
                comboBoxTimeSlot.Enabled = true;
                comboBoxTimeSlot.SelectedIndex =0;
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
                decimal discount = totalPrice *0.1m;
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

        private async Task<bool> SaveBooking()
        {
            try
            {
                if (selectedSpecialtyId == Guid.Empty)
                {
                    MessageBox.Show("Vui lòng chọn chuyên khoa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (selectedEmployeeId == Guid.Empty)
                {
                    MessageBox.Show("Vui lòng chọn bác sĩ!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (string.IsNullOrWhiteSpace(selectedDate) || string.IsNullOrWhiteSpace(selectedTime))
                {
                    MessageBox.Show("Vui lòng chọn ngày và giờ!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                // Parse local date + time
                DateTime startLocal;
                try
                {
                    startLocal = DateTime.ParseExact($"{selectedDate} {selectedTime}", "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
                    startLocal = DateTime.SpecifyKind(startLocal, DateTimeKind.Local);
                }
                catch (FormatException)
                {
                    MessageBox.Show("Định dạng ngày giờ không hợp lệ.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                var startUtc = startLocal.ToUniversalTime();

                var req = new CreateCustomerBookingRequest
                {
                    SpecialtyId = selectedSpecialtyId,
                    DoctorId = selectedEmployeeId,
                    SlotStartUtc = startUtc,
                    DurationMinutes =30,
                    CustomerName = textBoxName.Text.Trim(),
                    CustomerPhone = textBoxPhone.Text.Trim()
                };

                var resp = await _apiClient.CreateAsync(req);
                if (resp is null)
                {
                    MessageBox.Show("Đã xảy ra lỗi khi tạo lịch", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                MessageBox.Show("Đặt lịch thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return true;
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Lỗi kết nối: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private async void ButtonConfirmBooking_Click(object sender, EventArgs e)
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

            // Lưu đặt lịch
            bool success = await SaveBooking();

            if (success)
            {
                ShowStep(BookingStep.ThankYou);
            }
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
        public Guid Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }

    public class EmployeeData
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Specialty { get; set; }
    }
}
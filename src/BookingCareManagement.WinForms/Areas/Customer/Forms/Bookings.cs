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
using BookingCareManagement.WinForms.Shared.State;

namespace BookingCareManagement.WinForms.Areas.Customer.Forms
{
    public partial class Bookings : Form
    {
        private readonly CustomerBookingApiClient _apiClient;
        private readonly SessionState _session;

        private RadioButton? _rbPaymentCash;
        private RadioButton? _rbPaymentQr;
        private Guid _pendingAppointmentId = Guid.Empty;
        private const string PaymentMethodTransfer = "transfer";

        private Guid selectedSpecialtyId = Guid.Empty;
        private Guid selectedEmployeeId = Guid.Empty;
        private string selectedSpecialty = "";
        private string selectedEmployee = "";
        private string selectedDate = "";
        private string selectedTime = "";
        private decimal totalPrice =0;

        private List<SpecialtyData> specialties = new();
        private List<EmployeeData> employees = new();
        private List<string> timeSlots = new();

        private enum BookingStep
        {
            Specialty,
            Employee,
            DateTime,
            Payment,
            ThankYou
        }

        private BookingStep currentStep = BookingStep.Specialty;

        public Bookings(CustomerBookingApiClient apiClient, SessionState session)
        {
            _apiClient = apiClient;
            _session = session;
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
            // Back to start button in thank you panel
            if (this.Controls.Find("buttonBackToStart", true).FirstOrDefault() is Button backToStart)
            {
                backToStart.Click += ButtonBackToStart_Click;
            }

            // Ensure cards resize when container changes
            flowLayoutPanelSpecialties.SizeChanged += (s, e) => AdjustCardWidths();
            flowLayoutPanelEmployees.SizeChanged += (s, e) => AdjustCardWidths();

            InitializePaymentOptions();
        }

        private async void Bookings_Load(object? sender, EventArgs e)
        {
            // Load initial data from API, fallback to local static data if API not reachable
            await InitializeDataAsync();

            // Load current user profile to pre-fill name and phone
            _ = LoadCurrentUserProfileAsync();
            
            LoadSpecialties();
            // Make sure card widths match container
            AdjustCardWidths();
            // Server requires min lead of2 days
            dateTimePickerAppointment.MinDate = DateTime.Now.AddDays(2);
            dateTimePickerAppointment.MaxDate = DateTime.Now.AddMonths(3);
            dateTimePickerAppointment.Value = DateTime.Now.AddDays(2);
        }
        
        private async Task LoadCurrentUserProfileAsync()
        {
            try
            {
                // Prefer session state (already loaded at login or after editing account)
                if (_session != null && _session.IsAuthenticated)
                {
                    var nameFromSession = _session.DisplayName;
                    var phoneFromSession = string.Empty; // SessionState currently doesn't expose phone; try profile API as fallback

                    if (this.InvokeRequired)
                    {
                        this.Invoke(new Action(() =>
                        {
                            if (!string.IsNullOrWhiteSpace(nameFromSession)) textBoxName.Text = nameFromSession;
                        }));
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(nameFromSession)) textBoxName.Text = nameFromSession;
                    }

                    // Try to get phone from server profile if API available
                    try
                    {
                        var profile = await _apiClient.GetProfileAsync();
                        if (profile != null && !string.IsNullOrWhiteSpace(profile.PhoneNumber))
                        {
                            if (this.InvokeRequired)
                            {
                                this.Invoke(new Action(() => textBoxPhone.Text = profile.PhoneNumber));
                            }
                            else
                            {
                                textBoxPhone.Text = profile.PhoneNumber;
                            }
                        }
                    }
                    catch
                    {
                        // ignore
                    }

                    return;
                }

                // Fallback: call profile API directly
                var profileApi = await _apiClient.GetProfileAsync();
                if (profileApi is null) return;

                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() =>
                    {
                        if (!string.IsNullOrWhiteSpace(profileApi.FullName)) textBoxName.Text = profileApi.FullName;
                        if (!string.IsNullOrWhiteSpace(profileApi.PhoneNumber)) textBoxPhone.Text = profileApi.PhoneNumber;
                    }));
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(profileApi.FullName)) textBoxName.Text = profileApi.FullName;
                    if (!string.IsNullOrWhiteSpace(profileApi.PhoneNumber)) textBoxPhone.Text = profileApi.PhoneNumber;
                }
            }
            catch
            {
                // ignore
            }
        }

        private async Task InitializeDataAsync()
        {
            // Default time slots (fallback)
            timeSlots = new List<string>
            {
                
            };

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
                
            }
        }

        private void InitializePaymentOptions()
        {
            if (panelPayment == null)
            {
                return;
            }

            var groupBox = new GroupBox
            {
                Text = "Phương thức thanh toán",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Location = new Point(13, labelPaymentNote.Bottom + 10),
                Size = new Size(773, 70)
            };

            _rbPaymentCash = new RadioButton
            {
                Text = "Tiền mặt",
                ForeColor = Color.White,
                Location = new Point(20, 30),
                AutoSize = true,
                Checked = true
            };

            _rbPaymentQr = new RadioButton
            {
                Text = "Quét mã (MoMo/VNPay - dev)",
                ForeColor = Color.White,
                Location = new Point(160, 30),
                AutoSize = true
            };

            _rbPaymentCash.CheckedChanged += (_, _) => UpdatePaymentNote();
            _rbPaymentQr.CheckedChanged += (_, _) => UpdatePaymentNote();

            groupBox.Controls.Add(_rbPaymentCash);
            groupBox.Controls.Add(_rbPaymentQr);
            panelPayment.Controls.Add(groupBox);
            groupBox.BringToFront();

            // shift controls below the new group box
            var shift = 80;
            labelPromoCode.Top += shift;
            textBoxPromoCode.Top += shift;
            buttonApplyPromo.Top += shift;
            labelName.Top += shift;
            textBoxName.Top += shift;
            labelPhone.Top += shift;
            textBoxPhone.Top += shift;
            buttonConfirmBooking.Top += shift;

            UpdatePaymentNote();
        }

        private void UpdatePaymentNote()
        {
            if (_rbPaymentQr?.Checked == true)
            {
                labelPaymentNote.Text = "Thanh toán bằng chuyển khoản (quét mã). Sau khi quét thành công, hệ thống sẽ tự động cập nhật trạng thái.";
                buttonConfirmBooking.Text = "Thanh toán & đặt lịch";
            }
            else
            {
                labelPaymentNote.Text = "Thanh toán sẽ được thực hiện tại địa điểm đặt lịch.";
                buttonConfirmBooking.Text = "Đặt lịch";
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
            AdjustCardWidths();
        }

        private Panel CreateSpecialtyCard(SpecialtyData specialty)
        {
            var cardWidth =570;
            if (flowLayoutPanelSpecialties != null && flowLayoutPanelSpecialties.ClientSize.Width >200)
            {
                cardWidth = Math.Max(200, flowLayoutPanelSpecialties.ClientSize.Width -30);
            }
            
            Panel panel = new Panel
            {
                Size = new Size(cardWidth,80),
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
            var cardWidth =570;
            if (flowLayoutPanelEmployees != null && flowLayoutPanelEmployees.ClientSize.Width >200)
            {
                cardWidth = Math.Max(200, flowLayoutPanelEmployees.ClientSize.Width -30);
            }
            
            Panel panel = new Panel
            {
                Size = new Size(cardWidth,80),
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

        private void DateTimePickerAppointment_ValueChanged(object? sender, EventArgs e)
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
                        // show range: start - end
                        comboBoxTimeSlot.Items.Add($"{s.StartLocal:HH:mm} - {s.EndLocal:HH:mm}");
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
                    // assume default duration30 minutes
                    if (TimeSpan.TryParse(time, out var tsStart))
                    {
                        var tsEnd = tsStart.Add(TimeSpan.FromMinutes(30));
                        var startStr = tsStart.ToString(@"hh\:mm");
                        var endStr = tsEnd.ToString(@"hh\:mm");
                        comboBoxTimeSlot.Items.Add($"{startStr} - {endStr}");
                    }
                    else
                    {
                        comboBoxTimeSlot.Items.Add(time);
                    }
                }
            }

            if (comboBoxTimeSlot.Items.Count >0)
            {
                comboBoxTimeSlot.Enabled = true;
                comboBoxTimeSlot.SelectedIndex =0;
            }
        }

        private void ButtonConfirmDateTime_Click(object? sender, EventArgs e)
        {
            if (comboBoxTimeSlot.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn giờ!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            selectedTime = comboBoxTimeSlot.SelectedItem?.ToString() ?? string.Empty;
            selectedDate = dateTimePickerAppointment.Value.ToString("dd/MM/yyyy");

            labelSelectedDateTimeValue.Text = $"{selectedDate} - {selectedTime}";

            // Chuyển sang bước thanh toán
            ShowStep(BookingStep.Payment);
        }

        private void ButtonApplyPromo_Click(object? sender, EventArgs e)
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

        private async Task<AppointmentDto?> SaveBooking()
        {
            try
            {
                if (selectedSpecialtyId == Guid.Empty)
                {
                    MessageBox.Show("Vui lòng chọn chuyên khoa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return null;
                }

                if (selectedEmployeeId == Guid.Empty)
                {
                    MessageBox.Show("Vui lòng chọn bác sĩ!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return null;
                }

                if (string.IsNullOrWhiteSpace(selectedDate) || string.IsNullOrWhiteSpace(selectedTime))
                {
                    MessageBox.Show("Vui lòng chọn ngày và giờ!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return null;
                }

                // Parse local date + time
                DateTime startLocal;
                try
                {
                    // selectedTime may be a range like "14:30 -15:00". Extract start time
                    var timePart = selectedTime.Split('-')[0].Trim();
                    startLocal = DateTime.ParseExact($"{selectedDate} {timePart}", "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
                    startLocal = DateTime.SpecifyKind(startLocal, DateTimeKind.Local);
                }
                catch (FormatException)
                {
                    MessageBox.Show("Định dạng ngày giờ không hợp lệ.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
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
                    return null;
                }
                return resp;
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Lỗi kết nối: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        private async void ButtonConfirmBooking_Click(object? sender, EventArgs e)
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
            var booking = await SaveBooking();
            if (booking is null)
            {
                return;
            }

            _pendingAppointmentId = booking.Id;

            if (_rbPaymentQr?.Checked == true)
            {
                var paid = await ShowQrPaymentDialogAsync(_pendingAppointmentId);
                if (!paid)
                {
                    return;
                }

                var ok = await _apiClient.MarkPaidAsync(_pendingAppointmentId, PaymentMethodTransfer);
                if (!ok)
                {
                    MessageBox.Show("Không thể cập nhật trạng thái thanh toán.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                MessageBox.Show("Thanh toán thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ShowStep(BookingStep.ThankYou);
                return;
            }

            MessageBox.Show("Đặt lịch thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            ShowStep(BookingStep.ThankYou);
        }

        private Task<bool> ShowQrPaymentDialogAsync(Guid appointmentId)
        {
            using var dialog = new QrPaymentDialog(appointmentId, totalPrice);
            var result = dialog.ShowDialog(this);
            return Task.FromResult(result == DialogResult.OK);
        }

        private sealed class QrPaymentDialog : Form
        {
            public QrPaymentDialog(Guid appointmentId, decimal amount)
            {
                Text = "Thanh toán bằng quét mã";
                Size = new Size(360, 460);
                StartPosition = FormStartPosition.CenterParent;
                FormBorderStyle = FormBorderStyle.FixedDialog;
                MaximizeBox = false;
                MinimizeBox = false;

                var label = new Label
                {
                    Text = "Quét mã để thanh toán (dev)",
                    AutoSize = true,
                    Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                    Location = new Point(16, 16)
                };

                var qrBox = new PictureBox
                {
                    Size = new Size(280, 280),
                    Location = new Point(32, 50),
                    BorderStyle = BorderStyle.FixedSingle,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Image = BuildFakeQr($"APPT:{appointmentId}|AMT:{amount:0}")
                };

                var hint = new Label
                {
                    Text = $"Số tiền: {amount:N0} VNĐ\nMoMo/VNPay (dev)",
                    AutoSize = true,
                    Location = new Point(16, 340)
                };

                var btnPaid = new Button
                {
                    Text = "Tôi đã thanh toán",
                    BackColor = Color.FromArgb(34, 197, 94),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Size = new Size(160, 36),
                    Location = new Point(16, 392)
                };
                btnPaid.FlatAppearance.BorderSize = 0;
                btnPaid.Click += (_, _) => DialogResult = DialogResult.OK;

                var btnCancel = new Button
                {
                    Text = "Hủy",
                    BackColor = Color.FromArgb(239, 68, 68),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Size = new Size(120, 36),
                    Location = new Point(200, 392)
                };
                btnCancel.FlatAppearance.BorderSize = 0;
                btnCancel.Click += (_, _) => DialogResult = DialogResult.Cancel;

                Controls.Add(label);
                Controls.Add(qrBox);
                Controls.Add(hint);
                Controls.Add(btnPaid);
                Controls.Add(btnCancel);
            }

            private static Bitmap BuildFakeQr(string payload)
            {
                var bmp = new Bitmap(280, 280);
                using var g = Graphics.FromImage(bmp);
                g.Clear(Color.White);
                using var black = new SolidBrush(Color.Black);

                var rand = payload.GetHashCode();
                var size = 14;
                for (var y = 0; y < 20; y++)
                {
                    for (var x = 0; x < 20; x++)
                    {
                        rand = unchecked(rand * 31 + x + y);
                        if ((rand & 1) == 0)
                        {
                            g.FillRectangle(black, x * size, y * size, size - 2, size - 2);
                        }
                    }
                }

                return bmp;
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

        private void ButtonBack_Click(object? sender, EventArgs e)
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

        private void ButtonBackToStart_Click(object? sender, EventArgs e)
        {
            ResetBookingForm();
            ShowStep(BookingStep.Specialty);
        }

        private void ResetBookingForm()
        {
            // Reset selections and UI
            selectedSpecialty = string.Empty;
            selectedEmployee = string.Empty;
            selectedSpecialtyId = Guid.Empty;
            selectedEmployeeId = Guid.Empty;
            selectedDate = string.Empty;
            selectedTime = string.Empty;
            totalPrice =0;

            labelSelectedSpecialtyValue.Text = "Chưa chọn";
            labelSelectedEmployeeValue.Text = "Chưa chọn";
            labelSelectedDateTimeValue.Text = "Chưa chọn";
            labelTotalPrice.Text = "0 VNĐ";
            labelCheckoutTotal.Text = "0 VNĐ";
            panelTotalSection.Visible = false;

            textBoxName.Text = string.Empty;
            textBoxPhone.Text = string.Empty;
            textBoxPromoCode.Text = string.Empty;

            // Clear flow panels
            flowLayoutPanelEmployees.Controls.Clear();
            // reload specialties from cached list
            LoadSpecialties();
        }

        private void TextBoxSearch_Enter(object? sender, EventArgs e)
        {
            if (textBoxSearch.Text == "Tìm kiếm chuyên khoa")
            {
                textBoxSearch.Text = "";
                textBoxSearch.ForeColor = Color.Black;
            }
        }

        private void TextBoxSearch_Leave(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxSearch.Text))
            {
                textBoxSearch.Text = "Tìm kiếm chuyên khoa";
                textBoxSearch.ForeColor = Color.Gray;
            }
        }

        private void TextBoxSearch_TextChanged(object? sender, EventArgs e)
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
        // lay cai tren
        private void AdjustCardWidths()
        {
            try
            {
                if (flowLayoutPanelSpecialties != null)
                {
                    var w = Math.Max(200, flowLayoutPanelSpecialties.ClientSize.Width -30);
                    foreach (Panel p in flowLayoutPanelSpecialties.Controls.OfType<Panel>())
                    {
                        p.Width = w;
                    }
                }

                if (flowLayoutPanelEmployees != null)
                {
                    var w = Math.Max(200, flowLayoutPanelEmployees.ClientSize.Width -30);
                    foreach (Panel p in flowLayoutPanelEmployees.Controls.OfType<Panel>())
                    {
                        p.Width = w;
                    }
                }
            }
            catch
            {
                // ignore layout exceptions
            }
        }
        public async Task OpenForSpecialtyAsync(Guid specialtyId, string specialtyName, decimal price)
        {
            // Ensure initial data loaded
            await InitializeDataAsync();

            // Ensure user profile loaded so name/phone are prefilled
            await LoadCurrentUserProfileAsync();

            // Set selection
            selectedSpecialtyId = specialtyId;
            selectedSpecialty = specialtyName ?? string.Empty;
            totalPrice = price;

            labelSelectedSpecialtyValue.Text = specialtyName;
            labelTotalPrice.Text = $"{totalPrice:N0} VNĐ";
            labelCheckoutTotal.Text = $"{totalPrice:N0} VNĐ";
            panelTotalSection.Visible = true;

            // Switch to employee selection and load employees for the specialty
            ShowStep(BookingStep.Employee);
            await LoadEmployeesAsync(specialtyId);
        }
    }

    // Classes dữ liệu
    public class SpecialtyData
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }

    public class EmployeeData
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Specialty { get; set; } = string.Empty;
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static BookingCareManagement.WinForms.Customer;
using BookingCareManagement.WinForms.Areas.Admin.Services;
using BookingCareManagement.WinForms.Shared.Models.Dtos;
using System.Globalization;
using BookingCareManagement.WinForms.Areas.Doctor.Services;

namespace BookingCareManagement.WinForms
{
    public partial class Calendar : Form
    {
        private DateTime currentDate;
        private string currentView = "Month";
        // Allow either admin or doctor API client depending on caller
        private readonly AdminAppointmentsApiClient? _adminAppointmentsApiClient;
        private readonly DoctorAppointmentsApiClient? _doctorAppointmentsApiClient;
        private readonly CustomerService? _customerService;
        private List<CalendarEventDto> _events = new();
        private CancellationTokenSource? _loadCts;
        private DateTime _lastLoadedMonth = DateTime.MinValue;

        // Biến cho drag & drop
        private Label? draggedAppointment;
        private Point dragStartPoint;
        private Panel? sourcePanel;

        // state for active view
        private Button? _activeViewButton;
        private Button? _activeDoctorFilterButton;
        private string? _selectedDoctorName;
        private FlowLayoutPanel? _doctorFilterPanel;
        private Button? _doctorFilterToggleButton;
        private ComboBox? _specialtyFilterComboBox;
        private TextBox? _doctorSearchTextBox;
        private string? _selectedSpecialty;
        private bool _isDoctorFilterExpanded;
        private bool _isUpdatingFilters;

        private static readonly Color[] EventPalette = new[]
        {
            Color.FromArgb(254, 243, 199),
            Color.FromArgb(219, 234, 254),
            Color.FromArgb(220, 252, 231),
            Color.FromArgb(243, 232, 255),
            Color.FromArgb(254, 226, 226),
            Color.FromArgb(224, 231, 255),
            Color.FromArgb(240, 253, 250),
            Color.FromArgb(255, 237, 213)
        };

        // Chỉ giữ lại constructor DI
        public Calendar(AdminAppointmentsApiClient appointmentsApiClient, CustomerService? customerService = null)
        {
            _adminAppointmentsApiClient = appointmentsApiClient;
            _customerService = customerService;
            currentDate = DateTime.Now;
            InitializeComponent();
            InitializeCustomComponents();
        }

        // Overload for doctor client - uses doctor endpoints (avoids 403 when user is doctor)
        public Calendar(DoctorAppointmentsApiClient appointmentsApiClient, CustomerService? customerService = null)
        {
            _doctorAppointmentsApiClient = appointmentsApiClient;
            _customerService = customerService;
            currentDate = DateTime.Now;
            InitializeComponent();
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            UpdateStyles();
            EnableDoubleBuffering(calendarPanel);

            // Gắn sự kiện cho các nút đã được tạo bởi Designer
            AttachEventHandlers();
            ApplyButtonStyling();
            // ensure prev/next vertical align and remove their borders
            prevBtn.FlatAppearance.BorderSize = 0;
            nextBtn.FlatAppearance.BorderSize = 0;
            prevBtn.Location = new Point(prevBtn.Location.X, 19);
            nextBtn.Location = new Point(nextBtn.Location.X, 19);

            // set default combo selection
            comboBox1.SelectedIndex = 0;
            comboBox1.SelectedIndexChanged += (s, e) => RefreshCalendar();

            ApplySurfaceStyling();
            EnsureUserPanelLayout();

            RefreshCalendar();
        }

        private void AttachEventHandlers()
        {
            prevBtn.Click += (s, e) =>
            {
                if (currentView == "Month")
                    currentDate = currentDate.AddMonths(-1);
                else if (currentView == "Week")
                    currentDate = currentDate.AddDays(-7);
                else if (currentView == "Day")
                    currentDate = currentDate.AddDays(-1);
                RefreshCalendar();
            };

            nextBtn.Click += (s, e) =>
            {
                if (currentView == "Month")
                    currentDate = currentDate.AddMonths(1);
                else if (currentView == "Week")
                    currentDate = currentDate.AddDays(7);
                else if (currentView == "Day")
                    currentDate = currentDate.AddDays(1);
                RefreshCalendar();
            };

            CreateUserPanel();
        }

        private void ApplyButtonStyling()
        {
            // set base styles for non-add buttons
            var buttons = new[] { btnToday, monthBtn, weekBtn, dayBtn };
            foreach (var b in buttons)
            {
                if (b == null) continue;
                b.FlatStyle = FlatStyle.Flat;
                b.FlatAppearance.BorderSize = 1;
                b.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);
                b.BackColor = Color.FromArgb(255, 255, 255);
                b.ForeColor = Color.FromArgb(55, 65, 81);
                b.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
                b.Cursor = Cursors.Hand;
                b.Padding = new Padding(8, 6, 8, 6);
                b.MouseEnter += (s, e) =>
                {
                    if (s is not Button btn) return;
                    if (btn == _activeViewButton) return;
                    btn.BackColor = Color.FromArgb(248, 250, 252);
                };
                b.MouseLeave += (s, e) =>
                {
                    if (s is not Button btn) return;
                    if (btn == _activeViewButton) return;
                    btn.BackColor = Color.White;
                };
            }

            // primary action (add) keep static blue, no hover/active
           

            // wire view buttons to an explicit activator to ensure only one active
            Action<Button> activate = b =>
            {
                if (_activeViewButton != null && _activeViewButton != b)
                {
                    _activeViewButton.BackColor = Color.White;
                    _activeViewButton.ForeColor = Color.FromArgb(55, 65, 81);
                    _activeViewButton.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);
                }
                _activeViewButton = b;
                _activeViewButton.BackColor = Color.FromArgb(37, 99, 235);
                _activeViewButton.ForeColor = Color.White;
                _activeViewButton.FlatAppearance.BorderColor = Color.FromArgb(37, 99, 235);
            };

            monthBtn.Click += (s, e) => { currentView = "Month"; activate(monthBtn); RefreshCalendar(); };
            weekBtn.Click += (s, e) => { currentView = "Week"; activate(weekBtn); RefreshCalendar(); };
            dayBtn.Click += (s, e) => { currentView = "Day"; activate(dayBtn); RefreshCalendar(); };

            // set initial
            activate(monthBtn);
        }

        private void ApplySurfaceStyling()
        {
            BackColor = Color.FromArgb(248, 250, 252);
            headerPanel.BackColor = Color.FromArgb(248, 250, 252);
            navigationPanel.BackColor = Color.White;
            calendarPanel.BackColor = Color.FromArgb(248, 250, 252);

            headerTitleLabel.ForeColor = Color.FromArgb(15, 23, 42);
            headerTitleLabel.Font = new Font("Segoe UI", 22F, FontStyle.Bold);

            comboBox1.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
            comboBox1.BackColor = Color.White;
            comboBox1.ForeColor = Color.FromArgb(55, 65, 81);

            btnToday.BackColor = Color.White;
            btnToday.ForeColor = Color.FromArgb(55, 65, 81);
            btnToday.FlatAppearance.BorderColor = Color.FromArgb(226, 232, 240);

            monthLabel.ForeColor = Color.FromArgb(15, 23, 42);
            monthLabel.Font = new Font("Segoe UI", 12.5F, FontStyle.Bold);

            prevBtn.ForeColor = Color.FromArgb(71, 85, 105);
            nextBtn.ForeColor = Color.FromArgb(71, 85, 105);
        }

        private void EnsureUserPanelLayout()
        {
            userPanel.Visible = true;
            userPanel.BackColor = Color.White;
            userPanel.Padding = new Padding(30, 12, 30, 10);
            userPanel.Height = 130;

            userPanel.Controls.Clear();

            var headerRow = new Panel { Dock = DockStyle.Top, Height = 28, BackColor = Color.White };
            var title = new Label
            {
                Text = "Lịch",
                Dock = DockStyle.Left,
                Width = 120,
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42)
            };
            headerRow.Controls.Add(title);

            var filterRow = new Panel { Dock = DockStyle.Top, Height = 36, BackColor = Color.White, Padding = new Padding(0, 6, 0, 0) };

            _doctorSearchTextBox = new TextBox
            {
                Width = 220,
                Height = 28,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.Gray,
                Text = "Tìm bác sĩ",
                BorderStyle = BorderStyle.FixedSingle
            };
            _doctorSearchTextBox.Enter += (s, e) =>
            {
                if (_doctorSearchTextBox.Text == "Tìm bác sĩ")
                {
                    _doctorSearchTextBox.Text = string.Empty;
                    _doctorSearchTextBox.ForeColor = Color.FromArgb(15, 23, 42);
                }
            };
            _doctorSearchTextBox.Leave += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(_doctorSearchTextBox.Text))
                {
                    _doctorSearchTextBox.Text = "Tìm bác sĩ";
                    _doctorSearchTextBox.ForeColor = Color.Gray;
                }
            };
            _doctorSearchTextBox.TextChanged += (s, e) =>
            {
                if (_doctorSearchTextBox.Text == "Tìm bác sĩ") return;
                BuildDoctorFilters();
            };

            _specialtyFilterComboBox = new ComboBox
            {
                Width = 200,
                Height = 28,
                Font = new Font("Segoe UI", 9F),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _specialtyFilterComboBox.SelectedIndexChanged += (s, e) =>
            {
                if (_isUpdatingFilters) return;
                _selectedSpecialty = _specialtyFilterComboBox.SelectedItem?.ToString();
                if (_selectedSpecialty == "Tất cả chuyên khoa")
                {
                    _selectedSpecialty = null;
                }
                BuildDoctorFilters();
                RefreshCalendar();
            };

            filterRow.Controls.Add(_specialtyFilterComboBox);
            filterRow.Controls.Add(_doctorSearchTextBox);
            _specialtyFilterComboBox.Location = new Point(filterRow.Width - 200, 4);
            _doctorSearchTextBox.Location = new Point(filterRow.Width - 430, 4);
            filterRow.Resize += (s, e) =>
            {
                _specialtyFilterComboBox.Location = new Point(filterRow.Width - _specialtyFilterComboBox.Width, 4);
                _doctorSearchTextBox.Location = new Point(filterRow.Width - _specialtyFilterComboBox.Width - _doctorSearchTextBox.Width - 10, 4);
            };

            _doctorFilterPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(0, 4, 0, 0),
                BackColor = Color.White
            };

            _doctorFilterToggleButton = new Button
            {
                Text = "Xem thêm",
                Height = 28,
                AutoSize = true,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(37, 99, 235),
                Cursor = Cursors.Hand,
                Margin = new Padding(6, 0, 0, 0)
            };
            _doctorFilterToggleButton.FlatAppearance.BorderSize = 0;
            _doctorFilterToggleButton.Click += (s, e) => ToggleDoctorFilterWrap();

            var filtersContainer = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            filtersContainer.Controls.Add(_doctorFilterPanel);
            filtersContainer.Controls.Add(_doctorFilterToggleButton);
            filtersContainer.Resize += (s, e) =>
            {
                if (_doctorFilterToggleButton == null) return;
                _doctorFilterToggleButton.Location = new Point(filtersContainer.Width - _doctorFilterToggleButton.Width, 2);
            };

            userPanel.Controls.Add(filtersContainer);
            userPanel.Controls.Add(filterRow);
            userPanel.Controls.Add(headerRow);
        }

        private void CreateUserPanel()
        {
            // userPanel hidden by designer; nothing here
        }

        private void SwitchView(string view)
        {
            currentView = view;
            RefreshCalendar();
        }

        private void Navigate(int direction)
        {
            if (currentView == "Month")
                currentDate = currentDate.AddMonths(direction);
            else if (currentView == "Week")
                currentDate = currentDate.AddDays(7 * direction);
            else if (currentView == "Day")
                currentDate = currentDate.AddDays(direction);

            RefreshCalendar();
        }

        // ======================= CALENDAR VIEWS ==========================


        private string GetWeekRangeText()
        {
            DateTime startOfWeek = currentDate.AddDays(-(int)currentDate.DayOfWeek + 1);
            DateTime endOfWeek = startOfWeek.AddDays(6);
            return $"{startOfWeek:dd/MM} - {endOfWeek:dd/MM/yyyy}";
        }



        // ======================= MONTH VIEW =========================_
        private void CreateCalendar()
        {
            calendarPanel.Controls.Clear();

            // Header thứ trong tuần (Tiếng Việt)
            string[] dayNames = { "T2", "T3", "T4", "T5", "T6", "T7", "CN" };
            int headerY = 12;
            int sidePadding = 18;
            int cellGap = 6;
            int cellWidth = Math.Max(140, (calendarPanel.ClientSize.Width - sidePadding * 2 - cellGap * 6) / 7);

            for (int i = 0; i < 7; i++)
            {
                Label dayHeader = new Label
                {
                    Text = dayNames[i],
                    Location = new Point(sidePadding + i * (cellWidth + cellGap), headerY),
                    Size = new Size(cellWidth, 26),
                    Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(71, 85, 105),
                    TextAlign = ContentAlignment.TopLeft
                };
                calendarPanel.Controls.Add(dayHeader);
            }

            // Tạo ô lịch
            DateTime firstDay = new DateTime(currentDate.Year, currentDate.Month, 1);
            int daysInMonth = DateTime.DaysInMonth(currentDate.Year, currentDate.Month);
            int startDay = (int)firstDay.DayOfWeek;
            if (startDay == 0) startDay = 7;
            startDay--;

            int cellHeight = 135;
            int currentRow = 0;
            int currentCol = startDay;
            int dayCounter = 1;

            // Ngày tháng trước
            DateTime prevMonth = currentDate.AddMonths(-1);
            int prevMonthDays = DateTime.DaysInMonth(prevMonth.Year, prevMonth.Month);
            int prevStart = prevMonthDays - startDay + 1;

            for (int i = 0; i < startDay; i++)
            {
                Panel dayCell = CreateDayCell(prevStart + i, true, i, currentRow, cellWidth, cellHeight, sidePadding, cellGap);
                calendarPanel.Controls.Add(dayCell);
            }

            // Ngày tháng hiện tại
            while (dayCounter <= daysInMonth)
            {
                Panel dayCell = CreateDayCell(dayCounter, false, currentCol, currentRow, cellWidth, cellHeight, sidePadding, cellGap);

                calendarPanel.Controls.Add(dayCell);

                dayCounter++;
                currentCol++;
                if (currentCol >= 7)
                {
                    currentCol = 0;
                    currentRow++;
                }
            }

            // Ngày tháng sau
            int nextDay = 1;
            while (currentCol < 7)
            {
                Panel dayCell = CreateDayCell(nextDay, true, currentCol, currentRow, cellWidth, cellHeight, sidePadding, cellGap);
                calendarPanel.Controls.Add(dayCell);
                nextDay++;
                currentCol++;
            }
        }
        // ============= WEEK VIEW =============
        private void CreateWeekView()
        {
            calendarPanel.Controls.Clear();

            DateTime startOfWeek = currentDate.AddDays(-(int)currentDate.DayOfWeek + 1);
            if (startOfWeek > currentDate) startOfWeek = startOfWeek.AddDays(-7);

            // header strip
            var headerStrip = new Panel { Dock = DockStyle.Top, Height = 40, BackColor = Color.White };
            calendarPanel.Controls.Add(headerStrip);

            int totalWidth = Math.Max(700, calendarPanel.ClientSize.Width);
            int leftColWidth = 60;
            int columnWidth = (totalWidth - leftColWidth) / 7;

            string[] days = { "T2", "T3", "T4", "T5", "T6", "T7", "CN" };
            for (int i = 0; i < 7; i++)
            {
                DateTime day = startOfWeek.AddDays(i);
                var lbl = new Label
                {
                    Text = $"{days[i]} {day:dd/MM}",
                    Width = columnWidth,
                    Height = 40,
                    Location = new Point(leftColWidth + i * columnWidth, 0),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    ForeColor = (day.Date == DateTime.Today ? Color.FromArgb(37, 99, 235) : Color.Black)
                };
                headerStrip.Controls.Add(lbl);
            }

            // scrollable content panel
            var content = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.White };
            calendarPanel.Controls.Add(content);

            int hours = 24;
            int slotHeight = 80;

            // time column and rows inside a large inner panel to enable scrolling
            var inner = new Panel { Location = new Point(0, 0), Width = leftColWidth + 7 * columnWidth, Height = hours * slotHeight }; // will be scrolled
            content.Controls.Add(inner);

            for (int h = 0; h < hours; h++)
            {
                var time = new Label
                {
                    Text = $"{h:D2}:00",
                    Width = leftColWidth,
                    Height = slotHeight,
                    Location = new Point(0, h * slotHeight),
                    Font = new Font("Segoe UI", 9),
                    ForeColor = Color.Gray,
                    TextAlign = ContentAlignment.MiddleRight
                };
                inner.Controls.Add(time);

                for (int d = 0; d < 7; d++)
                {
                    var cell = new Panel
                    {
                        Width = columnWidth,
                        Height = slotHeight,
                        Location = new Point(leftColWidth + d * columnWidth, h * slotHeight),
                        BorderStyle = BorderStyle.FixedSingle
                    };
                    inner.Controls.Add(cell);
                }
            }

            // Render appointment blocks
            foreach (var ev in GetFilteredEvents())
            {
                var localStart = ev.StartUtc.ToLocalTime();
                var localEnd = ev.EndUtc.ToLocalTime();
                if (localStart.Date < startOfWeek.Date || localStart.Date > startOfWeek.AddDays(6).Date)
                    continue;

                int col = (localStart.Date - startOfWeek.Date).Days;
                double minutesFromStart = localStart.Hour * 60 + localStart.Minute;
                double durationMinutes = (localEnd - localStart).TotalMinutes;
                if (minutesFromStart < 0) minutesFromStart = 0;
                if (durationMinutes < 15) durationMinutes = 15;

                int top = (int)(minutesFromStart * slotHeight / 60.0);
                int height = (int)(durationMinutes * slotHeight / 60.0);

                var ap = CreateEventBlock(ev, new Rectangle(leftColWidth + col * columnWidth + 6, top, columnWidth - 12, Math.Max(32, height)));

                string primary = comboBox1.SelectedItem?.ToString() switch
                {
                    "Bác sĩ" => ev.DoctorName,
                    "Khách hàng" => ev.PatientName,
                    "Chuyên môn" => ev.SpecialtyName,
                    _ => ev.DoctorName
                };

                var lbl1 = new Label { Text = primary, AutoSize = false, Height = 18, Dock = DockStyle.Top, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), BackColor = Color.Transparent };
                var lbl2 = new Label { Text = $"{localStart:HH:mm} - {localEnd:HH:mm}", AutoSize = false, Height = 16, Dock = DockStyle.Top, Font = new Font("Segoe UI", 8.5F), ForeColor = Color.FromArgb(71, 85, 105), BackColor = Color.Transparent };
                ap.Controls.Add(lbl2);
                ap.Controls.Add(lbl1);
                ap.Click += (s, e) => MessageBox.Show($"{ev.SpecialtyName}\n{ev.DoctorName}\n{ev.PatientName}\n{ev.StartUtc.ToLocalTime():HH:mm} - {ev.EndUtc.ToLocalTime():HH:mm}", "Chi tiết cuộc hẹn", MessageBoxButtons.OK, MessageBoxIcon.Information);

                inner.Controls.Add(ap);
                ap.BringToFront();
            }
        }

        private void CreateDayView()
        {
            calendarPanel.Controls.Clear();

            int hours = 24;
            int slotHeight = 80;
            int leftColWidth = 70;
            int totalWidth = Math.Max(500, calendarPanel.ClientSize.Width);
            int columnWidth = totalWidth - leftColWidth;

            // header
            var header = new Panel { Dock = DockStyle.Top, Height = 40, BackColor = Color.White };
            var headerLabel = new Label { Text = currentDate.ToString("dddd, dd/MM/yyyy", new CultureInfo("vi-VN")), Dock = DockStyle.Fill, Font = new Font("Segoe UI", 12, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter };
            header.Controls.Add(headerLabel);
            calendarPanel.Controls.Add(header);

            var content = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, AutoScroll = true };
            calendarPanel.Controls.Add(content);

            var inner = new Panel { Location = new Point(0, 0), Width = leftColWidth + columnWidth, Height = hours * slotHeight };
            content.Controls.Add(inner);

            for (int h = 0; h < hours; h++)
            {
                var time = new Label { Text = $"{h:D2}:00", Width = leftColWidth, Height = slotHeight, Location = new Point(0, h * slotHeight), Font = new Font("Segoe UI", 9), ForeColor = Color.Gray, TextAlign = ContentAlignment.MiddleRight };
                inner.Controls.Add(time);
                var cell = new Panel { Width = columnWidth, Height = slotHeight, Location = new Point(leftColWidth, h * slotHeight), BorderStyle = BorderStyle.FixedSingle };
                inner.Controls.Add(cell);
            }

            DateTime dayStart = currentDate.Date;
            foreach (var ev in GetFilteredEvents().Where(e => e.StartUtc.ToLocalTime().Date == dayStart))
            {
                var localStart = ev.StartUtc.ToLocalTime();
                var localEnd = ev.EndUtc.ToLocalTime();
                double minutesFromStart = localStart.Hour * 60 + localStart.Minute;
                double durationMinutes = (localEnd - localStart).TotalMinutes;
                if (minutesFromStart < 0) minutesFromStart = 0;
                if (durationMinutes < 15) durationMinutes = 15;

                int top = (int)(minutesFromStart * slotHeight / 60.0);
                int height = (int)(durationMinutes * slotHeight / 60.0);

                var ap = CreateEventBlock(ev, new Rectangle(leftColWidth + 6, top, columnWidth - 12, Math.Max(32, height)));

                string primary = comboBox1.SelectedItem?.ToString() switch
                {
                    "Bác sĩ" => ev.DoctorName,
                    "Khách hàng" => ev.PatientName,
                    "Chuyên môn" => ev.SpecialtyName,
                    _ => ev.DoctorName
                };

                var lbl1 = new Label { Text = primary, AutoSize = false, Height = 18, Dock = DockStyle.Top, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), BackColor = Color.Transparent };
                var lbl2 = new Label { Text = $"{localStart:HH:mm} - {localEnd:HH:mm}", AutoSize = false, Height = 16, Dock = DockStyle.Top, Font = new Font("Segoe UI", 8.5F), ForeColor = Color.FromArgb(71, 85, 105), BackColor = Color.Transparent };
                ap.Controls.Add(lbl2);
                ap.Controls.Add(lbl1);
                ap.Click += (s, e) => MessageBox.Show($"{ev.SpecialtyName}\n{ev.DoctorName}\n{ev.PatientName}\n{ev.StartUtc.ToLocalTime():HH:mm} - {ev.EndUtc.ToLocalTime():HH:mm}", "Chi tiết cuộc hẹn", MessageBoxButtons.OK, MessageBoxIcon.Information);

                inner.Controls.Add(ap);
                ap.BringToFront();
            }
        }


        private async Task LoadMonthEventsAsync()
        {
            _loadCts?.Cancel();
            _loadCts = new CancellationTokenSource();
            var token = _loadCts.Token;
            var firstDay = new DateTime(currentDate.Year, currentDate.Month, 1);
            var lastDay = firstDay.AddMonths(1).AddDays(-1);
            if (_lastLoadedMonth.Year == firstDay.Year && _lastLoadedMonth.Month == firstDay.Month)
                return;
            try
            {
                // Use doctor client when available to avoid calling admin endpoints (403)
                IReadOnlyList<CalendarEventDto> events;
                if (_doctorAppointmentsApiClient != null)
                {
                    events = await _doctorAppointmentsApiClient.GetCalendarEventsAsync(
                        from: DateOnly.FromDateTime(firstDay),
                        to: DateOnly.FromDateTime(lastDay),
                        cancellationToken: token);
                }
                else if (_adminAppointmentsApiClient != null)
                {
                    events = await _adminAppointmentsApiClient.GetCalendarEventsAsync(
                        from: DateOnly.FromDateTime(firstDay),
                        to: DateOnly.FromDateTime(lastDay),
                        doctorIds: null,
                        cancellationToken: token);
                }
                else
                {
                    events = Array.Empty<CalendarEventDto>();
                }

                _events = events.ToList();
                _lastLoadedMonth = firstDay;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Không thể tải lịch: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _events = new List<CalendarEventDto>();
            }
        }

        private Panel CreateDayCell(int day, bool isOtherMonth, int col, int row, int width, int height, int sidePadding, int cellGap)
        {
            DateTime cellDate = new DateTime(currentDate.Year, currentDate.Month, 1).AddDays(day - 1);
            if (isOtherMonth)
            {
                if (day > 20) // đầu tháng trước
                {
                    var prevMonth = currentDate.AddMonths(-1);
                    cellDate = new DateTime(prevMonth.Year, prevMonth.Month, day);
                }
                else // đầu tháng sau
                {
                    var nextMonth = currentDate.AddMonths(1);
                    cellDate = new DateTime(nextMonth.Year, nextMonth.Month, day);
                }
            }
            Panel cell = new RoundedPanel
            {
                Location = new Point(sidePadding + col * (width + cellGap), 46 + row * (height + cellGap)),
                Size = new Size(width, height),
                BackColor = isOtherMonth ? Color.FromArgb(248, 250, 252) : Color.White,
                Cursor = Cursors.Hand,
                Padding = new Padding(10, 8, 10, 8)
            };
            if (cell is RoundedPanel roundedCell)
            {
                roundedCell.BorderColor = Color.FromArgb(226, 232, 240);
                roundedCell.BorderThickness = 1;
                roundedCell.CornerRadius = 8;
            }

            if (cellDate.Date == DateTime.Today)
            {
                if (cell is RoundedPanel todayCell)
                {
                    todayCell.BorderColor = Color.FromArgb(59, 130, 246);
                    todayCell.BorderThickness = 2;
                }
            }

            Label dayLabel = new Label
            {
                Text = day.ToString(),
                Location = new Point(2, 0),
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = isOtherMonth ? Color.FromArgb(148, 163, 184) : Color.FromArgb(51, 65, 85)
            };
            cell.Controls.Add(dayLabel);

            // Render appointments for this day
            var evs = GetFilteredEvents().Where(ev => ev.StartUtc.ToLocalTime().Date == cellDate.Date).ToList();
            int y = 26;
            foreach (var ev in evs)
            {
                string display = comboBox1.SelectedItem?.ToString() switch
                {
                    "Bác sĩ" => ev.DoctorName,
                    "Khách hàng" => ev.PatientName,
                    "Chuyên môn" => ev.SpecialtyName,
                    _ => ev.DoctorName
                };

                var chip = CreateEventChip(ev, display, new Rectangle(2, y, cell.Width - 8, 20));
                chip.Click += (s, e) =>
                {
                    if (s is not Control control) return;
                    if (control.Tag is not CalendarEventDto evt) return;
                    MessageBox.Show($"{evt.DoctorName}\n{evt.StartUtc.ToLocalTime():HH:mm} - {evt.EndUtc.ToLocalTime():HH:mm}\n{evt.SpecialtyName}", "Chi tiết cuộc hẹn", MessageBoxButtons.OK, MessageBoxIcon.Information);
                };
                cell.Controls.Add(chip);
                y += 22;
            }
            return cell;
        }

        private async void RefreshCalendar()
        {
            Label? monthLabel = navigationPanel.Controls["monthLabel"] as Label;
            // center monthLabel between prev and next
            if (monthLabel != null)
            {
                // First set the text based on the current view so measurement will be correct
                if (currentView == "Month")
                {
                    // Hiển thị dạng số ngắn: MM/yyyy (ví dụ 11/2025)
                    monthLabel.Text = currentDate.ToString("MM/yyyy", new CultureInfo("vi-VN"));
                }
                else if (currentView == "Week")
                {
                    var startOfWeek = currentDate.AddDays(-(int)currentDate.DayOfWeek + 1);
                    var endOfWeek = startOfWeek.AddDays(6);
                    monthLabel.Text = $"{startOfWeek:dd/MM} - {endOfWeek:dd/MM/yyyy}";
                }
                else if (currentView == "Day")
                {
                    // Hiển thị: Thứ, dd/MM/yyyy
                    monthLabel.Text = currentDate.ToString("dddd, dd/MM/yyyy", new CultureInfo("vi-VN"));
                }

                // Ensure label sizes to fit the new text, then center it between prev and next
                monthLabel.AutoSize = true;
                Size measure = TextRenderer.MeasureText(monthLabel.Text, monthLabel.Font);
                // add a small horizontal padding so text isn't flush against bounds
                monthLabel.Size = new Size(measure.Width + 8, 40);
                monthLabel.TextAlign = ContentAlignment.MiddleCenter;

                int midX = (prevBtn.Location.X + prevBtn.Width + nextBtn.Location.X) / 2;
                monthLabel.Location = new Point(Math.Max(0, midX - monthLabel.Width / 2), monthLabel.Location.Y);

                // Now create the selected view (after text is set and centered)
                if (currentView == "Month")
                {
                    await LoadMonthEventsAsync();
                    BuildDoctorFilters();
                    CreateCalendar();
                }
                else if (currentView == "Week")
                {
                    BuildDoctorFilters();
                    CreateWeekView();
                }
                else if (currentView == "Day")
                {
                    BuildDoctorFilters();
                    CreateDayView();
                }
            }
        }

        // Các class helper giữ nguyên
        public class CircularPictureBox : PictureBox
        {
            protected override void OnPaint(PaintEventArgs e)
            {
                GraphicsPath path = new GraphicsPath();
                path.AddEllipse(0, 0, this.Width - 1, this.Height - 1);
                this.Region = new Region(path);

                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (SolidBrush brush = new SolidBrush(this.BackColor))
                {
                    e.Graphics.FillEllipse(brush, 0, 0, this.Width - 1, this.Height - 1);
                }
            }
        }

        public class RoundedButton1 : Button
        {
            protected override void OnPaint(PaintEventArgs e)
            {
                GraphicsPath path = new GraphicsPath();
                int radius = 6;
                Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);

                path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
                path.AddArc(rect.X + rect.Width - radius, rect.Y, radius, radius, 270, 90);
                path.AddArc(rect.X + rect.Width - radius, rect.Y + rect.Height - radius, radius, radius, 0, 90);
                path.AddArc(rect.X, rect.Y + rect.Height - radius, radius, radius, 90, 90);
                path.CloseFigure();

                this.Region = new Region(path);
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                using (SolidBrush brush = new SolidBrush(this.BackColor))
                {
                    e.Graphics.FillPath(brush, path);
                }

                if (this.FlatAppearance.BorderSize > 0)
                {
                    using (Pen pen = new Pen(this.FlatAppearance.BorderColor, this.FlatAppearance.BorderSize))
                    {
                        e.Graphics.DrawPath(pen, path);
                    }
                }

                TextRenderer.DrawText(
                    e.Graphics,
                    this.Text,
                    this.Font,
                    this.ClientRectangle,
                    this.ForeColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
                );
            }
        }

        public class RoundedPanel : Panel
        {
            [Browsable(false)]
            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public int CornerRadius { get; set; } = 8;

            [Browsable(false)]
            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public int BorderThickness { get; set; } = 1;

            [Browsable(false)]
            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public Color BorderColor { get; set; } = Color.FromArgb(226, 232, 240);

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                using (var path = GetRoundedRectanglePath(ClientRectangle, CornerRadius))
                using (var borderPen = new Pen(BorderColor, BorderThickness))
                using (var brush = new SolidBrush(BackColor))
                {
                    e.Graphics.FillPath(brush, path);
                    if (BorderThickness > 0)
                    {
                        e.Graphics.DrawPath(borderPen, path);
                    }
                    Region = new Region(path);
                }
            }

            private static GraphicsPath GetRoundedRectanglePath(Rectangle bounds, int radius)
            {
                int diameter = radius * 2;
                var path = new GraphicsPath();
                var arc = new Rectangle(bounds.Location, new Size(diameter, diameter));

                path.AddArc(arc, 180, 90);
                arc.X = bounds.Right - diameter;
                path.AddArc(arc, 270, 90);
                arc.Y = bounds.Bottom - diameter;
                path.AddArc(arc, 0, 90);
                arc.X = bounds.Left;
                path.AddArc(arc, 90, 90);
                path.CloseFigure();

                return path;
            }
        }

        private void BuildDoctorFilters()
        {
            if (_doctorFilterPanel == null)
            {
                return;
            }

            UpdateSpecialtyFilterItems();

            _doctorFilterPanel.SuspendLayout();
            _doctorFilterPanel.Controls.Clear();

            var allButton = CreateFilterButton("Tất cả bác sĩ", isActive: _selectedDoctorName == null);
            allButton.Click += (s, e) =>
            {
                _selectedDoctorName = null;
                if (s is Button btn)
                {
                    SetActiveDoctorFilter(btn);
                }
                RefreshCalendar();
            };
            _doctorFilterPanel.Controls.Add(allButton);

            var doctors = GetAvailableDoctors();

            foreach (var doctor in doctors)
            {
                var btn = CreateFilterButton(doctor!, isActive: string.Equals(_selectedDoctorName, doctor, StringComparison.CurrentCultureIgnoreCase));
                btn.Click += (s, e) =>
                {
                    _selectedDoctorName = doctor;
                    if (s is Button button)
                    {
                        SetActiveDoctorFilter(button);
                    }
                    RefreshCalendar();
                };
                _doctorFilterPanel.Controls.Add(btn);
            }

            if (_activeDoctorFilterButton == null && _doctorFilterPanel.Controls.Count > 0)
            {
                _activeDoctorFilterButton = _doctorFilterPanel.Controls[0] as Button;
            }

            EnsureDoctorFilterToggleVisibility();

            _doctorFilterPanel.ResumeLayout();
        }

        private Button CreateFilterButton(string text, bool isActive)
        {
            var btn = new Button
            {
                Text = text,
                AutoSize = true,
                Height = 30,
                Padding = new Padding(12, 4, 12, 4),
                Margin = new Padding(0, 0, 8, 0),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = Color.FromArgb(226, 232, 240);

            if (isActive)
            {
                btn.BackColor = Color.FromArgb(37, 99, 235);
                btn.ForeColor = Color.White;
                btn.FlatAppearance.BorderColor = Color.FromArgb(37, 99, 235);
                _activeDoctorFilterButton = btn;
            }
            else
            {
                btn.BackColor = Color.White;
                btn.ForeColor = Color.FromArgb(55, 65, 81);
            }

            btn.MouseEnter += (s, e) =>
            {
                if (s is not Button hovered) return;
                if (hovered == _activeDoctorFilterButton) return;
                hovered.BackColor = Color.FromArgb(241, 245, 249);
            };
            btn.MouseLeave += (s, e) =>
            {
                if (s is not Button hovered) return;
                if (hovered == _activeDoctorFilterButton) return;
                hovered.BackColor = Color.White;
            };

            return btn;
        }

        private void EnableDoubleBuffering(Control control)
        {
            typeof(Control).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(control, true, null);
        }

        private void SetActiveDoctorFilter(Button btn)
        {
            if (_activeDoctorFilterButton != null && _activeDoctorFilterButton != btn)
            {
                _activeDoctorFilterButton.BackColor = Color.White;
                _activeDoctorFilterButton.ForeColor = Color.FromArgb(55, 65, 81);
                _activeDoctorFilterButton.FlatAppearance.BorderColor = Color.FromArgb(226, 232, 240);
            }

            btn.BackColor = Color.FromArgb(37, 99, 235);
            btn.ForeColor = Color.White;
            btn.FlatAppearance.BorderColor = Color.FromArgb(37, 99, 235);
            _activeDoctorFilterButton = btn;
        }

        private IEnumerable<CalendarEventDto> GetFilteredEvents()
        {
            IEnumerable<CalendarEventDto> result = _events;
            if (!string.IsNullOrWhiteSpace(_selectedSpecialty))
            {
                result = result.Where(e => string.Equals(e.SpecialtyName, _selectedSpecialty, StringComparison.CurrentCultureIgnoreCase));
            }
            if (!string.IsNullOrWhiteSpace(_selectedDoctorName))
            {
                result = result.Where(e => string.Equals(e.DoctorName, _selectedDoctorName, StringComparison.CurrentCultureIgnoreCase));
            }
            return result;
        }

        private List<string> GetAvailableDoctors()
        {
            var query = _events.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(_selectedSpecialty))
            {
                query = query.Where(e => string.Equals(e.SpecialtyName, _selectedSpecialty, StringComparison.CurrentCultureIgnoreCase));
            }

            var doctors = query
                .Select(e => e.DoctorName)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.CurrentCultureIgnoreCase)
                .OrderBy(name => name)
                .ToList();

            var searchText = _doctorSearchTextBox?.Text;
            if (!string.IsNullOrWhiteSpace(searchText) && searchText != "Tìm bác sĩ")
            {
                doctors = doctors
                    .Where(d => d.Contains(searchText, StringComparison.CurrentCultureIgnoreCase))
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(_selectedDoctorName) && !doctors.Contains(_selectedDoctorName, StringComparer.CurrentCultureIgnoreCase))
            {
                _selectedDoctorName = null;
                _activeDoctorFilterButton = null;
            }

            return doctors;
        }

        private void UpdateSpecialtyFilterItems()
        {
            if (_specialtyFilterComboBox == null) return;

            _isUpdatingFilters = true;

            var selected = _selectedSpecialty;
            var specialties = _events.Select(e => e.SpecialtyName)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.CurrentCultureIgnoreCase)
                .OrderBy(name => name)
                .ToList();

            _specialtyFilterComboBox.Items.Clear();
            _specialtyFilterComboBox.Items.Add("Tất cả chuyên khoa");
            foreach (var specialty in specialties)
            {
                _specialtyFilterComboBox.Items.Add(specialty!);
            }

            if (string.IsNullOrWhiteSpace(selected))
            {
                _specialtyFilterComboBox.SelectedIndex = 0;
            }
            else
            {
                var index = _specialtyFilterComboBox.Items.IndexOf(selected);
                _specialtyFilterComboBox.SelectedIndex = index >= 0 ? index : 0;
            }

            _isUpdatingFilters = false;
        }

        private void ToggleDoctorFilterWrap()
        {
            if (_doctorFilterPanel == null || _doctorFilterToggleButton == null) return;

            _isDoctorFilterExpanded = !_isDoctorFilterExpanded;
            _doctorFilterPanel.WrapContents = _isDoctorFilterExpanded;
            _doctorFilterPanel.AutoScroll = !_isDoctorFilterExpanded;
            _doctorFilterToggleButton.Text = _isDoctorFilterExpanded ? "Thu gọn" : "Xem thêm";
            userPanel.Height = _isDoctorFilterExpanded ? 200 : 130;
        }

        private void EnsureDoctorFilterToggleVisibility()
        {
            if (_doctorFilterPanel == null || _doctorFilterToggleButton == null) return;

            var totalWidth = _doctorFilterPanel.Controls.Cast<Control>().Sum(c => c.Width + c.Margin.Horizontal);
            bool shouldShowToggle = totalWidth > _doctorFilterPanel.Width;
            _doctorFilterToggleButton.Visible = shouldShowToggle;
        }

        private Panel CreateEventChip(CalendarEventDto ev, string text, Rectangle bounds)
        {
            var bg = PickEventColor(text);
            var chip = new RoundedPanel
            {
                Location = bounds.Location,
                Size = bounds.Size,
                BackColor = bg,
                BorderColor = Color.FromArgb(229, 231, 235),
                BorderThickness = 1,
                CornerRadius = 6,
                Tag = ev
            };
            var label = new Label
            {
                Text = text,
                Dock = DockStyle.Fill,
                AutoEllipsis = true,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
                ForeColor = Color.FromArgb(15, 23, 42),
                BackColor = Color.Transparent,
                Padding = new Padding(6, 0, 6, 0)
            };
            chip.Controls.Add(label);
            return chip;
        }

        private Panel CreateEventBlock(CalendarEventDto ev, Rectangle bounds)
        {
            var bg = PickEventColor(ev.DoctorName ?? ev.SpecialtyName ?? "Event");
            var panel = new RoundedPanel
            {
                Location = bounds.Location,
                Size = bounds.Size,
                BackColor = bg,
                BorderColor = Color.FromArgb(226, 232, 240),
                BorderThickness = 1,
                CornerRadius = 8,
                Cursor = Cursors.Hand,
                Tag = ev,
                Padding = new Padding(6)
            };
            return panel;
        }

        private Color PickEventColor(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return EventPalette[0];
            }
            int idx = Math.Abs(key.GetHashCode()) % EventPalette.Length;
            return EventPalette[idx];
        }


        // Giữ nguyên class AppointmentDialog
        public class AppointmentDialog : Form
        {
            // ... (giữ nguyên toàn bộ code của AppointmentDialog)
            private readonly CustomerService? _customerService;
            private ComboBox serviceComboBox = null!;
            private ComboBox employeeComboBox = null!;
            private DateTimePicker datePicker = null!;
            private ComboBox timeComboBox = null!;
            private TextBox customerTextBox = null!;
            private CheckBox notificationCheckBox = null!;
            private Button cancelBtn = null!;
            private Button saveBtn = null!;

            public AppointmentDialog(CustomerService? customerService = null)
            {
                _customerService = customerService;
                InitializeComponent1();
            }

            private void InitializeComponent1()
            {
                // ... (giữ nguyên toàn bộ code khởi tạo của AppointmentDialog)
                this.Text = "Thêm cuộc hẹn";
                this.Size = new Size(600, 520);
                this.BackColor = Color.White;
                this.StartPosition = FormStartPosition.CenterParent;
                this.FormBorderStyle = FormBorderStyle.FixedDialog;
                this.MaximizeBox = false;
                this.MinimizeBox = false;

                // Tiêu đề header
                Label headerLabel = new Label
                {
                    Text = "Thêm cuộc hẹn",
                    Location = new Point(30, 15),
                    Size = new Size(500, 35),
                    Font = new Font("Segoe UI", 16, FontStyle.Bold),
                    ForeColor = Color.FromArgb(17, 24, 39)
                };

                // Label "Services"
                Label serviceLabel = new Label
                {
                    Text = "Dịch vụ",
                    Location = new Point(30, 70),
                    Size = new Size(100, 20),
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    ForeColor = Color.FromArgb(17, 24, 39)
                };

                // ComboBox chọn dịch vụ
                serviceComboBox = new ComboBox
                {
                    Location = new Point(30, 95),
                    Size = new Size(520, 35),
                    Font = new Font("Segoe UI", 10),
                    DropDownStyle = ComboBoxStyle.DropDownList,
                };
                serviceComboBox.Items.AddRange(new string[] { "Khám tổng quát", "Nha khoa", "Tim mạch", "Nhi khoa" });

                // Label "Employees"
                Label employeeLabel = new Label
                {
                    Text = "Bác sĩ",
                    Location = new Point(30, 140),
                    Size = new Size(100, 20),
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    ForeColor = Color.FromArgb(17, 24, 39)
                };

                // ComboBox chọn nhân viên
                employeeComboBox = new ComboBox
                {
                    Location = new Point(30, 165),
                    Size = new Size(520, 35),
                    Font = new Font("Segoe UI", 10),
                    DropDownStyle = ComboBoxStyle.DropDownList,
                };
                employeeComboBox.Items.AddRange(new string[] { "BS. Nguyễn Văn A", "BS. Trần Thị B", "BS. Lê Văn C" });

                // Label "Date"
                Label dateLabel = new Label
                {
                    Text = "Ngày diễn ra",
                    Location = new Point(30, 210),
                    Size = new Size(150, 25),
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    ForeColor = Color.FromArgb(17, 24, 39)
                };

                // DatePicker chọn ngày
                datePicker = new DateTimePicker
                {
                    Location = new Point(30, 235),
                    Size = new Size(250, 35),
                    Font = new Font("Segoe UI", 10),
                    Format = DateTimePickerFormat.Short
                };

                // Label "Time"
                Label timeLabel = new Label
                {
                    Text = "Thời gian diễn ra",
                    Location = new Point(300, 210),
                    Size = new Size(150, 25),
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    ForeColor = Color.FromArgb(17, 24, 39)
                };

                // ComboBox chọn giờ
                timeComboBox = new ComboBox
                {
                    Location = new Point(300, 235),
                    Size = new Size(250, 35),
                    Font = new Font("Segoe UI", 10),
                    DropDownStyle = ComboBoxStyle.DropDownList,
                };
                for (int hour = 8; hour <= 17; hour++)
                {
                    timeComboBox.Items.Add($"{hour:D2}:00");
                    timeComboBox.Items.Add($"{hour:D2}:30");
                }

                // Label "Customers"
                Label customerLabel = new Label
                {
                    Text = "Khách hàng",
                    Location = new Point(30, 280),
                    Size = new Size(300, 25),
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    ForeColor = Color.FromArgb(17, 24, 39)
                };

                // Link thêm khách hàng mới
                LinkLabel newCustomerLink = new LinkLabel
                {
                    Text = "+ Khách hàng mới",
                    Location = new Point(400, 280),
                    Size = new Size(250, 20),
                    Font = new Font("Segoe UI", 9),
                    LinkColor = Color.FromArgb(37, 99, 235),
                    ActiveLinkColor = Color.FromArgb(37, 99, 235),
                    VisitedLinkColor = Color.FromArgb(37, 99, 235)
                };
                newCustomerLink.Click += (s, e) =>
                {
                    AddCustomerForm addCustomerForm = _customerService != null
                        ? new AddCustomerForm(_customerService)
                        : new AddCustomerForm();
                    DialogResult result = addCustomerForm.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        MessageBox.Show("Khách hàng đã được thêm thành công!", "Thông báo",
                                      MessageBoxButtons.OK, MessageBoxIcon.Information);

                        RefreshCustomerList();
                    }

                };

                // Panel chứa TextBox nhập tên khách hàng (có icon)
                Panel customerPanel = new Panel
                {
                    Location = new Point(30, 305),
                    Size = new Size(520, 35),
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = Color.White
                };

                // Icon người dùng
                Label customerIcon = new Label
                {
                    Text = "👥",
                    Location = new Point(5, 5),
                    Size = new Size(25, 25),
                    Font = new Font("Segoe UI", 12),
                    TextAlign = ContentAlignment.MiddleCenter
                };

                // TextBox nhập tên khách hàng
                customerTextBox = new TextBox
                {
                    Location = new Point(35, 5),
                    Size = new Size(475, 25),
                    Font = new Font("Segoe UI", 10),
                    BorderStyle = BorderStyle.None,
                    Text = "Gõ tên khách hàng"
                };
                customerTextBox.ForeColor = Color.Gray;
                customerTextBox.Enter += (s, e) =>
                {
                    if (customerTextBox.Text == "Gõ tên khách hàng")
                    {
                        customerTextBox.Text = "";
                        customerTextBox.ForeColor = Color.Black;
                    }
                };
                customerTextBox.Leave += (s, e) =>
                {
                    if (string.IsNullOrWhiteSpace(customerTextBox.Text))
                    {
                        customerTextBox.Text = "Gõ tên khách hàng";
                        customerTextBox.ForeColor = Color.Gray;
                    }
                };

                customerPanel.Controls.Add(customerIcon);
                customerPanel.Controls.Add(customerTextBox);

                // Checkbox gửi thông báo cho khách hàng
                notificationCheckBox = new CheckBox
                {
                    Text = "Gửi thông báo tới khách hàng",
                    Location = new Point(30, 355),
                    Size = new Size(300, 30),
                    Font = new Font("Segoe UI", 10),
                    Checked = true
                };

                // Nút Cancel (Hủy)
                cancelBtn = new RoundedButton1
                {
                    Text = "Hủy",
                    Location = new Point(360, 400),
                    Size = new Size(90, 40),
                    BackColor = Color.White,
                    ForeColor = Color.FromArgb(55, 65, 81),
                    Font = new Font("Segoe UI", 10),
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand
                };
                cancelBtn.FlatAppearance.BorderSize = 1;
                cancelBtn.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);
                cancelBtn.Click += (s, e) => this.Close();

                // Nút Save (Lưu)
                saveBtn = new RoundedButton1
                {
                    Text = "Lưu",
                    Location = new Point(460, 400),
                    Size = new Size(90, 40),
                    BackColor = Color.FromArgb(37, 99, 235),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand
                };
                saveBtn.FlatAppearance.BorderSize = 0;
                saveBtn.Click += SaveBtn_Click;

                // Thêm tất cả controls vào form
                this.Controls.Add(headerLabel);
                this.Controls.Add(serviceLabel);
                this.Controls.Add(serviceComboBox);
                this.Controls.Add(employeeLabel);
                this.Controls.Add(employeeComboBox);
                this.Controls.Add(dateLabel);
                this.Controls.Add(datePicker);
                this.Controls.Add(timeLabel);
                this.Controls.Add(timeComboBox);
                this.Controls.Add(customerLabel);
                this.Controls.Add(newCustomerLink);
                this.Controls.Add(customerPanel);
                this.Controls.Add(notificationCheckBox);
                this.Controls.Add(cancelBtn);
                this.Controls.Add(saveBtn);
            }

            private void RefreshCustomerList()
            {
                // Code để refresh danh sách khách hàng
            }

            private void SaveBtn_Click(object? sender, EventArgs e)
            {
                if (serviceComboBox.SelectedIndex == -1)
                {
                    MessageBox.Show("Vui lòng chọn dịch vụ", "Lỗi xác thực", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (employeeComboBox.SelectedIndex == -1)
                {
                    MessageBox.Show("Vui lòng chọn bác sĩ", "Lỗi xác thực", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (timeComboBox.SelectedIndex == -1)
                {
                    MessageBox.Show("Vui lòng chọn thời gian", "Lỗi xác thực", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(customerTextBox.Text) || customerTextBox.Text == "Gõ tên khách hàng")
                {
                    MessageBox.Show("Vui lòng nhập tên khách hàng", "Lỗi xác thực", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string message = $"Tạo cuộc hẹn thành công!\n\n" +
                               $"Dịch vụ: {serviceComboBox.SelectedItem}\n" +
                               $"Bác sĩ: {employeeComboBox.SelectedItem}\n" +
                               $"Ngày: {datePicker.Value:dd/MM/yyyy}\n" +
                               $"Thời gian: {timeComboBox.SelectedItem}\n" +
                               $"Khách hàng: {customerTextBox.Text}\n" +
                               $"Thông báo: {(notificationCheckBox.Checked ? "Có" : "Không")}";

                MessageBox.Show(message, "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void newAppointmentBtn_Click(object sender, EventArgs e)
        {
            AppointmentDialog dialog = new AppointmentDialog(_customerService);
            dialog.ShowDialog();   // mở form Add Appointment dạng popup (modal)

        }

        private void prevBtn_Click(object sender, EventArgs e)
        {

        }

        private void btnToday_Click(object sender, EventArgs e)
        {
            currentDate = DateTime.Now;
            RefreshCalendar();
        }

        private void monthBtn_Click(object sender, EventArgs e)
        {
            currentView = "Month";
            RefreshCalendar();
        }

        private void weekBtn_Click(object sender, EventArgs e)
        {
            currentView = "Week";
            RefreshCalendar();
        }

        private void dayBtn_Click(object sender, EventArgs e)
        {
            currentView = "Day";
            RefreshCalendar();
        }
    }
}
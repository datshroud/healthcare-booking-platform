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

namespace BookingCareManagement.WinForms
{
    public partial class Calendar : Form
    {
        private DateTime currentDate;
        private string currentView = "Month";
        private readonly AdminAppointmentsApiClient _appointmentsApiClient;
        private List<CalendarEventDto> _events = new();
        private CancellationTokenSource? _loadCts;
        private DateTime _lastLoadedMonth = DateTime.MinValue;

        // Biến cho drag & drop
        private Label draggedAppointment = null;
        private Point dragStartPoint;
        private Panel sourcePanel = null;

        // state for active view
        private Button? _activeViewButton;

        // Chỉ giữ lại constructor DI
        public Calendar(AdminAppointmentsApiClient appointmentsApiClient)
        {
            _appointmentsApiClient = appointmentsApiClient;
            currentDate = DateTime.Now;
            InitializeComponent();
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
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
                // no hover for view buttons
            }

            // primary action (add) keep static blue, no hover/active
            if (newAppointmentBtn != null)
            {
                newAppointmentBtn.FlatStyle = FlatStyle.Flat;
                newAppointmentBtn.FlatAppearance.BorderSize = 1;
                newAppointmentBtn.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);
                newAppointmentBtn.BackColor = Color.FromArgb(37, 99, 235);
                newAppointmentBtn.ForeColor = Color.White;
                newAppointmentBtn.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
                newAppointmentBtn.Cursor = Cursors.Hand;
                // remove hover handlers if any
                newAppointmentBtn.MouseEnter -= (s, e) => newAppointmentBtn.BackColor = Color.FromArgb(29, 78, 216);
                newAppointmentBtn.MouseLeave -= (s, e) => newAppointmentBtn.BackColor = Color.FromArgb(37, 99, 235);
            }

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
            int headerY = 15;
            int cellWidth = (this.ClientSize.Width - 60) / 7;

            for (int i = 0; i < 7; i++)
            {
                Label dayHeader = new Label
                {
                    Text = dayNames[i],
                    Location = new Point(15 + i * cellWidth, headerY),
                    Size = new Size(cellWidth, 30),
                    Font = new Font("Segoe UI", 11, FontStyle.Bold),
                    ForeColor = Color.FromArgb(55, 65, 81),
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

            int cellHeight = 150;
            int currentRow = 0;
            int currentCol = startDay;
            int dayCounter = 1;

            // Ngày tháng trước
            DateTime prevMonth = currentDate.AddMonths(-1);
            int prevMonthDays = DateTime.DaysInMonth(prevMonth.Year, prevMonth.Month);
            int prevStart = prevMonthDays - startDay + 1;

            for (int i = 0; i < startDay; i++)
            {
                Panel dayCell = CreateDayCell(prevStart + i, true, i, currentRow, cellWidth, cellHeight);
                calendarPanel.Controls.Add(dayCell);
            }

            // Ngày tháng hiện tại
            while (dayCounter <= daysInMonth)
            {
                Panel dayCell = CreateDayCell(dayCounter, false, currentCol, currentRow, cellWidth, cellHeight);

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
                Panel dayCell = CreateDayCell(nextDay, true, currentCol, currentRow, cellWidth, cellHeight);
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
            int slotHeight = 60;

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
            foreach (var ev in _events)
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

                var ap = new Panel
                {
                    Location = new Point(leftColWidth + col * columnWidth + 6, top),
                    Size = new Size(columnWidth - 12, Math.Max(24, height)),
                    BackColor = Color.FromArgb(207, 232, 255),
                    Cursor = Cursors.Hand,
                    Tag = ev
                };
                ap.Padding = new Padding(6);

                string primary = comboBox1.SelectedItem?.ToString() switch
                {
                    "Bác sĩ" => ev.DoctorName,
                    "Khách hàng" => ev.PatientName,
                    "Chuyên môn" => ev.SpecialtyName,
                    _ => ev.DoctorName
                };

                var lbl1 = new Label { Text = primary, AutoSize = false, Height = 18, Dock = DockStyle.Top, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(17, 24, 39) };
                var lbl2 = new Label { Text = $"{localStart:HH:mm} - {localEnd:HH:mm}", AutoSize = false, Height = 16, Dock = DockStyle.Top, Font = new Font("Segoe UI", 8.5F), ForeColor = Color.FromArgb(55, 65, 81) };
                ap.Controls.Add(lbl2); ap.Controls.Add(lbl1);
                ap.Click += (s, e) => MessageBox.Show($"{ev.SpecialtyName}\n{ev.DoctorName}\n{ev.PatientName}\n{ev.StartUtc.ToLocalTime():HH:mm} - {ev.EndUtc.ToLocalTime():HH:mm}", "Chi tiết cuộc hẹn", MessageBoxButtons.OK, MessageBoxIcon.Information);

                inner.Controls.Add(ap);
                ap.BringToFront();
            }
        }

        private void CreateDayView()
        {
            calendarPanel.Controls.Clear();

            int hours = 24;
            int slotHeight = 60;
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
            foreach (var ev in _events.Where(e => e.StartUtc.ToLocalTime().Date == dayStart))
            {
                var localStart = ev.StartUtc.ToLocalTime();
                var localEnd = ev.EndUtc.ToLocalTime();
                double minutesFromStart = localStart.Hour * 60 + localStart.Minute;
                double durationMinutes = (localEnd - localStart).TotalMinutes;
                if (minutesFromStart < 0) minutesFromStart = 0;
                if (durationMinutes < 15) durationMinutes = 15;

                int top = (int)(minutesFromStart * slotHeight / 60.0);
                int height = (int)(durationMinutes * slotHeight / 60.0);

                var ap = new Panel { Location = new Point(leftColWidth + 6, top), Size = new Size(columnWidth - 12, Math.Max(24, height)), BackColor = Color.FromArgb(207, 232, 255), Cursor = Cursors.Hand, Tag = ev };
                ap.Padding = new Padding(6);

                string primary = comboBox1.SelectedItem?.ToString() switch
                {
                    "Bác sĩ" => ev.DoctorName,
                    "Khách hàng" => ev.PatientName,
                    "Chuyên môn" => ev.SpecialtyName,
                    _ => ev.DoctorName
                };

                var lbl1 = new Label { Text = primary, AutoSize = false, Height = 18, Dock = DockStyle.Top, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(17, 24, 39) };
                var lbl2 = new Label { Text = $"{localStart:HH:mm} - {localEnd:HH:mm}", AutoSize = false, Height = 16, Dock = DockStyle.Top, Font = new Font("Segoe UI", 8.5F), ForeColor = Color.FromArgb(55, 65, 81) };
                ap.Controls.Add(lbl2); ap.Controls.Add(lbl1);
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
                var events = await _appointmentsApiClient.GetCalendarEventsAsync(
                    from: DateOnly.FromDateTime(firstDay),
                    to: DateOnly.FromDateTime(lastDay),
                    doctorIds: null,
                    cancellationToken: token);
                _events = events.ToList();
                _lastLoadedMonth = firstDay;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Không thể tải lịch: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _events = new List<CalendarEventDto>();
            }
        }

        private Panel CreateDayCell(int day, bool isOtherMonth, int col, int row, int width, int height)
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
            Panel cell = new Panel
            {
                Location = new Point(15 + col * width, 50 + row * height),
                Size = new Size(width - 2, height - 2),
                BackColor = isOtherMonth ? Color.FromArgb(249, 250, 251) : Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Cursor = Cursors.Hand
            };

            Label dayLabel = new Label
            {
                Text = day.ToString(),
                Location = new Point(10, 5),
                AutoSize = true,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = isOtherMonth ? Color.FromArgb(156, 163, 175) : Color.FromArgb(75, 85, 99)
            };
            cell.Controls.Add(dayLabel);

            // Render appointments for this day
            var evs = _events.Where(ev => ev.StartUtc.ToLocalTime().Date == cellDate.Date).ToList();
            int y = 28;
            foreach (var ev in evs)
            {
                string display = comboBox1.SelectedItem?.ToString() switch
                {
                    "Bác sĩ" => ev.DoctorName,
                    "Khách hàng" => ev.PatientName,
                    "Chuyên môn" => ev.SpecialtyName,
                    _ => ev.DoctorName
                };

                var lbl = new Label
                {
                    Text = display,
                    AutoSize = false,
                    Size = new Size(cell.Width - 16, 18),
                    Location = new Point(8, y),
                    BackColor = Color.FromArgb(207, 232, 255),
                    ForeColor = Color.FromArgb(17, 24, 39),
                    Font = new Font("Segoe UI", 9, FontStyle.Regular),
                    Padding = new Padding(2, 0, 2, 0),
                    Tag = ev
                };
                lbl.Click += (s, e) =>
                {
                    var evt = (CalendarEventDto)((Label)s).Tag;
                    MessageBox.Show($"{evt.DoctorName}\n{evt.StartUtc.ToLocalTime():HH:mm} - {evt.EndUtc.ToLocalTime():HH:mm}\n{evt.SpecialtyName}", "Chi tiết cuộc hẹn", MessageBoxButtons.OK, MessageBoxIcon.Information);
                };
                cell.Controls.Add(lbl);
                y += 20;
            }
            return cell;
        }

        private async void RefreshCalendar()
        {
            Label monthLabel = navigationPanel.Controls["monthLabel"] as Label;
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
                    CreateCalendar();
                }
                else if (currentView == "Week")
                {
                    CreateWeekView();
                }
                else if (currentView == "Day")
                {
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


        // Giữ nguyên class AppointmentDialog
        public class AppointmentDialog : Form
        {
            // ... (giữ nguyên toàn bộ code của AppointmentDialog)
            private ComboBox serviceComboBox;
            private ComboBox employeeComboBox;
            private DateTimePicker datePicker;
            private ComboBox timeComboBox;
            private TextBox customerTextBox;
            private CheckBox notificationCheckBox;
            private Button cancelBtn;
            private Button saveBtn;

            public AppointmentDialog()
            {
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
                    AddCustomerForm addCustomerForm = new AddCustomerForm();
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

            private void SaveBtn_Click(object sender, EventArgs e)
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
            AppointmentDialog dialog = new AppointmentDialog();
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
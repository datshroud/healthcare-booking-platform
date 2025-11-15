using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BookingCareManagement.WinForms
{
    public partial class Calendar : Form
    {
        private Panel headerPanel;
        private Panel userPanel;
        private Panel navigationPanel;
        private Panel calendarPanel;
        private Panel contentPanel;
        private DateTime currentDate;
        private string currentView = "Month";
        private RoundedButton1 monthBtn;
        private RoundedButton1 weekBtn;
        private RoundedButton1 dayBtn;

        public Calendar()
        {
            currentDate = new DateTime(2025, 11, 1);
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Text = "Calendar";
            this.Size = new Size(1600, 900);
            this.BackColor = Color.FromArgb(243, 244, 246);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;

            // Panel tiêu đề
            headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.FromArgb(243, 244, 246),
                Padding = new Padding(30, 20, 30, 0)
            };
            CreateHeader();

            // Panel người dùng
            userPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 100,
                BackColor = Color.White,
                Padding = new Padding(30, 20, 30, 20)
            };
            CreateUserPanel();

            // Panel điều hướng
            navigationPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.White,
                Padding = new Padding(30, 15, 30, 15)
            };
            CreateNavigationPanel();

            // Panel lịch chính
            calendarPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(15),
                AutoScroll = true
            };

            // contentPanel dùng chung cho Week/Day
            contentPanel = calendarPanel;

            CreateCalendar();

            this.Controls.Add(calendarPanel);
            this.Controls.Add(navigationPanel);
            this.Controls.Add(userPanel);
            this.Controls.Add(headerPanel);
        }

        private void CreateHeader()
        {
            Label title = new Label
            {
                Text = "Calendar",
                Location = new Point(30, 20),
                AutoSize = true,
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39)
            };

            // Nút tạo lịch hẹn mới
            RoundedButton1 newAppointmentBtn = new RoundedButton1
            {
                Name = "newAppointmentBtn",
                Text = "+  New Appointment",
                Size = new Size(200, 44),
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            newAppointmentBtn.FlatAppearance.BorderSize = 0;
            newAppointmentBtn.Click += (s, e) => MessageBox.Show("Thêm lịch hẹn mới", "Thông báo");

            // Căn chỉnh khi resize
            this.Resize += (s, e) =>
            {
                if (headerPanel.Controls["newAppointmentBtn"] != null)
                {
                    newAppointmentBtn.Location = new Point(this.ClientSize.Width - 230, 18);
                }
            };

            headerPanel.Controls.Add(title);
            headerPanel.Controls.Add(newAppointmentBtn);
        }

        private void CreateUserPanel()
        {
            // Avatar tròn
            CircularPictureBox avatar = new CircularPictureBox
            {
                Location = new Point(30, 10),
                Size = new Size(60, 60),
                BackColor = Color.FromArgb(219, 234, 254),
                SizeMode = PictureBoxSizeMode.Zoom
            };

            Label avatarInitial = new Label
            {
                Text = "👤",
                Location = new Point(30, 10),
                Size = new Size(60, 60),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 24),
                BackColor = Color.Transparent
            };

            Label userName = new Label
            {
                Text = "Jane\nDoe",
                Location = new Point(40, 75),
                Size = new Size(40, 35),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39),
                TextAlign = ContentAlignment.TopCenter
            };

            userPanel.Controls.Add(avatar);
            userPanel.Controls.Add(avatarInitial);
            userPanel.Controls.Add(userName);
            avatarInitial.BringToFront();
        }

        private void CreateNavigationPanel()
        {
            // Nút Today
            RoundedButton1 todayBtn = new RoundedButton1
            {
                Text = "Today",
                Location = new Point(30, 15),
                Size = new Size(80, 40),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(55, 65, 81),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            todayBtn.FlatAppearance.BorderSize = 1;
            todayBtn.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);
            todayBtn.Click += (s, e) =>
            {
                currentDate = DateTime.Now;
                RefreshCalendar();
            };

            // Nút tháng trước
            Button prevBtn = new Button
            {
                Text = "◀",
                Location = new Point(120, 15),
                Size = new Size(40, 40),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(55, 65, 81),
                Font = new Font("Segoe UI", 12),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            prevBtn.FlatAppearance.BorderSize = 0;
            prevBtn.Click += (s, e) =>
            {
                currentDate = currentDate.AddMonths(-1);
                RefreshCalendar();
            };

            // Label tháng/năm
            Label monthLabel = new Label
            {
                Name = "monthLabel",
                Text = currentDate.ToString("MMMM yyyy"),
                Location = new Point(170, 15),
                Size = new Size(200, 40),
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39),
                TextAlign = ContentAlignment.MiddleLeft
            };

            // Nút tháng sau
            Button nextBtn = new Button
            {
                Text = "▶",
                Location = new Point(300, 15),
                Size = new Size(40, 40),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(55, 65, 81),
                Font = new Font("Segoe UI", 12),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            nextBtn.FlatAppearance.BorderSize = 0;
            nextBtn.Click += (s, e) =>
            {
                currentDate = currentDate.AddMonths(1);
                RefreshCalendar();
            };

            // Tính vị trí phía bên phải
            int rightX = this.ClientSize.Width - 600;

            // Nút Options
            RoundedButton1 optionsBtn = new RoundedButton1
            {
                Name = "optionsBtn",
                Text = "Options ▼",
                Size = new Size(120, 40),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(55, 65, 81),
                Font = new Font("Segoe UI", 10),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            optionsBtn.FlatAppearance.BorderSize = 1;
            optionsBtn.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);

            // ===== NÚT VIEW (MONTH / WEEK / DAY) =====
            monthBtn = new RoundedButton1
            {
                Name = "monthBtn",
                Text = "Month",
                Size = new Size(80, 40),
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            monthBtn.FlatAppearance.BorderSize = 0;
            monthBtn.Click += (s, e) =>
            {
                currentView = "Month";
                HighlightViewButtons(monthBtn, weekBtn, dayBtn);
                RefreshCalendar();
            };

            weekBtn = new RoundedButton1
            {
                Name = "weekBtn",
                Text = "Week",
                Size = new Size(80, 40),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(55, 65, 81),
                Font = new Font("Segoe UI", 10),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            weekBtn.FlatAppearance.BorderSize = 1;
            weekBtn.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);
            weekBtn.Click += (s, e) =>
            {
                currentView = "Week";
                HighlightViewButtons(monthBtn, weekBtn, dayBtn);
                RefreshCalendar();
            };

            dayBtn = new RoundedButton1
            {
                Name = "dayBtn",
                Text = "Day",
                Size = new Size(80, 40),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(55, 65, 81),
                Font = new Font("Segoe UI", 10),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            dayBtn.FlatAppearance.BorderSize = 1;
            dayBtn.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);
            dayBtn.Click += (s, e) =>
            {
                currentView = "Day";
                HighlightViewButtons(monthBtn, weekBtn, dayBtn);
                RefreshCalendar();
            };

            // Nút Filters
            RoundedButton1 filtersBtn = new RoundedButton1
            {
                Name = "filtersBtn",
                Text = "⚙  Filters",
                Size = new Size(100, 40),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(55, 65, 81),
                Font = new Font("Segoe UI", 10),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            filtersBtn.FlatAppearance.BorderSize = 1;
            filtersBtn.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);

            // Update vị trí khi form thay đổi kích thước
            this.Resize += (s, e) =>
            {
                rightX = this.ClientSize.Width - 600;
                optionsBtn.Location = new Point(rightX, 15);
                monthBtn.Location = new Point(rightX + 130, 15);
                weekBtn.Location = new Point(rightX + 220, 15);
                dayBtn.Location = new Point(rightX + 310, 15);
                filtersBtn.Location = new Point(rightX + 400, 15);
            };

            navigationPanel.Controls.Add(todayBtn);
            navigationPanel.Controls.Add(prevBtn);
            navigationPanel.Controls.Add(monthLabel);
            navigationPanel.Controls.Add(nextBtn);
            navigationPanel.Controls.Add(optionsBtn);
            navigationPanel.Controls.Add(monthBtn);
            navigationPanel.Controls.Add(weekBtn);
            navigationPanel.Controls.Add(dayBtn);
            navigationPanel.Controls.Add(filtersBtn);
        }

        // Highlight nút view được chọn
        private void HighlightViewButtons(RoundedButton1 monthBtn, RoundedButton1 weekBtn, RoundedButton1 dayBtn)
        {
            // Reset
            monthBtn.BackColor = Color.White;
            monthBtn.ForeColor = Color.FromArgb(55, 65, 81);

            weekBtn.BackColor = Color.White;
            weekBtn.ForeColor = Color.FromArgb(55, 65, 81);

            dayBtn.BackColor = Color.White;
            dayBtn.ForeColor = Color.FromArgb(55, 65, 81);

            // Áp dụng màu theo view hiện tại
            if (currentView == "Month")
            {
                monthBtn.BackColor = Color.FromArgb(37, 99, 235);
                monthBtn.ForeColor = Color.White;
            }
            else if (currentView == "Week")
            {
                weekBtn.BackColor = Color.FromArgb(37, 99, 235);
                weekBtn.ForeColor = Color.White;
            }
            else if (currentView == "Day")
            {
                dayBtn.BackColor = Color.FromArgb(37, 99, 235);
                dayBtn.ForeColor = Color.White;
            }
        }

        // ======================= MONTH VIEW ==========================
        private void CreateCalendar()
        {
            calendarPanel.Controls.Clear();

            // Header thứ trong tuần
            string[] dayNames = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
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

                // Thêm lịch mẫu
                //if (dayCounter == 3)
                //{
                //    AddAppointment(dayCell, "", Color.FromArgb(254, 243, 199));
                //}
                //else if (dayCounter == 4)
                //{
                //    AddAppointment(dayCell, "", Color.FromArgb(220, 252, 231));
                //}
                //else if (dayCounter == 5)
                //{
                //    AddAppointment(dayCell, "", Color.FromArgb(219, 234, 254));
                //}

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
            contentPanel.Controls.Clear();

            DateTime startOfWeek = currentDate.AddDays(-(int)currentDate.DayOfWeek + 1);
            if (startOfWeek > currentDate) startOfWeek = startOfWeek.AddDays(-7);

            Panel main = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            contentPanel.Controls.Add(main);

            Panel header = new Panel { Height = 60, Dock = DockStyle.Top, BackColor = Color.White };
            main.Controls.Add(header);

            string[] days = { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };

            for (int i = 0; i < 7; i++)
            {
                DateTime day = startOfWeek.AddDays(i);

                Label lbl = new Label
                {
                    Text = $"{days[i]}\n{day:dd}",
                    Width = (main.Width - 60) / 7,
                    Height = 60,
                    Location = new Point(60 + i * ((main.Width - 60) / 7), 0),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 10),
                    ForeColor = (day.Date == DateTime.Today ? Color.FromArgb(37, 99, 235) : Color.Black)
                };
                header.Controls.Add(lbl);
            }

            // GRID
            Panel grid = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            main.Controls.Add(grid);

            int hours = 12;
            int startHour = 8;

            for (int h = 0; h < hours; h++)
            {
                Label time = new Label
                {
                    Text = $"{startHour + h}:00",
                    Width = 60,
                    Height = 50,
                    Location = new Point(0, h * 50),
                    Font = new Font("Segoe UI", 9),
                    ForeColor = Color.Gray,
                    TextAlign = ContentAlignment.MiddleRight
                };
                grid.Controls.Add(time);
            }

            for (int h = 0; h < hours; h++)
            {
                for (int d = 0; d < 7; d++)
                {
                    Panel cell = new Panel
                    {
                        Width = (grid.Width - 60) / 7,
                        Height = 50,
                        Location = new Point(60 + d * ((grid.Width - 60) / 7), h * 50),
                        BorderStyle = BorderStyle.FixedSingle
                    };
                    grid.Controls.Add(cell);
                }
            }
        }

        // ============= DAY VIEW =============
        private void CreateDayView()
        {
            contentPanel.Controls.Clear();

            Panel main = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            contentPanel.Controls.Add(main);

            Label header = new Label
            {
                Text = $"{currentDate:dddd, dd MMMM yyyy}",
                Dock = DockStyle.Top,
                Height = 60,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Padding = new Padding(20, 0, 0, 0)
            };
            main.Controls.Add(header);

            Panel body = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            main.Controls.Add(body);

            int hours = 12;
            int startHour = 8;

            for (int h = 0; h < hours; h++)
            {
                Label time = new Label
                {
                    Text = $"{startHour + h}:00",
                    Width = 60,
                    Height = 60,
                    Location = new Point(0, h * 60),
                    Font = new Font("Segoe UI", 9),
                    ForeColor = Color.Gray,
                    TextAlign = ContentAlignment.MiddleRight
                };
                body.Controls.Add(time);

                Panel cell = new Panel
                {
                    Width = body.Width - 70,
                    Height = 60,
                    Location = new Point(70, h * 60),
                    BorderStyle = BorderStyle.FixedSingle
                };
                body.Controls.Add(cell);
            }
        }

        private Panel CreateDayCell(int day, bool isOtherMonth, int col, int row, int width, int height)
        {
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
            return cell;
        }

        private void AddAppointment(Panel dayCell, string text, Color bgColor)
        {
            Label appointment = new Label
            {
                Text = text,
                Location = new Point(5, 35),
                Size = new Size(dayCell.Width - 10, 25),
                BackColor = bgColor,
                ForeColor = Color.FromArgb(17, 24, 39),
                Font = new Font("Segoe UI", 9),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(5, 5, 0, 0),
                Cursor = Cursors.Hand
            };
            dayCell.Controls.Add(appointment);
        }

        private void RefreshCalendar()
        {
            Label monthLabel = navigationPanel.Controls["monthLabel"] as Label;
            if (monthLabel != null)
                monthLabel.Text = currentDate.ToString("MMMM yyyy");

            if (currentView == "Month")
                CreateCalendar();
            else if (currentView == "Week")
                CreateWeekView();
            else if (currentView == "Day")
                CreateDayView();
        }
    }

    // Hình tròn avatar
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

    // Nút bo góc
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
}

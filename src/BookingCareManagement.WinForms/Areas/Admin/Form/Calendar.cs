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

        // Biến cho drag & drop
        private Label draggedAppointment = null;
        private Point dragStartPoint;
        private Panel sourcePanel = null;
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

            currentDate = DateTime.Now;
            CreateCalendar();
            CreateWeekView();
            CreateDayView();
            RefreshCalendar();

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
            // Thêm khả năng kéo thả cho title
            Point titleDragStart = Point.Empty;
            title.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    titleDragStart = e.Location;
                }
            };

            title.MouseMove += (s, e) =>
            {
                if (e.Button == MouseButtons.Left && titleDragStart != Point.Empty)
                {
                    title.Location = new Point(
                        title.Location.X + e.X - titleDragStart.X,
                        title.Location.Y + e.Y - titleDragStart.Y
                    );
                }
            };

            title.MouseUp += (s, e) =>
            {
                titleDragStart = Point.Empty;
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
            // Biến lưu vị trí bắt đầu kéo
            Point btnDragStart = Point.Empty;

            newAppointmentBtn.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    btnDragStart = e.Location;
                }
            };

            newAppointmentBtn.MouseMove += (s, e) =>
            {
                if (e.Button == MouseButtons.Left && btnDragStart != Point.Empty)
                {
                    newAppointmentBtn.Location = new Point(
                        newAppointmentBtn.Location.X + e.X - btnDragStart.X,
                        newAppointmentBtn.Location.Y + e.Y - btnDragStart.Y
                    );
                }
            };

            newAppointmentBtn.MouseUp += (s, e) =>
            {
                btnDragStart = Point.Empty;
            };

            newAppointmentBtn.Click += (s, e) =>
            {
                // Chỉ mở dialog nếu không đang kéo
                if (btnDragStart == Point.Empty)
                {
                    AppointmentDialog dialog = new AppointmentDialog();
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        RefreshCalendar();
                    }
                }
            };

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
            // Tạo container panel cho user info để có thể kéo thả
            Panel userContainer = new Panel
            {
                Location = new Point(30, 10),
                Size = new Size(100, 100),
                BackColor = Color.Transparent,
                Cursor = Cursors.SizeAll
            };

            CircularPictureBox avatar = new CircularPictureBox
            {
                Location = new Point(0, 0),
                Size = new Size(60, 60),
                BackColor = Color.FromArgb(219, 234, 254),
                SizeMode = PictureBoxSizeMode.Zoom
            };

            Label avatarInitial = new Label
            {
                Text = "👤",
                Location = new Point(0, 0),
                Size = new Size(60, 60),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 24),
                BackColor = Color.Transparent
            };

            Label userName = new Label
            {
                Text = "Jane\nDoe",
                Location = new Point(10, 65),
                Size = new Size(40, 35),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39),
                TextAlign = ContentAlignment.TopCenter
            };

            userContainer.Controls.Add(avatar);
            userContainer.Controls.Add(avatarInitial);
            userContainer.Controls.Add(userName);
            avatarInitial.BringToFront();

            // Thêm khả năng kéo thả cho user container
            Point dragStart = Point.Empty;
            userContainer.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    dragStart = e.Location;
                    userContainer.BringToFront();
                }
            };

            userContainer.MouseMove += (s, e) =>
            {
                if (e.Button == MouseButtons.Left && dragStart != Point.Empty)
                {
                    userContainer.Location = new Point(
                        userContainer.Location.X + e.X - dragStart.X,
                        userContainer.Location.Y + e.Y - dragStart.Y
                    );
                }
            };

            userContainer.MouseUp += (s, e) =>
            {
                dragStart = Point.Empty;
            };

            userPanel.Controls.Add(userContainer);
        }

        private void CreateNavigationPanel()
        {
            // Tạo helper method để thêm drag functionality
            Action<Control> MakeButtonDraggable = (control) =>
            {
                Point dragStart = Point.Empty;

                control.MouseDown += (s, e) =>
                {
                    if (e.Button == MouseButtons.Left)
                        dragStart = e.Location;
                };

                control.MouseMove += (s, e) =>
                {
                    if (e.Button == MouseButtons.Left && dragStart != Point.Empty)
                    {
                        control.Location = new Point(
                            control.Location.X + e.X - dragStart.X,
                            control.Location.Y + e.Y - dragStart.Y
                        );
                    }
                };

                control.MouseUp += (s, e) =>
                {
                    dragStart = Point.Empty;
                };
            };
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
                if (currentView == "Month")
                    currentDate = currentDate.AddMonths(-1);
                else if (currentView == "Week")
                    currentDate = currentDate.AddDays(-7);
                else if (currentView == "Day")
                    currentDate = currentDate.AddDays(-1);
                RefreshCalendar();
            };

            // Label tháng/năm
            Label monthLabel = new Label
            {
                Name = "monthLabel",
                Text = currentDate.ToString("dddd MMMM yyyy"),
                Location = new Point(170, 15),
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39),
                TextAlign = ContentAlignment.MiddleLeft
            };

            // Nút tháng sau
            Button nextBtn = new Button
            {
                Text = "▶",
                Location = new Point(800, 15),
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
                if(currentView == "Month")
                    currentDate = currentDate.AddMonths(1);
                else if(currentView == "Week")
                    currentDate = currentDate.AddDays(7);
                else if(currentView == "Day")
                    currentDate = currentDate.AddDays(1);
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

            //Panel main = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            //contentPanel.Controls.Add(main);

            //Panel header = new Panel { Height = 60, Dock = DockStyle.Top, BackColor = Color.White };
            //main.Controls.Add(header);

            string[] days = { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };

            for (int i = 0; i < 7; i++)
            {
                DateTime day = startOfWeek.AddDays(i);

                Label lbl = new Label
                {
                    Text = $"{days[i]}\n{day:dd}",
                    Width = (contentPanel.Width - 60) / 7,
                    Height = 60,
                    Location = new Point(60 + i * ((contentPanel.Width - 60) / 7), 0),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 10),
                    ForeColor = (day.Date == DateTime.Today ? Color.FromArgb(37, 99, 235) : Color.Black)
                };
                contentPanel.Controls.Add(lbl);
            }

            // GRID
            Panel grid = new Panel { Dock = DockStyle.Fill, BackColor = Color.White};
            contentPanel.Controls.Add(grid);

            int hours = 13;
            int startHour = 7;

            for (int h = 1; h < hours; h++)
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
                    TextAlign = ContentAlignment.MiddleRight,
                };
                contentPanel.Controls.Add(time);

                Panel cell = new Panel
                {
                    Width = contentPanel.Width - 70,
                    Height = 60,
                    Location = new Point(70, h * 60),
                    BorderStyle = BorderStyle.FixedSingle
                };
                contentPanel.Controls.Add(cell);
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
            Button NextBtn = navigationPanel.Controls["nextBtn"] as Button;
            if (monthLabel != null)
            {
                if (currentView == "Month")
                {
                    monthLabel.Text = currentDate.ToString(" MMMM, yyyy");
                    monthLabel.Size = new Size(300, 40);
                    if (NextBtn != null)
                        NextBtn.Location = new Point(500, 15);
                    CreateCalendar();
                }
                if (currentView == "Week")
                {
                    monthLabel.Text = currentDate.ToString(" MMMM, yyyy");
                    monthLabel.Size = new Size(350, 40);
                    CreateWeekView();
                }
                if (currentView == "Day")
                {
                    monthLabel.Text = currentDate.ToString("dddd, dd, MMMM, yyyy");
                    monthLabel.Size = new Size(400, 40);
                    CreateDayView();
                }
            }
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
    public partial class AppointmentDialog : Form
    {

        private ComboBox serviceComboBox;       
        private ComboBox employeeComboBox;    
        private DateTimePicker datePicker;      
        private ComboBox timeComboBox;        
        private TextBox customerTextBox;     
        private CheckBox notificationCheckBox;  
        private Button cancelBtn;               
        private Button saveBtn;                 

        // Constructor
        public AppointmentDialog()
        {
            InitializeComponent1();
        }

        // Khởi tạo các component của form
        private void InitializeComponent1()
        {
            // Thiết lập thuộc tính form
            this.Text = "Add Appointment";
            this.Size = new Size(600, 520);
            this.BackColor = Color.White;
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Tiêu đề header
            Label headerLabel = new Label
            {
                Text = "Add appointment",
                Location = new Point(30, 20),
                Size = new Size(500, 30),
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39)
            };

            // Nút đóng (X)
            Button closeBtn = new Button
            {
                Text = "✕",
                Location = new Point(540, 20),
                Size = new Size(30, 30),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(107, 114, 128),
                Font = new Font("Segoe UI", 14),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            closeBtn.FlatAppearance.BorderSize = 0;
            closeBtn.Click += (s, e) => this.Close();

            // Label "Services"
            Label serviceLabel = new Label
            {
                Text = "Services",
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
                FlatStyle = FlatStyle.Flat
            };
            serviceComboBox.Items.AddRange(new string[] { "General Checkup", "Dental Care", "Cardiology", "Pediatrics" });
            serviceComboBox.Text = "Select service";

            // Label "Employees"
            Label employeeLabel = new Label
            {
                Text = "Employees",
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
                FlatStyle = FlatStyle.Flat
            };
            employeeComboBox.Items.AddRange(new string[] { "Dr. John Smith", "Dr. Sarah Johnson", "Dr. Michael Brown" });
            employeeComboBox.Text = "Select employee";

            // Label "Date"
            Label dateLabel = new Label
            {
                Text = "Date",
                Location = new Point(30, 210),
                Size = new Size(100, 20),
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
                Text = "Time",
                Location = new Point(300, 210),
                Size = new Size(100, 20),
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
                FlatStyle = FlatStyle.Flat
            };
            // Thêm các khung giờ từ 8:00 đến 17:30
            for (int hour = 8; hour <= 17; hour++)
            {
                timeComboBox.Items.Add($"{hour:D2}:00");
                timeComboBox.Items.Add($"{hour:D2}:30");
            }
            timeComboBox.Text = "Select time";

            // Label "Customers"
            Label customerLabel = new Label
            {
                Text = "Customers",
                Location = new Point(30, 280),
                Size = new Size(400, 20),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39)
            };

            // Link thêm khách hàng mới
            LinkLabel newCustomerLink = new LinkLabel
            {
                Text = "+ New Customer",
                Location = new Point(400, 280),
                Size = new Size(100, 20),
                Font = new Font("Segoe UI", 9),
                LinkColor = Color.FromArgb(37, 99, 235),
                ActiveLinkColor = Color.FromArgb(37, 99, 235),
                VisitedLinkColor = Color.FromArgb(37, 99, 235)
            };
            newCustomerLink.Click += (s, e) => MessageBox.Show("Open New Customer dialog", "Info");

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
                Text = "Start typing a customer name"
            };
            customerTextBox.ForeColor = Color.Gray;
            // Xử lý sự kiện khi focus vào TextBox
            customerTextBox.Enter += (s, e) =>
            {
                if (customerTextBox.Text == "Start typing a customer name")
                {
                    customerTextBox.Text = "";
                    customerTextBox.ForeColor = Color.Black;
                }
            };
            // Xử lý sự kiện khi focus ra khỏi TextBox
            customerTextBox.Leave += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(customerTextBox.Text))
                {
                    customerTextBox.Text = "Start typing a customer name";
                    customerTextBox.ForeColor = Color.Gray;
                }
            };

            customerPanel.Controls.Add(customerIcon);
            customerPanel.Controls.Add(customerTextBox);

            // Checkbox gửi thông báo cho khách hàng
            notificationCheckBox = new CheckBox
            {
                Text = "Send notification to customer",
                Location = new Point(30, 355),
                Size = new Size(250, 25),
                Font = new Font("Segoe UI", 10),
                Checked = true
            };

            // Link "More Options" - Tùy chọn thêm
            LinkLabel moreOptionsLink = new LinkLabel
            {
                Text = "More Options",
                Location = new Point(80, 395),
                Size = new Size(100, 20),
                Font = new Font("Segoe UI", 10),
                LinkColor = Color.FromArgb(37, 99, 235),
                ActiveLinkColor = Color.FromArgb(37, 99, 235),
                VisitedLinkColor = Color.FromArgb(37, 99, 235)
            };
            moreOptionsLink.Click += (s, e) => MessageBox.Show("Open More Options", "Info");

            // Nút Cancel (Hủy)
            cancelBtn = new RoundedButton1
            {
                Text = "Cancel",
                Location = new Point(360, 430),
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
                Text = "Save",
                Location = new Point(460, 430),
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
            this.Controls.Add(closeBtn);
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
            this.Controls.Add(moreOptionsLink);
            this.Controls.Add(cancelBtn);
            this.Controls.Add(saveBtn);
        }

        // Xử lý sự kiện khi nhấn nút Save
        private void SaveBtn_Click(object sender, EventArgs e)
        {
            // Kiểm tra validation (xác thực dữ liệu nhập)

            // Kiểm tra đã chọn dịch vụ chưa
            if (serviceComboBox.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a service", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (employeeComboBox.SelectedIndex == -1)
            {
                MessageBox.Show("Please select an employee", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (timeComboBox.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a time", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(customerTextBox.Text) || customerTextBox.Text == "Start typing a customer name")
            {
                MessageBox.Show("Please enter a customer name", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Save appointment logic here
            string message = $"Appointment created successfully!\n\n" +
                           $"Service: {serviceComboBox.SelectedItem}\n" +
                           $"Employee: {employeeComboBox.SelectedItem}\n" +
                           $"Date: {datePicker.Value.ToShortDateString()}\n" +
                           $"Time: {timeComboBox.SelectedItem}\n" +
                           $"Customer: {customerTextBox.Text}\n" +
                           $"Notification: {(notificationCheckBox.Checked ? "Yes" : "No")}";

            MessageBox.Show(message, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}

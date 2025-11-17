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
using static BookingCareManagement.WinForms.Customer;

namespace BookingCareManagement.WinForms
{
    public partial class Calendar : Form
    {
        private DateTime currentDate;
        private string currentView = "Month";

        // Biến cho drag & drop
        private Label draggedAppointment = null;
        private Point dragStartPoint;
        private Panel sourcePanel = null;

        public Calendar()
        {
            currentDate = DateTime.Now;
            InitializeComponent(); // Sử dụng Designer-generated code
            InitializeCustomComponents(); // Khởi tạo các component tùy chỉnh
        }

        private void InitializeCustomComponents()
        {
            // Gắn sự kiện cho các nút đã được tạo bởi Designer
            AttachEventHandlers();
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
            return $"{startOfWeek:MMM dd} - {endOfWeek:MMM dd, yyyy}";
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
            calendarPanel.Controls.Clear();

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
                    Width = (calendarPanel.Width - 60) / 7,
                    Height = 60,
                    Location = new Point(60 + i * ((calendarPanel.Width - 60) / 7), 0),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 10),
                    ForeColor = (day.Date == DateTime.Today ? Color.FromArgb(37, 99, 235) : Color.Black)
                };
                calendarPanel.Controls.Add(lbl);
            }

            // GRID
            Panel grid = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            calendarPanel.Controls.Add(grid);

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
            calendarPanel.Controls.Clear();


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
                calendarPanel.Controls.Add(time);

                Panel cell = new Panel
                {
                    Width = calendarPanel.Width - 70,
                    Height = 60,
                    Location = new Point(70, h * 60),
                    BorderStyle = BorderStyle.FixedSingle
                };
                calendarPanel.Controls.Add(cell);
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
                    NextBtn.Location = new Point(400, 15);
                    CreateCalendar();
                }
                if (currentView == "Week")
                {
                    monthLabel.Text = currentDate.ToString(" MMMM, yyyy");
                    monthLabel.Size = new Size(350, 40);
                    NextBtn.Location = new Point(400, 15);
                    CreateWeekView();
                }
                if (currentView == "Day")
                {
                    monthLabel.Text = currentDate.ToString("dddd, dd, MMMM, yyyy");
                    monthLabel.Size = new Size(400, 40);
                    NextBtn.Location = new Point(520, 15);
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
                serviceComboBox.Items.AddRange(new string[] { "General Checkup", "Dental Care", "Cardiology", "Pediatrics" });

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
                employeeComboBox.Items.AddRange(new string[] { "Dr. John Smith", "Dr. Sarah Johnson", "Dr. Michael Brown" });

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
                    // Sử dụng dialog Add Customer
                    AddCustomerForm addCustomerForm = new AddCustomerForm();

                    // Có thể truyền dữ liệu nếu cần
                    // addCustomerForm.SomeProperty = someValue;

                    DialogResult result = addCustomerForm.ShowDialog();

                    // Xử lý kết quả sau khi dialog đóng
                    if (result == DialogResult.OK)
                    {
                        // Lấy dữ liệu từ form nếu cần
                        MessageBox.Show("Khách hàng đã được thêm thành công!", "Thông báo",
                                      MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Refresh dữ liệu hoặc thực hiện hành động khác
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
                    Text = "Start typing a customer name"
                };
                customerTextBox.ForeColor = Color.Gray;
                customerTextBox.Enter += (s, e) =>
                {
                    if (customerTextBox.Text == "Start typing a customer name")
                    {
                        customerTextBox.Text = "";
                        customerTextBox.ForeColor = Color.Black;
                    }
                };
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
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
    public partial class Customer : Form
    {
        private Panel headerPanel;
        private Panel contentPanel;
        private DataGridView customersDataGridView;

        public Customer()
        {
            InitializeComponents();
            LoadSampleData();
        }

        private void InitializeComponents()
        {
            // Cài đặt form
            this.Text = "Customers";
            this.Size = new Size(1400, 800);
            this.BackColor = Color.FromArgb(243, 244, 246);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Panel Header (phần tiêu đề)
            headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.FromArgb(243, 244, 246),
                Padding = new Padding(30, 20, 30, 20)
            };
            CreateHeader();

            // Panel Nội dung
            contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(243, 244, 246),
                Padding = new Padding(30, 10, 30, 1000)
            };
            CreateContent();

            this.Controls.Add(contentPanel);
            this.Controls.Add(headerPanel);
        }

        private void CreateHeader()
        {
            // Tiêu đề
            Label title = new Label
            {
                Text = "Customers (1)",
                Location = new Point(30, 20),
                AutoSize = true,
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39)
            };

            // Nút xuất dữ liệu
            RoundedButton exportBtn = new RoundedButton
            {
                Text = "⬇  Export Data",
                Location = new Point(900, 18),
                Size = new Size(140, 44),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(55, 65, 81),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            exportBtn.FlatAppearance.BorderSize = 1;
            exportBtn.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);
            exportBtn.Click += (s, e) => MessageBox.Show("Export Data", "Info");

            // Nút nhập dữ liệu
            RoundedButton importBtn = new RoundedButton
            {
                Text = "📄  Import Data",
                Location = new Point(1050, 18),
                Size = new Size(140, 44),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(55, 65, 81),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            importBtn.FlatAppearance.BorderSize = 1;
            importBtn.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);
            importBtn.Click += (s, e) => MessageBox.Show("Import Data", "Info");

            // Nút thêm khách hàng
            RoundedButton addBtn = new RoundedButton
            {
                Text = "+  Add Customer",
                Location = new Point(1200, 18),
                Size = new Size(160, 44),
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            addBtn.FlatAppearance.BorderSize = 0;
            addBtn.Click += (s, e) => MessageBox.Show("Add Customer", "Info");

            // Xử lý khi resize form
            this.Resize += (s, e) =>
            {
                int formWidth = this.ClientSize.Width;
                addBtn.Location = new Point(formWidth - 190, 18);
                importBtn.Location = new Point(formWidth - 340, 18);
                exportBtn.Location = new Point(formWidth - 490, 18);
            };

            headerPanel.Controls.Add(title);
            headerPanel.Controls.Add(exportBtn);
            headerPanel.Controls.Add(importBtn);
            headerPanel.Controls.Add(addBtn);
        }

        private void CreateContent()
        {
            // Panel trắng chính (dock fill để các control con dock đúng)
            // Có padding hai bên để DataGridView không chiếm 100% chiều rộng
            Panel whitePanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(243, 244, 246),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                Padding = new Padding(30, 10, 30, 50)
            };

            // Panel tìm kiếm (dock top)
            Panel searchPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.White
            };

            // Icon và textbox tìm kiếm
            Label searchIcon = new Label
            {
                Text = "🔍 ",
                Location = new Point(20, 25),
                Size = new Size(20, 20),
                Font = new Font("Segoe UI", 12)
            };

            TextBox searchBox = new TextBox
            {
                Location = new Point(55, 20),
                Size = new Size(300, 30),
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.None,
                Text = "Search"
            };
            searchBox.ForeColor = Color.Gray;
            searchBox.GotFocus += (s, e) =>
            {
                // Khi click vào thì xóa placeholder
                if (searchBox.Text == "Search")
                {
                    searchBox.Text = "";
                    searchBox.ForeColor = Color.Black;
                }
            };
            searchBox.LostFocus += (s, e) =>
            {
                // Khi bỏ focus thì trả lại placeholder
                if (string.IsNullOrWhiteSpace(searchBox.Text))
                {
                    searchBox.Text = "Search";
                    searchBox.ForeColor = Color.Gray;
                }
            };

            // Gạch dưới của ô tìm kiếm
            Panel searchUnderline = new Panel
            {
                Location = new Point(50, 55),
                Size = new Size(340, 1),
                BackColor = Color.FromArgb(229, 231, 235)
            };

            searchPanel.Controls.Add(searchBox);
            searchPanel.Controls.Add(searchUnderline);
            searchPanel.Controls.Add(searchIcon);
            searchIcon.BringToFront(); // đảm bảo icon luôn nằm trên cùng

            // DataGridView khách hàng – fill toàn bộ phần còn lại
            customersDataGridView = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersHeight = 50,
                RowTemplate = { Height = 70 },
                GridColor = Color.FromArgb(243, 244, 246),
                Font = new Font("Segoe UI", 10)
            };

            // Style tiêu đề cột
            customersDataGridView.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.White,
                ForeColor = Color.FromArgb(107, 114, 128),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 0, 0)
            };

            // Style ô dữ liệu
            customersDataGridView.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.White,
                ForeColor = Color.FromArgb(17, 24, 39),
                SelectionBackColor = Color.FromArgb(243, 244, 246),
                SelectionForeColor = Color.FromArgb(17, 24, 39),
                Padding = new Padding(15, 10, 0, 10)
            };

            // Style hàng xen kẽ
            customersDataGridView.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(249, 250, 251)
            };

            // Thêm các cột
            AddCheckBoxColumn();
            AddCustomerColumn();
            AddTextColumn("# of Appointments", "Appointments");
            AddTextColumn("Last Appointment", "LastAppointment");
            AddTextColumn("Created", "Created");

            // Đảm bảo chiều rộng tối thiểu
            foreach (DataGridViewColumn col in customersDataGridView.Columns)
            {
                col.MinimumWidth = 50;
            }

            // Thêm control vào whitePanel
            whitePanel.Controls.Add(customersDataGridView);
            whitePanel.Controls.Add(searchPanel);
            contentPanel.Controls.Add(whitePanel);
        }

        private void AddCheckBoxColumn()
        {
            DataGridViewCheckBoxColumn checkCol = new DataGridViewCheckBoxColumn
            {
                Name = "Select",
                HeaderText = "",
                FillWeight = 5 // tỉ lệ nhỏ
            };
            customersDataGridView.Columns.Add(checkCol);
        }

        private void AddCustomerColumn()
        {
            DataGridViewTextBoxColumn customerCol = new DataGridViewTextBoxColumn
            {
                Name = "Customer",
                HeaderText = "Customer",
                FillWeight = 30
            };
            customersDataGridView.Columns.Add(customerCol);
        }

        private void AddTextColumn(string headerText, string name)
        {
            DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn
            {
                Name = name,
                HeaderText = headerText,
                FillWeight = 15
            };
            customersDataGridView.Columns.Add(col);
        }

        private void AddActionColumn()
        {
            DataGridViewButtonColumn actionCol = new DataGridViewButtonColumn
            {
                Name = "Actions",
                HeaderText = "",
                Text = "⋮",
                UseColumnTextForButtonValue = true,
                FillWeight = 8
            };
            customersDataGridView.Columns.Add(actionCol);
        }

        private void LoadSampleData()
        {
            // (Hàm này hiện tại bỏ trống)
        }

        // Vẽ tùy chỉnh cho cell chứa thông tin customer
        private void customersDataGridView_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.ColumnIndex == 1 && e.RowIndex >= 0) // cột Customer
            {
                e.PaintBackground(e.CellBounds, true);

                // Vẽ avatar hình tròn
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(147, 197, 253)))
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    e.Graphics.FillEllipse(brush, e.CellBounds.X + 15, e.CellBounds.Y + 10, 50, 50);
                }

                // Vẽ chữ viết tắt
                using (Font font = new Font("Segoe UI", 12, FontStyle.Bold))
                using (SolidBrush textBrush = new SolidBrush(Color.White))
                {
                    StringFormat sf = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };
                    Rectangle avatarRect = new Rectangle(e.CellBounds.X + 15, e.CellBounds.Y + 10, 50, 50);
                    e.Graphics.DrawString("JD", font, textBrush, avatarRect, sf);
                }

                // Vẽ tên + email
                string[] lines = e.Value?.ToString().Split('\n') ?? new string[] { "", "" };
                using (Font nameFont = new Font("Segoe UI", 11, FontStyle.Bold))
                using (Font emailFont = new Font("Segoe UI", 9))
                using (SolidBrush nameBrush = new SolidBrush(Color.FromArgb(37, 99, 235)))
                using (SolidBrush emailBrush = new SolidBrush(Color.FromArgb(107, 114, 128)))
                {
                    e.Graphics.DrawString(lines[0], nameFont, nameBrush, e.CellBounds.X + 75, e.CellBounds.Y + 18);
                    e.Graphics.DrawString(lines.Length > 1 ? lines[1] : "", emailFont, emailBrush, e.CellBounds.X + 75, e.CellBounds.Y + 40);
                }

                e.Handled = true;
            }
        }

        public Customer(bool attachEvents) : this()
        {
            // Gắn event nếu có yêu cầu
            if (attachEvents)
            {
                customersDataGridView.CellPainting += customersDataGridView_CellPainting;
            }
        }
    }

    // Nút bo góc
    public class RoundedButton : Button
    {
        protected override void OnPaint(PaintEventArgs e)
        {
            GraphicsPath path = new GraphicsPath();
            int radius = 8;
            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);

            // Vẽ bo góc
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

            // Vẽ viền nếu có
            if (this.FlatAppearance.BorderSize > 0)
            {
                using (Pen pen = new Pen(this.FlatAppearance.BorderColor, this.FlatAppearance.BorderSize))
                {
                    e.Graphics.DrawPath(pen, path);
                }
            }

            // Vẽ text
            TextRenderer.DrawText(e.Graphics, this.Text, this.Font,
                this.ClientRectangle, this.ForeColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
    }
}

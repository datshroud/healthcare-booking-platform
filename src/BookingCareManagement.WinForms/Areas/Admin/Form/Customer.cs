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
        public Customer()
        {
            InitializeComponent();
            LoadSampleData();
            AttachEvents();
            AddCheckBoxColumn();
            AddCustomerColumn();
            AddTextColumn("# Số Cuộc hẹn", "Appointments");
            AddTextColumn("# Cuộc hẹn cuối cùng", "LastAppointment");
            AddTextColumn("# Ngày tạo tài khoản", "Created");
        }

        private void LoadSampleData()
        {

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
                HeaderText = "Khách hàng",
                FillWeight = 30,
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

        private void AttachEvents()
        {
            // Sự kiện cho nút
            exportBtn.Click += (s, e) => MessageBox.Show("Export Data", "Info");
            importBtn.Click += (s, e) => MessageBox.Show("Import Data", "Info");
            

            // Sự kiện cho search box
            searchBox.GotFocus += (s, e) =>
            {
                if (searchBox.Text == "Tìm kiếm")
                {
                    searchBox.Text = "";
                    searchBox.ForeColor = Color.Black;
                }
            };

            searchBox.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(searchBox.Text))
                {
                    searchBox.Text = "Search";
                    searchBox.ForeColor = Color.Gray;
                }
            };

            // Sự kiện resize form
            this.Resize += (s, e) =>
            {
                int formWidth = this.ClientSize.Width;
                addBtn.Location = new Point(formWidth - 190, 18);
                importBtn.Location = new Point(formWidth - 340, 18);
                exportBtn.Location = new Point(formWidth - 490, 18);
            };

            // Sự kiện vẽ cell
            customersDataGridView.CellPainting += customersDataGridView_CellPainting;
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

                    // Lấy tên từ dữ liệu để tạo chữ viết tắt
                    string fullName = e.Value?.ToString().Split('\n')[0] ?? "JD";
                    string initials = GetInitials(fullName);

                    e.Graphics.DrawString(initials, font, textBrush, avatarRect, sf);
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

        private string GetInitials(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return "JD";

            string[] names = fullName.Split(' ');
            if (names.Length >= 2)
                return $"{names[0][0]}{names[1][0]}".ToUpper();
            else if (names.Length == 1 && names[0].Length >= 2)
                return names[0].Substring(0, 2).ToUpper();
            else
                return "JD";
        }

        private void importBtn_Click(object sender, EventArgs e)
        {

        }

        private void addBtn_Click(object sender, EventArgs e)
        {
            AddCustomerForm addCustomerForm = new AddCustomerForm();
            addCustomerForm.ShowDialog();
        }
    
        public class AddCustomerForm : Form
        {
            private Label lblTitle;
            private Button btnClose;
            private Label lblFirstName;
            private TextBox txtFirstName;
            private Label lblLastName;
            private TextBox txtLastName;
            private Label lblEmail;
            private TextBox txtEmail;
            private Label lblPhone;
            private ComboBox cboCountryCode;
            private TextBox txtPhone;
            private CheckBox chkSendEmail;
            private Label lblEmailOption;
            private Button btnCancel;
            private Button btnAddCustomer;

            public AddCustomerForm()
            {
                InitializeComponents();
            }

            private void InitializeComponents()
            {
                // Form settings
                this.Text = "Add Customer";
                this.Size = new Size(560, 580);
                this.StartPosition = FormStartPosition.CenterScreen;
                this.FormBorderStyle = FormBorderStyle.FixedDialog;
                this.MaximizeBox = false;
                this.MinimizeBox = false;
                this.BackColor = Color.White;

                // Title Label
                lblTitle = new Label
                {
                    Text = "Thêm khách hàng",
                    Location = new Point(20, 20),
                    Size = new Size(250, 30),
                    Font = new Font("Segoe UI", 14, FontStyle.Bold),
                    ForeColor = Color.FromArgb(31, 41, 55)
                };
                this.Controls.Add(lblTitle);


                // First Name
                lblFirstName = new Label
                {
                    Text = "Họ *",
                    Location = new Point(25, 70),
                    Size = new Size(100, 20),
                    Font = new Font("Segoe UI", 9, FontStyle.Regular),
                    ForeColor = Color.FromArgb(31, 41, 55)
                };
                this.Controls.Add(lblFirstName);

                txtFirstName = new TextBox
                {
                    Location = new Point(25, 95),
                    Size = new Size(490, 30),
                    Font = new Font("Segoe UI", 10),
                    BorderStyle = BorderStyle.FixedSingle
                };
                txtFirstName.Text = "Nhập họ";
                txtFirstName.ForeColor = Color.LightGray;
                txtFirstName.Enter += RemovePlaceholder;
                txtFirstName.Leave += SetPlaceholder;
                this.Controls.Add(txtFirstName);

                // Last Name
                lblLastName = new Label
                {
                    Text = "Tên *",
                    Location = new Point(25, 140),
                    Size = new Size(100, 20),
                    Font = new Font("Segoe UI", 9, FontStyle.Regular),
                    ForeColor = Color.FromArgb(31, 41, 55)
                };
                this.Controls.Add(lblLastName);

                txtLastName = new TextBox
                {
                    Location = new Point(25, 165),
                    Size = new Size(490, 30),
                    Font = new Font("Segoe UI", 10),
                    BorderStyle = BorderStyle.FixedSingle
                };
                txtLastName.Text = "Nhập tên";
                txtLastName.ForeColor = Color.LightGray;
                txtLastName.Enter += RemovePlaceholder;
                txtLastName.Leave += SetPlaceholder;
                this.Controls.Add(txtLastName);

                // Email
                lblEmail = new Label
                {
                    Text = "Email *",
                    Location = new Point(25, 210),
                    Size = new Size(100, 20),
                    Font = new Font("Segoe UI", 9, FontStyle.Regular),
                    ForeColor = Color.FromArgb(31, 41, 55)
                };
                this.Controls.Add(lblEmail);

                txtEmail = new TextBox
                {
                    Location = new Point(25, 235),
                    Size = new Size(490, 30),
                    Font = new Font("Segoe UI", 10),
                    BorderStyle = BorderStyle.FixedSingle
                };
                txtEmail.Text = "example@yourcompany.com";
                txtEmail.ForeColor = Color.LightGray;
                txtEmail.Enter += RemovePlaceholder;
                txtEmail.Leave += SetPlaceholder;
                this.Controls.Add(txtEmail);

                // Phone
                lblPhone = new Label
                {
                    Text = "Điện thoại",
                    Location = new Point(25, 280),
                    Size = new Size(100, 20),
                    Font = new Font("Segoe UI", 9, FontStyle.Regular),
                    ForeColor = Color.FromArgb(31, 41, 55)
                };
                this.Controls.Add(lblPhone);

                // Country Code ComboBox
                cboCountryCode = new ComboBox
                {
                    Location = new Point(25, 305),
                    Size = new Size(100, 30),
                    Font = new Font("Segoe UI", 10),
                    DropDownStyle = ComboBoxStyle.DropDownList
                };
                cboCountryCode.Items.AddRange(new object[] { "+1 🇺🇸", "+44 🇬🇧", "+84 🇻🇳", "+86 🇨🇳", "+91 🇮🇳" });
                cboCountryCode.SelectedIndex = 0;
                this.Controls.Add(cboCountryCode);

                // Phone TextBox
                txtPhone = new TextBox
                {
                    Location = new Point(135, 305),
                    Size = new Size(380, 30),
                    Font = new Font("Segoe UI", 10),
                    BorderStyle = BorderStyle.FixedSingle
                };
                txtPhone.Text = "Số điện thoại";
                txtPhone.ForeColor = Color.LightGray;
                txtPhone.Enter += RemovePlaceholder;
                txtPhone.Leave += SetPlaceholder;
                this.Controls.Add(txtPhone);

                // Checkbox
                chkSendEmail = new CheckBox
                {
                    Location = new Point(25, 355),
                    Size = new Size(20, 20),
                    BackColor = Color.White
                };
                this.Controls.Add(chkSendEmail);

                lblEmailOption = new Label
                {
                    Text = "Gửi email có thông tin đăng nhập của khách hàng\nTùy chọn này yêu cầu địa chỉ email.",
                    Location = new Point(50, 353),
                    Size = new Size(450, 40),
                    Font = new Font("Segoe UI", 9),
                    ForeColor = Color.FromArgb(75, 85, 99)
                };
                this.Controls.Add(lblEmailOption);

                // Cancel Button
                btnCancel = new Button
                {
                    Text = "Hủy",
                    Location = new Point(310, 480),
                    Size = new Size(100, 40),
                    Font = new Font("Segoe UI", 10),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.White,
                    ForeColor = Color.FromArgb(55, 65, 81),
                    Cursor = Cursors.Hand
                };
                btnCancel.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);
                btnCancel.Click += BtnCancel_Click;
                this.Controls.Add(btnCancel);

                // Add Customer Button
                btnAddCustomer = new Button
                {
                    Text = "Thêm",
                    Location = new Point(415, 480),
                    Size = new Size(120, 40),
                    Font = new Font("Segoe UI", 10),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(37, 99, 235),
                    ForeColor = Color.White,
                    Cursor = Cursors.Hand
                };
                btnAddCustomer.FlatAppearance.BorderSize = 0;
                btnAddCustomer.Click += BtnAddCustomer_Click;
                this.Controls.Add(btnAddCustomer);
            }

            private void RemovePlaceholder(object sender, EventArgs e)
            {
                TextBox txt = sender as TextBox;
                if (txt.ForeColor == Color.LightGray)
                {
                    txt.Text = "";
                    txt.ForeColor = Color.Black;
                }
            }

            private void SetPlaceholder(object sender, EventArgs e)
            {
                TextBox txt = sender as TextBox;
                if (string.IsNullOrWhiteSpace(txt.Text))
                {
                    txt.ForeColor = Color.LightGray;
                    if (txt == txtFirstName)
                        txt.Text = "Nhập họ";
                    else if (txt == txtLastName)
                        txt.Text = "Nhập tên";
                    else if (txt == txtEmail)
                        txt.Text = "example@yourcompany.com";
                    else if (txt == txtPhone)
                        txt.Text = "Số điện thoại";
                }
            }

            private void BtnClose_Click(object sender, EventArgs e)
            {
                this.Close();
            }

            private void BtnCancel_Click(object sender, EventArgs e)
            {
                this.Close();
            }

            private void BtnAddCustomer_Click(object sender, EventArgs e)
            {
                // Validate inputs
                if (string.IsNullOrWhiteSpace(txtFirstName.Text) || txtFirstName.ForeColor == Color.LightGray)
                {
                    MessageBox.Show("Please enter first name!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtLastName.Text) || txtLastName.ForeColor == Color.LightGray)
                {
                    MessageBox.Show("Please enter last name!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtEmail.Text) || txtEmail.ForeColor == Color.LightGray)
                {
                    MessageBox.Show("Please enter email!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Save customer data
                string message = $"Customer Added Successfully!\n\n" +
                               $"First Name: {txtFirstName.Text}\n" +
                               $"Last Name: {txtLastName.Text}\n" +
                               $"Email: {txtEmail.Text}\n" +
                               $"Phone: {cboCountryCode.Text} {txtPhone.Text}\n" +
                               $"Send Email: {(chkSendEmail.Checked ? "Yes" : "No")}";

                MessageBox.Show(message, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
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
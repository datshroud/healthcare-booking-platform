using BookingCareManagement.WinForms.Areas.Admin.Services;
using BookingCareManagement.WinForms.Shared.Models.Dtos;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BookingCareManagement.WinForms
{
    public partial class Customer : Form
    {
        private ContextMenuStrip actionMenu;
        private int selectedRowIndex = -1;
        private List<CustomerDto> originalCustomers;
        private CustomerService _customerService;

        // Parameterless ctor fallback for places that still use 'new Customer()'
        public Customer() : this(new CustomerService(new SimpleHttpClientFactory())) { }

        // Accept CustomerService via DI
        public Customer(CustomerService customerService)
        {
            InitializeComponent();
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            InitializeCustomComponents();
            SetupDataGridView();
            _ = LoadCustomersAsync();
            AttachEvents();
            originalCustomers = new List<CustomerDto>();
        }

        #region Initialization Methods
        private void InitializeServices()
        {
        }

        private void InitializeCustomComponents()
        {
            // Tạo Context Menu
            actionMenu = new ContextMenuStrip();
            actionMenu.Items.Add("Chỉnh sửa", null, EditMenuItem_Click);
            actionMenu.Items.Add("Xóa", null, DeleteMenuItem_Click);
            actionMenu.Padding = new Padding(0);
        }

        private void SetupDataGridView()
        {
            AddCheckBoxColumn();
            AddCustomerColumn();
            AddTextColumn("# Số Cuộc hẹn", "Appointments");
            AddTextColumn("# Cuộc hẹn cuối cùng", "LastAppointment");
            AddTextColumn("# Ngày tạo tài khoản", "Created");
            AddActionColumn();
            // Riêng cột checkbox vẫn cho phép click
            if (customersDataGridView.Columns["Select"] != null)
            {
                customersDataGridView.Columns["Select"].ReadOnly = false;
            }

            // Riêng cột Actions vẫn cho phép click
            if (customersDataGridView.Columns["Actions"] != null)
            {
                customersDataGridView.Columns["Actions"].ReadOnly = false;
            }
        }
        #endregion

        #region Data GridView Setup
        private void AddCheckBoxColumn()
        {
            DataGridViewCheckBoxColumn checkCol = new DataGridViewCheckBoxColumn
            {
                Name = "Select",
                HeaderText = "",
                FillWeight =5
            };
            customersDataGridView.Columns.Add(checkCol);
        }

        private void AddCustomerColumn()
        {
            DataGridViewTextBoxColumn customerCol = new DataGridViewTextBoxColumn
            {
                Name = "Customer",
                HeaderText = "Khách hàng",
                FillWeight =30,
            };
            customersDataGridView.Columns.Add(customerCol);
        }

        private void AddTextColumn(string headerText, string name)
        {
            DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn
            {
                Name = name,
                HeaderText = headerText,
                FillWeight =15
            };
            customersDataGridView.Columns.Add(col);
        }

        private void AddActionColumn()
        {
            string horizontalEllipsis = "⋯";

            DataGridViewButtonColumn actionCol = new DataGridViewButtonColumn
            {
                Name = "Actions",
                HeaderText = "",
                UseColumnTextForButtonValue = true,
                Text = horizontalEllipsis,
                FillWeight =8,
            };
            customersDataGridView.Columns.Add(actionCol);
        }
        #endregion

        #region Data Loading Methods
        private async Task LoadCustomersAsync()
        {
            try
            {
                SetLoadingState(true);
                // Sửa: Sử dụng GetAllAsync() thay vì GetCustomersAsync()
                var customers = await _customerService.GetAllAsync();
                originalCustomers = customers.ToList();
                BindDataToGridView();
                UpdateCustomerCount();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        private void BindDataToGridView()
        {
            customersDataGridView.Rows.Clear();
            foreach (var customer in originalCustomers)
            {
                customersDataGridView.Rows.Add(
                    false, // Checkbox
                    $"{customer.FullName}\n{customer.Email}",
                    customer.AppointmentCount.ToString(),
                    customer.LastAppointment?.ToString("dd/MM/yyyy HH:mm") ?? "Chưa có",
                    customer.CreatedAt.ToString("dd/MM/yyyy")
                );
            }
        }

        private async Task SearchCustomersAsync(string searchText)
        {
            try
            {
                SetLoadingState(true);
                // Backend may not provide a search endpoint; fetch all and filter locally
                var customers = await _customerService.GetAllAsync();
                var filtered = customers.Where(c =>
 (!string.IsNullOrWhiteSpace(c.FullName) && c.FullName.Contains(searchText, StringComparison.OrdinalIgnoreCase)) ||
 (!string.IsNullOrWhiteSpace(c.Email) && c.Email.Contains(searchText, StringComparison.OrdinalIgnoreCase)) ||
 (c.PhoneNumber != null && c.PhoneNumber.Contains(searchText, StringComparison.OrdinalIgnoreCase))
 ).ToList();

 DisplaySearchResults(filtered);
 }
 catch (Exception ex)
 {
 MessageBox.Show($"Lỗi khi tìm kiếm: {ex.Message}", "Lỗi",
 MessageBoxButtons.OK, MessageBoxIcon.Error);
 }
 finally
 {
 SetLoadingState(false);
 }
        }

        private void DisplaySearchResults(List<CustomerDto> searchResults)
        {
            customersDataGridView.Rows.Clear();
            foreach (var customer in searchResults)
            {
                customersDataGridView.Rows.Add(
                    false,
                    $"{customer.FullName}\n{customer.Email}",
                    customer.AppointmentCount.ToString(),
                    customer.LastAppointment?.ToString("dd/MM/yyyy HH:mm") ?? "Chưa có",
                    customer.CreatedAt.ToString("dd/MM/yyyy")
                );
            }
            UpdateCustomerCountWithFilter(searchResults.Count);
        }

        private void SetLoadingState(bool isLoading)
        {
            customersDataGridView.Visible = !isLoading;
            Cursor = isLoading ? Cursors.WaitCursor : Cursors.Default;
        }

        private void UpdateCustomerCount()
        {
            int count = originalCustomers?.Count ??0;
            title.Text = $"Khách hàng ({count})";
        }

        private void UpdateCustomerCountWithFilter(int filteredCount)
        {
            title.Text = $"Khách hàng ({filteredCount})";
        }
        #endregion

        #region Event Handlers
        private void AttachEvents()
        {
            AttachButtonEvents();
            AttachSearchBoxEvents();
            AttachFormEvents();
            AttachDataGridViewEvents();
        }

        private void AttachButtonEvents()
        {
            exportBtn.Click += (s, e) => MessageBox.Show("Export Data", "Info");
            importBtn.Click += (s, e) => MessageBox.Show("Import Data", "Info");
            // addBtn.Click += addBtn_Click; // addBtn.Click is wired in the designer; do not attach again to avoid duplicate handling

            var refreshBtn = this.Controls.Find("refreshBtn", true).FirstOrDefault() as Button;
            if (refreshBtn != null)
            {
                refreshBtn.Click += async (s, e) => await LoadCustomersAsync();
            }
        }

        private void AttachSearchBoxEvents()
        {
            searchBox.GotFocus += SearchBox_GotFocus;
            searchBox.LostFocus += SearchBox_LostFocus;
            searchBox.KeyPress += SearchBox_KeyPress;
        }

        private void AttachFormEvents()
        {
            this.Resize += Form_Resize;
        }

        private void AttachDataGridViewEvents()
        {
            customersDataGridView.CellPainting += customersDataGridView_CellPainting;
            customersDataGridView.CellClick += customersDataGridView_CellClick;
        }

        private void SearchBox_GotFocus(object sender, EventArgs e)
        {
            if (searchBox.Text == "Search")
            {
                searchBox.Text = "";
                searchBox.ForeColor = Color.Black;
            }
        }

        private void SearchBox_LostFocus(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(searchBox.Text))
            {
                searchBox.Text = "Search";
                searchBox.ForeColor = Color.Gray;
            }
        }

        private async void SearchBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                string searchText = searchBox.Text.Trim();
                if (searchText == "Search" || string.IsNullOrWhiteSpace(searchText))
                {
                    await LoadCustomersAsync();
                }
                else
                {
                    await SearchCustomersAsync(searchText);
                }
            }
        }

        private void Form_Resize(object sender, EventArgs e)
        {
            int formWidth = this.ClientSize.Width;
            addBtn.Location = new Point(formWidth -190,18);
            importBtn.Location = new Point(formWidth -340,18);
            exportBtn.Location = new Point(formWidth -490,18);
        }

        private void addBtn_Click(object sender, EventArgs e)
        {
            // Use DI service when opening AddCustomerForm
            using var addCustomerForm = new AddCustomerForm(_customerService);
            if (addCustomerForm.ShowDialog() == DialogResult.OK)
            {
                _ = LoadCustomersAsync(); // Refresh data after adding
            }
        }

        private void customersDataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == customersDataGridView.Columns["Actions"].Index && e.RowIndex >=0)
            {
                selectedRowIndex = e.RowIndex;
                Rectangle cellRect = customersDataGridView.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);

                actionMenu.Show(customersDataGridView,
                    customersDataGridView.Location.X + cellRect.Right - actionMenu.Width,
                    customersDataGridView.Location.Y + cellRect.Y);
            }
        }
        #endregion

        #region Context Menu Actions
        private void EditMenuItem_Click(object sender, EventArgs e)
        {
            if (selectedRowIndex >=0 && selectedRowIndex < originalCustomers.Count)
            {
                var customerToEdit = originalCustomers[selectedRowIndex];
                using (EditCustomerForm editForm = new EditCustomerForm(customerToEdit, _customerService))
                {
                    if (editForm.ShowDialog() == DialogResult.OK)
                    {
                        _ = LoadCustomersAsync(); // Refresh data
                        MessageBox.Show($"Đã cập nhật thông tin cho khách hàng: {customerToEdit.FullName}",
                            "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }

        private async void DeleteMenuItem_Click(object sender, EventArgs e)
        {
            if (selectedRowIndex >=0 && selectedRowIndex < originalCustomers.Count)
            {
                var customerToDelete = originalCustomers[selectedRowIndex];
                var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa khách hàng {customerToDelete.FullName}?",
                    "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        // Sửa: Sử dụng DeleteAsync() thay vì DeleteCustomerAsync()
                        await _customerService.DeleteAsync(customerToDelete.Id);
                        await LoadCustomersAsync(); // Refresh data
                        MessageBox.Show("Đã xóa khách hàng thành công!", "Thông báo",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi khi xóa khách hàng: {ex.Message}", "Lỗi",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        #endregion

        #region Custom Painting
        private void customersDataGridView_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.ColumnIndex ==1 && e.RowIndex >=0) // Customer column
            {
                e.PaintBackground(e.CellBounds, true);

                if (e.RowIndex < originalCustomers.Count)
                {
                    var customer = originalCustomers[e.RowIndex];
                    bool isSelected = (customersDataGridView.Rows[e.RowIndex].Selected) || (e.State & DataGridViewElementStates.Selected) !=0;
                    DrawCustomerCell(e, customer, isSelected);
                }

                e.Handled = true;
            }
        }

        private void DrawCustomerCell(DataGridViewCellPaintingEventArgs e, CustomerDto customer, bool isSelected)
        {
            DrawAvatar(e, customer);
            DrawCustomerInfo(e, customer, isSelected);
        }

        private void DrawAvatar(DataGridViewCellPaintingEventArgs e, CustomerDto customer)
        {
            if (!string.IsNullOrEmpty(customer.AvatarUrl))
            {
                // TODO: Load and display avatar from URL
                DrawDefaultAvatar(e, customer.FullName);
            }
            else
            {
                DrawDefaultAvatar(e, customer.FullName);
            }
        }

        private void DrawDefaultAvatar(DataGridViewCellPaintingEventArgs e, string fullName)
        {
            // Draw circle background
            using (var brush = new SolidBrush(Color.FromArgb(147,197,253)))
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.FillEllipse(brush, e.CellBounds.X +15, e.CellBounds.Y +10,50,50);
            }

            // Draw initials
            using (var font = new Font("Segoe UI",12, FontStyle.Bold))
            using (var textBrush = new SolidBrush(Color.White))
            {
                var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                var avatarRect = new Rectangle(e.CellBounds.X +15, e.CellBounds.Y +10,50,50);
                string initials = GetInitials(fullName);
                e.Graphics.DrawString(initials, font, textBrush, avatarRect, sf);
            }
        }

        private void DrawCustomerInfo(DataGridViewCellPaintingEventArgs e, CustomerDto customer, bool isSelected)
        {
            // Name uses accent blue normally, but gray when selected so it remains readable on gray selection background
            var nameColor = isSelected ? Color.FromArgb(107,114,128) : Color.FromArgb(37,99,235);
            var emailColor = isSelected ? Color.FromArgb(156,163,175) : Color.FromArgb(107,114,128);

            using (var nameFont = new Font("Segoe UI",11, FontStyle.Bold))
            using (var emailFont = new Font("Segoe UI",9))
            using (var nameBrush = new SolidBrush(nameColor))
            using (var emailBrush = new SolidBrush(emailColor))
            {
                e.Graphics.DrawString(customer.FullName, nameFont, nameBrush,
                    e.CellBounds.X +75, e.CellBounds.Y +18);
                e.Graphics.DrawString(customer.Email, emailFont, emailBrush,
                    e.CellBounds.X +75, e.CellBounds.Y +40);
            }
        }

        private string GetInitials(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return "JD";

            string[] names = fullName.Split(' ');
            if (names.Length >=2)
                return $"{names[0][0]}{names[1][0]}".ToUpper();
            else if (names.Length ==1 && names[0].Length >=2)
                return names[0].Substring(0,2).ToUpper();
            else
                return "JD";
        }
        #endregion

        #region Unused Methods
        private void importBtn_Click(object sender, EventArgs e) { }
        #endregion
    }

    public class AddCustomerForm : Form
    {
        private CustomerService _customerService;

        // Controls
        private Label lblTitle;
        private Label lblFirstName, lblLastName, lblEmail, lblPhone, lblEmailOption;
        private TextBox txtFirstName, txtLastName, txtEmail, txtPhone;
        private ComboBox cboCountryCode;
        private CheckBox chkSendEmail;
        private Button btnCancel, btnAddCustomer;

        internal CustomerDto NewCustomer { get; private set; }

        // Default ctor fallback
        public AddCustomerForm() : this(new CustomerService(new SimpleHttpClientFactory())) { }

        // Accept CustomerService via DI
        public AddCustomerForm(CustomerService customerService)
        {
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            InitializeForm();
            InitializeControls();
            SetupEventHandlers();
        }

        #region Initialization
        private void InitializeForm()
        {
            this.Text = "Thêm khách hàng";
            this.Size = new Size(560,580);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;
        }

        private void InitializeControls()
        {
            CreateTitleLabel();
            CreateNameFields();
            CreateEmailField();
            CreatePhoneField();
            CreateCheckbox();
            CreateButtons();
        }

        private void SetupEventHandlers()
        {
            btnCancel.Click += (s, e) => this.Close();
            btnAddCustomer.Click += BtnAddCustomer_Click;
        }
        #endregion

        #region Control Creation Methods
        private void CreateTitleLabel()
        {
            lblTitle = new Label
            {
                Text = "Thêm khách hàng",
                Location = new Point(20,20),
                Size = new Size(250,30),
                Font = new Font("Segoe UI",14, FontStyle.Bold),
                ForeColor = Color.FromArgb(31,41,55)
            };
            this.Controls.Add(lblTitle);
        }

        private void CreateNameFields()
        {
            // First Name
            lblFirstName = CreateLabel("Họ *",25,70);
            txtFirstName = CreateTextBox("Nhập họ",25,95);

            // Last Name
            lblLastName = CreateLabel("Tên *",25,140);
            txtLastName = CreateTextBox("Nhập tên",25,165);

            this.Controls.AddRange(new Control[] { lblFirstName, txtFirstName, lblLastName, txtLastName });
        }

        private void CreateEmailField()
        {
            lblEmail = CreateLabel("Email *",25,210);
            txtEmail = CreateTextBox("example@yourcompany.com",25,235);

            this.Controls.AddRange(new Control[] { lblEmail, txtEmail });
        }

        private void CreatePhoneField()
        {
            // Put country code and phone input into a small panel so label appears above both, matching other fields
            var phoneRow = new Panel
            {
                Location = new Point(25,305),
                Size = new Size(490,34)
            };

            cboCountryCode = new ComboBox
            {
                Location = new Point(0,0),
                Size = new Size(100,30),
                Font = new Font("Segoe UI",10),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboCountryCode.Items.AddRange(new object[] { "+84 🇻🇳" });
            cboCountryCode.SelectedIndex =0;

            txtPhone = new TextBox
            {
                Location = new Point(110,0),
                Size = new Size(370,30),
                Font = new Font("Segoe UI",10),
                BorderStyle = BorderStyle.FixedSingle,
                Text = string.Empty,
                ForeColor = Color.Black
            };

            phoneRow.Controls.Add(cboCountryCode);
            phoneRow.Controls.Add(txtPhone);

            this.Controls.AddRange(new Control[] { lblPhone, phoneRow });
        }

        private void CreateCheckbox()
        {
            chkSendEmail = new CheckBox
            {
                Location = new Point(25,355),
                Size = new Size(20,20),
                BackColor = Color.White
            };

            lblEmailOption = new Label
            {
                Text = "Gửi email có thông tin đăng nhập của khách hàng\nTùy chọn này yêu cầu địa chỉ email.",
                Location = new Point(50,353),
                Size = new Size(450,40),
                Font = new Font("Segoe UI",9),
                ForeColor = Color.FromArgb(75,85,99)
            };

            this.Controls.AddRange(new Control[] { chkSendEmail, lblEmailOption });
        }

        private void CreateButtons()
        {
            btnCancel = CreateButton("Hủy",310,480, Color.White, Color.FromArgb(55,65,81));
            btnAddCustomer = CreateButton("Thêm",415,480, Color.FromArgb(37,99,235), Color.White);

            this.Controls.AddRange(new Control[] { btnCancel, btnAddCustomer });
        }
        #endregion

        #region Helper Methods
        private Label CreateLabel(String text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(100,20),
                Font = new Font("Segoe UI",9),
                ForeColor = Color.FromArgb(31,41,55)
            };
        }

        private TextBox CreateTextBox(String placeholder, int x, int y)
        {
            var textBox = new TextBox
            {
                Location = new Point(x, y),
                Size = new Size(490,30),
                Font = new Font("Segoe UI",10),
                BorderStyle = BorderStyle.FixedSingle
            };

            SetupPlaceholder(textBox, placeholder);
            return textBox;
        }

        private Button CreateButton(String text, int x, int y, Color backColor, Color foreColor)
        {
            var button = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(text == "Thêm" ?120 :100,40),
                Font = new Font("Segoe UI",10),
                FlatStyle = FlatStyle.Flat,
                BackColor = backColor,
                ForeColor = foreColor,
                Cursor = Cursors.Hand
            };

            if (backColor == Color.White)
            {
                button.FlatAppearance.BorderColor = Color.FromArgb(209,213,219);
            }
            else
            {
                button.FlatAppearance.BorderSize =0;
            }

            return button;
        }

        private void SetupPlaceholder(TextBox textBox, string placeholder)
        {
            textBox.Text = placeholder;
            textBox.ForeColor = Color.LightGray;
            textBox.Enter += RemovePlaceholder;
            textBox.Leave += SetPlaceholder;
        }
        #endregion

        #region Event Handlers
        private void RemovePlaceholder(object sender, EventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox.ForeColor == Color.LightGray)
            {
                textBox.Text = "";
                textBox.ForeColor = Color.Black;
            }
        }

        private void SetPlaceholder(object sender, EventArgs e)
        {
            var textBox = sender as TextBox;
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.ForeColor = Color.LightGray;
                textBox.Text = GetPlaceholderText(textBox);
            }
        }

        private string GetPlaceholderText(TextBox textBox)
        {
            return textBox == txtFirstName ? "Nhập họ" :
                   textBox == txtLastName ? "Nhập tên" :
                   textBox == txtEmail ? "example@yourcompany.com" :
                   textBox == txtPhone ? "Số điện thoại" : "";
        }
        #endregion

        #region Event Handlers
        private async void BtnAddCustomer_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs()) return;

            var success = await AddCustomerAsync();
            if (success)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
        #endregion

        #region Business Logic
        private bool ValidateInputs()
        {
            if (IsEmptyField(txtFirstName, "họ")) return false;
            if (IsEmptyField(txtLastName, "tên")) return false;
            if (IsEmptyField(txtEmail, "email")) return false;

            if (!ValidateEmail(txtEmail.Text))
            {
                MessageBox.Show("Email không hợp lệ! Vui lòng nhập đúng định dạng email.",
                    "Lỗi xác thực", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private bool IsEmptyField(TextBox textBox, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(textBox.Text) || textBox.ForeColor == Color.LightGray)
            {
                MessageBox.Show($"Vui lòng nhập {fieldName}!", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return true;
            }
            return false;
        }

        private bool ValidateEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> AddCustomerAsync()
        {
            try
            {
                var request = CreateCustomerRequest();
                // Sửa: Sử dụng CreateAsync() thay vì AddCustomerAsync()
                NewCustomer = await _customerService.CreateAsync(request);

                MessageBox.Show("Đã thêm khách hàng thành công!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi thêm khách hàng: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private CreateCustomerRequest CreateCustomerRequest()
        {
            return new CreateCustomerRequest
            {
                FirstName = txtFirstName.Text.Trim(),
                LastName = txtLastName.Text.Trim(),
                Email = txtEmail.Text.Trim(),
                PhoneNumber = GetPhoneNumber(),
                Gender = null,
                SendWelcomeEmail = chkSendEmail.Checked
            };
        }

        private string GetPhoneNumber()
        {
            if (txtPhone.ForeColor == Color.LightGray || string.IsNullOrWhiteSpace(txtPhone.Text))
                return string.Empty;

            var countryCode = cboCountryCode?.Text?.Split(' ')[0] ?? string.Empty;
            if (string.IsNullOrWhiteSpace(countryCode))
                return txtPhone.Text.Trim();

            return $"{countryCode} {txtPhone.Text.Trim()}";
        }
        #endregion
    }

    public class EditCustomerForm : Form
    {
        private CustomerDto _customer;
        private CustomerService _customerService;

        // Controls
        private Label lblTitle;
        private Label lblFirstName, lblLastName, lblEmail, lblPhone;
        private TextBox txtFirstName, txtLastName, txtEmail, txtPhone;
        private ComboBox cboCountryCode;
        private Button btnCancel, btnSaveCustomer;

        internal EditCustomerForm(CustomerDto customer, CustomerService customerService)
        {
            _customer = customer;
            _customerService = customerService;
            InitializeForm();
            InitializeControls();
            LoadCustomerData();
            SetupEventHandlers();
        }

        #region Initialization
        private void InitializeForm()
        {
            this.Text = "Chỉnh sửa khách hàng";
            this.Size = new Size(560, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;
        }

        private void InitializeControls()
        {
            CreateTitleLabel();
            CreateNameFields();
            CreateEmailField();
            CreatePhoneField();
            CreateButtons();
        }

        private void SetupEventHandlers()
        {
            btnCancel.Click += (s, e) =>
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            };
            btnSaveCustomer.Click += BtnSaveCustomer_Click;
        }
        #endregion

        #region Control Creation Methods
        private void CreateTitleLabel()
        {
            lblTitle = new Label
            {
                Text = "Chỉnh sửa khách hàng",
                Location = new Point(20, 20),
                Size = new Size(250, 30),
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(31, 41, 55)
            };
            this.Controls.Add(lblTitle);
        }

        private void CreateNameFields()
        {
            lblFirstName = CreateLabel("Họ *", 25, 70);
            txtFirstName = CreateTextBox(25, 95);

            lblLastName = CreateLabel("Tên *", 25, 140);
            txtLastName = CreateTextBox(25, 165);

            this.Controls.AddRange(new Control[] { lblFirstName, txtFirstName, lblLastName, txtLastName });
        }

        private void CreateEmailField()
        {
            lblEmail = CreateLabel("Email *", 25, 210);
            txtEmail = CreateTextBox(25, 235);

            this.Controls.AddRange(new Control[] { lblEmail, txtEmail });
        }

        private void CreatePhoneField()
        {
            // Make label match other fields
            lblPhone = CreateLabel("Điện thoại",25,280);

            var phoneRow = new Panel
            {
                Location = new Point(25,305),
                Size = new Size(490,34)
            };

            cboCountryCode = new ComboBox
            {
                Location = new Point(0,0),
                Size = new Size(100,30),
                Font = new Font("Segoe UI",10),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboCountryCode.Items.AddRange(new object[] { "+84 🇻🇳"});
            cboCountryCode.SelectedIndex = 0;

            txtPhone = new TextBox
            {
                Location = new Point(110,0),
                Size = new Size(370,30),
                Font = new Font("Segoe UI",10),
                BorderStyle = BorderStyle.FixedSingle,
                Text = string.Empty,
                ForeColor = Color.Black
            };

            phoneRow.Controls.Add(cboCountryCode);
            phoneRow.Controls.Add(txtPhone);

            this.Controls.AddRange(new Control[] { lblPhone, phoneRow });
        }

        private void CreateButtons()
        {
            btnCancel = CreateButton("Hủy", 310, 380, Color.White, Color.FromArgb(55, 65, 81));
            btnSaveCustomer = CreateButton("Lưu", 415, 380, Color.FromArgb(37, 99, 235), Color.White);

            this.Controls.AddRange(new Control[] { btnCancel, btnSaveCustomer });
        }
        #endregion

        #region Helper Methods
        private Label CreateLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(100, 20),
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(31, 41, 55)
            };
        }
        private Label CreateLabelPhone(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(50, 20),
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(31, 41, 55)
            };
        }

        private TextBox CreateTextBox(int x, int y)
        {
            return new TextBox
            {
                Location = new Point(x, y),
                Size = new Size(490, 30),
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle
            };
        }

        private Button CreateButton(string text, int x, int y, Color backColor, Color foreColor)
        {
            var button = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(text == "Lưu" ? 120 : 100, 40),
                Font = new Font("Segoe UI", 10),
                FlatStyle = FlatStyle.Flat,
                BackColor = backColor,
                ForeColor = foreColor,
                Cursor = Cursors.Hand
            };

            if (backColor == Color.White)
            {
                button.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);
            }
            else
            {
                button.FlatAppearance.BorderSize = 0;
            }

            return button;
        }
        #endregion

        #region Business Logic
        private void LoadCustomerData()
        {
            if (_customer != null)
            {
                // Tách họ và tên từ FullName
                string[] nameParts = _customer.FullName?.Split(' ') ?? new string[] { "", "" };
                txtFirstName.Text = nameParts.Length >0 ? nameParts[0] : "";
                txtLastName.Text = nameParts.Length >1 ? string.Join(" ", nameParts.Skip(1)) : "";

                txtEmail.Text = _customer.Email ?? "";

                // Xử lý số điện thoại
                if (!string.IsNullOrEmpty(_customer.PhoneNumber))
                {
                    string phone = _customer.PhoneNumber.Trim();
                    // Có thể phone được lưu kèm hoặc không kèm country code. Xử lý linh hoạt.
                    string[] phoneParts = phone.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    if (phoneParts.Length >1 && phoneParts[0].StartsWith("+"))
                    {
                        string countryCode = phoneParts[0];
                        string phoneNumber = string.Join(" ", phoneParts.Skip(1));

                        // Tìm country code trong combobox (so sánh startswith để hỗ trợ định dạng như "+84 🇻🇳")
                        for (int i =0; i < cboCountryCode.Items.Count; i++)
                        {
                            var item = cboCountryCode.Items[i].ToString();
                            if (!string.IsNullOrEmpty(item) && item.StartsWith(countryCode))
                            {
                                cboCountryCode.SelectedIndex = i;
                                break;
                            }
                        }

                        txtPhone.Text = phoneNumber;
                    }
                    else
                    {
                        // Không có country code, đặt vào ô số điện thoại và giữ country code mặc định
                        txtPhone.Text = phone;
                    }
                }
            }
        }

        private async void BtnSaveCustomer_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs()) return;

            var success = await UpdateCustomerAsync();
            if (success)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(txtFirstName.Text) ||
                string.IsNullOrWhiteSpace(txtLastName.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ Họ, Tên và Email!", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!ValidateEmail(txtEmail.Text.Trim()))
            {
                MessageBox.Show("Email không hợp lệ! Vui lòng nhập đúng định dạng email.",
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtEmail.Focus();
                txtEmail.SelectAll();
                return false;
            }

            return true;
        }

        private bool ValidateEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> UpdateCustomerAsync()
        {
            try
            {
                var request = CreateUpdateRequest();
                // Sửa: Sử dụng UpdateAsync() thay vì UpdateCustomerAsync()
                await _customerService.UpdateAsync(_customer.Id, request);

                // Cập nhật local object

                _customer.FirstName = txtFirstName.Text.Trim();
                _customer.LastName = txtLastName.Text.Trim();
                _customer.FullName = $"{txtFirstName.Text.Trim()} {_customer.LastName.Trim()}";
                _customer.Email = txtEmail.Text.Trim();
                _customer.PhoneNumber = GetFormattedPhoneNumber();

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật khách hàng: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private UpdateCustomerRequest CreateUpdateRequest()
        {
            return new UpdateCustomerRequest
            {
                Id = _customer.Id,
                FirstName = txtFirstName.Text.Trim(),
                LastName = txtLastName.Text.Trim(),
                Email = txtEmail.Text.Trim(),
                PhoneNumber = GetFormattedPhoneNumber(),
                Gender = _customer.Gender,
                DateOfBirth = _customer.DateOfBirth,
                InternalNote = _customer.InternalNote
            };
        }

        private string GetFormattedPhoneNumber()
        {
            if (string.IsNullOrWhiteSpace(txtPhone.Text))
                return "";

            string countryCode = cboCountryCode.Text.Split(' ')[0];
            return $"{countryCode} {txtPhone.Text.Trim()}";
        }
        #endregion
    }

    public class SimpleHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("https://healthcare-booking-dzhba4dmdjagcdbq.southeastasia-01.azurewebsites.net");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(30);
            return client;
        }
    }

    public class RoundedButton : Button
    {
        protected override void OnPaint(PaintEventArgs e)
        {
            GraphicsPath path = new GraphicsPath();
            int radius = 8;
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

            TextRenderer.DrawText(e.Graphics, this.Text, this.Font,
                this.ClientRectangle, this.ForeColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
    }
}
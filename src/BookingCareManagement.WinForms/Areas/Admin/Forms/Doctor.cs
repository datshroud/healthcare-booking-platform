using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace BookingCareManagement.WinForms.Areas.Admin.Forms
{
    public partial class Doctor : Form
    {
        private List<DoctorInfo> doctors;
        private List<DoctorInfo> filteredDoctors;

        public Doctor()
        {
            InitializeComponent();

            this.Load += Doctor_Load;
            this.textBoxSearch.Enter += TextBoxSearch_Enter;
            this.textBoxSearch.Leave += TextBoxSearch_Leave;
            this.textBoxSearch.TextChanged += TextBoxSearch_TextChanged;
            this.buttonAdd.Click += ButtonAdd_Click;
            this.buttonEdit.Click += ButtonEdit_Click;
            this.buttonDelete.Click += ButtonDelete_Click;
            this.dataGridViewDoctors.CellDoubleClick += DataGridViewDoctors_CellDoubleClick;
            this.dataGridViewDoctors.SelectionChanged += DataGridViewDoctors_SelectionChanged;
        }

        private void Doctor_Load(object sender, EventArgs e)
        {
            InitializeSampleData();
            LoadDoctors();
            buttonEdit.Enabled = false;
            buttonDelete.Enabled = false;
        }

        private void InitializeSampleData()
        {
            // Giả lập dữ liệu lịch làm việc
            doctors = new List<DoctorInfo>
            {
                new DoctorInfo { Id = 1, Name = "Nguyễn Văn A", Email = "nguyenvana@gmail.com", Phone = "0901234567", Specialty = "Nha Khoa", Status = "Đã duyệt", ImagePath = "", Schedule = "Thứ 2: 08:00-17:00; Thứ 4: 08:00-12:00" },
                new DoctorInfo { Id = 2, Name = "Trần Thị B", Email = "tranthib@gmail.com", Phone = "0909876543", Specialty = "Tim Mạch", Status = "Chờ duyệt", ImagePath = "", Schedule = "" },
                new DoctorInfo { Id = 3, Name = "Lê Văn C", Email = "levanc@gmail.com", Phone = "0912345678", Specialty = "Nội Khoa", Status = "Đã duyệt", ImagePath = "", Schedule = "Thứ 3: 13:00-17:00" },
            };
            filteredDoctors = new List<DoctorInfo>(doctors);
        }

        private void LoadDoctors()
        {
            dataGridViewDoctors.Rows.Clear();
            foreach (var doc in filteredDoctors)
            {
                int rowIndex = dataGridViewDoctors.Rows.Add();
                DataGridViewRow row = dataGridViewDoctors.Rows[rowIndex];

                if (!string.IsNullOrEmpty(doc.ImagePath) && System.IO.File.Exists(doc.ImagePath))
                {
                    row.Cells[0].Value = Image.FromFile(doc.ImagePath);
                }
                else
                {
                    Bitmap placeholder = new Bitmap(60, 60);
                    using (Graphics g = Graphics.FromImage(placeholder))
                    {
                        g.Clear(Color.FromArgb(220, 220, 220));
                        using (Font font = new Font("Segoe UI", 20, FontStyle.Bold))
                        {
                            string initial = doc.Name.Length > 0 ? doc.Name.Substring(0, 1) : "?";
                            SizeF size = g.MeasureString(initial, font);
                            g.DrawString(initial, font, Brushes.Gray, (60 - size.Width) / 2, (60 - size.Height) / 2);
                        }
                    }
                    row.Cells[0].Value = placeholder;
                }

                row.Cells[1].Value = doc.Name;
                row.Cells[2].Value = doc.Email;
                row.Cells[3].Value = doc.Phone;
                row.Cells[4].Value = doc.Specialty;
                row.Cells[5].Value = doc.Status;

                if (doc.Status == "Đã duyệt") row.Cells[5].Style.ForeColor = Color.Green;
                else row.Cells[5].Style.ForeColor = Color.OrangeRed;

                row.Tag = doc.Id;
            }
            labelCount.Text = $"({filteredDoctors.Count})";
        }

        // --- Search Logic ---
        private void TextBoxSearch_Enter(object sender, EventArgs e) { if (textBoxSearch.Text == "Tìm kiếm bác sĩ...") { textBoxSearch.Text = ""; textBoxSearch.ForeColor = Color.Black; } }
        private void TextBoxSearch_Leave(object sender, EventArgs e) { if (string.IsNullOrWhiteSpace(textBoxSearch.Text)) { textBoxSearch.Text = "Tìm kiếm bác sĩ..."; textBoxSearch.ForeColor = Color.Gray; } }
        private void TextBoxSearch_TextChanged(object sender, EventArgs e)
        {
            if (textBoxSearch.Text == "Tìm kiếm bác sĩ..." || string.IsNullOrWhiteSpace(textBoxSearch.Text)) filteredDoctors = new List<DoctorInfo>(doctors);
            else { string s = textBoxSearch.Text.ToLower(); filteredDoctors = doctors.Where(d => d.Name.ToLower().Contains(s) || d.Email.ToLower().Contains(s) || d.Phone.Contains(s)).ToList(); }
            LoadDoctors();
        }

        // --- Button Events ---
        private void DataGridViewDoctors_SelectionChanged(object sender, EventArgs e) { bool h = dataGridViewDoctors.SelectedRows.Count > 0; buttonEdit.Enabled = h; buttonDelete.Enabled = h; }

        private void ButtonAdd_Click(object sender, EventArgs e)
        {
            DoctorDialog dialog = new DoctorDialog(null);
            dialog.Text = "Thêm Bác Sĩ Mới";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                DoctorInfo newDoc = new DoctorInfo
                {
                    Id = doctors.Count > 0 ? doctors.Max(d => d.Id) + 1 : 1,
                    Name = dialog.DocName,
                    Email = dialog.Email,
                    Phone = dialog.Phone,
                    Specialty = dialog.Specialty,
                    Status = "Đã duyệt",
                    ImagePath = dialog.ImagePath,
                    Schedule = ""
                };
                doctors.Add(newDoc);
                filteredDoctors = new List<DoctorInfo>(doctors);
                LoadDoctors();
                MessageBox.Show("Thêm thành công!", "Thông báo");
            }
        }

        private void ButtonEdit_Click(object sender, EventArgs e)
        {
            if (dataGridViewDoctors.SelectedRows.Count == 0) return;
            int id = (int)dataGridViewDoctors.SelectedRows[0].Tag;
            var doc = doctors.FirstOrDefault(d => d.Id == id);
            if (doc != null)
            {
                DoctorDialog dialog = new DoctorDialog(doc);
                dialog.Text = "Cập Nhật Thông Tin";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    doc.Name = dialog.DocName; doc.Email = dialog.Email; doc.Phone = dialog.Phone; doc.Specialty = dialog.Specialty;
                    doc.Status = dialog.Status; doc.ImagePath = dialog.ImagePath;
                    doc.Schedule = dialog.ScheduleString; // Lưu lịch làm việc
                    LoadDoctors();
                    MessageBox.Show("Cập nhật thành công!", "Thông báo");
                }
            }
        }

        private void ButtonDelete_Click(object sender, EventArgs e)
        {
            if (dataGridViewDoctors.SelectedRows.Count == 0) return;
            int id = (int)dataGridViewDoctors.SelectedRows[0].Tag;
            var doc = doctors.FirstOrDefault(d => d.Id == id);
            if (doc != null && MessageBox.Show($"Xóa bác sĩ '{doc.Name}'?", "Xác nhận", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                doctors.Remove(doc); filteredDoctors = new List<DoctorInfo>(doctors); LoadDoctors();
            }
        }

        private void DataGridViewDoctors_CellDoubleClick(object sender, DataGridViewCellEventArgs e) { if (e.RowIndex >= 0) ButtonEdit_Click(sender, e); }
    }

    public class DoctorInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Specialty { get; set; }
        public string Status { get; set; }
        public string ImagePath { get; set; }
        public string Schedule { get; set; } // Chuỗi lưu lịch: "Thứ 2: 08:00-17:00; Thứ 3:..."
    }

    // --- DIALOG UPDATE CHO PHÉP CHỌN GIỜ KHÁC NHAU TỪNG THỨ ---
    public class DoctorDialog : Form
    {
        private TextBox txtName, txtEmail, txtPhone, txtImagePath;
        private ComboBox cboSpecialty, cboStatus;
        private Button btnBrowse, btnSave, btnCancel;
        private GroupBox grpSchedule;
        private FlowLayoutPanel flowSchedule; // Panel cuộn chứa các dòng lịch

        public string DocName { get; private set; }
        public string Email { get; private set; }
        public string Phone { get; private set; }
        public string Specialty { get; private set; }
        public string Status { get; private set; }
        public string ImagePath { get; private set; }
        public string ScheduleString { get; private set; } // Chuỗi lịch trả về

        private bool isEditMode;
        private string[] specialties = { "Nha Khoa", "Tim Mạch", "Nội Khoa", "Ngoại Khoa", "Sản Phụ Khoa", "Nhi Khoa" };
        private string[] daysOfWeek = { "Thứ 2", "Thứ 3", "Thứ 4", "Thứ 5", "Thứ 6", "Thứ 7", "Chủ Nhật" };

        // Dictionary để lưu các control từng dòng (Key = Tên thứ)
        private Dictionary<string, (CheckBox chk, DateTimePicker start, DateTimePicker end)> scheduleControls;

        public DoctorDialog(DoctorInfo doc = null)
        {
            isEditMode = (doc != null);
            scheduleControls = new Dictionary<string, (CheckBox, DateTimePicker, DateTimePicker)>();
            InitializeDialog();

            if (isEditMode)
            {
                txtName.Text = doc.Name; txtEmail.Text = doc.Email; txtPhone.Text = doc.Phone;
                cboSpecialty.SelectedItem = doc.Specialty; cboStatus.SelectedItem = doc.Status;
                txtImagePath.Text = doc.ImagePath;

                grpSchedule.Visible = true;
                this.Height = 700; // Form cao hơn để chứa lịch chi tiết
                LoadExistingSchedule(doc.Schedule); // Load lịch cũ lên form
            }
            else
            {
                cboStatus.Visible = false;
                grpSchedule.Visible = false;
                this.Height = 400;
            }
        }

        private void InitializeDialog()
        {
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false; this.MinimizeBox = false;
            this.Width = 600; // Rộng hơn chút

            // --- Các Control cơ bản (giống cũ) ---
            Label lblName = CreateLabel("Họ tên:", 20); txtName = CreateTextBox(20);
            Label lblEmail = CreateLabel("Email:", 60); txtEmail = CreateTextBox(60);
            Label lblPhone = CreateLabel("SĐT:", 100); txtPhone = CreateTextBox(100);
            Label lblSpec = CreateLabel("Chuyên khoa:", 140);
            cboSpecialty = new ComboBox { Left = 150, Top = 140, Width = 320, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };
            cboSpecialty.Items.AddRange(specialties);

            Label lblImg = CreateLabel("Ảnh:", 180);
            txtImagePath = new TextBox { Left = 150, Top = 180, Width = 240, ReadOnly = true, Font = new Font("Segoe UI", 10) };
            btnBrowse = new Button { Text = "...", Left = 400, Top = 178, Width = 70, Height = 27 };
            btnBrowse.Click += (s, e) => { OpenFileDialog d = new OpenFileDialog(); if (d.ShowDialog() == DialogResult.OK) txtImagePath.Text = d.FileName; };

            Label lblStatus = CreateLabel("Trạng thái:", 220);
            cboStatus = new ComboBox { Left = 150, Top = 220, Width = 320, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };
            cboStatus.Items.AddRange(new object[] { "Đã duyệt", "Chờ duyệt", "Ngưng hoạt động" });
            if (!isEditMode) lblStatus.Visible = false;

            // --- GROUP BOX LỊCH CHI TIẾT ---
            grpSchedule = new GroupBox { Text = "Cấu hình Lịch làm việc chi tiết", Left = 20, Top = 260, Width = 540, Height = 330, Font = new Font("Segoe UI", 10, FontStyle.Bold) };

            // Header cho lịch
            Label lblHeaderDay = new Label { Text = "Thứ", Location = new Point(40, 25), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Underline) };
            Label lblHeaderTime = new Label { Text = "Giờ làm việc (Bắt đầu - Kết thúc)", Location = new Point(150, 25), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Underline) };
            grpSchedule.Controls.Add(lblHeaderDay);
            grpSchedule.Controls.Add(lblHeaderTime);

            // FlowLayoutPanel để chứa các dòng
            flowSchedule = new FlowLayoutPanel { Location = new Point(10, 50), Size = new Size(520, 270), AutoScroll = true, FlowDirection = FlowDirection.TopDown, WrapContents = false };
            grpSchedule.Controls.Add(flowSchedule);

            // Tạo dòng cho từng ngày
            foreach (var day in daysOfWeek)
            {
                Panel pnlRow = new Panel { Size = new Size(490, 35) };

                CheckBox chkDay = new CheckBox { Text = day, Location = new Point(5, 8), Width = 100, Font = new Font("Segoe UI", 9) };

                DateTimePicker dtpStart = new DateTimePicker { Location = new Point(130, 5), Width = 90, Format = DateTimePickerFormat.Time, ShowUpDown = true, Enabled = false, Font = new Font("Segoe UI", 9) };
                // Set mặc định 8h
                dtpStart.Value = DateTime.Today.AddHours(8);

                Label lblTo = new Label { Text = "→", Location = new Point(230, 7), AutoSize = true, Font = new Font("Segoe UI", 9) };

                DateTimePicker dtpEnd = new DateTimePicker { Location = new Point(260, 5), Width = 90, Format = DateTimePickerFormat.Time, ShowUpDown = true, Enabled = false, Font = new Font("Segoe UI", 9) };
                // Set mặc định 17h
                dtpEnd.Value = DateTime.Today.AddHours(17);

                // Sự kiện Checkbox
                chkDay.CheckedChanged += (s, e) => {
                    dtpStart.Enabled = chkDay.Checked;
                    dtpEnd.Enabled = chkDay.Checked;
                };

                pnlRow.Controls.AddRange(new Control[] { chkDay, dtpStart, lblTo, dtpEnd });
                flowSchedule.Controls.Add(pnlRow);

                // Lưu vào Dictionary để truy xuất sau này
                scheduleControls.Add(day, (chkDay, dtpStart, dtpEnd));
            }

            // Buttons
            int btnTop = isEditMode ? 610 : 240;
            btnSave = new Button { Text = "Lưu", Top = btnTop, Left = 300, Width = 100, Height = 35, BackColor = Color.FromArgb(23, 162, 184), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            btnCancel = new Button { Text = "Hủy", Top = btnTop, Left = 420, Width = 100, Height = 35, BackColor = Color.Gray, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10) };

            btnSave.Click += BtnSave_Click;
            btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;

            this.Controls.AddRange(new Control[] { lblName, txtName, lblEmail, txtEmail, lblPhone, txtPhone, lblSpec, cboSpecialty, lblImg, txtImagePath, btnBrowse, lblStatus, cboStatus, grpSchedule, btnSave, btnCancel });
        }

        // Hàm parse chuỗi lịch cũ để hiển thị lên form
        private void LoadExistingSchedule(string scheduleStr)
        {
            if (string.IsNullOrEmpty(scheduleStr)) return;

            // Format chuỗi: "Thứ 2: 08:00-17:00; Thứ 3: 13:00-17:00"
            var parts = scheduleStr.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                try
                {
                    // part = " Thứ 2: 08:00-17:00"
                    var dayParts = part.Split(':');
                    if (dayParts.Length < 2) continue;

                    string dayName = dayParts[0].Trim(); // "Thứ 2"
                    string timeRange = dayParts[1].Trim(); // "08:00-17:00"

                    var times = timeRange.Split('-');
                    if (times.Length < 2) continue;

                    if (scheduleControls.ContainsKey(dayName))
                    {
                        var ctrl = scheduleControls[dayName];
                        ctrl.chk.Checked = true;
                        ctrl.start.Value = DateTime.Parse(times[0]);
                        ctrl.end.Value = DateTime.Parse(times[1]);
                    }
                }
                catch { /* Bỏ qua lỗi parse */ }
            }
        }

        // Helper UI
        private Label CreateLabel(string text, int top) => new Label { Text = text, Left = 20, Top = top, AutoSize = true, Font = new Font("Segoe UI", 10) };
        private TextBox CreateTextBox(int top) => new TextBox { Left = 150, Top = top, Width = 320, Font = new Font("Segoe UI", 10) };

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) || cboSpecialty.SelectedIndex == -1) { MessageBox.Show("Thiếu thông tin!"); return; }

            DocName = txtName.Text; Email = txtEmail.Text; Phone = txtPhone.Text;
            Specialty = cboSpecialty.SelectedItem.ToString(); ImagePath = txtImagePath.Text;

            if (isEditMode)
            {
                Status = cboStatus.SelectedItem.ToString();

                // --- LẤY DỮ LIỆU TỪ LỊCH ---
                List<string> scheduleList = new List<string>();
                foreach (var day in daysOfWeek)
                {
                    var ctrl = scheduleControls[day];
                    if (ctrl.chk.Checked)
                    {
                        // Format: "Thứ 2: 08:00-17:00"
                        string timeStr = $"{ctrl.start.Value:HH:mm}-{ctrl.end.Value:HH:mm}";
                        scheduleList.Add($"{day}: {timeStr}");
                    }
                }
                ScheduleString = string.Join("; ", scheduleList);
            }
            else Status = "Đã duyệt";

            DialogResult = DialogResult.OK;
        }
    }
}
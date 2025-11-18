using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace BookingCareManagement.WinForms.Areas.Admin.Forms
{
    public partial class Specialty : Form
    {
        private List<SpecialtyInfo> specialties;
        private List<SpecialtyInfo> filteredSpecialties;

        public Specialty()
        {
            InitializeComponent();

            // Thiết lập sự kiện
            this.Load += Specialty_Load;
            this.textBoxSearch.Enter += TextBoxSearch_Enter;
            this.textBoxSearch.Leave += TextBoxSearch_Leave;
            this.textBoxSearch.TextChanged += TextBoxSearch_TextChanged;
            this.buttonAdd.Click += ButtonAdd_Click;
            this.buttonEdit.Click += ButtonEdit_Click;
            this.buttonDelete.Click += ButtonDelete_Click;
            this.dataGridViewSpecialties.CellDoubleClick += DataGridViewSpecialties_CellDoubleClick;
        }

        private void Specialty_Load(object sender, EventArgs e)
        {
            InitializeSampleData();
            LoadSpecialties();

            // Disable nút sửa và xóa ban đầu
            buttonEdit.Enabled = false;
            buttonDelete.Enabled = false;

            // Sự kiện selection changed
            dataGridViewSpecialties.SelectionChanged += DataGridViewSpecialties_SelectionChanged;
        }

        private void InitializeSampleData()
        {
            specialties = new List<SpecialtyInfo>
            {
                new SpecialtyInfo { Id = 1, Name = "Nha Khoa", Doctors = "BS. Nguyễn Văn A, BS. Trần Thị B", Price = 200000, ImagePath = "" },
                new SpecialtyInfo { Id = 2, Name = "Tim Mạch", Doctors = "BS. Phạm Văn D", Price = 500000, ImagePath = "" },
                new SpecialtyInfo { Id = 3, Name = "Nội Khoa", Doctors = "BS. Đỗ Văn F, BS. Vũ Thị G", Price = 300000, ImagePath = "" },
                new SpecialtyInfo { Id = 4, Name = "Ngoại Khoa", Doctors = "BS. Bùi Văn H", Price = 800000, ImagePath = "" },
                new SpecialtyInfo { Id = 5, Name = "Sản Phụ Khoa", Doctors = "BS. Đinh Thị I", Price = 400000, ImagePath = "" },
            };

            filteredSpecialties = new List<SpecialtyInfo>(specialties);
        }

        private void LoadSpecialties()
        {
            dataGridViewSpecialties.Rows.Clear();

            foreach (var specialty in filteredSpecialties)
            {
                int rowIndex = dataGridViewSpecialties.Rows.Add();
                DataGridViewRow row = dataGridViewSpecialties.Rows[rowIndex];

                // Ảnh
                if (!string.IsNullOrEmpty(specialty.ImagePath) && System.IO.File.Exists(specialty.ImagePath))
                {
                    row.Cells[0].Value = Image.FromFile(specialty.ImagePath);
                }
                else
                {
                    Bitmap placeholder = new Bitmap(60, 60);
                    using (Graphics g = Graphics.FromImage(placeholder))
                    {
                        g.Clear(Color.FromArgb(220, 220, 220));
                        using (Font font = new Font("Segoe UI", 20, FontStyle.Bold))
                        {
                            string initial = specialty.Name.Length > 0 ? specialty.Name.Substring(0, 1) : "?";
                            SizeF size = g.MeasureString(initial, font);
                            g.DrawString(initial, font, Brushes.Gray,
                                (60 - size.Width) / 2, (60 - size.Height) / 2);
                        }
                    }
                    row.Cells[0].Value = placeholder;
                }

                row.Cells[1].Value = specialty.Name;
                row.Cells[2].Value = specialty.Doctors;
                row.Cells[3].Value = specialty.Price.ToString("N0") + " VNĐ";
                row.Tag = specialty.Id;
            }
            labelCount.Text = $"({filteredSpecialties.Count})";
        }

        private void TextBoxSearch_Enter(object sender, EventArgs e)
        {
            if (textBoxSearch.Text == "Tìm kiếm chuyên khoa...")
            {
                textBoxSearch.Text = "";
                textBoxSearch.ForeColor = Color.Black;
            }
        }

        private void TextBoxSearch_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxSearch.Text))
            {
                textBoxSearch.Text = "Tìm kiếm chuyên khoa...";
                textBoxSearch.ForeColor = Color.Gray;
            }
        }

        private void TextBoxSearch_TextChanged(object sender, EventArgs e)
        {
            if (textBoxSearch.Text == "Tìm kiếm chuyên khoa..." || string.IsNullOrWhiteSpace(textBoxSearch.Text))
            {
                filteredSpecialties = new List<SpecialtyInfo>(specialties);
            }
            else
            {
                string searchText = textBoxSearch.Text.ToLower();
                filteredSpecialties = specialties.Where(s =>
                    s.Name.ToLower().Contains(searchText) ||
                    s.Doctors.ToLower().Contains(searchText))
                    .ToList();
            }

            LoadSpecialties();
        }

        private void DataGridViewSpecialties_SelectionChanged(object sender, EventArgs e)
        {
            bool hasSelection = dataGridViewSpecialties.SelectedRows.Count > 0;
            buttonEdit.Enabled = hasSelection;
            buttonDelete.Enabled = hasSelection;
        }

        private void ButtonAdd_Click(object sender, EventArgs e)
        {
            SpecialtyDialog dialog = new SpecialtyDialog();
            dialog.Text = "Thêm Chuyên Khoa Mới";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                SpecialtyInfo newSpecialty = new SpecialtyInfo
                {
                    Id = specialties.Count > 0 ? specialties.Max(s => s.Id) + 1 : 1,
                    Name = dialog.SpecialtyName,
                    Doctors = dialog.Doctors,
                    Price = dialog.Price,
                    ImagePath = dialog.ImagePath
                };

                specialties.Add(newSpecialty);
                filteredSpecialties = new List<SpecialtyInfo>(specialties);
                LoadSpecialties();

                MessageBox.Show("Thêm chuyên khoa thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ButtonEdit_Click(object sender, EventArgs e)
        {
            if (dataGridViewSpecialties.SelectedRows.Count == 0) return;

            int selectedId = (int)dataGridViewSpecialties.SelectedRows[0].Tag;
            SpecialtyInfo selectedSpecialty = specialties.FirstOrDefault(s => s.Id == selectedId);

            if (selectedSpecialty != null)
            {
                SpecialtyDialog dialog = new SpecialtyDialog(selectedSpecialty);
                dialog.Text = "Sửa Thông Tin Chuyên Khoa";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    selectedSpecialty.Name = dialog.SpecialtyName;
                    selectedSpecialty.Doctors = dialog.Doctors;
                    selectedSpecialty.Price = dialog.Price;
                    selectedSpecialty.ImagePath = dialog.ImagePath;

                    LoadSpecialties();
                    MessageBox.Show("Cập nhật chuyên khoa thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void ButtonDelete_Click(object sender, EventArgs e)
        {
            if (dataGridViewSpecialties.SelectedRows.Count == 0) return;

            int selectedId = (int)dataGridViewSpecialties.SelectedRows[0].Tag;
            SpecialtyInfo selectedSpecialty = specialties.FirstOrDefault(s => s.Id == selectedId);

            if (selectedSpecialty != null)
            {
                DialogResult result = MessageBox.Show(
                    $"Bạn có chắc chắn muốn xóa chuyên khoa '{selectedSpecialty.Name}'?",
                    "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    specialties.Remove(selectedSpecialty);
                    filteredSpecialties = new List<SpecialtyInfo>(specialties);
                    LoadSpecialties();
                    MessageBox.Show("Xóa chuyên khoa thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void DataGridViewSpecialties_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) ButtonEdit_Click(sender, e);
        }
    }

    public class SpecialtyInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Doctors { get; set; }
        public decimal Price { get; set; }
        public string ImagePath { get; set; }
    }

    // --- CẬP NHẬT DIALOG ĐỂ CHỌN NHIỀU BÁC SĨ ---
    public class SpecialtyDialog : Form
    {
        private TextBox textBoxName;
        private CheckedListBox checkedListBoxDoctors; // Thay TextBox bằng CheckedListBox
        private TextBox textBoxPrice;
        private TextBox textBoxImagePath;
        private Button buttonBrowse;
        private Button buttonSave;
        private Button buttonCancel;

        // Danh sách bác sĩ giả định (Trong thực tế lấy từ Database)
        private List<string> availableDoctors = new List<string>
        {
            "BS. Nguyễn Văn A", "BS. Trần Thị B", "BS. Lê Văn C",
            "BS. Phạm Văn D", "BS. Hoàng Thị E", "BS. Đỗ Văn F",
            "BS. Vũ Thị G", "BS. Bùi Văn H", "BS. Đinh Thị I",
            "BS. Ngô Văn K", "BS. Phan Thị L", "BS. Mai Văn M"
        };

        public string SpecialtyName { get; private set; }
        public string Doctors { get; private set; }
        public decimal Price { get; private set; }
        public string ImagePath { get; private set; }

        public SpecialtyDialog(SpecialtyInfo specialty = null)
        {
            InitializeDialog();

            if (specialty != null)
            {
                textBoxName.Text = specialty.Name;
                textBoxPrice.Text = specialty.Price.ToString();
                textBoxImagePath.Text = specialty.ImagePath;

                // Tích chọn các bác sĩ đã có
                if (!string.IsNullOrEmpty(specialty.Doctors))
                {
                    var currentDoctors = specialty.Doctors.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < checkedListBoxDoctors.Items.Count; i++)
                    {
                        string doctorName = checkedListBoxDoctors.Items[i].ToString();
                        if (currentDoctors.Contains(doctorName))
                        {
                            checkedListBoxDoctors.SetItemChecked(i, true);
                        }
                    }
                }
            }
        }

        private void InitializeDialog()
        {
            this.Size = new Size(550, 450); // Tăng chiều cao form một chút
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Tên chuyên khoa
            Label lblName = new Label { Text = "Tên chuyên khoa:", Location = new Point(20, 20), Size = new Size(120, 25), Font = new Font("Segoe UI", 10) };
            textBoxName = new TextBox { Location = new Point(150, 20), Size = new Size(350, 25), Font = new Font("Segoe UI", 10) };

            // Bác sĩ (CheckedListBox)
            Label lblDoctors = new Label { Text = "Chọn Bác sĩ:", Location = new Point(20, 70), Size = new Size(120, 25), Font = new Font("Segoe UI", 10) };

            checkedListBoxDoctors = new CheckedListBox
            {
                Location = new Point(150, 70),
                Size = new Size(350, 120), // Chiều cao lớn hơn để hiện danh sách
                Font = new Font("Segoe UI", 10),
                CheckOnClick = true
            };
            // Thêm dữ liệu vào List
            checkedListBoxDoctors.Items.AddRange(availableDoctors.ToArray());

            // Giá tiền
            Label lblPrice = new Label { Text = "Giá tiền (VNĐ):", Location = new Point(20, 210), Size = new Size(120, 25), Font = new Font("Segoe UI", 10) };
            textBoxPrice = new TextBox { Location = new Point(150, 210), Size = new Size(350, 25), Font = new Font("Segoe UI", 10) };

            // Đường dẫn ảnh
            Label lblImage = new Label { Text = "Ảnh:", Location = new Point(20, 260), Size = new Size(120, 25), Font = new Font("Segoe UI", 10) };
            textBoxImagePath = new TextBox { Location = new Point(150, 260), Size = new Size(270, 25), Font = new Font("Segoe UI", 10), ReadOnly = true };
            buttonBrowse = new Button { Text = "Chọn...", Location = new Point(430, 258), Size = new Size(70, 29), Font = new Font("Segoe UI", 9) };
            buttonBrowse.Click += ButtonBrowse_Click;

            // Nút Lưu & Hủy
            buttonSave = new Button { Text = "Lưu", Location = new Point(280, 320), Size = new Size(100, 35), Font = new Font("Segoe UI", 10, FontStyle.Bold), BackColor = Color.FromArgb(23, 162, 184), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            buttonSave.FlatAppearance.BorderSize = 0;
            buttonSave.Click += ButtonSave_Click;

            buttonCancel = new Button { Text = "Hủy", Location = new Point(400, 320), Size = new Size(100, 35), Font = new Font("Segoe UI", 10), BackColor = Color.Gray, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            buttonCancel.FlatAppearance.BorderSize = 0;
            buttonCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            this.Controls.AddRange(new Control[] { lblName, textBoxName, lblDoctors, checkedListBoxDoctors, lblPrice, textBoxPrice, lblImage, textBoxImagePath, buttonBrowse, buttonSave, buttonCancel });
        }

        private void ButtonBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog { Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif", Title = "Chọn ảnh chuyên khoa" };
            if (openFileDialog.ShowDialog() == DialogResult.OK) textBoxImagePath.Text = openFileDialog.FileName;
        }

        private void ButtonSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxName.Text))
            {
                MessageBox.Show("Vui lòng nhập tên chuyên khoa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(textBoxPrice.Text, out decimal price) || price <= 0)
            {
                MessageBox.Show("Vui lòng nhập giá tiền hợp lệ!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Lấy danh sách bác sĩ đã chọn
            List<string> selectedDocs = new List<string>();
            foreach (var item in checkedListBoxDoctors.CheckedItems)
            {
                selectedDocs.Add(item.ToString());
            }

            if (selectedDocs.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn ít nhất một bác sĩ!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SpecialtyName = textBoxName.Text.Trim();
            Doctors = string.Join(", ", selectedDocs); // Gộp thành chuỗi
            Price = price;
            ImagePath = textBoxImagePath.Text.Trim();

            this.DialogResult = DialogResult.OK;
        }
    }
}
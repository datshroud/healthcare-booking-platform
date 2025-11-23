using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using BookingCareManagement.WinForms.Areas.Admin.Models;
using BookingCareManagement.WinForms.Areas.Admin.Services;
using BookingCareManagement.WinForms.Shared.Models.Dtos;
using System.Net.Http;

namespace BookingCareManagement.WinForms.Areas.Admin.Forms
{
    public partial class Specialty : Form
    {
        private List<SpecialtyDto> specialties = new();
        private List<SpecialtyDto> filteredSpecialties = new();
        private List<DoctorDto> doctors = new();

        private readonly AdminSpecialtyApiClient _specialtyApiClient;
        private readonly AdminDoctorApiClient _doctorApiClient;

        // shared HttpClient for async downloads
        private static readonly HttpClient _httpClient = new HttpClient();

        public Specialty(AdminSpecialtyApiClient specialtyApiClient, AdminDoctorApiClient doctorApiClient)
        {
            _specialtyApiClient = specialtyApiClient;
            _doctorApiClient = doctorApiClient;

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

        private async void Specialty_Load(object sender, EventArgs e)
        {
            // Disable nút sửa và xóa ban đầu
            buttonEdit.Enabled = false;
            buttonDelete.Enabled = false;

            // Sự kiện selection changed
            dataGridViewSpecialties.SelectionChanged += DataGridViewSpecialties_SelectionChanged;
            
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                // Hiển thị loading
                this.Cursor = Cursors.WaitCursor;
                
                // Load danh sách chuyên khoa và bác sĩ từ API
                var specialtiesTask = _specialtyApiClient.GetAllAsync();
                var doctorsTask = _doctorApiClient.GetAllAsync();
                
                await Task.WhenAll(specialtiesTask, doctorsTask);
                
                specialties = specialtiesTask.Result?.ToList() ?? new List<SpecialtyDto>();
                doctors = doctorsTask.Result?.ToList() ?? new List<DoctorDto>();
                
                filteredSpecialties = new List<SpecialtyDto>(specialties);
                LoadSpecialties();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void LoadSpecialties()
        {
            dataGridViewSpecialties.Rows.Clear();

            foreach (var specialty in filteredSpecialties)
            {
                int rowIndex = dataGridViewSpecialties.Rows.Add();
                DataGridViewRow row = dataGridViewSpecialties.Rows[rowIndex];

                // Show placeholder immediately
                row.Cells[0].Value = CreatePlaceholder(specialty.Name, specialty.Color);

                // Start async load and update cell when ready
                _ = LoadSpecialtyImageAsync(specialty, row);

                row.Cells[1].Value = specialty.Name;
                
                // Hiển thị danh sách bác sĩ
                var doctorNames = specialty.Doctors.Select(d => d.FullName);
                row.Cells[2].Value = doctorNames.Any() ? string.Join(", ", doctorNames) : "(Chưa có bác sĩ)";

                // Hiển thị giá tiền
                row.Cells[3].Value = specialty.Price > 0 
                    ? string.Format("{0:N0} VNĐ", specialty.Price) 
                    : "Liên hệ";
                
                row.Tag = specialty.Id;
            }
            labelCount.Text = $"({filteredSpecialties.Count})";
        }

        private async Task LoadSpecialtyImageAsync(SpecialtyDto specialty, DataGridViewRow row)
        {
            if (string.IsNullOrEmpty(specialty.ImageUrl))
            {
                return; // placeholder already set
            }

            try
            {
                // Local file
                if (File.Exists(specialty.ImageUrl))
                {
                    using var original = Image.FromFile(specialty.ImageUrl);
                    var resized = ResizeImage(original, 60, 60);
                    // update UI on UI thread
                    if (row.DataGridView != null && !row.DataGridView.IsDisposed)
                    {
                        dataGridViewSpecialties.InvokeIfRequired(() => row.Cells[0].Value = resized);
                    }
                    return;
                }

                // Remote URL
                if (Uri.TryCreate(specialty.ImageUrl, UriKind.Absolute, out Uri? uriResult)
                    && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
                {
                    // download async
                    using var resp = await _httpClient.GetAsync(specialty.ImageUrl);
                    if (!resp.IsSuccessStatusCode) return;
                    await using var stream = await resp.Content.ReadAsStreamAsync();
                    using var original = Image.FromStream(stream);
                    var resized = ResizeImage(original, 60, 60);
                    if (row.DataGridView != null && !row.DataGridView.IsDisposed)
                    {
                        dataGridViewSpecialties.InvokeIfRequired(() => row.Cells[0].Value = resized);
                    }
                    return;
                }
            }
            catch
            {
                // ignore and keep placeholder
            }
        }

        /// <summary>
        /// Load ảnh từ file/URL hoặc tạo placeholder nếu không có ảnh
        /// (kept for compatibility but no longer used synchronously)
        /// </summary>
        private Image LoadSpecialtyImage(SpecialtyDto specialty)
        {
            // Return placeholder quickly; real image will be loaded async
            return CreatePlaceholder(specialty.Name, specialty.Color);
        }

        /// <summary>
        /// Resize ảnh về kích thước mong muốn
        /// </summary>
        private Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                using (var wrapMode = new System.Drawing.Imaging.ImageAttributes())
                {
                    wrapMode.SetWrapMode(System.Drawing.Drawing2D.WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        private Bitmap CreatePlaceholder(string name, string? colorHex)
        {
            Bitmap placeholder = new Bitmap(60, 60);
            using (Graphics g = Graphics.FromImage(placeholder))
            {
                Color bgColor;
                try
                {
                    bgColor = string.IsNullOrEmpty(colorHex) 
                        ? Color.FromArgb(220, 220, 220) 
                        : ColorTranslator.FromHtml(colorHex);
                }
                catch
                {
                    bgColor = Color.FromArgb(220, 220, 220);
                }
                
                g.Clear(bgColor);
                using (Font font = new Font("Segoe UI", 20, FontStyle.Bold))
                {
                    string initial = name.Length > 0 ? name.Substring(0, 1).ToUpper() : "?";
                    SizeF size = g.MeasureString(initial, font);
                    
                    // Chọn màu chữ dựa trên độ sáng của màu nền
                    Brush textBrush = GetBrightness(bgColor) > 128 ? Brushes.Black : Brushes.White;
                    g.DrawString(initial, font, textBrush,
                        (60 - size.Width) / 2, (60 - size.Height) / 2);
                }
            }
            return placeholder;
        }

        private int GetBrightness(Color color)
        {
            return (int)Math.Sqrt(
                color.R * color.R * 0.241 +
                color.G * color.G * 0.691 +
                color.B * color.B * 0.068);
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
                filteredSpecialties = new List<SpecialtyDto>(specialties);
            }
            else
            {
                string searchText = textBoxSearch.Text.ToLower();
                filteredSpecialties = specialties.Where(s =>
                    s.Name.ToLower().Contains(searchText) ||
                    s.Doctors.Any(d => d.FullName.ToLower().Contains(searchText)))
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

        private async void ButtonAdd_Click(object sender, EventArgs e)
        {
            var editorForm = new SpecialtyEditorForm(doctors);
            if (editorForm.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    this.Cursor = Cursors.WaitCursor;
                    
                    var request = editorForm.BuildRequest();

                    // If image is a local file, upload it first
                    if (!string.IsNullOrWhiteSpace(request.ImageUrl) && File.Exists(request.ImageUrl))
                    {
                        try
                        {
                            var uploaded = await _specialtyApiClient.UploadFileAsync(request.ImageUrl);
                            if (!string.IsNullOrWhiteSpace(uploaded))
                            {
                                request.ImageUrl = uploaded;
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Không thể upload ảnh: {ex.Message}", "Lỗi upload", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            request.ImageUrl = null;
                        }
                    }

                    var createdSpecialty = await _specialtyApiClient.CreateAsync(request);
                    
                    if (createdSpecialty != null)
                    {
                        MessageBox.Show("Thêm chuyên khoa thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadDataAsync();
                    }
                    else
                    {
                        MessageBox.Show("Không thể thêm chuyên khoa. Vui lòng thử lại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi thêm chuyên khoa: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    this.Cursor = Cursors.Default;
                }
            }
        }

        private async void ButtonEdit_Click(object sender, EventArgs e)
        {
            if (dataGridViewSpecialties.SelectedRows.Count == 0) return;

            Guid selectedId = (Guid)dataGridViewSpecialties.SelectedRows[0].Tag;
            SpecialtyDto? selectedSpecialty = specialties.FirstOrDefault(s => s.Id == selectedId);

            if (selectedSpecialty != null)
            {
                var editorForm = new SpecialtyEditorForm(doctors, selectedSpecialty);
                if (editorForm.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        this.Cursor = Cursors.WaitCursor;
                        
                        var request = editorForm.BuildRequest();

                        // If image is a local file, upload it first
                        if (!string.IsNullOrWhiteSpace(request.ImageUrl) && File.Exists(request.ImageUrl))
                        {
                            try
                            {
                                var uploaded = await _specialtyApiClient.UploadFileAsync(request.ImageUrl);
                                if (!string.IsNullOrWhiteSpace(uploaded))
                                {
                                    request.ImageUrl = uploaded;
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Không thể upload ảnh: {ex.Message}", "Lỗi upload", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                request.ImageUrl = null;
                            }
                        }

                        await _specialtyApiClient.UpdateAsync(selectedId, request);
                        
                        MessageBox.Show("Cập nhật chuyên khoa thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadDataAsync();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi khi cập nhật chuyên khoa: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        this.Cursor = Cursors.Default;
                    }
                }
            }
        }

        private async void ButtonDelete_Click(object sender, EventArgs e)
        {
            if (dataGridViewSpecialties.SelectedRows.Count == 0) return;

            Guid selectedId = (Guid)dataGridViewSpecialties.SelectedRows[0].Tag;
            SpecialtyDto? selectedSpecialty = specialties.FirstOrDefault(s => s.Id == selectedId);

            if (selectedSpecialty != null)
            {
                DialogResult result = MessageBox.Show(
                    $"Bạn có chắc chắn muốn xóa chuyên khoa '{selectedSpecialty.Name}'?",
                    "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        this.Cursor = Cursors.WaitCursor;
                        
                        await _specialtyApiClient.DeleteAsync(selectedId);
                        
                        MessageBox.Show("Xóa chuyên khoa thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadDataAsync();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi khi xóa chuyên khoa: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        this.Cursor = Cursors.Default;
                    }
                }
            }
        }

        private void DataGridViewSpecialties_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) ButtonEdit_Click(sender, e);
        }
    }

    // helper for invoking actions on UI thread
    internal static class ControlExtensions
    {
        public static void InvokeIfRequired(this Control c, Action action)
        {
            if (c.IsHandleCreated && c.InvokeRequired)
            {
                c.Invoke(action);
            }
            else
            {
                action();
            }
        }
    }
}
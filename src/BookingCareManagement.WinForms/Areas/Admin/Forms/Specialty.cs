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
        // keep reference to currently shown context menu so it stays alive
        private ContextMenuStrip? _activeContextMenu;

        private List<SpecialtyDto> specialties = new();
        private List<SpecialtyDto> filteredSpecialties = new();
        private List<DoctorDto> doctors = new();

        private readonly AdminSpecialtyApiClient _specialtyApiClient;
        private readonly AdminDoctorApiClient _doctorApiClient;

        // Pagination fields
        private Panel panelPager;
        private Button btnPrevPage;
        private Button btnNextPage;
        private ComboBox comboPageSize;
        private Label lblPageInfoPager;
        private int _currentPage = 1;
        private int _pageSize = 7; // default 7 per page
        private int _totalItems = 0;

        // shared HttpClient for async downloads
        private static readonly HttpClient _httpClient = new HttpClient();

        public Specialty(AdminSpecialtyApiClient specialtyApiClient, AdminDoctorApiClient doctorApiClient)
        {
            _specialtyApiClient = specialtyApiClient;
            _doctorApiClient = doctorApiClient;

            InitializeComponent();

            BuildPager();

            // Thiết lập sự kiện
            this.Load += Specialty_Load;
            this.textBoxSearch.Enter += TextBoxSearch_Enter;
            this.textBoxSearch.Leave += TextBoxSearch_Leave;
            this.textBoxSearch.TextChanged += TextBoxSearch_TextChanged;
            this.buttonAdd.Click += ButtonAdd_Click;
            this.dataGridViewSpecialties.CellDoubleClick += DataGridViewSpecialties_CellDoubleClick;
            // show actions on mouse up to ensure menu receives click
            this.dataGridViewSpecialties.CellMouseUp += DataGridViewSpecialties_CellMouseUp;

            this.Shown += Specialty_Shown;
        }

        private void Specialty_Shown(object? sender, EventArgs e)
        {
            try
            {
                // place labelCount to the right of title to avoid overlap
                labelCount.Left = labelTitle.Right + 8;
                labelCount.Top = labelTitle.Top + (labelTitle.Height - labelCount.Height) / 2;
                labelCount.BringToFront();
            }
            catch { }
        }

        private void BuildPager()
        {
            panelPager = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 56,
                BackColor = Color.White,
                Padding = new Padding(27, 8, 27, 8)
            };

            var pagerInner = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = false,
                Width = 360,
                Padding = new Padding(6),
                BackColor = Color.FromArgb(248, 250, 252)
            };

            lblPageInfoPager = new Label { AutoSize = true, Text = "Trang 0 / 0", Padding = new Padding(0, 10, 6, 0) };
            // pager buttons with white background and black text
            btnPrevPage = new Button { Text = "‹ Trước", AutoSize = true, Enabled = false, FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = Color.Black, Cursor = Cursors.Hand };
            btnNextPage = new Button { Text = "Tiếp ›", AutoSize = true, Enabled = false, FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = Color.Black, Cursor = Cursors.Hand };
            // small border for pager buttons
            btnPrevPage.FlatAppearance.BorderSize = 1;
            btnPrevPage.FlatAppearance.BorderColor = Color.FromArgb(220, 220, 220);
            btnNextPage.FlatAppearance.BorderSize = 1;
            btnNextPage.FlatAppearance.BorderColor = Color.FromArgb(220, 220, 220);

            comboPageSize = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 80 };
            comboPageSize.Items.AddRange(new object[] { "7", "10", "25", "50", "100" });
            comboPageSize.SelectedItem = _pageSize.ToString();

            btnPrevPage.Click += (_, _) => { if (_currentPage > 1) { _currentPage--; LoadSpecialties(); } };
            btnNextPage.Click += (_, _) => { _currentPage++; LoadSpecialties(); };
            comboPageSize.SelectedIndexChanged += (_, _) => { if (int.TryParse(comboPageSize.SelectedItem?.ToString(), out var s)) { _pageSize = s; _currentPage = 1; LoadSpecialties(); } };

            pagerInner.Controls.Add(lblPageInfoPager);
            pagerInner.Controls.Add(new Label { Width = 12 });
            pagerInner.Controls.Add(btnPrevPage);
            pagerInner.Controls.Add(btnNextPage);
            pagerInner.Controls.Add(new Label { Width = 12 });
            pagerInner.Controls.Add(new Label { Text = "Hiển thị:", AutoSize = true, Padding = new Padding(6, 12, 0, 0) });
            pagerInner.Controls.Add(comboPageSize);

            panelPager.Controls.Add(pagerInner);

            // add a small bottom spacer so pager appears slightly above the bottom
            var panelBottomSpacer = new Panel { Dock = DockStyle.Bottom, Height = 12, BackColor = Color.Transparent };
            this.Controls.Add(panelBottomSpacer);
            // Add pager above spacer
            this.Controls.Add(panelPager);
            panelPager.BringToFront();
        }

        private async void Specialty_Load(object sender, EventArgs e)
        {
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
                _totalItems = filteredSpecialties.Count;
                _currentPage = 1;
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

            // paging
            _totalItems = filteredSpecialties.Count;
            int totalPages = _pageSize <= 0 ? 1 : Math.Max(1, (int)Math.Ceiling(_totalItems / (double)_pageSize));
            if (_currentPage < 1) _currentPage = 1;
            if (_currentPage > totalPages) _currentPage = totalPages;
            var paged = filteredSpecialties.Skip((_currentPage - 1) * _pageSize).Take(_pageSize).ToList();

            foreach (var specialty in paged)
            {
                int rowIndex = dataGridViewSpecialties.Rows.Add();
                DataGridViewRow row = dataGridViewSpecialties.Rows[rowIndex];

                // Show placeholder immediately (circular)
                row.Cells[0].Value = CreateCircularPlaceholder(specialty.Name, specialty.Color);

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

                // Hiển thị trạng thái
                row.Cells[4].Value = specialty.Active ? "Hoạt động" : "Không hoạt động";

                // Actions button
                row.Cells[5].Value = "···";

                row.Tag = specialty.Id;
            }
            labelCount.Text = $"({_totalItems})";

            // update pager UI
            int totalPagesNow = _pageSize <= 0 ? 1 : Math.Max(1, (int)Math.Ceiling(_totalItems / (double)_pageSize));
            lblPageInfoPager.Text = $"Trang {_currentPage} / {totalPagesNow}";
            btnPrevPage.Enabled = _currentPage > 1;
            btnNextPage.Enabled = _currentPage < totalPagesNow;
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
                    var circular = MakeCircularImage(resized, 60);
                    // update UI on UI thread
                    if (row.DataGridView != null && !row.DataGridView.IsDisposed)
                    {
                        dataGridViewSpecialties.InvokeIfRequired(() => row.Cells[0].Value = circular);
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
                    var circular = MakeCircularImage(resized, 60);
                    if (row.DataGridView != null && !row.DataGridView.IsDisposed)
                    {
                        dataGridViewSpecialties.InvokeIfRequired(() => row.Cells[0].Value = circular);
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
            return CreateCircularPlaceholder(specialty.Name, specialty.Color);
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

        private Bitmap CreateCircularPlaceholder(string name, string? colorHex)
        {
            int size = 60;
            var bmp = new Bitmap(size, size);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
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

                // Fill circle background
                using (Brush b = new SolidBrush(bgColor))
                {
                    g.FillEllipse(b, 0, 0, size - 1, size - 1);
                }

                using (Font font = new Font("Segoe UI", 20, FontStyle.Bold))
                {
                    string initial = !string.IsNullOrEmpty(name) ? name.Substring(0, 1).ToUpper() : "?";
                    SizeF sizeF = g.MeasureString(initial, font);

                    // Choose text color based on brightness
                    Brush textBrush = GetBrightness(bgColor) > 128 ? Brushes.Black : Brushes.White;
                    g.DrawString(initial, font, textBrush,
                        (size - sizeF.Width) / 2, (size - sizeF.Height) / 2);
                }

                // Draw subtle border
                using (Pen p = new Pen(Color.FromArgb(200, 200, 200)))
                {
                    g.DrawEllipse(p, 0, 0, size - 1, size - 1);
                }
            }
            return bmp;
        }

        private Image MakeCircularImage(Image source, int diameter)
        {
            var bmp = new Bitmap(diameter, diameter);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var path = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    path.AddEllipse(0, 0, diameter - 1, diameter - 1);
                    g.SetClip(path);
                    g.DrawImage(source, 0, 0, diameter, diameter);
                }
                // optional border
                using (var pen = new Pen(Color.FromArgb(200, 200, 200)))
                {
                    g.ResetClip();
                    g.DrawEllipse(pen, 0, 0, diameter - 1, diameter - 1);
                }
            }
            return bmp;
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
            if (textBoxSearch.Text == "🔍 Tìm kiếm...")
            {
                textBoxSearch.Text = "";
                textBoxSearch.ForeColor = Color.Black;
            }
        }

        private void TextBoxSearch_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxSearch.Text))
            {
                textBoxSearch.Text = "🔍 Tìm kiếm...";
                textBoxSearch.ForeColor = Color.FromArgb(100, 100, 100);
            }
        }

        private void TextBoxSearch_TextChanged(object sender, EventArgs e)
        {
            if (textBoxSearch.Text == "🔍 Tìm kiếm..." || string.IsNullOrWhiteSpace(textBoxSearch.Text))
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
            _currentPage = 1;
            LoadSpecialties();
         }

        private void DataGridViewSpecialties_SelectionChanged(object sender, EventArgs e)
        {
            // Selection changed event - no longer needed for button enabling
            // since Edit/Delete buttons are removed from the UI
        }

        private void DataGridViewSpecialties_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            // Check if click is on the Actions button column (column 5)
            if (e.ColumnIndex == 5)
            {
                DataGridViewRow row = dataGridViewSpecialties.Rows[e.RowIndex];
                var cellRect = dataGridViewSpecialties.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true);
                var screenPos = dataGridViewSpecialties.PointToScreen(new Point(cellRect.Left + cellRect.Width / 2, cellRect.Top + cellRect.Height));
                // Show context menu or handle actions
                ShowSpecialtyActions(row, screenPos);
            }
        }

        private void DataGridViewSpecialties_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (e.ColumnIndex == 5 && e.Button == MouseButtons.Left)
            {
                var row = dataGridViewSpecialties.Rows[e.RowIndex];
                // show at current cursor position
                var screenPos = Cursor.Position;
                this.BeginInvoke(new Action(() => ShowSpecialtyActions(row, screenPos)));
            }
        }

        private void ShowSpecialtyActions(DataGridViewRow row, Point screenPosition)
        {
            if (row?.Tag == null) return;
            Guid selectedId = (Guid)row.Tag;

            // dispose previous if any
            try { _activeContextMenu?.Dispose(); } catch { }

            _activeContextMenu = new ContextMenuStrip();
            _activeContextMenu.Tag = selectedId;
            _activeContextMenu.Items.Add("✏️ Sửa");
            _activeContextMenu.Items.Add("🗑️ Xóa");
            _activeContextMenu.ItemClicked += ActiveContextMenu_ItemClicked;
            _activeContextMenu.Closed += (s, e) => { _activeContextMenu?.Dispose(); _activeContextMenu = null; };

            // show near the cell rect to be consistent
            int colIndex = row.Cells.IndexOf(row.Cells[5]);
            var cellRect = dataGridViewSpecialties.GetCellDisplayRectangle(5, row.Index, true);
            var showAt = dataGridViewSpecialties.PointToScreen(new Point(cellRect.Left + cellRect.Width / 2, cellRect.Bottom));
            _activeContextMenu.Show(showAt);
        }

        private void ActiveContextMenu_ItemClicked(object? sender, ToolStripItemClickedEventArgs e)
        {
            if (sender is not ContextMenuStrip cms) return;
            if (!(cms.Tag is Guid id)) return;

            var text = e.ClickedItem?.Text;
            if (text == "✏️ Sửa") EditSpecialtyById(id);
            else if (text == "🗑️ Xóa") DeleteSpecialtyById(id);
        }

        private void EditSpecialtyById(Guid id)
        {
            var dto = specialties.FirstOrDefault(s => s.Id == id);
            if (dto == null)
            {
                MessageBox.Show("Không tìm thấy chuyên khoa để sửa.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            EditSpecialty(dto);
        }

        private void DeleteSpecialtyById(Guid id)
        {
            var dto = specialties.FirstOrDefault(s => s.Id == id);
            if (dto == null)
            {
                MessageBox.Show("Không tìm thấy chuyên khoa để xóa.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            DeleteSpecialty(dto);
        }

        private async void EditSpecialty(SpecialtyDto selectedSpecialty)
        {
            // ensure any active context menu is closed so dialog gets focus
            try { _activeContextMenu?.Close(); } catch { }

            try
            {
                var editorForm = new SpecialtyEditorForm(doctors, selectedSpecialty);
                // set this form as owner to ensure CenterParent works
                var result = editorForm.ShowDialog(this);

                if (result == DialogResult.OK)
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

                        await _specialtyApiClient.UpdateAsync(selectedSpecialty.Id, request);

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
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi mở form chỉnh sửa: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void DeleteSpecialty(SpecialtyDto selectedSpecialty)
        {
            DialogResult result = MessageBox.Show(
                $"Bạn có chắc chắn muốn xóa chuyên khoa '{selectedSpecialty.Name}'?",
                "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    this.Cursor = Cursors.WaitCursor;

                    await _specialtyApiClient.DeleteAsync(selectedSpecialty.Id);

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
                EditSpecialty(selectedSpecialty);
            }
        }

        private async void ButtonDelete_Click(object sender, EventArgs e)
        {
            if (dataGridViewSpecialties.SelectedRows.Count == 0) return;

            Guid selectedId = (Guid)dataGridViewSpecialties.SelectedRows[0].Tag;
            SpecialtyDto? selectedSpecialty = specialties.FirstOrDefault(s => s.Id == selectedId);

            if (selectedSpecialty != null)
            {
                DeleteSpecialty(selectedSpecialty);
            }
        }

        private void DataGridViewSpecialties_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridViewSpecialties.Rows[e.RowIndex];
                Guid selectedId = (Guid)row.Tag;
                SpecialtyDto? selectedSpecialty = specialties.FirstOrDefault(s => s.Id == selectedId);
                if (selectedSpecialty != null)
                {
                    EditSpecialty(selectedSpecialty);
                }
            }
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
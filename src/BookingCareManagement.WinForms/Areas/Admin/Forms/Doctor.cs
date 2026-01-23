using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using BookingCareManagement.WinForms.Areas.Admin.Models;
using BookingCareManagement.WinForms.Areas.Admin.Services;
using BookingCareManagement.WinForms.Shared.Models.Dtos;
using System.Net;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;

namespace BookingCareManagement.WinForms.Areas.Admin.Forms
{
    public partial class Doctor : Form
    {
        private List<DoctorDto> doctors = new();
        private List<DoctorDto> filteredDoctors = new();
        private List<SpecialtyDto> specialties = new();

        private readonly AdminDoctorApiClient _doctorApiClient;
        private readonly AdminSpecialtyApiClient _specialtyApiClient;

        // Pagination fields
        private Panel panelPager;
        private Button btnPrevPage;
        private Button btnNextPage;
        private ComboBox comboPageSize;
        private Label lblPageInfoPager;
        private int _currentPage = 1;
        private int _pageSize = 7; // default one page shows 7 doctors
        private int _totalItems = 0;

        // shared HttpClient for async avatar downloads
        private static readonly HttpClient _httpClient = new HttpClient();

        public Doctor(AdminDoctorApiClient doctorApiClient, AdminSpecialtyApiClient specialtyApiClient)
        {
            _doctorApiClient = doctorApiClient;
            _specialtyApiClient = specialtyApiClient;

            InitializeComponent();

            // build pager controls programmatically and add to panelMain under dataGridViewDoctors
            BuildPager();

            // =============== ENHANCE UI WITH HOVER EFFECTS ===============
            EnhanceButtonAppearance();

            this.Load += Doctor_Load;
            this.textBoxSearch.Enter += TextBoxSearch_Enter;
            this.textBoxSearch.Leave += TextBoxSearch_Leave;
            this.textBoxSearch.TextChanged += TextBoxSearch_TextChanged;
            this.buttonAdd.Click += ButtonAdd_Click;
            this.buttonFilter.Click += (s, e) => MessageBox.Show("Bộ lọc chưa được triển khai", "Thông báo");
            this.dataGridViewDoctors.CellDoubleClick += DataGridViewDoctors_CellDoubleClick;
            this.dataGridViewDoctors.CellClick += DataGridViewDoctors_CellClick;
            this.dataGridViewDoctors.SelectionChanged += DataGridViewDoctors_SelectionChanged;
        }

        /// <summary>
        /// Enhance button appearance with hover effects and modern styling
        /// </summary>
        private void EnhanceButtonAppearance()
        {
            // Configure Add button (Blue) with hover effect
            buttonAdd.MouseEnter += (s, e) => buttonAdd.BackColor = Color.FromArgb(0, 85, 179);
            buttonAdd.MouseLeave += (s, e) => buttonAdd.BackColor = Color.FromArgb(0, 102, 204);
            
            // Configure Filter button with hover effect
            buttonFilter.MouseEnter += (s, e) => buttonFilter.BackColor = Color.FromArgb(245, 245, 247);
            buttonFilter.MouseLeave += (s, e) => buttonFilter.BackColor = Color.White;
        }

        // =============== ACTIONS COLUMN HANDLING ===============
        private void DataGridViewDoctors_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            // If Actions column clicked, show context menu with options
            if (e.ColumnIndex == dataGridViewDoctors.Columns["ColumnActions"].Index)
            {
                var row = dataGridViewDoctors.Rows[e.RowIndex];
                var id = (Guid)row.Tag;
                var doc = doctors.FirstOrDefault(d => d.Id == id);
                if (doc == null) return;

                var menu = new ContextMenuStrip();
                menu.Items.Add("✏️ Chỉnh sửa", null, async (s, ea) =>
                {
                    // select row and open edit
                    row.Selected = true;
                    ButtonEdit_Click(this, EventArgs.Empty);
                });
                var toggleText = doc.Active ? "Vô hiệu hóa" : "Kích hoạt";
                menu.Items.Add(toggleText, null, async (s, ea) =>
                {
                    try
                    {
                        this.Cursor = Cursors.WaitCursor;
                        await _doctorApiClient.UpdateProfileAsync(id, new { AvatarUrl = doc.AvatarUrl, /* keep avatar */ });
                        // call status endpoint - backend expects PUT /api/Doctor/{id}/status
                        var client = new HttpClient();
                        var resp = await client.PutAsJsonAsync($"/api/Doctor/{id}/status", new { active = !doc.Active });
                    }
                    catch { }
                    finally { this.Cursor = Cursors.Default; }
                    await LoadDataAsync();
                });
                menu.Items.Add("🗑️ Xóa", null, async (s, ea) =>
                {
                    var confirm = MessageBox.Show($"Bạn có chắc chắn muốn xóa bác sĩ '{doc.FullName}'?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (confirm == DialogResult.Yes)
                    {
                        try
                        {
                            this.Cursor = Cursors.WaitCursor;
                            await _doctorApiClient.DeleteAsync(id);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Xóa thất bại: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        finally { this.Cursor = Cursors.Default; }
                        await LoadDataAsync();
                    }
                });

                var p = dataGridViewDoctors.PointToScreen(new Point(dataGridViewDoctors.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false).Right, dataGridViewDoctors.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false).Top));
                menu.Show(p);
            }
        }

        private void BuildPager()
        {
            panelPager = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 90,
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
            // Pager buttons: white background with black text as requested
            btnPrevPage = new Button { Text = "‹ Trước", AutoSize = true, Enabled = false, FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = Color.Black, Cursor = Cursors.Hand };
            btnNextPage = new Button { Text = "Tiếp ›", AutoSize = true, Enabled = false, FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = Color.Black, Cursor = Cursors.Hand };
            // No page-size selector: user requested to remove the "Hiển thị" control
            btnPrevPage.Click += (_, _) => { if (_currentPage > 1) { _currentPage--; LoadDoctors(); } };
            btnNextPage.Click += (_, _) => { _currentPage++; LoadDoctors(); };

            // Add controls in logical order: page info then spacer then buttons
            pagerInner.Controls.Add(lblPageInfoPager);
            pagerInner.Controls.Add(new Label { Width = 12 });
            pagerInner.Controls.Add(btnPrevPage);
            pagerInner.Controls.Add(btnNextPage);

            panelPager.Controls.Add(pagerInner);

            // Place pager at bottom of the form (outside panelMain) so it won't overlap grid content
            this.Controls.Add(panelPager);
            panelPager.BringToFront();
        }

        private async void Doctor_Load(object sender, EventArgs e)
        {
             await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;

                var doctorsTask = _doctorApiClient.GetAllAsync();
                var specialtiesTask = _specialtyApiClient.GetAllAsync();

                await Task.WhenAll(doctorsTask, specialtiesTask);

                doctors = doctorsTask.Result?.ToList() ?? new List<DoctorDto>();
                specialties = specialtiesTask.Result?.ToList() ?? new List<SpecialtyDto>();

                filteredDoctors = new List<DoctorDto>(doctors);
                _totalItems = filteredDoctors.Count;
                _currentPage = 1;
                LoadDoctors();
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

        private void LoadDoctors()
        {
            dataGridViewDoctors.Rows.Clear();

            // compute paging
            _totalItems = filteredDoctors.Count;
            int totalPages = _pageSize <= 0 ? 1 : Math.Max(1, (int)Math.Ceiling(_totalItems / (double)_pageSize));
            if (_currentPage < 1) _currentPage = 1;
            if (_currentPage > totalPages) _currentPage = totalPages;

            var paged = filteredDoctors.Skip((_currentPage - 1) * _pageSize).Take(_pageSize).ToList();

            foreach (var doc in paged)
            {
                int rowIndex = dataGridViewDoctors.Rows.Add();
                DataGridViewRow row = dataGridViewDoctors.Rows[rowIndex];

                // set placeholder immediately
                row.Cells[0].Value = CreatePlaceholder(doc.FullName);

                // async load avatar and update cell when ready
                _ = LoadDoctorAvatarAsync(doc, row);

                row.Cells[1].Value = doc.FullName;
                row.Cells[2].Value = doc.Email;
                row.Cells[3].Value = doc.PhoneNumber;
                row.Cells[4].Value = string.Join(", ", doc.Specialties);
                row.Cells[5].Value = doc.Active ? "Đã duyệt" : "Ngưng hoạt động";

                row.Cells[5].Style.ForeColor = doc.Active ? Color.Green : Color.OrangeRed;
                row.Tag = doc.Id;
            }
            labelCount.Text = $"({_totalItems})";

            // update pager UI
            int totalPagesNow = _pageSize <= 0 ? 1 : Math.Max(1, (int)Math.Ceiling(_totalItems / (double)_pageSize));
            lblPageInfoPager.Text = $"Trang {_currentPage} / {totalPagesNow}";
            btnPrevPage.Enabled = _currentPage > 1;
            btnNextPage.Enabled = _currentPage < totalPagesNow;
        }

        private async Task LoadDoctorAvatarAsync(DoctorDto doc, DataGridViewRow row)
        {
            if (string.IsNullOrWhiteSpace(doc.AvatarUrl)) return;

            try
            {
                // local file
                if (File.Exists(doc.AvatarUrl))
                {
                    using var img = Image.FromFile(doc.AvatarUrl);
                    using var tmp = new Bitmap(img, new Size(60, 60));
                    var rounded = MakeRoundedBitmap(tmp, 60);
                    if (row.DataGridView != null && !row.DataGridView.IsDisposed)
                    {
                        dataGridViewDoctors.InvokeIfRequired(() => row.Cells[0].Value = rounded);
                    }
                    return;
                }

                if (Uri.TryCreate(doc.AvatarUrl, UriKind.Absolute, out var uri) &&
                    (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
                {
                    using var resp = await _httpClient.GetAsync(doc.AvatarUrl);
                    if (!resp.IsSuccessStatusCode) return;
                    await using var stream = await resp.Content.ReadAsStreamAsync();
                    using var img = Image.FromStream(stream);
                    using var tmp = new Bitmap(img, new Size(60, 60));
                    var rounded = MakeRoundedBitmap(tmp, 60);
                    if (row.DataGridView != null && !row.DataGridView.IsDisposed)
                    {
                        dataGridViewDoctors.InvokeIfRequired(() => row.Cells[0].Value = rounded);
                    }
                    return;
                }
            }
            catch
            {
                // ignore and keep placeholder
            }
        }

        private Bitmap CreatePlaceholder(string name)
        {
            Bitmap placeholder = new Bitmap(60, 60);
            using (Graphics g = Graphics.FromImage(placeholder))
            {
                g.Clear(Color.FromArgb(220, 220, 220));
                using (Font font = new Font("Segoe UI", 20, FontStyle.Bold))
                {
                    string initial = string.IsNullOrEmpty(name) ? "?" : name.Substring(0, 1).ToUpper();
                    SizeF size = g.MeasureString(initial, font);
                    g.DrawString(initial, font, Brushes.Gray, (60 - size.Width) / 2, (60 - size.Height) / 2);
                }
            }

            var rounded = MakeRoundedBitmap(placeholder, 60);
            placeholder.Dispose();
            return rounded;
        }

        // Helper: create a circular cropped bitmap of specified diameter
        private static Bitmap MakeRoundedBitmap(Bitmap src, int diameter)
        {
            var dst = new Bitmap(diameter, diameter);
            dst.MakeTransparent();

            using (Graphics g = Graphics.FromImage(dst))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var path = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    path.AddEllipse(0, 0, diameter - 1, diameter - 1);
                    g.SetClip(path);
                    g.DrawImage(src, new Rectangle(0, 0, diameter, diameter));
                }
            }

            return dst;
        }

        // --- Search Logic ---
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
                textBoxSearch.ForeColor = Color.Gray;
            }
        }

        private void TextBoxSearch_TextChanged(object sender, EventArgs e)
        {
            if (textBoxSearch.Text == "🔍 Tìm kiếm..." || string.IsNullOrWhiteSpace(textBoxSearch.Text))
            {
                filteredDoctors = new List<DoctorDto>(doctors);
            }
            else
            {
                string s = textBoxSearch.Text.ToLower();
                filteredDoctors = doctors.Where(d =>
                    d.FullName.ToLower().Contains(s) ||
                    d.Email.ToLower().Contains(s) ||
                    d.PhoneNumber.Contains(s) ||
                    d.Specialties.Any(sp => sp.ToLower().Contains(s))
                ).ToList();
            }
            // reset to first page when search criteria changes
            _currentPage = 1;
            LoadDoctors();
        }

        // --- Button Events ---
        private void DataGridViewDoctors_SelectionChanged(object sender, EventArgs e)
        {
            bool hasSelection = dataGridViewDoctors.SelectedRows.Count > 0;
         }

        private async void ButtonAdd_Click(object sender, EventArgs e)
        {
            var editorForm = new DoctorEditorForm(specialties, _doctorApiClient);
            if (editorForm.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    this.Cursor = Cursors.WaitCursor;

                    var request = editorForm.BuildRequest();

                    // If avatar is a local file path, upload it first
                    if (!string.IsNullOrWhiteSpace(request.AvatarUrl) && File.Exists(request.AvatarUrl))
                    {
                        try
                        {
                            var uploaded = await _doctorApiClient.UploadFileAsync(request.AvatarUrl);
                            if (!string.IsNullOrWhiteSpace(uploaded))
                            {
                                request.AvatarUrl = uploaded; // server path (e.g. /uploads/avatars/..)
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Không thể upload ảnh: {ex.Message}", "Lỗi upload", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            // allow continue without avatar
                            request.AvatarUrl = null;
                        }
                    }

                    var createdDoctor = await _doctorApiClient.CreateAsync(request);

                    if (createdDoctor != null)
                    {
                        MessageBox.Show("Thêm bác sĩ thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadDataAsync();
                    }
                    else
                    {
                        MessageBox.Show("Không thể thêm bác sĩ. Vui lòng thử lại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi thêm bác sĩ: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    this.Cursor = Cursors.Default;
                }
            }
        }

        private async void ButtonEdit_Click(object sender, EventArgs e)
        {
            if (dataGridViewDoctors.SelectedRows.Count == 0) return;

            Guid id = (Guid)dataGridViewDoctors.SelectedRows[0].Tag;
            var doc = doctors.FirstOrDefault(d => d.Id == id);

            if (doc != null)
            {
                var editorForm = new DoctorEditorForm(specialties, _doctorApiClient, doc);
                if (editorForm.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        this.Cursor = Cursors.WaitCursor;

                        var request = editorForm.BuildRequest();

                        // If avatar is a local file path, upload it first
                        if (!string.IsNullOrWhiteSpace(request.AvatarUrl) && File.Exists(request.AvatarUrl))
                        {
                            try
                            {
                                var uploaded = await _doctorApiClient.UploadFileAsync(request.AvatarUrl);
                                if (!string.IsNullOrWhiteSpace(uploaded))
                                {
                                    request.AvatarUrl = uploaded; // server path
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Không thể upload ảnh: {ex.Message}", "Lỗi upload", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                request.AvatarUrl = null;
                            }
                        }

                        await _doctorApiClient.UpdateAsync(id, request);

                        var profileRequest = new
                        {
                            FirstName = request.FirstName,
                            LastName = request.LastName,
                            PhoneNumber = request.PhoneNumber,
                            DateOfBirth = (DateTime?)null,
                            Description = (string?)null,
                            AvatarUrl = request.AvatarUrl
                        };

                        await _doctorApiClient.UpdateProfileAsync(id, profileRequest);

                        var workingHours = editorForm.GetWorkingHours();
                        if (workingHours.Any())
                        {
                            var hoursList = workingHours.SelectMany(kvp => kvp.Value.Select(slot => new
                            {
                                DayOfWeek = (int)kvp.Key,
                                StartTime = slot.Start.ToString(@"hh\:mm"),
                                EndTime = slot.End.ToString(@"hh\:mm"),
                                Location = (string?)null
                            })).ToList();

                            var hoursRequest = new
                            {
                                LimitAppointments = true,
                                Hours = hoursList
                            };

                            await _doctorApiClient.UpdateWorkingHoursAsync(id, hoursRequest);
                        }

                        var daysOff = editorForm.GetDaysOff();
                        if (daysOff.Any())
                        {
                            // Day off already saved via editor (calls API directly). Nothing else here.
                        }

                        MessageBox.Show("Cập nhật bác sĩ thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        await LoadDataAsync();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi khi cập nhật bác sĩ: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            if (dataGridViewDoctors.SelectedRows.Count == 0) return;

            Guid id = (Guid)dataGridViewDoctors.SelectedRows[0].Tag;
            var doc = doctors.FirstOrDefault(d => d.Id == id);

            if (doc != null)
            {
                DialogResult result = MessageBox.Show(
                    $"Bạn có chắc chắn muốn xóa bác sĩ '{doc.FullName}'?",
                    "Xác nhận xóa",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        this.Cursor = Cursors.WaitCursor;

                        await _doctorApiClient.DeleteAsync(id);

                        MessageBox.Show("Xóa bác sĩ thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadDataAsync();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi khi xóa bác sĩ: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        this.Cursor = Cursors.Default;
                    }
                }
            }
        }

        private void DataGridViewDoctors_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) ButtonEdit_Click(sender, e);
        }
    }
}
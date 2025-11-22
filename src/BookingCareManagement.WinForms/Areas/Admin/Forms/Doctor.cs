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

namespace BookingCareManagement.WinForms.Areas.Admin.Forms
{
    public partial class Doctor : Form
    {
        private List<DoctorDto> doctors = new();
        private List<DoctorDto> filteredDoctors = new();
        private List<SpecialtyDto> specialties = new();

        private readonly AdminDoctorApiClient _doctorApiClient;
        private readonly AdminSpecialtyApiClient _specialtyApiClient;

        public Doctor(AdminDoctorApiClient doctorApiClient, AdminSpecialtyApiClient specialtyApiClient)
        {
            _doctorApiClient = doctorApiClient;
            _specialtyApiClient = specialtyApiClient;

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

        private async void Doctor_Load(object sender, EventArgs e)
        {
            buttonEdit.Enabled = false;
            buttonDelete.Enabled = false;
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
            foreach (var doc in filteredDoctors)
            {
                int rowIndex = dataGridViewDoctors.Rows.Add();
                DataGridViewRow row = dataGridViewDoctors.Rows[rowIndex];

                if (!string.IsNullOrEmpty(doc.AvatarUrl))
                {
                    try
                    {
                        if (System.IO.File.Exists(doc.AvatarUrl))
                        {
                            using (var img = Image.FromFile(doc.AvatarUrl))
                            {
                                row.Cells[0].Value = new Bitmap(img, new Size(60, 60));
                            }
                        }
                        else if (Uri.IsWellFormedUriString(doc.AvatarUrl, UriKind.Absolute))
                        {
                            var uri = new Uri(doc.AvatarUrl);
                            if (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
                            {
                                // Try to download the image synchronously (small images)
                                try
                                {
                                    var req = WebRequest.Create(doc.AvatarUrl);
                                    req.Timeout = 5000; // 5s timeout
                                    using var resp = req.GetResponse();
                                    using var stream = resp.GetResponseStream();
                                    if (stream != null)
                                    {
                                        using var img = Image.FromStream(stream);
                                        row.Cells[0].Value = new Bitmap(img, new Size(60, 60));
                                    }
                                    else
                                    {
                                        row.Cells[0].Value = CreatePlaceholderWithColor(doc.FullName);
                                    }
                                }
                                catch
                                {
                                    // fallback to placeholder if download fails
                                    row.Cells[0].Value = CreatePlaceholderWithColor(doc.FullName);
                                }
                            }
                            else
                            {
                                row.Cells[0].Value = CreatePlaceholder(doc.FullName);
                            }
                        }
                        else
                        {
                            row.Cells[0].Value = CreatePlaceholder(doc.FullName);
                        }
                    }
                    catch
                    {
                        row.Cells[0].Value = CreatePlaceholder(doc.FullName);
                    }
                }
                else
                {
                    row.Cells[0].Value = CreatePlaceholder(doc.FullName);
                }

                row.Cells[1].Value = doc.FullName;
                row.Cells[2].Value = doc.Email;
                row.Cells[3].Value = doc.PhoneNumber;
                row.Cells[4].Value = string.Join(", ", doc.Specialties);
                row.Cells[5].Value = doc.Active ? "Đã duyệt" : "Ngưng hoạt động";

                row.Cells[5].Style.ForeColor = doc.Active ? Color.Green : Color.OrangeRed;
                row.Tag = doc.Id;
            }
            labelCount.Text = $"({filteredDoctors.Count})";
        }

        private Bitmap CreatePlaceholder(string name)
        {
            Bitmap placeholder = new Bitmap(60, 60);
            using (Graphics g = Graphics.FromImage(placeholder))
            {
                g.Clear(Color.FromArgb(220, 220, 220));
                using (Font font = new Font("Segoe UI", 20, FontStyle.Bold))
                {
                    string initial = name.Length > 0 ? name.Substring(0, 1).ToUpper() : "?";
                    SizeF size = g.MeasureString(initial, font);
                    g.DrawString(initial, font, Brushes.Gray, (60 - size.Width) / 2, (60 - size.Height) / 2);
                }
            }
            return placeholder;
        }

        private Bitmap CreatePlaceholderWithColor(string name)
        {
            Bitmap placeholder = new Bitmap(60, 60);
            using (Graphics g = Graphics.FromImage(placeholder))
            {
                g.Clear(Color.FromArgb(33, 150, 243)); // Blue color
                using (Font font = new Font("Segoe UI", 20, FontStyle.Bold))
                {
                    string initial = name.Length > 0 ? name.Substring(0, 1).ToUpper() : "?";
                    SizeF size = g.MeasureString(initial, font);
                    g.DrawString(initial, font, Brushes.White, (60 - size.Width) / 2, (60 - size.Height) / 2);
                }
            }
            return placeholder;
        }

        // --- Search Logic ---
        private void TextBoxSearch_Enter(object sender, EventArgs e)
        {
            if (textBoxSearch.Text == "Tìm kiếm bác sĩ...")
            {
                textBoxSearch.Text = "";
                textBoxSearch.ForeColor = Color.Black;
            }
        }

        private void TextBoxSearch_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxSearch.Text))
            {
                textBoxSearch.Text = "Tìm kiếm bác sĩ...";
                textBoxSearch.ForeColor = Color.Gray;
            }
        }

        private void TextBoxSearch_TextChanged(object sender, EventArgs e)
        {
            if (textBoxSearch.Text == "Tìm kiếm bác sĩ..." || string.IsNullOrWhiteSpace(textBoxSearch.Text))
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
            LoadDoctors();
        }

        // --- Button Events ---
        private void DataGridViewDoctors_SelectionChanged(object sender, EventArgs e)
        {
            bool hasSelection = dataGridViewDoctors.SelectedRows.Count > 0;
            buttonEdit.Enabled = hasSelection;
            buttonDelete.Enabled = hasSelection;
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
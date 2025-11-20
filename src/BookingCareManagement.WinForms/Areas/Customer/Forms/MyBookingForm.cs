using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Globalization;
using BookingCareManagement.WinForms.Areas.Customer.Models;
using BookingCareManagement.WinForms.Shared.State;
using BookingCareManagement.WinForms.Areas.Customer.Services;

namespace BookingCareManagement.WinForms.Areas.Customer.Forms
{
    public partial class MyBookingForm : Form
    {
        // Core dependencies
        private readonly IHttpClientFactory _httpFactory;
        private readonly SessionState _session;
        private readonly CustomerBookingService _bookingService;

        private static readonly System.Collections.Generic.Dictionary<string, string> StatusMap =
            new(System.StringComparer.OrdinalIgnoreCase)
            {
                ["pending"] = "Chờ Duyệt",
                ["approved"] = "Đã Duyệt",
                ["canceled"] = "Đã Hủy",
                ["cancelled"] = "Đã Hủy",
                ["rejected"] = "Bị từ chối",
                ["noshow"] = "Vắng mặt",
            };

        public MyBookingForm(IHttpClientFactory httpFactory, SessionState session)
        {
            _httpFactory = httpFactory;
            _session = session;
            _bookingService = new CustomerBookingService(_httpFactory);

            InitializeComponent();

            // wire events
            this.Load += MyBookingForm_Load;
            this.panelActions.Resize += PanelActions_Resize;

            btnCancel.Paint += (s, e) => DrawRoundedButton(s as Button, e.Graphics, 10, Color.FromArgb(220, 53, 69));
            btnCancel.Click += BtnCancel_Click;

            dgvBookings.CellFormatting += DgvBookings_CellFormatting;
            dgvBookings.CellPainting += DgvBookings_CellPainting;
        }

        private static string MapStatusToVietnamese(string? code, string? label)
        {
            // If server already provided a localized label prefer it
            if (!string.IsNullOrWhiteSpace(label)) return label!;

            if (string.IsNullOrWhiteSpace(code)) return "Đang xử lý";

            // Try direct lookup
            if (StatusMap.TryGetValue(code.Trim(), out var mapped)) return mapped;

            // Try normalized lower without spaces
            var normalized = code.Trim().Replace(" ", string.Empty);
            if (StatusMap.TryGetValue(normalized, out mapped)) return mapped;

            // Fallback: if the code already looks Vietnamese, return it
            if (code.IndexOfAny(new[] { 'Đ', 'đ', 'ư', 'ơ', 'â', 'ă', 'ê', 'ô' }) >= 0)
            {
                return code;
            }

            return "Đang xử lý";
        }

        private static (string Icon, Color Bg, Color Fg) MapStatusBadge(string status)
        {
            if (string.IsNullOrWhiteSpace(status)) return ("⏳", Color.FromArgb(255, 249, 196), Color.FromArgb(181, 134, 0));
            var s = status.Trim().ToLowerInvariant();
            return s switch
            {
                "đã duyệt" or "approved" or "đã xác nhận" => ("✓", Color.FromArgb(220, 255, 228), Color.FromArgb(28, 139, 77)),
                "chờ duyệt" or "pending" or "chờ xác nhận" or "đang xử lý" or "dang xu ly" => ("⏳", Color.FromArgb(255, 249, 196), Color.FromArgb(181, 134, 0)),
                "đã hủy" or "canceled" or "cancelled" => ("✖", Color.FromArgb(255, 235, 238), Color.FromArgb(183, 28, 28)),
                "bị từ chối" or "rejected" => ("✖", Color.FromArgb(255, 235, 238), Color.FromArgb(183, 28, 28)),
                "vắng mặt" or "noshow" => ("⚠", Color.FromArgb(255, 243, 224), Color.FromArgb(124, 77, 32)),
                _ => ("•", Color.Transparent, Color.Black)
            };
        }

        // Sự kiện tô màu chữ Trạng Thái (kept for compatibility)
        private void DgvBookings_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // Price column formatting: ensure currency display
            if (dgvBookings.Columns[e.ColumnIndex].Name == "colPrice" && e.Value != null)
            {
                if (e.Value is decimal dec)
                {
                    e.Value = dec.ToString("C0", CultureInfo.GetCultureInfo("vi-VN"));
                    e.FormattingApplied = true;
                }
                else if (decimal.TryParse(Convert.ToString(e.Value), out var parsed))
                {
                    e.Value = parsed.ToString("C0", CultureInfo.GetCultureInfo("vi-VN"));
                    e.FormattingApplied = true;
                }
            }
        }

        private void DgvBookings_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0) return; // header
            if (dgvBookings.Columns[e.ColumnIndex].Name != "colStatus") return;

            e.Handled = true;
            var status = Convert.ToString(dgvBookings.Rows[e.RowIndex].Cells[e.ColumnIndex].Value) ?? string.Empty;
            var (icon, bg, fg) = MapStatusBadge(status);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // fill full cell background
            using (var bgBrush = new SolidBrush(bg))
            {
                g.FillRectangle(bgBrush, e.CellBounds);
            }

            // draw border parts (keep grid look)
            e.Paint(e.CellBounds, DataGridViewPaintParts.Border);

            // draw icon + status centered horizontally
            using (var iconFont = new Font("Segoe UI Symbol", 9, FontStyle.Regular))
            using (var textFont = new Font("Segoe UI", 9, FontStyle.Bold))
            using (var fgBrush = new SolidBrush(fg))
            {
                var iconSize = g.MeasureString(icon, iconFont);
                var textSize = g.MeasureString(status, textFont);
                var totalWidth = iconSize.Width + textSize.Width + 6;
                var x = e.CellBounds.Left + (e.CellBounds.Width - (int)totalWidth) / 2;
                var y = e.CellBounds.Top + (e.CellBounds.Height - (int)textSize.Height) / 2;
                g.DrawString(icon, iconFont, fgBrush, x, y);
                g.DrawString(" " + status, textFont, fgBrush, x + iconSize.Width, y);
            }
        }

        private void UpdateButtonPosition()
        {
            // Đặt nút cách lề phải 30px
            btnCancel.Location = new Point(panelActions.Width - btnCancel.Width - 30, 10);
        }

        private async void MyBookingForm_Load(object sender, EventArgs e)
        {
            UpdateButtonPosition();
            await LoadBookingsAsync();
        }

        private void PanelActions_Resize(object sender, EventArgs e)
        {
            UpdateButtonPosition();
        }

        private async Task LoadBookingsAsync()
        {
            dgvBookings.Rows.Clear();

            try
            {
                var items = await _bookingService.GetMyBookingsAsync();
                if (items == null || items.Length == 0) return;

                foreach (var it in items)
                {
                    var displayStatus = MapStatusToVietnamese(it.Status, it.StatusLabel);
                    var priceDisplay = it.Price; // DataGridView formatting handles currency via CellFormatting

                    // Add values in exact order of columns: Date, Time, Doctor, Specialty, Status, Price
                    var rowIndex = dgvBookings.Rows.Add(it.DateText, it.TimeText, it.DoctorName, it.SpecialtyName, displayStatus, priceDisplay);
                    var row = dgvBookings.Rows[rowIndex];
                    row.Tag = it.Id; // store appointment id for operations
                }

                if (dgvBookings.Rows.Count > 0) dgvBookings.Rows[0].Selected = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("LoadBookingsAsync error: " + ex);
                LoadDummyData();
            }
        }

        private async void BtnCancel_Click(object sender, EventArgs e)
        {
            if (dgvBookings.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn lịch cần hủy!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var status = dgvBookings.CurrentRow.Cells["colStatus"].Value?.ToString()?.Trim() ?? string.Empty;
            // If already cancelled, block
            if (status.Equals("Đã Hủy", StringComparison.OrdinalIgnoreCase) || status.IndexOf("hủy", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                MessageBox.Show("Lịch hẹn này đã được hủy trước đó.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var idObj = dgvBookings.CurrentRow.Tag;
            if (idObj is not Guid appointmentId)
            {
                // No appointment id: fallback to old behavior (remove row)
                var date = dgvBookings.CurrentRow.Cells["colDate"].Value?.ToString();
                var result = MessageBox.Show($"Bạn muốn hủy lịch ngày {date}?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    dgvBookings.Rows.Remove(dgvBookings.CurrentRow);
                }
                return;
            }

            var dateText = dgvBookings.CurrentRow.Cells["colDate"].Value?.ToString();
            var confirm = MessageBox.Show($"Bạn muốn hủy lịch ngày {dateText}?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            try
            {
                var success = await _bookingService.CancelBookingAsync(appointmentId);
                if (!success)
                {
                    MessageBox.Show("Không thể hủy lịch", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                MessageBox.Show("Đã hủy lịch hẹn thành công.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadBookingsAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("CancelBooking error: " + ex);
                MessageBox.Show("Không thể hủy lịch, vui lòng thử lại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadDummyData()
        {
            dgvBookings.Rows.Clear();
            dgvBookings.Rows.Add("Thứ Hai, 20/11/2023", "09:00 - 10:00", "BS. Nguyễn Văn A", "Nha Khoa", "Đã Duyệt", 200000);
            dgvBookings.Rows.Add("Thứ Tư, 22/11/2023", "14:30 - 16:30", "BS. Le Wilson", "Phẫu thuật", "Chờ Duyệt", 1000000);
            dgvBookings.Rows.Add("Chủ Nhật, 26/11/2023", "08:00 - 09:00", "BS. Trần B", "Niềng răng", "Đã Duyệt", 500000);
            dgvBookings.Rows.Add("Thứ Ba, 28/11/2023", "10:00 - 11:00", "BS. Sarah", "Nhổ răng khôn", "Đã Duyệt", 400000);

            if (dgvBookings.Rows.Count > 0) dgvBookings.Rows[0].Selected = true;
        }

        private void DrawRoundedButton(Button btn, Graphics g, int radius, Color backColor)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle r = new Rectangle(0, 0, btn.Width, btn.Height);
            g.Clear(this.panelActions.BackColor);

            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddArc(r.X, r.Y, radius, radius, 180, 90);
                path.AddArc(r.Right - radius, r.Y, radius, radius, 270, 90);
                path.AddArc(r.Right - radius, r.Bottom - radius, radius, radius, 0, 90);
                path.AddArc(r.X, r.Bottom - radius, radius, radius, 90, 90);
                path.CloseFigure();

                using (SolidBrush brush = new SolidBrush(backColor))
                {
                    g.FillPath(brush, path);
                }
                TextRenderer.DrawText(g, btn.Text, btn.Font, r, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using BookingCareManagement.WinForms.Areas.Admin.Services;
using BookingCareManagement.WinForms.Shared.Models.Dtos;
using Microsoft.Extensions.DependencyInjection;

namespace BookingCareManagement.WinForms.Areas.Customer.Forms
{
    public partial class Service : Form
    {
        private List<ServiceItem> services = new List<ServiceItem>();
        private IReadOnlyList<CustomerSpecialtyDto>? _specialtyDtos;
        private readonly AdminSpecialtyApiClient? _specialtyApiClient;
        private readonly IServiceProvider? _serviceProvider;

        // Parameterless constructor (used by designer)
        public Service()
        {
            InitializeComponent();

            // Cấu hình Form
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized; // Mở lên là phóng to luôn (tùy chọn)

            // Thiết lập sự kiện
            this.Load += Service_Load;

            // [QUAN TRỌNG] Thêm sự kiện Resize để căn giữa khi phóng to/thu nhỏ
            this.Resize += Service_Resize;

            // NOTE: do not attach textBoxSearch handlers here to avoid TextChanged firing during construction
        }

        // Constructor dùng khi có AdminSpecialtyApiClient (ví dụ gọi từ DI)
        public Service(AdminSpecialtyApiClient specialtyApiClient, IServiceProvider serviceProvider) : this()
        {
            _specialtyApiClient = specialtyApiClient;
            _serviceProvider = serviceProvider;
        }

        private async void Service_Load(object sender, EventArgs e)
        {
            try
            {
                try { System.IO.File.AppendAllText("debug_winforms.log", $"[{DateTime.Now:O}] Service_Load start. SpecialtyApiClient is {(_specialtyApiClient != null ? "present" : "null")}.\n"); } catch {}

                if (_specialtyApiClient != null)
                {
                    await LoadSpecialtiesAsync();
                }
                else
                {
                    // Khởi tạo dữ liệu mẫu nếu không có API client
                    InitializeSampleData();
                }

                // Attach search box events and placeholder (attach after InitializeComponent to avoid constructor-time TextChanged)
                if (this.textBoxSearch != null)
                {
                    // Ensure no duplicate handlers
                    this.textBoxSearch.Enter -= TextBoxSearch_Enter;
                    this.textBoxSearch.Leave -= TextBoxSearch_Leave;
                    this.textBoxSearch.TextChanged -= TextBoxSearch_TextChanged;

                    this.textBoxSearch.Enter += TextBoxSearch_Enter;
                    this.textBoxSearch.Leave += TextBoxSearch_Leave;
                    this.textBoxSearch.TextChanged += TextBoxSearch_TextChanged;

                    // Set Vietnamese placeholder
                    this.textBoxSearch.Text = "Tìm kiếm dịch vụ...";
                    this.textBoxSearch.ForeColor = Color.Gray;
                }

                // Hiển thị tất cả dịch vụ
                DisplayServices(services ?? new List<ServiceItem>());

                // Gọi hàm căn chỉnh lần đầu
                CenterServiceCards();

                try { System.IO.File.AppendAllText("debug_winforms.log", $"[{DateTime.Now:O}] Service_Load completed successfully. Services count: {(services?.Count ?? 0)}\n"); } catch {}
            }
            catch (Exception ex)
            {
                try { System.IO.File.AppendAllText("debug_winforms.log", $"[{DateTime.Now:O}] Service_Load exception: {ex}\n"); } catch {}
                MessageBox.Show(this, $"Lỗi khi mở trang Dịch vụ:\n{ex}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Sự kiện khi thay đổi kích thước Form
        private void Service_Resize(object sender, EventArgs e)
        {
            CenterServiceCards();
        }

        // Hàm tính toán để căn giữa nội dung
        private void CenterServiceCards()
        {
            int cardFullWidth = 370;
            int panelWidth = flowLayoutPanelServices?.ClientSize.Width ?? 0;
            int columns = Math.Max(1, panelWidth / cardFullWidth);
            int totalContentWidth = columns * cardFullWidth;
            int remainingSpace = panelWidth - totalContentWidth;
            int paddingLeft = Math.Max(0, remainingSpace / 2);
            if (flowLayoutPanelServices != null)
                flowLayoutPanelServices.Padding = new Padding(paddingLeft, 20, 0, 20);
        }

        private async Task LoadSpecialtiesAsync()
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
                // Use customer-facing API endpoint to get duration and doctors
                var dtos = await _specialtyApiClient!.GetCustomerAllAsync();
                _specialtyDtos = dtos;

                // Map specialties -> services, filter active ones (assume presence means active)
                services = dtos
                    .Select(s => new ServiceItem
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Price = s.Price ?? 0m,
                        Duration = FormatDuration(s.DurationMinutes),
                        Capacity = s.Doctors?.Count ?? 0,
                        ImagePath = string.IsNullOrWhiteSpace(s.ImageUrl) ? string.Empty : s.ImageUrl!,
                        Description = string.IsNullOrWhiteSpace(s.Description) ? string.Empty : s.Description!
                    })
                    .ToList();

                if (services.Count == 0)
                {
                    // fallback sample for visual
                    InitializeSampleData();
                }
            }
            catch (Exception ex)
            {
                try { System.IO.File.AppendAllText("debug_winforms.log", $"[{DateTime.Now:O}] LoadSpecialtiesAsync exception: {ex}\n"); } catch {}
                MessageBox.Show($"Không thể tải chuyên khoa từ API: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                InitializeSampleData();
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void InitializeSampleData()
        {
            services = new List<ServiceItem>();
            // Tạo 8 item để test hiển thị đầy màn hình
            for (int i = 1; i <= 8; i++)
            {
                services.Add(new ServiceItem
                {
                    Id = Guid.NewGuid(),
                    Name = i % 2 == 0 ? "Nha Khoa Thẩm Mỹ " + i : "Khám Tổng Quát " + i,
                    Price = i * 150000.00m,
                    Duration = i + "h",
                    Capacity = 1,
                    ImagePath = "",
                    Description = "Mô tả mẫu cho chuyên khoa."
                });
            }
        }

        private void DisplayServices(List<ServiceItem> servicesToDisplay)
        {
            if (flowLayoutPanelServices == null)
                return;

            flowLayoutPanelServices.Controls.Clear();
            flowLayoutPanelServices.SuspendLayout();

            foreach (var service in servicesToDisplay)
            {
                Panel serviceCard = CreateServiceCard(service);
                flowLayoutPanelServices.Controls.Add(serviceCard);
            }

            flowLayoutPanelServices.ResumeLayout();

            // Căn chỉnh lại sau khi render xong
            CenterServiceCards();
        }

        private Panel CreateServiceCard(ServiceItem service)
        {
            int cardWidth = 350;
            int cardHeight = 340; // reduced to bring bottom border closer to buttons

            // Panel chính cho card
            Panel cardPanel = new Panel
            {
                Size = new Size(cardWidth, cardHeight),
                BackColor = Color.White,
                // Margin: Left/Right = 10 -> Tổng chiều rộng chiếm dụng là 370
                Margin = new Padding(10, 10, 10, 20),
                BorderStyle = BorderStyle.FixedSingle,
                Tag = service
            };

            int pictureHeight = 160;
            // PictureBox
            PictureBox pictureBox = new PictureBox
            {
                Size = new Size(cardWidth, pictureHeight),
                Location = new Point(0, 0),
                // We'll draw a cropped/filled image into the PictureBox.Image, so use Normal
                SizeMode = PictureBoxSizeMode.Normal,
                BackColor = Color.FromArgb(245, 245, 245)
            };

            if (!string.IsNullOrEmpty(service.ImagePath))
            {
                try
                {
                    // Load image from URL or local path safely
                    Image? loaded = null;
                    if (Uri.IsWellFormedUriString(service.ImagePath, UriKind.Absolute))
                    {
                        var req = System.Net.WebRequest.Create(service.ImagePath);
                        using var resp = req.GetResponse();
                        using var stream = resp.GetResponseStream();
                        loaded = Image.FromStream(stream!);
                    }
                    else if (System.IO.File.Exists(service.ImagePath))
                    {
                        loaded = Image.FromFile(service.ImagePath);
                    }

                    if (loaded is not null)
                    {
                        // Create a cover/cropped image that fills the picture box (center-crop)
                        var cover = CreateCoverImage(loaded, pictureBox.Width, pictureBox.Height);
                        pictureBox.Image = cover;

                        // Dispose original if different
                        if (!object.ReferenceEquals(cover, loaded))
                        {
                            loaded.Dispose();
                        }
                    }
                }
                catch
                {
                    // ignore image load errors
                }
            }

            // Label giá tiền
            Label labelPrice = new Label
            {
                Text = $"₫{service.Price:N0}",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 33, 33),
                BackColor = Color.White,
                AutoSize = true,
                Padding = new Padding(5),
            };
            labelPrice.Location = new Point(cardWidth - 110, 10);
            pictureBox.Controls.Add(labelPrice);

            // Label tên
            Label labelName = new Label
            {
                Text = service.Name,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 33, 33),
                Location = new Point(10, pictureHeight + 10),
                Size = new Size(cardWidth - 20, 30)
            };

            // Label thời gian
            Label labelDuration = new Label
            {
                Text = $"Thời gian: {service.Duration}",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                Location = new Point(10, pictureHeight + 45),
                AutoSize = true
            };

            // Label sức chứa
            Label labelCapacity = new Label
            {
                Text = $"Bác sĩ: {service.Capacity}",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                Location = new Point(160, pictureHeight + 45),
                AutoSize = true
            };

            // Re-add buttons visually but keep functionality disabled (show notification)
            int btnWidth = 155;
            int btnHeight = 40;
            int btnY = cardHeight - 70; // position buttons relative to card bottom

            Button buttonLearnMore = new Button
            {
                Text = "Chi tiết",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(66, 66, 66),
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(btnWidth, btnHeight),
                Location = new Point(10, btnY),
                Cursor = Cursors.Hand,
                Tag = service
            };
            buttonLearnMore.FlatAppearance.BorderColor = Color.FromArgb(220, 220, 220);
            buttonLearnMore.Click += (s, e) =>
            {
                try
                {
                    // Try to find DTO for this service if available
                    CustomerSpecialtyDto? dto = null;
                    if (_specialtyDtos != null)
                    {
                        dto = _specialtyDtos.FirstOrDefault(x => x.Id == service.Id);
                    }

                    var details = new SpecialtyDetailsForm(service, dto);

                    // If hosted inside MainForm shell, show as modal owned by shell
                    var mainShell = System.Windows.Forms.Application.OpenForms.OfType<global::BookingCareManagement.WinForms.MainForm>().FirstOrDefault();
                    if (mainShell != null)
                    {
                        details.ShowDialog(mainShell);
                    }
                    else
                    {
                        details.ShowDialog(this);
                    }
                }
                catch (Exception ex)
                {
                    try { System.IO.File.AppendAllText("debug_winforms.log", $"[{DateTime.Now:O}] Open SpecialtyDetails exception: {ex}\n"); } catch {}
                    MessageBox.Show("Không thể mở chi tiết chuyên khoa.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            Button buttonBookNow = new Button
            {
                Text = "Đặt ngay",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(0, 123, 255),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(btnWidth, btnHeight),
                Location = new Point(10 + btnWidth + 10, btnY),
                Cursor = Cursors.Hand,
                Tag = service
            };
            buttonBookNow.FlatAppearance.BorderSize = 0;
            buttonBookNow.Click += async (s, e) =>
            {
                try
                {
                    if (_serviceProvider is null)
                    {
                        MessageBox.Show("Không thể mở form đặt lịch: DI provider chưa khởi tạo.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // If this Service form is hosted inside MainForm, ask MainForm to show Bookings in the content area
                    // Prefer resolving MainForm from DI so we target the shell instance created by the host.
                    // Prefer locating the already-displayed MainForm instance in Application.OpenForms (the actual shell).
                    // Only fallback to DI if no shell instance is found (DI would create a new instance otherwise).
                    var main = System.Windows.Forms.Application.OpenForms
                        .OfType<global::BookingCareManagement.WinForms.MainForm>()
                        .FirstOrDefault()
                        ?? _serviceProvider?.GetService<global::BookingCareManagement.WinForms.MainForm>();
                     if (main != null)
                     {
                         // Use BeginInvoke so we don't close the current child (Service) while still in its event handler
                         main.BeginInvoke(new Action(() =>
                         {
                             try
                             {
                                // Fire-and-forget the async helper on the UI thread
                                _ = main.ShowBookingsForSpecialtyAsync(service.Id, service.Name, service.Price);
                             }
                             catch (Exception ex)
                             {
                                 try { System.IO.File.AppendAllText("debug_winforms.log", $"[{DateTime.Now:O}] BeginInvoke ShowBookings exception: {ex}\n"); } catch {}
                             }
                         }));

                         return;
                     }

                    // Fallback: create and show modal Bookings if not hosted in shell
                    var bookings = _serviceProvider.GetService<Bookings>();
                    if (bookings is null)
                    {
                        MessageBox.Show("Không tìm thấy form đặt lịch.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Pre-initialize bookings (load data) BEFORE showing modal to avoid blank flicker
                    await bookings.OpenForSpecialtyAsync(service.Id, service.Name, service.Price);
                    bookings.ShowDialog(this);
                }
                catch (Exception ex)
                {
                    try { System.IO.File.AppendAllText("debug_winforms.log", $"[{DateTime.Now:O}] Open bookings exception: {ex}\n"); } catch {}
                    MessageBox.Show($"Không thể mở form đặt lịch:\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            // Add controls
            cardPanel.Controls.Add(pictureBox);
            cardPanel.Controls.Add(labelName);
            cardPanel.Controls.Add(labelDuration);
            cardPanel.Controls.Add(labelCapacity);
            cardPanel.Controls.Add(buttonLearnMore);
            cardPanel.Controls.Add(buttonBookNow);

            // Log card creation
            try { System.IO.File.AppendAllText("debug_winforms.log", $"[{DateTime.Now:O}] Service card created: {service.Name}\n"); } catch {}

            return cardPanel;
        }

        // --- Event Handlers ---

        private void ButtonLearnMore_Click(ServiceItem service)
        {
            MessageBox.Show($"Chi tiết dịch vụ: {service.Name}\nGiá: ₫{service.Price:N0}", "Thông tin");
        }

        private void ButtonBookNow_Click(ServiceItem service)
        {
            MessageBox.Show($"Bạn chọn đặt: {service.Name}", "Xác nhận");
        }

        private void TextBoxSearch_Enter(object sender, EventArgs e)
        {
            if (textBoxSearch.Text == "Tìm kiếm dịch vụ...")
            {
                textBoxSearch.Text = "";
                textBoxSearch.ForeColor = Color.Black;
            }
        }

        private void TextBoxSearch_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxSearch.Text))
            {
                textBoxSearch.Text = "Tìm kiếm dịch vụ...";
                textBoxSearch.ForeColor = Color.Gray;
            }
        }

        private void TextBoxSearch_TextChanged(object sender, EventArgs e)
        {
            // if services not loaded yet, ignore
            if (services == null || services.Count == 0)
            {
                // if placeholder text or empty, nothing to display
                if (string.IsNullOrWhiteSpace(textBoxSearch?.Text) || textBoxSearch?.Text == "Tìm kiếm dịch vụ...")
                    return;
            }

            if (textBoxSearch.Text == "Tìm kiếm dịch vụ..." || string.IsNullOrWhiteSpace(textBoxSearch.Text))
            {
                DisplayServices(services);
                return;
            }

            string searchText = textBoxSearch.Text.ToLower();
            List<ServiceItem> filteredServices = services.FindAll(s =>
                s.Name.ToLower().Contains(searchText));

            DisplayServices(filteredServices);
        }

        private static string FormatDuration(int? minutes)
        {
            if (!minutes.HasValue || minutes.Value <= 0)
            {
                return "30 phút";
            }

            var m = minutes.Value;
            if (m % 60 == 0)
            {
                var hours = m / 60;
                return hours == 1 ? "1 giờ" : $"{hours} giờ";
            }

            return $"{m} phút";
        }

        // Helper: create a cover (center-crop) image to fill target size preserving aspect ratio
        private static Image CreateCoverImage(Image src, int targetWidth, int targetHeight)
        {
            if (src == null)
                throw new ArgumentNullException(nameof(src));

            var dest = new Bitmap(targetWidth, targetHeight);
            using (var g = Graphics.FromImage(dest))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                // fill background with same color as picture box
                g.Clear(Color.FromArgb(245, 245, 245));

                double srcRatio = (double)src.Width / src.Height;
                double dstRatio = (double)targetWidth / targetHeight;

                Rectangle srcRect;
                if (srcRatio > dstRatio)
                {
                    // source is wider, crop left/right
                    int srcHeight = src.Height;
                    int srcWidth = (int)(srcHeight * dstRatio);
                    int srcX = (src.Width - srcWidth) / 2;
                    srcRect = new Rectangle(srcX, 0, srcWidth, srcHeight);
                }
                else
                {
                    // source is taller, crop top/bottom
                    int srcWidth = src.Width;
                    int srcHeight = (int)(srcWidth / dstRatio);
                    int srcY = (src.Height - srcHeight) / 2;
                    srcRect = new Rectangle(0, srcY, srcWidth, srcHeight);
                }

                var destRect = new Rectangle(0, 0, targetWidth, targetHeight);
                g.DrawImage(src, destRect, srcRect, GraphicsUnit.Pixel);
            }

            return dest;
        }
    }

    public class ServiceItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Duration { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public string ImagePath { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
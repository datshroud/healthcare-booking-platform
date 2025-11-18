using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace BookingCareManagement.WinForms.Areas.Customer.Forms
{
    public partial class Service : Form
    {
        private List<ServiceItem> services;

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

            this.textBoxSearch.Enter += TextBoxSearch_Enter;
            this.textBoxSearch.Leave += TextBoxSearch_Leave;
            this.textBoxSearch.TextChanged += TextBoxSearch_TextChanged;
        }

        private void Service_Load(object sender, EventArgs e)
        {
            // Khởi tạo dữ liệu mẫu
            InitializeSampleData();

            // Hiển thị tất cả dịch vụ
            DisplayServices(services);

            // Gọi hàm căn chỉnh lần đầu
            CenterServiceCards();
        }

        // Sự kiện khi thay đổi kích thước Form
        private void Service_Resize(object sender, EventArgs e)
        {
            CenterServiceCards();
        }

        // Hàm tính toán để căn giữa nội dung
        private void CenterServiceCards()
        {
            // Kích thước Card (350) + Margin Trái (10) + Margin Phải (10) = 370
            int cardFullWidth = 370;
            int panelWidth = flowLayoutPanelServices.ClientSize.Width;

            // Tính xem 1 hàng chứa được tối đa bao nhiêu card
            // Math.Max(1, ...) để đảm bảo ít nhất là 1 cột
            int columns = Math.Max(1, panelWidth / cardFullWidth);

            // Tính tổng chiều rộng thực tế của các card trong 1 hàng
            int totalContentWidth = columns * cardFullWidth;

            // Tính khoảng trống thừa ra
            int remainingSpace = panelWidth - totalContentWidth;

            // Chia đôi khoảng trống để làm Padding trái (để căn giữa)
            int paddingLeft = Math.Max(0, remainingSpace / 2);

            // Set Padding cho FlowLayoutPanel (Trên dưới giữ nguyên là 20)
            flowLayoutPanelServices.Padding = new Padding(paddingLeft, 20, 0, 20);
        }

        private void InitializeSampleData()
        {
            services = new List<ServiceItem>();
            // Tạo 8 item để test hiển thị đầy màn hình
            for (int i = 1; i <= 8; i++)
            {
                services.Add(new ServiceItem
                {
                    Name = i % 2 == 0 ? "Nha Khoa Thẩm Mỹ " + i : "Khám Tổng Quát " + i,
                    Price = i * 150000.00m,
                    Duration = i + "h",
                    Capacity = 1,
                    ImagePath = ""
                });
            }
        }

        private void DisplayServices(List<ServiceItem> servicesToDisplay)
        {
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
            int cardHeight = 420;

            // Panel chính cho card
            Panel cardPanel = new Panel
            {
                Size = new Size(cardWidth, cardHeight),
                BackColor = Color.White,
                // Margin: Left/Right = 10 -> Tổng chiều rộng chiếm dụng là 370
                Margin = new Padding(10, 10, 10, 20),
                BorderStyle = BorderStyle.FixedSingle
            };

            // PictureBox
            PictureBox pictureBox = new PictureBox
            {
                Size = new Size(cardWidth, 200),
                Location = new Point(0, 0),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(245, 245, 245)
            };

            if (!string.IsNullOrEmpty(service.ImagePath) && System.IO.File.Exists(service.ImagePath))
            {
                pictureBox.Image = Image.FromFile(service.ImagePath);
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
                Location = new Point(10, 215),
                Size = new Size(cardWidth - 20, 30)
            };

            // Label thời gian
            Label labelDuration = new Label
            {
                Text = $"Thời gian: {service.Duration}",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                Location = new Point(10, 250),
                AutoSize = true
            };

            // Label sức chứa
            Label labelCapacity = new Label
            {
                Text = $"Sức chứa: {service.Capacity}",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                Location = new Point(160, 250),
                AutoSize = true
            };

            // Nút bấm
            int btnWidth = 155;
            int btnHeight = 40;
            int btnY = 360;

            Button buttonLearnMore = new Button
            {
                Text = "Chi tiết",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(66, 66, 66),
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(btnWidth, btnHeight),
                Location = new Point(10, btnY),
                Cursor = Cursors.Hand
            };
            buttonLearnMore.FlatAppearance.BorderColor = Color.FromArgb(220, 220, 220);
            buttonLearnMore.Click += (s, e) => ButtonLearnMore_Click(service);

            Button buttonBookNow = new Button
            {
                Text = "Đặt ngay",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(0, 123, 255),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(btnWidth, btnHeight),
                Location = new Point(10 + btnWidth + 10, btnY),
                Cursor = Cursors.Hand
            };
            buttonBookNow.FlatAppearance.BorderSize = 0;
            buttonBookNow.Click += (s, e) => ButtonBookNow_Click(service);

            // Add controls
            cardPanel.Controls.Add(pictureBox);
            cardPanel.Controls.Add(labelName);
            cardPanel.Controls.Add(labelDuration);
            cardPanel.Controls.Add(labelCapacity);
            cardPanel.Controls.Add(buttonLearnMore);
            cardPanel.Controls.Add(buttonBookNow);

            return cardPanel;
        }

        // --- Event Handlers ---

        private void ButtonLearnMore_Click(ServiceItem service)
        {
            MessageBox.Show($"Chi tiết dịch vụ: {service.Name}\nGiá: ₫{service.Price:N0}", "Thông tin");
        }

        private void ButtonBookNow_Click(ServiceItem service)
        {
            // Mở form đặt lịch (Appointment) và truyền dữ liệu nếu cần
            // Appointment appointmentForm = new Appointment(); 
            // appointmentForm.ShowDialog(); 
            MessageBox.Show($"Bạn chọn đặt: {service.Name}", "Xác nhận");
        }

        private void TextBoxSearch_Enter(object sender, EventArgs e)
        {
            if (textBoxSearch.Text == "Search services")
            {
                textBoxSearch.Text = "";
                textBoxSearch.ForeColor = Color.Black;
            }
        }

        private void TextBoxSearch_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxSearch.Text))
            {
                textBoxSearch.Text = "Search services";
                textBoxSearch.ForeColor = Color.Gray;
            }
        }

        private void TextBoxSearch_TextChanged(object sender, EventArgs e)
        {
            if (textBoxSearch.Text == "Search services" || string.IsNullOrWhiteSpace(textBoxSearch.Text))
            {
                DisplayServices(services);
                return;
            }

            string searchText = textBoxSearch.Text.ToLower();
            List<ServiceItem> filteredServices = services.FindAll(s =>
                s.Name.ToLower().Contains(searchText));

            DisplayServices(filteredServices);
        }
    }

    public class ServiceItem
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Duration { get; set; }
        public int Capacity { get; set; }
        public string ImagePath { get; set; }
    }
}
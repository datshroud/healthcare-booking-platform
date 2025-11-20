using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BookingCareManagement.WinForms.Shared.Models.Dtos;

namespace BookingCareManagement.WinForms.Areas.Customer.Forms
{
    public class SpecialtyDetailsForm : Form
    {
        public SpecialtyDetailsForm(ServiceItem service, CustomerSpecialtyDto? dto = null)
        {
            this.Text = service?.Name ?? "Chi tiết";
            this.StartPosition = FormStartPosition.CenterParent;
            this.ClientSize = new Size(700, 520);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var header = new Label
            {
                Text = service?.Name ?? string.Empty,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Top,
                Height = 48,
                Padding = new Padding(12, 8, 12, 0)
            };

            var panelMain = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(12)
            };

            // Use Zoom so the image fits the box while preserving aspect ratio
            var picture = new PictureBox
            {
                Size = new Size(260, 160),
                Location = new Point(12, 12),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(245,245,245)
            };

            if (!string.IsNullOrEmpty(service?.ImagePath))
            {
                try
                {
                    if (Uri.IsWellFormedUriString(service.ImagePath, UriKind.Absolute))
                    {
                        var req = System.Net.WebRequest.Create(service.ImagePath);
                        using var resp = req.GetResponse();
                        using var stream = resp.GetResponseStream();
                        picture.Image = Image.FromStream(stream!);
                    }
                    else if (System.IO.File.Exists(service.ImagePath))
                    {
                        picture.Image = Image.FromFile(service.ImagePath);
                    }
                }
                catch { }
            }

            var lblPrice = new Label
            {
                // Use ASCII-friendly text to avoid encoding issues in some environments
                Text = $"Gia: {service.Price:N0} VND",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(33,33,33),
                AutoSize = true,
                Location = new Point(290, 12)
            };

            var lblDuration = new Label
            {
                Text = $"Thoi gian: {service.Duration}",
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.Gray,
                AutoSize = true,
                Location = new Point(290, 40)
            };

            var lblCapacity = new Label
            {
                Text = $"So bac si: {service.Capacity}",
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.Gray,
                AutoSize = true,
                Location = new Point(290, 62)
            };

            var descLabel = new Label
            {
                Text = "Mo ta chuyen khoa:",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Location = new Point(12, 190),
                AutoSize = true
            };

            var txtDesc = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.White,
                Font = new Font("Segoe UI", 9F),
                Location = new Point(12, 215),
                Size = new Size(this.ClientSize.Width - 36, 140),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            txtDesc.Text = dto?.Description ?? service?.Description ?? "Khong co mo ta";

            var doctorsLabel = new Label
            {
                Text = "Danh sach bac si:",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Location = new Point(12, 370),
                AutoSize = true
            };

            // Use a FlowLayoutPanel to present avatar + name rows
            var doctorsPanel = new FlowLayoutPanel
            {
                Location = new Point(12, 395),
                Size = new Size(this.ClientSize.Width - 36, 100),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AutoScroll = true,
                FlowDirection = FlowDirection.LeftToRight
            };

            if (dto?.Doctors != null && dto.Doctors.Count > 0)
            {
                foreach (var d in dto.Doctors)
                {
                    // Access strongly-typed properties
                    string name = d?.FullName ?? "Bac si";
                    string avatarUrl = d?.AvatarUrl ?? string.Empty;

                    var item = new Panel
                    {
                        Size = new Size(180, 80),
                        Margin = new Padding(6)
                    };

                    var avatar = new PictureBox
                    {
                        Size = new Size(56, 56),
                        Location = new Point(0, 0),
                        SizeMode = PictureBoxSizeMode.Zoom,
                        BackColor = Color.FromArgb(245, 245, 245)
                    };

                    bool imageSet = false;
                    if (!string.IsNullOrEmpty(avatarUrl))
                    {
                        try
                        {
                            if (Uri.IsWellFormedUriString(avatarUrl, UriKind.Absolute))
                            {
                                var req = System.Net.WebRequest.Create(avatarUrl);
                                using var resp = req.GetResponse();
                                using var stream = resp.GetResponseStream();
                                avatar.Image = Image.FromStream(stream!);
                                imageSet = true;
                            }
                            else if (System.IO.File.Exists(avatarUrl))
                            {
                                avatar.Image = Image.FromFile(avatarUrl);
                                imageSet = true;
                            }
                        }
                        catch { imageSet = false; }
                    }

                    if (!imageSet)
                    {
                        // Generate a simple placeholder avatar with the initial
                        avatar.Image = CreatePlaceholderBitmap(name, avatar.Size);
                    }

                    var lbl = new Label
                    {
                        Text = name,
                        Location = new Point(66, 10),
                        AutoSize = false,
                        Size = new Size(100, 40),
                        Font = new Font("Segoe UI", 9F, FontStyle.Regular)
                    };

                    item.Controls.Add(avatar);
                    item.Controls.Add(lbl);
                    doctorsPanel.Controls.Add(item);
                }
            }
            else
            {
                var empty = new Label
                {
                    Text = "Khong co bac si",
                    AutoSize = true,
                    Location = new Point(12, 400)
                };
                doctorsPanel.Controls.Add(empty);
            }

            var btnClose = new Button
            {
                Text = "Dong",
                BackColor = Color.FromArgb(59,130,246),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(100, 36),
                Location = new Point(this.ClientSize.Width - 116, this.ClientSize.Height - 48),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => this.Close();

            // Add to panelMain
            panelMain.Controls.Add(picture);
            panelMain.Controls.Add(lblPrice);
            panelMain.Controls.Add(lblDuration);
            panelMain.Controls.Add(lblCapacity);
            panelMain.Controls.Add(descLabel);
            panelMain.Controls.Add(txtDesc);
            panelMain.Controls.Add(doctorsLabel);
            panelMain.Controls.Add(doctorsPanel);
            panelMain.Controls.Add(btnClose);

            this.Controls.Add(panelMain);
            this.Controls.Add(header);
        }

        private Bitmap CreatePlaceholderBitmap(string name, Size size)
        {
            int width = Math.Max(1, size.Width);
            int height = Math.Max(1, size.Height);
            Bitmap bmp = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                // pick a background color based on name hash
                int hash = name?.GetHashCode() ?? 0;
                Random rnd = new Random(hash);
                Color bg = Color.FromArgb(255, 170 + rnd.Next(0, 60), 170 + rnd.Next(0, 60), 170 + rnd.Next(0, 60));
                g.Clear(bg);

                string initial = "?";
                if (!string.IsNullOrEmpty(name))
                {
                    initial = name.Trim().Substring(0, 1).ToUpper();
                }

                using (Font font = new Font("Segoe UI", Math.Max(10, width / 2), FontStyle.Bold))
                using (Brush brush = new SolidBrush(Color.White))
                {
                    SizeF textSize = g.MeasureString(initial, font);
                    g.DrawString(initial, font, brush, (width - textSize.Width) / 2f, (height - textSize.Height) / 2f);
                }
            }
            return bmp;
        }
    }
}

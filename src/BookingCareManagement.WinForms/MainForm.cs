using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BookingCareManagement.WinForms
{
    public partial class MainForm : Form
    {
        private Panel sidebarPanel;
        private Panel navbarPanel;
        private Panel contentPanel;
        private Panel mainContentPanel;
        private FlowLayoutPanel paymentCardsPanel;

        public MainForm()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            // Form settings
            this.Text = "Booking Website";
            this.Size = new Size(1400, 800);
            this.MinimumSize = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 242, 245);
            this.WindowState = FormWindowState.Maximized;

            // Navbar Panel
            navbarPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.White
            };
            CreateNavbar();

            // Sidebar Panel
            sidebarPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 250,
                BackColor = Color.FromArgb(15, 23, 42),
                Padding = new Padding(0, 10, 0, 0),
                AutoScroll = true
            };
            CreateSidebar();

            // Content Panel
            contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(240, 242, 245),
                Padding = new Padding(20),
                AutoScroll = true
            };
            CreateContent();

            // Add controls to form
            this.Controls.Add(contentPanel);
            this.Controls.Add(sidebarPanel);
            this.Controls.Add(navbarPanel);

            // Handle resize event
            this.Resize += MainForm_Resize;
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            // Adjust navbar buttons position on resize
            AdjustNavbarButtons();
        }

        private void AdjustNavbarButtons()
        {
            if (navbarPanel.Controls.Count < 3) return;

            int formWidth = this.ClientSize.Width;

            // Upgrade button - 200px from right edge
            if (navbarPanel.Controls["upgradeBtn"] != null)
            {
                navbarPanel.Controls["upgradeBtn"].Location = new Point(formWidth - 390, 12);
            }

            // Share button - 80px from right edge
            if (navbarPanel.Controls["shareBtn"] != null)
            {
                navbarPanel.Controls["shareBtn"].Location = new Point(formWidth - 270, 12);
            }

            // Avatar - 40px from right edge
            if (navbarPanel.Controls["avatar"] != null)
            {
                navbarPanel.Controls["avatar"].Location = new Point(formWidth - 60, 12);
            }

            if (navbarPanel.Controls["avatarText"] != null)
            {
                navbarPanel.Controls["avatarText"].Location = new Point(formWidth - 60, 12);
            }
        }

        private void CreateNavbar()
        {
            // Booking Website Button with Dropdown
            RoundedButton bookingBtn = new RoundedButton
            {
                Name = "bookingBtn",
                Text = "🌐 Booking Website ▼",
                Location = new Point(20, 12),
                Size = new Size(190, 36),
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            bookingBtn.FlatAppearance.BorderSize = 0;

            // Upgrade Button
            RoundedButton upgradeBtn = new RoundedButton
            {
                Name = "upgradeBtn",
                Text = "⚡ Upgrade",
                Size = new Size(110, 36),
                BackColor = Color.FromArgb(255, 237, 213),
                ForeColor = Color.FromArgb(180, 83, 9),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            upgradeBtn.FlatAppearance.BorderSize = 0;

            // Share Booking Button
            RoundedButton shareBtn = new RoundedButton
            {
                Name = "shareBtn",
                Text = "🔗 Share Booking",
                Size = new Size(140, 36),
                BackColor = Color.FromArgb(147, 51, 234),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            shareBtn.FlatAppearance.BorderSize = 0;

            // User Avatar
            CircularPictureBox avatar = new CircularPictureBox
            {
                Name = "avatar",
                Size = new Size(36, 36),
                BackColor = Color.FromArgb(59, 130, 246),
                SizeMode = PictureBoxSizeMode.CenterImage
            };

            Label avatarText = new Label
            {
                Name = "avatarText",
                Text = "DT",
                Size = new Size(36, 36),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.Transparent
            };

            navbarPanel.Controls.Add(bookingBtn);
            navbarPanel.Controls.Add(upgradeBtn);
            navbarPanel.Controls.Add(shareBtn);
            navbarPanel.Controls.Add(avatar);
            navbarPanel.Controls.Add(avatarText);
            avatarText.BringToFront();

            // Initial position adjustment
            AdjustNavbarButtons();
        }

        private void CreateSidebar()
        {
            string[] menuItems = {
                "📅 Calendar",
                "📊 Dashboard",
                "✅ Appointments",
                "👥 Employees",
                "👤 Customers",
                "🎯 Services",
                "📍 Locations",
                "💰 Finance",
                "✨ Features &\n   Integrations",
                "🎨 Customize",
                "⚙️ Settings"
            };

            int yPos = 20;
            foreach (string item in menuItems)
            {
                SidebarButton btn = new SidebarButton
                {
                    Text = item,
                    Location = new Point(10, yPos),
                    Size = new Size(230, item.Contains("\n") ? 55 : 45),
                    BackColor = item.Contains("Finance") ? Color.FromArgb(30, 41, 59) : Color.Transparent,
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10),
                    TextAlign = ContentAlignment.MiddleLeft,
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand,
                    Padding = new Padding(15, 0, 0, 0)
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, 41, 59);

                if (item.Contains("Finance"))
                {
                    btn.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                }

                sidebarPanel.Controls.Add(btn);
                yPos += item.Contains("\n") ? 60 : 50;
            }
        }

        private void CreateContent()
        {
            // Breadcrumb
            Label breadcrumb = new Label
            {
                Text = "Finance › Payments",
                Location = new Point(10, 10),
                AutoSize = true,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 65, 85)
            };

            // Main Content Panel with TableLayoutPanel for responsive layout
            TableLayoutPanel mainLayout = new TableLayoutPanel
            {
                Location = new Point(10, 60),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 2,
                RowCount = 1,
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 50, 0, 0)
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 240));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            // Left Navigation Panel
            Panel leftNav = new Panel
            {
                Width = 240,
                Height = 400,
                BackColor = Color.FromArgb(249, 250, 251),
                Padding = new Padding(10)
            };

            string[] leftMenuItems = {
                "💳 Payments",
                "📄 Invoices",
                "💵 Transactions",
                "% Taxes",
                "🎟️ Coupons",
                "🎁 Gift Cards",
                "💰 Commissions"
            };

            int y = 10;
            foreach (string item in leftMenuItems)
            {
                Button btn = new Button
                {
                    Text = item,
                    Location = new Point(10, y),
                    Size = new Size(220, 40),
                    BackColor = item.Contains("Payments") ? Color.FromArgb(239, 246, 255) : Color.Transparent,
                    ForeColor = item.Contains("Payments") ? Color.FromArgb(37, 99, 235) : Color.FromArgb(71, 85, 105),
                    Font = new Font("Segoe UI", 10),
                    TextAlign = ContentAlignment.MiddleLeft,
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand,
                    Padding = new Padding(10, 0, 0, 0)
                };
                btn.FlatAppearance.BorderSize = 0;
                leftNav.Controls.Add(btn);
                y += 45;
            }

            // Right Content Area
            Panel rightContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(30),
                AutoScroll = true
            };

            Label title = new Label
            {
                Text = "Payments",
                Location = new Point(30, 20),
                AutoSize = true,
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42)
            };

            Label priceSettings = new Label
            {
                Text = "PRICE SETTINGS",
                Location = new Point(30, 80),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(148, 163, 184)
            };

            Label paymentMethods = new Label
            {
                Text = "PAYMENT METHODS",
                Location = new Point(30, 250),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(148, 163, 184)
            };

            // FlowLayoutPanel for payment cards - responsive
            paymentCardsPanel = new FlowLayoutPanel
            {
                Location = new Point(30, 280),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Padding = new Padding(0),
                MaximumSize = new Size(this.Width - 350, 0)
            };

            string[] providers = { "Square", "PayPal", "Stripe", "Mollie", "Authorize.Net" };
            foreach (string provider in providers)
            {
                Panel card = CreatePaymentCard(provider);
                paymentCardsPanel.Controls.Add(card);
            }

            // Update payment cards panel width on resize
            this.Resize += (s, e) =>
            {
                paymentCardsPanel.MaximumSize = new Size(this.Width - 350, 0);
            };

            rightContent.Controls.Add(title);
            rightContent.Controls.Add(priceSettings);
            rightContent.Controls.Add(paymentMethods);
            rightContent.Controls.Add(paymentCardsPanel);

            mainLayout.Controls.Add(leftNav, 0, 0);
            mainLayout.Controls.Add(rightContent, 1, 0);

            contentPanel.Controls.Add(breadcrumb);
            contentPanel.Controls.Add(mainLayout);
        }

        private Panel CreatePaymentCard(string provider)
        {
            Panel card = new Panel
            {
                Size = new Size(260, 180),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 0, 20, 20)
            };

            Label name = new Label
            {
                Text = provider,
                Location = new Point(15, 20),
                AutoSize = true,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42)
            };

            Label status = new Label
            {
                Text = "Status",
                Location = new Point(15, 50),
                AutoSize = true,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(100, 116, 139)
            };

            Label notConnected = new Label
            {
                Text = "Not Connected",
                Location = new Point(15, 70),
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42)
            };

            RoundedButton connectBtn = new RoundedButton
            {
                Text = "Connect Account",
                Location = new Point(15, 120),
                Size = new Size(230, 40),
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            connectBtn.FlatAppearance.BorderSize = 0;

            card.Controls.Add(name);
            card.Controls.Add(status);
            card.Controls.Add(notConnected);
            card.Controls.Add(connectBtn);

            return card;
        }
    }

    // Custom Rounded Button
    public class RoundedButton : Button
    {
        protected override void OnPaint(PaintEventArgs e)
        {
            GraphicsPath path = new GraphicsPath();
            int radius = 8;
            path.AddArc(0, 0, radius, radius, 180, 90);
            path.AddArc(Width - radius, 0, radius, radius, 270, 90);
            path.AddArc(Width - radius, Height - radius, radius, radius, 0, 90);
            path.AddArc(0, Height - radius, radius, radius, 90, 90);
            path.CloseFigure();

            this.Region = new Region(path);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (SolidBrush brush = new SolidBrush(this.BackColor))
            {
                e.Graphics.FillPath(brush, path);
            }

            TextRenderer.DrawText(e.Graphics, this.Text, this.Font,
                this.ClientRectangle, this.ForeColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
    }

    // Custom Sidebar Button
    public class SidebarButton : Button
    {
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        }
    }

    // Circular Picture Box
    public class CircularPictureBox : PictureBox
    {
        protected override void OnPaint(PaintEventArgs e)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddEllipse(0, 0, this.Width - 1, this.Height - 1);
            this.Region = new Region(path);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (SolidBrush brush = new SolidBrush(this.BackColor))
            {
                e.Graphics.FillEllipse(brush, 0, 0, this.Width - 1, this.Height - 1);
            }
        }
    } 
    }

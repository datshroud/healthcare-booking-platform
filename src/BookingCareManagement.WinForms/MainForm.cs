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
        private SidebarButton activeButton;



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

            // Navbar Panel
            navbarPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.White
            };
            CreateNavbar();

            // Content Panel
            contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(240, 242, 245),
                Padding = new Padding(20),
                AutoScroll = true
            };
            //CreateContent();

            // Add controls to form
            this.Controls.Add(contentPanel);
           
            this.Controls.Add(navbarPanel);
            this.Controls.Add(sidebarPanel);

            // Handle resize event
            this.Resize += MainForm_Resize;

            // Click outside to close account menu
            this.Click += (s, e) => CloseAccountMenu();
            contentPanel.Click += (s, e) => CloseAccountMenu();
            sidebarPanel.Click += (s, e) => CloseAccountMenu();
        }

        private void CloseAccountMenu()
        {
            // Try find accountMenu both on form and in navbarPanel (backward compat)
            Panel accountMenu = this.Controls["accountMenu"] as Panel
                                ?? navbarPanel.Controls["accountMenu"] as Panel;
            if (accountMenu != null && accountMenu.Visible)
            {
                accountMenu.Visible = false;
            }
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            // Adjust navbar buttons position on resize
            AdjustNavbarButtons();
        }

        private void AdjustNavbarButtons()
        {
            if (navbarPanel.Controls.Count < 2) return;

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
                navbarPanel.Controls["avatar"].Location = new Point(formWidth - 350, 12);
            }

            if (navbarPanel.Controls["avatarText"] != null)
            {
                navbarPanel.Controls["avatarText"].Location = new Point(formWidth - 310, 12);
            }

            // Account menu - align with avatar (accountMenu is now added to form)
            Panel accountMenu = this.Controls["accountMenu"] as Panel
                                ?? navbarPanel.Controls["accountMenu"] as Panel;
            if (accountMenu != null)
            {
                // Place the menu just below the navbar panel
                accountMenu.Location = new Point(formWidth - 280, navbarPanel.Height);
            }
        }

        private Panel CreateAccountMenu()
        {
            Panel menu = new Panel
            {
                Size = new Size(260, 200),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            // User info section (moved to top-left so it's visible)
            Panel userInfo = new Panel
            {
                Location = new Point(10, 10),
                Size = new Size(240, 80),
                BackColor = Color.FromArgb(59, 130, 246),
                BorderStyle = BorderStyle.FixedSingle
            };

            CircularPictureBox userAvatar = new CircularPictureBox
            {
                Location = new Point(15, 15),
                Size = new Size(50, 50),
                BackColor = Color.FromArgb(59, 130, 246),
                SizeMode = PictureBoxSizeMode.CenterImage
            };

            Label userAvatarText = new Label
            {
                Text = "DT",
                Location = new Point(15, 15),
                Size = new Size(50, 50),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                BackColor = Color.Transparent
            };

            Label userName = new Label
            {
                Text = "Dai Tran",
                Location = new Point(75, 15),
                AutoSize = true,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42)
            };

            Label userEmail = new Label
            {
                Text = "dai.tran@booking.com",
                Location = new Point(75, 38),
                AutoSize = true,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Black
            };

            userInfo.Controls.Add(userAvatar);
            userInfo.Controls.Add(userAvatarText);
            userInfo.Controls.Add(userName);
            userInfo.Controls.Add(userEmail);
            userAvatarText.BringToFront();

            // Divider
            Panel divider1 = new Panel
            {
                Location = new Point(10, 95),
                Size = new Size(240, 1),
                BackColor = Color.FromArgb(226, 232, 240)
            };

            // Account Settings button
            Button accountSettingsBtn = new Button
            {
                Text = "⚙️  Account Settings",
                Location = new Point(10, 100),
                Size = new Size(240, 40),
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(51, 65, 85),
                Font = new Font("Segoe UI", 10),
                TextAlign = ContentAlignment.MiddleLeft,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Padding = new Padding(10, 0, 0, 0)
            };
            accountSettingsBtn.FlatAppearance.BorderSize = 0;
            accountSettingsBtn.FlatAppearance.MouseOverBackColor = Color.FromArgb(249, 250, 251);
            accountSettingsBtn.Click += (s, e) =>
            {
                MessageBox.Show("Account Settings clicked!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            // Divider
            Panel divider2 = new Panel
            {
                Location = new Point(10, 145),
                Size = new Size(240, 1),
                BackColor = Color.FromArgb(226, 232, 240)
            };

            // Logout button
            Button logoutBtn = new Button
            {
                Text = "🚪  Đăng xuất",
                Location = new Point(10, 150),
                Size = new Size(240, 40),
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(220, 38, 38),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Padding = new Padding(10, 0, 0, 0)
            };
            logoutBtn.FlatAppearance.BorderSize = 0;
            logoutBtn.FlatAppearance.MouseOverBackColor = Color.FromArgb(254, 242, 242);
            logoutBtn.Click += (s, e) =>
            {
                var result = MessageBox.Show("Bạn có chắc chắn muốn đăng xuất?", "Xác nhận đăng xuất",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // Thực hiện đăng xuất
                    MessageBox.Show("Đã đăng xuất thành công!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close();
                }
            };

            menu.Controls.Add(userInfo);
            menu.Controls.Add(divider1);
            menu.Controls.Add(accountSettingsBtn);
            menu.Controls.Add(divider2);
            menu.Controls.Add(logoutBtn);

            return menu;
        }

        private void ToggleAccountMenu()
        {
            // Look for accountMenu on form first (we add it to form), fallback to navbarPanel
            Panel accountMenu = this.Controls["accountMenu"] as Panel
                                ?? navbarPanel.Controls["accountMenu"] as Panel;
            if (accountMenu != null)
            {
                accountMenu.Visible = !accountMenu.Visible;
                if (accountMenu.Visible)
                {
                    accountMenu.BringToFront();
                }
            }
        }

        private void CreateNavbar()
        {
            // Booking Website Button with Dropdown
            RoundedButton bookingBtn = new RoundedButton
            {
                Name = "bookingBtn",
                Text = "🌐 Booking Website ▼",
                Location = new Point(100, 12),
                Size = new Size(190, 36),
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            bookingBtn.FlatAppearance.BorderSize = 0;

            

            // User Avatar
            CircularPictureBox avatar = new CircularPictureBox
            {
                Name = "avatar",
                Size = new Size(36, 36),
                BackColor = Color.FromArgb(249, 250, 251),
                SizeMode = PictureBoxSizeMode.CenterImage
            };

            Label avatarText = new Label
            {
                Name = "avatarText",
                Text = "DT",
                Size = new Size(36, 36),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };

            // Account dropdown menu
            Panel accountMenu = CreateAccountMenu();
            accountMenu.Visible = false;
            accountMenu.Name = "accountMenu";

            // Click event for avatar
            avatar.Click += (s, e) => ToggleAccountMenu();
            avatarText.Click += (s, e) => ToggleAccountMenu();

            navbarPanel.Controls.Add(bookingBtn);
            navbarPanel.Controls.Add(avatar);
            navbarPanel.Controls.Add(avatarText);

            // Add accountMenu to the form (so it can appear below the navbar without being clipped)
            this.Controls.Add(accountMenu);
            accountMenu.BringToFront();

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
                    BackColor = Color.Transparent,
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10),
                    TextAlign = ContentAlignment.MiddleLeft,
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand,
                    Padding = new Padding(15, 0, 0, 0)
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, 41, 59);

                btn.Click += (s, e) =>
               {
                   SetActiveButton(btn);
               };

                sidebarPanel.Controls.Add(btn);
                yPos += item.Contains("\n") ? 60 : 50;
                if (item.Contains("Dashboard"))
                {
                    SetActiveButton(btn); 
                }
            }


        }
        private void SetActiveButton(SidebarButton btn)
        {
            // reset nút cũ
            if (activeButton != null)
            {
                activeButton.BackColor = Color.Transparent;
                activeButton.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            }

            // set nút mới
            activeButton = btn;
            activeButton.BackColor = Color.FromArgb(30, 41, 59);
            activeButton.Font = new Font("Segoe UI", 10, FontStyle.Bold);
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
}


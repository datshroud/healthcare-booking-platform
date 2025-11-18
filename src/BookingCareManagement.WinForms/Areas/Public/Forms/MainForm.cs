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
            private Form activeChildForm = null;

            // Biến cho chức năng kéo thả
            private SidebarButton draggedButton = null;
            private Point dragStartPoint;
            private Panel dragIndicator;
            private int dragInsertIndex = -1;

            public MainForm()
            {
                InitializeComponent();
                InitializeCustomComponents();
                CreateSidebar();    
                this.Load += (s, e) =>
                {
                    OpenChildForm(new Calendar());
                };
            }

            private void InitializeCustomComponents()
            {
                // Không cần tạo panels nữa vì đã có trong Designer
                // Chỉ cần cấu hình thêm các thành phần

                //CreateSidebar();
                CreateNavbar();

                // Sự kiện khi thay đổi kích thước cửa sổ
                this.Resize += MainForm_Resize;

                // Click ra ngoài để đóng menu tài khoản
                this.Click += (s, e) => CloseAccountMenu();
                contentPanel.Click += (s, e) => CloseAccountMenu();
                sidebarPanel.Click += (s, e) => CloseAccountMenu();

                // Tạo drag indicator
                dragIndicator = new Panel
                {
                    Height = 3,
                    Width = 230,
                    BackColor = Color.FromArgb(59, 130, 246),
                    Visible = false
                };
                sidebarPanel.Controls.Add(dragIndicator);
                dragIndicator.BringToFront();

                // Sự kiện kéo thả cho sidebar
                sidebarPanel.DragOver += SidebarPanel_DragOver;
                sidebarPanel.DragDrop += SidebarPanel_DragDrop;
            }

            private void CloseAccountMenu()
            {
                Panel accountMenu = this.Controls["accountMenu"] as Panel
                                    ?? navbarPanel.Controls["accountMenu"] as Panel;

                if (accountMenu != null && accountMenu.Visible)
                {
                    accountMenu.Visible = false;
                }
            }

            private void MainForm_Resize(object sender, EventArgs e)
            {
                AdjustNavbarButtons();
            }

            private void AdjustNavbarButtons()
            {
                if (navbarPanel.Controls.Count < 2) return;

                int formWidth = this.ClientSize.Width;

                if (navbarPanel.Controls["upgradeBtn"] != null)
                {
                    navbarPanel.Controls["upgradeBtn"].Location = new Point(formWidth - 390, 12);
                }

                if (navbarPanel.Controls["shareBtn"] != null)
                {
                    navbarPanel.Controls["shareBtn"].Location = new Point(formWidth - 270, 12);
                }

                if (navbarPanel.Controls["avatar"] != null)
                {
                    navbarPanel.Controls["avatar"].Location = new Point(formWidth - 350, 12);
                }

                if (navbarPanel.Controls["avatarText"] != null)
                {
                    navbarPanel.Controls["avatarText"].Location = new Point(formWidth - 310, 12);
                }

                Panel accountMenu = this.Controls["accountMenu"] as Panel
                                    ?? navbarPanel.Controls["accountMenu"] as Panel;

                if (accountMenu != null)
                {
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

                Panel divider1 = new Panel
                {
                    Location = new Point(10, 95),
                    Size = new Size(240, 1),
                    BackColor = Color.FromArgb(226, 232, 240)
                };

                Button accountSettingsBtn = new Button
                {
                    Text = "⚙️  Cài đặt tài khoản",
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
                    MessageBox.Show("Bạn đã nhấn Cài đặt tài khoản!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                };

                Panel divider2 = new Panel
                {
                    Location = new Point(10, 145),
                    Size = new Size(240, 1),
                    BackColor = Color.FromArgb(226, 232, 240)
                };

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
                    var result = MessageBox.Show("Bạn có chắc chắn muốn đăng xuất?", "Xác nhận",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        MessageBox.Show("Đăng xuất thành công!", "Thông báo",
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

            private void OpenChildForm(Form childForm)
            {
                if (activeChildForm != null)
                    activeChildForm.Close();

                activeChildForm = childForm;
                childForm.TopLevel = false;
                childForm.FormBorderStyle = FormBorderStyle.None;
                childForm.Dock = DockStyle.Fill;
                contentPanel.Controls.Add(childForm);
                contentPanel.Tag = childForm;
                childForm.BringToFront();
                childForm.Show();
            }

            private void ToggleAccountMenu()
            {
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

                Panel accountMenu = CreateAccountMenu();
                accountMenu.Visible = false;
                accountMenu.Name = "accountMenu";

                avatar.Click += (s, e) => ToggleAccountMenu();
                avatarText.Click += (s, e) => ToggleAccountMenu();

             
                navbarPanel.Controls.Add(avatar);
                navbarPanel.Controls.Add(avatarText);

                this.Controls.Add(accountMenu);
                accountMenu.BringToFront();

                avatarText.BringToFront();

                AdjustNavbarButtons();
            }

            private void CreateSidebar()
            {
                string[] menuItems = {
                    "📅 Lịch",
                    "📊 Bảng điều khiển",
                    "✅ Cuộc hẹn",
                    "👥 Bác sĩ",
                    "👤 Khách hàng",
                    "🎯 Chuyên khoa",
                    "💰 Hóa đơn",
                    "⚙️ Cài đặt"
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
                        Padding = new Padding(15, 0, 0, 0),
                        AllowDrop = true
                    };

                    btn.FlatAppearance.BorderSize = 0;
                    btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, 41, 59);

                    // Sự kiện click
                    btn.Click += (s, e) =>
                    {
                        SetActiveButton(btn);
                        if (btn.Text.Contains("Khách hàng"))
                        {
                            OpenChildForm(new Customer());
                        }
                        if (btn.Text.Contains("Lịch"))
                        {
                            if (!(activeChildForm is Calendar))
                                OpenChildForm(new Calendar());
                        }
                    };

                    // Sự kiện kéo thả
                    btn.MouseDown += Button_MouseDown;
                    btn.MouseMove += Button_MouseMove;
                    btn.DragOver += Button_DragOver;
                    btn.DragDrop += Button_DragDrop;
                    btn.QueryContinueDrag += Button_QueryContinueDrag;

                    sidebarPanel.Controls.Add(btn);

                    yPos += item.Contains("\n") ? 60 : 50;

                    if (item.Contains("Lịch"))
                    {
                        SetActiveButton(btn);
                    }
                }
            }

            // Sự kiện kéo thả cho sidebar buttons
            private void Button_MouseDown(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Left)
                {
                    SidebarButton btn = sender as SidebarButton;
                    dragStartPoint = e.Location;
                    draggedButton = btn;
                }
            }

            private void Button_MouseMove(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Left && draggedButton != null)
                {
                    // Kiểm tra xem chuột đã di chuyển đủ xa chưa
                    if (Math.Abs(e.X - dragStartPoint.X) > 5 || Math.Abs(e.Y - dragStartPoint.Y) > 5)
                    {
                        draggedButton.DoDragDrop(draggedButton, DragDropEffects.Move);
                    }
                }
            }

            private void Button_DragOver(object sender, DragEventArgs e)
            {
                e.Effect = DragDropEffects.Move;
            }

            private void Button_DragDrop(object sender, DragEventArgs e)
            {
                // Xử lý trong SidebarPanel_DragDrop
            }

            private void Button_QueryContinueDrag(object sender, QueryContinueDragEventArgs e)
            {
                if (e.EscapePressed)
                {
                    e.Action = DragAction.Cancel;
                    dragIndicator.Visible = false;
                    draggedButton = null;
                }
            }

            private void SidebarPanel_DragOver(object sender, DragEventArgs e)
            {
                if (draggedButton == null) return;

                e.Effect = DragDropEffects.Move;

                // Tính toán vị trí chèn
                Point pt = sidebarPanel.PointToClient(new Point(e.X, e.Y));

                List<SidebarButton> buttons = sidebarPanel.Controls.OfType<SidebarButton>()
                    .OrderBy(b => b.Top).ToList();

                dragInsertIndex = -1;

                for (int i = 0; i < buttons.Count; i++)
                {
                    SidebarButton btn = buttons[i];
                    int btnMiddle = btn.Top + btn.Height / 2;

                    if (pt.Y < btnMiddle)
                    {
                        dragInsertIndex = i;
                        break;
                    }
                }

                if (dragInsertIndex == -1)
                {
                    dragInsertIndex = buttons.Count;
                }

                // Hiển thị drag indicator
                if (dragInsertIndex < buttons.Count)
                {
                    dragIndicator.Location = new Point(10, buttons[dragInsertIndex].Top - 2);
                }
                else if (buttons.Count > 0)
                {
                    SidebarButton lastBtn = buttons[buttons.Count - 1];
                    dragIndicator.Location = new Point(10, lastBtn.Bottom + 3);
                }

                dragIndicator.Visible = true;
                dragIndicator.BringToFront();
            }

            private void SidebarPanel_DragDrop(object sender, DragEventArgs e)
            {
                if (draggedButton == null || dragInsertIndex == -1)
                {
                    dragIndicator.Visible = false;
                    return;
                }

                // Lấy danh sách các button
                List<SidebarButton> buttons = sidebarPanel.Controls.OfType<SidebarButton>()
                    .OrderBy(b => b.Top).ToList();

                int oldIndex = buttons.IndexOf(draggedButton);

                if (oldIndex == dragInsertIndex || oldIndex + 1 == dragInsertIndex)
                {
                    dragIndicator.Visible = false;
                    draggedButton = null;
                    return;
                }

                // Xóa button khỏi vị trí cũ
                buttons.RemoveAt(oldIndex);

                // Điều chỉnh index nếu cần
                int newIndex = dragInsertIndex;
                if (oldIndex < dragInsertIndex)
                {
                    newIndex--;
                }

                // Chèn vào vị trí mới
                buttons.Insert(newIndex, draggedButton);

                // Cập nhật lại vị trí tất cả các button
                int yPos = 20;
                foreach (SidebarButton btn in buttons)
                {
                    btn.Location = new Point(10, yPos);
                    yPos += btn.Height + 5;
                }

                dragIndicator.Visible = false;
                draggedButton = null;
            }

            private void SetActiveButton(SidebarButton btn)
            {
                if (activeButton != null)
                {
                    activeButton.BackColor = Color.Transparent;
                    activeButton.Font = new Font("Segoe UI", 10, FontStyle.Regular);
                }

                activeButton = btn;
                activeButton.BackColor = Color.FromArgb(30, 41, 59);
                activeButton.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            }

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

            public class SidebarButton : Button
            {
                protected override void OnPaint(PaintEventArgs e)
                {
                    base.OnPaint(e);
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                }
            }

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

            private void btnLich_Click(object sender, EventArgs e)
            {
                if (this.Visible)
                {
                    OpenChildForm(new Calendar());
                }
            }

            private void btnBangDieuKhien_Click(object sender, EventArgs e)
            {

            }

            private void btnKhachHang_Click(object sender, EventArgs e)
            {
                if (this.Visible)
                {
                    OpenChildForm(new Customer());
                }
            }
        }
    }
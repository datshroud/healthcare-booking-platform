using BookingCareManagement.WinForms.Areas.Admin.Controls;
using BookingCareManagement.WinForms.Areas.Admin.Forms;
using BookingCareManagement.WinForms.Areas.Admin.Services;
using BookingCareManagement.WinForms.Areas.Doctor.Forms;
using BookingCareManagement.WinForms.Areas.Customer.Forms;
using BookingCareManagement.WinForms.Shared.State;
using BookingCareManagement.WinForms.Areas.Account.Forms;
using BookingCareManagement.WinForms.Shared.Services;
using Microsoft.Extensions.DependencyInjection;
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
using System.Net.Http;
using System.IO;
using BookingCareManagement.WinForms.Areas.Doctor.Services; // added for DoctorAppointmentsApiClient

namespace BookingCareManagement.WinForms
{
    public partial class MainForm : Form
    {
        private Panel sidebarPanel = null!;
        private Panel navbarPanel = null!;
        private Panel contentPanel = null!;
        private SidebarButton? activeButton = null;
        private Form? activeChildForm = null;

        // Biến cho chức năng kéo thả
        private SidebarButton? draggedButton = null;
        private Point dragStartPoint;
        private Panel? dragIndicator = null;
        private int dragInsertIndex = -1;

        private readonly IServiceProvider _serviceProvider;
        private readonly SessionState _sessionState;

        public MainForm(IServiceProvider serviceProvider)
        {
            try { System.IO.File.AppendAllText("debug_winforms.log", $"[{DateTime.Now:O}] MainForm: constructor start\n"); } catch {}
            _serviceProvider = serviceProvider;
            _sessionState = serviceProvider.GetRequiredService<SessionState>();
            
            InitializeComponent();
            InitializeCustomComponents();
            CreateSidebar();
            UpdateAccountDisplay();
            try { System.IO.File.AppendAllText("debug_winforms.log", $"[{DateTime.Now:O}] MainForm: after CreateSidebar\n"); } catch {}
            // Rebuild sidebar when session/profile changes (e.g. after login)
            _sessionState.StateChanged += (s, e) =>
            {
                if (this.IsHandleCreated && this.InvokeRequired)
                {
                    this.BeginInvoke(new Action(() =>
                    {
                        RebuildSidebar();
                        UpdateAccountDisplay();
                    }));
                }
                else
                {
                    RebuildSidebar();
                    UpdateAccountDisplay();
                }
            };

            this.Load += (s, e) =>
            {
                try { System.IO.File.AppendAllText("debug_winforms.log", $"[{DateTime.Now:O}] MainForm: Load event\n"); } catch {}
                OpenRoleDashboard();
            };
        }

        private void RebuildSidebar()
        {
            // Remove existing SidebarButton controls only, preserve other controls like dragIndicator
            var toRemove = sidebarPanel.Controls.OfType<SidebarButton>().ToList();
            foreach (var c in toRemove)
            {
                sidebarPanel.Controls.Remove(c);
                c.Dispose();
            }

            CreateSidebar();
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
            if (dragIndicator != null) dragIndicator.BringToFront();

            // Sự kiện kéo thả cho sidebar
            sidebarPanel.DragOver += SidebarPanel_DragOver;
            sidebarPanel.DragDrop += SidebarPanel_DragDrop;
        }

        private void OpenRoleDashboard()
        {
            // If user is doctor (but not admin), open doctor appointments
            if (HasDoctorAccess() && !HasAdminAccess())
            {
                var doctorForm = _serviceProvider.GetRequiredService<DoctorAppointmentsForm>();
                OpenChildForm(doctorForm);
                return;
            }

            // If user is admin, open admin dashboard
            if (HasAdminAccess())
            {
                var dashboard = _serviceProvider.GetRequiredService<DashboardForm>();
                OpenChildForm(dashboard);
                return;
            }

            // Default for regular customers: open Services page
            try
            {
                var serviceForm = _serviceProvider.GetRequiredService<Service>();
                OpenChildForm(serviceForm);
            }
            catch (Exception ex)
            {
                // Fallback to dashboard if Service resolution fails
                try
                {
                    var dashboard = _serviceProvider.GetRequiredService<DashboardForm>();
                    OpenChildForm(dashboard);
                }
                catch { }
            }
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

        private void MainForm_Resize(object? sender, EventArgs e)
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

            userAvatarText.Name = "account_avatarText";
            userAvatar.Name = "account_avatar";

            Label userName = new Label
            {
                Text = "Dai Tran",
                Location = new Point(75, 15),
                AutoSize = true,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42)
            };
            userName.Name = "account_userName";

            Label userEmail = new Label
            {
                Text = "dai.tran@booking.com",
                Location = new Point(75, 38),
                AutoSize = true,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Black
            };
            userEmail.Name = "account_userEmail";

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
                try
                {
                    var editor = _serviceProvider.GetService<Areas.Account.Forms.AccountEditorForm>();
                    if (editor != null)
                    {
                        var res = editor.ShowDialog(this);
                        if (res == DialogResult.OK)
                        {
                            // profile saved -> session updated inside editor
                            UpdateAccountDisplay();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Cài đặt tài khoản chưa được đăng ký.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Không thể mở cài đặt tài khoản: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
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

            logoutBtn.Click += async (s, e) =>
            {
                var result = MessageBox.Show("Bạn có chắc chắn muốn đăng xuất?", "Xác nhận",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    var auth = _serviceProvider.GetService<Shared.Services.AuthService>();
                    try
                    {
                        if (auth != null)
                        {
                            await auth.LogoutAsync();
                        }
                    }
                    catch
                    {
                        // ignore logout failures; continue to login prompt
                    }

                    var loginSuccessful = false;
                    try
                    {
                        this.Hide();
                        using var login = _serviceProvider.GetService<Login>();
                        if (login != null)
                        {
                            var r = login.ShowDialog(this);
                            if (r == DialogResult.OK)
                            {
                                loginSuccessful = true;
                                RebuildSidebar();
                                UpdateAccountDisplay();
                                OpenRoleDashboard();
                            }
                        }
                    }
                    finally
                    {
                        if (loginSuccessful)
                        {
                            this.Show();
                        }
                        else
                        {
                            this.Close();
                        }
                    }
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

            // If the requested child is the Bookings form, host it inside a scrollable wrapper
            if (childForm is BookingCareManagement.WinForms.Areas.Customer.Forms.Bookings)
            {
                // Remove any existing bookings host
                var existingHost = contentPanel.Controls.Find("bookingsHost", false).FirstOrDefault() as Panel;
                if (existingHost != null)
                {
                    contentPanel.Controls.Remove(existingHost);
                    existingHost.Dispose();
                }

                // Create a scrollable host panel that fills the content area
                var host = new Panel
                {
                    Name = "bookingsHost",
                    Dock = DockStyle.Fill,
                    AutoScroll = true,
                    BackColor = Color.Transparent
                };

                activeChildForm = childForm;
                childForm.TopLevel = false;
                childForm.FormBorderStyle = FormBorderStyle.None;

                // Let the bookings form keep its designed height; dock to Top so host can scroll vertically
                childForm.Dock = DockStyle.Top;

                // Add child into host, then host into contentPanel
                host.Controls.Add(childForm);
                contentPanel.Controls.Add(host);
                contentPanel.Tag = childForm;

                // Ensure child width matches host width to avoid horizontal scrollbar if not needed
                try
                {
                    childForm.Width = Math.Max(childForm.ClientSize.Width, host.ClientSize.Width);
                }
                catch { }

                childForm.BringToFront();
                childForm.Show();
                return;
            }

            // Default: host child forms normally (fill)
            activeChildForm = childForm;
            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill;
            contentPanel.Controls.Add(childForm);
            contentPanel.Tag = childForm;
            childForm.BringToFront();
            childForm.Show();
        }

        // Expose a safe public API so other forms can request being shown in the main content area
        public void ShowChildFormInShell(Form childForm)
        {
            // Delegate to existing OpenChildForm logic
            if (childForm == null) return;
            OpenChildForm(childForm);
        }

        private void ToggleAccountMenu()
        {
            Panel accountMenu = this.Controls["accountMenu"] as Panel
                                ?? navbarPanel.Controls["accountMenu"] as Panel;

            if (accountMenu != null)
            {
                UpdateAccountDisplay();
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

        private void UpdateAccountDisplay()
        {
            try
            {
                var avatarText = navbarPanel.Controls["avatarText"] as Label;
                var avatar = navbarPanel.Controls["avatar"] as PictureBox;

                Panel accountMenu = this.Controls["accountMenu"] as Panel
                                    ?? navbarPanel.Controls["accountMenu"] as Panel;

                var userName = accountMenu?.Controls.Find("account_userName", true).FirstOrDefault() as Label;
                var userEmail = accountMenu?.Controls.Find("account_userEmail", true).FirstOrDefault() as Label;
                var accountAvatarText = accountMenu?.Controls.Find("account_avatarText", true).FirstOrDefault() as Label;

                var name = _sessionState.DisplayName;
                var email = _sessionState.Email;
                string initials = string.Empty;
                if (!string.IsNullOrWhiteSpace(name))
                {
                    var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 1)
                        initials = parts[0].Substring(0, 1).ToUpperInvariant();
                    else
                        initials = (parts[0].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpperInvariant();
                }

                if (userName != null) userName.Text = string.IsNullOrWhiteSpace(name) ? email : name;
                if (userEmail != null) userEmail.Text = email;

                // Try to load avatar image asynchronously. If AvatarUrl missing or loading fails, fall back to initials.
                var avatarUrl = _sessionState.AvatarUrl;
                if (!string.IsNullOrWhiteSpace(avatarUrl) && avatar != null && avatarText != null && accountAvatarText != null)
                {
                    // Start a background task to fetch the image so we don't block UI
                    Task.Run(async () =>
                    {
                        try
                        {
                            var httpFactory = _serviceProvider.GetService(typeof(IHttpClientFactory)) as IHttpClientFactory;
                            string resolvedUrl = avatarUrl;
                            var apiClient = httpFactory?.CreateClient("BookingCareApi");
                            if (!resolvedUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase) && apiClient?.BaseAddress != null)
                            {
                                resolvedUrl = new Uri(apiClient.BaseAddress, resolvedUrl).ToString();
                            }

                            // Use the configured BookingCareApi client so auth headers (if any) are applied
                            if (apiClient != null)
                            {
                                using var resp = await apiClient.GetAsync(resolvedUrl);
                                resp.EnsureSuccessStatusCode();
                                using var ms = await resp.Content.ReadAsStreamAsync();
                                var img = Image.FromStream(ms);

                                // Apply image on UI thread
                                if (!this.IsDisposed)
                                {
                                    this.BeginInvoke(new Action(() =>
                                    {
                                        try
                                        {
                                            avatar.Image = img;
                                            avatarText.Visible = false;
                                            if (accountAvatarText != null) accountAvatarText.Visible = false;
                                        }
                                        catch { }
                                    }));
                                }
                                return;
                            }

                            // Fallback: try plain HttpClient if named client not available
                            using var http = new HttpClient();
                            var bytes = await http.GetByteArrayAsync(resolvedUrl);
                            using var ms2 = new System.IO.MemoryStream(bytes);
                            var img2 = Image.FromStream(ms2);

                            if (!this.IsDisposed)
                            {
                                this.BeginInvoke(new Action(() =>
                                {
                                    try
                                    {
                                        avatar.Image = img2;
                                        avatarText.Visible = false;
                                        if (accountAvatarText != null) accountAvatarText.Visible = false;
                                    }
                                    catch { }
                                }));
                            }

                            // (done above for both success paths)
                        }
                        catch {
                            // fallback to initials — ensure UI shows initials
                            if (!this.IsDisposed)
                            {
                                this.BeginInvoke(new Action(() =>
                                {
                                    try
                                    {
                                        if (avatar != null) avatar.Image = null;
                                        if (avatarText != null) avatarText.Text = initials;
                                        if (accountAvatarText != null) accountAvatarText.Text = initials;
                                        if (avatarText != null) avatarText.Visible = true;
                                        if (accountAvatarText != null) accountAvatarText.Visible = true;
                                    }
                                    catch { }
                                }));
                            }
                        }
                    });
                }
                else
                {
                    if (avatar != null) avatar.Image = null;
                    if (avatarText != null) avatarText.Text = initials;
                    if (accountAvatarText != null) accountAvatarText.Text = initials;
                    if (avatarText != null) avatarText.Visible = true;
                    if (accountAvatarText != null) accountAvatarText.Visible = true;
                }
            }
            catch { }
        }

        private void CreateSidebar()
        {
            // Role-aware sidebar
            // If the current session is Admin -> show an admin placeholder image/label
            // If Doctor (and not Admin) -> show a doctor placeholder
            // Otherwise (customer) -> show three main buttons: Dịch vụ, Đặt lịch, Lịch của tôi

            int yPos = 20;
            var isAdmin = HasAdminAccess();
            var isDoctor = HasDoctorAccess() && !isAdmin;

            if (isAdmin)
            {
                // Admin: show the full admin menu
                string[] adminItems = {
                    "📅 Lịch",
                    "📊 Bảng điều khiển",
                    "✅ Cuộc hẹn",
                    "👥 Bác sĩ",
                    "👤 Khách hàng",
                    "🎯 Chuyên khoa",
                    "💰 Hóa đơn",
                    "⚙️ Cài đặt"
                };

                int ay = yPos;
                foreach (var item in adminItems)
                {
                    SidebarButton btn = new SidebarButton
                    {
                        Text = item,
                        Location = new Point(10, ay),
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

                    btn.Click += (s, e) =>
                    {
                        SetActiveButton(btn);
                        if (btn.Text.Contains("Bảng") || btn.Text.Contains("Bang"))
                        {
                            OpenRoleDashboard();
                        }
                        if (btn.Text.Contains("Khách hàng"))
                        {
                            var customerForm = _serviceProvider.GetRequiredService<Customer>();
                            OpenChildForm(customerForm);
                        }
                        if (btn.Text.Contains("Lịch"))
                        {
                            if (!(activeChildForm is Calendar))
                            {
                                var appointmentsApiClient = _serviceProvider.GetRequiredService<AdminAppointmentsApiClient>();
                                OpenChildForm(new Calendar(appointmentsApiClient));
                            }
                        }
                        if (btn.Text.Contains("Cuộc hẹn"))
                        {
                            if (!(activeChildForm is AppointmentEditorForm))
                            {
                                var appointmentForm = _serviceProvider.GetRequiredService<AppointmentEditorForm>();
                                OpenChildForm(appointmentForm);
                            }
                        }
                        if (btn.Text.Contains("Hóa đơn"))
                        {
                            var invoiceForm = _serviceProvider.GetRequiredService<InvoiceEditorForm>();
                            OpenChildForm(invoiceForm);
                        }
                        if (btn.Text.Contains("Bác sĩ"))
                        {
                            if (!(activeChildForm is Doctor))
                            {
                                var doctorForm = _serviceProvider.GetRequiredService<Doctor>();
                                OpenChildForm(doctorForm);
                            }
                        }
                        if (btn.Text.Contains("Chuyên khoa"))
                        {
                            if (!(activeChildForm is Specialty))
                            {
                                var specialtyForm = _serviceProvider.GetRequiredService<Specialty>
                                ();
                                OpenChildForm(specialtyForm);
                            }
                        }
                    };

                    btn.MouseDown += Button_MouseDown;
                    btn.MouseMove += Button_MouseMove;
                    btn.DragOver += Button_DragOver;
                    btn.DragDrop += Button_DragDrop;
                    btn.QueryContinueDrag += Button_QueryContinueDrag;

                    sidebarPanel.Controls.Add(btn);
                    ay += item.Contains("\n") ? 60 : 50;

                    if (item.Contains("Bảng") || item.Contains("Bang"))
                    {
                        SetActiveButton(btn);
                    }
                }
                return;
            }

            if (isDoctor)
            {
                // Doctor: simplified menu
                string[] doctorItems = {
                    "📅 Lịch hẹn",
                    "✅ Quản lý lịch hẹn",
                    "👤 Khách hàng",
                    "📊 Thống kê"
                };

                int dy = yPos;
                foreach (var item in doctorItems)
                {
                    SidebarButton btn = new SidebarButton
                    {
                        Text = item,
                        Location = new Point(10, dy),
                        Size = new Size(230, 45),
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

                    btn.Click += (s, e) =>
                    {
                        SetActiveButton(btn);
                        if (item.StartsWith("📅"))
                        {
                            // Use doctor client when opening calendar for doctors
                            var doctorApi = _serviceProvider.GetRequiredService<DoctorAppointmentsApiClient>();
                            OpenChildForm(new Calendar(doctorApi));
                        }
                        if (item.Contains("Quản lý"))
                        {
                            var doctorAppointments = _serviceProvider.GetRequiredService<DoctorAppointmentsForm>();
                            OpenChildForm(doctorAppointments);
                        }
                        if (item.Contains("Khách hàng"))
                        {
                            var doctorCustomers = _serviceProvider.GetRequiredService<DoctorCustomerForm>();
                            OpenChildForm(doctorCustomers);
                        }
                    };

                    btn.MouseDown += Button_MouseDown;
                    btn.MouseMove += Button_MouseMove;
                    btn.DragOver += Button_DragOver;
                    btn.DragDrop += Button_DragDrop;
                    btn.QueryContinueDrag += Button_QueryContinueDrag;

                    sidebarPanel.Controls.Add(btn);
                    dy += 50;
                }

                return;
            }

            // Default: Customer view — three primary buttons
            SidebarButton btnServices = new SidebarButton
            {
                Text = "🩺  Dịch vụ",
                Location = new Point(10, yPos),
                Size = new Size(230, 45),
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10),
                TextAlign = ContentAlignment.MiddleLeft,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Padding = new Padding(15, 0, 0, 0)
            };
            yPos += 50;

            SidebarButton btnBook = new SidebarButton
            {
                Text = "📆  Đặt lịch",
                Location = new Point(10, yPos),
                Size = new Size(230, 45),
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10),
                TextAlign = ContentAlignment.MiddleLeft,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Padding = new Padding(15, 0, 0, 0)
            };
            yPos += 50;

            SidebarButton btnMyBookings = new SidebarButton
            {
                Text = "📋  Lịch của tôi",
                Location = new Point(10, yPos),
                Size = new Size(230, 45),
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10),
                TextAlign = ContentAlignment.MiddleLeft,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Padding = new Padding(15, 0, 0, 0)
            };

            foreach (var b in new[] { btnServices, btnBook, btnMyBookings })
            {
                b.FlatAppearance.BorderSize = 0;
                b.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, 41, 59);
                b.MouseDown += Button_MouseDown;
                b.MouseMove += Button_MouseMove;
                b.DragOver += Button_DragOver;
                b.DragDrop += Button_DragDrop;
                b.QueryContinueDrag += Button_QueryContinueDrag;
                sidebarPanel.Controls.Add(b);
            }

            // Handlers
            btnServices.Click += (s, e) =>
            {
                SetActiveButton(btnServices);
                try
                {
                    if (!(activeChildForm is Service))
                    {
                        var serviceForm = _serviceProvider.GetRequiredService<Service>();
                        OpenChildForm(serviceForm);
                    }
                }
                catch (Exception ex)
                {
                    try { System.IO.File.AppendAllText("debug_winforms.log", $"[{DateTime.Now:O}] Error opening Service form: {ex}\n"); } catch {}
                    MessageBox.Show($"Không thể mở trang Dịch vụ: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            btnBook.Click += (s, e) =>
            {
                SetActiveButton(btnBook);
                if (!(activeChildForm is Bookings))
                {
                    var bookingsForm = _serviceProvider.GetRequiredService<Bookings>();
                    OpenChildForm(bookingsForm);
                }
            };

            btnMyBookings.Click += (s, e) =>
            {
                SetActiveButton(btnMyBookings);
                if (!(activeChildForm is MyBookingForm))
                {
                    var myBookings = _serviceProvider.GetRequiredService<MyBookingForm>();
                    OpenChildForm(myBookings);
                }
            };

            // Default active for customer
            SetActiveButton(btnServices);
        }

        private bool HasDoctorAccess()
        {
            if (_sessionState.IsDoctor)
            {
                return true;
            }

            var redirect = _sessionState.LastRedirect;
            return redirect.IndexOf("doctor", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private bool HasAdminAccess()
        {
            if (_sessionState.IsAdmin)
            {
                return true;
            }

            var redirect = _sessionState.LastRedirect;
            return redirect.IndexOf("/doctor", StringComparison.OrdinalIgnoreCase) < 0 &&
                   redirect.IndexOf("/dashboard", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        // Sự kiện kéo thả cho sidebar buttons
        private void Button_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                var btn = sender as SidebarButton;
                if (btn == null) return;
                dragStartPoint = e.Location;
                draggedButton = btn;
            }
        }

        private void Button_MouseMove(object? sender, MouseEventArgs e)
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

        private void Button_DragOver(object? sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void Button_DragDrop(object? sender, DragEventArgs e)
        {
            // Xử lý trong SidebarPanel_DragDrop
        }

        private void Button_QueryContinueDrag(object? sender, QueryContinueDragEventArgs e)
        {
            if (e.EscapePressed)
            {
                e.Action = DragAction.Cancel;
                if (dragIndicator != null) dragIndicator.Visible = false;
                draggedButton = null;
            }
        }

        private void SidebarPanel_DragOver(object? sender, DragEventArgs e)
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
            if (dragIndicator != null)
            {
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
            }
            if (dragIndicator != null) dragIndicator.BringToFront();
        }

        private void SidebarPanel_DragDrop(object? sender, DragEventArgs e)
        {
            if (draggedButton == null || dragInsertIndex == -1)
            {
                if (dragIndicator != null) dragIndicator.Visible = false;
                return;
            }

            // Lấy danh sách các button
            List<SidebarButton> buttons = sidebarPanel.Controls.OfType<SidebarButton>()
                .OrderBy(b => b.Top).ToList();

            int oldIndex = buttons.IndexOf(draggedButton);

            if (oldIndex == dragInsertIndex || oldIndex + 1 == dragInsertIndex)
            {
                if (dragIndicator != null) dragIndicator.Visible = false;
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

            if (dragIndicator != null) dragIndicator.Visible = false;
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
                if (HasDoctorAccess() && !HasAdminAccess())
                {
                    var doctorApi = _serviceProvider.GetRequiredService<DoctorAppointmentsApiClient>();
                    OpenChildForm(new Calendar(doctorApi));
                }
                else
                {
                    var appointmentsApiClient = _serviceProvider.GetRequiredService<AdminAppointmentsApiClient>();
                    OpenChildForm(new Calendar(appointmentsApiClient));
                }
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

        // Open Bookings and preselect specialty safely from shell
        public async Task ShowBookingsForSpecialtyAsync(Guid specialtyId, string specialtyName, decimal price)
        {
            try
            {
                // If there's already a Bookings instance hosted, reuse it
                if (activeChildForm is BookingCareManagement.WinForms.Areas.Customer.Forms.Bookings existingBookings)
                {
                    await existingBookings.OpenForSpecialtyAsync(specialtyId, specialtyName, price);

                    // ensure it's visible and focused
                    existingBookings.BringToFront();
                    existingBookings.Show();
                    activeChildForm = existingBookings;
                    return;
                }

                // Resolve from DI so MainForm controls creation on UI thread
                var bookings = _serviceProvider.GetService<Bookings>();
                if (bookings is null)
                {
                    System.IO.File.AppendAllText("debug_winforms.log", $"[{DateTime.Now:O}] ShowBookingsForSpecialtyAsync: Bookings service not registered.\n");
                    return;
                }

                // Ensure bookings loads data before showing to avoid blank flicker
                await bookings.OpenForSpecialtyAsync(specialtyId, specialtyName, price);

                // Show inside content area
                OpenChildForm(bookings);

                // Activate the "Đặt lịch" sidebar button so UI reflects current view
                try
                {
                    var bookBtn = sidebarPanel.Controls.OfType<SidebarButton>()
                        .FirstOrDefault(b => (b.Text ?? string.Empty).IndexOf("Đặt lịch", StringComparison.OrdinalIgnoreCase) >= 0
                                             || (b.Text ?? string.Empty).IndexOf("Đặt", StringComparison.OrdinalIgnoreCase) >= 0);
                    if (bookBtn != null)
                    {
                        SetActiveButton(bookBtn);
                    }
                }
                catch { }
            }
            catch (Exception ex)
            {
                try { System.IO.File.AppendAllText("debug_winforms.log", $"[{DateTime.Now:O}] ShowBookingsForSpecialtyAsync exception: {ex}\n"); } catch {}
                MessageBox.Show($"Không thể mở trang đặt lịch: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

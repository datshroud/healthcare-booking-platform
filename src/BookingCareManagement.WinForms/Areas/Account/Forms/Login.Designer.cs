namespace BookingCareManagement.WinForms.Areas.Account.Forms
{
    partial class Login
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            panelLeft = new Panel();
            labelWelcome = new Label();
            labelWelcomeSubtitle = new Label();
            pictureBoxLogo = new PictureBox();
            panelRight = new Panel();
            panelForm = new Panel();
            linkLabelRegister = new LinkLabel();
            labelOr = new Label();
            linkLabelForgotPassword = new LinkLabel();
            buttonLogin = new Button();
            checkBoxRemember = new CheckBox();
            panelPasswordBorder = new Panel();
            textBoxPassword = new TextBox();
            labelPassword = new Label();
            panelUsernameBorder = new Panel();
            textBoxUsername = new TextBox();
            labelUsername = new Label();
            labelSubtitle = new Label();
            labelTitle = new Label();
            panelLeft.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxLogo).BeginInit();
            panelRight.SuspendLayout();
            panelForm.SuspendLayout();
            panelPasswordBorder.SuspendLayout();
            panelUsernameBorder.SuspendLayout();
            SuspendLayout();
            // 
            // panelLeft
            // 
            panelLeft.BackColor = Color.FromArgb(23, 162, 184);
            panelLeft.Controls.Add(labelWelcome);
            panelLeft.Controls.Add(labelWelcomeSubtitle);
            panelLeft.Controls.Add(pictureBoxLogo);
            panelLeft.Dock = DockStyle.Left;
            panelLeft.Location = new Point(0, 0);
            panelLeft.Margin = new Padding(3, 4, 3, 4);
            panelLeft.Name = "panelLeft";
            panelLeft.Size = new Size(420, 769);
            panelLeft.TabIndex = 0;
            // 
            // labelWelcome
            // 
            labelWelcome.Dock = DockStyle.Bottom;
            labelWelcome.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            labelWelcome.ForeColor = Color.White;
            labelWelcome.Location = new Point(0, 607);
            labelWelcome.Name = "labelWelcome";
            labelWelcome.Size = new Size(420, 50);
            labelWelcome.TabIndex = 2;
            labelWelcome.Text = "Chào mừng trở lại!";
            labelWelcome.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // labelWelcomeSubtitle
            // 
            labelWelcomeSubtitle.Dock = DockStyle.Bottom;
            labelWelcomeSubtitle.Font = new Font("Segoe UI", 10F);
            labelWelcomeSubtitle.ForeColor = Color.White;
            labelWelcomeSubtitle.Location = new Point(0, 657);
            labelWelcomeSubtitle.Name = "labelWelcomeSubtitle";
            labelWelcomeSubtitle.Padding = new Padding(20, 0, 20, 38);
            labelWelcomeSubtitle.Size = new Size(420, 112);
            labelWelcomeSubtitle.TabIndex = 1;
            labelWelcomeSubtitle.Text = "Đăng nhập để tiếp tục sử dụng\r\nHệ thống quản lý đặt khám BookingCare";
            labelWelcomeSubtitle.TextAlign = ContentAlignment.TopCenter;
            // 
            // pictureBoxLogo
            // 
            pictureBoxLogo.Anchor = AnchorStyles.None;
            pictureBoxLogo.Location = new Point(85, 188);
            pictureBoxLogo.Margin = new Padding(3, 4, 3, 4);
            pictureBoxLogo.Name = "pictureBoxLogo";
            pictureBoxLogo.Size = new Size(250, 312);
            pictureBoxLogo.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxLogo.TabIndex = 0;
            pictureBoxLogo.TabStop = false;
            // 
            // panelRight
            // 
            panelRight.BackColor = Color.White;
            panelRight.Controls.Add(panelForm);
            panelRight.Dock = DockStyle.Fill;
            panelRight.Location = new Point(420, 0);
            panelRight.Margin = new Padding(3, 4, 3, 4);
            panelRight.Name = "panelRight";
            panelRight.Padding = new Padding(50, 100, 50, 50);
            panelRight.Size = new Size(513, 769);
            panelRight.TabIndex = 1;
            // 
            // panelForm
            // 
            panelForm.Controls.Add(linkLabelRegister);
            panelForm.Controls.Add(labelOr);
            panelForm.Controls.Add(linkLabelForgotPassword);
            panelForm.Controls.Add(buttonLogin);
            panelForm.Controls.Add(checkBoxRemember);
            panelForm.Controls.Add(panelPasswordBorder);
            panelForm.Controls.Add(labelPassword);
            panelForm.Controls.Add(panelUsernameBorder);
            panelForm.Controls.Add(labelUsername);
            panelForm.Controls.Add(labelSubtitle);
            panelForm.Controls.Add(labelTitle);
            panelForm.Dock = DockStyle.Fill;
            panelForm.Location = new Point(50, 100);
            panelForm.Margin = new Padding(3, 4, 3, 4);
            panelForm.Name = "panelForm";
            panelForm.Size = new Size(413, 619);
            panelForm.TabIndex = 0;
            // 
            // linkLabelRegister
            // 
            linkLabelRegister.ActiveLinkColor = Color.FromArgb(20, 140, 160);
            linkLabelRegister.AutoSize = true;
            linkLabelRegister.Font = new Font("Segoe UI", 9.5F);
            linkLabelRegister.LinkBehavior = LinkBehavior.HoverUnderline;
            linkLabelRegister.LinkColor = Color.FromArgb(23, 162, 184);
            linkLabelRegister.Location = new Point(95, 581);
            linkLabelRegister.Name = "linkLabelRegister";
            linkLabelRegister.Size = new Size(240, 21);
            linkLabelRegister.TabIndex = 10;
            linkLabelRegister.TabStop = true;
            linkLabelRegister.Text = "Chưa có tài khoản? Đăng ký ngay";
            // 
            // labelOr
            // 
            labelOr.Font = new Font("Segoe UI", 9F);
            labelOr.ForeColor = Color.Gray;
            labelOr.Location = new Point(0, 544);
            labelOr.Name = "labelOr";
            labelOr.Size = new Size(413, 25);
            labelOr.TabIndex = 9;
            labelOr.Text = "────────────  hoặc  ────────────";
            labelOr.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // linkLabelForgotPassword
            // 
            linkLabelForgotPassword.ActiveLinkColor = Color.FromArgb(20, 140, 160);
            linkLabelForgotPassword.AutoSize = true;
            linkLabelForgotPassword.Font = new Font("Segoe UI", 9F);
            linkLabelForgotPassword.LinkBehavior = LinkBehavior.HoverUnderline;
            linkLabelForgotPassword.LinkColor = Color.FromArgb(23, 162, 184);
            linkLabelForgotPassword.Location = new Point(280, 319);
            linkLabelForgotPassword.Name = "linkLabelForgotPassword";
            linkLabelForgotPassword.Size = new Size(116, 20);
            linkLabelForgotPassword.TabIndex = 8;
            linkLabelForgotPassword.TabStop = true;
            linkLabelForgotPassword.Text = "Quên mật khẩu?";
            // 
            // buttonLogin
            // 
            buttonLogin.BackColor = Color.FromArgb(23, 162, 184);
            buttonLogin.Cursor = Cursors.Hand;
            buttonLogin.FlatAppearance.BorderSize = 0;
            buttonLogin.FlatStyle = FlatStyle.Flat;
            buttonLogin.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            buttonLogin.ForeColor = Color.White;
            buttonLogin.Location = new Point(0, 444);
            buttonLogin.Margin = new Padding(3, 4, 3, 4);
            buttonLogin.Name = "buttonLogin";
            buttonLogin.Size = new Size(413, 62);
            buttonLogin.TabIndex = 7;
            buttonLogin.Text = "ĐĂNG NHẬP";
            buttonLogin.UseVisualStyleBackColor = false;
            // 
            // checkBoxRemember
            // 
            checkBoxRemember.AutoSize = true;
            checkBoxRemember.Font = new Font("Segoe UI", 9F);
            checkBoxRemember.ForeColor = Color.FromArgb(64, 64, 64);
            checkBoxRemember.Location = new Point(3, 319);
            checkBoxRemember.Margin = new Padding(3, 4, 3, 4);
            checkBoxRemember.Name = "checkBoxRemember";
            checkBoxRemember.Size = new Size(134, 24);
            checkBoxRemember.TabIndex = 6;
            checkBoxRemember.Text = "Nhớ đăng nhập";
            checkBoxRemember.UseVisualStyleBackColor = true;
            // 
            // panelPasswordBorder
            // 
            panelPasswordBorder.BackColor = Color.FromArgb(240, 240, 240);
            panelPasswordBorder.Controls.Add(textBoxPassword);
            panelPasswordBorder.Location = new Point(0, 262);
            panelPasswordBorder.Margin = new Padding(3, 4, 3, 4);
            panelPasswordBorder.Name = "panelPasswordBorder";
            panelPasswordBorder.Padding = new Padding(2);
            panelPasswordBorder.Size = new Size(413, 48);
            panelPasswordBorder.TabIndex = 5;
            // 
            // textBoxPassword
            // 
            textBoxPassword.BorderStyle = BorderStyle.None;
            textBoxPassword.Dock = DockStyle.Fill;
            textBoxPassword.Font = new Font("Segoe UI", 11F);
            textBoxPassword.Location = new Point(2, 2);
            textBoxPassword.Margin = new Padding(3, 4, 3, 4);
            textBoxPassword.Multiline = true;
            textBoxPassword.Name = "textBoxPassword";
            textBoxPassword.PasswordChar = '●';
            textBoxPassword.Size = new Size(409, 44);
            textBoxPassword.TabIndex = 0;
            // 
            // labelPassword
            // 
            labelPassword.AutoSize = true;
            labelPassword.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point, 0);
            labelPassword.ForeColor = Color.FromArgb(64, 64, 64);
            labelPassword.Location = new Point(0, 231);
            labelPassword.Name = "labelPassword";
            labelPassword.Size = new Size(82, 23);
            labelPassword.TabIndex = 4;
            labelPassword.Text = "Mật khẩu";
            // 
            // panelUsernameBorder
            // 
            panelUsernameBorder.BackColor = Color.FromArgb(240, 240, 240);
            panelUsernameBorder.Controls.Add(textBoxUsername);
            panelUsernameBorder.Location = new Point(0, 169);
            panelUsernameBorder.Margin = new Padding(3, 4, 3, 4);
            panelUsernameBorder.Name = "panelUsernameBorder";
            panelUsernameBorder.Padding = new Padding(2);
            panelUsernameBorder.Size = new Size(413, 50);
            panelUsernameBorder.TabIndex = 3;
            // 
            // textBoxUsername
            // 
            textBoxUsername.BorderStyle = BorderStyle.None;
            textBoxUsername.Dock = DockStyle.Fill;
            textBoxUsername.Font = new Font("Segoe UI", 11F);
            textBoxUsername.Location = new Point(2, 2);
            textBoxUsername.Margin = new Padding(3, 4, 3, 4);
            textBoxUsername.Multiline = true;
            textBoxUsername.Name = "textBoxUsername";
            textBoxUsername.Size = new Size(409, 46);
            textBoxUsername.TabIndex = 0;
            // 
            // labelUsername
            // 
            labelUsername.AutoSize = true;
            labelUsername.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point, 0);
            labelUsername.ForeColor = Color.FromArgb(64, 64, 64);
            labelUsername.Location = new Point(0, 138);
            labelUsername.Name = "labelUsername";
            labelUsername.Size = new Size(124, 23);
            labelUsername.TabIndex = 2;
            labelUsername.Text = "Tên đăng nhập";
            // 
            // labelSubtitle
            // 
            labelSubtitle.Font = new Font("Segoe UI", 9.5F);
            labelSubtitle.ForeColor = Color.Gray;
            labelSubtitle.Location = new Point(0, 62);
            labelSubtitle.Name = "labelSubtitle";
            labelSubtitle.Size = new Size(413, 56);
            labelSubtitle.TabIndex = 1;
            labelSubtitle.Text = "Vui lòng đăng nhập để sử dụng hệ thống quản lý đặt khám";
            labelSubtitle.TextAlign = ContentAlignment.TopCenter;
            // 
            // labelTitle
            // 
            labelTitle.Font = new Font("Segoe UI", 22F, FontStyle.Bold);
            labelTitle.ForeColor = Color.FromArgb(23, 162, 184);
            labelTitle.Location = new Point(0, 0);
            labelTitle.Name = "labelTitle";
            labelTitle.Size = new Size(413, 62);
            labelTitle.TabIndex = 0;
            labelTitle.Text = "ĐĂNG NHẬP";
            labelTitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // Login
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(933, 769);
            Controls.Add(panelRight);
            Controls.Add(panelLeft);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Margin = new Padding(3, 4, 3, 4);
            MaximizeBox = false;
            Name = "Login";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Đăng nhập hệ thống";
            panelLeft.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBoxLogo).EndInit();
            panelRight.ResumeLayout(false);
            panelForm.ResumeLayout(false);
            panelForm.PerformLayout();
            panelPasswordBorder.ResumeLayout(false);
            panelPasswordBorder.PerformLayout();
            panelUsernameBorder.ResumeLayout(false);
            panelUsernameBorder.PerformLayout();
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelLeft;
        private System.Windows.Forms.Label labelWelcome;
        private System.Windows.Forms.Label labelWelcomeSubtitle;
        private System.Windows.Forms.PictureBox pictureBoxLogo;
        private System.Windows.Forms.Panel panelRight;
        private System.Windows.Forms.Panel panelForm;
        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.Label labelSubtitle;
        private System.Windows.Forms.Label labelUsername;
        private System.Windows.Forms.Panel panelUsernameBorder;
        private System.Windows.Forms.TextBox textBoxUsername;
        private System.Windows.Forms.Panel panelPasswordBorder;
        private System.Windows.Forms.TextBox textBoxPassword;
        private System.Windows.Forms.Label labelPassword;
        private System.Windows.Forms.CheckBox checkBoxRemember;
        private System.Windows.Forms.Button buttonLogin;
        private System.Windows.Forms.LinkLabel linkLabelForgotPassword;
        private System.Windows.Forms.LinkLabel linkLabelRegister;
        private System.Windows.Forms.Label labelOr;
    }
}
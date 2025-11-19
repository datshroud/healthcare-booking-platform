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
            this.panelLeft = new System.Windows.Forms.Panel();
            this.labelWelcome = new System.Windows.Forms.Label();
            this.labelWelcomeSubtitle = new System.Windows.Forms.Label();
            this.pictureBoxLogo = new System.Windows.Forms.PictureBox();
            this.panelRight = new System.Windows.Forms.Panel();
            this.panelForm = new System.Windows.Forms.Panel();
            this.linkLabelRegister = new System.Windows.Forms.LinkLabel();
            this.labelOr = new System.Windows.Forms.Label();
            this.linkLabelForgotPassword = new System.Windows.Forms.LinkLabel();
            this.buttonLogin = new System.Windows.Forms.Button();
            this.checkBoxRemember = new System.Windows.Forms.CheckBox();
            this.panelPasswordBorder = new System.Windows.Forms.Panel();
            this.textBoxPassword = new System.Windows.Forms.TextBox();
            this.labelPassword = new System.Windows.Forms.Label();
            this.panelUsernameBorder = new System.Windows.Forms.Panel();
            this.textBoxUsername = new System.Windows.Forms.TextBox();
            this.labelUsername = new System.Windows.Forms.Label();
            this.labelSubtitle = new System.Windows.Forms.Label();
            this.labelTitle = new System.Windows.Forms.Label();
            this.panelLeft.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLogo)).BeginInit();
            this.panelRight.SuspendLayout();
            this.panelForm.SuspendLayout();
            this.panelPasswordBorder.SuspendLayout();
            this.panelUsernameBorder.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelLeft
            // 
            this.panelLeft.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(162)))), ((int)(((byte)(184)))));
            this.panelLeft.Controls.Add(this.labelWelcome);
            this.panelLeft.Controls.Add(this.labelWelcomeSubtitle);
            this.panelLeft.Controls.Add(this.pictureBoxLogo);
            this.panelLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelLeft.Location = new System.Drawing.Point(0, 0);
            this.panelLeft.Name = "panelLeft";
            this.panelLeft.Size = new System.Drawing.Size(420, 615);
            this.panelLeft.TabIndex = 0;
            // 
            // labelWelcome
            // 
            this.labelWelcome.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.labelWelcome.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.labelWelcome.ForeColor = System.Drawing.Color.White;
            this.labelWelcome.Location = new System.Drawing.Point(0, 485);
            this.labelWelcome.Name = "labelWelcome";
            this.labelWelcome.Size = new System.Drawing.Size(420, 40);
            this.labelWelcome.TabIndex = 2;
            this.labelWelcome.Text = "Chào mừng trở lại!";
            this.labelWelcome.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelWelcomeSubtitle
            // 
            this.labelWelcomeSubtitle.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.labelWelcomeSubtitle.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.labelWelcomeSubtitle.ForeColor = System.Drawing.Color.White;
            this.labelWelcomeSubtitle.Location = new System.Drawing.Point(0, 525);
            this.labelWelcomeSubtitle.Name = "labelWelcomeSubtitle";
            this.labelWelcomeSubtitle.Padding = new System.Windows.Forms.Padding(20, 0, 20, 30);
            this.labelWelcomeSubtitle.Size = new System.Drawing.Size(420, 90);
            this.labelWelcomeSubtitle.TabIndex = 1;
            this.labelWelcomeSubtitle.Text = "Đăng nhập để tiếp tục sử dụng\r\nHệ thống quản lý đặt khám BookingCare";
            this.labelWelcomeSubtitle.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // pictureBoxLogo
            // 
            this.pictureBoxLogo.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.pictureBoxLogo.Location = new System.Drawing.Point(85, 150);
            this.pictureBoxLogo.Name = "pictureBoxLogo";
            this.pictureBoxLogo.Size = new System.Drawing.Size(250, 250);
            this.pictureBoxLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxLogo.TabIndex = 0;
            this.pictureBoxLogo.TabStop = false;
            // 
            // panelRight
            // 
            this.panelRight.BackColor = System.Drawing.Color.White;
            this.panelRight.Controls.Add(this.panelForm);
            this.panelRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelRight.Location = new System.Drawing.Point(420, 0);
            this.panelRight.Name = "panelRight";
            this.panelRight.Padding = new System.Windows.Forms.Padding(50, 80, 50, 40);
            this.panelRight.Size = new System.Drawing.Size(513, 615);
            this.panelRight.TabIndex = 1;
            // 
            // panelForm
            // 
            this.panelForm.Controls.Add(this.linkLabelRegister);
            this.panelForm.Controls.Add(this.labelOr);
            this.panelForm.Controls.Add(this.linkLabelForgotPassword);
            this.panelForm.Controls.Add(this.buttonLogin);
            this.panelForm.Controls.Add(this.checkBoxRemember);
            this.panelForm.Controls.Add(this.panelPasswordBorder);
            this.panelForm.Controls.Add(this.labelPassword);
            this.panelForm.Controls.Add(this.panelUsernameBorder);
            this.panelForm.Controls.Add(this.labelUsername);
            this.panelForm.Controls.Add(this.labelSubtitle);
            this.panelForm.Controls.Add(this.labelTitle);
            this.panelForm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelForm.Location = new System.Drawing.Point(50, 80);
            this.panelForm.Name = "panelForm";
            this.panelForm.Size = new System.Drawing.Size(413, 495);
            this.panelForm.TabIndex = 0;
            // 
            // linkLabelRegister
            // 
            this.linkLabelRegister.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(140)))), ((int)(((byte)(160)))));
            this.linkLabelRegister.AutoSize = true;
            this.linkLabelRegister.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.linkLabelRegister.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.linkLabelRegister.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(162)))), ((int)(((byte)(184)))));
            this.linkLabelRegister.Location = new System.Drawing.Point(95, 465);
            this.linkLabelRegister.Name = "linkLabelRegister";
            this.linkLabelRegister.Size = new System.Drawing.Size(225, 21);
            this.linkLabelRegister.TabIndex = 10;
            this.linkLabelRegister.TabStop = true;
            this.linkLabelRegister.Text = "Chưa có tài khoản? Đăng ký ngay";
            // 
            // labelOr
            // 
            this.labelOr.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.labelOr.ForeColor = System.Drawing.Color.Gray;
            this.labelOr.Location = new System.Drawing.Point(0, 435);
            this.labelOr.Name = "labelOr";
            this.labelOr.Size = new System.Drawing.Size(413, 20);
            this.labelOr.TabIndex = 9;
            this.labelOr.Text = "────────────  hoặc  ────────────";
            this.labelOr.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // linkLabelForgotPassword
            // 
            this.linkLabelForgotPassword.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(140)))), ((int)(((byte)(160)))));
            this.linkLabelForgotPassword.AutoSize = true;
            this.linkLabelForgotPassword.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.linkLabelForgotPassword.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.linkLabelForgotPassword.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(162)))), ((int)(((byte)(184)))));
            this.linkLabelForgotPassword.Location = new System.Drawing.Point(280, 255);
            this.linkLabelForgotPassword.Name = "linkLabelForgotPassword";
            this.linkLabelForgotPassword.Size = new System.Drawing.Size(115, 20);
            this.linkLabelForgotPassword.TabIndex = 8;
            this.linkLabelForgotPassword.TabStop = true;
            this.linkLabelForgotPassword.Text = "Quên mật khẩu?";
            // 
            // buttonLogin
            // 
            this.buttonLogin.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(162)))), ((int)(((byte)(184)))));
            this.buttonLogin.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonLogin.FlatAppearance.BorderSize = 0;
            this.buttonLogin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonLogin.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.buttonLogin.ForeColor = System.Drawing.Color.White;
            this.buttonLogin.Location = new System.Drawing.Point(0, 355);
            this.buttonLogin.Name = "buttonLogin";
            this.buttonLogin.Size = new System.Drawing.Size(413, 50);
            this.buttonLogin.TabIndex = 7;
            this.buttonLogin.Text = "ĐĂNG NHẬP";
            this.buttonLogin.UseVisualStyleBackColor = false;
            // 
            // checkBoxRemember
            // 
            this.checkBoxRemember.AutoSize = true;
            this.checkBoxRemember.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.checkBoxRemember.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.checkBoxRemember.Location = new System.Drawing.Point(3, 255);
            this.checkBoxRemember.Name = "checkBoxRemember";
            this.checkBoxRemember.Size = new System.Drawing.Size(139, 24);
            this.checkBoxRemember.TabIndex = 6;
            this.checkBoxRemember.Text = "Nhớ đăng nhập";
            this.checkBoxRemember.UseVisualStyleBackColor = true;
            // 
            // panelPasswordBorder
            // 
            this.panelPasswordBorder.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.panelPasswordBorder.Controls.Add(this.textBoxPassword);
            this.panelPasswordBorder.Location = new System.Drawing.Point(0, 210);
            this.panelPasswordBorder.Name = "panelPasswordBorder";
            this.panelPasswordBorder.Padding = new System.Windows.Forms.Padding(2);
            this.panelPasswordBorder.Size = new System.Drawing.Size(413, 38);
            this.panelPasswordBorder.TabIndex = 5;
            // 
            // textBoxPassword
            // 
            this.textBoxPassword.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBoxPassword.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxPassword.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.textBoxPassword.Location = new System.Drawing.Point(2, 2);
            this.textBoxPassword.Multiline = true;
            this.textBoxPassword.Name = "textBoxPassword";
            this.textBoxPassword.Padding = new System.Windows.Forms.Padding(10, 6, 10, 6);
            this.textBoxPassword.PasswordChar = '●';
            this.textBoxPassword.Size = new System.Drawing.Size(409, 34);
            this.textBoxPassword.TabIndex = 0;
            // 
            // labelPassword
            // 
            this.labelPassword.AutoSize = true;
            this.labelPassword.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelPassword.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.labelPassword.Location = new System.Drawing.Point(0, 185);
            this.labelPassword.Name = "labelPassword";
            this.labelPassword.Size = new System.Drawing.Size(82, 23);
            this.labelPassword.TabIndex = 4;
            this.labelPassword.Text = "Mật khẩu";
            // 
            // panelUsernameBorder
            // 
            this.panelUsernameBorder.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.panelUsernameBorder.Controls.Add(this.textBoxUsername);
            this.panelUsernameBorder.Location = new System.Drawing.Point(0, 135);
            this.panelUsernameBorder.Name = "panelUsernameBorder";
            this.panelUsernameBorder.Padding = new System.Windows.Forms.Padding(2);
            this.panelUsernameBorder.Size = new System.Drawing.Size(413, 38);
            this.panelUsernameBorder.TabIndex = 3;
            // 
            // textBoxUsername
            // 
            this.textBoxUsername.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBoxUsername.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxUsername.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.textBoxUsername.Location = new System.Drawing.Point(2, 2);
            this.textBoxUsername.Multiline = true;
            this.textBoxUsername.Name = "textBoxUsername";
            this.textBoxUsername.Padding = new System.Windows.Forms.Padding(10, 6, 10, 6);
            this.textBoxUsername.Size = new System.Drawing.Size(409, 34);
            this.textBoxUsername.TabIndex = 0;
            // 
            // labelUsername
            // 
            this.labelUsername.AutoSize = true;
            this.labelUsername.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelUsername.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.labelUsername.Location = new System.Drawing.Point(0, 110);
            this.labelUsername.Name = "labelUsername";
            this.labelUsername.Size = new System.Drawing.Size(124, 23);
            this.labelUsername.TabIndex = 2;
            this.labelUsername.Text = "Tên đăng nhập";
            // 
            // labelSubtitle
            // 
            this.labelSubtitle.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.labelSubtitle.ForeColor = System.Drawing.Color.Gray;
            this.labelSubtitle.Location = new System.Drawing.Point(0, 50);
            this.labelSubtitle.Name = "labelSubtitle";
            this.labelSubtitle.Size = new System.Drawing.Size(413, 45);
            this.labelSubtitle.TabIndex = 1;
            this.labelSubtitle.Text = "Vui lòng đăng nhập để sử dụng hệ thống quản lý đặt khám";
            this.labelSubtitle.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // labelTitle
            // 
            this.labelTitle.Font = new System.Drawing.Font("Segoe UI", 22F, System.Drawing.FontStyle.Bold);
            this.labelTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(162)))), ((int)(((byte)(184)))));
            this.labelTitle.Location = new System.Drawing.Point(0, 0);
            this.labelTitle.Name = "labelTitle";
            this.labelTitle.Size = new System.Drawing.Size(413, 50);
            this.labelTitle.TabIndex = 0;
            this.labelTitle.Text = "ĐĂNG NHẬP";
            this.labelTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Login
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(933, 615);
            this.Controls.Add(this.panelRight);
            this.Controls.Add(this.panelLeft);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Login";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Đăng nhập hệ thống";
            this.panelLeft.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLogo)).EndInit();
            this.panelRight.ResumeLayout(false);
            this.panelForm.ResumeLayout(false);
            this.panelForm.PerformLayout();
            this.panelPasswordBorder.ResumeLayout(false);
            this.panelPasswordBorder.PerformLayout();
            this.panelUsernameBorder.ResumeLayout(false);
            this.panelUsernameBorder.PerformLayout();
            this.ResumeLayout(false);

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
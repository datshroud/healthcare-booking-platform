namespace BookingCareManagement.WinForms
{
    partial class MainForm
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
                sidebarPanel = new Panel();
                navbarPanel = new Panel();
                contentPanel = new Panel();
                sidebarPanel.SuspendLayout();
                SuspendLayout();
                // 
                // sidebarPanel
                // 
                sidebarPanel.AllowDrop = true;
                sidebarPanel.AutoScroll = true;
                sidebarPanel.BackColor = Color.FromArgb(15, 23, 42);
                sidebarPanel.Dock = DockStyle.Left;
                sidebarPanel.Location = new Point(0, 0);
                sidebarPanel.Margin = new Padding(3, 4, 3, 4);
                sidebarPanel.Name = "sidebarPanel";
                sidebarPanel.Padding = new Padding(0, 12, 0, 0);
                sidebarPanel.Size = new Size(250, 1000);
                sidebarPanel.TabIndex = 0;
                // 
                // navbarPanel
                // 
                navbarPanel.BackColor = Color.White;
                navbarPanel.Dock = DockStyle.Top;
                navbarPanel.Location = new Point(250, 0);
                navbarPanel.Margin = new Padding(3, 4, 3, 4);
                navbarPanel.Name = "navbarPanel";
                navbarPanel.Size = new Size(1150, 75);
                navbarPanel.TabIndex = 1;
                // 
                // contentPanel
                // 
                contentPanel.AutoScroll = true;
                contentPanel.BackColor = Color.FromArgb(240, 242, 245);
                contentPanel.Dock = DockStyle.Fill;
                contentPanel.Location = new Point(250, 75);
                contentPanel.Margin = new Padding(3, 4, 3, 4);
                contentPanel.Name = "contentPanel";
                contentPanel.Padding = new Padding(20, 25, 20, 25);
                contentPanel.Size = new Size(1150, 925);
                contentPanel.TabIndex = 2;
                // 
                // MainForm
                // 
                AutoScaleDimensions = new SizeF(8F, 20F);
                AutoScaleMode = AutoScaleMode.Font;
                BackColor = Color.FromArgb(240, 242, 245);
                ClientSize = new Size(1400, 1000);
                Controls.Add(contentPanel);
                Controls.Add(navbarPanel);
                Controls.Add(sidebarPanel);
                Margin = new Padding(3, 4, 3, 4);
                MinimumSize = new Size(1000, 738);
                Name = "MainForm";
                StartPosition = FormStartPosition.CenterScreen;
                Text = "Booking Website";
                WindowState = FormWindowState.Maximized;
                sidebarPanel.ResumeLayout(false);
                ResumeLayout(false);
            }


            #endregion  

    }
}
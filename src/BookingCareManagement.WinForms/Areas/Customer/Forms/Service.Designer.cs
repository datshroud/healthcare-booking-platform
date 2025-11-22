namespace BookingCareManagement.WinForms.Areas.Customer.Forms
{
    partial class Service
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
            panelTop = new Panel();
            pictureBoxSearch = new PictureBox();
            textBoxSearch = new TextBox();
            labelTitle = new Label();
            flowLayoutPanelServices = new FlowLayoutPanel();
            panelTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxSearch).BeginInit();
            SuspendLayout();
            // 
            // panelTop
            // 
            panelTop.BackColor = Color.FromArgb(23, 162, 184);
            panelTop.Controls.Add(pictureBoxSearch);
            panelTop.Controls.Add(textBoxSearch);
            panelTop.Controls.Add(labelTitle);
            panelTop.Dock = DockStyle.Top;
            panelTop.Location = new Point(0, 0);
            panelTop.Margin = new Padding(4, 5, 4, 5);
            panelTop.Name = "panelTop";
            panelTop.Padding = new Padding(27, 15, 27, 15);
            panelTop.Size = new Size(1600, 108);
            panelTop.TabIndex = 0;
            // 
            // pictureBoxSearch
            // 
            pictureBoxSearch.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            pictureBoxSearch.Location = new Point(1187, 39);
            pictureBoxSearch.Margin = new Padding(4, 5, 4, 5);
            pictureBoxSearch.Name = "pictureBoxSearch";
            pictureBoxSearch.Size = new Size(27, 31);
            pictureBoxSearch.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxSearch.TabIndex = 5;
            pictureBoxSearch.TabStop = false;
            // 
            // textBoxSearch
            // 
            textBoxSearch.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            textBoxSearch.BorderStyle = BorderStyle.FixedSingle;
            textBoxSearch.Font = new Font("Segoe UI", 11F);
            textBoxSearch.ForeColor = Color.Black;
            textBoxSearch.Location = new Point(1227, 34);
            textBoxSearch.Margin = new Padding(4, 5, 4, 5);
            textBoxSearch.Name = "textBoxSearch";
            textBoxSearch.Size = new Size(333, 32);
            textBoxSearch.TabIndex = 4;
            textBoxSearch.Text = "";
            // 
            // labelTitle
            // 
            labelTitle.AutoSize = true;
            labelTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            labelTitle.ForeColor = Color.White;
            labelTitle.Location = new Point(27, 31);
            labelTitle.Margin = new Padding(4, 0, 4, 0);
            labelTitle.Name = "labelTitle";
            labelTitle.Size = new Size(112, 37);
            labelTitle.TabIndex = 0;
            labelTitle.Text = "Dịch vụ";
            // 
            // flowLayoutPanelServices
            // 
            flowLayoutPanelServices.AutoScroll = true;
            flowLayoutPanelServices.BackColor = Color.FromArgb(248, 249, 250);
            flowLayoutPanelServices.Dock = DockStyle.Fill;
            flowLayoutPanelServices.Location = new Point(0, 108);
            flowLayoutPanelServices.Margin = new Padding(4, 5, 4, 5);
            flowLayoutPanelServices.Name = "flowLayoutPanelServices";
            flowLayoutPanelServices.Padding = new Padding(27, 31, 27, 31);
            flowLayoutPanelServices.Size = new Size(1600, 947);
            flowLayoutPanelServices.TabIndex = 1;
            // 
            // Service
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(1600, 1055);
            Controls.Add(flowLayoutPanelServices);
            Controls.Add(panelTop);
            Margin = new Padding(4, 5, 4, 5);
            Name = "Service";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Quản lý dịch vụ";
            panelTop.ResumeLayout(false);
            panelTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxSearch).EndInit();
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.TextBox textBoxSearch;
        private System.Windows.Forms.PictureBox pictureBoxSearch;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelServices;
    }
}
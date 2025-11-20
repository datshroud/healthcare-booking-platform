namespace BookingCareManagement.WinForms.Areas.Admin.Forms
{
    partial class InvoiceReportForm
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
            mainPanel = new Panel();
            totalPanel = new Panel();
            lblTotal = new Label();
            lblTotalLabel = new Label();
            rowPanel = new Panel();
            lblAmount = new Label();
            txtServicePrice = new TextBox();
            numServiceQty = new NumericUpDown();
            txtServiceName = new TextBox();
            headerPanel = new Panel();
            lblAmountHeader = new Label();
            lblPriceHeader = new Label();
            lblQtyHeader = new Label();
            lblItemHeader = new Label();
            dtpInvoiceDate = new DateTimePicker();
            lblDateLabel = new Label();
            txtInvoiceNumber = new TextBox();
            lblInvoiceNoLabel = new Label();
            txtCustomerEmail = new TextBox();
            lblCustomerEmailLabel = new Label();
            txtCustomerName = new TextBox();
            lblCustomerNameLabel = new Label();
            blueLine = new Panel();
            lblTitle = new Label();
            lblLogo = new Label();
            btnDownloadPdf = new Button();
            btnPrint = new Button();
            btnClose = new Button();
            mainPanel.SuspendLayout();
            totalPanel.SuspendLayout();
            rowPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numServiceQty).BeginInit();
            headerPanel.SuspendLayout();
            SuspendLayout();
            // 
            // mainPanel
            // 
            mainPanel.BackColor = Color.White;
            mainPanel.BorderStyle = BorderStyle.FixedSingle;
            mainPanel.Controls.Add(totalPanel);
            mainPanel.Controls.Add(rowPanel);
            mainPanel.Controls.Add(headerPanel);
            mainPanel.Controls.Add(dtpInvoiceDate);
            mainPanel.Controls.Add(lblDateLabel);
            mainPanel.Controls.Add(txtInvoiceNumber);
            mainPanel.Controls.Add(lblInvoiceNoLabel);
            mainPanel.Controls.Add(txtCustomerEmail);
            mainPanel.Controls.Add(lblCustomerEmailLabel);
            mainPanel.Controls.Add(txtCustomerName);
            mainPanel.Controls.Add(lblCustomerNameLabel);
            mainPanel.Controls.Add(blueLine);
            mainPanel.Controls.Add(lblTitle);
            mainPanel.Controls.Add(lblLogo);
            mainPanel.Location = new Point(46, 53);
            mainPanel.Margin = new Padding(3, 4, 3, 4);
            mainPanel.Name = "mainPanel";
            mainPanel.Size = new Size(914, 733);
            mainPanel.TabIndex = 0;
            // 
            // totalPanel
            // 
            totalPanel.BackColor = Color.FromArgb(237, 245, 255);
            totalPanel.Controls.Add(lblTotal);
            totalPanel.Controls.Add(lblTotalLabel);
            totalPanel.Location = new Point(34, 480);
            totalPanel.Margin = new Padding(3, 4, 3, 4);
            totalPanel.Name = "totalPanel";
            totalPanel.Size = new Size(846, 67);
            totalPanel.TabIndex = 13;
            // 
            // lblTotal
            // 
            lblTotal.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblTotal.Location = new Point(674, 20);
            lblTotal.Name = "lblTotal";
            lblTotal.Size = new Size(154, 27);
            lblTotal.TabIndex = 1;
            lblTotal.Text = "0 ₫";
            lblTotal.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblTotalLabel
            // 
            lblTotalLabel.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblTotalLabel.Location = new Point(491, 20);
            lblTotalLabel.Name = "lblTotalLabel";
            lblTotalLabel.Size = new Size(160, 27);
            lblTotalLabel.TabIndex = 0;
            lblTotalLabel.Text = "Tổng cùng";
            lblTotalLabel.TextAlign = ContentAlignment.MiddleRight;
            // 
            // rowPanel
            // 
            rowPanel.BackColor = Color.FromArgb(248, 249, 250);
            rowPanel.Controls.Add(lblAmount);
            rowPanel.Controls.Add(txtServicePrice);
            rowPanel.Controls.Add(numServiceQty);
            rowPanel.Controls.Add(txtServiceName);
            rowPanel.Location = new Point(34, 400);
            rowPanel.Margin = new Padding(3, 4, 3, 4);
            rowPanel.Name = "rowPanel";
            rowPanel.Size = new Size(846, 53);
            rowPanel.TabIndex = 12;
            // 
            // lblAmount
            // 
            lblAmount.Font = new Font("Segoe UI", 10F);
            lblAmount.Location = new Point(674, 13);
            lblAmount.Name = "lblAmount";
            lblAmount.Size = new Size(154, 27);
            lblAmount.TabIndex = 3;
            lblAmount.Text = "0 ₫";
            lblAmount.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtServicePrice
            // 
            txtServicePrice.BorderStyle = BorderStyle.FixedSingle;
            txtServicePrice.Font = new Font("Segoe UI", 10F);
            txtServicePrice.Location = new Point(491, 13);
            txtServicePrice.Margin = new Padding(3, 4, 3, 4);
            txtServicePrice.Name = "txtServicePrice";
            txtServicePrice.Size = new Size(160, 30);
            txtServicePrice.TabIndex = 2;
            txtServicePrice.TextAlign = HorizontalAlignment.Center;
            txtServicePrice.TextChanged += txtServicePrice_TextChanged;
            // 
            // numServiceQty
            // 
            numServiceQty.Font = new Font("Segoe UI", 10F);
            numServiceQty.Location = new Point(331, 13);
            numServiceQty.Margin = new Padding(3, 4, 3, 4);
            numServiceQty.Maximum = new decimal(new int[] { 999, 0, 0, 0 });
            numServiceQty.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numServiceQty.Name = "numServiceQty";
            numServiceQty.Size = new Size(137, 30);
            numServiceQty.TabIndex = 1;
            numServiceQty.TextAlign = HorizontalAlignment.Center;
            numServiceQty.Value = new decimal(new int[] { 1, 0, 0, 0 });
            numServiceQty.ValueChanged += numServiceQty_ValueChanged;
            // 
            // txtServiceName
            // 
            txtServiceName.BorderStyle = BorderStyle.FixedSingle;
            txtServiceName.Font = new Font("Segoe UI", 10F);
            txtServiceName.Location = new Point(17, 13);
            txtServiceName.Margin = new Padding(3, 4, 3, 4);
            txtServiceName.Name = "txtServiceName";
            txtServiceName.Size = new Size(285, 30);
            txtServiceName.TabIndex = 0;
            // 
            // headerPanel
            // 
            headerPanel.BackColor = Color.FromArgb(237, 245, 255);
            headerPanel.Controls.Add(lblAmountHeader);
            headerPanel.Controls.Add(lblPriceHeader);
            headerPanel.Controls.Add(lblQtyHeader);
            headerPanel.Controls.Add(lblItemHeader);
            headerPanel.Location = new Point(34, 347);
            headerPanel.Margin = new Padding(3, 4, 3, 4);
            headerPanel.Name = "headerPanel";
            headerPanel.Size = new Size(846, 53);
            headerPanel.TabIndex = 11;
            // 
            // lblAmountHeader
            // 
            lblAmountHeader.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblAmountHeader.Location = new Point(674, 13);
            lblAmountHeader.Name = "lblAmountHeader";
            lblAmountHeader.Size = new Size(154, 27);
            lblAmountHeader.TabIndex = 3;
            lblAmountHeader.Text = "Thành tiền";
            lblAmountHeader.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblPriceHeader
            // 
            lblPriceHeader.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblPriceHeader.Location = new Point(491, 13);
            lblPriceHeader.Name = "lblPriceHeader";
            lblPriceHeader.Size = new Size(160, 27);
            lblPriceHeader.TabIndex = 2;
            lblPriceHeader.Text = "Đơn giá";
            lblPriceHeader.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblQtyHeader
            // 
            lblQtyHeader.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblQtyHeader.Location = new Point(331, 13);
            lblQtyHeader.Name = "lblQtyHeader";
            lblQtyHeader.Size = new Size(137, 27);
            lblQtyHeader.TabIndex = 1;
            lblQtyHeader.Text = "Số lượng";
            lblQtyHeader.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblItemHeader
            // 
            lblItemHeader.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblItemHeader.Location = new Point(17, 13);
            lblItemHeader.Name = "lblItemHeader";
            lblItemHeader.Size = new Size(286, 27);
            lblItemHeader.TabIndex = 0;
            lblItemHeader.Text = "Mặt hàng";
            // 
            // dtpInvoiceDate
            // 
            dtpInvoiceDate.CustomFormat = "MMMM dd, yyyy";
            dtpInvoiceDate.Font = new Font("Segoe UI", 9F);
            dtpInvoiceDate.Format = DateTimePickerFormat.Custom;
            dtpInvoiceDate.Location = new Point(720, 280);
            dtpInvoiceDate.Margin = new Padding(3, 4, 3, 4);
            dtpInvoiceDate.Name = "dtpInvoiceDate";
            dtpInvoiceDate.Size = new Size(159, 27);
            dtpInvoiceDate.TabIndex = 10;
            // 
            // lblDateLabel
            // 
            lblDateLabel.Font = new Font("Segoe UI", 9F);
            lblDateLabel.ForeColor = Color.FromArgb(108, 117, 125);
            lblDateLabel.Location = new Point(571, 280);
            lblDateLabel.Name = "lblDateLabel";
            lblDateLabel.Size = new Size(137, 33);
            lblDateLabel.TabIndex = 9;
            lblDateLabel.Text = "Ngày";
            lblDateLabel.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtInvoiceNumber
            // 
            txtInvoiceNumber.BorderStyle = BorderStyle.FixedSingle;
            txtInvoiceNumber.Font = new Font("Segoe UI", 9F);
            txtInvoiceNumber.Location = new Point(720, 240);
            txtInvoiceNumber.Margin = new Padding(3, 4, 3, 4);
            txtInvoiceNumber.Name = "txtInvoiceNumber";
            txtInvoiceNumber.Size = new Size(160, 27);
            txtInvoiceNumber.TabIndex = 8;
            txtInvoiceNumber.TextAlign = HorizontalAlignment.Right;
            // 
            // lblInvoiceNoLabel
            // 
            lblInvoiceNoLabel.Font = new Font("Segoe UI", 9F);
            lblInvoiceNoLabel.ForeColor = Color.FromArgb(108, 117, 125);
            lblInvoiceNoLabel.Location = new Point(571, 240);
            lblInvoiceNoLabel.Name = "lblInvoiceNoLabel";
            lblInvoiceNoLabel.Size = new Size(137, 27);
            lblInvoiceNoLabel.TabIndex = 7;
            lblInvoiceNoLabel.Text = "Hóa đơn #:";
            lblInvoiceNoLabel.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtCustomerEmail
            // 
            txtCustomerEmail.BorderStyle = BorderStyle.FixedSingle;
            txtCustomerEmail.Font = new Font("Segoe UI", 9F);
            txtCustomerEmail.Location = new Point(720, 193);
            txtCustomerEmail.Margin = new Padding(3, 4, 3, 4);
            txtCustomerEmail.Name = "txtCustomerEmail";
            txtCustomerEmail.Size = new Size(160, 27);
            txtCustomerEmail.TabIndex = 6;
            txtCustomerEmail.TextAlign = HorizontalAlignment.Right;
            // 
            // lblCustomerEmailLabel
            // 
            lblCustomerEmailLabel.Font = new Font("Segoe UI", 9F);
            lblCustomerEmailLabel.Location = new Point(571, 193);
            lblCustomerEmailLabel.Name = "lblCustomerEmailLabel";
            lblCustomerEmailLabel.Size = new Size(137, 27);
            lblCustomerEmailLabel.TabIndex = 5;
            lblCustomerEmailLabel.Text = "Email:";
            lblCustomerEmailLabel.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtCustomerName
            // 
            txtCustomerName.BorderStyle = BorderStyle.FixedSingle;
            txtCustomerName.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            txtCustomerName.Location = new Point(720, 147);
            txtCustomerName.Margin = new Padding(3, 4, 3, 4);
            txtCustomerName.Name = "txtCustomerName";
            txtCustomerName.Size = new Size(160, 32);
            txtCustomerName.TabIndex = 4;
            txtCustomerName.TextAlign = HorizontalAlignment.Right;
            // 
            // lblCustomerNameLabel
            // 
            lblCustomerNameLabel.Font = new Font("Segoe UI", 9F);
            lblCustomerNameLabel.Location = new Point(571, 147);
            lblCustomerNameLabel.Name = "lblCustomerNameLabel";
            lblCustomerNameLabel.Size = new Size(137, 33);
            lblCustomerNameLabel.TabIndex = 3;
            lblCustomerNameLabel.Text = "Tên khách hàng:";
            lblCustomerNameLabel.TextAlign = ContentAlignment.MiddleRight;
            // 
            // blueLine
            // 
            blueLine.BackColor = Color.FromArgb(13, 110, 253);
            blueLine.Location = new Point(34, 107);
            blueLine.Margin = new Padding(3, 4, 3, 4);
            blueLine.Name = "blueLine";
            blueLine.Size = new Size(846, 4);
            blueLine.TabIndex = 2;
            // 
            // lblTitle
            // 
            lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(33, 37, 41);
            lblTitle.Location = new Point(686, 40);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(194, 53);
            lblTitle.TabIndex = 1;
            lblTitle.Text = "Hóa đơn";
            lblTitle.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblLogo
            // 
            lblLogo.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblLogo.ForeColor = Color.FromArgb(13, 110, 253);
            lblLogo.Location = new Point(34, 40);
            lblLogo.Name = "lblLogo";
            lblLogo.Size = new Size(171, 53);
            lblLogo.TabIndex = 0;
            lblLogo.Text = "dental";
            // 
            // btnDownloadPdf
            // 
            btnDownloadPdf.BackColor = Color.FromArgb(40, 167, 69);
            btnDownloadPdf.Cursor = Cursors.Hand;
            btnDownloadPdf.FlatAppearance.BorderSize = 0;
            btnDownloadPdf.FlatStyle = FlatStyle.Flat;
            btnDownloadPdf.Font = new Font("Segoe UI", 10F);
            btnDownloadPdf.ForeColor = Color.White;
            btnDownloadPdf.Location = new Point(789, 800);
            btnDownloadPdf.Margin = new Padding(3, 4, 3, 4);
            btnDownloadPdf.Name = "btnDownloadPdf";
            btnDownloadPdf.Size = new Size(171, 53);
            btnDownloadPdf.TabIndex = 3;
            btnDownloadPdf.Text = "📥 Tải xuống PDF";
            btnDownloadPdf.UseVisualStyleBackColor = false;
            btnDownloadPdf.Click += BtnDownloadPdf_Click;
            // 
            // btnPrint
            // 
            btnPrint.BackColor = Color.FromArgb(13, 110, 253);
            btnPrint.Cursor = Cursors.Hand;
            btnPrint.FlatAppearance.BorderSize = 0;
            btnPrint.FlatStyle = FlatStyle.Flat;
            btnPrint.Font = new Font("Segoe UI", 10F);
            btnPrint.ForeColor = Color.White;
            btnPrint.Location = new Point(640, 800);
            btnPrint.Margin = new Padding(3, 4, 3, 4);
            btnPrint.Name = "btnPrint";
            btnPrint.Size = new Size(137, 53);
            btnPrint.TabIndex = 2;
            btnPrint.Text = "🖨️ In hóa đơn";
            btnPrint.UseVisualStyleBackColor = false;
            btnPrint.Click += BtnPrint_Click;
            // 
            // btnClose
            // 
            btnClose.BackColor = Color.FromArgb(108, 117, 125);
            btnClose.Cursor = Cursors.Hand;
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.Font = new Font("Segoe UI", 10F);
            btnClose.ForeColor = Color.White;
            btnClose.Location = new Point(491, 800);
            btnClose.Margin = new Padding(3, 4, 3, 4);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(137, 53);
            btnClose.TabIndex = 1;
            btnClose.Text = "❌ Đóng";
            btnClose.UseVisualStyleBackColor = false;
            btnClose.Click += btnClose_Click;
            // 
            // InvoiceReportForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(248, 249, 250);
            ClientSize = new Size(1029, 933);
            Controls.Add(btnDownloadPdf);
            Controls.Add(btnPrint);
            Controls.Add(btnClose);
            Controls.Add(mainPanel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Margin = new Padding(3, 4, 3, 4);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "InvoiceReportForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Invoice Report";
            mainPanel.ResumeLayout(false);
            mainPanel.PerformLayout();
            totalPanel.ResumeLayout(false);
            rowPanel.ResumeLayout(false);
            rowPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numServiceQty).EndInit();
            headerPanel.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel mainPanel;
        private System.Windows.Forms.Label lblLogo;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Panel blueLine;
        private System.Windows.Forms.Label lblCustomerNameLabel;
        private System.Windows.Forms.TextBox txtCustomerName;
        private System.Windows.Forms.TextBox txtCustomerEmail;
        private System.Windows.Forms.Label lblCustomerEmailLabel;
        private System.Windows.Forms.TextBox txtInvoiceNumber;
        private System.Windows.Forms.Label lblInvoiceNoLabel;
        private System.Windows.Forms.DateTimePicker dtpInvoiceDate;
        private System.Windows.Forms.Label lblDateLabel;
        private System.Windows.Forms.Panel headerPanel;
        private System.Windows.Forms.Label lblItemHeader;
        private System.Windows.Forms.Label lblQtyHeader;
        private System.Windows.Forms.Label lblPriceHeader;
        private System.Windows.Forms.Label lblAmountHeader;
        private System.Windows.Forms.Panel rowPanel;
        private System.Windows.Forms.TextBox txtServiceName;
        private System.Windows.Forms.NumericUpDown numServiceQty;
        private System.Windows.Forms.TextBox txtServicePrice;
        private System.Windows.Forms.Label lblAmount;
        private System.Windows.Forms.Panel totalPanel;
        private System.Windows.Forms.Label lblTotalLabel;
        private System.Windows.Forms.Label lblTotal;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnPrint;
        private System.Windows.Forms.Button btnDownloadPdf;
    }
}

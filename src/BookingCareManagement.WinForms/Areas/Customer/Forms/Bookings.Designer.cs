namespace BookingCareManagement.WinForms.Areas.Customer.Forms
{
    partial class Bookings
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
            panelHeader = new Panel();
            labelHeader = new Label();
            panelLeft = new Panel();
            panelLeftContent = new Panel();
            panelThankYou = new Panel();
            labelThankYouMessage = new Label();
            labelThankYouTitle = new Label();
            buttonBackToStart = new Button();
            panelPayment = new Panel();
            buttonConfirmBooking = new Button();
            textBoxPhone = new TextBox();
            labelPhone = new Label();
            textBoxName = new TextBox();
            labelName = new Label();
            buttonApplyPromo = new Button();
            textBoxPromoCode = new TextBox();
            labelPromoCode = new Label();
            labelPaymentNote = new Label();
            labelCheckoutTotal = new Label();
            labelTotalPayment = new Label();
            labelPaymentTitle = new Label();
            panelDateTime = new Panel();
            buttonConfirmDateTime = new Button();
            comboBoxTimeSlot = new ComboBox();
            labelTimeSlot = new Label();
            dateTimePickerAppointment = new DateTimePicker();
            labelDate = new Label();
            panelEmployee = new Panel();
            flowLayoutPanelEmployees = new FlowLayoutPanel();
            panelSpecialty = new Panel();
            flowLayoutPanelSpecialties = new FlowLayoutPanel();
            textBoxSearch = new TextBox();
            panelLeftHeader = new Panel();
            labelLeftTitle = new Label();
            buttonBack = new Button();
            panelRight = new Panel();
            panelRightContent = new Panel();
            panelTotalSection = new Panel();
            labelTotalPrice = new Label();
            labelTotalLabel = new Label();
            panelSelectedDateTime = new Panel();
            labelSelectedDateTimeValue = new Label();
            labelSelectedDateTimeLabel = new Label();
            panelSelectedEmployee = new Panel();
            labelSelectedEmployeeValue = new Label();
            labelSelectedEmployeeLabel = new Label();
            panelSelectedSpecialty = new Panel();
            labelSelectedSpecialtyValue = new Label();
            labelSelectedSpecialtyLabel = new Label();
            labelRightTitle = new Label();
            panelHeader.SuspendLayout();
            panelLeft.SuspendLayout();
            panelLeftContent.SuspendLayout();
            panelThankYou.SuspendLayout();
            panelPayment.SuspendLayout();
            panelDateTime.SuspendLayout();
            panelEmployee.SuspendLayout();
            panelSpecialty.SuspendLayout();
            panelLeftHeader.SuspendLayout();
            panelRight.SuspendLayout();
            panelRightContent.SuspendLayout();
            panelTotalSection.SuspendLayout();
            panelSelectedDateTime.SuspendLayout();
            panelSelectedEmployee.SuspendLayout();
            panelSelectedSpecialty.SuspendLayout();
            SuspendLayout();
            // 
            // panelHeader
            // 
            panelHeader.BackColor = Color.FromArgb(23, 162, 184);
            panelHeader.Controls.Add(labelHeader);
            panelHeader.Dock = DockStyle.Top;
            panelHeader.Location = new Point(0, 0);
            panelHeader.Margin = new Padding(4, 5, 4, 5);
            panelHeader.Name = "panelHeader";
            panelHeader.Size = new Size(1653, 110);
            panelHeader.TabIndex = 2;
            // 
            // labelHeader
            // 
            labelHeader.AutoSize = true;
            labelHeader.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            labelHeader.ForeColor = Color.White;
            labelHeader.Location = new Point(40, 38);
            labelHeader.Margin = new Padding(4, 0, 4, 0);
            labelHeader.Name = "labelHeader";
            labelHeader.Size = new Size(141, 46);
            labelHeader.TabIndex = 0;
            labelHeader.Text = "Đặt lịch";
            // 
            // panelLeft
            // 
            panelLeft.Anchor = AnchorStyles.None;
            panelLeft.AutoScroll = true;
            panelLeft.BackColor = Color.FromArgb(52, 58, 64);
            panelLeft.Controls.Add(panelLeftContent);
            panelLeft.Controls.Add(textBoxSearch);
            panelLeft.Controls.Add(panelLeftHeader);
            panelLeft.Location = new Point(80, 240);
            panelLeft.Margin = new Padding(4, 5, 4, 5);
            panelLeft.Name = "panelLeft";
            panelLeft.Size = new Size(867, 800);
            panelLeft.TabIndex = 0;
            // 
            // panelLeftContent
            // 
            panelLeftContent.AutoScroll = true;
            panelLeftContent.Controls.Add(panelThankYou);
            panelLeftContent.Controls.Add(panelPayment);
            panelLeftContent.Controls.Add(panelDateTime);
            panelLeftContent.Controls.Add(panelEmployee);
            panelLeftContent.Controls.Add(panelSpecialty);
            panelLeftContent.Location = new Point(27, 200);
            panelLeftContent.Margin = new Padding(4, 5, 4, 5);
            panelLeftContent.Name = "panelLeftContent";
            panelLeftContent.Size = new Size(813, 580);
            panelLeftContent.TabIndex = 2;
            // 
            // panelThankYou
            // 
            panelThankYou.Controls.Add(labelThankYouMessage);
            panelThankYou.Controls.Add(labelThankYouTitle);
            panelThankYou.Controls.Add(buttonBackToStart);
            panelThankYou.Dock = DockStyle.Fill;
            panelThankYou.Location = new Point(0, 0);
            panelThankYou.Margin = new Padding(4, 5, 4, 5);
            panelThankYou.Name = "panelThankYou";
            panelThankYou.Size = new Size(813, 580);
            panelThankYou.TabIndex = 4;
            panelThankYou.Visible = false;
            // 
            // labelThankYouMessage
            // 
            labelThankYouMessage.Font = new Font("Segoe UI", 13F);
            labelThankYouMessage.ForeColor = Color.White;
            labelThankYouMessage.Location = new Point(0, 220);
            labelThankYouMessage.Margin = new Padding(4, 0, 4, 0);
            labelThankYouMessage.Name = "labelThankYouMessage";
            labelThankYouMessage.Size = new Size(813, 100);
            labelThankYouMessage.TabIndex = 1;
            labelThankYouMessage.Text = "Đặt lịch thành công!\nCảm ơn bạn đã tin tưởng, và chúng tôi mong được gặp lại bạn!";
            labelThankYouMessage.TextAlign = ContentAlignment.TopCenter;
            // 
            // labelThankYouTitle
            // 
            labelThankYouTitle.Font = new Font("Segoe UI", 24F, FontStyle.Bold);
            labelThankYouTitle.ForeColor = Color.White;
            labelThankYouTitle.Location = new Point(0, 150);
            labelThankYouTitle.Margin = new Padding(4, 0, 4, 0);
            labelThankYouTitle.Name = "labelThankYouTitle";
            labelThankYouTitle.Size = new Size(813, 60);
            labelThankYouTitle.TabIndex = 0;
            labelThankYouTitle.Text = "Cảm ơn!";
            labelThankYouTitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // buttonBackToStart
            // 
            buttonBackToStart.BackColor = Color.FromArgb(255, 165, 0);
            buttonBackToStart.FlatAppearance.BorderSize = 0;
            buttonBackToStart.FlatStyle = FlatStyle.Flat;
            buttonBackToStart.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            buttonBackToStart.ForeColor = Color.White;
            buttonBackToStart.Location = new Point(200, 360);
            buttonBackToStart.Name = "buttonBackToStart";
            buttonBackToStart.Size = new Size(413, 60);
            buttonBackToStart.TabIndex = 2;
            buttonBackToStart.Text = "Đặt tiếp";
            buttonBackToStart.UseVisualStyleBackColor = false;
            // 
            // panelPayment
            // 
            panelPayment.AutoScroll = true;
            panelPayment.Controls.Add(buttonConfirmBooking);
            panelPayment.Controls.Add(textBoxPhone);
            panelPayment.Controls.Add(labelPhone);
            panelPayment.Controls.Add(textBoxName);
            panelPayment.Controls.Add(labelName);
            panelPayment.Controls.Add(buttonApplyPromo);
            panelPayment.Controls.Add(textBoxPromoCode);
            panelPayment.Controls.Add(labelPromoCode);
            panelPayment.Controls.Add(labelPaymentNote);
            panelPayment.Controls.Add(labelCheckoutTotal);
            panelPayment.Controls.Add(labelTotalPayment);
            panelPayment.Controls.Add(labelPaymentTitle);
            panelPayment.Dock = DockStyle.Fill;
            panelPayment.Location = new Point(0, 0);
            panelPayment.Margin = new Padding(4, 5, 4, 5);
            panelPayment.Name = "panelPayment";
            panelPayment.Size = new Size(813, 580);
            panelPayment.TabIndex = 3;
            panelPayment.Visible = false;
            // 
            // buttonConfirmBooking
            // 
            buttonConfirmBooking.BackColor = Color.FromArgb(255, 165, 0);
            buttonConfirmBooking.FlatAppearance.BorderSize = 0;
            buttonConfirmBooking.FlatStyle = FlatStyle.Flat;
            buttonConfirmBooking.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            buttonConfirmBooking.ForeColor = Color.White;
            buttonConfirmBooking.Location = new Point(13, 662);
            buttonConfirmBooking.Margin = new Padding(4, 5, 4, 5);
            buttonConfirmBooking.Name = "buttonConfirmBooking";
            buttonConfirmBooking.Size = new Size(773, 77);
            buttonConfirmBooking.TabIndex = 11;
            buttonConfirmBooking.Text = "Hoàn tất đặt lịch";
            buttonConfirmBooking.UseVisualStyleBackColor = false;
            // 
            // textBoxPhone
            // 
            textBoxPhone.BackColor = Color.White;
            textBoxPhone.BorderStyle = BorderStyle.FixedSingle;
            textBoxPhone.Font = new Font("Segoe UI", 11F);
            textBoxPhone.ForeColor = Color.Black;
            textBoxPhone.Location = new Point(13, 569);
            textBoxPhone.Margin = new Padding(4, 5, 4, 5);
            textBoxPhone.Name = "textBoxPhone";
            textBoxPhone.Size = new Size(773, 32);
            textBoxPhone.TabIndex = 10;
            // 
            // labelPhone
            // 
            labelPhone.AutoSize = true;
            labelPhone.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            labelPhone.ForeColor = Color.White;
            labelPhone.Location = new Point(13, 523);
            labelPhone.Margin = new Padding(4, 0, 4, 0);
            labelPhone.Name = "labelPhone";
            labelPhone.Size = new Size(129, 25);
            labelPhone.TabIndex = 9;
            labelPhone.Text = "Số điện thoại";
            // 
            // textBoxName
            // 
            textBoxName.BackColor = Color.White;
            textBoxName.BorderStyle = BorderStyle.FixedSingle;
            textBoxName.Font = new Font("Segoe UI", 11F);
            textBoxName.ForeColor = Color.Black;
            textBoxName.Location = new Point(13, 446);
            textBoxName.Margin = new Padding(4, 5, 4, 5);
            textBoxName.Name = "textBoxName";
            textBoxName.Size = new Size(773, 32);
            textBoxName.TabIndex = 8;
            // 
            // labelName
            // 
            labelName.AutoSize = true;
            labelName.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            labelName.ForeColor = Color.White;
            labelName.Location = new Point(13, 400);
            labelName.Margin = new Padding(4, 0, 4, 0);
            labelName.Name = "labelName";
            labelName.Size = new Size(98, 25);
            labelName.TabIndex = 7;
            labelName.Text = "Họ và tên";
            // 
            // buttonApplyPromo
            // 
            buttonApplyPromo.BackColor = Color.FromArgb(255, 165, 0);
            buttonApplyPromo.FlatAppearance.BorderSize = 0;
            buttonApplyPromo.FlatStyle = FlatStyle.Flat;
            buttonApplyPromo.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            buttonApplyPromo.ForeColor = Color.White;
            buttonApplyPromo.Location = new Point(600, 289);
            buttonApplyPromo.Margin = new Padding(4, 5, 4, 5);
            buttonApplyPromo.Name = "buttonApplyPromo";
            buttonApplyPromo.Size = new Size(187, 48);
            buttonApplyPromo.TabIndex = 6;
            buttonApplyPromo.Text = "Áp dụng";
            buttonApplyPromo.UseVisualStyleBackColor = false;
            // 
            // textBoxPromoCode
            // 
            textBoxPromoCode.BackColor = Color.White;
            textBoxPromoCode.BorderStyle = BorderStyle.FixedSingle;
            textBoxPromoCode.Font = new Font("Segoe UI", 11F);
            textBoxPromoCode.ForeColor = Color.Black;
            textBoxPromoCode.Location = new Point(13, 292);
            textBoxPromoCode.Margin = new Padding(4, 5, 4, 5);
            textBoxPromoCode.Name = "textBoxPromoCode";
            textBoxPromoCode.Size = new Size(573, 32);
            textBoxPromoCode.TabIndex = 5;
            // 
            // labelPromoCode
            // 
            labelPromoCode.AutoSize = true;
            labelPromoCode.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            labelPromoCode.ForeColor = Color.White;
            labelPromoCode.Location = new Point(13, 246);
            labelPromoCode.Margin = new Padding(4, 0, 4, 0);
            labelPromoCode.Name = "labelPromoCode";
            labelPromoCode.Size = new Size(193, 25);
            labelPromoCode.TabIndex = 4;
            labelPromoCode.Text = "Bạn có mã giảm giá?";
            // 
            // labelPaymentNote
            // 
            labelPaymentNote.Font = new Font("Segoe UI", 9F);
            labelPaymentNote.ForeColor = Color.Gray;
            labelPaymentNote.Location = new Point(13, 154);
            labelPaymentNote.Margin = new Padding(4, 0, 4, 0);
            labelPaymentNote.Name = "labelPaymentNote";
            labelPaymentNote.Size = new Size(773, 62);
            labelPaymentNote.TabIndex = 3;
            labelPaymentNote.Text = "Thanh toán sẽ được thực hiện tại địa điểm đặt lịch.";
            // 
            // labelCheckoutTotal
            // 
            labelCheckoutTotal.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            labelCheckoutTotal.ForeColor = Color.FromArgb(255, 165, 0);
            labelCheckoutTotal.Location = new Point(533, 85);
            labelCheckoutTotal.Margin = new Padding(4, 0, 4, 0);
            labelCheckoutTotal.Name = "labelCheckoutTotal";
            labelCheckoutTotal.Size = new Size(253, 46);
            labelCheckoutTotal.TabIndex = 2;
            labelCheckoutTotal.Text = "0 VNĐ";
            labelCheckoutTotal.TextAlign = ContentAlignment.MiddleRight;
            // 
            // labelTotalPayment
            // 
            labelTotalPayment.AutoSize = true;
            labelTotalPayment.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            labelTotalPayment.ForeColor = Color.White;
            labelTotalPayment.Location = new Point(13, 92);
            labelTotalPayment.Margin = new Padding(4, 0, 4, 0);
            labelTotalPayment.Name = "labelTotalPayment";
            labelTotalPayment.Size = new Size(98, 25);
            labelTotalPayment.TabIndex = 1;
            labelTotalPayment.Text = "Tổng tiền";
            // 
            // labelPaymentTitle
            // 
            labelPaymentTitle.AutoSize = true;
            labelPaymentTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            labelPaymentTitle.ForeColor = Color.White;
            labelPaymentTitle.Location = new Point(13, 15);
            labelPaymentTitle.Margin = new Padding(4, 0, 4, 0);
            labelPaymentTitle.Name = "labelPaymentTitle";
            labelPaymentTitle.Size = new Size(143, 32);
            labelPaymentTitle.TabIndex = 0;
            labelPaymentTitle.Text = "Thanh toán";
            // 
            // panelDateTime
            // 
            panelDateTime.Controls.Add(buttonConfirmDateTime);
            panelDateTime.Controls.Add(comboBoxTimeSlot);
            panelDateTime.Controls.Add(labelTimeSlot);
            panelDateTime.Controls.Add(dateTimePickerAppointment);
            panelDateTime.Controls.Add(labelDate);
            panelDateTime.Dock = DockStyle.Fill;
            panelDateTime.Location = new Point(0, 0);
            panelDateTime.Margin = new Padding(4, 5, 4, 5);
            panelDateTime.Name = "panelDateTime";
            panelDateTime.Size = new Size(813, 580);
            panelDateTime.TabIndex = 2;
            panelDateTime.Visible = false;
            // 
            // buttonConfirmDateTime
            // 
            buttonConfirmDateTime.BackColor = Color.FromArgb(255, 165, 0);
            buttonConfirmDateTime.FlatAppearance.BorderSize = 0;
            buttonConfirmDateTime.FlatStyle = FlatStyle.Flat;
            buttonConfirmDateTime.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            buttonConfirmDateTime.ForeColor = Color.White;
            buttonConfirmDateTime.Location = new Point(13, 285);
            buttonConfirmDateTime.Margin = new Padding(4, 5, 4, 5);
            buttonConfirmDateTime.Name = "buttonConfirmDateTime";
            buttonConfirmDateTime.Size = new Size(773, 69);
            buttonConfirmDateTime.TabIndex = 4;
            buttonConfirmDateTime.Text = "Xác nhận";
            buttonConfirmDateTime.UseVisualStyleBackColor = false;
            // 
            // comboBoxTimeSlot
            // 
            comboBoxTimeSlot.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxTimeSlot.Enabled = false;
            comboBoxTimeSlot.Font = new Font("Segoe UI", 11F);
            comboBoxTimeSlot.FormattingEnabled = true;
            comboBoxTimeSlot.Location = new Point(13, 200);
            comboBoxTimeSlot.Margin = new Padding(4, 5, 4, 5);
            comboBoxTimeSlot.Name = "comboBoxTimeSlot";
            comboBoxTimeSlot.Size = new Size(772, 33);
            comboBoxTimeSlot.TabIndex = 3;
            // 
            // labelTimeSlot
            // 
            labelTimeSlot.AutoSize = true;
            labelTimeSlot.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            labelTimeSlot.ForeColor = Color.White;
            labelTimeSlot.Location = new Point(13, 154);
            labelTimeSlot.Margin = new Padding(4, 0, 4, 0);
            labelTimeSlot.Name = "labelTimeSlot";
            labelTimeSlot.Size = new Size(93, 25);
            labelTimeSlot.TabIndex = 2;
            labelTimeSlot.Text = "Chọn giờ";
            // 
            // dateTimePickerAppointment
            // 
            dateTimePickerAppointment.Font = new Font("Segoe UI", 11F);
            dateTimePickerAppointment.Format = DateTimePickerFormat.Short;
            dateTimePickerAppointment.Location = new Point(13, 77);
            dateTimePickerAppointment.Margin = new Padding(4, 5, 4, 5);
            dateTimePickerAppointment.Name = "dateTimePickerAppointment";
            dateTimePickerAppointment.Size = new Size(772, 32);
            dateTimePickerAppointment.TabIndex = 1;
            // 
            // labelDate
            // 
            labelDate.AutoSize = true;
            labelDate.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            labelDate.ForeColor = Color.White;
            labelDate.Location = new Point(13, 31);
            labelDate.Margin = new Padding(4, 0, 4, 0);
            labelDate.Name = "labelDate";
            labelDate.Size = new Size(108, 25);
            labelDate.TabIndex = 0;
            labelDate.Text = "Chọn ngày";
            // 
            // panelEmployee
            // 
            panelEmployee.Controls.Add(flowLayoutPanelEmployees);
            panelEmployee.Dock = DockStyle.Fill;
            panelEmployee.Location = new Point(0, 0);
            panelEmployee.Margin = new Padding(4, 5, 4, 5);
            panelEmployee.Name = "panelEmployee";
            panelEmployee.Size = new Size(813, 580);
            panelEmployee.TabIndex = 1;
            panelEmployee.Visible = false;
            // 
            // flowLayoutPanelEmployees
            // 
            flowLayoutPanelEmployees.AutoScroll = true;
            flowLayoutPanelEmployees.Dock = DockStyle.Fill;
            flowLayoutPanelEmployees.Location = new Point(0, 0);
            flowLayoutPanelEmployees.Margin = new Padding(4, 5, 4, 5);
            flowLayoutPanelEmployees.Name = "flowLayoutPanelEmployees";
            flowLayoutPanelEmployees.Size = new Size(813, 580);
            flowLayoutPanelEmployees.TabIndex = 0;
            // 
            // panelSpecialty
            // 
            panelSpecialty.Controls.Add(flowLayoutPanelSpecialties);
            panelSpecialty.Dock = DockStyle.Fill;
            panelSpecialty.Location = new Point(0, 0);
            panelSpecialty.Margin = new Padding(4, 5, 4, 5);
            panelSpecialty.Name = "panelSpecialty";
            panelSpecialty.Size = new Size(813, 580);
            panelSpecialty.TabIndex = 0;
            // 
            // flowLayoutPanelSpecialties
            // 
            flowLayoutPanelSpecialties.AutoScroll = true;
            flowLayoutPanelSpecialties.Dock = DockStyle.Fill;
            flowLayoutPanelSpecialties.Location = new Point(0, 0);
            flowLayoutPanelSpecialties.Margin = new Padding(4, 5, 4, 5);
            flowLayoutPanelSpecialties.Name = "flowLayoutPanelSpecialties";
            flowLayoutPanelSpecialties.Size = new Size(813, 580);
            flowLayoutPanelSpecialties.TabIndex = 0;
            // 
            // textBoxSearch
            // 
            textBoxSearch.BackColor = Color.White;
            textBoxSearch.BorderStyle = BorderStyle.FixedSingle;
            textBoxSearch.Font = new Font("Segoe UI", 11F);
            textBoxSearch.ForeColor = Color.Gray;
            textBoxSearch.Location = new Point(27, 123);
            textBoxSearch.Margin = new Padding(4, 5, 4, 5);
            textBoxSearch.Name = "textBoxSearch";
            textBoxSearch.Size = new Size(813, 32);
            textBoxSearch.TabIndex = 1;
            textBoxSearch.Text = "Tìm kiếm chuyên khoa";
            // 
            // panelLeftHeader
            // 
            panelLeftHeader.Controls.Add(labelLeftTitle);
            panelLeftHeader.Controls.Add(buttonBack);
            panelLeftHeader.Location = new Point(27, 31);
            panelLeftHeader.Margin = new Padding(4, 5, 4, 5);
            panelLeftHeader.Name = "panelLeftHeader";
            panelLeftHeader.Size = new Size(813, 77);
            panelLeftHeader.TabIndex = 0;
            // 
            // labelLeftTitle
            // 
            labelLeftTitle.AutoSize = true;
            labelLeftTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            labelLeftTitle.ForeColor = Color.White;
            labelLeftTitle.Location = new Point(80, 18);
            labelLeftTitle.Margin = new Padding(4, 0, 4, 0);
            labelLeftTitle.Name = "labelLeftTitle";
            labelLeftTitle.Size = new Size(251, 37);
            labelLeftTitle.TabIndex = 1;
            labelLeftTitle.Text = "Chọn chuyên khoa";
            // 
            // buttonBack
            // 
            buttonBack.BackColor = Color.Transparent;
            buttonBack.FlatAppearance.BorderColor = Color.White;
            buttonBack.FlatStyle = FlatStyle.Flat;
            buttonBack.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            buttonBack.ForeColor = Color.White;
            buttonBack.Location = new Point(7, 14);
            buttonBack.Margin = new Padding(4, 5, 4, 5);
            buttonBack.Name = "buttonBack";
            buttonBack.Size = new Size(67, 54);
            buttonBack.TabIndex = 0;
            buttonBack.Text = "←";
            buttonBack.UseVisualStyleBackColor = false;
            buttonBack.Visible = false;
            // 
            // panelRight
            // 
            panelRight.Anchor = AnchorStyles.None;
            panelRight.BackColor = Color.FromArgb(52, 58, 64);
            panelRight.Controls.Add(panelRightContent);
            panelRight.Controls.Add(labelRightTitle);
            panelRight.Location = new Point(973, 240);
            panelRight.Margin = new Padding(4, 5, 4, 5);
            panelRight.Name = "panelRight";
            panelRight.Size = new Size(600, 800);
            panelRight.TabIndex = 1;
            // 
            // panelRightContent
            // 
            panelRightContent.Controls.Add(panelTotalSection);
            panelRightContent.Controls.Add(panelSelectedDateTime);
            panelRightContent.Controls.Add(panelSelectedEmployee);
            panelRightContent.Controls.Add(panelSelectedSpecialty);
            panelRightContent.Location = new Point(27, 108);
            panelRightContent.Margin = new Padding(4, 5, 4, 5);
            panelRightContent.Name = "panelRightContent";
            panelRightContent.Size = new Size(547, 670);
            panelRightContent.TabIndex = 1;
            // 
            // panelTotalSection
            // 
            panelTotalSection.BorderStyle = BorderStyle.FixedSingle;
            panelTotalSection.Controls.Add(labelTotalPrice);
            panelTotalSection.Controls.Add(labelTotalLabel);
            panelTotalSection.Location = new Point(0, 415);
            panelTotalSection.Margin = new Padding(4, 5, 4, 5);
            panelTotalSection.Name = "panelTotalSection";
            panelTotalSection.Padding = new Padding(20, 23, 20, 23);
            panelTotalSection.Size = new Size(546, 91);
            panelTotalSection.TabIndex = 3;
            panelTotalSection.Visible = false;
            // 
            // labelTotalPrice
            // 
            labelTotalPrice.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            labelTotalPrice.ForeColor = Color.White;
            labelTotalPrice.Location = new Point(333, 23);
            labelTotalPrice.Margin = new Padding(4, 0, 4, 0);
            labelTotalPrice.Name = "labelTotalPrice";
            labelTotalPrice.Size = new Size(187, 38);
            labelTotalPrice.TabIndex = 1;
            labelTotalPrice.Text = "0 VNĐ";
            labelTotalPrice.TextAlign = ContentAlignment.MiddleRight;
            // 
            // labelTotalLabel
            // 
            labelTotalLabel.AutoSize = true;
            labelTotalLabel.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            labelTotalLabel.ForeColor = Color.White;
            labelTotalLabel.Location = new Point(20, 28);
            labelTotalLabel.Margin = new Padding(4, 0, 4, 0);
            labelTotalLabel.Name = "labelTotalLabel";
            labelTotalLabel.Size = new Size(98, 25);
            labelTotalLabel.TabIndex = 0;
            labelTotalLabel.Text = "Tổng tiền";
            // 
            // panelSelectedDateTime
            // 
            panelSelectedDateTime.BorderStyle = BorderStyle.FixedSingle;
            panelSelectedDateTime.Controls.Add(labelSelectedDateTimeValue);
            panelSelectedDateTime.Controls.Add(labelSelectedDateTimeLabel);
            panelSelectedDateTime.Location = new Point(0, 277);
            panelSelectedDateTime.Margin = new Padding(4, 5, 4, 5);
            panelSelectedDateTime.Name = "panelSelectedDateTime";
            panelSelectedDateTime.Padding = new Padding(20, 23, 20, 23);
            panelSelectedDateTime.Size = new Size(546, 122);
            panelSelectedDateTime.TabIndex = 2;
            // 
            // labelSelectedDateTimeValue
            // 
            labelSelectedDateTimeValue.Font = new Font("Segoe UI", 10F);
            labelSelectedDateTimeValue.ForeColor = Color.LightGray;
            labelSelectedDateTimeValue.Location = new Point(20, 62);
            labelSelectedDateTimeValue.Margin = new Padding(4, 0, 4, 0);
            labelSelectedDateTimeValue.Name = "labelSelectedDateTimeValue";
            labelSelectedDateTimeValue.Size = new Size(493, 38);
            labelSelectedDateTimeValue.TabIndex = 1;
            labelSelectedDateTimeValue.Text = "Chưa chọn";
            // 
            // labelSelectedDateTimeLabel
            // 
            labelSelectedDateTimeLabel.AutoSize = true;
            labelSelectedDateTimeLabel.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            labelSelectedDateTimeLabel.ForeColor = Color.White;
            labelSelectedDateTimeLabel.Location = new Point(20, 15);
            labelSelectedDateTimeLabel.Margin = new Padding(4, 0, 4, 0);
            labelSelectedDateTimeLabel.Name = "labelSelectedDateTimeLabel";
            labelSelectedDateTimeLabel.Size = new Size(127, 25);
            labelSelectedDateTimeLabel.TabIndex = 0;
            labelSelectedDateTimeLabel.Text = "📅 Ngày & Giờ";
            // 
            // panelSelectedEmployee
            // 
            panelSelectedEmployee.BorderStyle = BorderStyle.FixedSingle;
            panelSelectedEmployee.Controls.Add(labelSelectedEmployeeValue);
            panelSelectedEmployee.Controls.Add(labelSelectedEmployeeLabel);
            panelSelectedEmployee.Location = new Point(0, 138);
            panelSelectedEmployee.Margin = new Padding(4, 5, 4, 5);
            panelSelectedEmployee.Name = "panelSelectedEmployee";
            panelSelectedEmployee.Padding = new Padding(20, 23, 20, 23);
            panelSelectedEmployee.Size = new Size(546, 122);
            panelSelectedEmployee.TabIndex = 1;
            // 
            // labelSelectedEmployeeValue
            // 
            labelSelectedEmployeeValue.Font = new Font("Segoe UI", 10F);
            labelSelectedEmployeeValue.ForeColor = Color.LightGray;
            labelSelectedEmployeeValue.Location = new Point(20, 62);
            labelSelectedEmployeeValue.Margin = new Padding(4, 0, 4, 0);
            labelSelectedEmployeeValue.Name = "labelSelectedEmployeeValue";
            labelSelectedEmployeeValue.Size = new Size(493, 38);
            labelSelectedEmployeeValue.TabIndex = 1;
            labelSelectedEmployeeValue.Text = "Chưa chọn";
            // 
            // labelSelectedEmployeeLabel
            // 
            labelSelectedEmployeeLabel.AutoSize = true;
            labelSelectedEmployeeLabel.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            labelSelectedEmployeeLabel.ForeColor = Color.White;
            labelSelectedEmployeeLabel.Location = new Point(20, 15);
            labelSelectedEmployeeLabel.Margin = new Padding(4, 0, 4, 0);
            labelSelectedEmployeeLabel.Name = "labelSelectedEmployeeLabel";
            labelSelectedEmployeeLabel.Size = new Size(129, 25);
            labelSelectedEmployeeLabel.TabIndex = 0;
            labelSelectedEmployeeLabel.Text = "👤 Nhân viên";
            // 
            // panelSelectedSpecialty
            // 
            panelSelectedSpecialty.BorderStyle = BorderStyle.FixedSingle;
            panelSelectedSpecialty.Controls.Add(labelSelectedSpecialtyValue);
            panelSelectedSpecialty.Controls.Add(labelSelectedSpecialtyLabel);
            panelSelectedSpecialty.Location = new Point(0, 0);
            panelSelectedSpecialty.Margin = new Padding(4, 5, 4, 5);
            panelSelectedSpecialty.Name = "panelSelectedSpecialty";
            panelSelectedSpecialty.Padding = new Padding(20, 23, 20, 23);
            panelSelectedSpecialty.Size = new Size(546, 122);
            panelSelectedSpecialty.TabIndex = 0;
            // 
            // labelSelectedSpecialtyValue
            // 
            labelSelectedSpecialtyValue.Font = new Font("Segoe UI", 10F);
            labelSelectedSpecialtyValue.ForeColor = Color.LightGray;
            labelSelectedSpecialtyValue.Location = new Point(20, 62);
            labelSelectedSpecialtyValue.Margin = new Padding(4, 0, 4, 0);
            labelSelectedSpecialtyValue.Name = "labelSelectedSpecialtyValue";
            labelSelectedSpecialtyValue.Size = new Size(493, 38);
            labelSelectedSpecialtyValue.TabIndex = 1;
            labelSelectedSpecialtyValue.Text = "Chưa chọn";
            // 
            // labelSelectedSpecialtyLabel
            // 
            labelSelectedSpecialtyLabel.AutoSize = true;
            labelSelectedSpecialtyLabel.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            labelSelectedSpecialtyLabel.ForeColor = Color.White;
            labelSelectedSpecialtyLabel.Location = new Point(20, 15);
            labelSelectedSpecialtyLabel.Margin = new Padding(4, 0, 4, 0);
            labelSelectedSpecialtyLabel.Name = "labelSelectedSpecialtyLabel";
            labelSelectedSpecialtyLabel.Size = new Size(155, 25);
            labelSelectedSpecialtyLabel.TabIndex = 0;
            labelSelectedSpecialtyLabel.Text = "💼 Chuyên khoa";
            // 
            // labelRightTitle
            // 
            labelRightTitle.AutoSize = true;
            labelRightTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            labelRightTitle.ForeColor = Color.White;
            labelRightTitle.Location = new Point(27, 31);
            labelRightTitle.Margin = new Padding(4, 0, 4, 0);
            labelRightTitle.Name = "labelRightTitle";
            labelRightTitle.Size = new Size(212, 37);
            labelRightTitle.TabIndex = 0;
            labelRightTitle.Text = "Chi tiết đặt lịch";
            // 
            // Bookings
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(1653, 1150);
            Controls.Add(panelRight);
            Controls.Add(panelLeft);
            Controls.Add(panelHeader);
            Margin = new Padding(4, 5, 4, 5);
            Name = "Bookings";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Đặt lịch hẹn";
            panelHeader.ResumeLayout(false);
            panelHeader.PerformLayout();
            panelLeft.ResumeLayout(false);
            panelLeft.PerformLayout();
            panelLeftContent.ResumeLayout(false);
            panelThankYou.ResumeLayout(false);
            panelPayment.ResumeLayout(false);
            panelPayment.PerformLayout();
            panelDateTime.ResumeLayout(false);
            panelDateTime.PerformLayout();
            panelEmployee.ResumeLayout(false);
            panelSpecialty.ResumeLayout(false);
            panelLeftHeader.ResumeLayout(false);
            panelLeftHeader.PerformLayout();
            panelRight.ResumeLayout(false);
            panelRight.PerformLayout();
            panelRightContent.ResumeLayout(false);
            panelTotalSection.ResumeLayout(false);
            panelTotalSection.PerformLayout();
            panelSelectedDateTime.ResumeLayout(false);
            panelSelectedDateTime.PerformLayout();
            panelSelectedEmployee.ResumeLayout(false);
            panelSelectedEmployee.PerformLayout();
            panelSelectedSpecialty.ResumeLayout(false);
            panelSelectedSpecialty.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel panelHeader;
        private System.Windows.Forms.Label labelHeader;
        private System.Windows.Forms.Panel panelLeft;
        private System.Windows.Forms.Panel panelLeftContent;
        private System.Windows.Forms.Panel panelThankYou;
        private System.Windows.Forms.Label labelThankYouMessage;
        private System.Windows.Forms.Label labelThankYouTitle;
        private System.Windows.Forms.Button buttonBackToStart;
        private System.Windows.Forms.Panel panelPayment;
        private System.Windows.Forms.Button buttonConfirmBooking;
        private System.Windows.Forms.TextBox textBoxPhone;
        private System.Windows.Forms.Label labelPhone;
        private System.Windows.Forms.TextBox textBoxName;
        private System.Windows.Forms.Label labelName;
        private System.Windows.Forms.Button buttonApplyPromo;
        private System.Windows.Forms.TextBox textBoxPromoCode;
        private System.Windows.Forms.Label labelPromoCode;
        private System.Windows.Forms.Label labelPaymentNote;
        private System.Windows.Forms.Label labelCheckoutTotal;
        private System.Windows.Forms.Label labelTotalPayment;
        private System.Windows.Forms.Label labelPaymentTitle;
        private System.Windows.Forms.Panel panelDateTime;
        private System.Windows.Forms.Button buttonConfirmDateTime;
        private System.Windows.Forms.ComboBox comboBoxTimeSlot;
        private System.Windows.Forms.Label labelTimeSlot;
        private System.Windows.Forms.DateTimePicker dateTimePickerAppointment;
        private System.Windows.Forms.Label labelDate;
        private System.Windows.Forms.Panel panelEmployee;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelEmployees;
        private System.Windows.Forms.Panel panelSpecialty;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelSpecialties;
        private System.Windows.Forms.TextBox textBoxSearch;
        private System.Windows.Forms.Panel panelLeftHeader;
        private System.Windows.Forms.Label labelLeftTitle;
        private System.Windows.Forms.Button buttonBack;
        private System.Windows.Forms.Panel panelRight;
        private System.Windows.Forms.Panel panelRightContent;
        private System.Windows.Forms.Panel panelTotalSection;
        private System.Windows.Forms.Label labelTotalPrice;
        private System.Windows.Forms.Label labelTotalLabel;
        private System.Windows.Forms.Panel panelSelectedDateTime;
        private System.Windows.Forms.Label labelSelectedDateTimeValue;
        private System.Windows.Forms.Label labelSelectedDateTimeLabel;
        private System.Windows.Forms.Panel panelSelectedEmployee;
        private System.Windows.Forms.Label labelSelectedEmployeeValue;
        private System.Windows.Forms.Label labelSelectedEmployeeLabel;
        private System.Windows.Forms.Panel panelSelectedSpecialty;
        private System.Windows.Forms.Label labelSelectedSpecialtyValue;
        private System.Windows.Forms.Label labelSelectedSpecialtyLabel;
        private System.Windows.Forms.Label labelRightTitle;
    }
}
namespace BookingCareManagement.WinForms;

partial class Form1
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
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
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        doctorsGrid = new DataGridView();
        refreshButton = new Button();
        loadingIndicator = new Label();
        button1 = new Button();
        ((System.ComponentModel.ISupportInitialize)doctorsGrid).BeginInit();
        SuspendLayout();
        // 
        // doctorsGrid
        // 
        doctorsGrid.AllowUserToAddRows = false;
        doctorsGrid.AllowUserToDeleteRows = false;
        doctorsGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        doctorsGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        doctorsGrid.Location = new Point(14, 387);
        doctorsGrid.Margin = new Padding(3, 4, 3, 4);
        doctorsGrid.MultiSelect = false;
        doctorsGrid.Name = "doctorsGrid";
        doctorsGrid.ReadOnly = true;
        doctorsGrid.RowHeadersVisible = false;
        doctorsGrid.RowHeadersWidth = 51;
        doctorsGrid.RowTemplate.Height = 25;
        doctorsGrid.Size = new Size(887, 197);
        doctorsGrid.TabIndex = 0;
        // 
        // refreshButton
        // 
        refreshButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        refreshButton.Location = new Point(785, 16);
        refreshButton.Margin = new Padding(3, 4, 3, 4);
        refreshButton.Name = "refreshButton";
        refreshButton.Size = new Size(115, 36);
        refreshButton.TabIndex = 1;
        refreshButton.Text = "Làm mới";
        refreshButton.UseVisualStyleBackColor = true;
        // 
        // loadingIndicator
        // 
        loadingIndicator.AutoSize = true;
        loadingIndicator.Location = new Point(14, 24);
        loadingIndicator.Name = "loadingIndicator";
        loadingIndicator.Size = new Size(167, 20);
        loadingIndicator.TabIndex = 2;
        loadingIndicator.Text = "Đang tải dữ liệu bác sĩ...";
        loadingIndicator.Visible = false;
        // 
        // button1
        // 
        button1.Location = new Point(163, 166);
        button1.Name = "button1";
        button1.Size = new Size(94, 29);
        button1.TabIndex = 3;
        button1.Text = "button1";
        button1.UseVisualStyleBackColor = true;
        button1.Click += button1_Click;
        // 
        // Form1
        // 
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(914, 600);
        Controls.Add(button1);
        Controls.Add(loadingIndicator);
        Controls.Add(refreshButton);
        Controls.Add(doctorsGrid);
        Margin = new Padding(3, 4, 3, 4);
        Name = "Form1";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "BookingCare - Quản lý bác sĩ";
        ((System.ComponentModel.ISupportInitialize)doctorsGrid).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private System.Windows.Forms.DataGridView doctorsGrid;
    private System.Windows.Forms.Button refreshButton;
    private System.Windows.Forms.Label loadingIndicator;
    private Button button1;
}

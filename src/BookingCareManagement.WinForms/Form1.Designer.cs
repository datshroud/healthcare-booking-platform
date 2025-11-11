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
        this.components = new System.ComponentModel.Container();
        this.doctorsGrid = new System.Windows.Forms.DataGridView();
        this.refreshButton = new System.Windows.Forms.Button();
        this.loadingIndicator = new System.Windows.Forms.Label();
        ((System.ComponentModel.ISupportInitialize)(this.doctorsGrid)).BeginInit();
        this.SuspendLayout();
        // 
        // doctorsGrid
        // 
        this.doctorsGrid.AllowUserToAddRows = false;
        this.doctorsGrid.AllowUserToDeleteRows = false;
        this.doctorsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
                    | System.Windows.Forms.AnchorStyles.Left) 
                    | System.Windows.Forms.AnchorStyles.Right)));
        this.doctorsGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        this.doctorsGrid.Location = new System.Drawing.Point(12, 51);
        this.doctorsGrid.MultiSelect = false;
        this.doctorsGrid.Name = "doctorsGrid";
        this.doctorsGrid.ReadOnly = true;
        this.doctorsGrid.RowHeadersVisible = false;
        this.doctorsGrid.RowTemplate.Height = 25;
        this.doctorsGrid.Size = new System.Drawing.Size(776, 387);
        this.doctorsGrid.TabIndex = 0;
        // 
        // refreshButton
        // 
        this.refreshButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.refreshButton.Location = new System.Drawing.Point(687, 12);
        this.refreshButton.Name = "refreshButton";
        this.refreshButton.Size = new System.Drawing.Size(101, 27);
        this.refreshButton.TabIndex = 1;
        this.refreshButton.Text = "Làm mới";
        this.refreshButton.UseVisualStyleBackColor = true;
        // 
        // loadingIndicator
        // 
        this.loadingIndicator.AutoSize = true;
        this.loadingIndicator.Location = new System.Drawing.Point(12, 18);
        this.loadingIndicator.Name = "loadingIndicator";
        this.loadingIndicator.Size = new System.Drawing.Size(137, 15);
        this.loadingIndicator.TabIndex = 2;
        this.loadingIndicator.Text = "Đang tải dữ liệu bác sĩ...";
        this.loadingIndicator.Visible = false;
        // 
        // Form1
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(800, 450);
        this.Controls.Add(this.loadingIndicator);
        this.Controls.Add(this.refreshButton);
        this.Controls.Add(this.doctorsGrid);
        this.Name = "Form1";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "BookingCare - Quản lý bác sĩ";
        ((System.ComponentModel.ISupportInitialize)(this.doctorsGrid)).EndInit();
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    #endregion

    private System.Windows.Forms.DataGridView doctorsGrid;
    private System.Windows.Forms.Button refreshButton;
    private System.Windows.Forms.Label loadingIndicator;
}

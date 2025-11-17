namespace BookingCareManagement.WinForms.Areas.Admin.Forms
{
    partial class Dashboard
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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend2 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.mainTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.cardsTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.panelKhachHang = new System.Windows.Forms.Panel();
            this.lblKhachHangTitle = new System.Windows.Forms.Label();
            this.lblKhachHangValue = new System.Windows.Forms.Label();
            this.panelDoanhThu = new System.Windows.Forms.Panel();
            this.lblDoanhThuTitle = new System.Windows.Forms.Label();
            this.lblDoanhThuValue = new System.Windows.Forms.Label();
            this.panelBacSi = new System.Windows.Forms.Panel();
            this.lblBacSiTitle = new System.Windows.Forms.Label();
            this.lblBacSiValue = new System.Windows.Forms.Label();
            this.chartsTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.panelLineChart = new System.Windows.Forms.Panel();
            this.chartCuocHen = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.panelPieChart = new System.Windows.Forms.Panel();
            this.chartChuyenKhoa = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.mainTableLayoutPanel.SuspendLayout();
            this.cardsTableLayoutPanel.SuspendLayout();
            this.panelKhachHang.SuspendLayout();
            this.panelDoanhThu.SuspendLayout();
            this.panelBacSi.SuspendLayout();
            this.chartsTableLayoutPanel.SuspendLayout();
            this.panelLineChart.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartCuocHen)).BeginInit();
            this.panelPieChart.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartChuyenKhoa)).BeginInit();
            this.SuspendLayout();
            // 
            // mainTableLayoutPanel
            // 
            this.mainTableLayoutPanel.BackColor = System.Drawing.Color.Transparent;
            this.mainTableLayoutPanel.ColumnCount = 1;
            this.mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainTableLayoutPanel.Controls.Add(this.cardsTableLayoutPanel, 0, 0);
            this.mainTableLayoutPanel.Controls.Add(this.chartsTableLayoutPanel, 0, 1);
            this.mainTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.mainTableLayoutPanel.Name = "mainTableLayoutPanel";
            this.mainTableLayoutPanel.Padding = new System.Windows.Forms.Padding(10);
            this.mainTableLayoutPanel.RowCount = 2;
            this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainTableLayoutPanel.Size = new System.Drawing.Size(1027, 600);
            this.mainTableLayoutPanel.TabIndex = 0;
            // 
            // cardsTableLayoutPanel
            // 
            this.cardsTableLayoutPanel.ColumnCount = 3;
            this.cardsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33F));
            this.cardsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33F));
            this.cardsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33F));
            this.cardsTableLayoutPanel.Controls.Add(this.panelKhachHang, 0, 0);
            this.cardsTableLayoutPanel.Controls.Add(this.panelDoanhThu, 1, 0);
            this.cardsTableLayoutPanel.Controls.Add(this.panelBacSi, 2, 0);
            this.cardsTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cardsTableLayoutPanel.Location = new System.Drawing.Point(13, 13);
            this.cardsTableLayoutPanel.Name = "cardsTableLayoutPanel";
            this.cardsTableLayoutPanel.RowCount = 1;
            this.cardsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.cardsTableLayoutPanel.Size = new System.Drawing.Size(1001, 144);
            this.cardsTableLayoutPanel.TabIndex = 0;
            // 
            // panelKhachHang
            // 
            this.panelKhachHang.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(79)))), ((int)(((byte)(172)))), ((int)(((byte)(254))))); // Màu xanh nhạt (Cyan)
            this.panelKhachHang.Controls.Add(this.lblKhachHangTitle);
            this.panelKhachHang.Controls.Add(this.lblKhachHangValue);
            this.panelKhachHang.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelKhachHang.Location = new System.Drawing.Point(10, 10);
            this.panelKhachHang.Margin = new System.Windows.Forms.Padding(10);
            this.panelKhachHang.Name = "panelKhachHang";
            this.panelKhachHang.Size = new System.Drawing.Size(313, 124);
            this.panelKhachHang.TabIndex = 0;
            // 
            // lblKhachHangTitle
            // 
            this.lblKhachHangTitle.AutoSize = true;
            this.lblKhachHangTitle.BackColor = System.Drawing.Color.Transparent;
            this.lblKhachHangTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblKhachHangTitle.ForeColor = System.Drawing.Color.White;
            this.lblKhachHangTitle.Location = new System.Drawing.Point(20, 20);
            this.lblKhachHangTitle.Name = "lblKhachHangTitle";
            this.lblKhachHangTitle.Size = new System.Drawing.Size(114, 28);
            this.lblKhachHangTitle.TabIndex = 0;
            this.lblKhachHangTitle.Text = "Khách hàng";
            // 
            // lblKhachHangValue
            // 
            this.lblKhachHangValue.AutoSize = true;
            this.lblKhachHangValue.BackColor = System.Drawing.Color.Transparent;
            this.lblKhachHangValue.Font = new System.Drawing.Font("Segoe UI", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblKhachHangValue.ForeColor = System.Drawing.Color.White;
            this.lblKhachHangValue.Location = new System.Drawing.Point(20, 55);
            this.lblKhachHangValue.Name = "lblKhachHangValue";
            this.lblKhachHangValue.Size = new System.Drawing.Size(92, 54);
            this.lblKhachHangValue.TabIndex = 1;
            this.lblKhachHangValue.Text = "150";
            // 
            // panelDoanhThu
            // 
            this.panelDoanhThu.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(67)))), ((int)(((byte)(233)))), ((int)(((byte)(123))))); // Màu xanh lá
            this.panelDoanhThu.Controls.Add(this.lblDoanhThuTitle);
            this.panelDoanhThu.Controls.Add(this.lblDoanhThuValue);
            this.panelDoanhThu.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelDoanhThu.Location = new System.Drawing.Point(343, 10);
            this.panelDoanhThu.Margin = new System.Windows.Forms.Padding(10);
            this.panelDoanhThu.Name = "panelDoanhThu";
            this.panelDoanhThu.Size = new System.Drawing.Size(313, 124);
            this.panelDoanhThu.TabIndex = 1;
            // 
            // lblDoanhThuTitle
            // 
            this.lblDoanhThuTitle.AutoSize = true;
            this.lblDoanhThuTitle.BackColor = System.Drawing.Color.Transparent;
            this.lblDoanhThuTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDoanhThuTitle.ForeColor = System.Drawing.Color.White;
            this.lblDoanhThuTitle.Location = new System.Drawing.Point(20, 20);
            this.lblDoanhThuTitle.Name = "lblDoanhThuTitle";
            this.lblDoanhThuTitle.Size = new System.Drawing.Size(104, 28);
            this.lblDoanhThuTitle.TabIndex = 0;
            this.lblDoanhThuTitle.Text = "Doanh thu";
            // 
            // lblDoanhThuValue
            // 
            this.lblDoanhThuValue.AutoSize = true;
            this.lblDoanhThuValue.BackColor = System.Drawing.Color.Transparent;
            this.lblDoanhThuValue.Font = new System.Drawing.Font("Segoe UI", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDoanhThuValue.ForeColor = System.Drawing.Color.White;
            this.lblDoanhThuValue.Location = new System.Drawing.Point(20, 55);
            this.lblDoanhThuValue.Name = "lblDoanhThuValue";
            this.lblDoanhThuValue.Size = new System.Drawing.Size(172, 54);
            this.lblDoanhThuValue.TabIndex = 1;
            this.lblDoanhThuValue.Text = "500.000";
            // 
            // panelBacSi
            // 
            this.panelBacSi.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(153)))), ((int)(((byte)(102))))); // Màu cam
            this.panelBacSi.Controls.Add(this.lblBacSiTitle);
            this.panelBacSi.Controls.Add(this.lblBacSiValue);
            this.panelBacSi.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelBacSi.Location = new System.Drawing.Point(676, 10);
            this.panelBacSi.Margin = new System.Windows.Forms.Padding(10);
            this.panelBacSi.Name = "panelBacSi";
            this.panelBacSi.Size = new System.Drawing.Size(315, 124);
            this.panelBacSi.TabIndex = 2;
            // 
            // lblBacSiTitle
            // 
            this.lblBacSiTitle.AutoSize = true;
            this.lblBacSiTitle.BackColor = System.Drawing.Color.Transparent;
            this.lblBacSiTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBacSiTitle.ForeColor = System.Drawing.Color.White;
            this.lblBacSiTitle.Location = new System.Drawing.Point(20, 20);
            this.lblBacSiTitle.Name = "lblBacSiTitle";
            this.lblBacSiTitle.Size = new System.Drawing.Size(60, 28);
            this.lblBacSiTitle.TabIndex = 0;
            this.lblBacSiTitle.Text = "Bác sĩ";
            // 
            // lblBacSiValue
            // 
            this.lblBacSiValue.AutoSize = true;
            this.lblBacSiValue.BackColor = System.Drawing.Color.Transparent;
            this.lblBacSiValue.Font = new System.Drawing.Font("Segoe UI", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBacSiValue.ForeColor = System.Drawing.Color.White;
            this.lblBacSiValue.Location = new System.Drawing.Point(20, 55);
            this.lblBacSiValue.Name = "lblBacSiValue";
            this.lblBacSiValue.Size = new System.Drawing.Size(69, 54);
            this.lblBacSiValue.TabIndex = 1;
            this.lblBacSiValue.Text = "20";
            // 
            // chartsTableLayoutPanel
            // 
            this.chartsTableLayoutPanel.ColumnCount = 2;
            this.chartsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.chartsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.chartsTableLayoutPanel.Controls.Add(this.panelLineChart, 0, 0);
            this.chartsTableLayoutPanel.Controls.Add(this.panelPieChart, 1, 0);
            this.chartsTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chartsTableLayoutPanel.Location = new System.Drawing.Point(13, 163);
            this.chartsTableLayoutPanel.Name = "chartsTableLayoutPanel";
            this.chartsTableLayoutPanel.RowCount = 1;
            this.chartsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.chartsTableLayoutPanel.Size = new System.Drawing.Size(1001, 424);
            this.chartsTableLayoutPanel.TabIndex = 1;
            // 
            // panelLineChart
            // 
            this.panelLineChart.BackColor = System.Drawing.Color.White;
            this.panelLineChart.Controls.Add(this.chartCuocHen);
            this.panelLineChart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelLineChart.Location = new System.Drawing.Point(10, 10);
            this.panelLineChart.Margin = new System.Windows.Forms.Padding(10);
            this.panelLineChart.Name = "panelLineChart";
            this.panelLineChart.Padding = new System.Windows.Forms.Padding(5);
            this.panelLineChart.Size = new System.Drawing.Size(580, 404);
            this.panelLineChart.TabIndex = 0;
            // 
            // chartCuocHen
            // 
            chartArea1.Name = "ChartArea1";
            this.chartCuocHen.ChartAreas.Add(chartArea1);
            this.chartCuocHen.Dock = System.Windows.Forms.DockStyle.Fill;
            legend1.Name = "Legend1";
            this.chartCuocHen.Legends.Add(legend1);
            this.chartCuocHen.Location = new System.Drawing.Point(5, 5);
            this.chartCuocHen.Name = "chartCuocHen";
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            series1.Legend = "Legend1";
            series1.Name = "CuocHen";
            this.chartCuocHen.Series.Add(series1);
            this.chartCuocHen.Size = new System.Drawing.Size(570, 394);
            this.chartCuocHen.TabIndex = 0;
            this.chartCuocHen.Text = "Biểu đồ cuộc hẹn";
            // 
            // panelPieChart
            // 
            this.panelPieChart.BackColor = System.Drawing.Color.White;
            this.panelPieChart.Controls.Add(this.chartChuyenKhoa);
            this.panelPieChart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelPieChart.Location = new System.Drawing.Point(610, 10);
            this.panelPieChart.Margin = new System.Windows.Forms.Padding(10);
            this.panelPieChart.Name = "panelPieChart";
            this.panelPieChart.Padding = new System.Windows.Forms.Padding(5);
            this.panelPieChart.Size = new System.Drawing.Size(381, 404);
            this.panelPieChart.TabIndex = 1;
            // 
            // chartChuyenKhoa
            // 
            chartArea2.Name = "ChartArea1";
            this.chartChuyenKhoa.ChartAreas.Add(chartArea2);
            this.chartChuyenKhoa.Dock = System.Windows.Forms.DockStyle.Fill;
            legend2.Name = "Legend1";
            this.chartChuyenKhoa.Legends.Add(legend2);
            this.chartChuyenKhoa.Location = new System.Drawing.Point(5, 5);
            this.chartChuyenKhoa.Name = "chartChuyenKhoa";
            series2.ChartArea = "ChartArea1";
            series2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Doughnut;
            series2.Legend = "Legend1";
            series2.Name = "ChuyenKhoa";
            this.chartChuyenKhoa.Series.Add(series2);
            this.chartChuyenKhoa.Size = new System.Drawing.Size(371, 394);
            this.chartChuyenKhoa.TabIndex = 0;
            this.chartChuyenKhoa.Text = "Biểu đồ chuyên khoa";
            // 
            // Dashboard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(242)))), ((int)(((byte)(245)))));
            this.ClientSize = new System.Drawing.Size(1027, 600);
            this.Controls.Add(this.mainTableLayoutPanel);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "Dashboard";
            this.Text = "Bảng điều khiển bệnh viện";
            this.mainTableLayoutPanel.ResumeLayout(false);
            this.cardsTableLayoutPanel.ResumeLayout(false);
            this.panelKhachHang.ResumeLayout(false);
            this.panelKhachHang.PerformLayout();
            this.panelDoanhThu.ResumeLayout(false);
            this.panelDoanhThu.PerformLayout();
            this.panelBacSi.ResumeLayout(false);
            this.panelBacSi.PerformLayout();
            this.chartsTableLayoutPanel.ResumeLayout(false);
            this.panelLineChart.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.chartCuocHen)).EndInit();
            this.panelPieChart.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.chartChuyenKhoa)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel mainTableLayoutPanel;
        private System.Windows.Forms.TableLayoutPanel cardsTableLayoutPanel;
        private System.Windows.Forms.Panel panelKhachHang;
        private System.Windows.Forms.Label lblKhachHangTitle;
        private System.Windows.Forms.Label lblKhachHangValue;
        private System.Windows.Forms.Panel panelDoanhThu;
        private System.Windows.Forms.Label lblDoanhThuTitle;
        private System.Windows.Forms.Label lblDoanhThuValue;
        private System.Windows.Forms.Panel panelBacSi;
        private System.Windows.Forms.Label lblBacSiTitle;
        private System.Windows.Forms.Label lblBacSiValue;
        private System.Windows.Forms.TableLayoutPanel chartsTableLayoutPanel;
        private System.Windows.Forms.Panel panelLineChart;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartCuocHen;
        private System.Windows.Forms.Panel panelPieChart;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartChuyenKhoa;
    }
}
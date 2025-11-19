using System;
using System.Drawing;
using System.Windows.Forms;
using iTextSharp.text;
using iTextSharp.text.pdf;
using BookingCareManagement.WinForms.Shared.Models.Dtos;
using System.IO;
using System.Text;
using PdfRectangle = iTextSharp.text.Rectangle;

namespace BookingCareManagement.WinForms.Areas.Admin.Forms
{
    public partial class InvoiceReportForm : Form
    {
        private readonly InvoiceDto _invoice;

        public InvoiceReportForm(InvoiceDto invoice)
        {
            _invoice = invoice;
            
            // Đăng ký encoding provider để hỗ trợ windows-1252
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            
            InitializeComponent();
            SetupFormProperties();
            LoadInvoiceData();
        }

        private void SetupFormProperties()
        {
            this.Text = $"Hóa đơn #{_invoice.InvoiceNumber}";
            this.Font = new System.Drawing.Font("Segoe UI", 10F);
        }

        private void LoadInvoiceData()
        {
            txtCustomerName.Text = _invoice.CustomerName;
            txtCustomerEmail.Text = _invoice.CustomerEmail ?? string.Empty;
            txtInvoiceNumber.Text = _invoice.InvoiceNumber.ToString();
            dtpInvoiceDate.Value = _invoice.InvoiceDate;
            txtServiceName.Text = _invoice.ServiceName ?? "Service";
            numServiceQty.Value = 1; // Default
            txtServicePrice.Text = _invoice.Total.ToString("N0");
            
            UpdateTotalDisplay();
        }

        private void numServiceQty_ValueChanged(object sender, EventArgs e)
        {
            UpdateTotal();
        }

        private void txtServicePrice_TextChanged(object sender, EventArgs e)
        {
            UpdateTotal();
        }

        private void UpdateTotal()
        {
            var qty = (int)numServiceQty.Value;
            decimal price = 0;
            
            if (decimal.TryParse(txtServicePrice.Text.Replace(",", "").Replace(".", ""), out var parsedPrice))
            {
                price = parsedPrice;
            }

            var total = qty * price;
            lblAmount.Text = total.ToString("N0") + " ₫";
            lblTotal.Text = total.ToString("N0") + " ₫";
        }

        private void UpdateTotalDisplay()
        {
            lblTotal.Text = _invoice.Total.ToString("N0") + " ₫";
            lblAmount.Text = _invoice.Total.ToString("N0") + " ₫";
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void BtnPrint_Click(object sender, EventArgs e)
        {
            try
            {
                // Tạo PrintDocument để in
                var printDoc = new System.Drawing.Printing.PrintDocument();
                printDoc.PrintPage += PrintDoc_PrintPage;

                var printDialog = new PrintDialog
                {
                    Document = printDoc
                };

                if (printDialog.ShowDialog() == DialogResult.OK)
                {
                    printDoc.Print();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi in: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PrintDoc_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            // Vẽ nội dung hóa đơn lên trang in
            var g = e.Graphics;
            var fontRegular = new System.Drawing.Font("Segoe UI", 10);
            var fontBold = new System.Drawing.Font("Segoe UI", 12, FontStyle.Bold);
            var fontTitle = new System.Drawing.Font("Segoe UI", 18, FontStyle.Bold);
            
            var y = 100f;
            var x = 100f;

            // Header
            g.DrawString("dental", fontTitle, new SolidBrush(Color.FromArgb(13, 110, 253)), x, y);
            g.DrawString("Hóa đơn", fontBold, Brushes.Black, 600, y);
            y += 50;

            // Line
            g.DrawLine(new Pen(Color.FromArgb(13, 110, 253), 2), x, y, 700, y);
            y += 30;

            // Customer info
            g.DrawString(txtCustomerName.Text, fontBold, Brushes.Black, 500, y);
            y += 25;
            g.DrawString(txtCustomerEmail.Text, fontRegular, Brushes.Gray, 500, y);
            y += 30;
            g.DrawString($"Hóa đơn #{txtInvoiceNumber.Text}", fontRegular, Brushes.Gray, 500, y);
            y += 20;
            g.DrawString(dtpInvoiceDate.Value.ToString("MMMM dd, yyyy"), fontRegular, Brushes.Gray, 500, y);
            y += 50;

            // Table
            g.FillRectangle(new SolidBrush(Color.FromArgb(237, 245, 255)), x, y, 600, 30);
            g.DrawString("Mặt hàng", fontBold, Brushes.Black, x + 10, y + 7);
            g.DrawString("Số lượng", fontBold, Brushes.Black, x + 250, y + 7);
            g.DrawString("Đơn giá", fontBold, Brushes.Black, x + 370, y + 7);
            g.DrawString("Thành tiền", fontBold, Brushes.Black, x + 490, y + 7);
            y += 35;

            // Row
            g.DrawString(txtServiceName.Text, fontRegular, Brushes.Black, x + 10, y);
            g.DrawString(numServiceQty.Value.ToString(), fontRegular, Brushes.Black, x + 250, y);
            g.DrawString(txtServicePrice.Text, fontRegular, Brushes.Black, x + 370, y);
            g.DrawString(lblTotal.Text, fontRegular, Brushes.Black, x + 490, y);
            y += 50;

            // Total
            g.FillRectangle(new SolidBrush(Color.FromArgb(237, 245, 255)), x, y, 600, 40);
            g.DrawString("Tổng cùng", fontBold, Brushes.Black, x + 370, y + 10);
            g.DrawString(lblTotal.Text, fontBold, Brushes.Black, x + 490, y + 10);
        }

        private void BtnDownloadPdf_Click(object sender, EventArgs e)
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "PDF files (*.pdf)|*.pdf",
                    FileName = $"HoaDon-{txtInvoiceNumber.Text}.pdf",
                    DefaultExt = "pdf"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    CreatePdfDocument(saveDialog.FileName);
                    MessageBox.Show($"Đã lưu file PDF: {saveDialog.FileName}", "Thành công", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tạo PDF: {ex.Message}", "Lỗi", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CreatePdfDocument(string filePath)
        {
            var document = new Document(PageSize.A4, 50, 50, 50, 50);
            var writer = PdfWriter.GetInstance(document, new FileStream(filePath, FileMode.Create));
            document.Open();

            // Fonts
            var baseFont = BaseFont.CreateFont("c:\\windows\\fonts\\arial.ttf", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            var titleFont = new iTextSharp.text.Font(baseFont, 18, iTextSharp.text.Font.BOLD, new BaseColor(13, 110, 253));
            var headerFont = new iTextSharp.text.Font(baseFont, 16, iTextSharp.text.Font.BOLD);
            var boldFont = new iTextSharp.text.Font(baseFont, 12, iTextSharp.text.Font.BOLD);
            var normalFont = new iTextSharp.text.Font(baseFont, 10);
            var grayFont = new iTextSharp.text.Font(baseFont, 9, iTextSharp.text.Font.NORMAL, BaseColor.GRAY);

            // Header
            var headerTable = new PdfPTable(2) { WidthPercentage = 100 };
            headerTable.SetWidths(new float[] { 1, 1 });

            var cellLogo = new PdfPCell(new Phrase("dental", titleFont))
            {
                Border = PdfRectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_LEFT,
                PaddingBottom = 10
            };

            var cellTitle = new PdfPCell(new Phrase("Hóa đơn", headerFont))
            {
                Border = PdfRectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_RIGHT,
                PaddingBottom = 10
            };

            headerTable.AddCell(cellLogo);
            headerTable.AddCell(cellTitle);
            document.Add(headerTable);

            // Blue line
            var line = new iTextSharp.text.pdf.draw.LineSeparator(2f, 100f, new BaseColor(13, 110, 253), Element.ALIGN_CENTER, -2);
            document.Add(line);
            document.Add(new Paragraph(" "));

            // Customer info (right-aligned)
            var customerTable = new PdfPTable(1) { WidthPercentage = 100 };
            customerTable.AddCell(new PdfPCell(new Phrase(txtCustomerName.Text, boldFont))
            {
                Border = PdfRectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_RIGHT,
                PaddingBottom = 5
            });
            customerTable.AddCell(new PdfPCell(new Phrase(txtCustomerEmail.Text, grayFont))
            {
                Border = PdfRectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_RIGHT,
                PaddingBottom = 10
            });
            customerTable.AddCell(new PdfPCell(new Phrase($"Hóa đơn #{txtInvoiceNumber.Text}", grayFont))
            {
                Border = PdfRectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_RIGHT,
                PaddingBottom = 3
            });
            customerTable.AddCell(new PdfPCell(new Phrase(dtpInvoiceDate.Value.ToString("MMMM dd, yyyy"), grayFont))
            {
                Border = PdfRectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_RIGHT,
                PaddingBottom = 15
            });
            document.Add(customerTable);

            // Items table
            var itemsTable = new PdfPTable(4) { WidthPercentage = 100 };
            itemsTable.SetWidths(new float[] { 2.5f, 1.2f, 1.5f, 1.8f });

            // Table header
            var headerBg = new BaseColor(237, 245, 255);
            itemsTable.AddCell(CreateTableCell("Mặt hàng", boldFont, headerBg, Element.ALIGN_LEFT));
            itemsTable.AddCell(CreateTableCell("Số lượng", boldFont, headerBg, Element.ALIGN_CENTER));
            itemsTable.AddCell(CreateTableCell("Đơn giá", boldFont, headerBg, Element.ALIGN_CENTER));
            itemsTable.AddCell(CreateTableCell("Thành tiền", boldFont, headerBg, Element.ALIGN_RIGHT));

            // Table row
            var rowBg = new BaseColor(248, 249, 250);
            itemsTable.AddCell(CreateTableCell(txtServiceName.Text, normalFont, rowBg, Element.ALIGN_LEFT));
            itemsTable.AddCell(CreateTableCell(numServiceQty.Value.ToString(), normalFont, rowBg, Element.ALIGN_CENTER));
            itemsTable.AddCell(CreateTableCell(txtServicePrice.Text, normalFont, rowBg, Element.ALIGN_CENTER));
            itemsTable.AddCell(CreateTableCell(lblTotal.Text, normalFont, rowBg, Element.ALIGN_RIGHT));

            document.Add(itemsTable);
            document.Add(new Paragraph(" "));

            // Total section
            var totalTable = new PdfPTable(2) { WidthPercentage = 100 };
            totalTable.SetWidths(new float[] { 3, 1 });

            totalTable.AddCell(new PdfPCell(new Phrase("Tổng cùng", boldFont))
            {
                BackgroundColor = headerBg,
                Border = PdfRectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_RIGHT,
                Padding = 10
            });
            totalTable.AddCell(new PdfPCell(new Phrase(lblTotal.Text, boldFont))
            {
                BackgroundColor = headerBg,
                Border = PdfRectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_RIGHT,
                Padding = 10
            });

            document.Add(totalTable);

            document.Close();
            writer.Close();
        }

        private PdfPCell CreateTableCell(string text, iTextSharp.text.Font font, BaseColor bgColor, int alignment)
        {
            return new PdfPCell(new Phrase(text, font))
            {
                BackgroundColor = bgColor,
                Border = PdfRectangle.NO_BORDER,
                HorizontalAlignment = alignment,
                Padding = 10
            };
        }
    }
}

document.addEventListener('DOMContentLoaded', async (event) => {

    // --- 1. NÚT TOGGLE BỘ LỌC CHÍNH ---
    const toggleBtn = document.getElementById("toggleFiltersButton");
    const filtersContainer = document.getElementById("filtersContainer");

    if (toggleBtn && filtersContainer) {
        toggleBtn.addEventListener("click", function () {
            const isHidden = window.getComputedStyle(filtersContainer).display === "none";
            filtersContainer.style.display = isHidden ? "block" : "none";
            toggleBtn.classList.toggle("active", isHidden);
        });
    }

    // --- 2. TÌM KIẾM BÊN TRONG DROPDOWN ---
    document.querySelectorAll('.filter-dropdown-menu input[type="text"]').forEach(searchBox => {
        searchBox.addEventListener('keyup', (e) => {
            const searchTerm = e.target.value.toLowerCase();
            const listItems = e.target.closest('.filter-dropdown-menu').querySelectorAll('.filter-options-list li');

            listItems.forEach(item => {
                const label = item.querySelector('label');
                if (label) {
                    const text = label.textContent.toLowerCase();
                    item.style.display = text.includes(searchTerm) ? '' : 'none';
                }
            });
        });
    });

    // --- 3. KÍCH HOẠT HÀM LỌC KHI THAY ĐỔI ---
    document.getElementById('mainSearchInput')?.addEventListener('input', applyInvoiceFilters);
    document.querySelectorAll('.filter-checkbox').forEach(checkbox => {
        checkbox.addEventListener('change', applyInvoiceFilters);
    });

    // --- 4. GẮN SỰ KIỆN CLICK  ---
    document.getElementById('invoicesTableBody')?.addEventListener('click', async function (e) {

        // Xử lý nút "Set as Paid" -> now calls backend API
        const setAsPaidButton = e.target.closest('.action-set-as-paid a');
        if (setAsPaidButton) {
            e.preventDefault();
            const row = setAsPaidButton.closest('tr.invoice-row');
            if (!row) return;

            const invoiceId = row.dataset.invoiceId;
            if (!invoiceId) {
                alert('Invoice id not found');
                return;
            }

            const originalHtml = setAsPaidButton.innerHTML;
            setAsPaidButton.innerHTML = '<i class="fa-solid fa-spinner fa-spin fa-fw me-2"></i> Đang xử lý...';
            setAsPaidButton.classList.add('disabled');

            try {
                const resp = await fetch(`/api/invoice/${invoiceId}/mark-as-paid`, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' }
                });

                if (!resp.ok) {
                    let errText = 'Không thể cập nhật hóa đơn.';
                    try {
                        const j = await resp.json();
                        errText = j?.detail || j?.title || errText;
                    } catch { }
                    throw new Error(errText);
                }

                // update UI after success
                const statusBadge = row.querySelector('.invoice-status-badge');
                if (statusBadge) {
                    statusBadge.classList.remove('status-pending');
                    statusBadge.classList.add('status-paid');
                    statusBadge.textContent = 'Đã thanh toán';
                }

                const paidLi = setAsPaidButton.closest('li.action-set-as-paid');
                if (paidLi) {
                    paidLi.style.display = 'none';
                    const divider = paidLi.nextElementSibling;
                    if (divider && divider.classList.contains('action-divider')) {
                        divider.style.display = 'none';
                    }
                }

            } catch (err) {
                console.error(err);
                alert(err.message || 'Lỗi khi cập nhật hóa đơn.');
            } finally {
                setAsPaidButton.innerHTML = originalHtml;
                setAsPaidButton.classList.remove('disabled');
            }

            return;
        }

        // Xử lý nút "Download"
        const downloadButton = e.target.closest('.action-download-pdf');
        if (downloadButton) {
            e.preventDefault();
            const row = downloadButton.closest('tr.invoice-row');
            if (!row) return;

            // Thu thập dữ liệu từ data-attributes
            const invoiceData = {
                invoiceId: row.dataset.invoiceId,
                customerName: row.dataset.customerName,
                customerEmail: row.dataset.customerEmail,
                invoiceDate: row.dataset.invoiceDate,
                serviceName: row.dataset.serviceName,
                serviceQty: row.dataset.serviceQty,
                servicePrice: row.dataset.servicePrice,
                total: row.dataset.total,
            };

            // Thêm trạng thái loading cho nút download
            const originalText = downloadButton.innerHTML;
            downloadButton.innerHTML = '<i class="fa-solid fa-spinner fa-spin fa-fw me-2"></i> Đang tạo...';

            try {
                // Gọi hàm tạo PDF (bây giờ là async)
                await generateInvoicePDF(invoiceData);
            } catch (err) {
                console.error("Lỗi khi tạo PDF:", err);
                alert("Đã xảy ra lỗi khi tạo file PDF.");
            } finally {
                // Khôi phục lại nút
                downloadButton.innerHTML = originalText;
            }
        }
    });

    // --- FETCH & RENDER FILTER OPTIONS ---
    await loadInvoiceFilters();

}); 


/**
 * HÀM LỌC BẢNG
 */
function getSelectedFilterValues(containerId) {
    const values = [];
    document.querySelectorAll(`${containerId} .filter-checkbox:checked`).forEach(checkbox => {
        values.push(checkbox.value.toLowerCase());
    });
    return values;
}

function applyInvoiceFilters() {
    const searchTerm = document.getElementById('mainSearchInput').value.toLowerCase();
    const selectedCustomers = getSelectedFilterValues("#filterCustomer");
    const selectedEmployees = getSelectedFilterValues("#filterEmployee");
    const selectedServices = getSelectedFilterValues("#filterService");

    const selectedStatusesRaw = [];
    document.querySelectorAll("#filterStatus .filter-checkbox:checked").forEach(checkbox => {
        if (checkbox.value === "Paid") selectedStatusesRaw.push("đã thanh toán");
        if (checkbox.value === "Pending") selectedStatusesRaw.push("đang chờ");
        if (checkbox.value === "Archived") selectedStatusesRaw.push("đã lưu trữ");
    });

    const allRows = document.querySelectorAll("#invoicesTableBody tr.invoice-row");

    allRows.forEach(row => {
        const rowText = (row.textContent || row.innerText).toLowerCase();
        const cells = row.getElementsByTagName("td");
        const rowCustomer = cells[1] ? cells[1].innerText.toLowerCase() : '';
        const rowService = cells[3] ? cells[3].innerText.toLowerCase() : '';
        const rowStatus = cells[4] ? cells[4].innerText.toLowerCase().trim() : '';

        const searchMatch = rowText.includes(searchTerm);
        const customerMatch = selectedCustomers.length === 0 || selectedCustomers.includes(rowCustomer);
        const serviceMatch = selectedServices.length === 0 || selectedServices.includes(rowService);
        const statusMatch = selectedStatusesRaw.length === 0 || selectedStatusesRaw.includes(rowStatus);

        if (searchMatch && customerMatch && serviceMatch && statusMatch) {
            row.style.display = '';
        } else {
            row.style.display = 'none';
        }
    });
}

//
// --- FIX: THÊM HÀM TRỢ GIÚP CHUYỂN FONT SANG BASE64 ---
//
function arrayBufferToBase64(buffer) {
    let binary = '';
    const bytes = new Uint8Array(buffer);
    const len = bytes.byteLength;
    for (let i = 0; i < len; i++) {
        binary += String.fromCharCode(bytes[i]);
    }
    return window.btoa(binary);
}


/**
 * HÀM TẠO VÀ TẢI PDF
 */
async function generateInvoicePDF(data) {

    // 1. Kiểm tra thư viện
    if (typeof window.jspdf === 'undefined' || typeof window.jspdf.jsPDF === 'undefined') {
        console.error("jsPDF chưa được tải.");
        alert("Không thể tạo PDF. Vui lòng thử tải lại trang.");
        return;
    }

    const { jsPDF } = window.jspdf;
    const doc = new jsPDF();

    if (typeof doc.autoTable !== 'function') {
        console.error("Plugin jsPDF-AutoTable chưa được tải.");
        alert("Không thể tạo PDF vì thiếu plugin AutoTable.");
        return;
    }

    // --- FIX: TẢI FONT VÀ CHUYỂN SANG BASE64 ---
    try {
        const [regFontBuffer, boldFontBuffer] = await Promise.all([
            fetch('/fonts/Roboto-Regular.ttf').then(res => res.arrayBuffer()),
            fetch('/fonts/Roboto-Bold.ttf').then(res => res.arrayBuffer())
        ]);

        // Chuyển đổi ArrayBuffer sang Base64
        const regFontBase64 = arrayBufferToBase64(regFontBuffer);
        const boldFontBase64 = arrayBufferToBase64(boldFontBuffer);

        // Thêm font (đã ở dạng Base64) vào VFS
        doc.addFileToVFS('Roboto-Regular.ttf', regFontBase64);
        doc.addFileToVFS('Roboto-Bold.ttf', boldFontBase64);

        // Đăng ký font với jsPDF
        doc.addFont('Roboto-Regular.ttf', 'Roboto', 'normal');
        doc.addFont('Roboto-Bold.ttf', 'Roboto', 'bold');

    } catch (err) {
        console.error("Không thể tải font. Đảm bảo file /fonts/Roboto-Regular.ttf và Roboto-Bold.ttf tồn tại.", err);
        alert("Lỗi tải font. Không thể tạo PDF với Tiếng Việt.");
        return;
    }


    const brandColor = "#0d6efd";
    const textColor = "#212529";
    const lightBlueBg = [237, 245, 255];
    const margin = 20;

    // --- 1. Header ---
    doc.setFont('Roboto', 'bold'); // Sử dụng font Roboto
    doc.setFontSize(22);
    doc.setTextColor(brandColor);
    doc.text("dental", margin, 20);

    doc.setFontSize(18);
    doc.setTextColor(textColor);
    doc.text("Hóa đơn", 210 - margin, 20, { align: "right" });

    doc.setDrawColor(brandColor);
    doc.setLineWidth(0.5);
    doc.line(margin, 25, 210 - margin, 25);

    // --- 2. Thông tin Hóa đơn ---
    doc.setFont('Roboto', 'bold');
    doc.setFontSize(12);
    doc.setTextColor(textColor);
    doc.text(data.customerName, 210 - margin, 35, { align: "right" });

    doc.setFont('Roboto', 'normal');
    doc.setFontSize(10);
    doc.text(data.customerEmail, 210 - margin, 40, { align: "right" });

    doc.text(`Hóa đơn #${data.invoiceId}`, 210 - margin, 50, { align: "right" });
    doc.text(data.invoiceDate, 210 - margin, 55, { align: "right" });

    // --- 3. Bảng dữ liệu ---
    const tableHead = [["Mặt hàng", "Số lượng", "Đơn giá", "Thành tiền"]];
    const tableBody = [
        [data.serviceName, data.serviceQty, data.servicePrice, data.total]
    ];

    doc.autoTable({
        startY: 70,
        margin: { left: margin, right: margin },
        head: tableHead,
        body: tableBody,
        theme: 'striped',
        styles: {
            font: 'Roboto', // Sử dụng font Roboto
            fontSize: 10,
            cellPadding: 3,
            valign: 'middle'
        },
        headStyles: {
            fillColor: lightBlueBg,
            textColor: textColor,
            font: 'Roboto', // Sử dụng font Roboto
            fontStyle: 'bold'
        },
        didParseCell: function (data) {
            if (data.column.index > 0) {
                data.cell.styles.halign = 'right';
            }
        }
    });

    // --- 4. Tổng cùng (Total) ---
    const finalY = doc.lastAutoTable.finalY + 10;

    doc.setFillColor(lightBlueBg[0], lightBlueBg[1], lightBlueBg[2]);
    doc.rect(margin, finalY - 5, 210 - (margin * 2), 10, 'F');

    doc.setFont('Roboto', 'bold');
    doc.setFontSize(12);
    doc.setTextColor(textColor);
    doc.text("Tổng cùng", 150, finalY, { align: "right" });
    doc.text(data.total, 210 - margin, finalY, { align: "right" });

    // --- 5. Lưu file ---
    doc.save(`HoaDon-${data.invoiceId}.pdf`);
}

/**
 * FETCH AND RENDER FILTER OPTIONS
 */
async function loadInvoiceFilters() {
    await Promise.all([
        fetchAndRenderFilter('services', '#filterService'),
        fetchAndRenderFilter('employees', '#filterEmployee'),
        fetchAndRenderFilter('customers', '#filterCustomer'),
        fetchAndRenderFilter('statuses', '#filterStatus')
    ]);
}

async function fetchAndRenderFilter(type, containerSelector) {
    let url = `/api/invoice/filters/${type}`;
    try {
        const resp = await fetch(url);
        if (!resp.ok) throw new Error('Lỗi tải dữ liệu bộ lọc');
        const data = await resp.json();
        renderFilterOptions(type, data, containerSelector);
    } catch (err) {
        console.error(`Lỗi tải bộ lọc ${type}:`, err);
    }
}

function renderFilterOptions(type, items, containerSelector) {
    const container = document.querySelector(containerSelector);
    if (!container) return;
    const list = container.querySelector('.filter-options-list');
    if (!list) return;
    list.innerHTML = '';
    items.forEach((item, idx) => {
        const id = `${type}_${idx}`;
        let label = item;
        if (type === 'statuses') {
            // Map status to Vietnamese if needed
            if (item.toLowerCase() === 'paid') label = 'Đã thanh toán';
            else if (item.toLowerCase() === 'pending') label = 'Đang chờ';
            else if (item.toLowerCase() === 'archived') label = 'Đã lưu trữ';
        }
        const li = document.createElement('li');
        li.innerHTML = `
            <div class="form-check">
                <input class="form-check-input filter-checkbox" type="checkbox" value="${item}" id="${id}">
                <label class="form-check-label" for="${id}">${label}</label>
            </div>
        `;
        list.appendChild(li);
    });
    // Gắn lại sự kiện cho checkbox mới
    list.querySelectorAll('.filter-checkbox').forEach(checkbox => {
        checkbox.addEventListener('change', applyInvoiceFilters);
    });
}
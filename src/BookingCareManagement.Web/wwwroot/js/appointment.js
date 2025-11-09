let fpStart = null;
let fpEnd = null;

function getSelectedFilterValues(containerId) {
    const values = [];
    document.querySelectorAll(`${containerId} .filter-checkbox:checked`).forEach(checkbox => {
        values.push(checkbox.value);
    });
    return values;
}

function applyFilters() {
    // 1. Lấy tất cả giá trị đang được lọc
    const searchTerm = document.getElementById('mainSearchInput').value.toLowerCase();
    const startDate = fpStart.selectedDates[0];
    const endDate = fpEnd.selectedDates[0];

    const selectedServices = getSelectedFilterValues("#filterService");
    const selectedCustomers = getSelectedFilterValues("#filterCustomer");
    const selectedEmployees = getSelectedFilterValues("#filterEmployee");
    const selectedStatuses = getSelectedFilterValues("#filterStatus");

    const allRows = document.querySelectorAll("#appointmentsTableBody tr.appointment-row");

    allRows.forEach(row => {
        // 2. Lấy dữ liệu của hàng
        const rowText = row.textContent || row.innerText;
        const rowDateISO = row.dataset.dateIso;
        const rowService = row.dataset.service;
        const rowCustomer = row.dataset.customer;
        const rowEmployee = row.dataset.employee;
        const rowStatus = row.dataset.status;

        // 3. Kiểm tra từng điều kiện
        const searchMatch = rowText.toLowerCase().includes(searchTerm);

        let dateMatch = true;
        if (rowDateISO) {
            const rowDate = new Date(rowDateISO + "T00:00:00");
            if (startDate && new Date(startDate.setHours(0, 0, 0, 0)) > rowDate) {
                dateMatch = false;
            }
            if (endDate && new Date(endDate.setHours(0, 0, 0, 0)) < rowDate) {
                dateMatch = false;
            }
        } else if (startDate || endDate) {
            dateMatch = false;
        }

        const serviceMatch = selectedServices.length === 0 || selectedServices.includes(rowService);
        const customerMatch = selectedCustomers.length === 0 || selectedCustomers.includes(rowCustomer);
        const employeeMatch = selectedEmployees.length === 0 || selectedEmployees.includes(rowEmployee);
        const statusMatch = selectedStatuses.length === 0 || selectedStatuses.includes(rowStatus);

        // 4. Ẩn hoặc hiện hàng
        if (searchMatch && dateMatch && serviceMatch && customerMatch && employeeMatch && statusMatch) {
            row.style.display = '';
        } else {
            row.style.display = 'none';
        }
    });

    // 5. Cập nhật lại số lượng & hiển thị
    updateAllCounts();
}

/**
 * Cập nhật số đếm, ẩn/hiện bảng & thông báo “No appointments”
 * (CODE CỦA BẠN - ĐÃ ĐÚNG)
 */
function updateAllCounts() {
    let totalVisibleCount = 0;
    const dateHeaders = document.querySelectorAll("#appointmentsTableBody tr.table-group-header");

    dateHeaders.forEach(header => {
        const dateGroup = header.dataset.dateGroup;
        const visibleRowsInGroup = document.querySelectorAll(
            `tr.appointment-row[data-date-group="${dateGroup}"]:not([style*="display: none"])`
        );
        const count = visibleRowsInGroup.length;

        header.querySelector('.day-count').innerText = count;
        header.style.display = (count === 0) ? 'none' : '';
        totalVisibleCount += count;
    });

    document.getElementById('totalAppointmentsCount').innerText = totalVisibleCount;

    // --- FIX CỦA BẠN (ĐÃ ĐÚNG) ---
    const tableContainer = document.getElementById('appointmentsTableContainer');
    const noAppointmentsBlock = document.getElementById('noAppointmentsBlock');

    if (tableContainer && noAppointmentsBlock) {
        const anyVisible = totalVisibleCount > 0;
        tableContainer.classList.toggle("d-none", !anyVisible);
        noAppointmentsBlock.classList.toggle("d-none", anyVisible);

        // Sửa lỗi layout (thêm display flex cho noAppointmentsBlock nếu nó được hiện)
        if (!anyVisible) {
            noAppointmentsBlock.classList.add("d-flex");
        } else {
            noAppointmentsBlock.classList.remove("d-flex");
        }
    }
}

function updateStatusButton(button, option) {
    const newValue = option.dataset.value;
    const newClass = option.dataset.class;
    const newIcon = option.dataset.icon;

    button.innerHTML = `<i class="fa-solid ${newIcon}"></i> ${newValue}`;
    button.className = `btn btn-sm btn-outline-light dropdown-toggle d-flex align-items-center gap-2 rounded-pill status-badge ${newClass}`;
}


// --- PHẦN 2: KHỞI TẠO SỰ KIỆN ---

document.addEventListener('DOMContentLoaded', (event) => {

    // Tooltip Bootstrap
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Nút toggle bộ lọc
    const toggleBtn = document.getElementById("toggleFiltersButton");
    const filtersContainer = document.getElementById("filtersContainer");

    if (toggleBtn && filtersContainer) {
        toggleBtn.addEventListener("click", function () {
            const isHidden = window.getComputedStyle(filtersContainer).display === "none";
            filtersContainer.style.display = isHidden ? "block" : "none";
            toggleBtn.classList.toggle("active", isHidden);
        });
    } else {
        console.warn("Không tìm thấy nút 'toggleFiltersButton' hoặc 'filtersContainer'.");
    }

    // Flatpickr
    if (typeof flatpickr !== 'undefined') {

        fpStart = flatpickr("#startDate", {
            dateFormat: "M j, Y",
            defaultDate: "today",
            onChange: (selectedDates) => {
                if (fpEnd) fpEnd.set('minDate', selectedDates[0]);
                applyFilters();
            }
        });
        fpEnd = flatpickr("#endDate", {
            dateFormat: "M j, Y",
            defaultDate: new Date().fp_incr(7),
            onChange: (selectedDates) => {
                if (fpStart) fpStart.set('maxDate', selectedDates[0]);
                applyFilters();
            }
        });

        setTimeout(applyFilters, 100);

    } else {
        console.error("Thư viện Flatpickr CHƯA ĐƯỢC TẢI.");
    }

    // Ô tìm kiếm chính
    document.getElementById('mainSearchInput')?.addEventListener('input', applyFilters);

    // Bộ lọc trong dropdown
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

    // Checkbox "chọn tất cả"
    document.getElementById('checkAllRows')?.addEventListener('change', (e) => {
        document.querySelectorAll('tbody tr.appointment-row:not([style*="display: none"]) .form-check-input').forEach(checkbox => {
            checkbox.checked = e.target.checked;
        });
    });

    // Checkbox bộ lọc
    document.querySelectorAll('.filter-checkbox').forEach(checkbox => {
        checkbox.addEventListener('change', applyFilters);
    });

    // Dropdown trạng thái (auto close + cập nhật)
    document.querySelectorAll('.status-option').forEach(option => {
        option.addEventListener('click', function (e) {
            e.preventDefault();

            const dropdownButton = this.closest('.status-dropdown').querySelector('.dropdown-toggle');
            updateStatusButton(dropdownButton, this);

            const parentRow = this.closest('tr.appointment-row');
            if (parentRow) {
                parentRow.dataset.status = this.dataset.value;
            }

            const dropdownInstance = bootstrap.Dropdown.getInstance(dropdownButton);
            if (dropdownInstance) dropdownInstance.hide();

            applyFilters();
        });
    });

    // 
    // FIX 3: THAY THẾ CODE XỬ LÝ NOTE MODAL BẰNG NOTE DROPDOWN
    //
    function closeNoteDropdown(button) {
        const dropdownButton = button.closest('.dropdown').querySelector('[data-bs-toggle="dropdown"]');
        const dropdownInstance = bootstrap.Dropdown.getInstance(dropdownButton);
        if (dropdownInstance) {
            dropdownInstance.hide();
        }
    }

    // Xử lý nút "Save Note" (dùng class)
    document.querySelectorAll('.save-note-btn').forEach(button => {
        button.addEventListener('click', function () {
            const dropdownMenu = this.closest('.dropdown-menu');
            const noteTextarea = dropdownMenu.querySelector('.note-textarea');

            console.log("Note saved:", noteTextarea.value);

            // (Đây là nơi bạn sẽ gọi API để lưu)

            // Xóa text
            noteTextarea.value = '';

            // Đóng dropdown
            closeNoteDropdown(this);
        });
    });

    // Xử lý nút "Cancel Note" (dùng class)
    document.querySelectorAll('.cancel-note-btn').forEach(button => {
        button.addEventListener('click', function () {
            const dropdownMenu = this.closest('.dropdown-menu');
            const noteTextarea = dropdownMenu.querySelector('.note-textarea');

            // Xóa text (nếu người dùng đã gõ gì đó)
            noteTextarea.value = '';

            // Đóng dropdown
            closeNoteDropdown(this);
        });
    });

});
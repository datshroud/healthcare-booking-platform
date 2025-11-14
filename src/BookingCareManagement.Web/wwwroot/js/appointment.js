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

    const tableContainer = document.getElementById('appointmentsTableContainer');
    const noAppointmentsBlock = document.getElementById('noAppointmentsBlock');

    if (tableContainer && noAppointmentsBlock) {
        const anyVisible = totalVisibleCount > 0;
        tableContainer.classList.toggle("d-none", !anyVisible);
        noAppointmentsBlock.classList.toggle("d-none", anyVisible);

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

    function closeNoteDropdown(button) {
        const dropdownButton = button.closest('.dropdown').querySelector('[data-bs-toggle="dropdown"]');
        const dropdownInstance = bootstrap.Dropdown.getInstance(dropdownButton);
        if (dropdownInstance) {
            dropdownInstance.hide();
        }
    }

    document.querySelectorAll('.save-note-btn').forEach(button => {
        button.addEventListener('click', function () {
            const dropdownMenu = this.closest('.dropdown-menu');
            const noteTextarea = dropdownMenu.querySelector('.note-textarea');
            console.log("Note saved:", noteTextarea.value);
            noteTextarea.value = '';
            closeNoteDropdown(this);
        });
    });

    document.querySelectorAll('.cancel-note-btn').forEach(button => {
        button.addEventListener('click', function () {
            const dropdownMenu = this.closest('.dropdown-menu');
            const noteTextarea = dropdownMenu.querySelector('.note-textarea');
            noteTextarea.value = '';
            closeNoteDropdown(this);
        });
    });


    let tomService = null;
    let tomEmployee = null;
    let tomTime = null;
    let tomCustomer = null;
    let fpModalDate = null;

    // 5.1. Hàm tạo các lựa chọn thời gian (9:00 AM - 4:00 PM, 30 phút)
    function generateTimeOptions() {
        const options = [];
        const startTime = 9 * 60; // 9:00 AM
        const endTime = 16 * 60; // 4:00 PM (16:00)
        const interval = 30; // 30 phút

        for (let minutes = startTime; minutes <= endTime; minutes += interval) {
            const hour = Math.floor(minutes / 60);
            const min = minutes % 60;
            const ampm = hour >= 12 ? 'PM' : 'AM';
            const displayHour = hour % 12 === 0 ? 12 : hour % 12;
            const displayMin = min === 0 ? '00' : min;

            const timeValue = `${displayHour}:${displayMin} ${ampm}`;
            options.push({
                value: timeValue,
                text: timeValue
            });
        }
        return options;
    }

    if (typeof TomSelect !== 'undefined') {
        const tomSettings = {
            create: false,
            sortField: { field: "text", direction: "asc" }
        };

        tomService = new TomSelect("#modalServiceSelect", tomSettings);
        tomEmployee = new TomSelect("#modalEmployeeSelect", tomSettings);
        tomCustomer = new TomSelect("#modalCustomerSelect", tomSettings);

        tomTime = new TomSelect("#modalTimeSelect", {
            options: generateTimeOptions(), 
            create: false,
        });
    }

    // Khởi tạo Flatpickr cho Date
    fpModalDate = flatpickr("#modalDateSelect", {
        dateFormat: "Y-m-d", 
    });


    const newApptModalEl = document.getElementById('newAppointmentModal');
    const newApptForm = document.getElementById('newAppointmentForm');
    const saveApptButton = document.getElementById('saveAppointmentButton');

    if (newApptModalEl) {
        newApptModalEl.addEventListener('hidden.bs.modal', function () {
            if (newApptForm) {
                newApptForm.reset();
            }

            if (tomService) tomService.clear();
            if (tomEmployee) tomEmployee.clear();
            if (tomTime) tomTime.clear();
            if (tomCustomer) tomCustomer.clear();

            if (fpModalDate) fpModalDate.clear();
        });
    }

    if (saveApptButton) {
        saveApptButton.addEventListener('click', function () {
            const service = tomService.getValue();
            const employee = tomEmployee.getValue();
            const time = tomTime.getValue();
            const customer = tomCustomer.getValue();
            const date = fpModalDate.selectedDates[0];

            console.log("Saving new appointment:", {
                customer,
                employee,
                service,
                date,
                time
            });

            const modalInstance = bootstrap.Modal.getInstance(newApptModalEl);
            modalInstance.hide();

        });
    }

});
const MIN_APPOINTMENT_LEAD_DAYS = 2;
const DEFAULT_APPOINTMENT_DURATION = 30;

let fpStart = null;
let fpEnd = null;
let newAppointmentAlertBox = null;
let tomService = null;
let tomEmployee = null;
let tomTime = null;
let tomCustomer = null;
let fpModalDate = null;
let manualPhoneWrapper = null;
let manualPhoneInput = null;
let adminModalEventsBound = false;
let appointmentModalMode = 'create';
let editingAppointmentId = null;
let editingAppointmentMeta = { durationMinutes: DEFAULT_APPOINTMENT_DURATION, clinicRoomId: null };
let modalStatusSelect = null;
let modalStatusFieldEl = null;
let modalTitleEl = null;
let newAppointmentModalEl = null;
let saveAppointmentButtonEl = null;

const appointmentModes = {
    doctor: {
        metadata: "/api/doctor/appointments/metadata",
        root: "/api/doctor/appointments"
    },
    admin: {
        metadata: "/api/admin/appointments/metadata",
        root: "/api/admin/appointments"
    }
};

const appointmentState = {
    mode: document.body?.dataset?.appointmentMode || 'doctor',
    metadata: null,
    appointments: [],
    isInitialized: false,
    isLoading: false
};

const statusToneClassMap = {
    pending: { badge: "text-warning border-warning-subtle", option: "text-warning" },
    approved: { badge: "text-success border-success-subtle", option: "text-success" },
    canceled: { badge: "text-secondary border-secondary-subtle", option: "text-secondary" },
    rejected: { badge: "text-danger border-danger-subtle", option: "text-danger" },
    noshow: { badge: "text-dark border-dark-subtle", option: "text-dark" }
};

let appointmentsRequestController = null;

function getMinimumAppointmentDate() {
    const minDate = new Date();
    minDate.setHours(0, 0, 0, 0);
    minDate.setDate(minDate.getDate() + MIN_APPOINTMENT_LEAD_DAYS);
    return minDate;
}

function showNewAppointmentAlert(message, variant = 'warning') {
    if (!newAppointmentAlertBox) {
        newAppointmentAlertBox = document.getElementById('newAppointmentAlert');
    }

    if (!newAppointmentAlertBox) {
        if (message) {
            console.warn(message);
        }
        return;
    }

    if (!message) {
        newAppointmentAlertBox.classList.add('d-none');
        newAppointmentAlertBox.textContent = '';
        return;
    }

    newAppointmentAlertBox.textContent = message;
    newAppointmentAlertBox.className = `alert alert-${variant}`;
    newAppointmentAlertBox.classList.remove('d-none');
}

function resetModalTimeSelect(tomInstance) {
    if (!tomInstance) {
        return;
    }
    tomInstance.clear(true);
    tomInstance.clearOptions();
    tomInstance.disable();
    tomInstance.input?.setAttribute('placeholder', 'Vui lòng chọn ngày trước');
}

function resolveAppointmentEndpoints() {
    return appointmentState.mode === 'admin'
        ? appointmentModes.admin
        : appointmentModes.doctor;
}

function getSelectedFilterValues(containerId) {
    const values = [];
    document.querySelectorAll(`${containerId} .filter-checkbox:checked`).forEach(checkbox => {
        values.push(checkbox.value);
    });
    return values;
}

function applyFilters() {
    const searchTerm = (document.getElementById('mainSearchInput')?.value || '').toLowerCase();
    const startDate = fpStart?.selectedDates?.[0] ? new Date(fpStart.selectedDates[0]) : null;
    const endDate = fpEnd?.selectedDates?.[0] ? new Date(fpEnd.selectedDates[0]) : null;

    if (startDate) {
        startDate.setHours(0, 0, 0, 0);
    }
    if (endDate) {
        endDate.setHours(23, 59, 59, 999);
    }

    const selectedServices = getSelectedFilterValues('#filterService');
    const selectedCustomers = getSelectedFilterValues('#filterCustomer');
    const selectedEmployees = getSelectedFilterValues('#filterEmployee');
    const selectedStatuses = getSelectedFilterValues('#filterStatus');

    const allRows = document.querySelectorAll('#appointmentsTableBody tr.appointment-row');

    allRows.forEach(row => {
        const rowText = row.textContent || row.innerText || '';
        const rowDateISO = row.dataset.dateIso;
        const rowService = row.dataset.service;
        const rowCustomer = row.dataset.customer;
        const rowEmployee = row.dataset.employee;
        const rowStatus = row.dataset.status;

        const searchMatch = rowText.toLowerCase().includes(searchTerm);

        let dateMatch = true;
        if (rowDateISO) {
            const rowDate = new Date(`${rowDateISO}T00:00:00`);
            if (startDate && startDate > rowDate) {
                dateMatch = false;
            }
            if (endDate && endDate < rowDate) {
                dateMatch = false;
            }
        } else if (startDate || endDate) {
            dateMatch = false;
        }

        const serviceMatch = selectedServices.length === 0 || selectedServices.includes(rowService);
        const customerMatch = selectedCustomers.length === 0 || selectedCustomers.includes(rowCustomer);
        const employeeMatch = selectedEmployees.length === 0 || selectedEmployees.includes(rowEmployee);
        const statusMatch = selectedStatuses.length === 0 || selectedStatuses.includes(rowStatus);

        row.style.display = (searchMatch && dateMatch && serviceMatch && customerMatch && employeeMatch && statusMatch)
            ? ''
            : 'none';
    });

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
    const anyVisible = totalVisibleCount > 0;

    if (tableContainer) {
        tableContainer.classList.toggle('d-none', !anyVisible);
    }

    if (noAppointmentsBlock) {
        noAppointmentsBlock.classList.toggle('d-none', anyVisible);
        noAppointmentsBlock.classList.toggle('d-flex', !anyVisible);
    }
}

function resolveToneStyles(tone) {
    if (!tone) {
        return statusToneClassMap.pending;
    }

    const key = tone.toLowerCase();
    return statusToneClassMap[key] || statusToneClassMap.pending;
}

function updateStatusButton(button, option) {
    const label = option.dataset.label || option.dataset.value || '';
    const icon = option.dataset.icon || 'fa-circle';
    const toneStyles = resolveToneStyles(option.dataset.tone);

    button.innerHTML = `<i class="fa-solid ${icon}"></i> ${label}`;
    button.className = `btn btn-sm btn-outline-light dropdown-toggle d-flex align-items-center gap-2 rounded-pill status-badge ${toneStyles.badge}`;
    button.dataset.statusCode = option.dataset.value;
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
                if (fpEnd && selectedDates[0]) {
                    fpEnd.set('minDate', selectedDates[0]);
                }
                handleDateRangeChanged();
            }
        });
        fpEnd = flatpickr("#endDate", {
            dateFormat: "M j, Y",
            defaultDate: new Date().fp_incr(7),
            onChange: (selectedDates) => {
                if (fpStart && selectedDates[0]) {
                    fpStart.set('maxDate', selectedDates[0]);
                }
                handleDateRangeChanged();
            }
        });

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

    attachFilterCheckboxHandlers();

    bindStatusDropdowns();

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
            console.log("Đã lưu ghi chú:", noteTextarea.value);
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


    function handleModalDateSelection(selectedDates) {
        if (!tomTime) {
            return;
        }

        if (!selectedDates.length) {
            resetModalTimeSelect(tomTime);
            return;
        }

        const selectedDate = selectedDates[0];
        const minDate = getMinimumAppointmentDate();
        if (appointmentModalMode !== 'edit' && selectedDate < minDate) {
            showNewAppointmentAlert(`Vui lòng chọn ngày khám sau tối thiểu ${MIN_APPOINTMENT_LEAD_DAYS} ngày kể từ hôm nay.`);
            if (fpModalDate) {
                fpModalDate.clear();
            }
            resetModalTimeSelect(tomTime);
            return;
        }

        showNewAppointmentAlert(null);
        loadAvailableTimeSlots();
    }

    if (typeof TomSelect !== 'undefined') {
        const tomSettings = {
            create: false,
            sortField: { field: "text", direction: "asc" }
        };

        const serviceSelect = document.getElementById('modalServiceSelect');
        const doctorSelect = document.getElementById('modalEmployeeSelect');
        const patientSelect = document.getElementById('modalCustomerSelect');
        const timeSelect = document.getElementById('modalTimeSelect');

        if (serviceSelect) {
            tomService = new TomSelect(serviceSelect, tomSettings);
        }

        if (doctorSelect) {
            tomEmployee = new TomSelect(doctorSelect, tomSettings);
        }

        if (patientSelect) {
            tomCustomer = new TomSelect(patientSelect, tomSettings);
        }

        [tomService, tomEmployee, tomCustomer].forEach(instance => instance?.disable());

        if (timeSelect) {
            tomTime = new TomSelect(timeSelect, {
                options: [],
                create: false,
                placeholder: 'Chọn khung giờ...'
            });
            resetModalTimeSelect(tomTime);
        }
    }

    // Khởi tạo Flatpickr cho Date
    fpModalDate = flatpickr("#modalDateSelect", {
        dateFormat: "Y-m-d",
        defaultDate: getMinimumAppointmentDate(),
        minDate: getMinimumAppointmentDate(),
        onChange: handleModalDateSelection,
        onOpen: () => showNewAppointmentAlert(null)
    });


    const newApptModalEl = document.getElementById('newAppointmentModal');
    const newApptForm = document.getElementById('newAppointmentForm');
    const saveApptButton = document.getElementById('saveAppointmentButton');
    manualPhoneWrapper = document.getElementById('manualPhoneWrapper');
    manualPhoneInput = document.getElementById('manualPhoneInput');
    modalStatusSelect = document.getElementById('modalStatusSelect');
    const modalStatusField = document.getElementById('modalStatusField');
    const modalTitle = document.getElementById('newAppointmentModalLabel');
    const newAppointmentTrigger = document.getElementById('newAppointmentButton');

    newAppointmentModalEl = newApptModalEl;
    saveAppointmentButtonEl = saveApptButton;
    modalStatusFieldEl = modalStatusField;
    modalTitleEl = modalTitle;

    if (newAppointmentTrigger) {
        newAppointmentTrigger.addEventListener('click', () => {
            appointmentModalMode = 'create';
            editingAppointmentId = null;
            editingAppointmentMeta = { durationMinutes: DEFAULT_APPOINTMENT_DURATION, clinicRoomId: null };
            if (modalTitleEl) {
                modalTitleEl.textContent = 'Thêm cuộc hẹn';
            }
            if (saveAppointmentButtonEl) {
                saveAppointmentButtonEl.textContent = 'Lưu';
            }
            if (modalStatusFieldEl) {
                modalStatusFieldEl.classList.add('d-none');
            }
            if (modalStatusSelect && appointmentState.metadata?.statuses?.length) {
                modalStatusSelect.value = appointmentState.metadata.statuses[0].code;
            }
        });
    }

    if (newApptModalEl) {
        newApptModalEl.addEventListener('hidden.bs.modal', function () {
            if (newApptForm) {
                newApptForm.reset();
            }

            if (tomService) tomService.clear();
            if (tomEmployee) tomEmployee.clear();
            resetModalTimeSelect(tomTime);
            if (tomCustomer) tomCustomer.clear();

            if (appointmentState.mode === 'doctor') {
                populateDoctorNewAppointmentOptions(appointmentState.metadata);
            }

            if (fpModalDate) {
                fpModalDate.clear();
                fpModalDate.set('minDate', getMinimumAppointmentDate());
            }

            showNewAppointmentAlert(null);
            resetManualPhoneCapture();
            appointmentModalMode = 'create';
            editingAppointmentId = null;
            editingAppointmentMeta = { durationMinutes: DEFAULT_APPOINTMENT_DURATION, clinicRoomId: null };
            if (modalStatusFieldEl) {
                modalStatusFieldEl.classList.add('d-none');
            }
            if (modalStatusSelect && appointmentState.metadata?.statuses?.length) {
                modalStatusSelect.value = appointmentState.metadata.statuses[0].code;
            }
            if (modalTitleEl) {
                modalTitleEl.textContent = 'Thêm cuộc hẹn';
            }
            if (saveAppointmentButtonEl) {
                saveAppointmentButtonEl.textContent = 'Lưu';
            }
        });

        newApptModalEl.addEventListener('show.bs.modal', function () {
            if (appointmentModalMode === 'create') {
                const minDate = getMinimumAppointmentDate();
                if (fpModalDate) {
                    fpModalDate.set('minDate', minDate);
                    fpModalDate.setDate(minDate, true);
                }
                resetModalTimeSelect(tomTime);
            }
            showNewAppointmentAlert(null);
            resetManualPhoneCapture();
            manualPhoneInput?.classList.remove('is-invalid');
        });
    }

    if (saveApptButton) {
        saveApptButton.addEventListener('click', async function () {
            const specialtyId = tomService?.getValue();
            const doctorId = tomEmployee?.getValue();
            const time = tomTime?.getValue();
            const patientId = tomCustomer?.getValue();
            const date = fpModalDate?.selectedDates?.[0];
            const manualPhone = (manualPhoneInput?.value || '').trim();
            const statusCode = modalStatusSelect?.value;

            if (!specialtyId || !doctorId || !patientId) {
                showNewAppointmentAlert('Vui lòng chọn đầy đủ chuyên khoa, bác sĩ và bệnh nhân.');
                return;
            }

            if (!date) {
                showNewAppointmentAlert('Vui lòng chọn ngày khám phù hợp.');
                return;
            }

            const minDate = getMinimumAppointmentDate();
            if (date < minDate) {
                showNewAppointmentAlert(`Ngày khám phải cách hiện tại ít nhất ${MIN_APPOINTMENT_LEAD_DAYS} ngày.`);
                return;
            }

            if (!time) {
                showNewAppointmentAlert('Vui lòng chọn khung giờ sau khi đã chọn ngày.');
                return;
            }

            const slotStartIso = normalizeSlotStartIso(time);
            if (!slotStartIso) {
                showNewAppointmentAlert('Khung giờ bạn chọn không hợp lệ, vui lòng thử lại.');
                return;
            }

            if (appointmentState.mode === 'admin' && patientRequiresManualPhone(patientId) && !manualPhone) {
                showNewAppointmentAlert('Bệnh nhân chưa có số điện thoại, vui lòng nhập để tiếp tục.');
                manualPhoneInput?.classList.add('is-invalid');
                manualPhoneInput?.focus();
                return;
            }

            let requestPayload;
            try {
                requestPayload = buildAdminAppointmentRequest({
                    specialtyId,
                    doctorId,
                    patientId,
                    slotStartIso,
                    manualPhone,
                    statusCode,
                    durationMinutes: DEFAULT_APPOINTMENT_DURATION,
                    clinicRoomId: appointmentModalMode === 'edit'
                        ? editingAppointmentMeta?.clinicRoomId || null
                        : null,
                    appointmentId: appointmentModalMode === 'edit' ? editingAppointmentId : null
                });
            } catch (error) {
                showNewAppointmentAlert(error?.message || 'Không thể chuẩn bị dữ liệu cuộc hẹn.');
                return;
            }

            const previousButtonText = saveApptButton.innerHTML;
            saveApptButton.disabled = true;
            saveApptButton.innerHTML = '<span class="spinner-border spinner-border-sm me-2" role="status"></span>Đang lưu...';
            showNewAppointmentAlert(null);

            try {
                await submitAppointmentMutation(requestPayload);
                const modalInstance = bootstrap.Modal.getInstance(newApptModalEl);
                modalInstance?.hide();
                await reloadDoctorAppointments();
            } catch (error) {
                const fallbackMessage = appointmentModalMode === 'edit'
                    ? 'Không thể cập nhật cuộc hẹn.'
                    : 'Không thể tạo cuộc hẹn mới.';
                showNewAppointmentAlert(error?.message || fallbackMessage, 'danger');
            } finally {
                saveApptButton.disabled = false;
                saveApptButton.innerHTML = previousButtonText;
            }
        });
    }

    initializeDoctorAppointmentScreen();

});

function bindStatusDropdowns(scope = document) {
    scope.querySelectorAll('.status-option').forEach(option => {
        if (option.dataset.statusBound === '1') {
            return;
        }

        option.dataset.statusBound = '1';
        option.addEventListener('click', handleStatusOptionClick);
    });
}

function bindAppointmentActionMenus(scope = document) {
    if (appointmentState.mode !== 'admin') {
        return;
    }

    scope.querySelectorAll('.edit-appointment-action').forEach(button => {
        if (button.dataset.actionBound === '1') {
            return;
        }

        button.dataset.actionBound = '1';
        button.addEventListener('click', (event) => {
            event.preventDefault();
            const dropdownToggle = button.closest('.dropdown')?.querySelector('.dropdown-toggle');
            if (dropdownToggle) {
                const dropdownInstance = bootstrap.Dropdown.getInstance(dropdownToggle);
                dropdownInstance?.hide();
            }

            const appointmentId = button.dataset.appointmentId;
            openEditAppointmentModal(appointmentId);
        });
    });
}

async function handleStatusOptionClick(event) {
    event.preventDefault();
    const optionElement = this;
    const dropdownButton = optionElement.closest('.status-dropdown')?.querySelector('.dropdown-toggle');
    if (!dropdownButton) {
        return;
    }

    const parentRow = dropdownButton.closest('tr.appointment-row');
    const appointmentId = parentRow?.dataset.appointmentId;
    const selectedStatus = optionElement.dataset.value;
    if (!appointmentId || !selectedStatus) {
        return;
    }

    if (dropdownButton.dataset.statusCode === selectedStatus) {
        const dropdownInstance = bootstrap.Dropdown.getInstance(dropdownButton);
        dropdownInstance?.hide();
        return;
    }

    toggleStatusButtonLoading(dropdownButton, true);
    try {
        const updated = await submitStatusUpdate(appointmentId, selectedStatus);
        const normalized = upsertAppointmentState(updated);
        updateStatusButton(dropdownButton, optionElement);
        dropdownButton.dataset.statusCode = normalized.status;
        if (dropdownButton.dataset.originalContent) {
            dropdownButton.dataset.originalContent = dropdownButton.innerHTML;
        }
        if (parentRow) {
            parentRow.dataset.status = normalized.status;
        }
        showAppointmentsError(null);
        applyFilters();
    } catch (error) {
        showAppointmentsError(error?.message || 'Không thể cập nhật trạng thái cuộc hẹn.');
    } finally {
        toggleStatusButtonLoading(dropdownButton, false);
        const dropdownInstance = bootstrap.Dropdown.getInstance(dropdownButton);
        dropdownInstance?.hide();
    }
}

function attachFilterCheckboxHandlers(scope = document) {
    scope.querySelectorAll('.filter-checkbox').forEach(checkbox => {
        if (checkbox.dataset.filterBound === '1') {
            return;
        }

        checkbox.dataset.filterBound = '1';
        checkbox.addEventListener('change', applyFilters);
    });
}

function setDoctorAppointmentsLoading(isLoading) {
    appointmentState.isLoading = isLoading;
    const loadingBlock = document.getElementById('appointmentsLoadingState');
    const tableContainer = document.getElementById('appointmentsTableContainer');
    const noAppointmentsBlock = document.getElementById('noAppointmentsBlock');

    if (loadingBlock) {
        loadingBlock.classList.toggle('d-none', !isLoading);
        loadingBlock.classList.toggle('d-flex', isLoading);
    }

    if (tableContainer) {
        if (isLoading) {
            tableContainer.classList.add('d-none');
        } else {
            tableContainer.classList.remove('d-none');
        }
    }

    if (noAppointmentsBlock && isLoading) {
        noAppointmentsBlock.classList.add('d-none');
        noAppointmentsBlock.classList.remove('d-flex');
    }
}

function showAppointmentsError(message) {
    const alertBox = document.getElementById('appointmentsError');
    if (!alertBox) {
        return;
    }

    if (!message) {
        alertBox.classList.add('d-none');
        alertBox.textContent = '';
        return;
    }

    alertBox.textContent = message;
    alertBox.classList.remove('d-none');
}

async function initializeDoctorAppointmentScreen() {
    try {
        setDoctorAppointmentsLoading(true);
        showAppointmentsError(null);
        const metadata = await fetchDoctorAppointmentMetadata();
        const normalizedMetadata = {
            doctorId: metadata?.doctorId ?? metadata?.DoctorId ?? null,
            doctorName: metadata?.doctorName ?? metadata?.DoctorName ?? '',
            specialties: (metadata?.specialties ?? metadata?.Specialties ?? []).map(mapSpecialtyDto),
            statuses: (metadata?.statuses ?? metadata?.Statuses ?? []).map(mapStatusDto),
            doctors: (metadata?.doctors ?? metadata?.Doctors ?? []).map(mapDoctorOptionDto),
            patients: (metadata?.patients ?? metadata?.Patients ?? []).map(mapPatientOptionDto)
        };
        appointmentState.metadata = normalizedMetadata;
        populateStatusFilterOptions(normalizedMetadata.statuses);
        populateModalStatusOptions(normalizedMetadata.statuses);
        if (appointmentState.mode === 'admin') {
            populateAdminNewAppointmentOptions(normalizedMetadata);
        } else {
            populateDoctorNewAppointmentOptions(normalizedMetadata);
        }
        attachFilterCheckboxHandlers();
        await reloadDoctorAppointments();
    } catch (error) {
        setDoctorAppointmentsLoading(false);
        showAppointmentsError(error?.message || 'Không thể tải dữ liệu cuộc hẹn.');
    }
}

async function fetchDoctorAppointmentMetadata() {
    const endpoints = resolveAppointmentEndpoints();
    const response = await fetch(endpoints.metadata, { credentials: 'include' });
    if (!response.ok) {
        throw await buildApiError(response);
    }

    return await response.json();
}

async function reloadDoctorAppointments() {
    if (!appointmentState.metadata) {
        return;
    }

    if (appointmentsRequestController) {
        appointmentsRequestController.abort();
    }

    appointmentsRequestController = new AbortController();
    setDoctorAppointmentsLoading(true);
    showAppointmentsError(null);

    try {
        const query = buildDateQueryString();
        const endpoints = resolveAppointmentEndpoints();
        const endpoint = query ? `${endpoints.root}?${query}` : endpoints.root;
        const response = await fetch(endpoint, {
            credentials: 'include',
            signal: appointmentsRequestController.signal
        });

        if (!response.ok) {
            throw await buildApiError(response);
        }

        const data = await response.json();
        appointmentState.appointments = Array.isArray(data) ? data.map(mapAppointmentDto) : [];
        populateFilterOptionsFromAppointments();
        setDoctorAppointmentsLoading(false);
        renderAppointmentsTable();
    } catch (error) {
        if (error.name === 'AbortError') {
            return;
        }
        appointmentState.appointments = [];
        setDoctorAppointmentsLoading(false);
        clearAppointmentTable();
        showAppointmentsError(error?.message || 'Không thể tải dữ liệu cuộc hẹn.');
    } finally {
        appointmentsRequestController = null;
        appointmentState.isInitialized = true;
    }
}

function buildDateQueryString() {
    const params = new URLSearchParams();
    const fromDate = getSelectedDateString(fpStart);
    const toDate = getSelectedDateString(fpEnd);

    if (fromDate) {
        params.append('from', fromDate);
    }
    if (toDate) {
        params.append('to', toDate);
    }

    return params.toString();
}

function getSelectedDateString(pickerInstance) {
    if (!pickerInstance || !pickerInstance.selectedDates || pickerInstance.selectedDates.length === 0) {
        return null;
    }

    return formatDateOnly(pickerInstance.selectedDates[0]);
}

function formatDateOnly(date) {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
}

function clearAppointmentTable() {
    const tbody = document.getElementById('appointmentsTableBody');
    if (tbody) {
        tbody.innerHTML = '';
    }
    updateAllCounts();
}

function renderAppointmentsTable() {
    const tbody = document.getElementById('appointmentsTableBody');
    if (!tbody) {
        return;
    }

    tbody.innerHTML = '';

    if (!appointmentState.appointments.length) {
        updateAllCounts();
        return;
    }

    const fragment = document.createDocumentFragment();
    let currentDateKey = null;
    const sortedAppointments = [...appointmentState.appointments]
        .sort((a, b) => new Date(a.startUtc) - new Date(b.startUtc));

    sortedAppointments.forEach(appointment => {
        if (appointment.dateKey !== currentDateKey) {
            currentDateKey = appointment.dateKey;
            fragment.appendChild(createDateHeaderRow(appointment));
        }

        fragment.appendChild(createAppointmentRow(appointment));
    });

    tbody.appendChild(fragment);
    bindStatusDropdowns(tbody);
    bindAppointmentActionMenus(tbody);
    attachFilterCheckboxHandlers(tbody);
    applyFilters();
}

function createDateHeaderRow(appointment) {
    const header = document.createElement('tr');
    header.className = 'table-group-header';
    header.dataset.dateGroup = appointment.dateKey;
    const label = appointment.dateLabel?.toUpperCase?.() || appointment.dateLabel || appointment.dateKey;

    const cell = document.createElement('td');
    cell.colSpan = 9;
    cell.className = 'pt-4 pb-1 small text-muted fw-bold';
    cell.style.textTransform = 'uppercase';
    cell.innerHTML = `${label} (<span class="day-count">0</span>)`;
    header.appendChild(cell);
    return header;
}

function createAppointmentRow(appointment) {
    const row = document.createElement('tr');
    row.className = 'appointment-row';
    row.dataset.dateGroup = appointment.dateKey;
    row.dataset.dateIso = appointment.dateKey;
    row.dataset.service = appointment.specialtyName;
    row.dataset.customer = appointment.patientName;
    row.dataset.employee = appointment.doctorName;
    row.dataset.status = appointment.status;
    row.dataset.appointmentId = appointment.id;

    row.appendChild(createCheckboxCell());
    row.appendChild(createTimeCell(appointment));
    row.appendChild(createServiceCell(appointment));
    row.appendChild(createCustomerCell(appointment));
    row.appendChild(createDurationCell(appointment.durationMinutes));
    row.appendChild(createStatusCell(appointment));
    row.appendChild(createDoctorCell(appointment));
    row.appendChild(createNoteCell());
    row.appendChild(createActionsCell(appointment));

    return row;
}

function createCheckboxCell() {
    const td = document.createElement('td');
    const checkbox = document.createElement('input');
    checkbox.type = 'checkbox';
    checkbox.className = 'form-check-input';
    td.appendChild(checkbox);
    return td;
}

function createTimeCell(appointment) {
    const td = document.createElement('td');
    const timeLabel = document.createElement('div');
    timeLabel.className = 'fw-semibold';
    timeLabel.textContent = appointment.timeLabel;

    const clinicRoom = document.createElement('div');
    clinicRoom.className = 'text-muted small';
    clinicRoom.textContent = appointment.clinicRoom || '';

    td.appendChild(timeLabel);
    td.appendChild(clinicRoom);
    return td;
}

function createServiceCell(appointment) {
    const td = document.createElement('td');
    const badge = document.createElement('span');
    badge.className = 'badge rounded-pill text-white fw-semibold';
    badge.style.backgroundColor = appointment.specialtyColor || '#0ea5e9';
    badge.textContent = appointment.specialtyName;
    td.appendChild(badge);
    return td;
}

function createCustomerCell(appointment) {
    const td = document.createElement('td');
    const name = document.createElement('div');
    name.className = 'fw-semibold';
    name.textContent = appointment.patientName;

    const phone = document.createElement('div');
    phone.className = 'text-muted small';
    phone.textContent = appointment.customerPhone || '—';

    td.appendChild(name);
    td.appendChild(phone);
    return td;
}

function createDurationCell(durationMinutes) {
    const td = document.createElement('td');
    td.textContent = formatDurationLabel(durationMinutes);
    return td;
}

function createStatusCell(appointment) {
    const td = document.createElement('td');
    const dropdown = document.createElement('div');
    dropdown.className = 'dropdown status-dropdown';

    const button = document.createElement('button');
    button.type = 'button';
    button.className = `btn btn-sm btn-outline-light dropdown-toggle d-flex align-items-center gap-2 rounded-pill status-badge ${resolveToneStyles(appointment.statusTone).badge}`;
    button.dataset.bsToggle = 'dropdown';
    button.dataset.bsContainer = 'body';
    button.dataset.bsStrategy = 'fixed';
    button.innerHTML = `<i class="fa-solid ${appointment.statusIcon}"></i> ${appointment.statusLabel}`;
    button.dataset.statusCode = appointment.status;
    dropdown.appendChild(button);

    const menu = document.createElement('ul');
    menu.className = 'dropdown-menu';

    (appointmentState.metadata?.statuses ?? []).forEach(status => {
        const toneStyles = resolveToneStyles(status.tone);
        const li = document.createElement('li');
        const anchor = document.createElement('a');
        anchor.href = '#';
        anchor.className = `dropdown-item status-option ${toneStyles.option}`;
        anchor.dataset.value = status.code;
        anchor.dataset.label = status.label;
        anchor.dataset.icon = status.icon;
        anchor.dataset.tone = status.tone;
        anchor.innerHTML = `<i class="fa-solid ${status.icon} ${toneStyles.option} me-2"></i> ${status.label}`;
        li.appendChild(anchor);
        menu.appendChild(li);
    });

    dropdown.appendChild(menu);
    td.appendChild(dropdown);
    return td;
}

function createDoctorCell(appointment) {
    const td = document.createElement('td');
    const wrapper = document.createElement('div');
    wrapper.className = 'd-flex align-items-center gap-2';

    const avatarWrapper = document.createElement('div');
    avatarWrapper.className = 'flex-shrink-0';
    avatarWrapper.style.width = '32px';
    avatarWrapper.style.height = '32px';

    if (appointment.doctorAvatarUrl) {
        const img = document.createElement('img');
        img.src = appointment.doctorAvatarUrl;
        img.alt = appointment.doctorName || 'Bác sĩ';
        img.width = 32;
        img.height = 32;
        img.className = 'rounded-circle object-fit-cover';
        avatarWrapper.appendChild(img);
    } else {
        const fallback = document.createElement('div');
        fallback.className = 'rounded-circle bg-primary text-white fw-semibold d-flex align-items-center justify-content-center';
        fallback.style.width = '32px';
        fallback.style.height = '32px';
        fallback.textContent = appointment.doctorInitials || 'BS';
        avatarWrapper.appendChild(fallback);
    }

    const info = document.createElement('div');
    const name = document.createElement('div');
    name.className = 'fw-semibold';
    name.textContent = appointment.doctorName;
    const room = document.createElement('div');
    room.className = 'text-muted small';
    room.textContent = appointment.clinicRoom || '';
    info.appendChild(name);
    info.appendChild(room);

    wrapper.appendChild(avatarWrapper);
    wrapper.appendChild(info);
    td.appendChild(wrapper);
    return td;
}

function createNoteCell() {
    const td = document.createElement('td');
    const placeholder = document.createElement('span');
    placeholder.className = 'text-muted';
    placeholder.textContent = '—';
    td.appendChild(placeholder);
    return td;
}

function createActionsCell(appointment) {
    const td = document.createElement('td');
    td.className = 'text-end';

    const dropdown = document.createElement('div');
    dropdown.className = 'dropdown';

    const toggle = document.createElement('button');
    toggle.className = 'btn btn-link text-muted p-0';
    toggle.type = 'button';
    toggle.dataset.bsToggle = 'dropdown';
    toggle.dataset.bsContainer = 'body';
    toggle.dataset.bsStrategy = 'fixed';
    toggle.innerHTML = '<i class="fa-solid fa-ellipsis-vertical fs-5"></i>';
    dropdown.appendChild(toggle);

    const menu = document.createElement('ul');
    menu.className = 'dropdown-menu dropdown-menu-end shadow-sm border-0';
    menu.style.borderRadius = '10px';

    if (appointmentState.mode === 'admin') {
        const editItem = document.createElement('li');
        const editButton = document.createElement('button');
        editButton.type = 'button';
        editButton.className = 'dropdown-item edit-appointment-action';
        editButton.dataset.appointmentId = appointment.id;
        editButton.textContent = 'Chỉnh sửa';
        editItem.appendChild(editButton);
        menu.appendChild(editItem);
    } else {
        const placeholder = document.createElement('li');
        const label = document.createElement('span');
        label.className = 'dropdown-item text-muted';
        label.textContent = 'Tính năng đang phát triển';
        placeholder.appendChild(label);
        menu.appendChild(placeholder);
    }

    dropdown.appendChild(menu);
    td.appendChild(dropdown);
    return td;
}

function formatDurationLabel(minutes) {
    if (!minutes || minutes <= 0) {
        return '—';
    }

    const hours = Math.floor(minutes / 60);
    const remaining = minutes % 60;

    if (hours && remaining) {
        return `${hours}h ${remaining}p`;
    }
    if (hours) {
        return `${hours}h`;
    }
    return `${remaining}p`;
}

function populateFilterOptionsFromAppointments() {
    const appointments = appointmentState.appointments;
    const services = uniqueValues(appointments.map(x => x.specialtyName));
    const customers = uniqueValues(appointments.map(x => x.patientName));
    const employees = uniqueValues(appointments.map(x => x.doctorName));

    populateFilterList('#filterService', services, 'service', getSelectedFilterValues('#filterService'));
    populateFilterList('#filterCustomer', customers, 'customer', getSelectedFilterValues('#filterCustomer'));
    populateFilterList('#filterEmployee', employees, 'employee', getSelectedFilterValues('#filterEmployee'));
}

function populateStatusFilterOptions(statuses) {
    const options = statuses.map(status => ({ value: status.code, label: status.label }));
    populateFilterList('#filterStatus', options, 'status', getSelectedFilterValues('#filterStatus'));
}

function populateModalStatusOptions(statuses) {
    if (!modalStatusSelect) {
        modalStatusSelect = document.getElementById('modalStatusSelect');
    }

    if (!modalStatusSelect) {
        return;
    }

    modalStatusSelect.innerHTML = '';

    if (!Array.isArray(statuses) || !statuses.length) {
        const option = document.createElement('option');
        option.value = 'pending';
        option.textContent = 'Chờ xác nhận';
        modalStatusSelect.appendChild(option);
        modalStatusSelect.value = 'pending';
        return;
    }

    statuses.forEach(status => {
        const option = document.createElement('option');
        option.value = status.code;
        option.textContent = status.label;
        modalStatusSelect.appendChild(option);
    });

    modalStatusSelect.value = statuses[0].code;
}

function populateFilterList(containerSelector, values, prefix, selectedValues = []) {
    const list = document.querySelector(`${containerSelector} .filter-options-list`);
    if (!list) {
        return;
    }

    list.innerHTML = '';

    if (!values.length) {
        const emptyItem = document.createElement('li');
        emptyItem.className = 'text-muted small px-1';
        emptyItem.textContent = 'Chưa có dữ liệu';
        list.appendChild(emptyItem);
        return;
    }

    const selectedSet = new Set(selectedValues);
    const fragment = document.createDocumentFragment();

    values.forEach((value, index) => {
        const actualValue = typeof value === 'string' ? value : value.value;
        const labelText = typeof value === 'string' ? value : value.label;
        if (!actualValue) {
            return;
        }

        const li = document.createElement('li');
        const wrapper = document.createElement('div');
        wrapper.className = 'form-check';

        const input = document.createElement('input');
        input.type = 'checkbox';
        input.className = 'form-check-input filter-checkbox';
        input.value = actualValue;
        input.id = `${prefix}-option-${index}`;
        input.checked = selectedSet.has(actualValue);

        const label = document.createElement('label');
        label.className = 'form-check-label';
        label.htmlFor = input.id;
        label.textContent = labelText;

        wrapper.appendChild(input);
        wrapper.appendChild(label);
        li.appendChild(wrapper);
        fragment.appendChild(li);
    });

    list.appendChild(fragment);
    attachFilterCheckboxHandlers(list);
}

function uniqueValues(values) {
    return [...new Set(values.filter(value => !!value))].sort((a, b) => a.localeCompare(b, 'vi'));
}

function mapAppointmentDto(source) {
    return {
        id: source.id ?? source.Id,
        doctorId: source.doctorId ?? source.DoctorId ?? null,
        specialtyId: source.specialtyId ?? source.SpecialtyId,
        specialtyName: source.specialtyName ?? source.SpecialtyName ?? '',
        specialtyColor: source.specialtyColor ?? source.SpecialtyColor ?? '#0ea5e9',
        patientName: source.patientName ?? source.PatientName ?? '',
        customerPhone: source.customerPhone ?? source.CustomerPhone ?? '',
        patientId: source.patientId ?? source.PatientId ?? null,
        status: source.status ?? source.Status ?? '',
        statusLabel: source.statusLabel ?? source.StatusLabel ?? '',
        statusTone: source.statusTone ?? source.StatusTone ?? 'pending',
        statusIcon: source.statusIcon ?? source.StatusIcon ?? 'fa-circle',
        doctorName: source.doctorName ?? source.DoctorName ?? '',
        doctorInitials: source.doctorInitials ?? source.DoctorInitials ?? 'BS',
        doctorAvatarUrl: source.doctorAvatarUrl ?? source.DoctorAvatarUrl ?? '',
        startUtc: source.startUtc ?? source.StartUtc,
        endUtc: source.endUtc ?? source.EndUtc,
        dateLabel: source.dateLabel ?? source.DateLabel ?? '',
        dateKey: source.dateKey ?? source.DateKey ?? '',
        timeLabel: source.timeLabel ?? source.TimeLabel ?? '',
        durationMinutes: source.durationMinutes ?? source.DurationMinutes ?? 0,
        clinicRoom: source.clinicRoom ?? source.ClinicRoom ?? '',
        clinicRoomId: source.clinicRoomId ?? source.ClinicRoomId ?? null
    };
}

function mapSpecialtyDto(source) {
    return {
        id: source.id ?? source.Id ?? '',
        name: source.name ?? source.Name ?? '',
        color: source.color ?? source.Color ?? '#0ea5e9'
    };
}

function mapStatusDto(source) {
    return {
        code: source.code ?? source.Code ?? '',
        label: source.label ?? source.Label ?? '',
        tone: source.tone ?? source.Tone ?? 'pending',
        icon: source.icon ?? source.Icon ?? 'fa-circle'
    };
}

function mapDoctorOptionDto(source) {
    const rawId = source?.id ?? source?.Id ?? '';
    const rawSpecialtyIds = source?.specialtyIds ?? source?.SpecialtyIds ?? [];
    const normalizedSpecialtyIds = Array.isArray(rawSpecialtyIds)
        ? rawSpecialtyIds
            .filter(id => id !== null && id !== undefined && id !== '')
            .map(id => String(id))
        : [];

    return {
        id: rawId !== null && rawId !== undefined ? String(rawId) : '',
        name: source.name ?? source.Name ?? '',
        avatarUrl: source.avatarUrl ?? source.AvatarUrl ?? '',
        specialtyIds: normalizedSpecialtyIds
    };
}

function mapPatientOptionDto(source) {
    return {
        id: source.id ?? source.Id ?? '',
        name: source.name ?? source.Name ?? '',
        phoneNumber: source.phoneNumber ?? source.PhoneNumber ?? ''
    };
}

function populateAdminNewAppointmentOptions(metadata) {
    if (appointmentState.mode !== 'admin') {
        return;
    }

    const specialtyOptions = (metadata?.specialties ?? []).map(item => ({
        value: item.id != null ? String(item.id) : '',
        text: item.name
    }));
    const patientOptions = (metadata?.patients ?? []).map(item => ({
        value: item.id != null ? String(item.id) : '',
        text: buildPatientOptionLabel(item)
    }));

    refreshTomSelectOptions(tomService, specialtyOptions, 'Chọn chuyên khoa khám');
    refreshTomSelectOptions(tomCustomer, patientOptions, 'Chọn bệnh nhân');
    updateAdminDoctorSelect(tomService?.getValue() || '');
    handleAdminPatientSelectionChange(tomCustomer?.getValue() || '');
    bindAdminModalEvents();
}

function populateDoctorNewAppointmentOptions(metadata) {
    if (appointmentState.mode !== 'doctor' || !metadata) {
        return;
    }

    const specialtyOptions = (metadata.specialties ?? []).map(item => ({
        value: item.id != null ? String(item.id) : '',
        text: item.name
    }));

    if (tomService) {
        const placeholder = specialtyOptions.length ? 'Chuyên khoa của bạn' : 'Chưa có chuyên khoa khả dụng';
        refreshTomSelectOptions(tomService, specialtyOptions, placeholder);
        if (specialtyOptions.length) {
            tomService.setValue(specialtyOptions[0].value, true);
        }
        tomService.disable();
    }

    if (tomEmployee) {
        const doctorOption = metadata.doctorId
            ? [{ value: String(metadata.doctorId), text: metadata.doctorName || 'Bác sĩ của bạn' }]
            : [];
        const placeholder = doctorOption.length ? metadata.doctorName || 'Bác sĩ phụ trách' : 'Không tìm thấy bác sĩ';
        refreshTomSelectOptions(tomEmployee, doctorOption, placeholder);
        if (doctorOption.length) {
            tomEmployee.setValue(doctorOption[0].value, true);
        }
        tomEmployee.disable();
    }

    if (tomCustomer) {
        const patientOptions = (metadata.patients ?? []).map(item => ({
            value: item.id != null ? String(item.id) : '',
            text: buildPatientOptionLabel(item)
        }));
        const placeholder = patientOptions.length ? 'Chọn bệnh nhân' : 'Chưa có bệnh nhân khả dụng';
        refreshTomSelectOptions(tomCustomer, patientOptions, placeholder);
    }
}

function refreshTomSelectOptions(instance, options, placeholder) {
    if (!instance) {
        return;
    }

    instance.clear(true);
    instance.clearOptions();

    const hasOptions = Array.isArray(options) && options.length > 0;
    if (hasOptions) {
        instance.addOptions(options);
        instance.enable();
    } else {
        instance.disable();
    }

    if (placeholder && instance.input) {
        instance.input.setAttribute('placeholder', placeholder);
    }
}

function buildPatientOptionLabel(patient) {
    if (!patient) {
        return '';
    }

    const displayName = (patient.name || '').trim();
    const phone = patient.phoneNumber ? ` (${patient.phoneNumber})` : '';
    const identifier = (patient.id || '').slice(0, 5) || 'mới';
    const base = displayName || `Khách hàng ${identifier}`;
    return `${base}${phone}`.trim();
}

async function buildApiError(response) {
    try {
        const payload = await response.json();
        const message = payload?.title || payload?.detail;
        if (message) {
            return new Error(message);
        }
    } catch (error) {
        // ignore
    }

    return new Error('Máy chủ không phản hồi. Vui lòng thử lại.');
}

function handleDateRangeChanged() {
    if (appointmentState.metadata) {
        reloadDoctorAppointments();
    }
}

function formatSlotLabel(startValue, endValue) {
    const start = startValue ? new Date(startValue) : null;
    const end = endValue ? new Date(endValue) : null;

    if (!start || Number.isNaN(start.valueOf()) || !end || Number.isNaN(end.valueOf())) {
        return '';
    }

    const options = { hour: '2-digit', minute: '2-digit', hour12: false };
    return `${start.toLocaleTimeString([], options)} - ${end.toLocaleTimeString([], options)}`;
}

function normalizeSlotStartIso(selectedValue) {
    const normalizedValue = normalizeTomSelectValue(selectedValue);
    if (!normalizedValue) {
        return null;
    }

    const parsed = new Date(normalizedValue);
    if (Number.isNaN(parsed.valueOf())) {
        return null;
    }

    return parsed.toISOString();
}

function renderModalTimeSlots(slots, preselectStartUtc = null) {
    if (!tomTime) {
        return;
    }

    tomTime.clear(true);
    tomTime.clearOptions();

    const options = (slots ?? [])
        .filter(slot => slot?.startUtc)
        .map(slot => ({
            value: slot.startUtc,
            text: formatSlotLabel(slot.startLocal || slot.startUtc, slot.endLocal || slot.endUtc) || 'Khung giờ'
        }));

    if (!options.length) {
        tomTime.disable();
        tomTime.input?.setAttribute('placeholder', 'Không có khung giờ trống');
        return;
    }

    tomTime.addOptions(options);
    tomTime.enable();
    tomTime.input?.setAttribute('placeholder', 'Chọn khung giờ khám');

    if (preselectStartUtc) {
        tomTime.setValue(String(preselectStartUtc), true);
    }
}

async function loadAvailableTimeSlots(preselectStartUtc = null) {
    if (!tomTime) {
        return;
    }

    const doctorId = normalizeGuidValue(tomEmployee?.getValue());
    const selectedDate = fpModalDate?.selectedDates?.[0];

    if (!doctorId) {
        resetModalTimeSelect(tomTime);
        tomTime.input?.setAttribute('placeholder', 'Vui lòng chọn bác sĩ trước');
        return;
    }

    if (!(selectedDate instanceof Date)) {
        resetModalTimeSelect(tomTime);
        return;
    }

    const excludeParam = appointmentModalMode === 'edit' && editingAppointmentId
        ? `&excludeAppointmentId=${editingAppointmentId}`
        : '';

    try {
        tomTime.disable();
        tomTime.clear(true);
        tomTime.clearOptions();
        tomTime.addOption({ value: '', text: 'Đang tải...' });
        tomTime.refreshOptions(false);

        const response = await fetch(`/api/customer-booking/doctors/${doctorId}/time-slots?date=${formatDateOnly(selectedDate)}${excludeParam}`);
        if (!response.ok) {
            throw await buildApiError(response);
        }

        const slots = await response.json();
        renderModalTimeSlots(slots, preselectStartUtc);
    } catch (error) {
        resetModalTimeSelect(tomTime);
        tomTime.input?.setAttribute('placeholder', 'Không tải được khung giờ');
        if (error?.message) {
            showNewAppointmentAlert(error.message, 'danger');
        }
    }
}

function buildAdminAppointmentRequest({
    specialtyId,
    doctorId,
    patientId,
    slotStartIso,
    manualPhone,
    statusCode,
    durationMinutes = DEFAULT_APPOINTMENT_DURATION,
    clinicRoomId = null,
    appointmentId = null
}) {
    if (!specialtyId) {
        throw new Error('Vui lòng chọn chuyên khoa.');
    }

    if (appointmentState.mode === 'admin' && (!doctorId || !patientId)) {
        throw new Error('Vui lòng chọn đầy đủ bác sĩ và bệnh nhân.');
    }

    const endpoints = resolveAppointmentEndpoints();
    const normalizedDuration = durationMinutes > 0 ? durationMinutes : DEFAULT_APPOINTMENT_DURATION;
    const normalizedStatus = statusCode || 'pending';
    const isEdit = !!appointmentId;

    if (appointmentState.mode === 'admin') {
        const patient = findPatientOptionById(patientId);
        if (!patient) {
            throw new Error('Không tìm thấy thông tin bệnh nhân.');
        }

        const storedPhone = (patient.phoneNumber || '').trim();
        const fallbackPhone = (manualPhone || '').trim();
        const resolvedPhone = storedPhone || fallbackPhone;

        if (!resolvedPhone) {
            throw new Error('Bệnh nhân chưa có số điện thoại, vui lòng nhập để tiếp tục.');
        }

        return {
            endpoint: isEdit ? `${endpoints.root}/${appointmentId}` : endpoints.root,
            method: isEdit ? 'PUT' : 'POST',
            body: {
                doctorId,
                specialtyId,
                patientId: patient.id || patientId,
                patientName: patient.name || '',
                customerPhone: resolvedPhone,
                slotStartUtc: slotStartIso,
                durationMinutes: normalizedDuration,
                status: normalizedStatus,
                clinicRoomId
            }
        };
    }

    const patient = findPatientOptionById(patientId);
    if (!patient?.name || !patient?.phoneNumber) {
        throw new Error('Vui lòng đảm bảo hồ sơ bệnh nhân có đầy đủ tên và số điện thoại.');
    }

    return {
        endpoint: isEdit ? `${endpoints.root}/${appointmentId}` : endpoints.root,
        method: isEdit ? 'PUT' : 'POST',
        body: {
            specialtyId,
            patientName: patient.name,
            customerPhone: patient.phoneNumber,
            slotStartUtc: slotStartIso,
            durationMinutes: normalizedDuration,
            status: normalizedStatus,
            clinicRoomId
        }
    };
}

function findPatientOptionById(id) {
    if (!id || !appointmentState.metadata?.patients) {
        return null;
    }

    return appointmentState.metadata.patients.find(option => String(option.id ?? option.Id ?? '') === String(id)) || null;
}

async function submitAppointmentMutation(payload) {
    const response = await fetch(payload.endpoint, {
        method: payload.method || 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        credentials: 'include',
        body: JSON.stringify(payload.body)
    });

    if (!response.ok) {
        throw await buildApiError(response);
    }

    return await response.json();
}

function bindAdminModalEvents() {
    if (adminModalEventsBound || appointmentState.mode !== 'admin') {
        return;
    }

    if (tomService) {
        tomService.on('change', (value) => {
            const normalizedValue = normalizeTomSelectValue(value);
            const currentDoctorId = tomEmployee?.getValue();
            updateAdminDoctorSelect(normalizedValue, { preserveDoctorId: currentDoctorId });
        });
    }

    if (tomEmployee) {
        tomEmployee.on('change', (value) => {
            const normalizedDoctorId = normalizeTomSelectValue(value);
            const currentSpecialtyId = normalizeTomSelectValue(tomService?.getValue());

            if (!currentSpecialtyId && normalizedDoctorId) {
                const doctorOption = findDoctorOptionById(normalizedDoctorId);
                const doctorSpecialtyId = doctorOption?.specialtyIds?.[0];
                if (doctorSpecialtyId) {
                    const specialty = findSpecialtyById(doctorSpecialtyId);
                    ensureTomSelectOption(tomService, doctorSpecialtyId, specialty?.name || 'Chuyên khoa');
                    tomService?.setValue(String(doctorSpecialtyId), true);
                    updateAdminDoctorSelect(doctorSpecialtyId, { preserveDoctorId: normalizedDoctorId });
                }
            } else if (currentSpecialtyId) {
                updateAdminDoctorSelect(currentSpecialtyId, { preserveDoctorId: normalizedDoctorId });
            }

            if (fpModalDate?.selectedDates?.length) {
                loadAvailableTimeSlots();
            } else {
                resetModalTimeSelect(tomTime);
            }
        });
    }

    if (tomCustomer) {
        tomCustomer.on('change', (value) => {
            const normalizedValue = normalizeTomSelectValue(value);
            handleAdminPatientSelectionChange(normalizedValue);
        });
    }

    if (manualPhoneInput) {
        manualPhoneInput.addEventListener('input', () => {
            manualPhoneInput.classList.remove('is-invalid');
        });
    }

    adminModalEventsBound = true;
}

function updateAdminDoctorSelect(selectedSpecialtyId, { preserveDoctorId } = {}) {
    if (appointmentState.mode !== 'admin') {
        return;
    }

    const normalizedSpecialtyId = normalizeTomSelectValue(selectedSpecialtyId);
    const doctors = getDoctorsForSpecialtyId(normalizedSpecialtyId);
    const hasSpecialty = !!normalizeGuidValue(normalizedSpecialtyId);
    const placeholder = doctors.length
        ? 'Chọn bác sĩ phụ trách'
        : hasSpecialty
            ? 'Chưa có bác sĩ thuộc chuyên khoa này'
            : 'Chưa có bác sĩ khả dụng';

    refreshTomSelectOptions(tomEmployee, buildDoctorSelectOptions(doctors), placeholder);

    const preferredDoctorId = normalizeTomSelectValue(preserveDoctorId ?? tomEmployee?.getValue());
    const doctorStillValid = !!preferredDoctorId
        && doctors.some(doc => String(doc.id) === String(preferredDoctorId));

    if (doctorStillValid) {
        const doctorOption = findDoctorOptionById(preferredDoctorId);
        ensureTomSelectOption(tomEmployee, preferredDoctorId, doctorOption?.name || 'Bác sĩ');
        tomEmployee.setValue(preferredDoctorId, true);
        if (fpModalDate?.selectedDates?.length) {
            loadAvailableTimeSlots();
        }
        return;
    }

    if (tomEmployee) {
        tomEmployee.clear(true);
    }
    resetModalTimeSelect(tomTime);
}

function getDoctorsForSpecialtyId(specialtyId) {
    const doctors = appointmentState.metadata?.doctors ?? [];
    const normalizedSpecialtyId = normalizeGuidValue(specialtyId);
    if (!normalizedSpecialtyId) {
        return doctors;
    }

    return doctors.filter(doc =>
        (doc?.specialtyIds ?? []).some(id => normalizeGuidValue(id) === normalizedSpecialtyId));
}

function buildDoctorSelectOptions(doctors) {
    if (!Array.isArray(doctors)) {
        return [];
    }

    return doctors
        .filter(doc => doc && doc.id)
        .map(doc => ({
            value: String(doc.id),
            text: doc.name || 'Bác sĩ'
        }));
}

function findDoctorOptionById(doctorId) {
    const normalizedId = normalizeTomSelectValue(doctorId);
    if (!normalizedId || !appointmentState.metadata?.doctors) {
        return null;
    }

    return appointmentState.metadata.doctors
        .find(doc => String(doc.id) === String(normalizedId))
        || null;
}

function findSpecialtyById(specialtyId) {
    const normalizedId = normalizeTomSelectValue(specialtyId);
    if (!normalizedId || !appointmentState.metadata?.specialties) {
        return null;
    }

    return appointmentState.metadata.specialties
        .find(specialty => String(specialty.id) === String(normalizedId))
        || null;
}

function normalizeTomSelectValue(value) {
    if (Array.isArray(value)) {
        return value[0] ?? '';
    }
    return value ?? '';
}

function normalizeGuidValue(value) {
    if (value === null || value === undefined) {
        return '';
    }
    return String(value).trim().toLowerCase();
}

function patientRequiresManualPhone(patientId) {
    if (appointmentState.mode !== 'admin') {
        return false;
    }

    const normalizedId = normalizeTomSelectValue(patientId);
    if (!normalizedId) {
        return false;
    }

    const patient = findPatientOptionById(normalizedId);
    if (!patient) {
        return false;
    }

    return !(patient.phoneNumber && patient.phoneNumber.trim().length > 0);
}

function handleAdminPatientSelectionChange(patientId) {
    if (appointmentState.mode !== 'admin') {
        return;
    }

    const normalizedId = normalizeTomSelectValue(patientId);
    if (!normalizedId) {
        resetManualPhoneCapture();
        return;
    }

    if (patientRequiresManualPhone(normalizedId)) {
        showManualPhoneCapture();
        return;
    }

    hideManualPhoneCapture();
}

function showManualPhoneCapture() {
    if (!manualPhoneWrapper) {
        return;
    }

    const wasHidden = manualPhoneWrapper.classList.contains('d-none');
    manualPhoneWrapper.classList.remove('d-none');
    manualPhoneWrapper.dataset.requiresPhone = '1';

    if (manualPhoneInput) {
        if (wasHidden) {
            manualPhoneInput.value = '';
        }
        manualPhoneInput.classList.remove('is-invalid');
        manualPhoneInput.focus();
    }
}

function hideManualPhoneCapture() {
    if (!manualPhoneWrapper) {
        return;
    }

    manualPhoneWrapper.classList.add('d-none');
    manualPhoneWrapper.dataset.requiresPhone = '0';

    if (manualPhoneInput) {
        manualPhoneInput.value = '';
        manualPhoneInput.classList.remove('is-invalid');
    }
}

function resetManualPhoneCapture() {
    hideManualPhoneCapture();
}

function toggleStatusButtonLoading(button, isLoading) {
    if (!button) {
        return;
    }

    if (isLoading) {
        if (!button.dataset.originalContent) {
            button.dataset.originalContent = button.innerHTML;
        }
        button.disabled = true;
        button.innerHTML = '<span class="spinner-border spinner-border-sm" role="status"></span>';
        return;
    }

    button.disabled = false;
    if (button.dataset.originalContent) {
        button.innerHTML = button.dataset.originalContent;
        delete button.dataset.originalContent;
    }
}

async function submitStatusUpdate(appointmentId, statusCode) {
    const endpoints = resolveAppointmentEndpoints();
    const response = await fetch(`${endpoints.root}/${appointmentId}/status`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        credentials: 'include',
        body: JSON.stringify({ status: statusCode })
    });

    if (!response.ok) {
        throw await buildApiError(response);
    }

    return await response.json();
}

function upsertAppointmentState(serverDto) {
    const normalized = mapAppointmentDto(serverDto);
    const index = appointmentState.appointments.findIndex(item => item.id === normalized.id);
    if (index >= 0) {
        appointmentState.appointments[index] = normalized;
    } else {
        appointmentState.appointments.push(normalized);
    }
    return normalized;
}

function ensureTomSelectOption(instance, value, label) {
    if (!instance || !value) {
        return;
    }

    const normalizedValue = String(value);
    if (!instance.options?.[normalizedValue]) {
        instance.addOption({ value: normalizedValue, text: label || normalizedValue });
    }
}

function openEditAppointmentModal(appointmentId) {
    if (appointmentState.mode !== 'admin') {
        return;
    }

    if (!appointmentId) {
        showAppointmentsError('Không xác định được cuộc hẹn cần chỉnh sửa.');
        return;
    }

    const appointment = appointmentState.appointments.find(item => String(item.id) === String(appointmentId));
    if (!appointment) {
        showAppointmentsError('Không tìm thấy dữ liệu cuộc hẹn.');
        return;
    }

    appointmentModalMode = 'edit';
    editingAppointmentId = appointment.id;
    editingAppointmentMeta = {
        durationMinutes: DEFAULT_APPOINTMENT_DURATION,
        clinicRoomId: appointment.clinicRoomId || null
    };

    showNewAppointmentAlert(null);
    manualPhoneInput?.classList.remove('is-invalid');

    if (modalTitleEl) {
        modalTitleEl.textContent = 'Chỉnh sửa cuộc hẹn';
    }
    if (saveAppointmentButtonEl) {
        saveAppointmentButtonEl.textContent = 'Cập nhật';
    }
    if (modalStatusFieldEl) {
        modalStatusFieldEl.classList.remove('d-none');
    }
    if (modalStatusSelect) {
        modalStatusSelect.value = appointment.status || modalStatusSelect.value;
    }

    if (tomService) {
        const specialtyValue = appointment.specialtyId ? String(appointment.specialtyId) : '';
        if (specialtyValue) {
            ensureTomSelectOption(tomService, specialtyValue, appointment.specialtyName);
            tomService.setValue(specialtyValue, true);
            updateAdminDoctorSelect(specialtyValue);
        } else {
            tomService.clear(true);
        }
    }

    if (tomEmployee) {
        const doctorValue = appointment.doctorId ? String(appointment.doctorId) : '';
        if (doctorValue) {
            ensureTomSelectOption(tomEmployee, doctorValue, appointment.doctorName);
            tomEmployee.setValue(doctorValue, true);
        } else {
            tomEmployee.clear(true);
        }
    }

    if (tomCustomer) {
        const patientValue = appointment.patientId ? String(appointment.patientId) : '';
        if (patientValue) {
            ensureTomSelectOption(tomCustomer, patientValue, appointment.patientName);
            tomCustomer.setValue(patientValue, true);
        } else {
            tomCustomer.clear(true);
        }
    }

    if (patientRequiresManualPhone(appointment.patientId)) {
        showManualPhoneCapture();
        if (manualPhoneInput) {
            manualPhoneInput.value = appointment.customerPhone || '';
        }
    } else {
        hideManualPhoneCapture();
        if (manualPhoneInput) {
            manualPhoneInput.value = '';
        }
    }

    if (fpModalDate) {
        const slotStart = new Date(appointment.startUtc);
        if (!Number.isNaN(slotStart.valueOf())) {
            const defaultMin = getMinimumAppointmentDate();
            const effectiveMin = slotStart < defaultMin ? slotStart : defaultMin;
            fpModalDate.set('minDate', effectiveMin);
            fpModalDate.setDate(slotStart, true);
            loadAvailableTimeSlots(appointment.startUtc);
        }
    }

    const modalElement = newAppointmentModalEl || document.getElementById('newAppointmentModal');
    if (modalElement) {
        const modalInstance = bootstrap.Modal.getOrCreateInstance(modalElement);
        modalInstance.show();
    }
}
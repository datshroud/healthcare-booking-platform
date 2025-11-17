const DEFAULT_SPECIALTY_COLOR = '#0ea5e9';

const calendarEndpoints = {
    doctor: {
        metadata: '/api/doctor/appointments/metadata',
        events: '/api/doctor/appointments/calendar'
    },
    admin: {
        metadata: '/api/admin/appointments/metadata',
        events: '/api/admin/appointments/calendar'
    }
};

const calendarState = {
    mode: document.body?.dataset?.calendarMode || 'doctor',
    metadata: null,
    employees: [],
    specialtyColorMap: new Map(),
    events: [],
    selectedEmployeeIds: new Set(['all']),
    viewBy: 'employee',
    currentRange: null
};

const calendarTitleEl = document.getElementById('calendarTitle');
const viewByToggleEl = document.getElementById('viewByDropdownToggle');
const employeeFilterBar = document.querySelector('.employee-filter-bar');
const viewSwitchers = document.getElementById('viewSwitchers');
const calendarErrorEl = document.getElementById('calendarError');

let calendar = null;
let eventsRequestController = null;

document.addEventListener('DOMContentLoaded', function () {
    initializeCalendarScreen();
});

async function initializeCalendarScreen() {
    try {
        showCalendarError(null);
        await loadCalendarMetadata();
        buildEmployeeFilterBar();
        initializeCalendar();
        bindNavigationButtons();
        bindViewSwitchers();
        bindViewByDropdown();
    } catch (error) {
        showCalendarError(error?.message || 'Không thể tải dữ liệu lịch.');
    }
}

function resolveCalendarEndpoints() {
    return calendarEndpoints[calendarState.mode] || calendarEndpoints.doctor;
}

async function loadCalendarMetadata() {
    const endpoints = resolveCalendarEndpoints();
    const response = await fetch(endpoints.metadata, { credentials: 'include' });
    if (!response.ok) {
        throw await buildApiError(response);
    }

    const metadata = await response.json();
    calendarState.metadata = metadata;

    const specialties = (metadata?.specialties ?? metadata?.Specialties ?? []).map(mapSpecialtyDto);
    calendarState.specialtyColorMap = new Map(
        specialties
            .filter(item => item.id)
            .map(item => [item.id, item.color || DEFAULT_SPECIALTY_COLOR])
    );

    if (calendarState.mode === 'admin') {
        calendarState.employees = (metadata?.doctors ?? metadata?.Doctors ?? [])
            .map(mapDoctorOption)
            .filter(item => !!item.id);
    } else {
        const doctorId = normalizeGuid(metadata?.doctorId ?? metadata?.DoctorId);
        const doctorName = metadata?.doctorName ?? metadata?.DoctorName ?? 'Bác sĩ';
        const doctorAvatar = metadata?.doctorAvatarUrl ?? metadata?.DoctorAvatarUrl ?? '';
        calendarState.employees = [
            {
                id: doctorId || 'current-doctor',
                name: doctorName,
                avatarUrl: doctorAvatar
            }
        ];
    }

    calendarState.selectedEmployeeIds = new Set(['all']);
}

function buildEmployeeFilterBar() {
    if (!employeeFilterBar) {
        return;
    }

    employeeFilterBar.innerHTML = '';

    if (!calendarState.employees.length) {
        const placeholder = document.createElement('span');
        placeholder.className = 'text-muted small';
        placeholder.textContent = 'Chưa có bác sĩ khả dụng';
        employeeFilterBar.appendChild(placeholder);
        return;
    }

    const allButton = document.createElement('button');
    allButton.type = 'button';
    allButton.className = 'employee-filter-btn btn';
    allButton.dataset.employeeId = 'all';
    allButton.innerHTML = `
        <span class="avatar-icon d-flex align-items-center justify-content-center">
            <i class="fa-solid fa-users"></i>
        </span>
        <span class="employee-name">Tất cả bác sĩ</span>`;
    employeeFilterBar.appendChild(allButton);

    calendarState.employees.forEach(employee => {
        if (!employee.id) {
            return;
        }

        const button = document.createElement('button');
        button.type = 'button';
        button.className = 'employee-filter-btn btn';
        button.dataset.employeeId = employee.id;

        if (employee.avatarUrl) {
            const img = document.createElement('img');
            img.src = employee.avatarUrl;
            img.alt = employee.name || 'Bác sĩ';
            img.width = 40;
            img.height = 40;
            img.className = 'avatar-img';
            button.appendChild(img);
        } else {
            const fallback = document.createElement('span');
            fallback.className = 'avatar-icon d-flex align-items-center justify-content-center';
            fallback.innerHTML = '<i class="fa-solid fa-user-doctor"></i>';
            button.appendChild(fallback);
        }

        const label = document.createElement('span');
        label.className = 'employee-name';
        label.textContent = employee.name || 'Bác sĩ';
        button.appendChild(label);
        employeeFilterBar.appendChild(button);
    });

    if (employeeFilterBar.dataset.bound !== '1') {
        employeeFilterBar.dataset.bound = '1';
        employeeFilterBar.addEventListener('click', handleEmployeeFilterClick);
    }

    updateEmployeeButtonStates();
}

function initializeCalendar() {
    const calendarEl = document.getElementById('calendar');
    if (!calendarEl) {
        console.error('Không tìm thấy phần tử lịch.');
        return;
    }

    if (typeof FullCalendar === 'undefined') {
        console.error('Thư viện FullCalendar chưa được tải.');
        return;
    }

    calendar = new FullCalendar.Calendar(calendarEl, {
        themeSystem: 'bootstrap5',
        headerToolbar: false,
        initialView: 'dayGridMonth',
        initialDate: new Date(),
        height: 800,
        editable: true,
        allDaySlot: false,
        firstDay: 1, // Bắt đầu từ Thứ 2
        navLinks: true,
        navLinkDayClick: 'timeGridDay',
        nowIndicator: true,

        // FIX: THÊM NGÔN NGỮ TIẾNG VIỆT
        locale: 'vi',
        buttonText: {
            today: 'Hôm nay',
            month: 'Tháng',
            week: 'Tuần',
            day: 'Ngày'
        },

        events: [],

        // Cập nhật tiêu đề VÀ nút active
        datesSet: async function (dateInfo) {
            calendarState.currentRange = {
                start: new Date(dateInfo.start),
                end: new Date(dateInfo.end)
            };

            if (calendarTitleEl) {
                calendarTitleEl.innerText = capitalizeFirst(dateInfo.view.title);
            }
            updateViewSwitcherButtons(dateInfo.view.type);
            await reloadCalendarEvents();
        },

        eventClick: function (info) {
            // (FullCalendar tự xử lý popover)
        },
        eventDidMount: function (arg) {
            const fcBg = (arg.event.extendedProps && arg.event.extendedProps.fcBg) || arg.event.backgroundColor || '#f8f9fa';
            const fcLeft = (arg.event.extendedProps && arg.event.extendedProps.fcLeft) || (arg.event.borderColor || '#6c757d');
            const fcText = (arg.event.extendedProps && arg.event.extendedProps.fcText) || arg.event.textColor || '#000000';

            try {
                arg.el.style.setProperty('background-color', fcBg, 'important');
                arg.el.style.setProperty('color', fcText, 'important');
                arg.el.style.setProperty('border-left', `4px solid ${fcLeft}`, 'important');
                arg.el.style.setProperty('border-radius', '6px', 'important');
                arg.el.style.setProperty('padding-left', '6px', 'important');

                const titleEl = arg.el.querySelector('.fc-event-main-text');
                if (titleEl) titleEl.style.setProperty('color', fcText, 'important');

                const timeEl = arg.el.querySelector('.fc-event-time-text, .fc-event-time');
                if (timeEl) {
                    const txt = timeEl.textContent || '';
                    if (txt.length > 0 && /[a-zA-ZÀ-ỹ]/.test(txt.charAt(0))) {
                        timeEl.textContent = txt.charAt(0).toUpperCase() + txt.slice(1);
                    }
                    timeEl.style.setProperty('color', fcText, 'important');
                }
            } catch (e) {
            }
        },

        // Bỏ giờ ở chế độ Month
        eventContent: function (arg) {
            let content = { html: '' };
            let title = arg.event.title;

            if (arg.view.type === 'dayGridMonth') {
                // Chế độ Month: Chỉ hiển thị tiêu đề
                content.html = `<div class="fc-event-main-text">${title}</div>`;
            } else {
                // Chế độ Week/Day: Hiển thị tiêu đề VÀ giờ
                let timeText = arg.timeText;
                if (timeText && timeText.length > 0 && /[a-zA-ZÀ-ỹ]/.test(timeText.charAt(0))) {
                    timeText = timeText.charAt(0).toUpperCase() + timeText.slice(1);
                }
                content.html = `
                    <div class="fc-event-main-text">${title}</div>
                    <div class="fc-event-time-text">${timeText}</div>
                `;
            }
            return content;
        },

        // Thêm nút "+" vào chế độ Month
        dayCellDidMount: function (arg) {
            if (arg.view.type === 'dayGridMonth') {
                let addIconEl = document.createElement('div');
                addIconEl.className = 'day-add-icon';
                addIconEl.innerHTML = '<i class="fa-solid fa-plus"></i>';
                addIconEl.addEventListener('click', (e) => {
                    e.stopPropagation();
                    // FIX: DỊCH CONSOLE.LOG
                    console.log('Thêm cuộc hẹn vào ngày', arg.date);
                    // (Mở modal/form tại đây)
                });
                const dayTopEl = arg.el.querySelector('.fc-daygrid-day-top');
                if (dayTopEl) {
                    dayTopEl.prepend(addIconEl);
                }
            }
        }
    });

    calendar.render();
}

function bindNavigationButtons() {
    document.getElementById('btnPrev')?.addEventListener('click', () => calendar?.prev());
    document.getElementById('btnNext')?.addEventListener('click', () => calendar?.next());
    document.getElementById('btnToday')?.addEventListener('click', () => calendar?.today());
}

function bindViewSwitchers() {
    if (!viewSwitchers) {
        return;
    }

    viewSwitchers.addEventListener('click', (e) => {
        if (!calendar || !e.target.matches('button')) {
            return;
        }

        const view = e.target.dataset.view;
        if (view) {
            calendar.changeView(view);
        }
    });
}

function bindViewByDropdown() {
    const dropdownMenu = document.getElementById('viewByDropdownMenu');
    if (!dropdownMenu) {
        return;
    }

    dropdownMenu.addEventListener('click', (e) => {
        e.preventDefault();
        const option = e.target.closest('a.dropdown-item');
        if (!option) {
            return;
        }

        const newValue = option.dataset.value;
        if (!newValue || newValue === calendarState.viewBy) {
            return;
        }

        calendarState.viewBy = newValue;
        if (viewByToggleEl) {
            viewByToggleEl.innerHTML = option.innerHTML;
        }

        refreshCalendarEvents();
    });
}

function updateViewSwitcherButtons(currentView) {
    if (!viewSwitchers) {
        return;
    }

    viewSwitchers.querySelectorAll('button').forEach(btn => {
        const btnView = btn.dataset.view;
        btn.classList.remove('btn-primary', 'btn-outline-secondary');
        btn.classList.add(btnView === currentView ? 'btn-primary' : 'btn-outline-secondary');
    });
}

async function reloadCalendarEvents() {
    if (!calendarState.currentRange) {
        return;
    }

    if (eventsRequestController) {
        eventsRequestController.abort();
        eventsRequestController = null;
    }

    eventsRequestController = new AbortController();
    const params = new URLSearchParams();
    const fromDate = formatDateOnly(calendarState.currentRange.start);
    const inclusiveEnd = subtractDays(calendarState.currentRange.end, 1);
    const toDate = formatDateOnly(inclusiveEnd);

    if (fromDate) {
        params.append('from', fromDate);
    }

    if (toDate) {
        params.append('to', toDate);
    }

    const endpoints = resolveCalendarEndpoints();
    const url = params.toString() ? `${endpoints.events}?${params}` : endpoints.events;

    try {
        const response = await fetch(url, {
            credentials: 'include',
            signal: eventsRequestController.signal
        });

        if (!response.ok) {
            throw await buildApiError(response);
        }

        const payload = await response.json();
        calendarState.events = Array.isArray(payload)
            ? payload.map(mapCalendarEventDto)
            : [];

        showCalendarError(null);
        refreshCalendarEvents();
    } catch (error) {
        if (error.name === 'AbortError') {
            return;
        }
        showCalendarError(error?.message || 'Không thể tải dữ liệu lịch.');
    } finally {
        eventsRequestController = null;
    }
}

function refreshCalendarEvents() {
    if (!calendar) {
        return;
    }

    let filteredEvents = [...calendarState.events];
    const selected = calendarState.selectedEmployeeIds;

    if (!selected.has('all')) {
        filteredEvents = filteredEvents.filter(evt => evt.doctorId && selected.has(evt.doctorId));
    }

    const processedEvents = processEvents(filteredEvents);
    calendar.removeAllEvents();
    calendar.addEventSource(processedEvents);
}

function processEvents(eventsToProcess) {
    return eventsToProcess.map(event => {
        const baseColor = event.specialtyColor || calendarState.specialtyColorMap.get(event.specialtyId) || DEFAULT_SPECIALTY_COLOR;
        const bgLight = lighten(baseColor, 60);
        const leftBorder = darken(baseColor, 25);

        return {
            id: event.id,
            start: event.startUtc,
            end: event.endUtc,
            title: resolveEventTitle(event),
            backgroundColor: bgLight,
            borderColor: leftBorder,
            textColor: '#000000',
            extendedProps: {
                ...event,
                fcBg: bgLight,
                fcLeft: leftBorder,
                fcText: '#000000'
            }
        };
    });
}

function resolveEventTitle(event) {
    switch (calendarState.viewBy) {
        case 'customer':
            return event.patientName || 'Bệnh nhân';
        case 'service':
            return event.specialtyName || 'Chuyên khoa';
        default:
            return event.doctorName || 'Bác sĩ';
    }
}

function handleEmployeeFilterClick(e) {
    const button = e.target.closest('.employee-filter-btn');
    if (!button) {
        return;
    }

    const employeeId = button.dataset.employeeId;
    if (!employeeId) {
        return;
    }

    if (employeeId === 'all') {
        calendarState.selectedEmployeeIds = new Set(['all']);
    } else {
        if (calendarState.selectedEmployeeIds.has('all')) {
            calendarState.selectedEmployeeIds.delete('all');
        }

        if (calendarState.selectedEmployeeIds.has(employeeId)) {
            calendarState.selectedEmployeeIds.delete(employeeId);
        } else {
            calendarState.selectedEmployeeIds.add(employeeId);
        }

        if (calendarState.selectedEmployeeIds.size === 0) {
            calendarState.selectedEmployeeIds.add('all');
        }
    }

    updateEmployeeButtonStates();
    refreshCalendarEvents();
}

function updateEmployeeButtonStates() {
    if (!employeeFilterBar) {
        return;
    }

    employeeFilterBar.querySelectorAll('.employee-filter-btn').forEach(btn => {
        const id = btn.dataset.employeeId;
        if (!id) {
            return;
        }

        if (id === 'all') {
            btn.classList.toggle('active', calendarState.selectedEmployeeIds.has('all'));
        } else {
            btn.classList.toggle('active', calendarState.selectedEmployeeIds.has(id));
        }
    });
}

function showCalendarError(message) {
    if (!calendarErrorEl) {
        return;
    }

    if (!message) {
        calendarErrorEl.classList.add('d-none');
        calendarErrorEl.textContent = '';
        return;
    }

    calendarErrorEl.textContent = message;
    calendarErrorEl.classList.remove('d-none');
}

function hexToRgb(hex) {
    if (!hex) return { r: 0, g: 0, b: 0 };
    let clean = hex.replace('#', '');
    if (clean.length === 3) {
        clean = clean.split('').map(c => c + c).join('');
    }
    const bigint = parseInt(clean, 16);
    return {
        r: (bigint >> 16) & 255,
        g: (bigint >> 8) & 255,
        b: bigint & 255
    };
}

function rgbToHex(r, g, b) {
    const clamp = v => Math.max(0, Math.min(255, Math.round(v)));
    return '#' + [clamp(r), clamp(g), clamp(b)].map(c => c.toString(16).padStart(2, '0')).join('');
}

function lighten(hex, percent) {
    const { r, g, b } = hexToRgb(hex);
    const nr = Math.round(r + (255 - r) * (percent / 100));
    const ng = Math.round(g + (255 - g) * (percent / 100));
    const nb = Math.round(b + (255 - b) * (percent / 100));
    return rgbToHex(nr, ng, nb);
}

function darken(hex, percent) {
    const { r, g, b } = hexToRgb(hex);
    const nr = Math.round(r * (1 - percent / 100));
    const ng = Math.round(g * (1 - percent / 100));
    const nb = Math.round(b * (1 - percent / 100));
    return rgbToHex(nr, ng, nb);
}

function capitalizeFirst(value) {
    if (!value) {
        return value;
    }
    return value.charAt(0).toUpperCase() + value.slice(1);
}

function formatDateOnly(date) {
    if (!date || Number.isNaN(date.getTime())) {
        return null;
    }

    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
}

function subtractDays(date, days) {
    if (!date) {
        return null;
    }
    const clone = new Date(date.getTime());
    clone.setDate(clone.getDate() - days);
    return clone;
}

function mapSpecialtyDto(source) {
    return {
        id: normalizeGuid(source?.id ?? source?.Id),
        name: source?.name ?? source?.Name ?? '',
        color: source?.color ?? source?.Color ?? DEFAULT_SPECIALTY_COLOR
    };
}

function mapDoctorOption(source) {
    return {
        id: normalizeGuid(source?.id ?? source?.Id),
        name: source?.name ?? source?.Name ?? '',
        avatarUrl: source?.avatarUrl ?? source?.AvatarUrl ?? ''
    };
}

function mapCalendarEventDto(source) {
    return {
        id: source?.id ?? source?.Id,
        doctorId: normalizeGuid(source?.doctorId ?? source?.DoctorId),
        doctorName: source?.doctorName ?? source?.DoctorName ?? '',
        doctorAvatarUrl: source?.doctorAvatarUrl ?? source?.DoctorAvatarUrl ?? '',
        specialtyId: normalizeGuid(source?.specialtyId ?? source?.SpecialtyId),
        specialtyName: source?.specialtyName ?? source?.SpecialtyName ?? '',
        specialtyColor: source?.specialtyColor ?? source?.SpecialtyColor ?? DEFAULT_SPECIALTY_COLOR,
        patientName: source?.patientName ?? source?.PatientName ?? '',
        customerPhone: source?.customerPhone ?? source?.CustomerPhone ?? '',
        status: source?.status ?? source?.Status ?? '',
        statusLabel: source?.statusLabel ?? source?.StatusLabel ?? '',
        statusTone: source?.statusTone ?? source?.StatusTone ?? 'pending',
        statusIcon: source?.statusIcon ?? source?.StatusIcon ?? 'fa-circle',
        clinicRoom: source?.clinicRoom ?? source?.ClinicRoom ?? '',
        startUtc: source?.startUtc ?? source?.StartUtc,
        endUtc: source?.endUtc ?? source?.EndUtc
    };
}

function normalizeGuid(value) {
    if (!value) {
        return '';
    }
    return String(value).toLowerCase();
}

async function buildApiError(response) {
    try {
        const payload = await response.json();
        const message = payload?.title || payload?.detail;
        if (message) {
            return new Error(message);
        }
    } catch (error) {
        // Bỏ qua lỗi parse
    }

    return new Error('Máy chủ không phản hồi. Vui lòng thử lại.');
}
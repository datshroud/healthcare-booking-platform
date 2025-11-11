document.addEventListener('DOMContentLoaded', function () {

    // --- 1. DỮ LIỆU VÀ CẤU HÌNH ---

    // Dữ liệu mẫu
    const rawEvents = [
        {
            id: '1',
            start: '2025-11-03T10:00:00',
            end: '2025-11-03T11:00:00',
            service: 'Therapeutic Exercise',
            customer: 'Jone Doe',
            employee: 'Jane Doe'
        },
        {
            id: '2',
            start: '2025-11-04T10:30:00',
            end: '2025-11-04T11:30:00',
            service: 'Therapeutic Stretching',
            customer: 'Jone Doe',
            employee: 'Mark Roe'
        },
        {
            id: '3',
            start: '2025-11-05T11:00:00',
            end: '2025-11-05T12:30:00',
            service: 'Heat and Ice Therapy',
            customer: 'Jone Doe',
            employee: 'Sara Smith'
        },
        {
            id: '4',
            start: '2025-11-05T09:00:00',
            end: '2025-11-05T10:00:00',
            service: 'Therapeutic Exercise',
            customer: 'Alex Smith',
            employee: 'Le Ngoc Bao Chan'
        },
        {
            id: '5',
            start: '2025-11-11T10:00:00',
            end: '2025-11-11T11:00:00',
            service: 'Heat and Ice Therapy',
            customer: 'Jone Doe',
            employee: 'Jane Doe'
        }
    ];

    const serviceColors = {
        'Therapeutic Exercise': '#0dcaf0',
        'Therapeutic Stretching': '#fd7e14',
        'Heat and Ice Therapy': '#d63384',
        'default': '#6c757d'
    };

    let currentViewBy = 'employee'; // Mặc định là Employee
    let selectedEmployeeIds = ['all'];

    const calendarTitleEl = document.getElementById('calendarTitle');
    const viewByToggleEl = document.getElementById('viewByDropdownToggle'); // Nút "View by"
    const employeeFilterBar = document.querySelector('.employee-filter-bar');
    const viewSwitchers = document.getElementById('viewSwitchers');

    // Convert hex -> rgb
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

    function capitalizeFirst(s) {
        if (!s) return s;
        return s.charAt(0).toUpperCase() + s.slice(1);
    }

    /**
     * Tải lại và hiển thị sự kiện
     */
    function refreshCalendarEvents() {
        // 1. Lọc theo Nhân viên
        let filteredEvents = rawEvents;
        if (!selectedEmployeeIds.includes('all') && selectedEmployeeIds.length > 0) {
            filteredEvents = rawEvents.filter(event => selectedEmployeeIds.includes(event.employee));
        }

        // 2. Xử lý "View By" (đổi tiêu đề) và Màu (luôn theo service)
        const processedEvents = processEvents(filteredEvents);

        // 3. Cập nhật lịch
        calendar.removeAllEvents();
        calendar.addEventSource(processedEvents);
    }

    function processEvents(eventsToProcess) {
        return eventsToProcess.map(event => {
            let title = '';

            // 1. Title thay đổi theo "currentViewBy"
            if (currentViewBy === 'employee') {
                title = event.employee;
            } else if (currentViewBy === 'customer') {
                title = event.customer;
            } else { // service
                title = event.service;
            }

            // 2. Màu sắc LUÔN LUÔN dựa trên "service"
            const base = serviceColors[event.service] || serviceColors['default'];

            const bgLight = lighten(base, 60); 
            const leftBorder = darken(base, 25);

            return {
                ...event, // Giữ lại start, end, id...
                title: title, // Tiêu đề động
                backgroundColor: bgLight,
                borderColor: leftBorder,
                textColor: '#000000',
                fcBg: bgLight,
                fcLeft: leftBorder,
                fcText: '#000000'
            };
        });
    }

    // --- 2. KHỞI TẠO FULLCALENDAR ---

    const calendarEl = document.getElementById('calendar');
    const calendar = new FullCalendar.Calendar(calendarEl, {
        themeSystem: 'bootstrap5',
        headerToolbar: false,
        initialView: 'dayGridMonth',
        initialDate: '2025-11-11',
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

        events: processEvents(rawEvents), // Tải lần đầu (tất cả)

        // Cập nhật tiêu đề VÀ nút active
        datesSet: function (dateInfo) {
            if (calendarTitleEl) {
                calendarTitleEl.innerText = capitalizeFirst(dateInfo.view.title);
            }
            if (viewSwitchers) {
                const currentView = dateInfo.view.type;
                viewSwitchers.querySelectorAll('button').forEach(btn => {
                    const btnView = btn.dataset.view;
                    btn.classList.remove('btn-primary', 'btn-outline-secondary');
                    btn.classList.add(btnView === currentView ? 'btn-primary' : 'btn-outline-secondary');
                });
            }
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

    // --- 3. KẾT NỐI CÁC NÚT ĐIỀU KHIỂN TÙY CHỈNH ---

    // Nút điều hướng
    document.getElementById('btnPrev')?.addEventListener('click', () => calendar.prev());
    document.getElementById('btnNext')?.addEventListener('click', () => calendar.next());
    document.getElementById('btnToday')?.addEventListener('click', () => calendar.today());

    // Nút đổi chế độ xem (Month/Week/Day)
    viewSwitchers?.addEventListener('click', (e) => {
        if (e.target.matches('button')) {
            const view = e.target.dataset.view;
            if (view) {
                calendar.changeView(view);
            }
        }
    });

    // Dropdown "View By"
    document.getElementById('viewByDropdownMenu')?.addEventListener('click', (e) => {
        e.preventDefault();
        if (e.target.matches('a.dropdown-item')) {
            const newValue = e.target.dataset.value;
            const newText = e.target.innerHTML; // Lấy cả icon và chữ

            if (newValue !== currentViewBy) {
                currentViewBy = newValue;

                // Cập nhật text của nút dropdown
                if (viewByToggleEl) {
                    viewByToggleEl.innerHTML = newText;
                }

                // Tải lại sự kiện với tiêu đề mới (màu giữ nguyên)
                refreshCalendarEvents();
            }
        }
    });

    // --- 4. LOGIC LỌC NHÂN VIÊN ---
    employeeFilterBar?.addEventListener('click', (e) => {
        const clickedButton = e.target.closest('.employee-filter-btn');
        if (!clickedButton) return;

        const clickedId = clickedButton.dataset.employeeId;
        const allEmployeesBtn = employeeFilterBar.querySelector('[data-employee-id="all"]');

        if (clickedId === 'all') {
            // Nếu bấm "All"
            selectedEmployeeIds = ['all'];
            employeeFilterBar.querySelectorAll('.employee-filter-btn').forEach(btn => {
                btn.classList.toggle('active', btn === allEmployeesBtn);
            });
        } else {
            // Nếu bấm vào 1 nhân viên
            allEmployeesBtn.classList.remove('active');
            selectedEmployeeIds = selectedEmployeeIds.filter(id => id !== 'all');

            const index = selectedEmployeeIds.indexOf(clickedId);
            if (index > -1) {
                selectedEmployeeIds.splice(index, 1);
                clickedButton.classList.remove('active');
            } else {
                selectedEmployeeIds.push(clickedId);
                clickedButton.classList.add('active');
            }

            if (selectedEmployeeIds.length === 0) {
                selectedEmployeeIds = ['all'];
                allEmployeesBtn.classList.add('active');
            }
        }

        refreshCalendarEvents();
    });

});
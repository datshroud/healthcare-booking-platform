/* DÁN ĐÈ LÊN FILE: wwwroot/js/calendar.js */

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
        }
    ];

    // Định nghĩa màu sắc
    const colors = {
        employee: { 'Jane Doe': '#0d6efd', 'Mark Roe': '#198754', 'Sara Smith': '#dc3545', 'Le Ngoc Bao Chan': '#6f42c1', 'default': '#6c757d' },
        customer: { 'Jone Doe': '#ffc107', 'Alex Smith': '#fd7e14', 'default': '#6c757d' },
        service: { 'Therapeutic Exercise': '#0dcaf0', 'Therapeutic Stretching': '#fd7e14', 'Heat and Ice Therapy': '#d63384', 'default': '#6c757d' }
    };

    let currentViewBy = 'employee';
    let selectedEmployeeIds = ['all'];

    const calendarTitleEl = document.getElementById('calendarTitle');
    const viewByToggleEl = document.getElementById('viewByDropdownToggle');
    const employeeFilterBar = document.querySelector('.employee-filter-bar');
    const viewSwitchers = document.getElementById('viewSwitchers'); // Lấy DOM 1 lần

    /**
     * Tải lại và hiển thị sự kiện
     */
    function refreshCalendarEvents() {
        let filteredEvents = rawEvents;
        if (!selectedEmployeeIds.includes('all') && selectedEmployeeIds.length > 0) {
            filteredEvents = rawEvents.filter(event => selectedEmployeeIds.includes(event.employee));
        }

        const processedEvents = processEvents(currentViewBy, filteredEvents);

        calendar.removeAllEvents();
        calendar.addEventSource(processedEvents);
    }

    /**
     * Xử lý "View By" (đổi màu và tiêu đề)
     */
    function processEvents(viewBy, eventsToProcess) {
        return eventsToProcess.map(event => {
            let title = '';
            let color = '';
            let colorMap = {};

            if (viewBy === 'employee') {
                title = event.employee;
                colorMap = colors.employee;
                color = colorMap[event.employee] || colorMap['default'];
            } else if (viewBy === 'customer') {
                title = event.customer;
                colorMap = colors.customer;
                color = colorMap[event.customer] || colorMap['default'];
            } else { // service
                title = event.service;
                colorMap = colors.service;
                color = colorMap[event.service] || colorMap['default'];
            }

            return {
                ...event,
                title: title,
                backgroundColor: color,
                borderColor: color
            };
        });
    }

    // --- 2. KHỞI TẠO FULLCALENDAR ---

    const calendarEl = document.getElementById('calendar');
    const calendar = new FullCalendar.Calendar(calendarEl, {
        themeSystem: 'bootstrap5',
        headerToolbar: false,
        initialView: 'dayGridMonth',
        initialDate: '2025-11-01',
        height: 800,
        editable: true,

        allDaySlot: false,

        navLinks: true,
        navLinkDayClick: 'timeGridDay',

        events: processEvents(currentViewBy, rawEvents),

        datesSet: function (dateInfo) {
            if (calendarTitleEl) {
                calendarTitleEl.innerText = dateInfo.view.title;
            }

            // Cập nhật nút Month/Week/Day
            if (viewSwitchers) {
                const currentView = dateInfo.view.type; // e.g., "timeGridDay"
                viewSwitchers.querySelectorAll('button').forEach(btn => {
                    const btnView = btn.dataset.view;
                    btn.classList.remove('btn-primary', 'btn-outline-secondary');
                    btn.classList.add(btnView === currentView ? 'btn-primary' : 'btn-outline-secondary');
                });
            }
        },

        eventClick: function (info) {
        }
    });

    calendar.render();

    // --- 3. KẾT NỐI CÁC NÚT ĐIỀU KHIỂN TÙY CHỈNH ---

    // Nút điều hướng
    document.getElementById('btnPrev')?.addEventListener('click', () => calendar.prev());
    document.getElementById('btnNext')?.addEventListener('click', () => calendar.next());
    document.getElementById('btnToday')?.addEventListener('click', () => calendar.today());

    // Nút đổi chế độ xem (Month/Week/Day)
    // (Callback 'datesSet' ở trên sẽ tự xử lý việc đổi màu active)
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
            const newText = e.target.innerHTML;

            if (newValue !== currentViewBy) {
                currentViewBy = newValue;
                if (viewByToggleEl) {
                    viewByToggleEl.innerHTML = newText;
                }
                refreshCalendarEvents(); // Tải lại sự kiện
            }
        }
    });

    // --- 4. LOGIC LỌC NHÂN VIÊN ---
    employeeFilterBar.addEventListener('click', (e) => {
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
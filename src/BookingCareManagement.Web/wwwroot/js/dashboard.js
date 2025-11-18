document.addEventListener("DOMContentLoaded", () => {
    const FONT_FAMILY = "'Be Vietnam Pro', sans-serif";

    // --- KIỂM TRA DỮ LIỆU TỪ C# ---
    if (typeof dashboardData === 'undefined') {
        console.error('LỖI: Không tìm thấy dữ liệu "dashboardData" từ C#. Biểu đồ sẽ dùng dữ liệu tĩnh.');
        // Tạo dữ liệu giả nếu không có
        window.dashboardData = {
            newCustomerSparkline: [0, 0, 0, 1, 1, 0, 0],
            revenueSparkline: [0, 0, 0, 0, 0, 0, 0],
            pendingSparkline: [0, 1, 0, 0, 1, 0, 0], // Dữ liệu giả mới
            mainLineChart: [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1],
            mainLineLabels: ['W', 'T', 'F', 'S', 'S', 'M', 'T', 'W', 'T', 'F', 'S', 'S', 'M', 'T', 'W', 'T', 'F', 'S', 'S', 'M', 'T'],
            donutNew: 100,
            donutReturning: 0
        };
    }

    // --- Cấu hình chung cho Sparkline ---
    const sparklineOptions = {
        chart: { type: 'line', height: 80, sparkline: { enabled: true } },
        stroke: { curve: 'smooth', width: 3 },
        tooltip: { enabled: true, x: { show: false }, y: { title: { formatter: () => '' } }, marker: { show: false } },
        grid: { padding: { top: 10, bottom: 10 } }
    };

    // --- Biểu đồ 1: Khách hàng mới (Sparkline) ---
    let optionsNewCustomer = {
        ...sparklineOptions,
        series: [{
            name: 'Khách hàng',
            data: dashboardData.newCustomerSparkline
        }],
        colors: ['#28a745']
    };
    let chartNewCustomer = new ApexCharts(document.querySelector("#sparkline-new-customer"), optionsNewCustomer);
    chartNewCustomer.render();

    // --- Biểu đồ 2: Doanh thu (Sparkline) ---
    let optionsRevenue = {
        ...sparklineOptions,
        series: [{
            name: 'Doanh thu',
            data: dashboardData.revenueSparkline
        }],
        colors: ['#6c757d'],
        tooltip: {
            enabled: true,
            x: { show: false },
            y: {
                title: { formatter: () => '' },
                formatter: (val) => new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val)
            },
            marker: { show: false }
        },
    };
    let chartRevenue = new ApexCharts(document.querySelector("#sparkline-revenue"), optionsRevenue);
    chartRevenue.render();

    // --- Biểu đồ 3: Lịch hẹn đang chờ (Sparkline) ---
    // ⭐️ THAY THẾ: "Chiếm dụng" -> "Đang chờ" ⭐️
    let optionsPending = {
        ...sparklineOptions,
        series: [{
            name: 'Đang chờ',
            data: dashboardData.pendingSparkline // Dữ liệu mới
        }],
        colors: ['#f0ac41'] // Màu Vàng/Cam
    };
    let chartPending = new ApexCharts(document.querySelector("#sparkline-pending"), optionsPending); // ID MỚI
    chartPending.render();

    // --- Biểu đồ 4: Biểu đồ đường chính (Lịch hẹn) ---
    let optionsMainLine = {
        series: [{
            name: 'Lịch hẹn',
            data: dashboardData.mainLineChart
        }],
        chart: {
            height: 250,
            type: 'line',
            toolbar: { show: false },
            zoom: { enabled: false }
        },
        colors: ['#0d6efd'],
        stroke: {
            curve: 'straight',
            width: 4
        },
        grid: {
            borderColor: '#f0f0f0',
            strokeDashArray: 4
        },
        xaxis: {
            categories: dashboardData.mainLineLabels, // Dữ liệu thật
            labels: {
                style: {
                    fontFamily: FONT_FAMILY,
                    colors: '#6c757d',
                    fontWeight: 500,
                    fontSize: '12px'
                }
            },
            axisBorder: { show: false },
            axisTicks: { show: false }
        },
        yaxis: {
            labels: {
                style: {
                    fontFamily: FONT_FAMILY,
                    colors: '#6c757d',
                    fontWeight: 500,
                    fontSize: '12px'
                }
            }
        },
        tooltip: {
            enabled: true,
            x: { show: false },
            style: {
                fontFamily: FONT_FAMILY,
            }
        }
    };
    let chartMainLine = new ApexCharts(document.querySelector("#main-line-chart"), optionsMainLine);
    chartMainLine.render();

    // --- Cấu hình chung cho Biểu đồ tròn ---
    const donutOptions = {
        chart: {
            type: 'donut',
            height: 200
        },
        plotOptions: {
            pie: {
                donut: {
                    size: '75%',
                    labels: {
                        show: true,
                        name: {
                            show: false
                        },
                        value: {
                            show: true,
                            fontSize: '2rem',
                            fontFamily: FONT_FAMILY,
                            fontWeight: 700,
                            color: '#333',
                            offsetY: 8,
                            formatter: (val) => val + "%"
                        }
                    }
                }
            }
        },
        dataLabels: {
            enabled: false
        },
        legend: {
            show: false
        },
        tooltip: {
            enabled: false
        }
    };

    // --- Biểu đồ 5: Khách hàng mới (Donut) ---
    let optionsDonutNew = {
        ...donutOptions,
        series: [dashboardData.donutNew],
        labels: ['Khách hàng mới'],
        colors: ['#0d6efd']
    };
    let chartDonutNew = new ApexCharts(document.querySelector("#donut-new-customer"), optionsDonutNew);
    chartDonutNew.render();

    // --- Biểu đồ 6: Khách hàng quay lại (Donut) ---
    let optionsDonutReturning = {
        ...donutOptions,
        series: [dashboardData.donutReturning],
        labels: ['Khách hàng quay lại'],
        colors: ['#e9ecef']
    };
    let chartDonutReturning = new ApexCharts(document.querySelector("#donut-returning-customer"), optionsDonutReturning);
    chartDonutReturning.render();

    // --- XÓA NÚT DEMO ---
    const demoButton = document.getElementById('updateDataButton');
    if (demoButton) {
        demoButton.remove();
    }

    // --- Kích hoạt Popover cho Heatmap (Giữ nguyên) ---
    const heatmapCells = document.querySelectorAll('.heatmap-cell');
    heatmapCells.forEach(cell => {
        const date = cell.getAttribute('data-date');
        const percentage = cell.getAttribute('data-percentage');
        if (!date) {
            return;
        }
        // Sửa định dạng ngày (an toàn hơn)
        const dateParts = date.split('-'); // yyyy-MM-dd
        const formattedDate = `${dateParts[2]}/${dateParts[1]}/${dateParts[0]}`;

        const popoverContent = `
                        <strong>Ngày:</strong> ${formattedDate}<br>
                        <strong>Công suất:</strong> ${percentage}
                    `;
        const popover = new bootstrap.Popover(cell, {
            title: 'Chi tiết công suất',
            content: popoverContent,
            trigger: 'click',
            placement: 'top',
            html: true,
            customClass: 'shadow-sm'
        });
        cell.addEventListener('shown.bs.popover', () => {
            heatmapCells.forEach(otherCell => {
                if (otherCell !== cell) {
                    const otherPopover = bootstrap.Popover.getInstance(otherCell);
                    if (otherPopover) {
                        otherPopover.hide();
                    }
                }
            });
        });
    });


    // ⭐️⭐️⭐️ BẮT ĐẦU LOGIC LỌC CUỘC HẸN ⭐️⭐️⭐️

    // 1. Hàm lọc danh sách cuộc hẹn
    function filterAppointmentList() {
        const filterDropdown = document.getElementById('appointmentStatusFilter');
        if (!filterDropdown) return;

        const selectedStatus = filterDropdown.value;
        const appointmentList = document.getElementById('recentAppointmentsList');
        if (!appointmentList) return;

        const appointments = appointmentList.querySelectorAll('.appointment-item');

        appointments.forEach(item => {
            const itemStatus = item.dataset.status; // Lấy status từ data-status

            // Hiển thị nếu là "all" hoặc status khớp
            if (selectedStatus === 'all' || itemStatus === selectedStatus) {
                // Đổi từ 'display = ""' sang 'display = "grid"'
                // để khớp với CSS grid của '.appointment-item'
                item.style.display = 'grid';
            } else {
                item.style.display = 'none'; // Ẩn đi
            }
        });
    }

    // 2. Gán sự kiện 'change' cho dropdown
    const statusFilterDropdown = document.getElementById('appointmentStatusFilter');
    if (statusFilterDropdown) {
        statusFilterDropdown.addEventListener('change', filterAppointmentList);
    }
});
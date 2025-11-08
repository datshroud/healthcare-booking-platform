document.addEventListener("DOMContentLoaded", () => {
    const FONT_FAMILY = "'Be Vietnam Pro', sans-serif";

    // --- Cấu hình chung cho Sparkline ---
    const sparklineOptions = {
        chart: {
            type: 'line',
            height: 80,
            sparkline: {
                enabled: true
            }
        },
        stroke: {
            curve: 'smooth',
            width: 3
        },
        tooltip: {
            enabled: true,
            x: { show: false },
            y: {
                title: {
                    formatter: () => ''
                }
            },
            marker: { show: false }
        },
        grid: {
            padding: {
                top: 10,
                bottom: 10
            }
        }
    };

    // --- Biểu đồ 1: Khách hàng mới (Sparkline) ---
    let optionsNewCustomer = {
        ...sparklineOptions,
        series: [{
            name: 'Khách hàng',
            data: [0, 0, 0, 1, 1, 0, 0]
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
            data: [0, 0, 0, 0, 0, 0, 0]
        }],
        colors: ['#6c757d']
    };
    let chartRevenue = new ApexCharts(document.querySelector("#sparkline-revenue"), optionsRevenue);
    chartRevenue.render();

    // --- Biểu đồ 3: Sự chiếm dụng (Sparkline) ---
    let optionsOccupancy = {
        ...sparklineOptions,
        series: [{
            name: 'Chiếm dụng',
            data: [0.5, 0.5, 0.5, 1.3, 1.3, 0.5, 0.5]
        }],
        colors: ['#28a745']
    };
    let chartOccupancy = new ApexCharts(document.querySelector("#sparkline-occupancy"), optionsOccupancy);
    chartOccupancy.render();

    // --- Biểu đồ 4: Biểu đồ đường chính (Lịch hẹn) ---
    let optionsMainLine = {
        series: [{
            name: 'Lịch hẹn',
            data: [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1]
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
            categories: ['W', 'T', 'F', 'S', 'S', 'M', 'T', 'W', 'T', 'F', 'S', 'S', 'M', 'T', 'W', 'T', 'F', 'S', 'S', 'M', 'T', 'W', 'T', 'F'],
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
        series: [100],
        labels: ['Khách hàng mới'],
        colors: ['#0d6efd']
    };
    let chartDonutNew = new ApexCharts(document.querySelector("#donut-new-customer"), optionsDonutNew);
    chartDonutNew.render();

    // --- Biểu đồ 6: Khách hàng quay lại (Donut) ---
    let optionsDonutReturning = {
        ...donutOptions,
        series: [0],
        labels: ['Khách hàng quay lại'],
        colors: ['#e9ecef']
    };
    let chartDonutReturning = new ApexCharts(document.querySelector("#donut-returning-customer"), optionsDonutReturning);
    chartDonutReturning.render();

    // --- Chức năng Demo Cập nhật Dữ liệu ---
    const updateData = () => {
        let newMainData = Array.from({ length: 21 }, () => Math.floor(Math.random() * 5));
        let newSparkleData = Array.from({ length: 7 }, () => Math.floor(Math.random() * 5));
        let newDonutNewVal = Math.floor(Math.random() * 100);
        let newDonutReturningVal = 100 - newDonutNewVal;

        chartMainLine.updateSeries([{ data: newMainData }]);
        chartNewCustomer.updateSeries([{ data: newSparkleData }]);
        chartDonutNew.updateSeries([newDonutNewVal]);
        chartDonutReturning.updateSeries([newDonutReturningVal]);
        document.querySelector('.card-value').innerText = newSparkleData.reduce((a, b) => a + b, 0);
    };

    document.getElementById('updateDataButton').addEventListener('click', updateData);

    // --- CẬP NHẬT 2: Kích hoạt Popover cho Heatmap ---

    // Lấy tất cả các ô heatmap
    const heatmapCells = document.querySelectorAll('.heatmap-cell');

    // Đóng tất cả popover đang mở
    function closeAllPopovers() {
        heatmapCells.forEach(cell => {
            const popover = bootstrap.Popover.getInstance(cell);
            if (popover) {
                popover.hide();
            }
        });
    }

    // Khởi tạo và gán sự kiện cho từng ô
    heatmapCells.forEach(cell => {
        const date = cell.getAttribute('data-date');
        const percentage = cell.getAttribute('data-percentage');

        // Nếu ô không có dữ liệu (không có ngày), thì bỏ qua
        if (!date) {
            return;
        }

        // Định dạng lại ngày (từ yyyy-mm-dd -> dd/mm/yyyy)
        const dateObj = new Date(date + 'T00:00:00'); // Thêm T00 để tránh lỗi múi giờ
        const formattedDate = dateObj.toLocaleDateString('vi-VN');

        // Tạo nội dung cho popover
        const popoverContent = `
                    <strong>Ngày:</strong> ${formattedDate}<br>
                    <strong>Công suất:</strong> ${percentage}
                `;

        // Khởi tạo popover cho ô này
        const popover = new bootstrap.Popover(cell, {
            title: 'Chi tiết công suất',
            content: popoverContent,
            trigger: 'click', // Kích hoạt khi bấm
            placement: 'top',
            html: true,       // Cho phép chèn HTML
            customClass: 'shadow-sm'
        });

        // Thêm sự kiện: Khi một popover được mở, đóng tất cả các cái khác
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

});
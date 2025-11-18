(function () {
    const API_BASE = "/api/doctor/dashboard";
    const APPOINTMENT_API = "/api/doctor/appointments";
    const STAT_ENDPOINTS = {
        "new-customers": "new-customers",
        "revenue": "revenue",
        "occupancy": "occupancy"
    };

    document.addEventListener("DOMContentLoaded", () => {
        const root = document.querySelector("[data-doctor-dashboard]");
        if (!root) {
            return;
        }
        const dashboard = new DoctorDashboard(root);
        dashboard.init();
    });

    class DoctorDashboard {
        constructor(root) {
            this.root = root;
            this.statCards = Array.from(root.querySelectorAll("[data-stat-card]"));
            this.sparklineCharts = new Map();
            this.trendChart = null;
            this.customerMixChart = null;
            this.heatmapMonthInput = root.querySelector("[data-heatmap-month]");
            this.heatmapGrid = root.querySelector("[data-heatmap-grid]");
            this.trendRangeSelect = root.querySelector("[data-trend-range]");
            this.trendLabel = root.querySelector("[data-trend-label]");
            this.trendConfirmed = root.querySelector("[data-trend-confirmed]");
            this.trendCanceled = root.querySelector("[data-trend-canceled]");
            this.mixRangeSelect = root.querySelector("[data-mix-range]") || this.trendRangeSelect;
            this.mixLabel = root.querySelector("[data-mix-label]");
            this.mixNewValue = root.querySelector("[data-mix-new]");
            this.mixReturnValue = root.querySelector("[data-mix-return]");
            this.performanceRangeSelect = root.querySelector("[data-performance-range]");
            this.performanceServices = root.querySelector("[data-performance-services]");
            this.performancePatients = root.querySelector("[data-performance-patients]");
            this.upcomingLabel = root.querySelector("[data-upcoming-label]");
            this.upcomingList = root.querySelector("[data-upcoming-list]");
            this.clockElement = root.querySelector("[data-dashboard-clock]");
        }

        init() {
            this.bindStatSelects();
            this.bindTrendSelect();
            this.bindMixSelect();
            this.bindPerformanceSelect();
            this.bindHeatmapInput();
            this.refreshAll();
            this.startClock();
        }

        bindStatSelects() {
            this.statCards.forEach((card) => {
                const select = card.querySelector("[data-range-select]");
                select?.addEventListener("change", () => {
                    this.loadStatCard(card, select.value);
                });
            });
        }

        bindTrendSelect() {
            this.trendRangeSelect?.addEventListener("change", () => {
                this.loadTrend();
            });
        }

        bindMixSelect() {
            this.mixRangeSelect?.addEventListener("change", () => {
                this.loadCustomerMix();
            });
        }

        bindPerformanceSelect() {
            this.performanceRangeSelect?.addEventListener("change", () => {
                this.loadPerformance();
            });
        }

        bindHeatmapInput() {
            if (!this.heatmapMonthInput) {
                return;
            }

            if (!this.heatmapMonthInput.value) {
                const now = new Date();
                this.heatmapMonthInput.value = `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, "0")}`;
            }

            this.heatmapMonthInput.addEventListener("change", () => {
                this.loadHeatmap();
            });
        }

        async refreshAll() {
            const tasks = [
                ...this.statCards.map((card) => this.loadStatCard(card, this.getSelectValue(card, "[data-range-select]"))),
                this.loadTrend(),
                this.loadCustomerMix(),
                this.loadHeatmap(),
                this.loadPerformance(),
                this.loadUpcomingAppointments()
            ];

            await Promise.allSettled(tasks);
        }

        async loadStatCard(card, range) {
            const key = card.dataset.statCard;
            if (!key || !STAT_ENDPOINTS[key]) {
                return;
            }

            const labelEl = card.querySelector("[data-stat-label]");
            const valueEl = card.querySelector("[data-stat-value]");
            const chartEl = card.querySelector("[data-stat-chart]");
            try {
                labelEl.textContent = "Đang tải...";
                valueEl.textContent = "--";
                const endpoint = `${API_BASE}/${STAT_ENDPOINTS[key]}?${new URLSearchParams({ range: range || "this-week" })}`;
                const response = await fetchJson(endpoint);
                if (!response) {
                    labelEl.textContent = "Không có dữ liệu";
                    return;
                }

                labelEl.textContent = response.rangeLabel || "";
                valueEl.textContent = this.formatStatValue(key, response.total);
                this.updateSparkline(key, chartEl, response.points || []);
            } catch (error) {
                console.error("Doctor dashboard stat error", error);
                labelEl.textContent = "Không thể tải";
            }
        }

        updateSparkline(key, container, points) {
            if (!container || typeof ApexCharts === "undefined") {
                return;
            }

            const seriesData = (points || []).map((point) => ({
                x: point.date,
                y: Number(point.value) || 0
            }));

            const existing = this.sparklineCharts.get(key);
            if (existing) {
                existing.updateOptions({
                    yaxis: this.resolveSparklineAxisConfig(key)
                }, false, true);
                existing.updateSeries([{ name: "Giá trị", data: seriesData }], true);
                return;
            }

            const chart = new ApexCharts(container, {
                chart: {
                    type: "area",
                    height: 120,
                    sparkline: { enabled: true },
                    animations: { enabled: false }
                },
                stroke: { curve: "smooth", width: 3 },
                fill: { opacity: 0.2 },
                colors: this.getSparklineColor(key),
                tooltip: { theme: "dark" },
                series: [{ name: "Giá trị", data: seriesData }],
                yaxis: this.resolveSparklineAxisConfig(key)
            });
            chart.render();
            this.sparklineCharts.set(key, chart);
        }

        resolveSparklineAxisConfig(key) {
            if (key === "occupancy") {
                return { min: 0, max: 100, tickAmount: 4 };
            }
            return undefined;
        }

        getSparklineColor(key) {
            switch (key) {
                case "revenue":
                    return ["#0d6efd"];
                case "occupancy":
                    return ["#10b981"];
                default:
                    return ["#6366f1"];
            }
        }

        formatStatValue(key, value) {
            const numeric = Number(value) || 0;
            if (key === "revenue") {
                return new Intl.NumberFormat("vi-VN", { style: "currency", currency: "VND", maximumFractionDigits: 0 }).format(numeric);
            }

            if (key === "occupancy") {
                const clamped = Math.max(0, Math.min(100, numeric));
                return `${clamped.toFixed(1)}%`;
            }

            return new Intl.NumberFormat("vi-VN").format(numeric);
        }

        async loadTrend() {
            const range = this.trendRangeSelect?.value || "this-week";
            try {
                this.trendLabel.textContent = "Đang tải...";
                const endpoint = `${API_BASE}/appointments-trend?${new URLSearchParams({ range })}`;
                const response = await fetchJson(endpoint);
                if (!response) {
                    this.trendLabel.textContent = "Không có dữ liệu";
                    return;
                }

                this.trendLabel.textContent = response.rangeLabel || "";
                this.trendConfirmed.textContent = response.confirmedCount ?? 0;
                this.trendCanceled.textContent = response.canceledCount ?? 0;

                const seriesData = (response.points || []).map((point) => ({
                    x: point.date,
                    y: Number(point.value) || 0
                }));
                this.renderTrendChart(seriesData);
            } catch (error) {
                console.error("Doctor dashboard trend error", error);
                this.trendLabel.textContent = "Không thể tải";
            }
        }

        renderTrendChart(seriesData) {
            const container = document.getElementById("doctorAppointmentsTrend");
            if (!container || typeof ApexCharts === "undefined") {
                return;
            }

            if (this.trendChart) {
                this.trendChart.updateSeries([{ name: "Lịch hẹn", data: seriesData }], true);
                return;
            }

            this.trendChart = new ApexCharts(container, {
                chart: {
                    type: "line",
                    height: 320,
                    toolbar: { show: false },
                    animations: { enabled: false }
                },
                stroke: { curve: "smooth", width: 3 },
                markers: { size: 4 },
                colors: ["#2563eb"],
                dataLabels: { enabled: false },
                xaxis: { type: "category" },
                yaxis: { min: 0, forceNiceScale: true },
                series: [{ name: "Lịch hẹn", data: seriesData }]
            });
            this.trendChart.render();
        }

        async loadCustomerMix() {
            const range = this.mixRangeSelect?.value || "this-week";
            try {
                this.mixLabel.textContent = "Đang tải...";
                const endpoint = `${API_BASE}/customer-mix?${new URLSearchParams({ range })}`;
                const response = await fetchJson(endpoint);
                if (!response) {
                    this.mixLabel.textContent = "Không có dữ liệu";
                    return;
                }

                this.mixLabel.textContent = response.rangeLabel || "";
                const newCustomers = response.newCustomers || 0;
                const returningCustomers = response.returningCustomers || 0;
                this.renderCustomerMixChart([newCustomers, returningCustomers]);
                this.updateCustomerMixMeta(newCustomers, returningCustomers);
            } catch (error) {
                console.error("Doctor dashboard mix error", error);
                this.mixLabel.textContent = "Không thể tải";
            }
        }

        updateCustomerMixMeta(newCustomers, returningCustomers) {
            const total = newCustomers + returningCustomers;
            const newPercent = total ? Math.round((newCustomers / total) * 100) : 0;
            const returnPercent = total ? Math.round((returningCustomers / total) * 100) : 0;

            if (this.mixNewValue) {
                this.mixNewValue.textContent = `${newCustomers} ${newPercent}%`;
            }

            if (this.mixReturnValue) {
                this.mixReturnValue.textContent = `${returningCustomers} ${returnPercent}%`;
            }
        }

        renderCustomerMixChart(data) {
            const container = document.getElementById("doctorCustomerMix");
            if (!container || typeof ApexCharts === "undefined") {
                return;
            }

            const options = {
                chart: {
                    type: "donut",
                    height: 260
                },
                labels: ["Bệnh nhân mới", "Quay lại"],
                colors: ["#6366f1", "#10b981"],
                dataLabels: { enabled: false },
                legend: { position: "bottom" },
                series: data
            };

            if (this.customerMixChart) {
                this.customerMixChart.updateOptions(options, false, true);
                return;
            }

            this.customerMixChart = new ApexCharts(container, options);
            this.customerMixChart.render();
        }

        async loadHeatmap() {
            if (!this.heatmapGrid) {
                return;
            }

            const monthValue = this.heatmapMonthInput?.value;
            if (!monthValue) {
                return;
            }

            this.heatmapGrid.innerHTML = "<p class=\"text-muted small\">Đang tải...</p>";
            try {
                const endpoint = `${API_BASE}/heatmap?${new URLSearchParams({ month: monthValue })}`;
                const response = await fetchJson(endpoint);
                if (!response || !Array.isArray(response.cells)) {
                    this.heatmapGrid.innerHTML = "<p class=\"text-muted small\">Không có dữ liệu</p>";
                    return;
                }

                this.renderHeatmapCells(response);
            } catch (error) {
                console.error("Doctor dashboard heatmap error", error);
                this.heatmapGrid.innerHTML = "<p class=\"text-muted small\">Không thể tải</p>";
            }
        }

        renderHeatmapCells(response) {
            const { year, month, cells } = response;
            const monthStart = new Date(year, month - 1, 1);
            const monthDays = new Date(year, month, 0).getDate();
            const offset = (monthStart.getDay() + 6) % 7; // Monday first
            const fragment = document.createDocumentFragment();

            for (let i = 0; i < offset; i++) {
                const emptyCell = document.createElement("div");
                emptyCell.className = "heatmap-cell level-0";
                fragment.appendChild(emptyCell);
            }

            const cellMap = new Map();
            cells.forEach((cell) => {
                cellMap.set(cell.date, cell);
            });

            for (let day = 1; day <= monthDays; day++) {
                const dateKey = `${year}-${String(month).padStart(2, "0")}-${String(day).padStart(2, "0")}`;
                const matching = cellMap.get(dateKey);
                const element = document.createElement("div");
                element.classList.add("heatmap-cell");
                if (matching) {
                    const percent = Math.max(0, Math.min(100, Number(matching.occupancyPercent) || 0));
                    element.classList.add(this.resolveHeatmapLevel(percent));
                    element.dataset.date = dateKey;
                    element.title = `${percent.toFixed(1)}%`;                
                } else {
                    element.classList.add("level-0");
                }
                fragment.appendChild(element);
            }

            this.heatmapGrid.innerHTML = "";
            this.heatmapGrid.appendChild(fragment);
        }

        resolveHeatmapLevel(percent) {
            if (percent >= 81) return "level-4";
            if (percent >= 41) return "level-3";
            if (percent >= 21) return "level-2";
            if (percent > 0) return "level-1";
            return "level-0";
        }

        async loadPerformance() {
            if (!this.performanceServices || !this.performancePatients) {
                return;
            }

            this.performanceServices.innerHTML = "<p class=\"text-muted\">Đang tải...</p>";
            this.performancePatients.innerHTML = "<p class=\"text-muted\">Đang tải...</p>";

            try {
                const range = this.performanceRangeSelect?.value || "this-week";
                const endpoint = `${API_BASE}/performance?${new URLSearchParams({ range })}`;
                const response = await fetchJson(endpoint);
                if (!response) {
                    this.performanceServices.innerHTML = "<p class=\"text-muted\">Không có dữ liệu</p>";
                    this.performancePatients.innerHTML = "<p class=\"text-muted\">Không có dữ liệu</p>";
                    return;
                }

                this.renderServicePerformance(response.services || []);
                this.renderPatientPerformance(response.patients || []);
            } catch (error) {
                console.error("Doctor dashboard performance error", error);
                this.performanceServices.innerHTML = "<p class=\"text-muted\">Không thể tải</p>";
                this.performancePatients.innerHTML = "<p class=\"text-muted\">Không thể tải</p>";
            }
        }

        renderServicePerformance(services) {
            if (!this.performanceServices) {
                return;
            }

            if (!services.length) {
                this.performanceServices.innerHTML = "<p class=\"text-muted\">Chưa có dịch vụ nào trong giai đoạn.</p>";
                return;
            }

            const fragment = document.createDocumentFragment();
            services.forEach((service) => {
                const row = document.createElement("div");
                row.className = "d-flex justify-content-between align-items-center py-2 border-bottom";
                row.innerHTML = `
                    <div class="d-flex align-items-center gap-2">
                        <span class="badge rounded-pill" style="background-color:${service.color};">&nbsp;</span>
                        <span>${service.name}</span>
                    </div>
                    <div class="text-end">
                        <div class="fw-semibold">${service.appointmentCount} ca</div>
                        <small class="text-muted">${service.occupancyPercent?.toFixed ? service.occupancyPercent.toFixed(1) : service.occupancyPercent || 0}% tổng ca</small>
                    </div>`;
                fragment.appendChild(row);
            });

            this.performanceServices.innerHTML = "";
            this.performanceServices.appendChild(fragment);
        }

        renderPatientPerformance(patients) {
            if (!this.performancePatients) {
                return;
            }

            if (!patients.length) {
                this.performancePatients.innerHTML = "<p class=\"text-muted\">Chưa có dữ liệu bệnh nhân.</p>";
                return;
            }

            const fragment = document.createDocumentFragment();
            patients.forEach((patient) => {
                const row = document.createElement("div");
                row.className = "d-flex justify-content-between align-items-center py-2 border-bottom";
                row.innerHTML = `
                    <div>
                        <div class="fw-semibold">${patient.name}</div>
                        <small class="text-muted">Lần gần nhất: ${patient.lastVisitLabel || "--"}</small>
                    </div>
                    <div class="text-end">
                        <div class="fw-semibold">${patient.appointmentCount} ca</div>
                        <small class="text-muted">${patient.phone || ""}</small>
                    </div>`;
                fragment.appendChild(row);
            });

            this.performancePatients.innerHTML = "";
            this.performancePatients.appendChild(fragment);
        }

        async loadUpcomingAppointments() {
            if (!this.upcomingList) {
                return;
            }

            const now = new Date();
            const from = now.toISOString().substring(0, 10);
            const toDate = new Date(now);
            toDate.setDate(now.getDate() + 7);
            const to = toDate.toISOString().substring(0, 10);

            this.upcomingLabel.textContent = `Trong tuần (${this.formatVietnamDate(now)} - ${this.formatVietnamDate(toDate)})`;
            this.upcomingList.innerHTML = "<p class=\"text-muted\">Đang tải...</p>";
            try {
                const endpoint = `${APPOINTMENT_API}?${new URLSearchParams({ from, to })}`;
                const response = await fetchJson(endpoint);
                if (!Array.isArray(response) || !response.length) {
                    this.upcomingList.innerHTML = "<p class=\"text-muted\">Không có lịch hẹn nào trong 7 ngày tới.</p>";
                    return;
                }

                const upcoming = response
                    .map((item) => ({
                        ...item,
                        startUtc: item.startUtc ? new Date(item.startUtc) : null
                    }))
                    .filter((item) => item.startUtc && item.startUtc >= now)
                    .sort((a, b) => a.startUtc - b.startUtc)
                    .slice(0, 6);

                if (!upcoming.length) {
                    this.upcomingList.innerHTML = "<p class=\"text-muted\">Không có lịch hẹn phù hợp.</p>";
                    return;
                }

                const fragment = document.createDocumentFragment();
                upcoming.forEach((appt) => {
                    fragment.appendChild(this.buildAppointmentRow(appt));
                });
                this.upcomingList.innerHTML = "";
                this.upcomingList.appendChild(fragment);
            } catch (error) {
                console.error("Doctor dashboard upcoming error", error);
                this.upcomingList.innerHTML = "<p class=\"text-muted\">Không thể tải danh sách.</p>";
            }
        }

        buildAppointmentRow(appt) {
            const row = document.createElement("div");
            row.className = "appointment-item";
            const toneClass = this.mapStatusTone(appt.statusTone);
            row.innerHTML = `
                <div class="service-color-bar" style="background-color:${appt.specialtyColor || "#0d6efd"}"></div>
                <div class="appointment-info">
                    <div class="date">${appt.dateLabel || ""}</div>
                    <div class="service">${appt.specialtyName || "Chuyên khoa"}</div>
                </div>
                <div class="appointment-customer">${appt.patientName || "Bệnh nhân"}</div>
                <div>
                    <span class="btn btn-status ${toneClass}">
                        <i class="fa-solid ${appt.statusIcon || "fa-clock"} me-1"></i>
                        ${appt.statusLabel || "Trạng thái"}
                    </span>
                </div>
                <div class="text-end">
                    <div class="fw-semibold">${appt.timeLabel || ""}</div>
                    <small class="text-muted">${appt.clinicRoom || ""}</small>
                </div>`;
            return row;
        }

        mapStatusTone(tone) {
            switch (tone) {
                case "approved":
                    return "btn-status-approved";
                case "canceled":
                case "rejected":
                    return "btn-status-canceled";
                case "noshow":
                    return "btn-status-noshow";
                case "pending":
                default:
                    return "btn-status-pending";
            }
        }

        getSelectValue(parent, selector) {
            const select = parent.querySelector(selector);
            return select?.value || "this-week";
        }

        formatVietnamDate(date) {
            return date.toLocaleDateString("vi-VN", { day: "2-digit", month: "2-digit" });
        }

        startClock() {
            if (!this.clockElement) {
                return;
            }

            const update = () => {
                const now = new Date();
                this.clockElement.textContent = now.toLocaleString("vi-VN");
            };

            update();
            setInterval(update, 60000);
        }
    }

    async function fetchJson(url) {
        const response = await fetch(url, {
            credentials: "include",
            headers: { Accept: "application/json" }
        });

        if (!response.ok) {
            const errorText = await safeReadBody(response);
            throw new Error(errorText || `Request failed: ${response.status}`);
        }

        if (response.status === 204) {
            return null;
        }

        const contentType = response.headers.get("content-type") || "";
        if (contentType.includes("application/json")) {
            return response.json();
        }

        return response.text();
    }

    async function safeReadBody(response) {
        try {
            return await response.text();
        } catch {
            return "";
        }
    }
})();

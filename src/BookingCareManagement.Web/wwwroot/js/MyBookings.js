(() => {
    const apiBase = "/api/customer-booking";
    const filterLabels = {
        all: "Toàn bộ lịch hẹn",
        upcoming: "Lịch sắp tới",
        past: "Lịch đã qua"
    };
    const emptyMessages = {
        all: "Bạn chưa có lịch hẹn nào. Hãy đặt ngay hôm nay!",
        upcoming: "Không có lịch sắp tới. Hãy đặt lịch mới để tiếp tục chăm sóc sức khỏe.",
        past: "Chưa có lịch đã qua để hiển thị."
    };
    const MIN_RESCHEDULE_LEAD_DAYS = 2;

    const elements = {
        dropdownButton: document.getElementById("filterDropdown"),
        dropdownMenu: document.getElementById("dropdownMenu"),
        dropdownItems: document.querySelectorAll("#dropdownMenu .dropdown-item-custom"),
        filterLabel: document.querySelector("[data-current-filter-label]"),
        list: document.getElementById("bookingsList"),
        loading: document.getElementById("bookingsLoading"),
        empty: document.getElementById("emptyState"),
        emptyText: document.querySelector("#emptyState .empty-state-text"),
        error: document.getElementById("bookingsError"),
        rescheduleModal: document.getElementById("rescheduleModal"),
        rescheduleForm: document.getElementById("rescheduleForm"),
        rescheduleDate: document.getElementById("rescheduleDate"),
        rescheduleSlot: document.getElementById("rescheduleSlot"),
        rescheduleSlotHint: document.getElementById("rescheduleSlotHint"),
        rescheduleDismissButtons: document.querySelectorAll("[data-reschedule-dismiss]")
    };

    const state = {
        filter: "all",
        bookings: new Map(),
        openMenu: null,
        rescheduleTarget: null,
        rescheduleSlots: []
    };

    const defaultEmptyMessage = elements.emptyText?.textContent ?? "";

    const toggleDropdown = () => {
        elements.dropdownMenu?.classList.toggle("show");
        elements.dropdownButton?.classList.toggle("active");
    };

    const closeDropdown = () => {
        elements.dropdownMenu?.classList.remove("show");
        elements.dropdownButton?.classList.remove("active");
    };

    const setLoading = (isLoading) => {
        if (!elements.loading) {
            return;
        }
        elements.loading.classList.toggle("d-none", !isLoading);
        if (isLoading) {
            elements.list?.classList.add("d-none");
            elements.empty?.classList.add("d-none");
        }
    };

    const showError = (message) => {
        if (!elements.error) {
            return;
        }
        if (!message) {
            elements.error.classList.add("d-none");
            elements.error.textContent = "";
            return;
        }
        elements.error.textContent = message;
        elements.error.classList.remove("d-none");
    };

    const renderEmptyState = (message = defaultEmptyMessage) => {
        if (!elements.empty) {
            return;
        }
        if (elements.emptyText) {
            elements.emptyText.textContent = message;
        }
        elements.empty.classList.remove("d-none");
        elements.list?.classList.add("d-none");
    };

    const hideEmptyState = () => {
        elements.empty?.classList.add("d-none");
    };

    const escapeHtml = (value) => {
        if (!value) {
            return "";
        }
        return value
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#39;");
    };

    const buildAvatar = (booking) => {
        if (booking.doctorAvatarUrl) {
            const src = escapeHtml(booking.doctorAvatarUrl);
            const alt = escapeHtml(booking.doctorName || "Bác sĩ");
            return `<img src="${src}" alt="${alt}">`;
        }

        const fallback = (booking.doctorName || "B").trim().charAt(0).toUpperCase() || "B";
        return `<div class="booking-doctor__fallback">${escapeHtml(fallback)}</div>`;
    };

    const buildActions = (booking) => {
        const disabled = booking.status === "canceled" || booking.status === "rejected";
        const actionAttrs = disabled ? "disabled" : "";
        return `
            <div class="booking-card__actions">
                <button class="btn-review" data-action="review" data-booking-id="${booking.id}">
                    <i class="fa-regular fa-star"></i>
                    Đánh giá
                </button>
                <div class="action-menu" data-menu>
                    <button class="action-menu__trigger" type="button" data-menu-trigger ${actionAttrs}>
                        <i class="fa-solid fa-ellipsis"></i>
                    </button>
                    <div class="action-menu__dropdown">
                        <button class="action-menu__item" data-action="reschedule" data-booking-id="${booking.id}" ${actionAttrs}>
                            <i class="fa-solid fa-clock-rotate-left"></i>
                            Đổi lịch
                        </button>
                        <button class="action-menu__item" data-action="cancel" data-booking-id="${booking.id}" ${actionAttrs}>
                            <i class="fa-solid fa-xmark"></i>
                            Hủy lịch
                        </button>
                    </div>
                </div>
            </div>`;
    };

    const renderBookingCard = (booking) => {
        const doctorName = escapeHtml(booking.doctorName || "Bác sĩ");
        const specialtyName = escapeHtml(booking.specialtyName || "Chuyên khoa");
        const specialtyColor = booking.specialtyColor ? `style="background-color: ${escapeHtml(booking.specialtyColor)};"` : "";
        const clinicRoom = escapeHtml(booking.clinicRoom || "Đang cập nhật");
        const patientName = escapeHtml(booking.patientName || "Không xác định");
        const dateText = escapeHtml(booking.dateText || "--/--/----");
        const timeText = escapeHtml(booking.timeText || "--:--");
        const statusTone = escapeHtml(booking.statusTone || "pending");
        const statusIcon = escapeHtml(booking.statusIcon || "fa-clock");
        const statusLabel = escapeHtml(booking.statusLabel || booking.status || "Đang xử lý");

        return `
            <div class="booking-card" data-status="${statusTone}" data-booking-id="${booking.id}">
                <div class="booking-card__header">
                    <div class="booking-card__header-left">
                        <div class="booking-doctor">
                            ${buildAvatar(booking)}
                            <div>
                                <p class="doctor-role">Bác sĩ phụ trách</p>
                                <p class="doctor-name">${doctorName}</p>
                                <span class="specialty-chip" ${specialtyColor}>${specialtyName}</span>
                            </div>
                        </div>
                    </div>
                    ${buildActions(booking)}
                    <span class="status-pill ${statusTone}">
                        <i class="fa-solid ${statusIcon}"></i>
                        ${statusLabel}
                    </span>
                </div>
                <div class="booking-card__body">
                    <div class="booking-meta">
                        <i class="fa-regular fa-calendar"></i>
                        <div>
                            <p class="mb-0 fw-semibold">Ngày khám</p>
                            <small>${dateText}</small>
                        </div>
                    </div>
                    <div class="booking-meta">
                        <i class="fa-regular fa-clock"></i>
                        <div>
                            <p class="mb-0 fw-semibold">Khung giờ</p>
                            <small>${timeText}</small>
                        </div>
                    </div>
                    <div class="booking-meta">
                        <i class="fa-solid fa-location-dot"></i>
                        <div>
                            <p class="mb-0 fw-semibold">Địa điểm</p>
                            <small>${clinicRoom}</small>
                        </div>
                    </div>
                    <div class="booking-meta">
                        <i class="fa-solid fa-user"></i>
                        <div>
                            <p class="mb-0 fw-semibold">Bệnh nhân</p>
                            <small>${patientName}</small>
                        </div>
                    </div>
                </div>
            </div>`;
    };

    const renderBookings = (bookings) => {
        if (!elements.list) {
            return;
        }

        state.bookings.clear();
        if (!Array.isArray(bookings) || bookings.length === 0) {
            renderEmptyState(emptyMessages[state.filter] || defaultEmptyMessage);
            return;
        }

        bookings.forEach((booking) => state.bookings.set(booking.id, booking));
        hideEmptyState();
        elements.list.innerHTML = bookings.map(renderBookingCard).join("\n");
        elements.list.classList.remove("d-none");
    };

    const setActiveFilter = (filter) => {
        state.filter = filter;
        elements.dropdownItems.forEach((item) => {
            const itemFilter = item.getAttribute("data-filter");
            item.classList.toggle("active", itemFilter === filter);
        });

        if (elements.filterLabel) {
            elements.filterLabel.textContent = filterLabels[filter] || filterLabels.all;
        }
    };

    const renderUnauthorized = () => {
        showError("Vui lòng đăng nhập để xem lịch hẹn của bạn.");
        renderEmptyState("Vui lòng đăng nhập để xem lịch hẹn của bạn.");
    };

    const loadBookings = async (filter) => {
        setLoading(true);
        showError("");
        try {
            const response = await fetch(`${apiBase}/my-bookings?filter=${filter}`, { credentials: "include" });
            if (response.status === 401) {
                renderUnauthorized();
                return;
            }

            if (!response.ok) {
                const text = await response.text();
                throw new Error(text || response.statusText);
            }

            const data = await response.json();
            renderBookings(Array.isArray(data) ? data : []);
        } catch (error) {
            console.error(error);
            showError("Không thể tải lịch hẹn. Vui lòng thử lại sau.");
            renderEmptyState("Không thể tải dữ liệu");
        } finally {
            setLoading(false);
        }
    };

    const reloadCurrentFilter = () => loadBookings(state.filter);

    const openMenu = (trigger) => {
        if (!trigger) {
            return;
        }
        const menu = trigger.closest(".action-menu");
        if (state.openMenu && state.openMenu !== menu) {
            state.openMenu.classList.remove("open");
        }
        menu?.classList.toggle("open");
        state.openMenu = menu?.classList.contains("open") ? menu : null;
    };

    const closeMenuOnOutsideClick = (event) => {
        if (!state.openMenu) {
            return;
        }
        if (!state.openMenu.contains(event.target)) {
            state.openMenu.classList.remove("open");
            state.openMenu = null;
        }
    };

    const handleReview = (bookingId) => {
        window.location.href = `/reviews?appointmentId=${bookingId}`;
    };

    const handleCancel = async (bookingId) => {
        if (!confirm("Bạn chắc chắn muốn hủy lịch này?")) {
            return;
        }

        try {
            const response = await fetch(`${apiBase}/${bookingId}/cancel`, {
                method: "POST",
                credentials: "include"
            });

            if (response.status === 401) {
                renderUnauthorized();
                return;
            }

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(errorText || "Không thể hủy lịch");
            }

            alert("Đã hủy lịch hẹn thành công.");
            await reloadCurrentFilter();
        } catch (error) {
            console.error(error);
            alert(error.message || "Không thể hủy lịch, vui lòng thử lại.");
        }
    };

    const timeRangeFormatter = new Intl.DateTimeFormat("vi-VN", { hour: "2-digit", minute: "2-digit" });

    const formatDateInputValue = (date) => {
        const year = date.getFullYear();
        const month = String(date.getMonth() + 1).padStart(2, "0");
        const day = String(date.getDate()).padStart(2, "0");
        return `${year}-${month}-${day}`;
    };

    const getMinRescheduleDate = () => {
        const minDate = new Date();
        minDate.setHours(0, 0, 0, 0);
        minDate.setDate(minDate.getDate() + MIN_RESCHEDULE_LEAD_DAYS);
        return minDate;
    };

    const getBookingDuration = (booking) => {
        if (!booking) {
            return 30;
        }
        if (booking.durationMinutes && booking.durationMinutes > 0) {
            return booking.durationMinutes;
        }
        const start = new Date(booking.startUtc);
        const end = new Date(booking.endUtc);
        const diff = Math.round((end - start) / 60000);
        return diff > 0 ? diff : 30;
    };

    const showSlotHint = (message, tone = "muted") => {
        if (!elements.rescheduleSlotHint) {
            return;
        }
        elements.rescheduleSlotHint.classList.remove("text-muted", "text-warning", "text-danger");
        if (!message) {
            elements.rescheduleSlotHint.classList.add("d-none");
            elements.rescheduleSlotHint.textContent = "";
            return;
        }

        elements.rescheduleSlotHint.classList.remove("d-none");
        const toneClass = tone === "warning" ? "text-warning" : tone === "danger" ? "text-danger" : "text-muted";
        elements.rescheduleSlotHint.classList.add(toneClass);
        elements.rescheduleSlotHint.textContent = message;
    };

    const resetRescheduleForm = () => {
        state.rescheduleSlots = [];
        if (elements.rescheduleForm) {
            elements.rescheduleForm.reset();
        }
        if (elements.rescheduleSlot) {
            elements.rescheduleSlot.innerHTML = "<option value=\"\">Vui lòng chọn ngày trước</option>";
            elements.rescheduleSlot.disabled = true;
        }
        showSlotHint("Chọn ngày để xem các khung giờ trống của bác sĩ.", "muted");
    };

    const setSlotLoadingState = (message = "Đang tải khung giờ...") => {
        if (!elements.rescheduleSlot) {
            return;
        }
        elements.rescheduleSlot.disabled = true;
        elements.rescheduleSlot.innerHTML = `<option value="">${message}</option>`;
    };

    const populateSlotOptions = (slots) => {
        if (!elements.rescheduleSlot) {
            return;
        }

        if (!Array.isArray(slots) || slots.length === 0) {
            elements.rescheduleSlot.disabled = true;
            elements.rescheduleSlot.innerHTML = "<option value=\"\">Không còn khung giờ trống, hãy chọn ngày khác</option>";
            showSlotHint("Không còn khung giờ trống cho ngày này, vui lòng chọn ngày khác.", "warning");
            return;
        }

        const options = ["<option value=\"\">-- Chọn khung giờ --</option>"];
        slots.forEach((slot) => {
            const start = new Date(slot.startLocal);
            const end = new Date(slot.endLocal);
            const label = `${timeRangeFormatter.format(start)} - ${timeRangeFormatter.format(end)}`;
            options.push(`<option value="${slot.startUtc}">${label}</option>`);
        });
        elements.rescheduleSlot.innerHTML = options.join("");
        elements.rescheduleSlot.disabled = false;
        showSlotHint(`Có ${slots.length} khung giờ trống trong ngày đã chọn.`, "muted");
    };

    const loadRescheduleSlots = async () => {
        if (!state.rescheduleTarget || !elements.rescheduleDate) {
            return;
        }

        const booking = state.bookings.get(state.rescheduleTarget);
        if (!booking) {
            return;
        }

        const dateValue = elements.rescheduleDate.value;
        if (!dateValue) {
            resetRescheduleForm();
            return;
        }

        setSlotLoadingState();
        showSlotHint("Đang kiểm tra khung giờ trống...", "muted");

        try {
            const response = await fetch(`${apiBase}/doctors/${booking.doctorId}/time-slots?date=${dateValue}`, { credentials: "include" });
            if (response.status === 401) {
                closeRescheduleModal();
                renderUnauthorized();
                return;
            }

            if (!response.ok) {
                const text = await response.text();
                throw new Error(text || "Không thể tải khung giờ");
            }

            const slots = await response.json();
            const available = Array.isArray(slots) ? slots.filter((slot) => slot.isAvailable !== false) : [];
            state.rescheduleSlots = available;
            populateSlotOptions(available);
        } catch (error) {
            console.error(error);
            state.rescheduleSlots = [];
            setSlotLoadingState("Không thể tải khung giờ");
            showSlotHint(error.message || "Không thể tải khung giờ, vui lòng thử lại.", "danger");
        }
    };

    const handleRescheduleDateChange = () => {
        if (!elements.rescheduleDate) {
            return;
        }

        if (elements.rescheduleDate.min && elements.rescheduleDate.value < elements.rescheduleDate.min) {
            elements.rescheduleDate.value = elements.rescheduleDate.min;
        }

        loadRescheduleSlots();
    };

    const openRescheduleModal = (bookingId) => {
        const booking = state.bookings.get(bookingId);
        if (!booking || !elements.rescheduleModal) {
            return;
        }

        state.rescheduleTarget = bookingId;
        const minDate = getMinRescheduleDate();
        const bookingStartLocal = new Date(booking.startUtc);
        const defaultDate = bookingStartLocal < minDate ? minDate : bookingStartLocal;

        if (elements.rescheduleDate) {
            elements.rescheduleDate.min = formatDateInputValue(minDate);
            elements.rescheduleDate.value = formatDateInputValue(defaultDate);
        }

        setSlotLoadingState();
        showSlotHint("Đang kiểm tra khung giờ trống...", "muted");

        elements.rescheduleModal.classList.remove("d-none");
        elements.rescheduleModal.setAttribute("aria-hidden", "false");
        loadRescheduleSlots();
    };

    const closeRescheduleModal = () => {
        if (!elements.rescheduleModal) {
            return;
        }
        elements.rescheduleModal.classList.add("d-none");
        elements.rescheduleModal.setAttribute("aria-hidden", "true");
        state.rescheduleTarget = null;
        resetRescheduleForm();
    };

    const submitReschedule = async (event) => {
        event.preventDefault();
        if (!state.rescheduleTarget || !elements.rescheduleSlot) {
            return;
        }

        const slotStartUtc = elements.rescheduleSlot.value;
        if (!slotStartUtc) {
            alert("Vui lòng chọn khung giờ trống.");
            return;
        }

        const booking = state.bookings.get(state.rescheduleTarget);
        const durationMinutes = getBookingDuration(booking);

        try {
            const response = await fetch(`${apiBase}/${state.rescheduleTarget}/reschedule`, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                credentials: "include",
                body: JSON.stringify({ slotStartUtc, durationMinutes })
            });

            if (response.status === 401) {
                closeRescheduleModal();
                renderUnauthorized();
                return;
            }

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(errorText || "Không thể đổi lịch");
            }

            closeRescheduleModal();
            alert("Đổi lịch thành công. Lịch sẽ trở về trạng thái chờ xác nhận.");
            await reloadCurrentFilter();
        } catch (error) {
            console.error(error);
            alert(error.message || "Không thể đổi lịch, vui lòng thử lại.");
        }
    };

    const handleAction = (action, bookingId, target) => {
        switch (action) {
            case "review":
                handleReview(bookingId);
                break;
            case "reschedule":
                openRescheduleModal(bookingId);
                break;
            case "cancel":
                handleCancel(bookingId);
                break;
            case "menu":
                openMenu(target);
                break;
            default:
                break;
        }
    };

    const attachDropdownHandlers = () => {
        if (!elements.dropdownButton || !elements.dropdownMenu) {
            return;
        }

        elements.dropdownButton.addEventListener("click", (event) => {
            event.stopPropagation();
            toggleDropdown();
        });

        elements.dropdownItems.forEach((item) => {
            item.addEventListener("click", (event) => {
                event.preventDefault();
                const targetFilter = item.getAttribute("data-filter") || "all";
                if (targetFilter === state.filter) {
                    closeDropdown();
                    return;
                }
                setActiveFilter(targetFilter);
                closeDropdown();
                loadBookings(targetFilter);
            });
        });

        document.addEventListener("click", (event) => {
            if (!elements.dropdownButton.contains(event.target) && !elements.dropdownMenu.contains(event.target)) {
                closeDropdown();
            }
        });
    };

    const attachListHandlers = () => {
        if (!elements.list) {
            return;
        }

        elements.list.addEventListener("click", (event) => {
            const menuTrigger = event.target.closest("[data-menu-trigger]");
            if (menuTrigger) {
                event.stopPropagation();
                handleAction("menu", null, menuTrigger);
                return;
            }

            const actionBtn = event.target.closest("[data-action]");
            if (!actionBtn) {
                return;
            }
            const action = actionBtn.getAttribute("data-action");
            const bookingId = actionBtn.getAttribute("data-booking-id");
            if (!bookingId || actionBtn.disabled) {
                return;
            }
            handleAction(action, bookingId, actionBtn);
        });

        document.addEventListener("click", closeMenuOnOutsideClick);
    };

    const attachRescheduleHandlers = () => {
        elements.rescheduleDismissButtons.forEach((btn) => {
            btn.addEventListener("click", closeRescheduleModal);
        });

        elements.rescheduleDate?.addEventListener("change", handleRescheduleDateChange);
        elements.rescheduleForm?.addEventListener("submit", submitReschedule);
    };

    const init = () => {
        attachDropdownHandlers();
        attachListHandlers();
        attachRescheduleHandlers();
        resetRescheduleForm();
        setActiveFilter(state.filter);
        loadBookings(state.filter);
    };

    document.addEventListener("DOMContentLoaded", init);
})();
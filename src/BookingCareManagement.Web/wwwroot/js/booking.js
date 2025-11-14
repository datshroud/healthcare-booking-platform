document.addEventListener("DOMContentLoaded", () => {
    const dom = {
        specialtyList: document.getElementById("specialty-list"),
        doctorList: document.getElementById("employee-list"),
        datetimeList: document.getElementById("datetime-list"),
        leftTitle: document.getElementById("left-title"),
        backButton: document.getElementById("btn-back"),
        searchBox: document.getElementById("search-box"),
        selectedSpecialty: document.getElementById("selected-specialty"),
        selectedDoctor: document.getElementById("selected-employee"),
        selectedDateTime: document.getElementById("selected-datetime"),
        totalSection: document.getElementById("total-section"),
        totalPrice: document.getElementById("total-price"),
        datePicker: document.getElementById("date-picker"),
        timeSlot: document.getElementById("time-slot"),
        confirmSlot: document.getElementById("confirm-datetime"),
        paymentSection: document.getElementById("payment-section"),
        thankYouSection: document.getElementById("thankyou-section"),
        checkoutTotal: document.getElementById("checkout-total"),
        promoInput: document.getElementById("promo-code"),
        applyPromoBtn: document.getElementById("apply-promo"),
        promoMessage: document.getElementById("promo-message"),
        confirmBooking: document.getElementById("confirm-booking"),
        customerName: document.getElementById("customer-name"),
        customerPhone: document.getElementById("customer-phone"),
        addToCalendarBtn: document.getElementById("add-to-calendar"),
        calendarDropdown: document.getElementById("calendarDropdown"),
        calendarSelected: document.getElementById("calendarSelected")
    };

    const state = {
        step: "specialty",
        specialties: [],
        doctors: [],
        slots: [],
        selectedSpecialty: null,
        selectedDoctor: null,
        selectedSlot: null,
        profile: null
    };

    const currency = new Intl.NumberFormat("vi-VN", { style: "currency", currency: "VND", minimumFractionDigits: 0 });
    const apiBase = "/api/customer-booking";
    const MIN_LEAD_DAYS = 2;

    const fetchJson = async (url, options) => {
        const response = await fetch(url, {
            headers: {
                "Content-Type": "application/json"
            },
            ...options
        });

        if (!response.ok) {
            const payload = await response.json().catch(() => null);
            const message = payload?.title || payload?.detail || "Đã có lỗi xảy ra";
            throw new Error(message);
        }

        if (response.status === 204) {
            return null;
        }

        return response.json();
    };

    const showLoading = (container, message = "Đang tải...") => {
        container.innerHTML = `
            <div class="text-center w-100 py-4">
                <div class="spinner-border text-success" role="status"></div>
                <p class="mt-3 text-secondary">${message}</p>
            </div>`;
    };

    const showEmpty = (container, message) => {
        container.innerHTML = `
            <div class="text-center w-100 py-5 text-secondary">
                <i class="bi bi-emoji-neutral display-6 d-block mb-3"></i>
                <p class="mb-0">${message}</p>
            </div>`;
    };

    const formatPrice = (price) => {
        if (price == null) {
            return "Liên hệ";
        }

        if (price <= 0) {
            return "Miễn phí";
        }

        return currency.format(price);
    };

    const formatDuration = (minutes) => {
        if (!minutes) {
            return "30 phút";
        }

        if (minutes % 60 === 0) {
            return `${minutes / 60} giờ`;
        }

        return `${minutes} phút`;
    };

    const getMinBookingDate = () => {
        const now = new Date();
        now.setHours(0, 0, 0, 0);
        now.setDate(now.getDate() + MIN_LEAD_DAYS);
        return now;
    };

    const formatDateInputValue = (date) => {
        const year = date.getFullYear();
        const month = String(date.getMonth() + 1).padStart(2, "0");
        const day = String(date.getDate()).padStart(2, "0");
        return `${year}-${month}-${day}`;
    };

    const setStep = (nextStep) => {
        state.step = nextStep;
        switch (nextStep) {
            case "specialty":
                dom.leftTitle.textContent = "Chọn chuyên khoa";
                dom.backButton.classList.add("d-none");
                dom.searchBox.classList.remove("d-none");
                dom.searchBox.placeholder = "Tìm kiếm chuyên khoa";
                dom.specialtyList.classList.remove("d-none");
                dom.doctorList.classList.add("d-none");
                dom.datetimeList.classList.add("d-none");
                dom.paymentSection.classList.add("d-none");
                dom.thankYouSection.classList.add("d-none");
                break;
            case "doctor":
                dom.leftTitle.textContent = "Chọn bác sĩ";
                dom.backButton.classList.remove("d-none");
                dom.searchBox.classList.remove("d-none");
                dom.searchBox.placeholder = "Tìm kiếm bác sĩ";
                dom.specialtyList.classList.add("d-none");
                dom.doctorList.classList.remove("d-none");
                dom.datetimeList.classList.add("d-none");
                dom.paymentSection.classList.add("d-none");
                dom.thankYouSection.classList.add("d-none");
                break;
            case "datetime":
                dom.leftTitle.textContent = "Chọn ngày & giờ";
                dom.backButton.classList.remove("d-none");
                dom.searchBox.classList.add("d-none");
                dom.specialtyList.classList.add("d-none");
                dom.doctorList.classList.add("d-none");
                dom.datetimeList.classList.remove("d-none");
                dom.paymentSection.classList.add("d-none");
                dom.thankYouSection.classList.add("d-none");
                break;
            case "payment":
                dom.leftTitle.textContent = "Thanh toán";
                dom.backButton.classList.remove("d-none");
                dom.specialtyList.classList.add("d-none");
                dom.doctorList.classList.add("d-none");
                dom.datetimeList.classList.add("d-none");
                dom.paymentSection.classList.remove("d-none");
                dom.thankYouSection.classList.add("d-none");
                break;
            case "thankyou":
                dom.leftTitle.textContent = "Hoàn tất đặt lịch";
                dom.backButton.classList.add("d-none");
                dom.searchBox.classList.add("d-none");
                dom.specialtyList.classList.add("d-none");
                dom.doctorList.classList.add("d-none");
                dom.datetimeList.classList.add("d-none");
                dom.paymentSection.classList.add("d-none");
                dom.thankYouSection.classList.remove("d-none");
                break;
        }
    };

    const renderSpecialties = (specialties) => {
        if (!specialties.length) {
            showEmpty(dom.specialtyList, "Hiện chưa có chuyên khoa khả dụng.");
            return;
        }

        dom.specialtyList.innerHTML = "";
        specialties.forEach((sp) => {
            const card = document.createElement("div");
            card.className = "specialty-item d-flex align-items-center justify-content-between p-3 mb-3 rounded-3 bg-body-tertiary text-light border border-secondary";
            card.dataset.search = `${sp.name} ${sp.description ?? ""}`.toLowerCase();

            card.innerHTML = `
                <div class="d-flex align-items-center">
                    <img src="${sp.imageUrl || "https://placehold.co/120x120?text=Specialty"}" class="rounded me-3" alt="specialty" width="70" height="70" />
                    <div>
                        <h5 class="mb-1 fw-semibold service-name">${sp.name}</h5>
                        <small class="text-secondary d-block">${sp.description || ""}</small>
                        <small class="text-secondary">Thời lượng <b>${formatDuration(sp.durationMinutes)}</b></small>
                    </div>
                </div>
                <div class="text-end">
                    <div class="fw-bold mb-2">${formatPrice(sp.price)}</div>
                    <button class="btn btn-success btn-sm" type="button">Chọn</button>
                </div>`;

            card.querySelector("button").addEventListener("click", () => selectSpecialty(sp));
            dom.specialtyList.appendChild(card);
        });
    };

    const renderDoctors = (doctors) => {
        if (!doctors.length) {
            showEmpty(dom.doctorList, "Chuyên khoa này chưa có bác sĩ nào.");
            return;
        }

        dom.doctorList.innerHTML = "";
        doctors.forEach((doctor) => {
            const card = document.createElement("div");
            card.className = "employee-item d-flex align-items-center justify-content-between p-3 mb-3 rounded-3 bg-body-tertiary text-light border border-secondary";
            card.dataset.search = doctor.fullName.toLowerCase();

            card.innerHTML = `
                <div class="d-flex align-items-center">
                    <img src="${doctor.avatarUrl || "https://placehold.co/96x96?text=Dr"}" class="rounded-circle me-3" alt="doctor" width="60" height="60" />
                    <div>
                        <h5 class="mb-1 fw-semibold employee-name">${doctor.fullName}</h5>
                        <small class="text-secondary">Bác sĩ</small>
                    </div>
                </div>
                <button class="btn btn-primary btn-sm" type="button">Chọn</button>`;

            card.querySelector("button").addEventListener("click", () => selectDoctor(doctor));
            dom.doctorList.appendChild(card);
        });
    };

    const renderSlots = (slots) => {
        if (!slots.length) {
            dom.timeSlot.innerHTML = `<option value="">Không còn khung giờ trống</option>`;
            dom.timeSlot.disabled = true;
            return;
        }

        dom.timeSlot.disabled = false;
        dom.timeSlot.innerHTML = `<option value="">-- Chọn giờ --</option>` +
            slots.map(slot => {
                const start = new Date(slot.startLocal);
                const end = new Date(slot.endLocal);
                const label = `${start.toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" })} - ${end.toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" })}`;
                return `<option value="${slot.startUtc}" data-label="${label}">${label}</option>`;
            }).join("");
    };

    const selectSpecialty = (specialty) => {
        state.selectedSpecialty = specialty;
        state.selectedDoctor = null;
        state.slots = [];
        state.selectedSlot = null;

        dom.selectedSpecialty.innerHTML = `
            <div class="d-flex justify-content-between align-items-start">
                <div>
                    <span class="d-block">${specialty.name}</span>
                    <small class="text-secondary">${specialty.description || ""}</small>
                </div>
                <span class="price">${formatPrice(specialty.price)}</span>
            </div>`;

        dom.totalSection.classList.remove("d-none");
        dom.totalPrice.textContent = formatPrice(specialty.price);
        dom.checkoutTotal.textContent = formatPrice(specialty.price);
        dom.promoMessage.classList.add("d-none");
        dom.promoInput.value = "";

        loadDoctors(specialty.id);
        setStep("doctor");
    };

    const selectDoctor = (doctor) => {
        state.selectedDoctor = doctor;
        state.selectedSlot = null;
        dom.selectedDoctor.innerHTML = `<div>${doctor.fullName}</div>`;

        const minIso = formatDateInputValue(getMinBookingDate());
        dom.datePicker.value = minIso;
        dom.datePicker.min = minIso;
        dom.timeSlot.innerHTML = `<option value="">Đang tải...</option>`;
        dom.timeSlot.disabled = true;
        dom.selectedDateTime.innerHTML = "";

        setStep("datetime");
        loadSlots();
    };

    const selectSlot = () => {
        const slotValue = dom.timeSlot.value;
        if (!slotValue) {
            state.selectedSlot = null;
            return;
        }

        const selectedOption = dom.timeSlot.options[dom.timeSlot.selectedIndex];
        state.selectedSlot = {
            startUtc: slotValue,
            label: selectedOption.dataset.label
        };
    };

    const confirmSlot = () => {
        if (!state.selectedSlot || !dom.datePicker.value) {
            Swal.fire({
                background: "#1e1e1e",
                color: "#f8f9fa",
                icon: "warning",
                title: "Thiếu thông tin",
                text: "Vui lòng chọn ngày và giờ khám.",
                confirmButtonColor: "#f39c12"
            });
            return;
        }

        const selectedDate = new Date(dom.datePicker.value).toLocaleDateString("vi-VN");
        dom.selectedDateTime.innerHTML = `<div>${selectedDate} - ${state.selectedSlot.label}</div>`;
        dom.checkoutTotal.textContent = dom.totalPrice.textContent;
        setStep("payment");
    };

    const submitBooking = async () => {
        const name = dom.customerName.value.trim();
        const phone = dom.customerPhone.value.trim();

        if (!name || !phone) {
            Swal.fire({
                background: "#1e1e1e",
                color: "#f8f9fa",
                icon: "warning",
                title: "Thiếu thông tin",
                text: "Vui lòng nhập họ tên và số điện thoại.",
                confirmButtonColor: "#f39c12"
            });
            return;
        }

        if (!state.selectedSpecialty || !state.selectedDoctor || !state.selectedSlot) {
            Swal.fire({
                background: "#1e1e1e",
                color: "#f8f9fa",
                icon: "error",
                title: "Chưa hoàn tất",
                text: "Bạn cần chọn chuyên khoa, bác sĩ và khung giờ trước.",
                confirmButtonColor: "#e74c3c"
            });
            return;
        }

        dom.confirmBooking.disabled = true;
        dom.confirmBooking.innerHTML = `<span class="spinner-border spinner-border-sm me-2"></span>Đang đặt lịch...`;

        try {
            const payload = {
                specialtyId: state.selectedSpecialty.id,
                doctorId: state.selectedDoctor.id,
                slotStartUtc: state.selectedSlot.startUtc,
                durationMinutes: state.selectedSpecialty.durationMinutes || 30,
                customerName: name,
                customerPhone: phone
            };

            await fetchJson(`${apiBase}`, {
                method: "POST",
                body: JSON.stringify(payload)
            });

            setStep("thankyou");
        } catch (error) {
            Swal.fire({
                background: "#1e1e1e",
                color: "#f8f9fa",
                icon: "error",
                title: "Đặt lịch thất bại",
                text: error.message,
                confirmButtonColor: "#e74c3c"
            });
        } finally {
            dom.confirmBooking.disabled = false;
            dom.confirmBooking.textContent = "Hoàn tất đặt lịch";
        }
    };

    const applyPromo = () => {
        const code = dom.promoInput.value.trim().toUpperCase();
        const rawPrice = state.selectedSpecialty?.price ?? 0;

        if (!rawPrice || rawPrice <= 0) {
            dom.promoMessage.textContent = "Chuyên khoa này chưa hỗ trợ giảm giá.";
            dom.promoMessage.classList.remove("d-none", "text-success");
            dom.promoMessage.classList.add("text-warning");
            return;
        }

        if (code === "DISCOUNT10") {
            const discounted = rawPrice * 0.9;
            dom.checkoutTotal.textContent = formatPrice(discounted);
            dom.promoMessage.textContent = "🎉 Đã áp dụng mã giảm 10%";
            dom.promoMessage.classList.remove("d-none", "text-warning");
            dom.promoMessage.classList.add("text-success");
        } else if (!code) {
            dom.promoMessage.textContent = "Vui lòng nhập mã giảm giá";
            dom.promoMessage.classList.remove("d-none", "text-success");
            dom.promoMessage.classList.add("text-warning");
        } else {
            dom.promoMessage.textContent = "❌ Mã giảm giá không hợp lệ";
            dom.promoMessage.classList.remove("d-none", "text-success");
            dom.promoMessage.classList.add("text-warning");
        }
    };

    const handleBack = () => {
        if (state.step === "doctor") {
            setStep("specialty");
            state.selectedDoctor = null;
            dom.searchBox.value = "";
            return;
        }

        if (state.step === "datetime") {
            setStep("doctor");
            dom.selectedDateTime.innerHTML = "";
            dom.searchBox.value = "";
            return;
        }

        if (state.step === "payment") {
            setStep("datetime");
            return;
        }
    };

    const handleSearch = () => {
        const keyword = dom.searchBox.value.trim().toLowerCase();
        const selector = state.step === "doctor" ? "#employee-list .employee-item" : "#specialty-list .specialty-item";
        document.querySelectorAll(selector).forEach(item => {
            const target = item.dataset.search || "";
            item.style.display = target.includes(keyword) ? "flex" : "none";
        });
    };

    const loadSpecialties = async () => {
        showLoading(dom.specialtyList);
        try {
            state.specialties = await fetchJson(`${apiBase}/specialties`);
            renderSpecialties(state.specialties);
        } catch (error) {
            showEmpty(dom.specialtyList, error.message);
        }
    };

    const loadDoctors = async (specialtyId) => {
        showLoading(dom.doctorList, "Đang tải danh sách bác sĩ...");
        try {
            state.doctors = await fetchJson(`${apiBase}/specialties/${specialtyId}/doctors`);
            renderDoctors(state.doctors);
        } catch (error) {
            showEmpty(dom.doctorList, error.message);
        }
    };

    const loadSlots = async () => {
        if (!state.selectedDoctor || !dom.datePicker.value) {
            return;
        }

        if (dom.datePicker.min && dom.datePicker.value < dom.datePicker.min) {
            dom.datePicker.value = dom.datePicker.min;
        }

        dom.timeSlot.innerHTML = `<option value="">Đang tải...</option>`;
        dom.timeSlot.disabled = true;

        try {
            state.slots = await fetchJson(`${apiBase}/doctors/${state.selectedDoctor.id}/time-slots?date=${dom.datePicker.value}`);
            renderSlots(state.slots);
        } catch (error) {
            dom.timeSlot.innerHTML = `<option value="">${error.message}</option>`;
            dom.timeSlot.disabled = true;
        }
    };

    const loadCustomerProfile = async () => {
        try {
            const response = await fetch(`${apiBase}/profile`, {
                headers: { "Content-Type": "application/json" }
            });

            if (!response.ok) {
                return;
            }

            const profile = await response.json();
            state.profile = profile;

            if (profile.fullName && !dom.customerName.value) {
                dom.customerName.value = profile.fullName;
            }

            if (profile.phoneNumber && !dom.customerPhone.value) {
                dom.customerPhone.value = profile.phoneNumber;
            }
        } catch (_error) {
            /* Silent fail to avoid interrupting guests */
        }
    };

    // Event bindings
    dom.backButton.addEventListener("click", handleBack);
    dom.searchBox.addEventListener("input", handleSearch);
    dom.datePicker.addEventListener("change", () => {
        if (dom.datePicker.value && dom.datePicker.min && dom.datePicker.value < dom.datePicker.min) {
            dom.datePicker.value = dom.datePicker.min;
        }
        state.selectedSlot = null;
        loadSlots();
    });
    dom.timeSlot.addEventListener("change", selectSlot);
    dom.confirmSlot.addEventListener("click", confirmSlot);
    dom.applyPromoBtn.addEventListener("click", applyPromo);
    dom.confirmBooking.addEventListener("click", submitBooking);

    document.querySelectorAll(".dropdown-menu .dropdown-item").forEach(item => {
        item.addEventListener("click", (evt) => {
            evt.preventDefault();
            dom.calendarSelected.innerHTML = evt.currentTarget.innerHTML;
            dom.calendarDropdown.dataset.value = evt.currentTarget.dataset.value;
        });
    });

    dom.addToCalendarBtn.addEventListener("click", () => {
        if (!dom.calendarDropdown.dataset.value) {
            Swal.fire({
                background: "#1e1e1e",
                color: "#f8f9fa",
                icon: "info",
                title: "Chưa chọn lịch",
                text: "Vui lòng chọn nền tảng lịch muốn thêm.",
                confirmButtonColor: "#3498db"
            });
        }
    });

    // Init
    setStep("specialty");
    loadSpecialties();
    loadCustomerProfile();
});

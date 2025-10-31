document.addEventListener("DOMContentLoaded", function () {
    const chooseButtons = document.querySelectorAll(".choose-service");
    const serviceList = document.getElementById("service-list");
    const employeeList = document.getElementById("employee-list");
    const datetimeList = document.getElementById("datetime-list");
    const leftTitle = document.getElementById("left-title");
    const backButton = document.getElementById("btn-back");
    const searchBox = document.getElementById("search-box");

    const selectedService = document.getElementById("selected-service");
    const selectedEmployee = document.getElementById("selected-employee");
    const selectedDateTime = document.getElementById("selected-datetime");
    const totalPriceSection = document.getElementById("total-section");
    const totalPrice = document.getElementById("total-price");

    const datePicker = document.getElementById("date-picker");
    const timeSlot = document.getElementById("time-slot");
    const confirmBtn = document.getElementById("confirm-datetime");

    const promoInput = document.getElementById("promo-code");
    const applyPromoBtn = document.getElementById("apply-promo");
    const promoMessage = document.getElementById("promo-message");
    const checkoutTotal = document.getElementById("checkout-total");
    const confirmBooking = document.getElementById("confirm-booking");

    applyPromoBtn.addEventListener("click", function () {
        const code = promoInput.value.trim().toUpperCase();
        const originalPrice = parseFloat(totalPrice.textContent.replace("$", ""));

        if (code === "DISCOUNT10") {
            const discounted = (originalPrice * 0.9).toFixed(2);
            checkoutTotal.textContent = `$${discounted}`;
            promoMessage.textContent = "🎉 Mã giảm giá đã được áp dụng (-10%)!";
            promoMessage.classList.remove("d-none", "text-warning");
            promoMessage.classList.add("text-success");
        } else if (code === "") {
            promoMessage.textContent = "Vui lòng nhập mã giảm giá!";
            promoMessage.classList.remove("d-none", "text-success");
            promoMessage.classList.add("text-warning");
        } else {
            promoMessage.textContent = "❌ Mã giảm giá không hợp lệ!";
            promoMessage.classList.remove("d-none", "text-success");
            promoMessage.classList.add("text-warning");
        }
    });


    let currentStep = "service";
    let selectedServiceInfo = null;
    let selectedEmployeeInfo = null;

    // Giả lập giờ làm việc
    const availableHours = ["08:00", "09:00", "10:00", "11:00", "13:00", "14:00", "15:00", "16:00"];

    // Giả lập giờ đã được đặt (sau này sẽ từ DB)
    const bookedSlots = {
        "Nguyễn Văn An": ["09:00", "14:00"], // nhân viên này đã bận 9h và 14h
        "Trần Thị Bình": ["10:00", "15:00"]
    };

    // === B1: CHỌN DỊCH VỤ ===
    chooseButtons.forEach(btn => {
        btn.addEventListener("click", function () {
            const name = this.dataset.name;
            const duration = this.dataset.duration;
            const price = parseFloat(this.dataset.price).toFixed(2);

            selectedServiceInfo = { name, duration, price };

            selectedService.innerHTML = `
                <div>
                    <span>${name} <small class="text-secondary">(${duration})</small></span>
                    <span class="price">$${price}</span>
                </div>
            `;


            totalPrice.textContent = `$${price}`;
            totalPriceSection.classList.remove("d-none");

            serviceList.classList.add("d-none");
            employeeList.classList.remove("d-none");
            leftTitle.textContent = "Chọn nhân viên";
            backButton.classList.remove("d-none");
            searchBox.placeholder = "Tìm kiếm nhân viên";
            searchBox.value = "";
            currentStep = "employee";
        });
    });

    // === B2: CHỌN NHÂN VIÊN ===
    document.querySelectorAll(".employee-item button").forEach(btn => {
        btn.addEventListener("click", function () {
            const employeeName = this.closest(".employee-item").querySelector(".employee-name").textContent;
            selectedEmployeeInfo = { name: employeeName };

            selectedEmployee.innerHTML = `<div>${employeeName}</div>`;

            employeeList.classList.add("d-none");
            datetimeList.classList.remove("d-none");
            leftTitle.textContent = "Chọn ngày & giờ";
            searchBox.classList.add("d-none");
            currentStep = "datetime";

            // Giới hạn ngày chỉ được chọn từ hôm nay
            const today = new Date().toISOString().split("T")[0];
            datePicker.min = today;
            datePicker.value = "";
            timeSlot.innerHTML = `<option value="">-- Vui lòng chọn ngày trước --</option>`;
        });
    });

    // === B3: CHỌN NGÀY ===
    datePicker.addEventListener("change", function () {
        const date = this.value;
        if (!date || !selectedEmployeeInfo) return;

        const empName = selectedEmployeeInfo.name;
        const booked = bookedSlots[empName] || [];

        // Lọc ra giờ trống
        const freeHours = availableHours.filter(h => !booked.includes(h));

        timeSlot.innerHTML = freeHours.map(h => `<option value="${h}">${h}</option>`).join("");
    });

    // === B4: XÁC NHẬN NGÀY GIỜ ===
    confirmBtn.addEventListener("click", function () {
        const date = datePicker.value;
        const time = timeSlot.value;
        if (!date || !time) {
            alert("Vui lòng chọn ngày và giờ!");
            return;
        }

        const displayDate = new Date(date).toLocaleDateString("vi-VN");
        selectedDateTime.innerHTML = `<div>${displayDate} - ${time}</div>`;

        // === B5: CHUYỂN SANG THANH TOÁN ===
        datetimeList.classList.add("d-none");
        document.getElementById("payment-section").classList.remove("d-none");
        leftTitle.textContent = "Thanh toán";
        backButton.classList.remove("d-none");
        currentStep = "payment";

        document.getElementById("checkout-total").textContent = totalPrice.textContent;
    });

    // === NÚT QUAY LẠI ===
    backButton.addEventListener("click", function () {
        if (currentStep === "datetime") {
            datetimeList.classList.add("d-none");
            employeeList.classList.remove("d-none");
            leftTitle.textContent = "Chọn nhân viên";
            currentStep = "employee";
            selectedDateTime.innerHTML = "";
            searchBox.classList.remove("d-none");
            searchBox.placeholder = "Tìm kiếm nhân viên";
            return;
        }

        if (currentStep === "employee") {
            employeeList.classList.add("d-none");
            serviceList.classList.remove("d-none");
            leftTitle.textContent = "Chọn dịch vụ";
            currentStep = "service";
            backButton.classList.add("d-none");
            selectedService.innerHTML = "";
            selectedEmployee.innerHTML = "";
            selectedDateTime.innerHTML = "";
            totalPriceSection.classList.add("d-none");
            searchBox.placeholder = "Tìm kiếm dịch vụ";
            searchBox.value = "";
            return;
        }
        if (currentStep === "payment") {
            document.getElementById("payment-section").classList.add("d-none");
            datetimeList.classList.remove("d-none");
            leftTitle.textContent = "Chọn ngày & giờ";
            currentStep = "datetime";
            return;
        }
    });

    // === TÌM KIẾM ===
    searchBox.addEventListener("input", function () {
        const keyword = this.value.toLowerCase().trim();

        if (currentStep === "employee") {
            document.querySelectorAll("#employee-list .employee-item").forEach(item => {
                const name = item.querySelector(".employee-name").textContent.toLowerCase();
                item.style.display = name.includes(keyword) ? "flex" : "none";
            });
        } else if (currentStep === "service") {
            document.querySelectorAll("#service-list .service-item").forEach(item => {
                const name = item.querySelector(".service-name").textContent.toLowerCase();
                item.style.display = name.includes(keyword) ? "flex" : "none";
            });
        }
    });
    confirmBooking.addEventListener("click", function () {
        const name = document.getElementById("customer-name").value.trim();
        const phone = document.getElementById("customer-phone").value.trim();

        if (!name || !phone) {
            Swal.fire({
                background: "#1e1e1e",
                color: "#f8f9fa",
                icon: "warning",
                title: "Thiếu thông tin!",
                text: "Vui lòng nhập đầy đủ họ tên và số điện thoại.",
                confirmButtonColor: "#f39c12"
            });
            return;
        }

        // Ẩn phần thanh toán, hiện phần cảm ơn
        document.getElementById("payment-section").classList.add("d-none");
        document.getElementById("thankyou-section").classList.remove("d-none");

        // Cập nhật tiêu đề khung trái
        leftTitle.textContent = "Hoàn tất đặt lịch";
        backButton.classList.add("d-none");
        searchBox.classList.add("d-none");

        // Reset thông tin sau khi hiển thị trang cảm ơn
        currentStep = "thankyou";
    });
    // 🌙 Custom dropdown logic
    document.querySelectorAll('.dropdown-menu .dropdown-item').forEach(item => {
        item.addEventListener('click', e => {
            e.preventDefault();
            const selected = e.currentTarget;
            document.getElementById('calendarSelected').innerHTML = selected.innerHTML;
            document.getElementById('calendarDropdown').dataset.value = selected.dataset.value;
        });
    });

    // 📅 Xử lý nút Add to Calendar
    document.getElementById("add-to-calendar").addEventListener("click", () => {
        const selectedCalendar = document.getElementById("calendarDropdown").dataset.value;

        if (!selectedCalendar) {
            alert("Please select a calendar to add your booking.");
            return;
        }

        // Tạm thời chỉ hiển thị thông báo (sau này sẽ mở link API tương ứng)
        console.log(`Added booking to: ${selectedCalendar}`);
    });


});



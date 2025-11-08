/* File: wwwroot/js/appointment.js
  LƯU Ý: Đã xóa "DOMContentLoaded" wrapper
*/

// --- 1. XỬ LÝ TOGGLE BỘ LỌC (FILTER) ---
const toggleBtn = document.getElementById("toggleFiltersButton");
const filtersContainer = document.getElementById("filtersContainer");

if (toggleBtn && filtersContainer) {
    toggleBtn.addEventListener("click", function () {

        // Lấy trạng thái hiển thị hiện tại
        const isHidden = window.getComputedStyle(filtersContainer).display === "none";

        // Bật/tắt khu vực filter
        if (isHidden) {
            filtersContainer.style.display = "block"; // Hiển thị
            toggleBtn.classList.add("active"); // Thêm class 'active' để đổi màu nút
        } else {
            filtersContainer.style.display = "none"; // Ẩn đi
            toggleBtn.classList.remove("active"); // Bỏ class 'active'
        }
    });
} else {
    // Thêm dòng này để kiểm tra
    console.warn("Không tìm thấy nút 'toggleFiltersButton' hoặc 'filtersContainer'.");
}

// --- 2. XỬ LÝ DATE RANGE PICKER (BỘ CHỌN NGÀY) ---
// Kiểm tra xem thư viện flatpickr đã được tải chưa
if (typeof flatpickr !== 'undefined') {

    // Khởi tạo cho ô "Start Date"
    const fpStart = flatpickr("#startDate", {
        dateFormat: "M j, Y", // Định dạng: Nov 9, 2025
        onChange: function (selectedDates, dateStr, instance) {
            if (fpEnd) {
                fpEnd.set('minDate', selectedDates[0]);
            }
        }
    });

    // Khởi tạo cho ô "End Date"
    const fpEnd = flatpickr("#endDate", {
        dateFormat: "M j, Y",
        onChange: function (selectedDates, dateStr, instance) {
            if (fpStart) {
                fpStart.set('maxDate', selectedDates[0]);
            }
        }
    });

} else {
    // Báo lỗi nếu quên chèn thư viện
    console.error("Thư viện Flatpickr CHƯA ĐƯỢC TẢI. Bộ lọc ngày sẽ không hoạt động.");
}

// --- 3. XỬ LÝ TÌM KIẾM BÊN TRONG DROPDOWN ---
document.querySelectorAll('.filter-dropdown-menu input[type="text"]').forEach(searchBox => {
    searchBox.addEventListener('keyup', function (e) {
        const searchTerm = e.target.value.toLowerCase();
        const listItems = e.target.closest('.filter-dropdown-menu').querySelectorAll('.filter-options-list li');

        listItems.forEach(item => {
            const label = item.querySelector('label');
            if (label) {
                const text = label.textContent.toLowerCase();
                if (text.includes(searchTerm)) {
                    item.style.display = '';
                } else {
                    item.style.display = 'none';
                }
            }
        });
    });
});/* File: wwwroot/js/appointment.js
  LƯU Ý: Đã xóa "DOMContentLoaded" wrapper
*/

// --- 1. XỬ LÝ TOGGLE BỘ LỌC (FILTER) ---
const toggleBtn = document.getElementById("toggleFiltersButton");
const filtersContainer = document.getElementById("filtersContainer");

if (toggleBtn && filtersContainer) {
    toggleBtn.addEventListener("click", function () {

        // Lấy trạng thái hiển thị hiện tại
        const isHidden = window.getComputedStyle(filtersContainer).display === "none";

        // Bật/tắt khu vực filter
        if (isHidden) {
            filtersContainer.style.display = "block"; // Hiển thị
            toggleBtn.classList.add("active"); // Thêm class 'active' để đổi màu nút
        } else {
            filtersContainer.style.display = "none"; // Ẩn đi
            toggleBtn.classList.remove("active"); // Bỏ class 'active'
        }
    });
} else {
    // Thêm dòng này để kiểm tra
    console.warn("Không tìm thấy nút 'toggleFiltersButton' hoặc 'filtersContainer'.");
}

// --- 2. XỬ LÝ DATE RANGE PICKER (BỘ CHỌN NGÀY) ---
// Kiểm tra xem thư viện flatpickr đã được tải chưa
if (typeof flatpickr !== 'undefined') {

    // Khởi tạo cho ô "Start Date"
    const fpStart = flatpickr("#startDate", {
        dateFormat: "M j, Y", // Định dạng: Nov 9, 2025
        onChange: function (selectedDates, dateStr, instance) {
            if (fpEnd) {
                fpEnd.set('minDate', selectedDates[0]);
            }
        }
    });

    // Khởi tạo cho ô "End Date"
    const fpEnd = flatpickr("#endDate", {
        dateFormat: "M j, Y",
        onChange: function (selectedDates, dateStr, instance) {
            if (fpStart) {
                fpStart.set('maxDate', selectedDates[0]);
            }
        }
    });

} else {
    // Báo lỗi nếu quên chèn thư viện
    console.error("Thư viện Flatpickr CHƯA ĐƯỢC TẢI. Bộ lọc ngày sẽ không hoạt động.");
}

// --- 3. XỬ LÝ TÌM KIẾM BÊN TRONG DROPDOWN ---
document.querySelectorAll('.filter-dropdown-menu input[type="text"]').forEach(searchBox => {
    searchBox.addEventListener('keyup', function (e) {
        const searchTerm = e.target.value.toLowerCase();
        const listItems = e.target.closest('.filter-dropdown-menu').querySelectorAll('.filter-options-list li');

        listItems.forEach(item => {
            const label = item.querySelector('label');
            if (label) {
                const text = label.textContent.toLowerCase();
                if (text.includes(searchTerm)) {
                    item.style.display = '';
                } else {
                    item.style.display = 'none';
                }
            }
        });
    });
});/* File: wwwroot/js/appointment.js
  LƯU Ý: Đã xóa "DOMContentLoaded" wrapper
*/

// --- 1. XỬ LÝ TOGGLE BỘ LỌC (FILTER) ---
const toggleBtn = document.getElementById("toggleFiltersButton");
const filtersContainer = document.getElementById("filtersContainer");

if (toggleBtn && filtersContainer) {
    toggleBtn.addEventListener("click", function () {

        // Lấy trạng thái hiển thị hiện tại
        const isHidden = window.getComputedStyle(filtersContainer).display === "none";

        // Bật/tắt khu vực filter
        if (isHidden) {
            filtersContainer.style.display = "block"; // Hiển thị
            toggleBtn.classList.add("active"); // Thêm class 'active' để đổi màu nút
        } else {
            filtersContainer.style.display = "none"; // Ẩn đi
            toggleBtn.classList.remove("active"); // Bỏ class 'active'
        }
    });
} else {
    // Thêm dòng này để kiểm tra
    console.warn("Không tìm thấy nút 'toggleFiltersButton' hoặc 'filtersContainer'.");
}

// --- 2. XỬ LÝ DATE RANGE PICKER (BỘ CHỌN NGÀY) ---
// Kiểm tra xem thư viện flatpickr đã được tải chưa
if (typeof flatpickr !== 'undefined') {

    // Khởi tạo cho ô "Start Date"
    const fpStart = flatpickr("#startDate", {
        dateFormat: "M j, Y", // Định dạng: Nov 9, 2025
        onChange: function (selectedDates, dateStr, instance) {
            if (fpEnd) {
                fpEnd.set('minDate', selectedDates[0]);
            }
        }
    });

    // Khởi tạo cho ô "End Date"
    const fpEnd = flatpickr("#endDate", {
        dateFormat: "M j, Y",
        onChange: function (selectedDates, dateStr, instance) {
            if (fpStart) {
                fpStart.set('maxDate', selectedDates[0]);
            }
        }
    });

} else {
    // Báo lỗi nếu quên chèn thư viện
    console.error("Thư viện Flatpickr CHƯA ĐƯỢC TẢI. Bộ lọc ngày sẽ không hoạt động.");
}

// --- 3. XỬ LÝ TÌM KIẾM BÊN TRONG DROPDOWN ---
document.querySelectorAll('.filter-dropdown-menu input[type="text"]').forEach(searchBox => {
    searchBox.addEventListener('keyup', function (e) {
        const searchTerm = e.target.value.toLowerCase();
        const listItems = e.target.closest('.filter-dropdown-menu').querySelectorAll('.filter-options-list li');

        listItems.forEach(item => {
            const label = item.querySelector('label');
            if (label) {
                const text = label.textContent.toLowerCase();
                if (text.includes(searchTerm)) {
                    item.style.display = '';
                } else {
                    item.style.display = 'none';
                }
            }
        });
    });
});/* File: wwwroot/js/appointment.js
  LƯU Ý: Đã xóa "DOMContentLoaded" wrapper
*/

// --- 1. XỬ LÝ TOGGLE BỘ LỌC (FILTER) ---
const toggleBtn = document.getElementById("toggleFiltersButton");
const filtersContainer = document.getElementById("filtersContainer");

if (toggleBtn && filtersContainer) {
    toggleBtn.addEventListener("click", function () {

        // Lấy trạng thái hiển thị hiện tại
        const isHidden = window.getComputedStyle(filtersContainer).display === "none";

        // Bật/tắt khu vực filter
        if (isHidden) {
            filtersContainer.style.display = "block"; // Hiển thị
            toggleBtn.classList.add("active"); // Thêm class 'active' để đổi màu nút
        } else {
            filtersContainer.style.display = "none"; // Ẩn đi
            toggleBtn.classList.remove("active"); // Bỏ class 'active'
        }
    });
} else {
    // Thêm dòng này để kiểm tra
    console.warn("Không tìm thấy nút 'toggleFiltersButton' hoặc 'filtersContainer'.");
}

// --- 2. XỬ LÝ DATE RANGE PICKER (BỘ CHỌN NGÀY) ---
// Kiểm tra xem thư viện flatpickr đã được tải chưa
if (typeof flatpickr !== 'undefined') {

    // Khởi tạo cho ô "Start Date"
    const fpStart = flatpickr("#startDate", {
        dateFormat: "M j, Y", // Định dạng: Nov 9, 2025
        onChange: function (selectedDates, dateStr, instance) {
            if (fpEnd) {
                fpEnd.set('minDate', selectedDates[0]);
            }
        }
    });

    // Khởi tạo cho ô "End Date"
    const fpEnd = flatpickr("#endDate", {
        dateFormat: "M j, Y",
        onChange: function (selectedDates, dateStr, instance) {
            if (fpStart) {
                fpStart.set('maxDate', selectedDates[0]);
            }
        }
    });

} else {
    // Báo lỗi nếu quên chèn thư viện
    console.error("Thư viện Flatpickr CHƯA ĐƯỢC TẢI. Bộ lọc ngày sẽ không hoạt động.");
}

// --- 3. XỬ LÝ TÌM KIẾM BÊN TRONG DROPDOWN ---
document.querySelectorAll('.filter-dropdown-menu input[type="text"]').forEach(searchBox => {
    searchBox.addEventListener('keyup', function (e) {
        const searchTerm = e.target.value.toLowerCase();
        const listItems = e.target.closest('.filter-dropdown-menu').querySelectorAll('.filter-options-list li');

        listItems.forEach(item => {
            const label = item.querySelector('label');
            if (label) {
                const text = label.textContent.toLowerCase();
                if (text.includes(searchTerm)) {
                    item.style.display = '';
                } else {
                    item.style.display = 'none';
                }
            }
        });
    });
});/* File: wwwroot/js/appointment.js
  LƯU Ý: Đã xóa "DOMContentLoaded" wrapper
*/

// --- 1. XỬ LÝ TOGGLE BỘ LỌC (FILTER) ---
const toggleBtn = document.getElementById("toggleFiltersButton");
const filtersContainer = document.getElementById("filtersContainer");

if (toggleBtn && filtersContainer) {
    toggleBtn.addEventListener("click", function () {

        // Lấy trạng thái hiển thị hiện tại
        const isHidden = window.getComputedStyle(filtersContainer).display === "none";

        // Bật/tắt khu vực filter
        if (isHidden) {
            filtersContainer.style.display = "block"; // Hiển thị
            toggleBtn.classList.add("active"); // Thêm class 'active' để đổi màu nút
        } else {
            filtersContainer.style.display = "none"; // Ẩn đi
            toggleBtn.classList.remove("active"); // Bỏ class 'active'
        }
    });
} else {
    // Thêm dòng này để kiểm tra
    console.warn("Không tìm thấy nút 'toggleFiltersButton' hoặc 'filtersContainer'.");
}

// --- 2. XỬ LÝ DATE RANGE PICKER (BỘ CHỌN NGÀY) ---
// Kiểm tra xem thư viện flatpickr đã được tải chưa
if (typeof flatpickr !== 'undefined') {

    // Khởi tạo cho ô "Start Date"
    const fpStart = flatpickr("#startDate", {
        dateFormat: "M j, Y", // Định dạng: Nov 9, 2025
        onChange: function (selectedDates, dateStr, instance) {
            if (fpEnd) {
                fpEnd.set('minDate', selectedDates[0]);
            }
        }
    });

    // Khởi tạo cho ô "End Date"
    const fpEnd = flatpickr("#endDate", {
        dateFormat: "M j, Y",
        onChange: function (selectedDates, dateStr, instance) {
            if (fpStart) {
                fpStart.set('maxDate', selectedDates[0]);
            }
        }
    });

} else {
    // Báo lỗi nếu quên chèn thư viện
    console.error("Thư viện Flatpickr CHƯA ĐƯỢC TẢI. Bộ lọc ngày sẽ không hoạt động.");
}

// --- 3. XỬ LÝ TÌM KIẾM BÊN TRONG DROPDOWN ---
document.querySelectorAll('.filter-dropdown-menu input[type="text"]').forEach(searchBox => {
    searchBox.addEventListener('keyup', function (e) {
        const searchTerm = e.target.value.toLowerCase();
        const listItems = e.target.closest('.filter-dropdown-menu').querySelectorAll('.filter-options-list li');

        listItems.forEach(item => {
            const label = item.querySelector('label');
            if (label) {
                const text = label.textContent.toLowerCase();
                if (text.includes(searchTerm)) {
                    item.style.display = '';
                } else {
                    item.style.display = 'none';
                }
            }
        });
    });
});/* File: wwwroot/js/appointment.js
  LƯU Ý: Đã xóa "DOMContentLoaded" wrapper
*/

// --- 1. XỬ LÝ TOGGLE BỘ LỌC (FILTER) ---
const toggleBtn = document.getElementById("toggleFiltersButton");
const filtersContainer = document.getElementById("filtersContainer");

if (toggleBtn && filtersContainer) {
    toggleBtn.addEventListener("click", function () {

        // Lấy trạng thái hiển thị hiện tại
        const isHidden = window.getComputedStyle(filtersContainer).display === "none";

        // Bật/tắt khu vực filter
        if (isHidden) {
            filtersContainer.style.display = "block"; // Hiển thị
            toggleBtn.classList.add("active"); // Thêm class 'active' để đổi màu nút
        } else {
            filtersContainer.style.display = "none"; // Ẩn đi
            toggleBtn.classList.remove("active"); // Bỏ class 'active'
        }
    });
} else {
    // Thêm dòng này để kiểm tra
    console.warn("Không tìm thấy nút 'toggleFiltersButton' hoặc 'filtersContainer'.");
}

// --- 2. XỬ LÝ DATE RANGE PICKER (BỘ CHỌN NGÀY) ---
// Kiểm tra xem thư viện flatpickr đã được tải chưa
if (typeof flatpickr !== 'undefined') {

    // Khởi tạo cho ô "Start Date"
    const fpStart = flatpickr("#startDate", {
        dateFormat: "M j, Y", // Định dạng: Nov 9, 2025
        onChange: function (selectedDates, dateStr, instance) {
            if (fpEnd) {
                fpEnd.set('minDate', selectedDates[0]);
            }
        }
    });

    // Khởi tạo cho ô "End Date"
    const fpEnd = flatpickr("#endDate", {
        dateFormat: "M j, Y",
        onChange: function (selectedDates, dateStr, instance) {
            if (fpStart) {
                fpStart.set('maxDate', selectedDates[0]);
            }
        }
    });

} else {
    // Báo lỗi nếu quên chèn thư viện
    console.error("Thư viện Flatpickr CHƯA ĐƯỢC TẢI. Bộ lọc ngày sẽ không hoạt động.");
}

// --- 3. XỬ LÝ TÌM KIẾM BÊN TRONG DROPDOWN ---
document.querySelectorAll('.filter-dropdown-menu input[type="text"]').forEach(searchBox => {
    searchBox.addEventListener('keyup', function (e) {
        const searchTerm = e.target.value.toLowerCase();
        const listItems = e.target.closest('.filter-dropdown-menu').querySelectorAll('.filter-options-list li');

        listItems.forEach(item => {
            const label = item.querySelector('label');
            if (label) {
                const text = label.textContent.toLowerCase();
                if (text.includes(searchTerm)) {
                    item.style.display = '';
                } else {
                    item.style.display = 'none';
                }
            }
        });
    });
});
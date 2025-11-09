function toggleDropdown(btn) {
    const dropdown = btn.nextElementSibling;
    const allDropdowns = document.querySelectorAll('.dropdown-menu-custom');

    allDropdowns.forEach(d => {
        if (d !== dropdown) {
            d.classList.remove('show');
        }
    });

    dropdown.classList.toggle('show');
}

// Close dropdown when clicking outside
document.addEventListener('click', function (e) {
    if (!e.target.closest('.action-menu')) {
        document.querySelectorAll('.dropdown-menu-custom').forEach(d => {
            d.classList.remove('show');
        });
    }
});

// Select all checkbox
document.getElementById('selectAll').addEventListener('change', function () {
    const checkboxes = document.querySelectorAll('tbody input[type="checkbox"]');
    checkboxes.forEach(cb => cb.checked = this.checked);
});

// Search functionality
document.getElementById('searchInput').addEventListener('input', function (e) {
    const searchTerm = e.target.value.toLowerCase();
    const rows = document.querySelectorAll('tbody tr');

    rows.forEach(row => {
        const text = row.textContent.toLowerCase();
        row.style.display = text.includes(searchTerm) ? '' : 'none';
    });
});

// ... Ngay bên dưới các script cũ của bạn ...

// === BẮT ĐẦU: MÃ XEM TRƯỚC ẢNH ===

// 1. Tìm các phần tử HTML chúng ta cần
const fileInput = document.getElementById('fileUploadInput'); // Input ẩn
const uploadArea = document.querySelector('.file-upload-area'); // Khu vực upload
const addEmployeeModal = document.getElementById('addEmployeeModal'); // Toàn bộ modal

// 2. Lưu lại nội dung HTML gốc của khu vực upload
const originalUploadAreaContent = uploadArea.innerHTML;

// 3. Thêm một "tai nghe" vào input ẩn
// Nó sẽ kích hoạt mỗi khi người dùng CHỌN XONG một tệp
fileInput.addEventListener('change', function (event) {
    // Lấy tệp người dùng đã chọn (chỉ lấy tệp đầu tiên)
    const file = event.target.files[0];

    // 4. Kiểm tra xem có tệp không VÀ có phải là ảnh không
    if (file && file.type.startsWith('image/')) {
        // 5. Tạo một công cụ "Đọc Tệp"
        const reader = new FileReader();

        // 6. Ra lệnh cho công cụ: "Sau khi đọc xong, hãy làm điều này:"
        reader.onload = function (e) {
            // e.target.result chính là nội dung tệp đã được mã hóa
            // mà thẻ <img> có thể đọc được
            uploadArea.innerHTML = `
                <img src="${e.target.result}" alt="Ảnh xem trước" class="image-preview">
            `;
            uploadArea.classList.add('has-image');
        }

        // 7. Bắt đầu đọc tệp
        reader.readAsDataURL(file);

    } else if (file) {
        // Nếu chọn tệp nhưng không phải ảnh
        alert('Vui lòng chỉ chọn tệp hình ảnh.');
        fileInput.value = null; // Xóa tệp đã chọn
    }
});


// 8. (Rất quan trọng) Reset lại khu vực upload khi modal bị đóng
// Nếu không, lần sau mở modal, bạn sẽ vẫn thấy ảnh cũ
addEmployeeModal.addEventListener('hidden.bs.modal', function () {
    // Đặt lại nội dung khu vực upload về ban đầu
    uploadArea.innerHTML = originalUploadAreaContent;

    // Xóa tệp đã chọn trong input ẩn
    fileInput.value = null;

    // (Tùy chọn) Reset cả form nếu bạn muốn
    // addEmployeeModal.querySelector('form').reset();
    uploadArea.classList.remove('has-image');
});

// Kích hoạt Select2 cho ô chọn dịch vụ
$(document).ready(function () {
    $('#service').select2({
        placeholder: "Chọn dịch vụ...",
        allowClear: true,
        width: '100%',
        dropdownParent: $('#addEmployeeModal') // rất quan trọng để dropdown hoạt động trong modal Bootstrap
    });
});
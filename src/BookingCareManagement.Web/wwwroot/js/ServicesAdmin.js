// Toggle service dropdown
function toggleServiceDropdown(btn) {
    const dropdown = btn.nextElementSibling;
    const allDropdowns = document.querySelectorAll('.dropdown-menu-service');

    allDropdowns.forEach(d => {
        if (d !== dropdown) {
            d.classList.remove('show');
        }
    });

    if (dropdown) {
        dropdown.classList.toggle('show');
    }
}

// Close dropdown when clicking outside
document.addEventListener('click', function (e) {
    // Đóng menu DỊCH VỤ nếu click ra ngoài
    if (!e.target.closest('.btn-more-service')) {
        document.querySelectorAll('.dropdown-menu-service').forEach(d => {
            d.classList.remove('show');
        });
    }

    // Đóng menu DANH MỤC nếu click ra ngoài
    if (!e.target.closest('.btn-more-category')) {
        document.querySelectorAll('.dropdown-menu-category').forEach(d => {
            d.classList.remove('show');
        });
    }
});

// Tab switching
document.querySelectorAll('.tab-item').forEach(tab => {
    tab.addEventListener('click', function () {
        document.querySelectorAll('.tab-item').forEach(t => t.classList.remove('active'));
        this.classList.add('active');
    });
});

// --- BỔ SUNG JS ĐỂ CẬP NHẬT TEXT SORT DROPDOWN ---
const sortMenu = document.getElementById('sort-options-menu');
const sortLabel = document.getElementById('current-sort-label');

if (sortMenu && sortLabel) {
    sortMenu.addEventListener('click', function (e) {
        if (e.target.tagName === 'A') {
            e.preventDefault();
            const selectedText = e.target.textContent;
            sortLabel.textContent = selectedText;
            sortMenu.querySelectorAll('.dropdown-item').forEach(item => {
                item.classList.remove('active');
            });
            e.target.classList.add('active');
        }
    });
}

// --- BỔ SUNG JS ĐỂ KHỞI TẠO CKEDITOR ---
let serviceEditorInstance;
const addServiceModal = document.getElementById('addServiceModal');

// === THÊM VÀO: CÁC BIẾN CHO UPLOAD ẢNH DỊCH VỤ ===
const serviceInput = document.getElementById('serviceImageInput');
const serviceUploadBox = document.getElementById('serviceUploadBox');
// Phải kiểm tra serviceUploadBox tồn tại không, nếu không sẽ lỗi
const originalServiceUploadHTML = serviceUploadBox ? serviceUploadBox.innerHTML : '';
// === KẾT THÚC THÊM BIẾN ===


if (addServiceModal) {
    // Sự kiện "shown.bs.modal" bắn ra KHI modal đã hiển thị xong
    addServiceModal.addEventListener('shown.bs.modal', () => {
        // (Code cũ của bạn) Chỉ khởi tạo nếu chưa có
        if (!serviceEditorInstance) {
            ClassicEditor
                .create(document.querySelector('#serviceDescriptionEditor'), {
                    toolbar: [
                        'bold', 'italic', 'underline', 'strikethrough', '|',
                        'bulletedList', 'numberedList', '|',
                        'alignment', '|',
                        'link', 'undo', 'redo'
                    ]
                })
                .then(editor => {
                    console.log('CKEditor đã được khởi tạo', editor);
                    serviceEditorInstance = editor; // Lưu lại instance
                })
                .catch(error => {
                    console.error('Lỗi khởi tạo CKEditor:', error);
                });
        }
    });

    // (Quan trọng) Hủy editor và RESET ẢNH khi modal bị đóng
    addServiceModal.addEventListener('hidden.bs.modal', () => {

        // (Code cũ của bạn) Hủy CKEditor
        if (serviceEditorInstance) {
            serviceEditorInstance.destroy()
                .then(() => {
                    console.log('CKEditor đã bị hủy');
                    serviceEditorInstance = null; // Reset
                })
                .catch(error => {
                    console.error('Lỗi hủy CKEditor:', error);
                });
        }

        // === THÊM VÀO: LOGIC ĐỂ RESET ẢNH UPLOAD ===
        // Chúng ta gộp chung vào 1 listener 'hidden.bs.modal'
        if (serviceUploadBox) {
            serviceUploadBox.innerHTML = originalServiceUploadHTML;
            serviceUploadBox.classList.remove('has-image');
        }
        if (serviceInput) {
            serviceInput.value = null;
        }
        // === KẾT THÚC THÊM LOGIC RESET ===
    });
}

// === THÊM VÀO: HÀM ĐỂ LẮNG NGHE SỰ KIỆN CHỌN ẢNH ===
// (Đoạn này nằm độc lập bên ngoài)
if (serviceInput && serviceUploadBox) {
    serviceInput.addEventListener('change', function (e) {
        const file = e.target.files[0];

        if (file && file.type.startsWith('image/')) {
            const reader = new FileReader();

            reader.onload = function (event) {
                serviceUploadBox.innerHTML = ''; // Xóa icon/text
                const img = document.createElement('img');
                img.src = event.target.result;
                img.alt = "Xem trước ảnh dịch vụ";
                img.className = 'image-preview'; // Áp dụng CSS xem trước

                serviceUploadBox.appendChild(img);
                serviceUploadBox.classList.add('has-image');
            }

            reader.readAsDataURL(file);

        } else if (file) {
            alert('Vui lòng chỉ chọn tệp hình ảnh.');
            serviceInput.value = null;
        }
    });
}
// === KẾT THÚC HÀM CHỌN ẢNH ===


// --- BỔ SUNG JS CHO DROPDOWN DANH MỤC ---
function toggleCategoryDropdown(btn) {
    const dropdown = btn.nextElementSibling;
    const allDropdowns = document.querySelectorAll('.dropdown-menu-category');

    allDropdowns.forEach(d => {
        if (d !== dropdown) {
            d.classList.remove('show');
        }
    });

    document.querySelectorAll('.dropdown-menu-service').forEach(d => {
        d.classList.remove('show');
    });

    if (dropdown) {
        dropdown.classList.toggle('show');
    }
}

$(document).ready(function () {
    $('#serviceEmployees').select2({
        placeholder: "Select employees...",
        allowClear: true,
        width: '100%',
        dropdownParent: $('#addServiceModal')
    });
});
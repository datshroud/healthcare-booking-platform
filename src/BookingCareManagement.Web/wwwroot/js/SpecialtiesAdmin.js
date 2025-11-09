// Toggle specialty dropdown
function toggleSpecialtyDropdown(btn) {
    const dropdown = btn.nextElementSibling;
    const allDropdowns = document.querySelectorAll('.dropdown-menu-specialty');

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
    // Đóng menu CHUYÊN KHOA nếu click ra ngoài
    if (!e.target.closest('.btn-more-specialty')) {
        document.querySelectorAll('.dropdown-menu-specialty').forEach(d => {
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
let specialtyEditorInstance;
const addSpecialtyModal = document.getElementById('addSpecialtyModal');

// === THÊM VÀO: CÁC BIẾN CHO UPLOAD ẢNH CHUYÊN KHOA ===
const specialtyInput = document.getElementById('specialtyImageInput');
const specialtyUploadBox = document.getElementById('specialtyUploadBox');
// Phải kiểm tra specialtyUploadBox tồn tại không, nếu không sẽ lỗi
const originalSpecialtyUploadHTML = specialtyUploadBox ? specialtyUploadBox.innerHTML : '';
// === KẾT THÚC THÊM BIẾN ===


if (addSpecialtyModal) {
    // Sự kiện "shown.bs.modal" bắn ra KHI modal đã hiển thị xong
    addSpecialtyModal.addEventListener('shown.bs.modal', () => {
        // (Code cũ của bạn) Chỉ khởi tạo nếu chưa có
        if (!specialtyEditorInstance) {
            ClassicEditor
                .create(document.querySelector('#specialtyDescriptionEditor'), {
                    toolbar: [
                        'bold', 'italic', 'underline', 'strikethrough', '|',
                        'bulletedList', 'numberedList', '|',
                        'alignment', '|',
                        'link', 'undo', 'redo'
                    ]
                })
                .then(editor => {
                    console.log('CKEditor đã được khởi tạo', editor);
                    specialtyEditorInstance = editor; // Lưu lại instance
                })
                .catch(error => {
                    console.error('Lỗi khởi tạo CKEditor:', error);
                });
        }
    });

    // (Quan trọng) Hủy editor và RESET ẢNH khi modal bị đóng
    addSpecialtyModal.addEventListener('hidden.bs.modal', () => {

        // (Code cũ của bạn) Hủy CKEditor
        if (specialtyEditorInstance) {
            specialtyEditorInstance.destroy()
                .then(() => {
                    console.log('CKEditor đã bị hủy');
                    specialtyEditorInstance = null; // Reset
                })
                .catch(error => {
                    console.error('Lỗi hủy CKEditor:', error);
                });
        }

        // === THÊM VÀO: LOGIC ĐỂ RESET ẢNH UPLOAD ===
        // Chúng ta gộp chung vào 1 listener 'hidden.bs.modal'
        if (specialtyUploadBox) {
            specialtyUploadBox.innerHTML = originalSpecialtyUploadHTML;
            specialtyUploadBox.classList.remove('has-image');
        }
        if (specialtyInput) {
            specialtyInput.value = null;
        }
        // === KẾT THÚC THÊM LOGIC RESET ===
    });
}

// === THÊM VÀO: HÀM ĐỂ LẮNG NGHE SỰ KIỆN CHỌN ẢNH ===
// (Đoạn này nằm độc lập bên ngoài)
if (specialtyInput && specialtyUploadBox) {
    specialtyInput.addEventListener('change', function (e) {
        const file = e.target.files[0];

        if (file && file.type.startsWith('image/')) {
            const reader = new FileReader();

            reader.onload = function (event) {
                specialtyUploadBox.innerHTML = ''; // Xóa icon/text
                const img = document.createElement('img');
                img.src = event.target.result;
                img.alt = "Xem trước ảnh dịch vụ";
                img.className = 'image-preview'; // Áp dụng CSS xem trước

                specialtyUploadBox.appendChild(img);
                specialtyUploadBox.classList.add('has-image');
            }

            reader.readAsDataURL(file);

        } else if (file) {
            alert('Vui lòng chỉ chọn tệp hình ảnh.');
            specialtyInput.value = null;
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
    $('#specialtyDoctors').select2({
        placeholder: "Select doctors...",
        allowClear: true,
        width: '100%',
        dropdownParent: $('#addSpecialtyModal')
    });
});
// Biến toàn cục
let specialtyEditorInstance;
const addSpecialtyModal = document.getElementById('addSpecialtyModal');
const specialtyInput = document.getElementById('specialtyImageInput');
const specialtyUploadBox = document.getElementById('specialtyUploadBox');
const originalSpecialtyUploadHTML = specialtyUploadBox ? specialtyUploadBox.innerHTML : '';
const addCategoryModal = document.getElementById('addCategoryModal');

document.addEventListener('DOMContentLoaded', function () {

    // === 1. BIẾN ===
    const categoryListContainer = document.getElementById('category-list-container');
    const categoryCountSpan = document.getElementById('category-count');
    const btnAddCategorySubmit = document.getElementById('btn-add-category-submit');
    const addCategoryForm = document.getElementById('add-category-form');

    const serviceListContainer = document.getElementById('service-list-container');
    const serviceCountSpan = document.getElementById('service-count');
    const btnAddServiceSubmit = document.getElementById('btn-add-service-submit');

    const specialtyCategorySelect = $('#specialtyCategory');
    const specialtyDoctorsSelect = $('#specialtyDoctors');

    // === NÂNG CẤP: Biến cache ===
    let allDoctors = [];
    let allCategories = [];
    let allServicesCache = []; // <-- Cache cho tất cả Dịch vụ (để lọc)

    // Tải mọi thứ
    loadCategories();
    loadServices(); // Tải tất cả dịch vụ 1 lần

    // === 2. TẢI DANH MỤC (Specialty) ===
    async function loadCategories() {
        if (!categoryListContainer) return;
        try {
            const response = await fetch('/api/Specialty');
            if (!response.ok) throw new Error('Không thể tải danh mục.');
            allCategories = await response.json();
            renderCategoryList(allCategories);
        } catch (error) {
            console.error(error);
            categoryListContainer.innerHTML = '<p class="text-danger p-3">Lỗi tải danh mục.</p>';
        }
    }

    function renderCategoryList(categories) {
        categoryListContainer.innerHTML = '';
        const allCard = `
            <div class="category-card active" data-id="all">
                <div class="category-card-header">
                    <div><div class="category-name">Tất cả các chuyên khoa</div></div>
                </div>
            </div>`;
        categoryListContainer.insertAdjacentHTML('beforeend', allCard);

        categories.forEach(category => {
            const categoryCard = `
                <div class="category-card other-category" data-id="${category.id}">
                    <div class="category-card-header">
                        <div>
                            <div class="category-name">${category.name}</div>
                            <div class="category-subtitle">0 Dịch vụ</div> 
                        </div>
                        <div style="position: relative;">
                            <button class="btn-more-category"><i class="fas fa-ellipsis-h"></i></button>
                            <div class="dropdown-menu-category">
                                <button class="dropdown-item-category btn-edit-category" data-id="${category.id}">
                                    <i class="fas fa-edit"></i> <span>Chỉnh sửa</span>
                                </button>
                                <button class="dropdown-item-category text-danger btn-delete-category" data-id="${category.id}">
                                    <i class="fas fa-trash"></i> <span>Xóa danh mục</span>
                                </button>
                            </div>
                        </div>
                    </div>
                </div>`;
            categoryListContainer.insertAdjacentHTML('beforeend', categoryCard);
        });
        categoryCountSpan.textContent = `(${categories.length})`;
    }

    // === 3. TẢI DỊCH VỤ (Service) ===
    async function loadServices() {
        if (!serviceListContainer) return;
        try {
            const response = await fetch('/api/Service');
            if (!response.ok) throw new Error('Không thể tải dịch vụ.');

            allServicesCache = await response.json(); // <-- LƯU VÀO CACHE

            // Lần đầu tải, hiển thị tất cả
            renderServiceList(allServicesCache, "Tất cả các chuyên khoa");
        } catch (error) {
            console.error(error);
            serviceListContainer.innerHTML = '<p class="text-danger p-3">Lỗi tải dịch vụ.</p>';
        }
    }

    // === NÂNG CẤP: Hàm render Dịch vụ (thêm 'title') ===
    function renderServiceList(services, title) {
        serviceListContainer.innerHTML = '';

        // Cập nhật tiêu đề và số lượng
        const titleElement = document.querySelector('.specialties-title');
        if (titleElement) {
            titleElement.innerHTML = `${title} <span class="specialties-count" id="service-count">(${services.length})</span>`;
        }

        if (services.length === 0) {
            serviceListContainer.innerHTML = '<p class="p-3">Không có dịch vụ nào.</p>';
            return;
        }

        services.forEach(service => {
            const price = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(service.price);

            let durationText = `${service.durationInMinutes} phút`;
            if (service.durationInMinutes >= 60) {
                const hours = Math.floor(service.durationInMinutes / 60);
                const minutes = service.durationInMinutes % 60;
                durationText = `${hours} giờ` + (minutes > 0 ? ` ${minutes} phút` : '');
            }

            const doctorsHtml = service.doctors.map(doc =>
                doc.avatarUrl
                    ? `<img src="${doc.avatarUrl}" class="doctor-avatar-small" alt="${doc.fullName}">`
                    : `<div class="doctor-avatar-small doctor-avatar-teal">${doc.fullName ? doc.fullName[0].toUpperCase() : 'D'}</div>`
            ).join('');

            const serviceItem = `
                <div class="specialty-item ${service.active ? '' : 'inactive'}" data-id="${service.id}" style="--specialty-color:${service.color || '#6c757d'}">
                    <div class="drag-handle"><i class="fas fa-grip-vertical"></i></div>
                    <div class="specialty-info">
                        ${service.imageUrl
                    ? `<img src="${service.imageUrl}" class="specialty-avatar" alt="${service.name}">`
                    : `<img src="https://ui-avatars.com/api/?name=${encodeURIComponent(service.name)}&background=${(service.color || '6c757d').substring(1)}&color=fff&size=40&rounded=true" class="specialty-avatar" alt="${service.name}">`
                }
                        <div class="specialty-details">
                            <div class="specialty-name">${service.name}</div>
                            <div class="specialty-duration">${durationText}</div>
                        </div>
                    </div>
                    <div class="specialty-price">${price}</div>
                    <div class="specialty-doctors">${doctorsHtml}</div>
                    <div style="position: relative;">
                        <button class="btn-more-specialty"><i class="fas fa-ellipsis-h"></i></button>
                        <div class="dropdown-menu-specialty">
                            <button class="dropdown-item-specialty btn-edit-service" data-id="${service.id}">
                                <i class="fas fa-edit"></i> <span>Chỉnh sửa</span>
                            </button>
                            ${service.active
                    ? `<button class="dropdown-item-specialty btn-block-service" data-id="${service.id}">
                                     <i class="fas fa-ban"></i> <span>Chặn dịch vụ</span>
                                   </button>`
                    : `<button class="dropdown-item-specialty btn-unblock-service" data-id="${service.id}">
                                     <i class="fas fa-unlock"></i> <span>Bỏ chặn</span>
                                   </button>`
                }
                            <button class="dropdown-item-specialty text-danger btn-delete-service" data-id="${service.id}">
                                <i class="fas fa-trash"></i> <span>Xóa vĩnh viễn</span>
                            </button>
                        </div>
                    </div>
                </div>`;
            serviceListContainer.insertAdjacentHTML('beforeend', serviceItem);
        });
    }

    // === 4. TẢI DATA CHO MODAL ===
    async function loadModalDropdowns() {
        try {
            if (allDoctors.length === 0) {
                const docRes = await fetch('/api/Doctor');
                allDoctors = await docRes.json();
            }
            specialtyDoctorsSelect.empty();
            allDoctors.forEach(doc => {
                if (doc.active) {
                    const option = new Option(doc.fullName, doc.id, false, false);
                    specialtyDoctorsSelect.append(option);
                }
            });
            specialtyDoctorsSelect.trigger('change');

            if (allCategories.length === 0) { // Nếu chưa tải
                await loadCategories();
            }
            specialtyCategorySelect.empty();
            allCategories.forEach(cat => {
                const option = new Option(cat.name, cat.id, false, false);
                specialtyCategorySelect.append(option);
            });
            specialtyCategorySelect.trigger('change');

        } catch (error) {
            console.error("Lỗi tải data cho modal: ", error);
        }
    }

    // === 5. XỬ LÝ SỰ KIỆN CLICK ===

    // 5a. Thêm Danh mục (POST /api/Specialty)
    if (btnAddCategorySubmit) {
        btnAddCategorySubmit.addEventListener('click', async () => {
            const categoryName = document.getElementById('categoryNameInput').value;
            if (!categoryName) {
                Swal.fire('Lỗi', 'Tên danh mục là bắt buộc.', 'warning');
                return;
            }
            const command = { name: categoryName, description: null, imageUrl: null };
            try {
                const response = await fetch('/api/Specialty', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(command)
                });
                if (!response.ok) throw new Error('Không thể tạo danh mục.');

                const modalInstance = bootstrap.Modal.getInstance(addCategoryModal);
                modalInstance.hide();
                addCategoryForm.reset();
                Swal.fire('Thành công!', 'Đã tạo danh mục mới.', 'success');
                loadCategories(); // Tải lại danh sách
            } catch (error) {
                Swal.fire('Lỗi', error.message, 'error');
            }
        });
    }

    // 5b. Thêm Dịch vụ (POST /api/Service)
    if (btnAddServiceSubmit) {
        btnAddServiceSubmit.addEventListener('click', async () => {
            btnAddServiceSubmit.disabled = true;
            btnAddServiceSubmit.innerHTML = '<span class="spinner-border spinner-border-sm"></span> Đang thêm...';
            try {
                let imageUrl = null;
                const file = specialtyInput.files[0];
                if (file) {
                    const formData = new FormData();
                    formData.append('file', file);
                    const uploadRes = await fetch('/api/Upload', { method: 'POST', body: formData });
                    if (!uploadRes.ok) throw new Error('Upload ảnh thất bại.');
                    const result = await uploadRes.json();
                    imageUrl = result.avatarUrl;
                }

                const description = specialtyEditorInstance ? specialtyEditorInstance.getData() : '';
                const command = {
                    name: document.getElementById('specialtyName').value,
                    description: description,
                    price: parseFloat(document.getElementById('specialtyPrice').value) || 0,
                    durationInMinutes: parseInt(document.getElementById('specialtyDuration').value) || 30,
                    color: document.getElementById('specialtyColor').value,
                    imageUrl: imageUrl,
                    specialtyId: specialtyCategorySelect.val(),
                    doctorIds: specialtyDoctorsSelect.val() || []
                };

                if (!command.name || !command.specialtyId || !command.durationInMinutes) {
                    throw new Error('Tên, Danh mục, và Thời gian là bắt buộc.');
                }

                const response = await fetch('/api/Service', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(command)
                });
                if (!response.ok) {
                    const err = await response.json();
                    throw new Error(err.detail || 'Không thể tạo dịch vụ.');
                }

                const modalInstance = bootstrap.Modal.getInstance(addSpecialtyModal);
                modalInstance.hide();
                Swal.fire('Thành công!', 'Đã tạo dịch vụ mới.', 'success');
                loadServices(); // Tải lại danh sách dịch vụ

            } catch (error) {
                Swal.fire('Lỗi', error.message, 'error');
            } finally {
                btnAddServiceSubmit.disabled = false;
                btnAddServiceSubmit.innerHTML = 'Thêm vào';
            }
        });
    }

    // 5c. Bắt sự kiện click trên Danh mục (Sidebar)
    if (categoryListContainer) {
        categoryListContainer.addEventListener('click', async (e) => {
            const deleteButton = e.target.closest('.btn-delete-category');
            const editButton = e.target.closest('.btn-edit-category');
            const dropdownButton = e.target.closest('.btn-more-category');
            const categoryCard = e.target.closest('.category-card');

            // XÓA DANH MỤC
            if (deleteButton) {
                const categoryId = deleteButton.getAttribute('data-id');
                Swal.fire({
                    title: 'Xóa danh mục?',
                    text: "Bạn có chắc muốn xóa vĩnh viễn danh mục này?",
                    icon: 'warning',
                    showCancelButton: true,
                    confirmButtonColor: '#d33',
                    confirmButtonText: 'Đồng ý, Xóa!',
                    cancelButtonText: 'Hủy'
                }).then(async (result) => {
                    if (result.isConfirmed) {
                        try {
                            const response = await fetch(`/api/Specialty/${categoryId}`, { method: 'DELETE' });
                            if (!response.ok) throw new Error('Không thể xóa danh mục.');
                            Swal.fire('Đã xóa!', 'Danh mục đã bị xóa.', 'success');
                            loadCategories(); // Tải lại
                        } catch (error) {
                            Swal.fire('Lỗi!', error.message, 'error');
                        }
                    }
                });
                return; // Dừng lại
            }

            // SỬA DANH MỤC
            if (editButton) {
                alert('Chức năng "Sửa danh mục" chưa được lập trình.');
                return; // Dừng lại
            }

            // MỞ DROPDOWN
            if (dropdownButton) {
                toggleCategoryDropdown(dropdownButton);
                return; // Dừng lại
            }

            // === NÂNG CẤP: CHỌN DANH MỤC (LỌC) ===
            if (categoryCard) {
                // 1. Cập nhật UI (Active class)
                categoryListContainer.querySelectorAll('.category-card').forEach(c => c.classList.remove('active'));
                categoryCard.classList.add('active');

                const categoryId = categoryCard.getAttribute('data-id');
                const categoryName = categoryCard.querySelector('.category-name').textContent;

                if (categoryId === 'all') {
                    // 2a. Hiển thị TẤT CẢ
                    renderServiceList(allServicesCache, "Tất cả các chuyên khoa");
                } else {
                    // 2b. Lọc và hiển thị
                    const filteredServices = allServicesCache.filter(service => service.specialtyId === categoryId);
                    renderServiceList(filteredServices, categoryName);
                }
            }
        });
    }

    // 5d. Bắt các sự kiện click trên Dịch vụ (Main Content)
    if (serviceListContainer) {
        serviceListContainer.addEventListener('click', async (e) => {
            const dropdownButton = e.target.closest('.btn-more-specialty');
            if (dropdownButton) {
                toggleSpecialtyDropdown(dropdownButton);
                return;
            }

            const blockButton = e.target.closest('.btn-block-service');
            if (blockButton) {
                const serviceId = blockButton.getAttribute('data-id');
                Swal.fire({
                    title: 'Chặn dịch vụ?', icon: 'warning', showCancelButton: true,
                    confirmButtonText: 'Đồng ý, Chặn!', cancelButtonText: 'Hủy'
                }).then(async (result) => {
                    if (result.isConfirmed) {
                        await fetch(`/api/Service/${serviceId}`, { method: 'DELETE' });
                        loadServices();
                    }
                });
                return;
            }

            const unblockButton = e.target.closest('.btn-unblock-service');
            if (unblockButton) {
                const serviceId = unblockButton.getAttribute('data-id');
                await fetch(`/api/Service/${serviceId}/activate`, { method: 'PUT' });
                loadServices();
                return;
            }

            const deleteButton = e.target.closest('.btn-delete-service');
            if (deleteButton) {
                const serviceId = deleteButton.getAttribute('data-id');
                Swal.fire({
                    title: 'Xóa Vĩnh Viễn?', text: "Dịch vụ sẽ bị xóa vĩnh viễn.",
                    icon: 'error', showCancelButton: true,
                    confirmButtonText: 'Đồng ý, Xóa!', cancelButtonText: 'Hủy'
                }).then(async (result) => {
                    if (result.isConfirmed) {
                        await fetch(`/api/Service/${serviceId}/permanent`, { method: 'DELETE' });
                        loadServices();
                    }
                });
                return;
            }
        });
    }

    // === 6. CODE HỖ TRỢ (TỪ FILE CỦA BẠN) ===

    // Upload ảnh (cho Dịch vụ)
    if (specialtyInput && specialtyUploadBox) {
        specialtyInput.addEventListener('change', function (e) {
            const file = e.target.files[0];
            if (file && file.type.startsWith('image/')) {
                const reader = new FileReader();
                reader.onload = function (event) {
                    specialtyUploadBox.innerHTML = `<img src="${event.target.result}" alt="Xem trước" class="image-preview">`;
                    specialtyUploadBox.classList.add('has-image');
                }
                reader.readAsDataURL(file);
            } else if (file) {
                Swal.fire('Lỗi File', 'Vui lòng chỉ chọn tệp hình ảnh.', 'warning');
                specialtyInput.value = null;
            }
        });
    }

    // Xử lý Modal "Thêm Dịch vụ"
    if (addSpecialtyModal) {
        addSpecialtyModal.addEventListener('shown.bs.modal', () => {
            if (!specialtyEditorInstance) {
                ClassicEditor.create(document.querySelector('#specialtyDescriptionEditor'), {
                    toolbar: ['bold', 'italic', 'underline', 'bulletedList', 'numberedList', 'link']
                })
                    .then(editor => { specialtyEditorInstance = editor; })
                    .catch(error => { console.error('Lỗi CKEditor:', error); });
            }

            // Tải data và KHỞI TẠO SELECT2
            loadModalDropdowns().then(() => {
                // Chỉ khởi tạo nếu chưa có
                if (!specialtyDoctorsSelect.data('select2')) {
                    specialtyDoctorsSelect.select2({
                        placeholder: "Chọn bác sĩ...",
                        allowClear: true,
                        width: '100%',
                        dropdownParent: $('#addSpecialtyModal')
                    });
                }
                if (!specialtyCategorySelect.data('select2')) {
                    specialtyCategorySelect.select2({
                        placeholder: "Chọn danh mục...",
                        allowClear: false,
                        width: '100%',
                        dropdownParent: $('#addSpecialtyModal')
                    });
                }
            });
        });

        // Reset modal khi đóng
        addSpecialtyModal.addEventListener('hidden.bs.modal', () => {
            if (specialtyEditorInstance) {
                specialtyEditorInstance.destroy()
                    .then(() => { specialtyEditorInstance = null; })
                    .catch(error => { console.error('Lỗi hủy CKEditor:', error); });
            }
            if (specialtyUploadBox) {
                specialtyUploadBox.innerHTML = originalSpecialtyUploadHTML;
                specialtyUploadBox.classList.remove('has-image');
            }
            if (specialtyInput) {
                specialtyInput.value = null;
            }
            specialtyDoctorsSelect.val(null).trigger('change');
            specialtyCategorySelect.val(null).trigger('change');
            document.getElementById('add-service-form').reset();
            document.getElementById('specialtyColor').value = '#c0eb75';
            document.querySelector('.color-swatch').style.backgroundColor = '#c0eb75';
            document.querySelector('.color-swatch input').value = '#c0eb75';
        });
    }

    // (Các hàm toggle dropdown)
    function toggleCategoryDropdown(btn) {
        const dropdown = btn.nextElementSibling;
        const allDropdowns = document.querySelectorAll('.dropdown-menu-category');
        allDropdowns.forEach(d => { if (d !== dropdown) d.classList.remove('show'); });
        dropdown.classList.toggle('show');
    }
    function toggleSpecialtyDropdown(btn) {
        const dropdown = btn.nextElementSibling;
        const allDropdowns = document.querySelectorAll('.dropdown-menu-specialty');
        allDropdowns.forEach(d => { if (d !== dropdown) d.classList.remove('show'); });
        dropdown.classList.toggle('show');
    }

    // Đóng dropdown khi click ra ngoài
    document.addEventListener('click', function (e) {
        if (!e.target.closest('.btn-more-category')) {
            document.querySelectorAll('.dropdown-menu-category').forEach(d => d.classList.remove('show'));
        }
        if (!e.target.closest('.btn-more-specialty')) {
            document.querySelectorAll('.dropdown-menu-specialty').forEach(d => d.classList.remove('show'));
        }
    });

    // (Code cũ cho Sort Dropdown, Tabs...)
});
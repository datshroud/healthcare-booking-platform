// Chạy code khi toàn bộ trang HTML đã tải xong
document.addEventListener('DOMContentLoaded', function () {

    // === PHẦN 1: TẢI DANH SÁCH BÁC SĨ (GET) ===

    const doctorTableBody = document.getElementById('doctor-table-body');
    const doctorCountTitle = document.getElementById('doctor-count-title');
    const doctorPlanInfo = document.getElementById('doctor-plan-info');
    loadDoctors(); // Tải danh sách bác sĩ ngay khi mở trang

    async function loadDoctors() {
        try {
            const response = await fetch('/api/Doctor');
            if (!response.ok) {
                throw new Error('Không thể tải danh sách bác sĩ');
            }
            const doctors = await response.json();
            updateDoctorCounters(doctors.length);
            renderDoctorTable(doctors);
        } catch (error) {
            console.error(error);
            doctorTableBody.innerHTML = '<tr><td colspan="6">Không thể tải dữ liệu.</td></tr>';
            updateDoctorCounters(0);
        }
    }

    function updateDoctorCounters(count) {
        if (doctorCountTitle) {
            doctorCountTitle.textContent = `Bác sĩ (${count})`;
        }

        if (doctorPlanInfo) {
            const planLimit = parseInt(doctorPlanInfo.getAttribute('data-plan-limit') || '0', 10) || 0;
            const limitText = planLimit > 0 ? `${count}/${planLimit}` : `${count}`;
            doctorPlanInfo.textContent = `Bác sĩ trong gói ${limitText}`;
        }
    }

    function renderDoctorTable(doctors) {
        doctorTableBody.innerHTML = ''; // Xóa bảng cũ

        if (doctors.length === 0) {
            doctorTableBody.innerHTML = '<tr><td colspan="6">Chưa có bác sĩ nào.</td></tr>';
            return;
        }

        doctors.forEach(doctor => {
            const row = document.createElement('tr');
            row.setAttribute('data-id', doctor.id); // Gán ID lên <tr>

            // 1. Checkbox
            const cellCheckbox = `<td><input type="checkbox"></td>`;

            // 2. Tên (Avatar + Tên)
            // Tạm thời dùng chữ cái đầu
            const resolvedName = resolveDoctorName(doctor);
            const firstLetter = resolvedName ? resolvedName[0].toUpperCase() : 'B';
            const avatar = doctor.avatarUrl
                ? `<img src="${doctor.avatarUrl}" class="employee-avatar" alt="${resolvedName}">`
                : `<div class="employee-avatar avatar-teal">${firstLetter}</div>`;

            const cellName = `
                <td>
                    <div class="employee-name-cell">
                        ${avatar}
                        <div class="employee-name">
                            <span>${resolvedName}</span>
                        </div>
                    </div>
                </td>`;

            // 3. Email
            const cellEmail = `<td>${doctor.email || ''}</td>`;

            // 4. Điện thoại
            const cellPhone = `<td>${doctor.phoneNumber || ''}</td>`;

            // 5. Trạng thái
            const statusBadge = doctor.active
                ? '<span class="status-badge">Có sẵn</span>'
                : '<span class="status-badge status-inactive">Vô hiệu hóa</span>';
            const cellStatus = `<td>${statusBadge}</td>`;

            // 6. Nút Action (Sửa/Xóa)
            const cellActions = `
                <td>
                    <div class="action-menu">
                        <button class="btn-action"><i class="fas fa-ellipsis-h"></i></button>
                        <div class="dropdown-menu-custom">
                            <button class="dropdown-item-custom btn-edit-doctor" data-id="${doctor.id}">
                                <i class="fas fa-edit"></i> Chỉnh sửa bác sĩ
                            </button>
                            <button class="dropdown-item-custom btn-delete-doctor text-danger" data-id="${doctor.id}">
                                <i class="fas fa-trash"></i> Xóa bác sĩ
                            </button>
                        </div>
                    </div>
                </td>`;

            row.innerHTML = cellCheckbox + cellName + cellEmail + cellPhone + cellStatus + cellActions;
            doctorTableBody.appendChild(row);
        });
    }

    // === PHẦN 2: THÊM BÁC SĨ MỚI (POST) ===

    const addEmployeeModal = document.getElementById('addEmployeeModal');
    const btnAddSubmit = document.getElementById('btn-add-employee-submit');
    const addForm = addEmployeeModal.querySelector('form');

    // Khi bấm nút "Thêm nhân viên" trong modal
    btnAddSubmit.addEventListener('click', async function () {
        // Thu thập dữ liệu từ form
        const newDoctorData = {
            firstName: document.getElementById('firstName').value,
            lastName: document.getElementById('lastName').value,
            email: document.getElementById('email').value,
            phoneNumber: document.getElementById('phone').value,

            // !! QUAN TRỌNG !!
            // Tạm thời gửi mảng rỗng. 
            // Bước tiếp theo chúng ta sẽ làm API GET /api/Specialty để lấy
            specialtyIds: []
            // Tạm thời chưa xử lý upload file ảnh
        };

        try {
            const response = await fetch('/api/Doctor', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(newDoctorData),
            });

            if (!response.ok) {
                // Nếu lỗi (ví dụ: Trùng Email)
                const errorData = await response.json();
                throw new Error(errorData.detail || 'Không thể tạo bác sĩ.');
            }

            // Thành công!
            const createdDoctor = await response.json();

            // Đóng modal
            const modalInstance = bootstrap.Modal.getInstance(addEmployeeModal);
            modalInstance.hide();

            addForm.reset(); // Xóa trắng form
            loadDoctors(); // Tải lại bảng

            // (Nên dùng thư viện thông báo (Toast/SweetAlert) ở đây)
            alert(`Thêm bác sĩ ${resolveDoctorName(createdDoctor)} thành công!`);

        } catch (error) {
            console.error(error);
            alert(`Lỗi: ${error.message}`);
        }
    });

    // === PHẦN 3: XÓA BÁC SĨ (DELETE) ===

    // Dùng Event Delegation (ủy quyền sự kiện)
    // Vì các nút xóa được tạo động, chúng ta phải "nghe" ở `<tbody>`
    doctorTableBody.addEventListener('click', async function (e) {

        // Kiểm tra xem có bấm trúng nút "Xóa" không
        const deleteButton = e.target.closest('.btn-delete-doctor');
        if (deleteButton) {
            const doctorId = deleteButton.getAttribute('data-id');

            // (Nên dùng SweetAlert ở đây để confirm đẹp hơn)
            if (!confirm('Bạn có chắc chắn muốn xóa bác sĩ này?\n(Hành động này sẽ vô hiệu hóa bác sĩ)')) {
                return;
            }

            try {
                const response = await fetch(`/api/Doctor/${doctorId}`, {
                    method: 'DELETE'
                });

                if (!response.ok) {
                    const errorData = await response.json();
                    throw new Error(errorData.detail || 'Không thể xóa bác sĩ.');
                }

                // Thành công!
                alert('Xóa (vô hiệu hóa) bác sĩ thành công.');
                loadDoctors(); // Tải lại bảng

            } catch (error) {
                console.error(error);
                alert(`Lỗi: ${error.message}`);
            }
        }

        // Tương tự, bắt sự kiện cho nút SỬA
        const editButton = e.target.closest('.btn-edit-doctor');
        if (editButton) {
            const doctorId = editButton.getAttribute('data-id');
            // Chuyển đến trang chỉnh sửa bác sĩ
            window.location.href = `/doctors/${doctorId}/info`;
        }
    });

    // === PHẦN 4: CODE CŨ CỦA BẠN (ĐÃ TỐI ƯU) ===

    // Đóng/mở dropdown (dùng ủy quyền sự kiện)
    doctorTableBody.addEventListener('click', function (e) {
        const actionButton = e.target.closest('.btn-action');
        if (actionButton) {
            toggleDropdown(actionButton);
        }
    });

    function toggleDropdown(btn) {
        const dropdown = btn.nextElementSibling;
        const allDropdowns = document.querySelectorAll('.dropdown-menu-custom');

        // Đóng tất cả dropdown khác
        allDropdowns.forEach(d => {
            if (d !== dropdown) {
                d.classList.remove('show');
            }
        });

        // Mở/đóng cái hiện tại
        dropdown.classList.toggle('show');
    }

    // Đóng dropdown khi bấm ra ngoài
    document.addEventListener('click', function (e) {
        if (!e.target.closest('.action-menu')) {
            document.querySelectorAll('.dropdown-menu-custom').forEach(d => {
                d.classList.remove('show');
            });
        }
    });

    // Select all checkbox
    document.getElementById('selectAll').addEventListener('change', function () {
        const checkboxes = doctorTableBody.querySelectorAll('input[type="checkbox"]');
        checkboxes.forEach(cb => cb.checked = this.checked);
    });

    // Search (Client-side)
    document.getElementById('searchInput').addEventListener('input', function (e) {
        const searchTerm = e.target.value.toLowerCase();
        const rows = doctorTableBody.querySelectorAll('tr');
        rows.forEach(row => {
            const text = row.textContent.toLowerCase();
            row.style.display = text.includes(searchTerm) ? '' : 'none';
        });
    });

    // === PHẦN 5: CODE MODAL (ẢNH, SELECT2) ===

    // (Toàn bộ code xem trước ảnh và Select2 của bạn ở đây)
    // ...
    const fileInput = document.getElementById('fileUploadInput');
    const uploadArea = document.querySelector('.file-upload-area');
    const originalUploadAreaContent = uploadArea.innerHTML;

    fileInput.addEventListener('change', function (event) {
        const file = event.target.files[0];
        if (file && file.type.startsWith('image/')) {
            const reader = new FileReader();
            reader.onload = function (e) {
                uploadArea.innerHTML = `<img src="${e.target.result}" alt="Ảnh xem trước" class="image-preview">`;
                uploadArea.classList.add('has-image');
            }
            reader.readAsDataURL(file);
        } else if (file) {
            alert('Vui lòng chỉ chọn tệp hình ảnh.');
            fileInput.value = null;
        }
    });

    addEmployeeModal.addEventListener('hidden.bs.modal', function () {
        uploadArea.innerHTML = originalUploadAreaContent;
        fileInput.value = null;
        uploadArea.classList.remove('has-image');
        addForm.reset(); // Reset form khi đóng
        $('#service').val(null).trigger('change'); // Reset Select2
    });

    function resolveDoctorName(doctor) {
        if (!doctor) {
            return '';
        }

        if (doctor.fullName && doctor.fullName.trim().length > 0) {
            return doctor.fullName.trim();
        }

        const pieces = [doctor.firstName, doctor.lastName]
            .map(x => (x || '').trim())
            .filter(x => x.length > 0);

        if (pieces.length > 0) {
            return pieces.join(' ');
        }

        return doctor.email || doctor.phoneNumber || 'Bác sĩ';
    }

    // Kích hoạt Select2
    $('#service').select2({
        placeholder: "Chọn dịch vụ...",
        allowClear: true,
        width: '100%',
        dropdownParent: $('#addEmployeeModal')
    });

});
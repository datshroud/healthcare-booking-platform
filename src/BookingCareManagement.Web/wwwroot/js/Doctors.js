// Chạy code khi toàn bộ trang HTML đã tải xong
document.addEventListener('DOMContentLoaded', function () {

    // === PHẦN 1: TẢI DANH SÁCH BÁC SĨ (GET) ===

    const doctorTableBody = document.getElementById('doctor-table-body');
    const doctorCountTitle = document.getElementById('doctor-count-title');
    const doctorPlanInfo = document.getElementById('doctor-plan-info');
    let specialtiesCache = []; // Biến cache cho chuyên khoa

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
            row.setAttribute('data-id', doctor.id);

            // ... (Code render table giữ nguyên) ...
            const cellCheckbox = `<td><input type="checkbox"></td>`;
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
            const cellEmail = `<td>${doctor.email || ''}</td>`;
            const cellPhone = `<td>${doctor.phoneNumber || ''}</td>`;
            const statusBadge = doctor.active
                ? '<span class="status-badge">Có sẵn</span>'
                : '<span class="status-badge status-inactive">Vô hiệu hóa</span>';
            const cellStatus = `<td>${statusBadge}</td>`;
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

        // THAY ĐỔI 1: Lấy ID chuyên khoa *duy nhất* đã chọn
        // (Select2 ở chế độ 'chọn một' sẽ trả về 1 string ID hoặc null)
        const selectedSpecialtyId = $('#service').val();

        // THAY ĐỔI 2: Tạo một mảng để gửi đi (khớp với backend)
        let specialtyIdsToSend = [];
        if (selectedSpecialtyId) {
            specialtyIdsToSend.push(selectedSpecialtyId); // Bỏ 1 ID duy nhất vào mảng
        }

        // Thu thập dữ liệu từ form
        const newDoctorData = {
            firstName: document.getElementById('firstName').value,
            lastName: document.getElementById('lastName').value,
            email: document.getElementById('email').value,
            phoneNumber: document.getElementById('phone').value,

            // THAY ĐỔI 3: Gán mảng (0 hoặc 1 phần tử)
            specialtyIds: specialtyIdsToSend
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
                const errorData = await response.json();
                throw new Error(errorData.detail || 'Không thể tạo bác sĩ.');
            }

            const createdDoctor = await response.json();
            const modalInstance = bootstrap.Modal.getInstance(addEmployeeModal);
            modalInstance.hide();
            addForm.reset();
            loadDoctors();
            alert(`Thêm bác sĩ ${resolveDoctorName(createdDoctor)} thành công!`);

        } catch (error) {
            console.error(error);
            alert(`Lỗi: ${error.message}`);
        }
    });

    // === PHẦN 3: XÓA BÁC SĨ (DELETE) ===

    doctorTableBody.addEventListener('click', async function (e) {
        // ... (Code xóa giữ nguyên) ...
        const deleteButton = e.target.closest('.btn-delete-doctor');
        if (deleteButton) {
            const doctorId = deleteButton.getAttribute('data-id');
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
                alert('Xóa (vô hiệu hóa) bác sĩ thành công.');
                loadDoctors();
            } catch (error) {
                console.error(error);
                alert(`Lỗi: ${error.message}`);
            }
        }

        const editButton = e.target.closest('.btn-edit-doctor');
        if (editButton) {
            const doctorId = editButton.getAttribute('data-id');
            window.location.href = `/doctors/${doctorId}/info`;
        }
    });

    // === PHẦN 4: CODE DROPDOWN VÀ SEARCH (Giữ nguyên) ===

    doctorTableBody.addEventListener('click', function (e) {
        // ... (Giữ nguyên) ...
        const actionButton = e.target.closest('.btn-action');
        if (actionButton) {
            toggleDropdown(actionButton);
        }
    });

    function toggleDropdown(btn) {
        // ... (Giữ nguyên) ...
        const dropdown = btn.nextElementSibling;
        const allDropdowns = document.querySelectorAll('.dropdown-menu-custom');
        allDropdowns.forEach(d => {
            if (d !== dropdown) {
                d.classList.remove('show');
            }
        });
        dropdown.classList.toggle('show');
    }

    document.addEventListener('click', function (e) {
        // ... (Giữ nguyên) ...
        if (!e.target.closest('.action-menu')) {
            document.querySelectorAll('.dropdown-menu-custom').forEach(d => {
                d.classList.remove('show');
            });
        }
    });

    document.getElementById('selectAll').addEventListener('change', function () {
        // ... (Giữ nguyên) ...
        const checkboxes = doctorTableBody.querySelectorAll('input[type="checkbox"]');
        checkboxes.forEach(cb => cb.checked = this.checked);
    });

    document.getElementById('searchInput').addEventListener('input', function (e) {
        // ... (Giữ nguyên) ...
        const searchTerm = e.target.value.toLowerCase();
        const rows = doctorTableBody.querySelectorAll('tr');
        rows.forEach(row => {
            const text = row.textContent.toLowerCase();
            row.style.display = text.includes(searchTerm) ? '' : 'none';
        });
    });

    // === PHẦN 5: CODE MODAL (ẢNH, SELECT2) ===

    const fileInput = document.getElementById('fileUploadInput');
    const uploadArea = document.querySelector('.file-upload-area');
    const originalUploadAreaContent = uploadArea.innerHTML;

    fileInput.addEventListener('change', function (event) {
        // ... (Code upload ảnh giữ nguyên) ...
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

    addEmployeeModal.addEventListener('shown.bs.modal', function () {
        loadSpecialtiesForModal();
    });

    addEmployeeModal.addEventListener('hidden.bs.modal', function () {
        uploadArea.innerHTML = originalUploadAreaContent;
        fileInput.value = null;
        uploadArea.classList.remove('has-image');
        addForm.reset();

        // THAY ĐỔI: Reset Select2 về trạng thái rỗng
        const selectBox = document.getElementById('service');
        selectBox.innerHTML = ''; // Xóa các option cũ
        $('#service').val(null).trigger('change');
    });

    function resolveDoctorName(doctor) {
        // ... (Giữ nguyên) ...
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

    async function loadSpecialtiesForModal() {
        if (specialtiesCache.length > 0) {
            populateSpecialtyOptions(specialtiesCache);
            return;
        }

        try {
            const response = await fetch('/api/Specialty');
            if (!response.ok) throw new Error('Không thể tải chuyên khoa');
            const specialties = await response.json();

            specialtiesCache = specialties.filter(s => s.active);
            populateSpecialtyOptions(specialtiesCache);

        } catch (error) {
            console.error(error);
            const selectBox = document.getElementById('service');
            selectBox.innerHTML = '<option value="" disabled>Lỗi tải chuyên khoa</option>';
        }
    }

    // THAY ĐỔI: Cập nhật hàm này cho chế độ "chọn một"
    function populateSpecialtyOptions(specialties) {
        const selectBox = document.getElementById('service');
        selectBox.innerHTML = ''; // Xóa option "loading"

        // THÊM: Thêm 1 option rỗng ở đầu cho placeholder của Select2
        const placeholderOption = document.createElement('option');
        placeholderOption.value = "";
        placeholderOption.textContent = ""; // Select2 sẽ dùng placeholder text
        selectBox.appendChild(placeholderOption);

        if (specialties.length === 0) {
            placeholderOption.textContent = "Không có chuyên khoa nào";
            placeholderOption.disabled = true;
        } else {
            specialties.forEach(spec => {
                const option = document.createElement('option');
                option.value = spec.id;
                option.textContent = spec.name;
                selectBox.appendChild(option);
            });
        }

        // Khởi tạo Select2 (không còn 'multiple')
        $('#service').select2({
            placeholder: "Chọn (chỉ một) chuyên khoa...", // Sửa placeholder
            allowClear: true, // Cho phép xóa lựa chọn
            width: '100%',
            dropdownParent: $('#addEmployeeModal')
        });

        $('#service').val(null).trigger('change');
    }
});
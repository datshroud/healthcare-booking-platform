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

            const cellCheckbox = `<td><input type="checkbox"></td>`;
            const resolvedName = resolveDoctorName(doctor);
            const firstLetter = resolvedName ? resolvedName[0].toUpperCase() : 'B';

            // SỬA LỖI HIỂN THỊ AVATAR: Kiểm tra 'avatarUrl' có rỗng/null không
            const avatar = (doctor.avatarUrl && doctor.avatarUrl.trim() !== "")
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
            const toggleLabel = doctor.active ? 'Vô hiệu hóa bác sĩ' : 'Kích hoạt bác sĩ';
            const toggleIcon = doctor.active ? 'fa-user-slash' : 'fa-user-check';
            const cellActions = `
                <td>
                    <div class="action-menu">
                        <button class="btn-action"><i class="fas fa-ellipsis-h"></i></button>
                        <div class="dropdown-menu-custom">
                            <button class="dropdown-item-custom btn-edit-doctor" data-id="${doctor.id}">
                                <i class="fas fa-edit"></i> Chỉnh sửa bác sĩ
                            </button>
                            <button class="dropdown-item-custom btn-toggle-doctor-status" data-id="${doctor.id}" data-active="${doctor.active}">
                                <i class="fas ${toggleIcon}"></i> ${toggleLabel}
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

    // Lấy input file của modal bác sĩ
    const fileInput = document.getElementById('fileUploadInput');
    const uploadArea = document.querySelector('.file-upload-area');
    const originalUploadAreaContent = uploadArea.innerHTML;

    // ⭐️ HÀM NÀY ĐÃ ĐƯỢC CẬP NHẬT ⭐️
    btnAddSubmit.addEventListener('click', async function () {

        // 1. Lấy file đã chọn
        const file = fileInput?.files?.[0];
        let avatarUrl = null; // Mặc định là null

        // Hiển thị loading
        btnAddSubmit.disabled = true;
        btnAddSubmit.innerHTML = '<span class="spinner-border spinner-border-sm me-2" role="status"></span>Đang lưu...';

        try {
            // 2. Upload ảnh NẾU CÓ CHỌN FILE
            if (file) {
                console.log("Đang upload ảnh bác sĩ...");
                const formData = new FormData();
                formData.append("file", file); // Tên "file" phải khớp với API

                // Gọi API Upload
                // (Bạn phải tạo UploadController.cs để xử lý /api/Upload)
                const uploadRes = await fetch("/api/Upload", {
                    method: "POST",
                    body: formData
                });

                if (!uploadRes.ok) {
                    const err = await uploadRes.json().catch(() => ({}));
                    throw new Error(err.title || err.detail || "Tải ảnh thất bại. Vui lòng thử lại.");
                }

                const result = await uploadRes.json();

                // Giả định API trả về { "avatarUrl": "..." }
                avatarUrl = result.avatarUrl;

                if (!avatarUrl) {
                    throw new Error("API upload không trả về 'avatarUrl'.");
                }
                console.log("Upload ảnh thành công:", avatarUrl);
            }

            // 3. Lấy dữ liệu chữ từ form
            const selectedSpecialtyId = $('#service').val();
            let specialtyIdsToSend = [];
            if (selectedSpecialtyId) {
                specialtyIdsToSend.push(selectedSpecialtyId);
            }

            // 4. Gộp dữ liệu (bao gồm cả link ảnh)
            const newDoctorData = {
                firstName: document.getElementById('firstName').value,
                lastName: document.getElementById('lastName').value,
                email: document.getElementById('email').value,
                phoneNumber: document.getElementById('phone').value,
                specialtyIds: specialtyIdsToSend,
                avatarUrl: avatarUrl // Gửi link ảnh (hoặc null) lên backend
            };

            // 5. Gửi request tạo Bác sĩ
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

            loadDoctors();
            alert(`Thêm bác sĩ ${resolveDoctorName(createdDoctor)} thành công!`);

        } catch (error) {
            console.error(error);
            alert(`Lỗi: ${error.message}`);
        } finally {
            // 6. Trả lại trạng thái nút bấm
            btnAddSubmit.disabled = false;
            btnAddSubmit.innerHTML = 'Thêm bác sĩ';
        }
    });

    // === PHẦN 3: XÓA BÁC SĨ (DELETE) ===
    async function parseJsonSafe(response) {
        const contentType = response.headers.get('content-type') || '';
        const contentLength = response.headers.get('content-length');

        if (contentLength === '0') {
            return null;
        }

        const text = await response.text();
        if (!text || !contentType.includes('application/json')) {
            return null;
        }

        try {
            return JSON.parse(text);
        } catch {
            return null;
        }
    }

    doctorTableBody.addEventListener('click', async function (e) {
        // ... (Code xóa giữ nguyên) ...
        const deleteButton = e.target.closest('.btn-delete-doctor');
        if (deleteButton) {
            const doctorId = deleteButton.getAttribute('data-id');
            if (!confirm('Bạn có chắc chắn muốn xóa bác sĩ này khỏi hệ thống?\n(Hành động này sẽ xóa vĩnh viễn dữ liệu bác sĩ)')) {
                return;
            }
            try {
                const response = await fetch(`/api/Doctor/${doctorId}`, { method: 'DELETE' });
                if (!response.ok) {
                    const errorData = await parseJsonSafe(response);
                    const message = errorData?.detail || errorData?.title || 'Không thể xóa bác sĩ.';
                    throw new Error(message);
                }
                alert('Xóa bác sĩ thành công.');
                loadDoctors();
            } catch (error) {
                console.error(error);
                alert(`Lỗi: ${error.message}`);
            }
        }

        const toggleButton = e.target.closest('.btn-toggle-doctor-status');
        if (toggleButton) {
            const doctorId = toggleButton.getAttribute('data-id');
            const isActive = toggleButton.getAttribute('data-active') === 'true';
            const targetActive = !isActive;
            const actionText = targetActive ? 'kích hoạt' : 'vô hiệu hóa';

            if (!confirm(`Bạn có chắc chắn muốn ${actionText} bác sĩ này?`)) {
                return;
            }

            try {
                const response = await fetch(`/api/Doctor/${doctorId}/status`, {
                    method: 'PUT',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({ active: targetActive })
                });

                if (!response.ok) {
                    const errorData = await parseJsonSafe(response);
                    const message = errorData?.detail || errorData?.title || 'Không thể cập nhật trạng thái bác sĩ.';
                    throw new Error(message);
                }

                alert(`${targetActive ? 'Đã kích hoạt' : 'Đã vô hiệu hóa'} bác sĩ thành công.`);
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
        const actionButton = e.target.closest('.btn-action');
        if (actionButton) {
            toggleDropdown(actionButton);
        }
    });

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

    document.addEventListener('click', function (e) {
        if (!e.target.closest('.action-menu')) {
            document.querySelectorAll('.dropdown-menu-custom').forEach(d => {
                d.classList.remove('show');
            });
        }
    });

    document.getElementById('selectAll').addEventListener('change', function () {
        const checkboxes = doctorTableBody.querySelectorAll('input[type="checkbox"]');
        checkboxes.forEach(cb => cb.checked = this.checked);
    });

    document.getElementById('searchInput').addEventListener('input', function (e) {
        const searchTerm = e.target.value.toLowerCase();
        const rows = doctorTableBody.querySelectorAll('tr');
        rows.forEach(row => {
            const text = row.textContent.toLowerCase();
            row.style.display = text.includes(searchTerm) ? '' : 'none';
        });
    });

    // === PHẦN 5: CODE MODAL (ẢNH, SELECT2) ===

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

    addEmployeeModal.addEventListener('shown.bs.modal', function () {
        loadSpecialtiesForModal();
    });

    addEmployeeModal.addEventListener('hidden.bs.modal', function () {
        uploadArea.innerHTML = originalUploadAreaContent;
        fileInput.value = null; // ⭐️ Reset input file
        uploadArea.classList.remove('has-image');
        addForm.reset();

        const selectBox = document.getElementById('service');
        selectBox.innerHTML = '';
        $('#service').val(null).trigger('change');
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

    function populateSpecialtyOptions(specialties) {
        const selectBox = document.getElementById('service');
        selectBox.innerHTML = '';

        const placeholderOption = document.createElement('option');
        placeholderOption.value = "";
        placeholderOption.textContent = "";
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

        // Khởi tạo Select2 (chọn một)
        $('#service').select2({
            placeholder: "Chọn (chỉ một) chuyên khoa...",
            allowClear: true,
            width: '100%',
            dropdownParent: $('#addEmployeeModal')
        });

        $('#service').val(null).trigger('change');
    }
});
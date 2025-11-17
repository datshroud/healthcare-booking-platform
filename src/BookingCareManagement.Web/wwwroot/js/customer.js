(function () {

    // === 1. LẤY CÁC BIẾN DOM QUAN TRỌNG ===
    const customerTableBody = document.getElementById('customerTableBody');
    const customerCountEl = document.getElementById('customerCount');
    const searchInput = document.getElementById('searchInput');
    const selectAll = document.getElementById('selectAll');

    // Nút
    const openAddBtn = document.getElementById('openAddBtn');
    const exportBtn = document.getElementById('exportBtn');
    const importBtn = document.getElementById('importBtn');
    const importFile = document.getElementById('importFile');

    // Modal "Add"
    const addModal = document.getElementById('addModal');
    const addForm = document.getElementById('addCustomerForm');
    const addModalCloseX = document.getElementById('addModalCloseX');
    const addCancelBtn = document.getElementById('addCancelBtn');

    // Modal "Edit"
    const editModal = document.getElementById('editModal');
    const editForm = document.getElementById('editCustomerForm');
    const closeModalBtn = document.getElementById('modalCloseBtn');
    const modalDeleteBtn = document.getElementById('modalDeleteBtn');
    const saveCustomerBtn = document.getElementById('saveCustomerBtn');

    // Biến cache
    let allCustomersCache = []; // Lưu trữ data gốc từ API
    let currentEditCustomerId = null; // Lưu ID của khách hàng đang sửa

    // === 2. HÀM CHÍNH: TẢI VÀ HIỂN THỊ DỮ LIỆU ===

    // Tải khách hàng từ API khi trang được mở
    loadCustomers();

    async function loadCustomers() {
        try {
            customerTableBody.innerHTML = '<tr><td colspan="6" class="text-center">Đang tải dữ liệu...</td></tr>';

            const response = await fetch('/api/Customer');
            if (!response.ok) {
                throw new Error('Không thể tải danh sách khách hàng.');
            }

            allCustomersCache = await response.json(); // Lưu vào cache
            renderCustomerTable(allCustomersCache);

        } catch (error) {
            console.error(error);
            customerTableBody.innerHTML = '<tr><td colspan="6" class="text-center text-danger">Lỗi tải dữ liệu.</td></tr>';
        }
    }

    // Hiển thị dữ liệu lên bảng
    function renderCustomerTable(customers) {
        customerTableBody.innerHTML = ''; // Xóa bảng

        if (customers.length === 0) {
            customerTableBody.innerHTML = '<tr><td colspan="6" class="text-center">Chưa có khách hàng nào.</td></tr>';
            updateCount();
            return;
        }

        customers.forEach(customer => {
            const tr = document.createElement('tr');
            const firstName = (customer.firstName || '').trim();
            const lastName = (customer.lastName || '').trim();
            const dtoFullName = (customer.fullName || '').trim();
            const composedName = `${firstName} ${lastName}`.trim();
            const displayName = dtoFullName || composedName || (customer.email || 'Unknown');
            const appointmentCount = Number.isFinite(customer.appointmentCount)
                ? customer.appointmentCount
                : 0;
            const formattedAppointmentCount = appointmentCount.toLocaleString('vi-VN');

            // Lưu ID và data vào `dataset` để "Edit"
            tr.dataset.id = customer.id;
            tr.dataset.firstName = firstName;
            tr.dataset.lastName = lastName;
            const email = customer.email || '';
            tr.dataset.email = email;
            tr.dataset.phone = customer.phoneNumber || '';
            tr.dataset.gender = customer.gender || '';
            tr.dataset.dob = customer.dateOfBirth ? customer.dateOfBirth.split('T')[0] : '';
            tr.dataset.note = customer.internalNote || '';
            tr.dataset.fullName = displayName;

            const firstLetter = (displayName ? displayName[0] : 'U').toUpperCase();
            const colors = ['#6f42c1', '#198754', '#0d6efd', '#e83e8c', '#fd7e14'];
            const color = colors[(firstLetter.charCodeAt(0) || 0) % colors.length];

            // Định dạng ngày
            const created = formatDate(customer.createdAt);
            const lastApp = formatDate(customer.lastAppointment);

            tr.innerHTML = `
                <td><input type="checkbox" class="selectRow"></td>
                <td>
                    <div class="customer-info">
                        ${customer.avatarUrl
                    ? `<img src="${customer.avatarUrl}" class="avatar-img">`
                    : `<div class="avatar" style="background:${color}">${firstLetter}</div>`
                }
                        <div class="customer-details">
                            <div class="customer-name">${displayName}</div>
                            <div class="customer-email">${email}</div>
                        </div>
                    </div>
                </td>
                <td>${formattedAppointmentCount}</td>
                <td>${lastApp}</td>
                <td>${created}</td>
                <td>
                    <div class="actions-dropdown">
                        <button class="actions-btn" aria-expanded="false" title="Actions">
                            <i class="fas fa-ellipsis-v"></i>
                        </button>
                        <div class="actions-menu-list" role="menu">
                            <button class="actions-item edit" type="button">Edit</button>
                            <button class="actions-item delete" type="button">Delete</button>
                        </div>
                    </div>
                </td>
            `;
            customerTableBody.appendChild(tr);
        });

        updateCount();
    }

    // === 3. HÀM THÊM MỚI (POST /api/Customer) ===
    if (addForm) {
        addForm.addEventListener('submit', async function (ev) {
            ev.preventDefault();
            const submitBtn = addForm.querySelector('button[type="submit"]');
            submitBtn.disabled = true;
            submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm"></span> Adding...';

            const command = {
                firstName: document.getElementById('addFirstName').value,
                lastName: document.getElementById('addLastName').value,
                email: document.getElementById('addEmail').value,
                phoneNumber: document.getElementById('addPhone').value
            };

            try {
                const response = await fetch('/api/Customer', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(command)
                });

                if (!response.ok) {
                    const error = await response.json();
                    throw new Error(error.detail || 'Không thể tạo khách hàng.');
                }

                Swal.fire('Thành công!', 'Đã tạo khách hàng mới.', 'success');
                closeAddModal();
                loadCustomers(); // Tải lại toàn bộ bảng

            } catch (error) {
                Swal.fire('Lỗi!', error.message, 'error');
            } finally {
                submitBtn.disabled = false;
                submitBtn.innerHTML = 'Add Customer';
            }
        });
    }

    // === 4. HÀM SỬA (PUT /api/Customer/{id}) ===
    if (editForm) {
        editForm.addEventListener('submit', async function (ev) {
            ev.preventDefault();
            if (!currentEditCustomerId) return closeEditModal();

            if (saveCustomerBtn) {
                saveCustomerBtn.disabled = true;
                saveCustomerBtn.innerHTML = '<span class="spinner-border spinner-border-sm"></span> Saving...';
            }

            // Lấy dữ liệu từ CSDL đã thêm (Gender, DOB, Note)
            const command = {
                id: currentEditCustomerId,
                firstName: document.getElementById('firstNameInput').value,
                lastName: document.getElementById('lastNameInput').value,
                email: document.getElementById('emailInput').value,
                phoneNumber: document.getElementById('phoneInput').value,
                gender: document.getElementById('genderSelect').value,
                dateOfBirth: document.getElementById('dobInput').value || null,
                internalNote: document.getElementById('noteTextarea').value
            };

            try {
                const response = await fetch(`/api/Customer/${currentEditCustomerId}`, {
                    method: 'PUT',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(command)
                });

                if (!response.ok) {
                    const error = await response.json();
                    throw new Error(error.detail || 'Không thể cập nhật.');
                }

                Swal.fire('Đã lưu!', 'Thông tin khách hàng đã được cập nhật.', 'success');
                closeEditModal();
                loadCustomers(); // Tải lại toàn bộ bảng

            } catch (error) {
                Swal.fire('Lỗi!', error.message, 'error');
            } finally {
                if (saveCustomerBtn) {
                    saveCustomerBtn.disabled = false;
                    saveCustomerBtn.innerHTML = 'Save';
                }
            }
        });
    }

    // === 5. HÀM XÓA (DELETE /api/Customer/{id}) ===
    async function deleteCustomer(id) {
        // Nâng cấp `confirm` lên `Swal.fire`
        const result = await Swal.fire({
            title: 'Xóa khách hàng?',
            text: "Bạn có chắc muốn xóa vĩnh viễn khách hàng này?",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            confirmButtonText: 'Đồng ý, Xóa!',
            cancelButtonText: 'Hủy'
        });

        if (result.isConfirmed) {
            try {
                const response = await fetch(`/api/Customer/${id}`, {
                    method: 'DELETE'
                });

                if (!response.ok) {
                    const error = await response.json();
                    throw new Error(error.detail || 'Không thể xóa khách hàng.');
                }

                Swal.fire('Đã xóa!', 'Khách hàng đã bị xóa.', 'success');
                closeEditModal(); // Đóng modal edit nếu đang mở
                loadCustomers(); // Tải lại bảng

            } catch (error) {
                Swal.fire('Lỗi!', error.message, 'error');
            }
        }
    }

    // === 6. LOGIC MỞ/ĐÓNG MODAL VÀ SỰ KIỆN CLICK (Đã cập nhật) ===

    // Mở modal Edit
    function openEditModal(row) {
        if (!editModal) return;
        currentEditCustomerId = row.dataset.id; // Lấy ID

        // Điền data từ `dataset` (đã lưu khi render)
        document.getElementById('firstNameInput').value = row.dataset.firstName;
        document.getElementById('lastNameInput').value = row.dataset.lastName;
        document.getElementById('emailInput').value = row.dataset.email;
        document.getElementById('phoneInput').value = row.dataset.phone;
        document.getElementById('genderSelect').value = row.dataset.gender;
        document.getElementById('dobInput').value = row.dataset.dob;
        document.getElementById('noteTextarea').value = row.dataset.note;

        editModal.classList.add('show');
        editModal.setAttribute('aria-hidden', 'false');
    }

    function closeEditModal() {
        if (!editModal) return;
        editModal.classList.remove('show');
        editModal.setAttribute('aria-hidden', 'true');
        currentEditCustomerId = null;
    }

    // Mở modal Add
    function openAddModal() {
        if (addModal) { addModal.classList.add('show'); addModal.setAttribute('aria-hidden', 'false'); }
    }
    function closeAddModal() {
        if (addModal) { addModal.classList.remove('show'); addModal.setAttribute('aria-hidden', 'true'); }
        if (addForm) addForm.reset();
    }

    if (openAddBtn) openAddBtn.addEventListener('click', openAddModal);
    if (addModalCloseX) addModalCloseX.addEventListener('click', closeAddModal);
    if (addCancelBtn) addCancelBtn.addEventListener('click', closeAddModal);
    if (addModal) addModal.addEventListener('click', function (e) { if (e.target === addModal) closeAddModal(); });
    if (closeModalBtn) closeModalBtn.addEventListener('click', closeEditModal);

    // Xóa từ modal Edit
    if (modalDeleteBtn) {
        modalDeleteBtn.addEventListener('click', function () {
            if (currentEditCustomerId) {
                deleteCustomer(currentEditCustomerId);
            }
        });
    }

    // Xử lý nút Sửa/Xóa trên hàng
    document.addEventListener('click', function (e) {
        const btn = e.target.closest('.actions-btn');
        if (btn) {
            // (Code mở/đóng dropdown action)
            const menu = btn.parentElement.querySelector('.actions-menu-list');
            const isShown = menu.classList.contains('show');
            closeAllActionMenus();
            if (!isShown) {
                menu.classList.add('show');
                btn.classList.add('active');
            }
            e.stopPropagation();
            return;
        }

        const actionItem = e.target.closest('.actions-item');
        if (actionItem) {
            const row = actionItem.closest('tr');
            if (actionItem.classList.contains('edit')) {
                closeAllActionMenus();
                openEditModal(row); // Mở modal edit
            } else if (actionItem.classList.contains('delete')) {
                deleteCustomer(row.dataset.id); // Gọi hàm Xóa API
            }
            e.stopPropagation();
            return;
        }

        closeAllActionMenus();
    });

    // === 7. CÁC HÀM CŨ (Giữ nguyên) ===

    // Đếm số hàng (vẫn cần cho Search)
    function updateCount() {
        const tbody = document.getElementById('customerTableBody');
        const countEl = document.getElementById('customerCount');
        if (tbody && countEl) {
            const rows = Array.from(tbody.querySelectorAll('tr'))
                .filter(r => r.style.display !== 'none').length;
            countEl.textContent = rows;
        }
    }

    // Checkbox All
    if (selectAll) {
        selectAll.addEventListener('change', function () {
            const checked = this.checked;
            document.querySelectorAll('.selectRow').forEach(cb => cb.checked = checked);
        });
    }

    // Search (Client-side)
    // Dùng data từ cache thay vì DOM
    function filterRows() {
        const q = (searchInput && searchInput.value || '').trim().toLowerCase();

        if (!q) {
            renderCustomerTable(allCustomersCache); // Hiển thị lại tất cả
            return;
        }

        const filtered = allCustomersCache.filter(c => {
            const fullName = (c.fullName || '').toLowerCase();
            const firstName = (c.firstName || '').toLowerCase();
            const lastName = (c.lastName || '').toLowerCase();
            const email = (c.email || '').toLowerCase();
            const phone = (c.phoneNumber || '').toLowerCase();
            const composed = `${firstName} ${lastName}`.trim();
            return (
                fullName.includes(q) ||
                composed.includes(q) ||
                email.includes(q) ||
                phone.includes(q)
            );
        });

        renderCustomerTable(filtered); // Hiển thị kết quả lọc
    }
    if (searchInput) {
        searchInput.addEventListener('input', filterRows);
    }

    function closeAllActionMenus() {
        document.querySelectorAll('.actions-menu-list.show').forEach(m => m.classList.remove('show'));
        document.querySelectorAll('.actions-btn.active').forEach(b => b.classList.remove('active'));
    }

    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') {
            closeAllActionMenus();
            closeEditModal();
            closeAddModal();
        }
    });

    function formatDate(value) {
        if (!value) return '';
        const date = new Date(value);
        if (Number.isNaN(date.getTime())) return '';
        return date.toLocaleDateString('en-US', { month: 'long', day: 'numeric', year: 'numeric' });
    }

    // (Code Export/Import CSV giữ nguyên)
    // ...
    // ... (Toàn bộ code export/import của bạn có thể dán vào đây) ...
    // ...

})();
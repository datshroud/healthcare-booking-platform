(function () {

    function updateCount() {
        const tbody = document.getElementById('customerTableBody');
        const countEl = document.getElementById('customerCount');
        if (tbody && countEl) {
            // count only visible rows (not display:none)
            const rows = Array.from(tbody.querySelectorAll('tr'))
                .filter(r => r.style.display !== 'none').length;
            countEl.textContent = rows;
        }
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', updateCount);
    } else {
        updateCount();
    }


    const selectAll = document.getElementById('selectAll');
    if (selectAll) {
        selectAll.addEventListener('change', function () {
            const checked = this.checked;
            document.querySelectorAll('.selectRow').forEach(cb => cb.checked = checked);
        });
    }

    // --- Search/filter ---
    const searchInput = document.getElementById('searchInput');
    function filterRows() {
        const q = (searchInput && searchInput.value || '').trim().toLowerCase();
        const tbody = document.getElementById('customerTableBody');
        if (!tbody) return;
        const rows = Array.from(tbody.querySelectorAll('tr'));
        if (!q) {
            rows.forEach(r => r.style.display = '');
            updateCount();
            return;
        }
        rows.forEach(r => {
            const nameEl = r.querySelector('.customer-name');
            const emailEl = r.querySelector('.customer-email');
            const name = nameEl ? nameEl.textContent.toLowerCase() : '';
            const email = emailEl ? emailEl.textContent.toLowerCase() : '';
            const rest = Array.from(r.querySelectorAll('td')).slice(2).map(td => td.textContent.toLowerCase()).join(' ');
            const match = name.includes(q) || email.includes(q) || rest.includes(q);
            r.style.display = match ? '' : 'none';
        });
        updateCount();
    }
    if (searchInput) {
        searchInput.addEventListener('input', filterRows);
    }
    function closeAllActionMenus() {
        document.querySelectorAll('.actions-menu-list.show').forEach(m => m.classList.remove('show'));
        document.querySelectorAll('.actions-btn.active').forEach(b => b.classList.remove('active'));
    }


    let currentEditRow = null;
    const editModal = document.getElementById('editModal');
    const firstNameInput = document.getElementById('firstNameInput');
    const lastNameInput = document.getElementById('lastNameInput');
    const emailInput = document.getElementById('emailInput');
    const phoneInput = document.getElementById('phoneInput');
    const genderSelect = document.getElementById('genderSelect');
    const dobInput = document.getElementById('dobInput');
    const noteTextarea = document.getElementById('noteTextarea');
    const closeModalBtn = document.getElementById('modalCloseBtn');
    const saveCustomerBtn = document.getElementById('saveCustomerBtn');
    const modalDeleteBtn = document.getElementById('modalDeleteBtn');

    function openEditModal(row) {
        currentEditRow = row;
        const nameEl = row.querySelector('.customer-name');
        const emailEl = row.querySelector('.customer-email');
        const fullName = nameEl ? nameEl.textContent.trim() : '';
        const parts = fullName.split(' ');
        const first = parts.shift() || '';
        const last = parts.join(' ') || '';
        firstNameInput.value = first;
        lastNameInput.value = last;
        emailInput.value = emailEl ? emailEl.textContent.trim() : '';

        phoneInput.value = row.dataset.phone || '';
        genderSelect.value = row.dataset.gender || '';
        dobInput.value = row.dataset.dob || '';
        noteTextarea.value = row.dataset.note || '';
        editModal.classList.add('show');
        editModal.setAttribute('aria-hidden', 'false');
    }

    function closeEditModal() {
        editModal.classList.remove('show');
        editModal.setAttribute('aria-hidden', 'true');
        currentEditRow = null;
    }


    document.addEventListener('click', function (e) {
        const btn = e.target.closest('.actions-btn');
        if (btn) {
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
                openEditModal(row);
            } else if (actionItem.classList.contains('delete')) {
                if (confirm('Delete this customer?')) {
                    row.remove();
                    updateCount();
                }
            }
            e.stopPropagation();
            return;
        }


        closeAllActionMenus();
    });


    if (closeModalBtn) closeModalBtn.addEventListener('click', closeEditModal);


    const editForm = document.getElementById('editCustomerForm');
    if (editForm) {
        editForm.addEventListener('submit', function (ev) {
            ev.preventDefault();
            if (!currentEditRow) return closeEditModal();
            const fullName = (firstNameInput.value || '') + (lastNameInput.value ? ' ' + lastNameInput.value : '');
            const nameEl = currentEditRow.querySelector('.customer-name');
            const emailEl = currentEditRow.querySelector('.customer-email');
            if (nameEl) nameEl.textContent = fullName || 'Unknown';
            if (emailEl) emailEl.textContent = emailInput.value || '';

            currentEditRow.dataset.phone = phoneInput.value || '';
            currentEditRow.dataset.gender = genderSelect.value || '';
            currentEditRow.dataset.dob = dobInput.value || '';
            currentEditRow.dataset.note = noteTextarea.value || '';
            updateCount();
            closeEditModal();
        });
    }

    // Delete from modal
    if (modalDeleteBtn) {
        modalDeleteBtn.addEventListener('click', function () {
            if (!currentEditRow) return;
            if (confirm('Delete this customer?')) {
                currentEditRow.remove();
                updateCount();
                closeEditModal();
            }
        });
    }


    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') {
            closeAllActionMenus();
            closeEditModal();
            // close add modal too
            const addModal = document.getElementById('addModal');
            if (addModal) addModal.classList.remove('show');
        }
    });

    // --- Add Customer modal behavior ---
    const openAddBtn = document.getElementById('openAddBtn');
    const addModal = document.getElementById('addModal');
    const addModalCloseX = document.getElementById('addModalCloseX');
    const addCancelBtn = document.getElementById('addCancelBtn');
    const addForm = document.getElementById('addCustomerForm');
    const addFirstName = document.getElementById('addFirstName');
    const addLastName = document.getElementById('addLastName');
    const addEmail = document.getElementById('addEmail');
    const addPhone = document.getElementById('addPhone');

    function openAddModal() {
        if (addModal) { addModal.classList.add('show'); addModal.setAttribute('aria-hidden', 'false'); }
    }
    function closeAddModal() {
        if (addModal) { addModal.classList.remove('show'); addModal.setAttribute('aria-hidden', 'true'); }
        if (addForm) addForm.reset();
    }

    if (openAddBtn) openAddBtn.addEventListener('click', function () { openAddModal(); });
    if (addModalCloseX) addModalCloseX.addEventListener('click', closeAddModal);
    if (addCancelBtn) addCancelBtn.addEventListener('click', closeAddModal);

    // close add modal when clicking overlay background
    if (addModal) {
        addModal.addEventListener('click', function (e) { if (e.target === addModal) closeAddModal(); });
    }

    // On submit create a new row and append
    if (addForm) {
        addForm.addEventListener('submit', function (ev) {
            ev.preventDefault();
            const f = (addFirstName.value || '').trim();
            const l = (addLastName.value || '').trim();
            const full = (f + (l ? ' ' + l : '')).trim() || 'Unknown';
            const email = (addEmail.value || '').trim();
            const phone = (addPhone.value || '').trim();
            const initials = (f[0] || l[0] || 'U').toUpperCase();
            // pick a color by char code
            const colors = ['#6f42c1', '#198754', '#0d6efd', '#e83e8c', '#fd7e14'];
            const color = colors[(initials.charCodeAt(0) || 0) % colors.length];
            const created = new Date().toLocaleDateString('en-US', { month: 'long', day: 'numeric', year: 'numeric' });

            const tbody = document.getElementById('customerTableBody');
            if (!tbody) return closeAddModal();

            const tr = document.createElement('tr');
            tr.innerHTML = `
                        <td>
                            <input type="checkbox" class="selectRow">
                        </td>
                        <td>
                            <div class="customer-info">
                                <div class="avatar" style="background:${color}">${initials}</div>
                                <div class="customer-details">
                                    <div class="customer-name">${full}</div>
                                    <div class="customer-email">${email}</div>
                                </div>
                            </div>
                        </td>
                        <td>0</td>
                        <td></td>
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
            // store phone on row
            tr.dataset.phone = phone;
            tbody.appendChild(tr);
            updateCount();
            closeAddModal();
        });
    }

    // --- Export / Import CSV ---
    const exportBtn = document.getElementById('exportBtn');
    const importBtn = document.getElementById('importBtn');
    const importFile = document.getElementById('importFile');

    function csvEscape(val) {
        if (val == null) return '';
        const s = String(val);
        if (s.includes('"') || s.includes(',') || s.includes('\n')) {
            return '"' + s.replace(/"/g, '""') + '"';
        }
        return s;
    }

    function getTableData() {
        const tbody = document.getElementById('customerTableBody');
        if (!tbody) return [];
        const rows = Array.from(tbody.querySelectorAll('tr'));
        return rows.map(r => {
            const nameEl = r.querySelector('.customer-name');
            const emailEl = r.querySelector('.customer-email');
            const tds = r.querySelectorAll('td');
            const appointments = tds[2] ? tds[2].textContent.trim() : '';
            const lastApp = tds[3] ? tds[3].textContent.trim() : '';
            const created = tds[4] ? tds[4].textContent.trim() : '';
            const full = nameEl ? nameEl.textContent.trim() : '';
            const parts = full.split(' ');
            const first = parts.shift() || '';
            const last = parts.join(' ') || '';
            return {
                FirstName: first,
                LastName: last,
                Email: emailEl ? emailEl.textContent.trim() : '',
                Phone: r.dataset.phone || '',
                Appointments: appointments,
                LastAppointment: lastApp,
                Created: created
            };
        });
    }

    function downloadCSV(filename, text) {
        const blob = new Blob([text], { type: 'text/csv;charset=utf-8;' });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = filename;
        document.body.appendChild(a);
        a.click();
        setTimeout(() => { URL.revokeObjectURL(url); a.remove(); }, 100);
    }

    function exportCSV() {
        const data = getTableData();
        const headers = ['FirstName', 'LastName', 'Email', 'Phone', 'Appointments', 'LastAppointment', 'Created'];
        const lines = [headers.join(',')];
        data.forEach(row => {
            const vals = headers.map(h => csvEscape(row[h]));
            lines.push(vals.join(','));
        });
        downloadCSV('customers.csv', lines.join('\n'));
    }

    // basic CSV parse that handles quoted fields
    function parseCSV(text) {
        const rows = [];
        let cur = '';
        let inQuotes = false;
        const row = [];
        for (let i = 0; i < text.length; i++) {
            const ch = text[i];
            const next = text[i + 1];
            if (inQuotes) {
                if (ch === '"' && next === '"') {
                    cur += '"'; i++; // escaped quote
                } else if (ch === '"') {
                    inQuotes = false;
                } else {
                    cur += ch;
                }
            } else {
                if (ch === '"') {
                    inQuotes = true;
                } else if (ch === ',') {
                    row.push(cur); cur = '';
                } else if (ch === '\r') {
                    continue;
                } else if (ch === '\n') {
                    row.push(cur); rows.push(row.slice()); row.length = 0; cur = '';
                } else {
                    cur += ch;
                }
            }
        }
        // last field
        if (cur !== '' || inQuotes || row.length) row.push(cur);
        if (row.length) rows.push(row.slice());
        return rows;
    }

    function importCSVText(text) {
        const raw = parseCSV(text);
        if (raw.length === 0) return 0;
        const header = raw[0].map(h => (h || '').trim());
        const idx = name => header.findIndex(h => h.toLowerCase() === name.toLowerCase());
        const iFirst = idx('firstname');
        const iLast = idx('lastname');
        const iName = idx('name');
        const iEmail = idx('email');
        const iPhone = idx('phone');
        const iAppointments = idx('appointments');
        const iLastApp = idx('lastappointment');
        const iCreated = idx('created');
        const tbody = document.getElementById('customerTableBody');
        if (!tbody) return 0;
        const colors = ['#6f42c1', '#198754', '#0d6efd', '#e83e8c', '#fd7e14'];
        let added = 0;
        for (let r = 1; r < raw.length; r++) {
            const cols = raw[r];
            if (cols.length === 1 && cols[0].trim() === '') continue;
            let first = '';
            let last = '';
            if (iName >= 0) {
                const nv = cols[iName] || '';
                const parts = nv.trim().split(' ');
                first = parts.shift() || '';
                last = parts.join(' ') || '';
            } else {
                if (iFirst >= 0) first = cols[iFirst] || '';
                if (iLast >= 0) last = cols[iLast] || '';
            }
            const email = iEmail >= 0 ? (cols[iEmail] || '') : '';
            const phone = iPhone >= 0 ? (cols[iPhone] || '') : '';
            const appointments = iAppointments >= 0 ? (cols[iAppointments] || '0') : '0';
            const lastApp = iLastApp >= 0 ? (cols[iLastApp] || '') : '';
            const created = iCreated >= 0 ? (cols[iCreated] || new Date().toLocaleDateString('en-US')) : new Date().toLocaleDateString('en-US');
            const initials = (first[0] || last[0] || 'U').toUpperCase();
            const color = colors[(initials.charCodeAt(0) || 0) % colors.length];
            const tr = document.createElement('tr');
            tr.innerHTML = `
                        <td>
                            <input type="checkbox" class="selectRow">
                        </td>
                        <td>
                            <div class="customer-info">
                                <div class="avatar" style="background:${color}">${initials}</div>
                                <div class="customer-details">
                                    <div class="customer-name">${(first + (last ? ' ' + last : '')).trim()}</div>
                                    <div class="customer-email">${(email || '').trim()}</div>
                                </div>
                            </div>
                        </td>
                        <td>${appointments}</td>
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
            tr.dataset.phone = phone || '';
            tbody.appendChild(tr);
            added++;
        }
        if (added) updateCount();
        return added;
    }

    if (exportBtn) exportBtn.addEventListener('click', function () { exportCSV(); });
    if (importBtn) {
        importBtn.addEventListener('click', function () { if (importFile) importFile.click(); });
    }
    if (importFile) {
        importFile.addEventListener('change', function (e) {
            const file = this.files && this.files[0];
            if (!file) return;
            const reader = new FileReader();
            reader.onload = function (ev) {
                try {
                    const text = String(ev.target.result || '');
                    const added = importCSVText(text);
                    alert('Imported ' + added + ' customers');
                } catch (err) {
                    alert('Failed to import CSV: ' + err.message);
                }
            };
            reader.readAsText(file, 'utf-8');
            // reset input so same file can be selected again
            this.value = '';
        });
    }

})();
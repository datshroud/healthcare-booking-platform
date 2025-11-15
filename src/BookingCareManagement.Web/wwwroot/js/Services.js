document.addEventListener('DOMContentLoaded', () => {
    const dom = {
        employeeFilter: document.getElementById('employeeFilter'),
        employeeFilterLabel: document.querySelector('#employeeFilter span'),
        employeeMenu: document.getElementById('employeeMenu'),
        doctorPlaceholder: document.querySelector('[data-role="doctor-placeholder"]'),
        sortFilter: document.getElementById('sortFilter'),
        sortFilterLabel: document.querySelector('#sortFilter span'),
        sortMenu: document.getElementById('sortMenu'),
        searchInput: document.getElementById('searchInput'),
        servicesGrid: document.getElementById('servicesGrid'),
        servicesLoading: document.getElementById('servicesLoading'),
        servicesEmpty: document.getElementById('servicesEmpty'),
        modalElement: document.getElementById('serviceDetailModal'),
        modalTitle: document.getElementById('serviceModalTitle'),
        modalSubtitle: document.getElementById('serviceModalSubtitle'),
        modalDuration: document.getElementById('serviceModalDuration'),
        modalPrice: document.getElementById('serviceModalPrice'),
        modalDescription: document.getElementById('serviceModalDescription'),
        modalImage: document.getElementById('serviceModalImage'),
        modalDoctorCount: document.getElementById('serviceModalDoctorCount'),
        modalDoctors: document.getElementById('serviceModalDoctors'),
        modalBookBtn: document.getElementById('serviceModalBookBtn')
    };

    const modalInstance = window.bootstrap?.Modal ? new window.bootstrap.Modal(dom.modalElement) : null;
    const apiBase = '/api/customer-booking';

    const state = {
        specialties: [],
        filtered: [],
        selectedDoctorId: '',
        selectedSort: 'name-asc',
        selectedSpecialty: null
    };

    const currency = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND', minimumFractionDigits: 0 });

    const formatPrice = (price) => {
        if (typeof price === 'number' && !Number.isNaN(price)) {
            return currency.format(price);
        }

        return 'Liên hệ';
    };

    const formatDuration = (minutes) => {
        if (!minutes || Number.isNaN(minutes)) {
            return '30 phút';
        }

        if (minutes % 60 === 0) {
            const hours = minutes / 60;
            return hours === 1 ? '1 giờ' : `${hours} giờ`;
        }

        return `${minutes} phút`;
    };

    const getInitials = (value) => {
        if (!value) {
            return 'Dr';
        }

        const parts = value.trim().split(/\s+/);
        if (parts.length === 1) {
            return parts[0].slice(0, 2).toUpperCase();
        }

        return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
    };

    const toPlainText = (value) => {
        if (!value) {
            return '';
        }

        const temp = document.createElement('div');
        temp.innerHTML = value;
        return temp.textContent || temp.innerText || '';
    };

    const truncate = (value, max = 110) => {
        const text = toPlainText(value);
        if (text.length <= max) {
            return text;
        }

        return `${text.slice(0, max)}...`;
    };

    const toggleMenu = (menu, button, show) => {
        if (!menu || !button) {
            return;
        }

        const shouldShow = typeof show === 'boolean' ? show : !menu.classList.contains('show');
        if (shouldShow) {
            menu.classList.add('show');
            button.classList.add('active');
        } else {
            menu.classList.remove('show');
            button.classList.remove('active');
        }
    };

    const closeAllMenus = () => {
        toggleMenu(dom.employeeMenu, dom.employeeFilter, false);
        toggleMenu(dom.sortMenu, dom.sortFilter, false);
    };

    const fetchJson = async (url) => {
        const response = await fetch(url, { headers: { 'Content-Type': 'application/json' } });
        if (!response.ok) {
            const payload = await response.json().catch(() => null);
            const title = payload?.title || payload?.detail || 'Đã có lỗi xảy ra';
            throw new Error(title);
        }

        return response.json();
    };

    const createAvatarStack = (doctors) => {
        const wrapper = document.createElement('div');
        wrapper.className = 'provider-avatars';

        const group = doctors.slice(0, 4);
        if (group.length === 0) {
            const empty = document.createElement('span');
            empty.className = 'text-secondary small';
            empty.textContent = 'Chưa có bác sĩ nào';
            wrapper.appendChild(empty);
            return wrapper;
        }

        group.forEach((doctor) => {
            const avatar = document.createElement('div');
            avatar.className = 'provider-avatar';
            avatar.title = doctor.fullName;

            if (doctor.avatarUrl) {
                const image = document.createElement('img');
                image.src = doctor.avatarUrl;
                image.alt = doctor.fullName;
                avatar.appendChild(image);
            } else {
                avatar.textContent = getInitials(doctor.fullName);
            }

            wrapper.appendChild(avatar);
        });

        if (doctors.length > group.length) {
            const more = document.createElement('div');
            more.className = 'provider-avatar';
            more.textContent = `+${doctors.length - group.length}`;
            wrapper.appendChild(more);
        }

        return wrapper;
    };

    const createServiceCard = (specialty) => {
        const card = document.createElement('div');
        card.className = 'service-card';

        const imageWrapper = document.createElement('div');
        imageWrapper.className = 'service-image';

        const image = document.createElement('img');
        image.src = specialty.imageUrl || 'https://placehold.co/800x600?text=Specialty';
        image.alt = specialty.name;
        imageWrapper.appendChild(image);

        const priceBadge = document.createElement('span');
        priceBadge.className = 'service-price';
        priceBadge.textContent = formatPrice(specialty.price);
        imageWrapper.appendChild(priceBadge);

        const content = document.createElement('div');
        content.className = 'service-content';

        const title = document.createElement('h3');
        title.className = 'service-name';
        title.textContent = specialty.name;

        const blurb = document.createElement('p');
        blurb.className = 'service-blurb';
        blurb.textContent = truncate(specialty.description);

        const meta = document.createElement('div');
        meta.className = 'service-meta';

        const durationMeta = document.createElement('span');
        durationMeta.className = 'meta-item';
        durationMeta.innerHTML = `<span class="meta-label">Thời lượng</span><span class="meta-value">${formatDuration(specialty.durationMinutes)}</span>`;

        const doctorMeta = document.createElement('span');
        doctorMeta.className = 'meta-item';
        const doctorLabel = specialty.doctors?.length ? `${specialty.doctors.length} bác sĩ` : 'Đang cập nhật';
        doctorMeta.innerHTML = `<span class="meta-label">Bác sĩ</span><span class="meta-value">${doctorLabel}</span>`;

        meta.append(durationMeta, doctorMeta);

        const avatars = createAvatarStack(specialty.doctors || []);

        const actions = document.createElement('div');
        actions.className = 'service-actions';

        const learnBtn = document.createElement('button');
        learnBtn.className = 'btn-outline';
        learnBtn.type = 'button';
        learnBtn.textContent = 'Tìm hiểu thêm';
        learnBtn.addEventListener('click', () => openModal(specialty));

        const bookBtn = document.createElement('button');
        bookBtn.className = 'btn-primary';
        bookBtn.type = 'button';
        bookBtn.textContent = 'Đặt lịch ngay';
        bookBtn.addEventListener('click', () => navigateToBooking(specialty.id));

        actions.append(learnBtn, bookBtn);

        content.append(title, blurb, meta, avatars, actions);
        card.append(imageWrapper, content);
        return card;
    };

    const renderServices = (specialties) => {
        dom.servicesGrid.innerHTML = '';

        if (!specialties.length) {
            dom.servicesGrid.classList.add('d-none');
            dom.servicesEmpty.classList.remove('d-none');
            return;
        }

        dom.servicesEmpty.classList.add('d-none');
        dom.servicesGrid.classList.remove('d-none');

        const fragment = document.createDocumentFragment();
        specialties.forEach((specialty) => fragment.appendChild(createServiceCard(specialty)));
        dom.servicesGrid.appendChild(fragment);
    };

    const sortSpecialties = (items) => {
        const copy = [...items];
        switch (state.selectedSort) {
            case 'price-asc':
                return copy.sort((a, b) => (typeof a.price === 'number' ? a.price : Number.POSITIVE_INFINITY) - (typeof b.price === 'number' ? b.price : Number.POSITIVE_INFINITY));
            case 'price-desc':
                return copy.sort((a, b) => (typeof b.price === 'number' ? b.price : Number.NEGATIVE_INFINITY) - (typeof a.price === 'number' ? a.price : Number.NEGATIVE_INFINITY));
            case 'name-desc':
                return copy.sort((a, b) => b.name.localeCompare(a.name, 'vi-VN'));
            case 'name-asc':
            default:
                return copy.sort((a, b) => a.name.localeCompare(b.name, 'vi-VN'));
        }
    };

    const applyFilters = () => {
        const keyword = dom.searchInput?.value.trim().toLowerCase() || '';
        let filtered = [...state.specialties];

        if (state.selectedDoctorId) {
            filtered = filtered.filter((sp) => sp.doctors?.some((doc) => doc.id === state.selectedDoctorId));
        }

        if (keyword) {
            filtered = filtered.filter((sp) => {
                const name = sp.name?.toLowerCase() || '';
                const desc = sp.description?.toLowerCase() || '';
                return name.includes(keyword) || desc.includes(keyword);
            });
        }

        state.filtered = sortSpecialties(filtered);
        renderServices(state.filtered);
    };

    const populateDoctorFilter = () => {
        if (!dom.employeeMenu) {
            return;
        }

        const seen = new Map();
        state.specialties.forEach((sp) => {
            (sp.doctors || []).forEach((doc) => {
                if (!seen.has(doc.id)) {
                    seen.set(doc.id, doc.fullName);
                }
            });
        });

        const placeholder = dom.doctorPlaceholder;
        if (placeholder) {
            placeholder.remove();
        }

        if (!seen.size) {
            const empty = document.createElement('div');
            empty.className = 'dropdown-placeholder';
            empty.textContent = 'Chưa có bác sĩ nào';
            dom.employeeMenu.appendChild(empty);
            return;
        }

        const entries = Array.from(seen.entries()).sort((a, b) => a[1].localeCompare(b[1], 'vi-VN'));
        entries.forEach(([id, fullName]) => {
            const button = document.createElement('button');
            button.type = 'button';
            button.className = 'dropdown-item-custom';
            button.dataset.doctorId = id;
            button.textContent = fullName;
            dom.employeeMenu.appendChild(button);
        });
    };

    const openModal = (specialty) => {
        state.selectedSpecialty = specialty;

        dom.modalTitle.textContent = specialty.name;
        dom.modalSubtitle.textContent = specialty.description ? truncate(specialty.description, 150) : 'Thông tin chi tiết về chuyên khoa';
        dom.modalDuration.textContent = formatDuration(specialty.durationMinutes);
        dom.modalPrice.textContent = formatPrice(specialty.price);
        dom.modalDescription.innerHTML = specialty.description || 'Nội dung đang được cập nhật.';
        dom.modalImage.src = specialty.imageUrl || 'https://placehold.co/800x600?text=Specialty';
        dom.modalImage.alt = specialty.name;

        const doctors = specialty.doctors || [];
        dom.modalDoctorCount.textContent = doctors.length ? `${doctors.length} bác sĩ` : 'Chưa có bác sĩ được gán';
        dom.modalDoctors.innerHTML = '';

        if (!doctors.length) {
            const empty = document.createElement('div');
            empty.className = 'text-secondary';
            empty.textContent = 'Chúng tôi sẽ sớm cập nhật bác sĩ cho chuyên khoa này.';
            dom.modalDoctors.appendChild(empty);
        } else {
            const fragment = document.createDocumentFragment();
            doctors.forEach((doctor) => {
                const card = document.createElement('div');
                card.className = 'provider-card';

                if (doctor.avatarUrl) {
                    const img = document.createElement('img');
                    img.src = doctor.avatarUrl;
                    img.alt = doctor.fullName;
                    card.appendChild(img);
                } else {
                    const initials = document.createElement('div');
                    initials.className = 'provider-initials';
                    initials.textContent = getInitials(doctor.fullName);
                    card.appendChild(initials);
                }

                const name = document.createElement('span');
                name.textContent = doctor.fullName;
                card.appendChild(name);

                fragment.appendChild(card);
            });

            dom.modalDoctors.appendChild(fragment);
        }

        modalInstance?.show();
    };

    const navigateToBooking = (specialtyId) => {
        if (!specialtyId) {
            return;
        }

        const target = new URL('/booking', window.location.origin);
        target.searchParams.set('specialtyId', specialtyId);
        window.location.href = target.toString();
    };

    const handleDoctorFilterClick = (target) => {
        const doctorId = target.dataset.doctorId ?? '';
        state.selectedDoctorId = doctorId;
        if (dom.employeeFilterLabel) {
            dom.employeeFilterLabel.textContent = doctorId ? target.textContent.trim() : 'Tất cả bác sĩ';
        }
        closeAllMenus();
        applyFilters();
    };

    const handleSortClick = (target) => {
        const sort = target.dataset.sort || 'name-asc';
        state.selectedSort = sort;
        if (dom.sortFilterLabel) {
            dom.sortFilterLabel.textContent = target.textContent.trim();
        }
        closeAllMenus();
        applyFilters();
    };

    const handleDropdownClick = (event) => {
        const doctorItem = event.target.closest('#employeeMenu .dropdown-item-custom');
        if (doctorItem) {
            event.preventDefault();
            handleDoctorFilterClick(doctorItem);
            return;
        }

        const sortItem = event.target.closest('#sortMenu .dropdown-item-custom');
        if (sortItem) {
            event.preventDefault();
            handleSortClick(sortItem);
        }
    };

    const loadSpecialties = async () => {
        dom.servicesLoading.classList.remove('d-none');
        dom.servicesGrid.classList.add('d-none');
        dom.servicesEmpty.classList.add('d-none');

        try {
            const data = await fetchJson(`${apiBase}/specialties`);
            state.specialties = data ?? [];
            populateDoctorFilter();
            applyFilters();
        } catch (error) {
            dom.servicesEmpty.classList.remove('d-none');
            const emptyText = dom.servicesEmpty.querySelector('p');
            if (emptyText) {
                emptyText.textContent = error.message;
            }
        } finally {
            dom.servicesLoading.classList.add('d-none');
        }
    };

    dom.employeeFilter?.addEventListener('click', (event) => {
        event.stopPropagation();
        toggleMenu(dom.employeeMenu, dom.employeeFilter);
        toggleMenu(dom.sortMenu, dom.sortFilter, false);
    });

    dom.sortFilter?.addEventListener('click', (event) => {
        event.stopPropagation();
        toggleMenu(dom.sortMenu, dom.sortFilter);
        toggleMenu(dom.employeeMenu, dom.employeeFilter, false);
    });

    document.addEventListener('click', (event) => {
        if (!dom.employeeMenu?.contains(event.target) && !dom.employeeFilter?.contains(event.target) && !dom.sortMenu?.contains(event.target) && !dom.sortFilter?.contains(event.target)) {
            closeAllMenus();
        }
    });

    dom.employeeMenu?.addEventListener('click', handleDropdownClick);
    dom.sortMenu?.addEventListener('click', handleDropdownClick);

    dom.searchInput?.addEventListener('input', () => {
        applyFilters();
    });

    dom.modalBookBtn?.addEventListener('click', () => {
        if (state.selectedSpecialty) {
            navigateToBooking(state.selectedSpecialty.id);
        }
    });

    loadSpecialties();
});
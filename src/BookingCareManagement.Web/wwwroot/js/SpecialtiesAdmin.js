const DEFAULT_SPECIALTY_COLOR = "#1a73e8";
let specialtyEditorInstance;
let editingSpecialtyId = null;
let editingSpecialtyImageUrl = null; // Dùng để lưu URL khi Sửa

const addSpecialtyModal = document.getElementById("addSpecialtyModal");
const addSpecialtyModalTitle = document.getElementById("addSpecialtyModalLabel");
const defaultSpecialtyModalTitle = addSpecialtyModalTitle ? addSpecialtyModalTitle.textContent : "Thêm chuyên khoa";
const specialtyInput = document.getElementById("specialtyImageInput"); // Input file
const specialtyUploadBox = document.getElementById("specialtyUploadBox"); // Khu vực xem trước
const originalSpecialtyUploadHTML = specialtyUploadBox ? specialtyUploadBox.innerHTML : "";
let specialtiesRequestController = null;
let isSavingSpecialty = false;
const specialtyColorInput = document.getElementById("specialtyColor");
const specialtyColorPicker = document.getElementById("specialtyColorPicker");
const specialtyDoctorsSelect = document.getElementById("specialtyDoctors");

const specialtyListContainer = document.getElementById("specialty-list-container");
const specialtyCountSpan = document.getElementById("specialty-count");
const btnSaveSpecialty = document.getElementById("btn-save-specialty");
const defaultSaveSpecialtyText = btnSaveSpecialty ? btnSaveSpecialty.textContent : "Thêm chuyên khoa";
const specialtyNameInput = document.getElementById("specialtyName");
const specialtySlugInput = document.getElementById("specialtySlug");
const addSpecialtyForm = document.getElementById("add-specialty-form");
const specialtyPriceInput = document.getElementById("specialtyPrice");

const specialtyColorSwatch = document.querySelector("#specialtyColor")?.closest(".color-picker-wrapper")?.querySelector(".color-swatch");

const currencyFormatter = new Intl.NumberFormat("vi-VN", {
    style: "currency",
    currency: "VND",
    maximumFractionDigits: 0
});

let doctorsCache = [];
let specialtiesCache = [];

if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", boot);
} else {
    boot();
}

function boot() {
    setColorValue(DEFAULT_SPECIALTY_COLOR);
    loadSpecialties();
    wireModalLifecycle();
    wireFileUploadPreview();
    wireColorInputs();
    wireDoctorSelection();
    wireSaveSpecialty();
    wireListActions();
}

async function loadSpecialties() {
    // ... (Giữ nguyên) ...
    if (!specialtyListContainer) {
        return;
    }
    specialtiesRequestController?.abort();
    specialtiesRequestController = new AbortController();
    specialtyListContainer.innerHTML = '<div class="p-3 text-center text-muted">Đang tải chuyên khoa...</div>';
    try {
        const response = await fetch("/api/Specialty", { signal: specialtiesRequestController.signal });
        if (!response.ok) {
            throw new Error("Không thể tải chuyên khoa.");
        }
        const payload = await response.json();
        specialtiesCache = dedupeSpecialties(payload);
        renderSpecialtyList(specialtiesCache);
    } catch (error) {
        if (error.name === "AbortError") {
            return;
        }
        console.error(error);
        specialtyListContainer.innerHTML = '<p class="text-danger p-3">Lỗi tải chuyên khoa. Vui lòng thử lại.</p>';
    } finally {
        specialtiesRequestController = null;
    }
}

async function ensureDoctorsLoaded(currentDoctorIds = new Set()) {
    // ... (Giữ nguyên) ...
    if (doctorsCache.length > 0) {
        populateDoctorOptions(currentDoctorIds);
        return doctorsCache;
    }
    try {
        const response = await fetch("/api/Doctor");
        if (!response.ok) {
            throw new Error("Không thể tải danh sách bác sĩ.");
        }
        doctorsCache = await response.json();
        populateDoctorOptions(currentDoctorIds);
        return doctorsCache;
    } catch (error) {
        console.error(error);
        Swal.fire("Lỗi", error.message, "error");
        return [];
    }
}

function populateDoctorOptions(currentDoctorIds = new Set()) {
    // ... (Giữ nguyên) ...
    if (!specialtyDoctorsSelect) {
        return;
    }
    specialtyDoctorsSelect.innerHTML = "";
    doctorsCache
        .filter((doc) => {
            if (doc.active === false) {
                return false;
            }
            const doctorHasSpecialty = doc.specialties && doc.specialties.length > 0;
            const belongsToThisSpecialty = currentDoctorIds.has(doc.id);
            return !doctorHasSpecialty || belongsToThisSpecialty;
        })
        .forEach((doc) => {
            const option = document.createElement("option");
            option.value = doc.id;
            const fullName = doc.fullName?.trim() || `${doc.firstName ?? ""} ${doc.lastName ?? ""}`.trim() || doc.email || "Bác sĩ";
            option.textContent = fullName;
            specialtyDoctorsSelect.appendChild(option);
        });
}


function renderSpecialtyList(items) {
    // ... (Giữ nguyên) ...
    if (!specialtyListContainer) {
        return;
    }
    const uniqueItems = dedupeSpecialties(items);
    specialtiesCache = uniqueItems;
    specialtyListContainer.innerHTML = "";
    if (specialtyCountSpan) {
        specialtyCountSpan.textContent = `(${uniqueItems.length})`;
    }
    if (uniqueItems.length === 0) {
        specialtyListContainer.innerHTML = '<p class="p-3 text-center text-muted">Chưa có chuyên khoa nào.</p>';
        return;
    }
    uniqueItems.forEach((specialty) => {
        const color = normalizeColor(specialty.color) || DEFAULT_SPECIALTY_COLOR;
        const statusClass = specialty.active ? "active" : "inactive";
        const statusLabel = specialty.active ? "Hoạt động" : "Đã khóa";
        const toggleLabel = specialty.active ? "Khóa chuyên khoa" : "Mở khóa chuyên khoa";
        const toggleIcon = specialty.active ? "fa-lock" : "fa-unlock";
        const descriptionText = truncateText(stripHtml(specialty.description || ""), 140) || "Chưa có mô tả";
        const doctorNames = Array.isArray(specialty.doctors)
            ? specialty.doctors.map((d) => d.fullName).filter(Boolean).join(", ")
            : "";
        const doctorsHtml = doctorNames
            ? `<div class="specialty-doctors"><span class="specialty-doctors-label">Nhân sự:</span> ${doctorNames}</div>`
            : '<div class="specialty-doctors text-muted">Chưa gán nhân sự</div>';
        const priceLabel = formatCurrency(specialty.price);

        // ⭐️ SỬA LỖI HIỂN THỊ AVATAR: Kiểm tra 'imageUrl' có rỗng/null không
        const avatarUrl = (specialty.imageUrl && specialty.imageUrl.trim() !== "")
            ? specialty.imageUrl
            : `https://ui-avatars.com/api/?name=${encodeURIComponent(specialty.name)}&background=${color.replace("#", "")}&color=fff&rounded=true&size=40`;

        const item = `
            <div class="specialty-item ${statusClass}" data-id="${specialty.id}" style="border-left-color:${color}">
                <div class="drag-handle"><i class="fas fa-grip-vertical"></i></div>
                <div class="specialty-info">
                    <div class="specialty-color-dot" style="background-color:${color}"></div>
                    <img src="${avatarUrl}" class="specialty-avatar" alt="${specialty.name}">
                    <div class="specialty-details">
                        <div class="specialty-name">${specialty.name}</div>
                        ${doctorsHtml}
                    </div>
                </div>
                <div class="specialty-price">${priceLabel}</div>
                <div class="specialty-status ${statusClass}">${statusLabel}</div>
                <div class="specialty-description">${descriptionText}</div>
                <div style="position: relative;">
                    <button class="btn-more-specialty"><i class="fas fa-ellipsis-h"></i></button>
                    <div class="dropdown-menu-specialty">
                        <button class="dropdown-item-specialty btn-toggle-specialty" data-id="${specialty.id}" data-active="${(!specialty.active).toString()}">
                            <i class="fas ${toggleIcon}"></i> <span>${toggleLabel}</span>
                        </button>
                        <button class="dropdown-item-specialty btn-edit-specialty" data-id="${specialty.id}">
                            <i class="fas fa-edit"></i> <span>Chỉnh sửa</span>
                        </button>
                        <button class="dropdown-item-specialty text-danger btn-delete-specialty" data-id="${specialty.id}">
                            <i class="fas fa-trash"></i> <span>Xóa chuyên khoa</span>
                        </button>
                    </div>
                </div>
            </div>`;
        specialtyListContainer.insertAdjacentHTML("beforeend", item);
    });
}

function wireListActions() {
    // ... (Giữ nguyên) ...
    if (!specialtyListContainer) {
        return;
    }
    specialtyListContainer.addEventListener("click", (e) => {
        const dropdownButton = e.target.closest(".btn-more-specialty");
        if (dropdownButton) {
            toggleSpecialtyDropdown(dropdownButton);
            return;
        }
        const toggleButton = e.target.closest(".btn-toggle-specialty");
        if (toggleButton) {
            const specialtyId = toggleButton.getAttribute("data-id");
            const active = toggleButton.getAttribute("data-active") === "true";
            toggleSpecialtyStatus(specialtyId, active);
            return;
        }
        const deleteButton = e.target.closest(".btn-delete-specialty");
        if (deleteButton) {
            const specialtyId = deleteButton.getAttribute("data-id");
            confirmDeleteSpecialty(specialtyId);
            return;
        }
        const editButton = e.target.closest(".btn-edit-specialty");
        if (editButton) {
            const specialtyId = editButton.getAttribute("data-id");
            openEditSpecialtyModal(specialtyId);
            return;
        }
    });
    document.addEventListener("click", (e) => {
        if (!e.target.closest(".btn-more-specialty")) {
            document.querySelectorAll(".dropdown-menu-specialty").forEach((menu) => menu.classList.remove("show"));
        }
    });
}

function toggleSpecialtyDropdown(button) {
    // ... (Giữ nguyên) ...
    const dropdown = button.nextElementSibling;
    document.querySelectorAll(".dropdown-menu-specialty").forEach((menu) => {
        if (menu !== dropdown) {
            menu.classList.remove("show");
        }
    });
    dropdown.classList.toggle("show");
}

function confirmDeleteSpecialty(specialtyId) {
    // ... (Giữ nguyên) ...
    Swal.fire({
        title: "Xóa chuyên khoa?",
        text: "Bạn có chắc muốn xóa chuyên khoa này?",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#d33",
        confirmButtonText: "Đồng ý, xóa",
        cancelButtonText: "Hủy"
    }).then(async (result) => {
        if (!result.isConfirmed) {
            return;
        }
        try {
            const response = await fetch(`/api/Specialty/${specialtyId}`, { method: "DELETE" });
            if (!response.ok) {
                throw new Error("Không thể xóa chuyên khoa.");
            }
            showSuccess("Chuyên khoa đã được xóa.");
            await loadSpecialties();
        } catch (error) {
            showError(error.message);
        }
    });
}

async function toggleSpecialtyStatus(specialtyId, active) {
    if (!specialtyId) {
        return;
    }

    const actionLabel = active ? "mở khóa" : "khóa";
    const confirm = await Swal.fire({
        title: `${active ? "Mở khóa" : "Khóa"} chuyên khoa?`,
        text: `Bạn có chắc muốn ${actionLabel} chuyên khoa này?`,
        icon: "question",
        showCancelButton: true,
        confirmButtonText: "Tiếp tục",
        cancelButtonText: "Hủy"
    });

    if (!confirm.isConfirmed) {
        return;
    }

    try {
        const response = await fetch(`/api/Specialty/${specialtyId}/status`, {
            method: "PATCH",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ active })
        });
        if (!response.ok) {
            throw new Error("Không thể cập nhật trạng thái chuyên khoa.");
        }
        showSuccess(active ? "Chuyên khoa đã được mở khóa." : "Chuyên khoa đã được khóa.");
        await loadSpecialties();
    } catch (error) {
        showError(error.message);
    }
}

async function openEditSpecialtyModal(specialtyId) {
    // ... (Giữ nguyên) ...
    if (!specialtyId) {
        return;
    }
    try {
        Swal.fire({
            title: "Đang tải dữ liệu",
            allowOutsideClick: false,
            didOpen: () => Swal.showLoading()
        });
        const response = await fetch(`/api/Specialty/${specialtyId}`);
        if (!response.ok) {
            throw new Error("Không thể tải thông tin chuyên khoa.");
        }
        const specialty = await response.json();
        editingSpecialtyId = specialtyId;
        editingSpecialtyImageUrl = specialty.imageUrl || null;
        if (specialtyInput) {
            specialtyInput.value = "";
        }
        if (specialtyNameInput) {
            specialtyNameInput.value = specialty.name ?? "";
        }
        if (specialtySlugInput) {
            specialtySlugInput.value = specialty.slug ?? "";
        }
        if (specialtyPriceInput) {
            specialtyPriceInput.value = specialty.price ?? 0;
        }
        setColorValue(specialty.color || DEFAULT_SPECIALTY_COLOR);
        setEditorContent(specialty.description || "");
        const currentDoctorIds = new Set((specialty.doctors || []).map((d) => d.id));
        await ensureDoctorsLoaded(currentDoctorIds);
        setDoctorSelections(Array.from(currentDoctorIds));
        updateUploadPreview(editingSpecialtyImageUrl);
        if (addSpecialtyModalTitle) {
            addSpecialtyModalTitle.textContent = "Chỉnh sửa chuyên khoa";
        }
        if (btnSaveSpecialty) {
            btnSaveSpecialty.textContent = "Lưu thay đổi";
        }
        Swal.close();
        bootstrap.Modal.getOrCreateInstance(addSpecialtyModal).show();
    } catch (error) {
        Swal.close();
        showError(error.message);
    }
}

function setDoctorSelections(doctorIds = []) {
    // ... (Giữ nguyên) ...
    if (!specialtyDoctorsSelect) {
        return;
    }
    const ids = new Set((doctorIds || []).map((id) => String(id).toLowerCase()));
    Array.from(specialtyDoctorsSelect.options).forEach((option) => {
        option.selected = ids.has(option.value.toLowerCase());
    });
}

function wireModalLifecycle() {
    // ... (Giữ nguyên) ...
    if (!addSpecialtyModal) {
        return;
    }
    addSpecialtyModal.addEventListener("shown.bs.modal", () => {
        const currentDoctorIds = new Set();
        if (editingSpecialtyId) {
            // (Đã xử lý trong openEditSpecialtyModal)
        } else {
            ensureDoctorsLoaded(currentDoctorIds);
        }
        if (!specialtyEditorInstance) {
            ClassicEditor.create(document.querySelector("#specialtyDescriptionEditor"), {
                toolbar: ["bold", "italic", "underline", "bulletedList", "numberedList", "link"]
            })
                .then((editor) => {
                    specialtyEditorInstance = editor;
                })
                .catch((err) => console.error("CKEditor", err));
        }
    });
    addSpecialtyModal.addEventListener("hidden.bs.modal", resetForm);
}

function wireFileUploadPreview() {
    // ... (Giữ nguyên) ...
    if (!specialtyInput || !specialtyUploadBox) {
        return;
    }
    specialtyInput.addEventListener("change", (e) => {
        const file = e.target.files?.[0];
        if (file && file.type.startsWith("image/")) {
            const reader = new FileReader();
            reader.onload = (event) => {
                updateUploadPreview(event.target?.result);
            };
            reader.readAsDataURL(file);
        } else if (file) {
            Swal.fire("Lỗi File", "Vui lòng chỉ chọn tệp hình ảnh.", "warning");
            specialtyInput.value = "";
            updateUploadPreview(editingSpecialtyImageUrl);
        }
    });
}

function wireColorInputs() {
    // ... (Giữ nguyên) ...
    if (specialtyColorPicker) {
        specialtyColorPicker.addEventListener("input", (e) => {
            setColorValue(e.target.value);
        });
    }
    if (specialtyColorInput) {
        specialtyColorInput.addEventListener("input", (e) => {
            setColorValue(e.target.value);
        });
    }
}

function wireDoctorSelection() {
    // ... (Giữ nguyên) ...
    if (!specialtyDoctorsSelect) {
        return;
    }
    specialtyDoctorsSelect.addEventListener("focus", () => {
        const currentDoctorIds = new Set();
        if (editingSpecialtyId) {
            const ids = Array.from(specialtyDoctorsSelect.selectedOptions).map((opt) => opt.value);
            ids.forEach(id => currentDoctorIds.add(id));
        }
        ensureDoctorsLoaded(currentDoctorIds);
    });
}

// ⭐️ HÀM NÀY ĐÃ ĐƯỢC CẬP NHẬT ⭐️
function wireSaveSpecialty() {
    if (!btnSaveSpecialty) {
        return;
    }

    btnSaveSpecialty.addEventListener("click", async () => {
        if (isSavingSpecialty) {
            return;
        }

        const name = specialtyNameInput?.value.trim();
        if (!name) {
            Swal.fire("Lỗi", "Tên chuyên khoa là bắt buộc.", "warning");
            return;
        }

        isSavingSpecialty = true;
        btnSaveSpecialty.disabled = true;
        btnSaveSpecialty.innerHTML = '<span class="spinner-border spinner-border-sm"></span> Đang lưu...';

        try {
            const isEditing = Boolean(editingSpecialtyId);

            // Nếu là "Sửa", dùng URL ảnh cũ. Nếu là "Thêm", bắt đầu bằng null.
            let imageUrl = isEditing ? editingSpecialtyImageUrl : null;

            // 1. Lấy file
            const file = specialtyInput?.files?.[0];

            // 2. Upload NẾU CÓ CHỌN file (cho cả Thêm và Sửa)
            if (file) {
                console.log("Đang upload ảnh chuyên khoa...");
                const formData = new FormData();
                formData.append("file", file); // Tên "file" phải khớp với API

                const uploadRes = await fetch("/api/Upload", { method: "POST", body: formData });
                if (!uploadRes.ok) {
                    throw new Error("Tải ảnh thất bại.");
                }
                const result = await uploadRes.json();
                imageUrl = result.avatarUrl; // Lấy URL mới

                if (!imageUrl) {
                    throw new Error("API upload không trả về 'avatarUrl'.");
                }
                console.log("Upload ảnh thành công:", imageUrl);
            }

            // 3. Lấy dữ liệu chữ
            const selectedDoctorIds = specialtyDoctorsSelect
                ? Array.from(specialtyDoctorsSelect.selectedOptions).map((opt) => opt.value)
                : [];

            const description = specialtyEditorInstance ? specialtyEditorInstance.getData() : "";

            // 4. Gộp dữ liệu
            const command = {
                name,
                slug: specialtySlugInput?.value.trim() || null,
                description: description || null,
                imageUrl: imageUrl, // Gửi link ảnh (mới hoặc cũ)
                color: normalizeColor(specialtyColorInput?.value) || DEFAULT_SPECIALTY_COLOR,
                price: parsePriceInput(specialtyPriceInput?.value),
                doctorIds: selectedDoctorIds
            };

            // 5. Gửi request
            let endpoint = "/api/Specialty";
            let method = "POST";
            if (isEditing) {
                endpoint = `/api/Specialty/${editingSpecialtyId}`;
                method = "PUT";
                command.id = editingSpecialtyId;
            }

            const response = await fetch(endpoint, {
                method,
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(command)
            });

            if (!response.ok) {
                const err = await response.json().catch(() => ({}));
                throw new Error(err.detail || (isEditing ? "Không thể cập nhật chuyên khoa." : "Không thể tạo chuyên khoa."));
            }

            closeSpecialtyModal();
            showSuccess(isEditing ? "Đã cập nhật chuyên khoa." : "Đã tạo chuyên khoa mới.");
            resetForm();
            await loadSpecialties(); // Tải lại để thấy ảnh mới
        } catch (error) {
            showError(error.message);
        } finally {
            btnSaveSpecialty.disabled = false;
            const stillEditing = Boolean(editingSpecialtyId);
            btnSaveSpecialty.innerHTML = stillEditing ? "Lưu thay đổi" : defaultSaveSpecialtyText;
            isSavingSpecialty = false;
        }
    });
}

function resetForm() {
    // ... (Giữ nguyên) ...
    addSpecialtyForm?.reset();
    setColorValue(DEFAULT_SPECIALTY_COLOR);
    editingSpecialtyId = null;
    editingSpecialtyImageUrl = null;
    setEditorContent("");
    setDoctorSelections([]);
    updateUploadPreview(null);
    if (specialtyInput) {
        specialtyInput.value = "";
    }
    if (specialtyPriceInput) {
        specialtyPriceInput.value = "";
    }
    if (addSpecialtyModalTitle) {
        addSpecialtyModalTitle.textContent = defaultSpecialtyModalTitle;
    }
    if (btnSaveSpecialty) {
        btnSaveSpecialty.textContent = defaultSaveSpecialtyText;
    }
}

function normalizeColor(value) {
    // ... (Giữ nguyên) ...
    if (!value) {
        return DEFAULT_SPECIALTY_COLOR;
    }
    let normalized = value.trim();
    if (!normalized.startsWith("#")) {
        normalized = `#${normalized.replace(/#/g, "")}`;
    }
    if (normalized.length === 4) {
        normalized = `#${normalized[1]}${normalized[1]}${normalized[2]}${normalized[2]}${normalized[3]}${normalized[3]}`;
    }
    return normalized.slice(0, 7);
}

function dedupeSpecialties(items) {
    // ... (Giữ nguyên) ...
    if (!Array.isArray(items) || items.length === 0) {
        return [];
    }
    const map = new Map();
    items.forEach((item) => {
        if (!item || !item.id) {
            return;
        }
        if (!map.has(item.id)) {
            map.set(item.id, item);
        }
    });
    return Array.from(map.values());
}

function setColorValue(value) {
    // ... (Giữ nguyên) ...
    const normalized = normalizeColor(value);
    if (specialtyColorInput) {
        specialtyColorInput.value = normalized;
    }
    if (specialtyColorPicker) {
        specialtyColorPicker.value = normalized;
    }
    if (specialtyColorSwatch) {
        specialtyColorSwatch.style.backgroundColor = normalized;
    }
}

function setEditorContent(value) {
    // ... (Giữ nguyên) ...
    if (specialtyEditorInstance) {
        specialtyEditorInstance.setData(value || "");
        return;
    }
    const startedAt = Date.now();
    const intervalId = setInterval(() => {
        if (specialtyEditorInstance) {
            specialtyEditorInstance.setData(value || "");
            clearInterval(intervalId);
        } else if (Date.now() - startedAt > 5000) {
            clearInterval(intervalId);
        }
    }, 50);
}

function updateUploadPreview(imageUrl) {
    // ... (Giữ nguyên) ...
    if (!specialtyUploadBox) {
        return;
    }
    if (imageUrl) {
        specialtyUploadBox.innerHTML = `<img src="${imageUrl}" alt="Xem trước" class="image-preview">`;
        specialtyUploadBox.classList.add("has-image");
    } else {
        specialtyUploadBox.innerHTML = originalSpecialtyUploadHTML;
        specialtyUploadBox.classList.remove("has-image");
    }
}

function closeSpecialtyModal() {
    // ... (Giũ nguyên) ...
    if (!addSpecialtyModal) {
        return;
    }
    const modalInstance = bootstrap.Modal.getInstance(addSpecialtyModal) ?? bootstrap.Modal.getOrCreateInstance(addSpecialtyModal);
    modalInstance.hide();
}

function showSuccess(message) {
    // ... (Giữ nguyên) ...
    Swal.fire({
        icon: "success",
        title: "Thành công",
        text: message,
        timer: 1800,
        showConfirmButton: false
    });
}

function showError(message) {
    // ... (Giữ nguyên) ...
    Swal.fire({
        icon: "error",
        title: "Lỗi",
        text: message
    });
}

function stripHtml(html) {
    // ... (Giữ nguyên) ...
    const div = document.createElement("div");
    div.innerHTML = html;
    return div.textContent || div.innerText || "";
}

function truncateText(text, maxLength) {
    // ... (Giữ nguyên) ...
    if (!text) {
        return "";
    }
    return text.length > maxLength ? `${text.substring(0, maxLength)}…` : text;
}

function formatCurrency(value) {
    const numeric = Number(value ?? 0);
    if (!Number.isFinite(numeric)) {
        return currencyFormatter.format(0);
    }
    return currencyFormatter.format(numeric);
}

function parsePriceInput(value) {
    if (value === undefined || value === null) {
        return 0;
    }
    const normalized = Number(String(value).replace(/[^0-9.,-]/g, "").replace(",", "."));
    if (!Number.isFinite(normalized) || normalized < 0) {
        return 0;
    }
    return Math.round(normalized);
}
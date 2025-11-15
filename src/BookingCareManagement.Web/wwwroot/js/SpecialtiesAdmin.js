const DEFAULT_SPECIALTY_COLOR = "#1a73e8";
let specialtyEditorInstance;
let editingSpecialtyId = null;
let editingSpecialtyImageUrl = null;

const addSpecialtyModal = document.getElementById("addSpecialtyModal");
const addSpecialtyModalTitle = document.getElementById("addSpecialtyModalLabel");
const defaultSpecialtyModalTitle = addSpecialtyModalTitle ? addSpecialtyModalTitle.textContent : "Thêm chuyên khoa";
const specialtyInput = document.getElementById("specialtyImageInput");
const specialtyUploadBox = document.getElementById("specialtyUploadBox");
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

const specialtyColorSwatch = document.querySelector("#specialtyColor")?.closest(".color-picker-wrapper")?.querySelector(".color-swatch");

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

// THAY ĐỔI 1: Hàm này giờ sẽ nhận 'currentDoctorIds' (các bác sĩ thuộc CK đang sửa)
async function ensureDoctorsLoaded(currentDoctorIds = new Set()) {
    if (doctorsCache.length > 0) {
        // Nếu đã có cache, chỉ cần lọc lại danh sách hiển thị
        populateDoctorOptions(currentDoctorIds);
        return doctorsCache;
    }
    try {
        // Nếu chưa có cache, tải mới
        const response = await fetch("/api/Doctor");
        if (!response.ok) {
            throw new Error("Không thể tải danh sách bác sĩ.");
        }
        doctorsCache = await response.json();
        // Lọc danh sách ngay khi tải về
        populateDoctorOptions(currentDoctorIds);
        return doctorsCache;
    } catch (error) {
        console.error(error);
        Swal.fire("Lỗi", error.message, "error");
        return [];
    }
}

// THAY ĐỔI 2: Hàm này nhận 'currentDoctorIds' và lọc dựa trên đó
function populateDoctorOptions(currentDoctorIds = new Set()) {
    if (!specialtyDoctorsSelect) {
        return;
    }

    specialtyDoctorsSelect.innerHTML = ""; // Xóa danh sách cũ

    doctorsCache
        .filter((doc) => {
            // Lọc 1: Bác sĩ không hoạt động -> Ẩn
            if (doc.active === false) {
                return false;
            }

            // Lọc 2: Kiểm tra xem bác sĩ đã có chuyên khoa chưa
            // (Giả định DTO bác sĩ từ /api/Doctor có mảng 'specialties')
            const doctorHasSpecialty = doc.specialties && doc.specialties.length > 0;

            // Lọc 3: Kiểm tra xem bác sĩ có thuộc chuyên khoa *hiện tại* đang sửa không
            const belongsToThisSpecialty = currentDoctorIds.has(doc.id);

            // CHỈ HIỂN THỊ BÁC SĨ NẾU:
            // 1. Họ chưa có chuyên khoa (còn trống)
            // 2. HOẶC họ đã thuộc về chính chuyên khoa này (để còn sửa)
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
        const descriptionText = truncateText(stripHtml(specialty.description || ""), 140) || "Chưa có mô tả";

        // SỬA LỖI HIỂN THỊ: Đảm bảo 'specialty.doctors' là một mảng
        const doctorNames = Array.isArray(specialty.doctors)
            ? specialty.doctors.map((d) => d.fullName).filter(Boolean).join(", ")
            : "";

        const doctorsHtml = doctorNames
            ? `<div class="specialty-doctors">${doctorNames}</div>`
            : '<div class="specialty-doctors text-muted">Chưa gán bác sĩ</div>';

        const avatarUrl = specialty.imageUrl
            || `https://ui-avatars.com/api/?name=${encodeURIComponent(specialty.name)}&background=${color.replace("#", "")}&color=fff&rounded=true&size=40`;

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
                <div class="specialty-status ${statusClass}">${statusLabel}</div>
                <div class="specialty-description">${descriptionText}</div>
                <div style="position: relative;">
                    <button class="btn-more-specialty"><i class="fas fa-ellipsis-h"></i></button>
                    <div class="dropdown-menu-specialty">
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
    if (!specialtyListContainer) {
        return;
    }

    specialtyListContainer.addEventListener("click", (e) => {
        const dropdownButton = e.target.closest(".btn-more-specialty");
        if (dropdownButton) {
            toggleSpecialtyDropdown(dropdownButton);
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
    const dropdown = button.nextElementSibling;
    document.querySelectorAll(".dropdown-menu-specialty").forEach((menu) => {
        if (menu !== dropdown) {
            menu.classList.remove("show");
        }
    });
    dropdown.classList.toggle("show");
}

function confirmDeleteSpecialty(specialtyId) {
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

// THAY ĐỔI 3: Cập nhật hàm này để truyền 'currentDoctorIds'
async function openEditSpecialtyModal(specialtyId) {
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

        setColorValue(specialty.color || DEFAULT_SPECIALTY_COLOR);
        setEditorContent(specialty.description || "");

        // Lấy danh sách ID bác sĩ của chuyên khoa NÀY
        const currentDoctorIds = new Set((specialty.doctors || []).map((d) => d.id));

        // Tải danh sách bác sĩ VỚI thông tin lọc
        await ensureDoctorsLoaded(currentDoctorIds);

        // Chọn các bác sĩ thuộc chuyên khoa này
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
    if (!specialtyDoctorsSelect) {
        return;
    }

    const ids = new Set((doctorIds || []).map((id) => String(id).toLowerCase()));
    Array.from(specialtyDoctorsSelect.options).forEach((option) => {
        option.selected = ids.has(option.value.toLowerCase());
    });
}

// THAY ĐỔI 4: Cập nhật hàm này để truyền Set rỗng khi THÊM MỚI
function wireModalLifecycle() {
    if (!addSpecialtyModal) {
        return;
    }

    addSpecialtyModal.addEventListener("shown.bs.modal", () => {
        // Khi modal mở, gọi ensureDoctorsLoaded
        // Nếu là modal Thêm mới (editingSpecialtyId = null),
        // chúng ta truyền một Set rỗng, nó sẽ chỉ lọc ra các bác sĩ chưa có chuyên khoa
        const currentDoctorIds = new Set();
        if (editingSpecialtyId) {
            // Trường hợp này đã được xử lý trong openEditSpecialtyModal,
            // nhưng chúng ta vẫn có thể gọi lại để đảm bảo
            // (Tuy nhiên, logic của openEdit đã gọi ensureDoctorsLoaded rồi)
        } else {
            // Đây là modal THÊM MỚI
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
    if (!specialtyDoctorsSelect) {
        return;
    }

    specialtyDoctorsSelect.addEventListener("focus", () => {
        // Khi focus, chúng ta cần biết ID chuyên khoa đang sửa
        const currentDoctorIds = new Set();
        if (editingSpecialtyId) {
            // Nếu đang sửa, tìm các bác sĩ đã được chọn
            const ids = Array.from(specialtyDoctorsSelect.selectedOptions).map((opt) => opt.value);
            ids.forEach(id => currentDoctorIds.add(id));
        }
        ensureDoctorsLoaded(currentDoctorIds);
    });
}

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
            let imageUrl = isEditing ? editingSpecialtyImageUrl : null;
            const file = specialtyInput?.files?.[0];
            if (file) {
                const formData = new FormData();
                formData.append("file", file);
                const uploadRes = await fetch("/api/Upload", { method: "POST", body: formData });
                if (!uploadRes.ok) {
                    throw new Error("Tải ảnh thất bại.");
                }
                const result = await uploadRes.json();
                imageUrl = result.avatarUrl;
                editingSpecialtyImageUrl = imageUrl;
            }

            const selectedDoctorIds = specialtyDoctorsSelect
                ? Array.from(specialtyDoctorsSelect.selectedOptions).map((opt) => opt.value)
                : [];

            const description = specialtyEditorInstance ? specialtyEditorInstance.getData() : "";

            const command = {
                name,
                slug: specialtySlugInput?.value.trim() || null,
                description: description || null,
                imageUrl,
                color: normalizeColor(specialtyColorInput?.value) || DEFAULT_SPECIALTY_COLOR,
                doctorIds: selectedDoctorIds
            };

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
            await loadSpecialties();
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

    if (addSpecialtyModalTitle) {
        addSpecialtyModalTitle.textContent = defaultSpecialtyModalTitle;
    }

    if (btnSaveSpecialty) {
        btnSaveSpecialty.textContent = defaultSaveSpecialtyText;
    }
}

function normalizeColor(value) {
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
    if (!addSpecialtyModal) {
        return;
    }
    const modalInstance = bootstrap.Modal.getInstance(addSpecialtyModal) ?? bootstrap.Modal.getOrCreateInstance(addSpecialtyModal);
    modalInstance.hide();
}

function showSuccess(message) {
    Swal.fire({
        icon: "success",
        title: "Thành công",
        text: message,
        timer: 1800,
        showConfirmButton: false
    });
}

function showError(message) {
    Swal.fire({
        icon: "error",
        title: "Lỗi",
        text: message
    });
}

function stripHtml(html) {
    const div = document.createElement("div");
    div.innerHTML = html;
    return div.textContent || div.innerText || "";
}

function truncateText(text, maxLength) {
    if (!text) {
        return "";
    }
    return text.length > maxLength ? `${text.substring(0, maxLength)}…` : text;
}
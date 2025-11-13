const DEFAULT_SPECIALTY_COLOR = "#1a73e8";
let specialtyEditorInstance;

const addSpecialtyModal = document.getElementById("addSpecialtyModal");
const specialtyInput = document.getElementById("specialtyImageInput");
const specialtyUploadBox = document.getElementById("specialtyUploadBox");
const originalSpecialtyUploadHTML = specialtyUploadBox ? specialtyUploadBox.innerHTML : "";
const specialtyColorInput = document.getElementById("specialtyColor");
const specialtyColorPicker = document.getElementById("specialtyColorPicker");
const specialtyDoctorsSelect = document.getElementById("specialtyDoctors");

const specialtyListContainer = document.getElementById("specialty-list-container");
const specialtyCountSpan = document.getElementById("specialty-count");
const btnSaveSpecialty = document.getElementById("btn-save-specialty");
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
    specialtyListContainer.innerHTML = '<div class="p-3 text-center text-muted">Đang tải chuyên khoa...</div>';

    try {
        const response = await fetch("/api/Specialty");
        if (!response.ok) {
            throw new Error("Không thể tải chuyên khoa.");
        }
        specialtiesCache = await response.json();
        renderSpecialtyList(specialtiesCache);
    } catch (error) {
        console.error(error);
        specialtyListContainer.innerHTML = '<p class="text-danger p-3">Lỗi tải chuyên khoa. Vui lòng thử lại.</p>';
    }
}

async function ensureDoctorsLoaded() {
    if (doctorsCache.length > 0) {
        populateDoctorOptions();
        return doctorsCache;
    }
    try {
        const response = await fetch("/api/Doctor");
        if (!response.ok) {
            throw new Error("Không thể tải danh sách bác sĩ.");
        }
        doctorsCache = await response.json();
        populateDoctorOptions();
        return doctorsCache;
    } catch (error) {
        console.error(error);
        Swal.fire("Lỗi", error.message, "error");
        return [];
    }
}

function populateDoctorOptions() {
    if (!specialtyDoctorsSelect) {
        return;
    }

    const selected = new Set(Array.from(specialtyDoctorsSelect.selectedOptions).map((o) => o.value));
    specialtyDoctorsSelect.innerHTML = "";
    doctorsCache
        .filter((doc) => doc.active !== false)
        .forEach((doc) => {
            const option = document.createElement("option");
            option.value = doc.id;
            const fullName = doc.fullName?.trim() || `${doc.firstName ?? ""} ${doc.lastName ?? ""}`.trim() || doc.email || "Bác sĩ";
            option.textContent = fullName;
            if (selected.has(option.value)) {
                option.selected = true;
            }
            specialtyDoctorsSelect.appendChild(option);
        });
}

function renderSpecialtyList(items) {
    if (!specialtyListContainer) {
        return;
    }

    specialtyListContainer.innerHTML = "";
    if (specialtyCountSpan) {
        specialtyCountSpan.textContent = `(${items.length})`;
    }

    if (items.length === 0) {
        specialtyListContainer.innerHTML = '<p class="p-3 text-center text-muted">Chưa có chuyên khoa nào.</p>';
        return;
    }

    items.forEach((specialty) => {
        const color = normalizeColor(specialty.color) || DEFAULT_SPECIALTY_COLOR;
        const statusClass = specialty.active ? "active" : "inactive";
        const statusLabel = specialty.active ? "Hoạt động" : "Đã khóa";
        const descriptionText = truncateText(stripHtml(specialty.description || ""), 140) || "Chưa có mô tả";
        const doctorNames = (specialty.doctors || []).map((d) => d.fullName).filter(Boolean).join(", ");
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
            Swal.fire("Thông báo", "Tính năng chỉnh sửa đang được phát triển.", "info");
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
            Swal.fire("Đã xóa", "Chuyên khoa đã được xóa.", "success");
            loadSpecialties();
        } catch (error) {
            Swal.fire("Lỗi", error.message, "error");
        }
    });
}

function wireModalLifecycle() {
    if (!addSpecialtyModal) {
        return;
    }

    addSpecialtyModal.addEventListener("shown.bs.modal", () => {
        ensureDoctorsLoaded();

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
                specialtyUploadBox.innerHTML = `<img src="${event.target?.result}" alt="Xem trước" class="image-preview">`;
                specialtyUploadBox.classList.add("has-image");
            };
            reader.readAsDataURL(file);
        } else if (file) {
            Swal.fire("Lỗi File", "Vui lòng chỉ chọn tệp hình ảnh.", "warning");
            specialtyInput.value = "";
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
        ensureDoctorsLoaded();
    });
}

function wireSaveSpecialty() {
    if (!btnSaveSpecialty) {
        return;
    }

    btnSaveSpecialty.addEventListener("click", async () => {
        const name = specialtyNameInput?.value.trim();
        if (!name) {
            Swal.fire("Lỗi", "Tên chuyên khoa là bắt buộc.", "warning");
            return;
        }

        btnSaveSpecialty.disabled = true;
        btnSaveSpecialty.innerHTML = '<span class="spinner-border spinner-border-sm"></span> Đang lưu...';

        try {
            let imageUrl = null;
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

            const response = await fetch("/api/Specialty", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(command)
            });

            if (!response.ok) {
                const err = await response.json().catch(() => ({}));
                throw new Error(err.detail || "Không thể tạo chuyên khoa.");
            }

            Swal.fire("Thành công", "Đã tạo chuyên khoa mới.", "success");
            bootstrap.Modal.getInstance(addSpecialtyModal)?.hide();
            resetForm();
            loadSpecialties();
        } catch (error) {
            Swal.fire("Lỗi", error.message, "error");
        } finally {
            btnSaveSpecialty.disabled = false;
            btnSaveSpecialty.innerHTML = "Thêm chuyên khoa";
        }
    });
}

function resetForm() {
    addSpecialtyForm?.reset();
    setColorValue(DEFAULT_SPECIALTY_COLOR);

    if (specialtyEditorInstance) {
        specialtyEditorInstance.setData("");
    }

    if (specialtyDoctorsSelect) {
        Array.from(specialtyDoctorsSelect.options).forEach((opt) => (opt.selected = false));
    }

    if (specialtyUploadBox) {
        specialtyUploadBox.innerHTML = originalSpecialtyUploadHTML;
        specialtyUploadBox.classList.remove("has-image");
    }

    if (specialtyInput) {
        specialtyInput.value = "";
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

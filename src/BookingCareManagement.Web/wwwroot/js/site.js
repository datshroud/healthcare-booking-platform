document.addEventListener("DOMContentLoaded", function () {
    const userInfo = document.getElementById("userDropdown");

    if (userInfo) {
        // Ngăn dropdown tự đóng khi click bên trong panel
        const dropdownMenu = userInfo.closest(".user-dropdown").querySelector(".dropdown-menu");
        dropdownMenu.addEventListener("click", (e) => e.stopPropagation());
    }
});

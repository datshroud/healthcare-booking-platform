document.addEventListener("DOMContentLoaded", function () {
    const btnProfile = document.getElementById("btnProfile");
    const btnSettings = document.getElementById("btnSettings");
    const profileContent = document.getElementById("profileContent");
    const settingsContent = document.getElementById("settingsContent");

    if (btnProfile && btnSettings && profileContent && settingsContent) {
        // Khi nhấn nút "Profile Details"
        btnProfile.addEventListener("click", () => {
            btnProfile.classList.add("active-tab");
            btnSettings.classList.remove("active-tab");
            profileContent.classList.remove("d-none");
            settingsContent.classList.add("d-none");
        });

        // Khi nhấn nút "Account Settings"
        btnSettings.addEventListener("click", () => {
            btnSettings.classList.add("active-tab");
            btnProfile.classList.remove("active-tab");
            settingsContent.classList.remove("d-none");
            profileContent.classList.add("d-none");
        });
    }
});


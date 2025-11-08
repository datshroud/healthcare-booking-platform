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

﻿
// const btnPersonal = document.getElementById('btnPersonalInfo');
// const btnChange = document.getElementById('btnChangePassword');
// const personal = document.getElementById('personalInfoSection');
// const change = document.getElementById('changePasswordSection');
// const titlePath = document.querySelector('.title-section span');

//     btnPersonal.addEventListener('click', () => {
//     btnPersonal.classList.add('active');
// btnChange.classList.remove('active');
// personal.classList.remove('d-none');
// change.classList.add('d-none');
// titlePath.textContent = "› Personal Info";
//     });

//     btnChange.addEventListener('click', () => {
//     btnChange.classList.add('active');
// btnPersonal.classList.remove('active');
// change.classList.remove('d-none');
// personal.classList.add('d-none');
// titlePath.textContent = "› Change Password";
//     });

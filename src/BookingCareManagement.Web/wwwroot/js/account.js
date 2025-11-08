
const btnPersonal = document.getElementById('btnPersonalInfo');
const btnChange = document.getElementById('btnChangePassword');
const personal = document.getElementById('personalInfoSection');
const change = document.getElementById('changePasswordSection');
const titlePath = document.querySelector('.title-section span');

    btnPersonal.addEventListener('click', () => {
    btnPersonal.classList.add('active');
btnChange.classList.remove('active');
personal.classList.remove('d-none');
change.classList.add('d-none');
titlePath.textContent = "› Personal Info";
    });

    btnChange.addEventListener('click', () => {
    btnChange.classList.add('active');
btnPersonal.classList.remove('active');
change.classList.remove('d-none');
personal.classList.add('d-none');
titlePath.textContent = "› Change Password";
    });
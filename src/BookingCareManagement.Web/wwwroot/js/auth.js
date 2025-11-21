// goi api json
async function postJson(url, body){
    const res = await fetch(url, {
        method: 'POST',
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(body),
        credentials: 'include'
    });
    // If the server redirected (e.g. returned a redirect to a page), fetch follows redirects
    // and we may get an HTML response. Handle redirected/html responses before attempting to parse JSON.
    if (res.redirected) {
        // return an object with redirect URL so callers can navigate
        return { redirect: res.url };
    }

    if (!res.ok) {
        const text = await res.text();
        throw new Error(text || `HTTP ${res.status}`);
        // let msg = 'Có lỗi xảy ra.';
        // try {
        //     const contentType = res.headers.get('content-type') || '';
        //     if (contentType.includes('application/json')) {
        //         const body = await res.json();
        //         msg = body.detail || body.message || msg;
        //     } else {
        //         // non-json error (maybe HTML) - read text
        //         const txt = await res.text();
        //         msg = txt || msg;
        //     }
        // } catch {}
        // const err = new Error(msg);
        // err.status = res.status;
        // throw err;
    }
    const contentType = res.headers.get('content-type') || '';
    if (contentType.includes('application/json')) {
        return res.status === 204 ? {} : res.json();
    }
    // If server returned HTML (e.g. after redirect), return redirect info
    if (contentType.includes('text/html')) {
        // the fetch already followed redirect; res.url is the final URL
        return { redirect: res.url };
    }

    // Fallback: try to parse JSON, otherwise return empty
    try { return res.json(); } catch { return {}; }

}

function getReturnUrl() {
    const u = new URLSearchParams(location.search).get('returnUrl');
    const here = location.pathname + location.search + location.hash;
    return u && u.startsWith('/') ? u : here;
}

function showFieldError(field, message) {
    if (!field) return;

    field.classList.add('input-invalid');
    field.setAttribute('aria-invalid', 'true');

    let err = field.nextElementSibling;
    if (!err || !err.classList.contains('field-error')){
        err = document.createElement('div');
        err.className = 'field-error';
        field.insertAdjacentElement('afterend', err);
    }
    err.textContent = message;

    const id = field.id || ('fld_' + Math.random().toString(36).slice(2));
    field.id = id;
    err.id = id + '_error';
    field.setAttribute('aria-describedby', err.id);
}
function clearFieldError(field) {
    if (!field) return;
    field.classList.remove('input-invalid');
    field.removeAttribute('aria-invalid');
    const errEl = field.nextElementSibling;
    if (errEl && errEl.classList.contains('field-error')) {
        errEl.remove();
    }
}

function emailLooksValid(email) {
    if (typeof email !== 'string') return false;
    const v = email.trim();

    // Dùng validator gốc của trình duyệt (theo chuẩn WHATWG)
    const input = document.createElement('input');
    input.type = 'email';
    input.value = v;
    if (!input.checkValidity()) return false;

    // Giới hạn độ dài theo khuyến nghị (RFC 5321/5322)
    if (v.length > 254) return false;

    // Tách local@domain
    const at = v.indexOf('@');
    if (at <= 0 || at === v.length - 1) return false;

    const local = v.slice(0, at);
    const domain = v.slice(at + 1);

    // Local-part tối đa 64 ký tự
    if (local.length > 64) return false;

    // Không bắt đầu/kết thúc bằng dấu chấm, không có ".."
    if (local.startsWith('.') || local.endsWith('.')) return false;
    if (local.includes('..')) return false;
    if (domain.startsWith('.') || domain.endsWith('.')) return false;
    if (domain.includes('..')) return false;

    // Domain phải có ít nhất 1 dấu chấm (thực dụng cho user-facing)
    if (!domain.includes('.')) return false;

    // Kiểm tra từng nhãn miền: 1–63 ký tự, không đầu/cuối bằng '-'
    const labels = domain.split('.');
    for (const label of labels) {
        if (label.length < 1 || label.length > 63) return false;
        if (label.startsWith('-') || label.endsWith('-')) return false;
    }

    return true;
}

function setBusy(button, busy = true) {
    if (!button) return;
    button.disabled = busy;
    button.dataset.loading = busy ? '1' : '';
}

// login
const loginForm = document.getElementById('login-form');
if (loginForm){
    const loginEmailEl = loginForm.querySelector("[name=email]");
    const loginPasswordEl = loginForm.querySelector("[name=password]");
    [loginEmailEl, loginPasswordEl].forEach(preventNewlines);

    loginForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        const email = loginEmailEl?.value.trim() || '';
        const password = loginPasswordEl?.value || '';
        const returnUrl = getReturnUrl();
        try {
            const resp = await postJson(`/api/account/auth/login?returnUrl=${encodeURIComponent(returnUrl)}`, { email, password });
            const target = resp?.redirect || returnUrl || '/';
            window.location.assign(target);
        } catch (err) {
            alert(err.message);
        }
    });

    // gg login
    const btnGoogle = document.getElementById('google-login');
    if (btnGoogle) {
        btnGoogle.addEventListener('click', () => {
            window.location.href = `/api/account/auth/google/start?returnUrl=${encodeURIComponent(getReturnUrl())}`;
        });
    }

    // db login
    const btnDbLogin = document.getElementById('db-login');
    if (btnDbLogin){
        btnDbLogin.addEventListener('click', async (e) => {
            e.preventDefault();

            const emailEl = loginEmailEl;
            const passwordEl = loginPasswordEl;

            [emailEl, passwordEl].forEach(el => clearFieldError(el));

            const email = emailEl?.value.trim() || '';
            const password = passwordEl.value || '';

            if (!emailLooksValid(email)){
                showFieldError(emailEl, 'Email không hợp lệ.');
                emailEl.focus();
                return;
            }
            if (!password){
                showFieldError(passwordEl, 'Mật khẩu không được để trống.');
                passwordEl.focus();
                return;
            }


            const returnUrl = getReturnUrl();
            try {
                setBusy(btnDbLogin, true);
                const resp = await postJson(`/api/account/auth/login?returnUrl=${encodeURIComponent(returnUrl)}`,
                    { email, password });
                const target = resp?.redirect || returnUrl || '/';
                window.location.assign(target);
            } catch (err) { 
                showFieldError(passwordEl, err.message || 'Đăng nhập thất bại.');
            } finally { setBusy(btnDbLogin, false); }
        });
    }
}



// signup
function buildFullPhoneNumber(form) {
    const countryCode = form.querySelector('.country-code')?.value.trim() || '';
    const phoneNumber = form.querySelector("[name=phoneNumber]")?.value.trim() || '';
    const digits = phoneNumber.replace(/\D/g, '');
    return countryCode + digits;
}

function passwordLooksOk(pw) {
    return typeof pw === 'string' && pw.length >= 6;
}


function preventNewlines(field) {
    if (!field) return;

    const stripNewlines = () => {
        const cleaned = field.value.replace(/[\r\n]+/g, '');
        if (cleaned !== field.value) {
            field.value = cleaned;
        }
    };

    field.addEventListener('input', stripNewlines);
    field.addEventListener('paste', () => setTimeout(stripNewlines));
}





const signupForm = document.getElementById('signup-form');
if (signupForm){
    const firstEl = signupForm.querySelector("[name=firstname]");
    const lastEl = signupForm.querySelector("[name=lastname]");
    const emailEl = signupForm.querySelector("[name=email]");
    const passwordEl = signupForm.querySelector("[name=password]");
    const dobEl = signupForm.querySelector("[name=dateOfBirth]");
    const phoneNumberEl = signupForm.querySelector("[name=phoneNumber]");
    [firstEl, lastEl, emailEl, passwordEl, phoneNumberEl].forEach(preventNewlines);

    signupForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        const first = firstEl?.value.trim() || '';
        const last = lastEl?.value.trim() || '';
        const email = emailEl?.value.trim() || '';
        const password = passwordEl?.value || '';
        const phone = (phoneNumberEl?.value || '').trim();
        const dob = dobEl?.value || null;
        const returnUrl = getReturnUrl();
        try {
            const resp = await postJson(`/api/account/auth/register?returnUrl=${encodeURIComponent(returnUrl)}`, {
                firstName: first,
                lastName: last,
                email,
                password,
                phoneNumber: phone,
                dateOfBirth: dob
            });
            const target = resp?.redirect || returnUrl || '/';
            window.location.assign(target);
        } catch (err) { alert(err.message); }
    });

    const btnGoogle = document.getElementById('google-signup');
    if (btnGoogle) {
        btnGoogle.addEventListener('click', () => {
            window.location.href = `/api/account/auth/google/start?returnUrl=${encodeURIComponent(getReturnUrl())}`;
        });
    }

    // db signup
    const btnDbSignup = document.getElementById('db-signup');
    if (btnDbSignup){
        btnDbSignup.addEventListener('click', async (e) => {
            e.preventDefault();

            [firstEl, lastEl, emailEl, passwordEl, dobEl, phoneNumberEl].forEach(el => clearFieldError(el));

            const first = firstEl?.value.trim() || '';
            const last = lastEl?.value.trim() || '';
            const email = emailEl?.value.trim() || '';
            const password = passwordEl?.value || '';
            const dob = dobEl?.value || null;
            const phoneNumber = buildFullPhoneNumber(signupForm);

            if (!first){
                showFieldError(firstEl, 'Họ không được để trống');
                firstEl.focus();
                return;
            }
            if (!last){
                showFieldError(lastEl, 'Tên không được để trống');
                lastEl.focus();
                return;
            }
            if (!emailLooksValid(email)){
                showFieldError(emailEl, 'Email không hợp lệ.');
                emailEl.focus();
                return;
            }
            if (!passwordLooksOk(password)){
                showFieldError(passwordEl, 'Mật khẩu tối thiểu 6 ký tự.');
                passwordEl.focus();
                return;
            }
            if (!phoneNumber || phoneNumber.length < 6){
                showFieldError(phoneNumberEl, 'Số điện thoại không hợp lệ');
                phoneNumberEl.focus();
                return;
            }

            const returnUrl = getReturnUrl();

            try {
                setBusy(btnDbSignup, true);
                const resp = await postJson(`/api/account/auth/register?returnUrl=${encodeURIComponent(returnUrl)}`, {
                    firstName: first,
                    lastName: last,
                    email,
                    password,
                    phoneNumber,
                    dateOfBirth: dob
                });
                const target = resp?.redirect || returnUrl || '/';
                window.location.assign(target);
            } catch (err) {
                showFieldError(phoneNumberEl, err.message || 'Đăng ký thất bại.');
            }
            finally { setBusy(btnDbSignup, false); }
        });
    }
}

// forgot password
const forgotForm = document.getElementById('forgot-form');
if (forgotForm){
    forgotForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        const email = forgotForm.querySelector("[name=email]").value.trim();
            try {
            await postJson('/api/account/auth/forgot-password', { email });
            alert("Vui lòng kiểm tra email để đặt lại mật khẩu.");
        } catch (err) { alert(err.message); }
    });
}
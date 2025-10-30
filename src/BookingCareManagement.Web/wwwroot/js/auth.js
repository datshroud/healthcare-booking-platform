// goi api json
async function postJson(url, body){
    const res = await fetch(url, {
        method: 'POST',
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(body)
    });
    if (!res.ok) {
        const text = await res.text();
        throw new Error(text || `HTTP ${res.status}`);
    }
    return await res.json();
}

// luu token 
function saveToken(resp) {
    localStorage.setItem('access_token', resp.accessToken);
    localStorage.setItem("access_expires", resp.expiresAt);
    localStorage.setItem("refresh_token", resp.refreshToken);
}

function showError(message) {
    alert(err?.message || 'An error occurred');
}

// login
const loginForm = document.getElementById('login-form');
if (loginForm){
    loginForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        const email = document.querySelector("[name=email]").value.Trim();
        const password = document.querySelector("[name=password]").value;
        try {
            const resp = await postJson('/api/auth/login', { email, password });
            saveToken(resp);
            window.location.href = '/';
        } catch (err) {
            showError(err);
        }
    });

    // gg login
    const btnGoogle = document.getElementById('google-login');
    if (btnGoogle) {
        btnGoogle.addEventListener('click', () => {
            window.location.href = '/api/auth/google/start'; 
        })
    }
}

// signup
const signupForm = document.getElementById('signup-form');
if (signupForm){
    signupForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        const first = document.querySelector("[name=firstname]").value.Trim();
        const last = document.querySelector("[name=lastname]").value.Trim();
        const email = document.querySelector("[name=email]").value.Trim();
        const password = document.querySelector("[name=password]").value;
        try {
            const resp = await postJson('/api/auth/register', {
                fullName: `${first} ${last}`,
                email,
                password
            });
            saveToken(resp);
            window.location.href = '/';
        } catch (err) { showError(err); }
    });

    const btnGoogle = document.getElementById('google-signup');
    if (btnGoogle) {
        btnGoogle.addEventListener('click', () => {
            window.location.href = '/api/auth/google/start'; 
        });
    }
}

// forgot password
const forgotForm = document.getElementById('forgot-form');
if (forgotForm){
    forgotForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        const email = document.querySelector("[name=email]").value.Trim();
        try {
            await postJson('/api/auth/forgot-password', { email });
            alert("Please check your email!");
        } catch (err) { showError(err); }
    });
}
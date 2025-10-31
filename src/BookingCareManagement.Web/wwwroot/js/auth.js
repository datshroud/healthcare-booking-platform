// goi api json
async function postJson(url, body){
    const res = await fetch(url, {
        method: 'POST',
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(body),
        credentials: 'include'
    });
    if (!res.ok) {
        const text = await res.text();
        throw new Error(text || `HTTP ${res.status}`);
    }
}

// login
const loginForm = document.getElementById('login-form');
if (loginForm){
    loginForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        const email = loginForm.querySelector("[name=email]").value.Trim();
        const password = loginForm.querySelector("[name=password]").value;
        try {
            const resp = await postJson('/api/auth/login', { email, password });
            window.location.href = '/';
        } catch (err) {
            alert(err.message);
        }
    });

    // gg login
    const btnGoogle = document.getElementById('google-login');
    if (btnGoogle) {
        btnGoogle.addEventListener('click', () => {
            window.location.href = '/api/auth/google/start?returnUrl=/'; 
        });
    }
}

// signup
const signupForm = document.getElementById('signup-form');
if (signupForm){
    signupForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        const first = signupForm.querySelector("[name=firstname]").value.Trim();
        const last = signupForm.querySelector("[name=lastname]").value.Trim();
        const email = signupForm.querySelector("[name=email]").value.Trim();
        const password = signupForm.querySelector("[name=password]").value;
        try {
            await postJson('/api/auth/register', {
                fullName: `${first} ${last}`.trim(),
                email,
                password
            });
            window.location.href = '/';
        } catch (err) { alert(err.message); }
    });

    const btnGoogle = document.getElementById('google-signup');
    if (btnGoogle) {
        btnGoogle.addEventListener('click', () => {
            window.location.href = '/api/auth/google/start?returnUrl=/'; 
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
            await postJson('/api/auth/forgot-password', { email });
            alert("Please check your email!");
        } catch (err) { alert(err.message); }
    });
}
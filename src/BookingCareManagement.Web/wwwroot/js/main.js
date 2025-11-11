document.addEventListener("DOMContentLoaded", function () {
    const currentPath = window.location.pathname.toLowerCase();
    const sidebarLinks = document.querySelectorAll('.sidebar a');

    sidebarLinks.forEach(link => {
        const linkPath = link.getAttribute('href').toLowerCase();

        // Nếu đường dẫn trang hiện tại BẮT ĐẦU BẰNG href của link
        // (ví dụ: /doctors/edit/123 cũng sẽ highlight link /doctors)
        if (currentPath.startsWith(linkPath) && linkPath !== "/") {
            link.classList.add('active');
        } else {
            link.classList.remove('active');
        }
    });

    // Xử lý riêng cho link Dashboard (vì /doctors cũng bắt đầu bằng /)
    if (currentPath === "/dashboard" || currentPath === "/") {
        document.querySelector('.sidebar a[href="/dashboard"]').classList.add('active');
    }

// Sao chép clipboard
const copyBtn = document.querySelector('.dropdown-menu button.btn-outline-secondary');
if (copyBtn) {
    copyBtn.addEventListener('click', function () {
        const input = this.previousElementSibling;
        navigator.clipboard.writeText(input.value).then(() => {
            this.innerHTML = '<i class="bi bi-clipboard-check text-success"></i>';
            setTimeout(() => {
                this.innerHTML = '<i class="bi bi-clipboard"></i>';
            }, 1500);
        });
    });
        }

// Giữ panel chuông mở khi chuyển tab
const notifWrapper = document.getElementById('notifDropdownWrapper');
const notifMenu = notifWrapper?.querySelector('.dropdown-menu');

if (notifMenu) {
    notifMenu.addEventListener('click', function (e) {
        // Ngăn dropdown tự đóng khi click trong vùng menu
        e.stopPropagation();
    });

// Ngăn auto-close khi click tab
const tabs = notifMenu.querySelectorAll('[data-bs-toggle="tab"]');
            tabs.forEach(tab => {
    tab.addEventListener('click', function (e) {
        e.preventDefault();
        e.stopPropagation();
        const target = document.querySelector(this.dataset.bsTarget);
        notifMenu.querySelectorAll('.tab-pane').forEach(p => p.classList.remove('show', 'active'));
        target.classList.add('show', 'active');
        notifMenu.querySelectorAll('.nav-link').forEach(t => t.classList.remove('active'));
        this.classList.add('active');
    });
            });
    }
});

// dang xuat
const logoutBtn = document.getElementById('logout-btn');
if (logoutBtn){
    logoutBtn.addEventListener('click', async () => {
        try {
            const resp = await fetch('api/account/auth/logout', { method: 'POST', credentials: 'include' });
            if (resp.ok){
                // reload to update UI (cookies cleared server-side)
                window.location.href = '/';
                return;
            }
            // try to read error
            let txt = '';
            try { txt = await resp.text(); } catch {}
            alert('Đăng xuất thất bại: ' + (txt || resp.status));
        } catch (err) {
            console.error(err); 
            alert('Có lỗi khi đăng xuất.');
        }
    });
}

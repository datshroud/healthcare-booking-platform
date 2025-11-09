document.addEventListener("DOMContentLoaded", function () {
    // Sidebar active
    document.querySelectorAll('.sidebar a').forEach(link => {
        link.addEventListener('click', function () {
            document.querySelectorAll('.sidebar a').forEach(a => a.classList.remove('active'));
            this.classList.add('active');
        });
    });

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
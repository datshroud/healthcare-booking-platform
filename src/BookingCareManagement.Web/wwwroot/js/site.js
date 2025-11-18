document.addEventListener("DOMContentLoaded", function () {
    const userInfo = document.getElementById("userDropdown");

    if (userInfo) {
        const dropdownEl = userInfo.closest('.dropdown');
        const dropdownMenu = dropdownEl ? dropdownEl.querySelector(".dropdown-menu") : null;

        if (dropdownEl && dropdownMenu) {
            const openMenu = () => {
                dropdownEl.classList.add('show');
                dropdownMenu.classList.add('show');
                userInfo.setAttribute('aria-expanded', 'true');
                dropdownMenu.setAttribute('data-open', 'true');
            };

            const closeMenu = () => {
                dropdownEl.classList.remove('show');
                dropdownMenu.classList.remove('show');
                dropdownMenu.removeAttribute('data-open');
                userInfo.setAttribute('aria-expanded', 'false');
            };

            const toggleMenu = () => {
                const isOpen = dropdownMenu.getAttribute('data-open') === 'true';
                if (isOpen) {
                    closeMenu();
                } else {
                    openMenu();
                }
            };

            userInfo.addEventListener('click', (evt) => {
                evt.preventDefault();
                evt.stopPropagation();
                toggleMenu();
            });

            dropdownMenu.addEventListener('click', (evt) => evt.stopPropagation());

            document.addEventListener('click', (evt) => {
                if (!dropdownEl.contains(evt.target)) {
                    closeMenu();
                }
            });

            document.addEventListener('keydown', (evt) => {
                if (evt.key === 'Escape') {
                    closeMenu();
                }
            });
        }
    }
    // logout button handler
    const logoutBtn = document.getElementById('logout-btn');
    if (logoutBtn) {
        logoutBtn.addEventListener('click', async () => {
            try {
                const resp = await fetch('/api/account/auth/logout', { method: 'POST', credentials: 'include' });
                if (resp.ok) {
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
});

// (function () {
// 	function setHeaderFooterVars() {
// 		try {
// 			var header = document.querySelector('header');
// 			var footer = document.querySelector('.custom-footer');
// 			if (header) {
// 				var h = header.offsetHeight;
// 				document.body.style.setProperty('--navbar-height', h + 'px');
// 			}
// 			if (footer) {
// 				var f = footer.offsetHeight;
// 				document.body.style.setProperty('--footer-height', f + 'px');
// 			}
// 		} catch (e) {
// 			// silent
// 		}
// 	}

// 	var resizeTimer;
// 	function onResize() {
// 		clearTimeout(resizeTimer);
// 		resizeTimer = setTimeout(setHeaderFooterVars, 120);
// 	}

// 	if (document.readyState === 'complete' || document.readyState === 'interactive') {
// 		setHeaderFooterVars();
// 	} else {
// 		document.addEventListener('DOMContentLoaded', setHeaderFooterVars);
// 	}

// 	window.addEventListener('resize', onResize);
// 	// also re-run once after fonts/images may have loaded
// 	window.addEventListener('load', function () {
// 		setTimeout(setHeaderFooterVars, 100);
// 	});
// })();

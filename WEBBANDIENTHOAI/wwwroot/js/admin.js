document.addEventListener('DOMContentLoaded', function () {
    /* --- Quick Actions dropdown: smooth, accessible, keyboard navigation --- */
    (function () {
        const root = document.getElementById('quick-root');
        const btn = document.getElementById('quick-actions-btn');
        const menu = document.getElementById('quick-dropdown');
        if (!btn || !menu || !root) return;

        const items = Array.from(menu.querySelectorAll('[role="menuitem"]'));
        let open = false;

        function openMenu() {
            open = true;
            root.classList.add('open');
            btn.setAttribute('aria-expanded', 'true');
            // focus first item after animation starts
            setTimeout(() => { items[0]?.focus(); }, 120);
        }
        function closeMenu(restoreFocus = true) {
            open = false;
            root.classList.remove('open');
            btn.setAttribute('aria-expanded', 'false');
            if (restoreFocus) btn.focus();
        }

        // toggle by click
        btn.addEventListener('click', (e) => {
            e.stopPropagation();
            open ? closeMenu() : openMenu();
        });

        // close clicking outside
        document.addEventListener('click', (e) => {
            if (!root.contains(e.target) && open) closeMenu();
        });

        // keyboard handling
        document.addEventListener('keydown', (e) => {
            if (!open) {
                // open via ArrowDown when button focused
                if ((document.activeElement === btn && e.key === 'ArrowDown')) {
                    e.preventDefault(); openMenu(); return;
                }
                return;
            }

            if (e.key === 'Escape') { e.preventDefault(); closeMenu(); return; }

            const idx = items.indexOf(document.activeElement);
            if (e.key === 'ArrowDown') {
                e.preventDefault(); items[(idx + 1) % items.length]?.focus();
            } else if (e.key === 'ArrowUp') {
                e.preventDefault(); items[(idx - 1 + items.length) % items.length]?.focus();
            } else if (e.key === 'Home') {
                e.preventDefault(); items[0]?.focus();
            } else if (e.key === 'End') {
                e.preventDefault(); items[items.length - 1]?.focus();
            } else if (e.key === 'Tab') {
                // close when tabbing away
                setTimeout(() => { if (!root.contains(document.activeElement)) closeMenu(false); }, 10);
            }
        });

        // close after selecting item (allow item's click handler to run)
        items.forEach(it => it.addEventListener('click', () => { setTimeout(() => closeMenu(false), 10); }));

        // add basic handlers for quick actions (placeholder: replace with real logic)
        document.getElementById('qa-add-product')?.addEventListener('click', () => alert('Quick: Thêm sản phẩm'));
        document.getElementById('qa-add-staff')?.addEventListener('click', () => alert('Quick: Thêm nhân viên'));
        document.getElementById('qa-import')?.addEventListener('click', () => alert('Quick: Import Excel'));
        document.getElementById('qa-export')?.addEventListener('click', () => alert('Quick: Export báo cáo'));
        document.getElementById('qa-create-coupon')?.addEventListener('click', () => alert('Quick: Tạo mã giảm giá'));
        document.getElementById('qa-nhapkho')?.addEventListener('click', () => alert('Quick: Nhập kho'));
        document.getElementById('qa-xuatkho')?.addEventListener('click', () => alert('Quick: Xuất kho'));
    })();

    // Top toolbar buttons (placeholders)
    document.getElementById('btn-import')?.addEventListener('click', () => alert('Import CSV — mở dialog upload'));
    document.getElementById('btn-export')?.addEventListener('click', () => alert('Export CSV — tạo file export'));
    document.getElementById('btn-add-product')?.addEventListener('click', () => alert('Mở form "Thêm sản phẩm"'));
    document.getElementById('btn-nhapkho')?.addEventListener('click', () => alert('Mở form "Nhập kho" (UI/Modal)'));
    document.getElementById('btn-xuatkho')?.addEventListener('click', () => alert('Mở form "Xuất kho" (UI/Modal)'));

    // Simple nav active management (no routing; set active class)
    (function () {
        const nav = document.getElementById('main-nav');
        if (!nav) return;
        nav.addEventListener('click', (e) => {
            const a = e.target.closest('a.nav-link');
            if (!a) return;
            nav.querySelectorAll('a.nav-link').forEach(el => el.classList.remove('nav-link--active'), el.removeAttribute('aria-current'));
            a.classList.add('nav-link--active'); a.setAttribute('aria-current', 'page');
            e.preventDefault();
        });
    })();
});

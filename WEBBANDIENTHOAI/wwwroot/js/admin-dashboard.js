// admin-dashboard.js
(function () {
    // Quick actions dropdown (accessible)
    const quickRoot = document.getElementById('quick-root');
    const quickBtn = document.getElementById('quick-actions-btn');
    const quickMenu = document.getElementById('quick-dropdown');
    if (quickBtn && quickRoot && quickMenu) {
        const items = Array.from(quickMenu.querySelectorAll('[role="menuitem"]'));
        let open = false;
        function openMenu() {
            open = true;
            quickRoot.classList.add('open');
            quickBtn.setAttribute('aria-expanded', 'true');
            setTimeout(() => items[0]?.focus(), 120);
        }
        function closeMenu() {
            open = false;
            quickRoot.classList.remove('open');
            quickBtn.setAttribute('aria-expanded', 'false');
            quickBtn.focus();
        }
        quickBtn.addEventListener('click', e => { e.stopPropagation(); open ? closeMenu() : openMenu(); });
        document.addEventListener('click', e => { if (!quickRoot.contains(e.target) && open) closeMenu(); });
        document.addEventListener('keydown', e => {
            if (!open) {
                if (document.activeElement === quickBtn && e.key === 'ArrowDown') { e.preventDefault(); openMenu(); }
                return;
            }
            if (e.key === 'Escape') { e.preventDefault(); closeMenu(); return; }
            const idx = items.indexOf(document.activeElement);
            if (e.key === 'ArrowDown') { e.preventDefault(); items[(idx + 1) % items.length]?.focus(); }
            if (e.key === 'ArrowUp') { e.preventDefault(); items[(idx - 1 + items.length) % items.length]?.focus(); }
        });
        items.forEach(it => it.addEventListener('click', () => setTimeout(() => quickBtn.focus(), 10)));
    }

    // Toolbar button placeholders (replace with real behavior)
    document.getElementById('btn-import')?.addEventListener('click', () => alert('Import CSV — mở dialog upload'));
    document.getElementById('btn-export')?.addEventListener('click', () => alert('Export CSV — tạo file export'));
    document.getElementById('btn-add-product')?.addEventListener('click', () => {
        // navigate to add product if route exists
        const url = '/Admin/AddProduct';
        window.location.href = url;
    });
    document.getElementById('btn-nhapkho')?.addEventListener('click', () => window.location.href = '/Admin/Inventory');
    document.getElementById('btn-xuatkho')?.addEventListener('click', () => window.location.href = '/Admin/Inventory');

    // Simple nav active toggling
    (function () {
        const nav = document.getElementById('main-nav');
        if (!nav) return;
        nav.addEventListener('click', (e) => {
            const a = e.target.closest('a.nav-link');
            if (!a) return;
            nav.querySelectorAll('a.nav-link').forEach(el => el.classList.remove('nav-link--active'));
            a.classList.add('nav-link--active');
        });
    })();

    // Chart handling: create once, destroy if re-create
    let revenueChart = null;
    let stockChart = null;

    function createRevenueChart(labels, data) {
        const canvas = document.getElementById('revenueChart');
        const placeholder = document.getElementById('revenuePlaceholder');
        if (!canvas) return;

        // if no data -> show placeholder
        if (!labels || !labels.length || !data || !data.length) {
            canvas.style.display = 'none';
            if (placeholder) placeholder.style.display = 'flex';
            return;
        } else {
            canvas.style.display = 'block';
            if (placeholder) placeholder.style.display = 'none';
        }

        // destroy previous instance if exists
        if (revenueChart) {
            try { revenueChart.destroy(); } catch (e) { /* ignore */ }
        }

        const ctx = canvas.getContext('2d');
        revenueChart = new Chart(ctx, {
            type: 'line',
            data: {
                labels: labels,
                datasets: [{
                    label: 'Doanh thu',
                    data: data,
                    tension: 0.3,
                    fill: true,
                    backgroundColor: (ctx) => {
                        // subtle gradient
                        const g = ctx.createLinearGradient(0, 0, 0, 200);
                        g.addColorStop(0, 'rgba(249,115,22,0.16)');
                        g.addColorStop(1, 'rgba(249,115,22,0.02)');
                        return g;
                    },
                    borderColor: 'rgba(249,115,22,1)',
                    pointRadius: 3,
                    pointHoverRadius: 6,
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false },
                    tooltip: { mode: 'index', intersect: false }
                },
                scales: {
                    x: { grid: { display: false }, ticks: { maxRotation: 0 } },
                    y: {
                        grid: { color: '#f3f4f6' },
                        ticks: {
                            callback: value => new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND', maximumFractionDigits: 0 }).format(value)
                        }
                    }
                }
            }
        });
    }

    function createStockDonut(labels, data) {
        const canvas = document.getElementById('stockDonut');
        if (!canvas) return;
        if (!labels || !labels.length || !data || !data.length) return;

        if (stockChart) {
            try { stockChart.destroy(); } catch (e) { }
        }
        const ctx = canvas.getContext('2d');
        stockChart = new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: labels,
                datasets: [{
                    data: data,
                    backgroundColor: ['#f97316', '#fbbf24', '#60a5fa'],
                    hoverOffset: 6,
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { position: 'bottom' }
                }
            }
        });
    }

    // initialize charts on DOMContentLoaded
    document.addEventListener('DOMContentLoaded', function () {
        try {
            const labels = window.DASHBOARD?.revenueLabels ?? [];
            const data = (window.DASHBOARD?.revenueData ?? []).map(v => Number(v));
            createRevenueChart(labels, data);

            const stockLabels = window.DASHBOARD?.stockLabels ?? [];
            const stockData = (window.DASHBOARD?.stockData ?? []).map(v => Number(v));
            if (stockLabels.length && stockData.length) createStockDonut(stockLabels, stockData);
        } catch (ex) {
            console.error('Error initializing dashboard charts', ex);
        }
    });

    // optional: re-render charts when window regained focus (but avoid infinite loops)
    window.addEventListener('focus', () => {
        // If needed, you can refresh only if underlying data changed (not implemented here).
    });

    // Logout confirmation
    document.addEventListener('DOMContentLoaded', function () {
        const logoutBtn = document.getElementById('nav-logout');
        const modal = document.getElementById('logout-confirm-modal');
        const confirmYesBtn = document.getElementById('logout-confirm-yes');
        const confirmNoBtn = document.getElementById('logout-confirm-no');

        if (logoutBtn && modal && confirmYesBtn && confirmNoBtn) {
            logoutBtn.addEventListener('click', function (e) {
                e.preventDefault(); // Prevent navigating to the logout URL immediately
                modal.classList.remove('hidden');
            });

            confirmNoBtn.addEventListener('click', function () {
                modal.classList.add('hidden');
            });

            confirmYesBtn.addEventListener('click', function () {
                // Redirect to the original logout URL
                window.location.href = logoutBtn.href;
            });

            // Also hide the modal if the user clicks outside of it
            modal.addEventListener('click', function(e) {
                if (e.target === modal) {
                    modal.classList.add('hidden');
                }
            });
        }
    });
})();

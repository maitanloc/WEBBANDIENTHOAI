// wwwroot/js/admin-panel-loader.js
(function () {
    const panelRoot = document.getElementById('panel-root');
    const spinner = document.getElementById('panel-spinner');
    const alertsRoot = document.getElementById('panel-alerts');

    if (!panelRoot) return;

    const showSpinner = () => spinner && spinner.classList.remove('hidden');
    const hideSpinner = () => spinner && spinner.classList.add('hidden');

    function setActiveSidebarLink(url) {
        document.querySelectorAll('.panel-link').forEach(a => {
            try {
                const aUrl = new URL(a.getAttribute('data-url') || a.href, location.origin);
                const u = new URL(url, location.origin);
                if (aUrl.pathname === u.pathname) a.classList.add('active-panel-link');
                else a.classList.remove('active-panel-link');
            } catch (e) { }
        });
    }

    async function executeScripts(html) {
        const tmp = document.createElement('div');
        tmp.innerHTML = html;
        const scripts = tmp.querySelectorAll('script');
        for (const s of scripts) {
            if (s.src) {
                await new Promise((res) => {
                    const sc = document.createElement('script');
                    sc.src = s.src;
                    sc.async = false;
                    sc.onload = res;
                    sc.onerror = res;
                    document.body.appendChild(sc);
                });
            } else {
                try {
                    const sc = document.createElement('script');
                    sc.text = s.textContent;
                    document.body.appendChild(sc);
                    document.body.removeChild(sc);
                } catch (err) {
                    console.warn('Inline script exec error', err);
                }
            }
        }
    }

    function extractTempAlerts(html) {
        try {
            const tmp = document.createElement('div');
            tmp.innerHTML = html;
            const alert = tmp.querySelector('#panel-tempdata-alert');
            if (alert && alertsRoot) {
                alertsRoot.innerHTML = alert.innerHTML;
                setTimeout(() => { alertsRoot.innerHTML = ''; }, 5000);
            }
        } catch (e) { }
    }

    async function injectHtml(html, title) {
        panelRoot.innerHTML = html;
        if (title) document.title = title + ' - WebBánĐiệnThoại';
        await executeScripts(html);
        window.scrollTo({ top: 0, behavior: 'smooth' });
    }

    async function loadPanel(url, addToHistory = true) {
        try {
            showSpinner();
            const res = await fetch(url, {
                method: 'GET',
                headers: { 'X-Requested-With': 'XMLHttpRequest', 'Accept': 'text/html' },
                credentials: 'same-origin'
            });

            if (!res.ok) {
                const txt = await res.text();
                const fallback = txt && txt.length < 4000 ? txt : `<div class='p-6 text-red-600'>Lỗi ${res.status} khi tải nội dung. <a href='${url}'>Mở trang đầy đủ</a></div>`;
                await injectHtml(fallback, null);
                hideSpinner();
                return;
            }

            const html = await res.text();
            const pageTitle = res.headers.get('X-Page-Title') || null;
            extractTempAlerts(html);
            await injectHtml(html, pageTitle);
            setActiveSidebarLink(url);
            if (addToHistory) window.history.pushState({ url }, '', url);
        } catch (err) {
            console.error('panel loader error', err);
            await injectHtml(`<div class="p-6 text-red-600">Lỗi kết nối. Vui lòng thử lại.</div>`);
        } finally {
            hideSpinner();
        }
    }

    document.addEventListener('click', function (e) {
        const el = e.target.closest && e.target.closest('.panel-link, [data-url]');
        if (!el) return;
        if (e.ctrlKey || e.metaKey || e.shiftKey || e.button === 1) return;
        const url = el.getAttribute('data-url') || el.href;
        if (!url) return;
        e.preventDefault();
        loadPanel(url, true);
    });

    window.addEventListener('popstate', function (e) {
        const stateUrl = e.state && e.state.url ? e.state.url : (location.pathname + location.search);
        if (stateUrl) loadPanel(stateUrl, false);
    });

    document.addEventListener('DOMContentLoaded', function () {
        setActiveSidebarLink(window.location.href);
    });

    window.adminPanel = { loadPanel };
})();

// wwwroot/js/auth.js
(function () {
    "use strict";

    // helpers
    const $ = (sel) => document.querySelector(sel);
    const $$ = (sel) => Array.from(document.querySelectorAll(sel));

    function id(s) { return document.getElementById(s); }

    // DOM elements
    const btnLogin = id('btnLogin');
    const btnRegister = id('btnRegister');
    const loginPanel = id('loginPanel');
    const registerPanel = id('registerPanel');
    const jumpRegister = id('jumpRegister');
    const jumpLogin = id('jumpLogin');

    function showLogin() {
        if (btnLogin) btnLogin.classList.add('bg-primary', 'text-white');
        if (btnRegister) btnRegister.classList.remove('bg-primary', 'text-white');

        if (registerPanel) registerPanel.classList.add('hidden-right');
        if (registerPanel) registerPanel.classList.remove('visible');

        if (loginPanel) loginPanel.classList.remove('hidden-left');
        if (loginPanel) loginPanel.classList.add('visible');
    }

    function showRegister() {
        if (btnRegister) btnRegister.classList.add('bg-primary', 'text-white');
        if (btnLogin) btnLogin.classList.remove('bg-primary', 'text-white');

        if (loginPanel) loginPanel.classList.add('hidden-left');
        if (loginPanel) loginPanel.classList.remove('visible');

        if (registerPanel) registerPanel.classList.remove('hidden-right');
        if (registerPanel) registerPanel.classList.add('visible');
    }

    // toggle password visibility
    function toggle(buttonId, inputId) {
        const btn = id(buttonId);
        const input = id(inputId);
        if (!btn || !input) return;
        btn.addEventListener('click', () => {
            input.type = input.type === 'password' ? 'text' : 'password';
            const icon = btn.querySelector('.material-symbols-outlined');
            if (icon) icon.textContent = input.type === 'password' ? 'visibility' : 'visibility_off';
        });
    }

    // password strength bar
    function setupPasswordMeter(inputId, barId, labelId) {
        const input = id(inputId);
        const bar = id(barId);
        const label = id(labelId);
        if (!input || !bar || !label) return;
        input.addEventListener('input', () => {
            const v = input.value || "";
            let score = 0;
            if (v.length >= 8) score++;
            if (/[A-Z]/.test(v)) score++;
            if (/[0-9]/.test(v)) score++;
            if (/[^A-Za-z0-9]/.test(v)) score++;
            bar.style.width = ((score / 4) * 100) + '%';
            const labels = ['Yếu', 'TB', 'Tốt', 'Rất tốt', 'Tuyệt vời'];
            label.textContent = labels[Math.min(score, 4)];
        });
    }

    // init DOM on ready
    document.addEventListener('DOMContentLoaded', function () {
        // initial show
        showLogin();

        // attach clicks
        btnLogin && btnLogin.addEventListener('click', showLogin);
        btnRegister && btnRegister.addEventListener('click', showRegister);
        jumpRegister && jumpRegister.addEventListener('click', showRegister);
        jumpLogin && jumpLogin.addEventListener('click', showLogin);

        // toggles
        toggle('toggleLogin', 'loginPw');
        toggle('toggleReg', 'regPw');

        // password meter
        setupPasswordMeter('regPw', 'bar', 'pwText');

        // small safety: prevent double form submit by disabling button after submit
        const forms = $$('form');
        forms.forEach(f => {
            f.addEventListener('submit', (ev) => {
                const submitBtn = f.querySelector('button[type="submit"]');
                if (submitBtn) {
                    submitBtn.disabled = true;
                    // give small timeout; server will redirect / re-enable on error
                    setTimeout(() => { submitBtn.disabled = false; }, 2500);
                }
            });
        });
    });

})();

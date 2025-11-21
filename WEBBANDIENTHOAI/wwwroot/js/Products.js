// Products.js - small UI helpers for product views

(function () {
    // thumbnail click -> swap main image
    document.addEventListener('click', function (e) {
        var t = e.target;
        // if clicked inside a thumb button or the img inside it
        if (t.closest && t.closest('.thumb-btn')) {
            var btn = t.closest('.thumb-btn');
            var src = btn.getAttribute('data-src') || (btn.querySelector('img') && btn.querySelector('img').getAttribute('src'));
            if (src) {
                var main = document.getElementById('mainProductImage');
                if (main) main.src = src;
            }
        }
    });

    // fade out alerts after 5s (matching status page)
    window.setTimeout(function () {
        var alerts = document.querySelectorAll('.alert');
        alerts.forEach(function (a) {
            a.style.transition = 'opacity 0.6s ease';
            a.style.opacity = '0';
            setTimeout(function () { if (a.parentNode) a.parentNode.removeChild(a); }, 700);
        });
    }, 5000);

    // Optional: intercept filter form to keep paging reset to 1
    var filterForm = document.getElementById('filterForm');
    if (filterForm) {
        filterForm.addEventListener('submit', function () {
            // ensure page param removed so server returns page 1
            var pageInput = filterForm.querySelector('input[name="page"]');
            if (pageInput) pageInput.value = 1;
        });
    }
})();

// products-index.js
(function () {
    'use strict';

    // Xử lý form filter
    document.addEventListener('DOMContentLoaded', function () {
        const filterForm = document.getElementById('product-filter-form');
        if (filterForm) {
            filterForm.addEventListener('submit', function (e) {
                e.preventDefault();

                const formData = new FormData(filterForm);
                const searchParams = new URLSearchParams();

                // Thêm tất cả các tham số từ form
                for (const [key, value] of formData.entries()) {
                    if (value) {
                        searchParams.append(key, value);
                    }
                }

                // Chuyển hướng với các tham số filter
                const url = `${window.location.pathname}?${searchParams.toString()}`;
                window.location.href = url;
            });
        }

        // Xử lý nút filter (nếu có ID khác)
        const filterApplyBtn = document.getElementById('filter-apply');
        if (filterApplyBtn) {
            filterApplyBtn.addEventListener('click', function () {
                const filterForm = document.getElementById('product-filter-form');
                if (filterForm) {
                    filterForm.dispatchEvent(new Event('submit'));
                }
            });
        }
    });

})();
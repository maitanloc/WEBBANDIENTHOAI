// products-edit-full.js
(function () {
    'use strict';

    // Form validation
    function initFormValidation() {
        const form = document.querySelector('form');
        if (!form) return;

        const requiredFields = form.querySelectorAll('[required]');

        requiredFields.forEach(field => {
            field.addEventListener('blur', function () {
                validateField(this);
            });

            field.addEventListener('input', function () {
                if (this.classList.contains('error')) {
                    validateField(this);
                }
            });
        });

        function validateField(field) {
            const value = field.value.trim();
            const errorElement = field.parentNode.querySelector('.form-error') || createErrorElement(field);

            if (!value) {
                field.classList.add('error');
                errorElement.textContent = 'Trường này là bắt buộc';
            } else {
                field.classList.remove('error');
                errorElement.textContent = '';

                // Additional validation for specific fields
                if (field.type === 'number' && field.min) {
                    const numValue = parseFloat(value);
                    const minValue = parseFloat(field.min);
                    if (numValue < minValue) {
                        field.classList.add('error');
                        errorElement.textContent = `Giá trị phải lớn hơn hoặc bằng ${minValue}`;
                    }
                }

                if (field.maxLength && value.length > field.maxLength) {
                    field.classList.add('error');
                    errorElement.textContent = `Vượt quá ${field.maxLength} ký tự`;
                }
            }
        }

        function createErrorElement(field) {
            const errorElement = document.createElement('div');
            errorElement.className = 'form-error';
            field.parentNode.appendChild(errorElement);
            return errorElement;
        }
    }

    // Character counter for textareas
    function initCharacterCounters() {
        const textareas = document.querySelectorAll('textarea[maxlength]');

        textareas.forEach(textarea => {
            const maxLength = parseInt(textarea.getAttribute('maxlength'));
            const counterId = textarea.id + '-counter';

            // Create counter element if it doesn't exist
            let counterElement = document.getElementById(counterId);
            if (!counterElement) {
                counterElement = document.createElement('div');
                counterElement.id = counterId;
                counterElement.className = 'text-xs text-gray-500 mt-1';
                textarea.parentNode.appendChild(counterElement);
            }

            function updateCounter() {
                const currentLength = textarea.value.length;
                counterElement.textContent = `${currentLength}/${maxLength} ký tự`;

                if (currentLength > maxLength * 0.9) {
                    counterElement.classList.add('text-orange-500');
                } else {
                    counterElement.classList.remove('text-orange-500');
                }
            }

            textarea.addEventListener('input', updateCounter);
            updateCounter(); // Initialize
        });
    }

    // Image preview and validation
    function initImageHandling() {
        const imageInputs = document.querySelectorAll('input[type="file"][accept*="image"]');

        imageInputs.forEach(input => {
            input.addEventListener('change', function (e) {
                const file = e.target.files[0];
                if (!file) return;

                // Validate file size (3MB)
                if (file.size > 3 * 1024 * 1024) {
                    alert('Kích thước ảnh không được vượt quá 3MB');
                    this.value = '';
                    return;
                }

                // Validate file type
                const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/webp'];
                if (!allowedTypes.includes(file.type)) {
                    alert('Chỉ chấp nhận file ảnh (JPG, JPEG, PNG, GIF, WEBP)');
                    this.value = '';
                    return;
                }

                // Create preview
                const reader = new FileReader();
                reader.onload = function (e) {
                    const previewContainer = input.parentNode.querySelector('.image-preview-container');
                    if (previewContainer) {
                        let img = previewContainer.querySelector('img');
                        if (!img) {
                            img = document.createElement('img');
                            img.className = 'image-preview';
                            previewContainer.innerHTML = '';
                            previewContainer.appendChild(img);
                        }
                        img.src = e.target.result;
                        previewContainer.classList.remove('image-placeholder');
                    }
                };
                reader.readAsDataURL(file);
            });
        });
    }

    // Price formatting
    function initPriceFormatting() {
        const priceInputs = document.querySelectorAll('input[type="number"][step="0.01"]');

        priceInputs.forEach(input => {
            input.addEventListener('blur', function () {
                const value = parseFloat(this.value);
                if (!isNaN(value)) {
                    this.value = value.toFixed(2);
                }
            });
        });
    }

    // Form submission handling
    function initFormSubmission() {
        const form = document.querySelector('form');
        if (!form) return;

        form.addEventListener('submit', function (e) {
            // Validate all required fields
            const requiredFields = form.querySelectorAll('[required]');
            let isValid = true;
            const firstInvalidField = [];

            requiredFields.forEach(field => {
                if (!field.value.trim()) {
                    isValid = false;
                    field.classList.add('error');
                    if (firstInvalidField.length === 0) {
                        firstInvalidField.push(field);
                    }
                }
            });

            if (!isValid) {
                e.preventDefault();
                alert('Vui lòng điền đầy đủ các trường bắt buộc (đánh dấu *)');
                if (firstInvalidField[0]) {
                    firstInvalidField[0].focus();
                }
                return;
            }

            // Show loading state
            const submitButton = form.querySelector('button[type="submit"]');
            if (submitButton) {
                submitButton.disabled = true;
                submitButton.innerHTML = '<span class="animate-spin">⟳</span> Đang xử lý...';
            }

            // Show global loader
            if (window.showLoader) {
                window.showLoader();
            }
        });
    }

    // Initialize everything when DOM is loaded
    document.addEventListener('DOMContentLoaded', function () {
        initFormValidation();
        initCharacterCounters();
        initImageHandling();
        initPriceFormatting();
        initFormSubmission();

        console.log('Product edit form initialized');
    });

    // Export functions for potential external use
    window.ProductEdit = {
        initFormValidation,
        initCharacterCounters,
        initImageHandling,
        initPriceFormatting,
        initFormSubmission
    };

})();
// products-details.js
(function () {
    'use strict';

    // Image gallery functionality
    function initImageGallery() {
        const mainImage = document.getElementById('main-product-image');
        const thumbnails = document.querySelectorAll('.image-thumbnail');

        if (!mainImage || thumbnails.length === 0) return;

        thumbnails.forEach(thumb => {
            thumb.addEventListener('click', function () {
                const img = this.querySelector('img');
                if (img && img.src) {
                    // Update main image
                    mainImage.src = img.src;

                    // Update active state
                    thumbnails.forEach(t => t.classList.remove('active'));
                    this.classList.add('active');
                }
            });
        });
    }

    // Print product details
    function initPrintFunctionality() {
        const printBtn = document.getElementById('print-product');
        if (printBtn) {
            printBtn.addEventListener('click', function () {
                window.print();
            });
        }
    }

    // Share product functionality
    function initShareFunctionality() {
        const shareBtn = document.getElementById('share-product');
        if (shareBtn && navigator.share) {
            shareBtn.style.display = 'inline-flex';
            shareBtn.addEventListener('click', async function () {
                try {
                    await navigator.share({
                        title: document.title,
                        text: 'Xem sản phẩm này',
                        url: window.location.href,
                    });
                } catch (err) {
                    console.log('Error sharing:', err);
                }
            });
        }
    }

    // Copy product link
    function initCopyLinkFunctionality() {
        const copyBtn = document.getElementById('copy-product-link');
        if (copyBtn) {
            copyBtn.addEventListener('click', function () {
                const url = window.location.href;
                navigator.clipboard.writeText(url).then(() => {
                    // Show success message
                    const originalText = this.innerHTML;
                    this.innerHTML = '<span class="material-symbols-outlined text-sm">check</span> Đã copy';
                    this.classList.add('bg-green-600');

                    setTimeout(() => {
                        this.innerHTML = originalText;
                        this.classList.remove('bg-green-600');
                    }, 2000);
                }).catch(err => {
                    console.error('Failed to copy:', err);
                });
            });
        }
    }

    // Initialize everything when DOM is loaded
    document.addEventListener('DOMContentLoaded', function () {
        initImageGallery();
        initPrintFunctionality();
        initShareFunctionality();
        initCopyLinkFunctionality();

        // Add loading state to images
        const images = document.querySelectorAll('img');
        images.forEach(img => {
            img.addEventListener('load', function () {
                this.classList.remove('image-loading');
            });

            if (!img.complete) {
                img.classList.add('image-loading');
            }
        });
    });

    // Export functions for potential external use
    window.ProductDetails = {
        initImageGallery,
        initPrintFunctionality,
        initShareFunctionality,
        initCopyLinkFunctionality
    };

})();
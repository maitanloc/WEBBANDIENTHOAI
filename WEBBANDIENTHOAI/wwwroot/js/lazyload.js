document.addEventListener("DOMContentLoaded", function () {
  let lazyImages = [].slice.call(document.querySelectorAll("img.lazy"));

  if ("IntersectionObserver" in window) {
    let lazyImageObserver = new IntersectionObserver(
      function (entries, observer) {
        entries.forEach(function (entry) {
          if (entry.isIntersecting) {
            let lazyImage = entry.target;
            // Chuyển src từ data-src
            if (lazyImage.dataset.src) {
              lazyImage.src = lazyImage.dataset.src;
            }
            lazyImage.classList.remove("lazy");
            lazyImage.classList.add("loaded"); // Dành cho các CSS transitions (nếu cần)
            lazyImageObserver.unobserve(lazyImage);
          }
        });
      },
      {
        // Tải sớm khi cách viewport 50px
        rootMargin: "0px 0px 50px 0px",
      },
    );

    lazyImages.forEach(function (lazyImage) {
      lazyImageObserver.observe(lazyImage);
    });
  } else {
    // Fallback cho trình duyệt cũ: Nạp tức thời
    lazyImages.forEach(function (lazyImage) {
      if (lazyImage.dataset.src) {
        lazyImage.src = lazyImage.dataset.src;
      }
    });
  }
});

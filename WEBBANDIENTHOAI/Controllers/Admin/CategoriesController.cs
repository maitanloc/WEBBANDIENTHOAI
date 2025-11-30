using Microsoft.AspNetCore.Mvc;
using WEBBANDIENTHOAI.Models;
using System.Threading.Tasks;
using WEBBANDIENTHOAI.Repository.Admin;

namespace WEBBANDIENTHOAI.Controllers.Admin
{
    public class CategoriesController : Controller
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoriesController(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        // GET: Categories
        public async Task<IActionResult> Index()
        {
            var categories = await _categoryRepository.GetAllWithProductsAsync();
            return View(categories);
        }

        // POST: Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCategoryDto dto)
        {
            if (ModelState.IsValid)
            {
                var category = new Category
                {
                    CategoryName = dto.CategoryName,
                    Description = dto.Description,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _categoryRepository.CreateAsync(category);

                if (result)
                {
                    TempData["Success"] = "Thêm danh mục thành công!";
                }
                else
                {
                    TempData["Error"] = "Lỗi khi thêm danh mục!";
                }

                return RedirectToAction(nameof(Index));
            }

            // Nếu có lỗi validation, trả về view với thông báo lỗi
            var categories = await _categoryRepository.GetAllWithProductsAsync();
            return View("Index", categories);
        }

        // POST: Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateCategoryDto dto)
        {
            if (ModelState.IsValid)
            {
                var category = await _categoryRepository.GetByIdAsync(dto.CategoryId);
                if (category == null)
                {
                    TempData["Error"] = "Không tìm thấy danh mục!";
                    return RedirectToAction(nameof(Index));
                }

                category.CategoryName = dto.CategoryName;
                category.Description = dto.Description;

                var result = await _categoryRepository.UpdateAsync(category);

                if (result)
                {
                    TempData["Success"] = "Cập nhật danh mục thành công!";
                }
                else
                {
                    TempData["Error"] = "Lỗi khi cập nhật danh mục!";
                }

                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "Dữ liệu không hợp lệ!";
            return RedirectToAction(nameof(Index));
        }

        // POST: Categories/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var hasProducts = await _categoryRepository.HasProductsAsync(id);
            if (hasProducts)
            {
                TempData["Error"] = "Không thể xóa danh mục này vì đang có sản phẩm thuộc danh mục.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _categoryRepository.DeleteAsync(id);

            if (result)
            {
                TempData["Success"] = "Xóa danh mục thành công!";
            }
            else
            {
                TempData["Error"] = "Lỗi khi xóa danh mục!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
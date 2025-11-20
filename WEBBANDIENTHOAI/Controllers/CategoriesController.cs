using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using WEBBANDIENTHOAI.Data;
using WEBBANDIENTHOAI.Models;

namespace WEBBANDIENTHOAI.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly AppDbContext _context;

        public CategoriesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Categories
        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories
                .Include(c => c.Products)
                .ToListAsync();
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

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Thêm danh mục thành công!";
                return RedirectToAction(nameof(Index));
            }

            // Nếu có lỗi validation, trả về view với thông báo lỗi
            var categories = await _context.Categories.ToListAsync();
            return View("Index", categories);
        }

        // POST: Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateCategoryDto dto)
        {
            if (ModelState.IsValid)
            {
                var category = await _context.Categories.FindAsync(dto.CategoryId);
                if (category == null)
                {
                    TempData["Error"] = "Không tìm thấy danh mục!";
                    return RedirectToAction(nameof(Index));
                }

                category.CategoryName = dto.CategoryName;
                category.Description = dto.Description;

                try
                {
                    _context.Categories.Update(category);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Cập nhật danh mục thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(dto.CategoryId))
                    {
                        TempData["Error"] = "Danh mục không tồn tại!";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            TempData["Error"] = "Dữ liệu không hợp lệ!";
            return RedirectToAction(nameof(Index));
        }

        // POST: Categories/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
            {
                TempData["Error"] = "Không tìm thấy danh mục!";
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra xem danh mục có sản phẩm không
            if (category.Products?.Any() == true)
            {
                TempData["Error"] = "Không thể xóa danh mục này vì đang có sản phẩm thuộc danh mục.";
                return RedirectToAction(nameof(Index));
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Xóa danh mục thành công!";
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.CategoryId == id);
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using WEBBANDIENTHOAI.Data;
using WEBBANDIENTHOAI.Models;

namespace WEBBANDIENTHOAI.Controllers
{
    public class ProductStatusesController : Controller
    {
        private readonly AppDbContext _context;

        public ProductStatusesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Product/ProductStatuses
        [Route("Product/ProductStatuses")]
        public async Task<IActionResult> ProductStatuses()
        {
            var statuses = await _context.ProductStatuses
                .Include(ps => ps.Products)
                .ToListAsync();
            return View("~/Views/Product/ProductStatuses.cshtml", statuses);
        }

        // POST: Product/CreateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Product/CreateStatus")]
        public async Task<IActionResult> CreateStatus(CreateProductStatusDto dto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var status = new ProductStatus
                    {
                        StatusName = dto.StatusName,
                        Description = dto.Description
                        // KHÔNG gán giá trị cho StatusId - để DB tự generate
                    };

                    _context.ProductStatuses.Add(status);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Thêm trạng thái thành công!";
                    return RedirectToAction("ProductStatuses", "ProductStatuses");
                }
                catch (DbUpdateException ex)
                {
                    // Log lỗi chi tiết
                    Console.WriteLine($"Lỗi khi thêm trạng thái: {ex.InnerException?.Message}");
                    TempData["Error"] = "Lỗi khi thêm trạng thái. Vui lòng thử lại.";
                }
            }
            else
            {
                TempData["Error"] = "Dữ liệu không hợp lệ!";
            }

            return RedirectToAction("ProductStatuses", "ProductStatuses");
        }
        // POST: Product/EditStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Product/EditStatus")]
        public async Task<IActionResult> EditStatus(UpdateProductStatusDto dto)
        {
            if (ModelState.IsValid)
            {
                var status = await _context.ProductStatuses.FindAsync(dto.StatusId);
                if (status == null)
                {
                    TempData["Error"] = "Không tìm thấy trạng thái!";
                    return RedirectToAction("ProductStatuses", "ProductStatuses");
                }

                status.StatusName = dto.StatusName;
                status.Description = dto.Description;

                _context.ProductStatuses.Update(status);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Cập nhật trạng thái thành công!";
                return RedirectToAction("ProductStatuses", "ProductStatuses");
            }

            TempData["Error"] = "Dữ liệu không hợp lệ!";
            return RedirectToAction("ProductStatuses", "ProductStatuses");
        }

        // POST: Product/DeleteStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Product/DeleteStatus")]
        public async Task<IActionResult> DeleteStatus(byte id)
        {
            var status = await _context.ProductStatuses
                .Include(ps => ps.Products)
                .FirstOrDefaultAsync(ps => ps.StatusId == id);

            if (status == null)
            {
                TempData["Error"] = "Không tìm thấy trạng thái!";
                return RedirectToAction("ProductStatuses", "ProductStatuses");
            }

            if (status.Products?.Any() == true)
            {
                TempData["Error"] = "Không thể xóa trạng thái này vì đang có sản phẩm sử dụng!";
                return RedirectToAction("ProductStatuses", "ProductStatuses");
            }

            _context.ProductStatuses.Remove(status);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Xóa trạng thái thành công!";
            return RedirectToAction("ProductStatuses", "ProductStatuses");
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using WEBBANDIENTHOAI.Models;
using System.Threading.Tasks;
using WEBBANDIENTHOAI.Repository.Admin;

namespace WEBBANDIENTHOAI.Controllers.Admin
{
    public class ProductStatusesController : Controller
    {
        private readonly IProductStatusRepository _productStatusRepository;

        public ProductStatusesController(IProductStatusRepository productStatusRepository)
        {
            _productStatusRepository = productStatusRepository;
        }

        // GET: Product/ProductStatuses
        [Route("Product/ProductStatuses")]
        public async Task<IActionResult> ProductStatuses()
        {
            var statuses = await _productStatusRepository.GetAllWithProductsAsync();
            return View("~/Views/Products/ProductStatuses.cshtml", statuses);
        }

        // POST: Product/CreateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Products/CreateStatus")]
        public async Task<IActionResult> CreateStatus(CreateProductStatusDto dto)
        {
            if (ModelState.IsValid)
            {
                var status = new ProductStatus
                {
                    StatusName = dto.StatusName,
                    Description = dto.Description
                };

                var result = await _productStatusRepository.CreateAsync(status);

                if (result)
                {
                    TempData["Success"] = "Thêm trạng thái thành công!";
                }
                else
                {
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
        [Route("Products/EditStatus")]
        public async Task<IActionResult> EditStatus(UpdateProductStatusDto dto)
        {
            if (ModelState.IsValid)
            {
                var status = await _productStatusRepository.GetByIdAsync(dto.StatusId);
                if (status == null)
                {
                    TempData["Error"] = "Không tìm thấy trạng thái!";
                    return RedirectToAction("ProductStatuses", "ProductStatuses");
                }

                status.StatusName = dto.StatusName;
                status.Description = dto.Description;

                var result = await _productStatusRepository.UpdateAsync(status);

                if (result)
                {
                    TempData["Success"] = "Cập nhật trạng thái thành công!";
                }
                else
                {
                    TempData["Error"] = "Lỗi khi cập nhật trạng thái!";
                }
            }
            else
            {
                TempData["Error"] = "Dữ liệu không hợp lệ!";
            }

            return RedirectToAction("ProductStatuses", "ProductStatuses");
        }

        // POST: Product/DeleteStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Products/DeleteStatus")]
        public async Task<IActionResult> DeleteStatus(byte id)
        {
            var hasProducts = await _productStatusRepository.HasProductsAsync(id);
            if (hasProducts)
            {
                TempData["Error"] = "Không thể xóa trạng thái này vì đang có sản phẩm sử dụng!";
                return RedirectToAction("ProductStatuses", "ProductStatuses");
            }

            var result = await _productStatusRepository.DeleteAsync(id);

            if (result)
            {
                TempData["Success"] = "Xóa trạng thái thành công!";
            }
            else
            {
                TempData["Error"] = "Lỗi khi xóa trạng thái!";
            }

            return RedirectToAction("ProductStatuses", "ProductStatuses");
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using WEBBANDIENTHOAI.Data;
using WEBBANDIENTHOAI.Models;

namespace WEBBANDIENTHOAI.Controllers.Admin
{
    public class ProductOptionsController : Controller
    {
        private readonly AppDbContext _context;

        public ProductOptionsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: ProductOptions
        public async Task<IActionResult> Index(int? productId, string groupName)
        {
            var query = _context.ProductOptions.Include(p => p.Product).AsQueryable();

            if (productId.HasValue)
            {
                query = query.Where(o => o.ProductId == productId.Value);
                ViewBag.SelectedProductId = productId.Value;
            }

            if (!string.IsNullOrEmpty(groupName))
            {
                query = query.Where(o => o.GroupName == groupName);
                ViewBag.SelectedGroupName = groupName;
            }

            ViewBag.Products = new SelectList(await _context.Products.ToListAsync(), "ProductId", "Name", productId);

            return View(await query.OrderBy(o => o.ProductId).ThenBy(o => o.GroupName).ThenBy(o => o.DisplayOrder).ToListAsync());
        }

        // GET: ProductOptions/Create
        public IActionResult Create()
        {
            ViewBag.ProductId = new SelectList(_context.Products, "ProductId", "Name");
            return View();
        }

        // POST: ProductOptions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductId,GroupName,OptionName,AdditionalPrice,DisplayOrder,IsActive")] ProductOption productOption)
        {
            if (ModelState.IsValid)
            {
                _context.Add(productOption);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Thêm tùy chọn thành công!";
                return RedirectToAction(nameof(Index), new { productId = productOption.ProductId });
            }
            ViewBag.ProductId = new SelectList(_context.Products, "ProductId", "Name", productOption.ProductId);
            return View(productOption);
        }

        // GET: ProductOptions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var productOption = await _context.ProductOptions.FindAsync(id);
            if (productOption == null) return NotFound();

            ViewBag.ProductId = new SelectList(_context.Products, "ProductId", "Name", productOption.ProductId);
            return View(productOption);
        }

        // POST: ProductOptions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductOptionId,ProductId,GroupName,OptionName,AdditionalPrice,DisplayOrder,IsActive")] ProductOption productOption)
        {
            if (id != productOption.ProductOptionId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(productOption);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật tùy chọn thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductOptionExists(productOption.ProductOptionId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index), new { productId = productOption.ProductId });
            }
            ViewBag.ProductId = new SelectList(_context.Products, "ProductId", "Name", productOption.ProductId);
            return View(productOption);
        }

        // POST: ProductOptions/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var productOption = await _context.ProductOptions.FindAsync(id);
            if (productOption != null)
            {
                _context.ProductOptions.Remove(productOption);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Xóa tùy chọn thành công!";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ProductOptionExists(int id)
        {
            return _context.ProductOptions.Any(e => e.ProductOptionId == id);
        }
    }
}

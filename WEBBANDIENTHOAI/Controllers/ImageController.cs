using Microsoft.AspNetCore.Mvc;
using WEBBANDIENTHOAI.Data;

namespace WEBBANDIENTHOAI.Controllers
{
    public class ImageController : Controller
    {
        private readonly AppDbContext _context;

        public ImageController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult ProductImage(int id)
        {
            var img = _context.ProductImages.FirstOrDefault(x => x.ImageId == id);

            if (img == null || img.ImagePath == null)
                return File("~/images/default-product.png", "image/png");

            return File(img.ImagePath, "image/jpeg");
        }
    }
}

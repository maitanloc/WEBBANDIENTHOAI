using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEBBANDIENTHOAI.Data;
using WEBBANDIENTHOAI.Helpers;
using WEBBANDIENTHOAI.Models;
using WEBBANDIENTHOAI.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using WEBBANDIENTHOAI.Repository.TaiKhoan;

namespace WEBBANDIENTHOAI.Controllers.NguoiDung
{
    public class HomeUserController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ICustomerRepository _customerRepository;

        public HomeUserController(AppDbContext context, ICustomerRepository customerRepository)
        {
            _context = context;
            _customerRepository = customerRepository;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Kiểm tra đăng nhập
                var userId = HttpContext.Session.GetString("UserId");
                var role = HttpContext.Session.GetString("RoleName");

                if (string.IsNullOrEmpty(userId) || role != "Customer")
                {
                    return RedirectToAction("Login", "Account");
                }

                // Lấy thông tin khách hàng từ session
                var customerId = int.Parse(userId);
                var customer = await _customerRepository.GetCustomerByIdAsync(customerId);

                if (customer != null)
                {
                    // Truyền thông tin khách hàng qua ViewBag
                    ViewBag.CustomerName = customer.FullName;
                    ViewBag.CustomerEmail = customer.Email;
                    ViewBag.CustomerPhone = customer.Phone;
                    ViewBag.CustomerAddress = customer.Address;
                    ViewBag.IsLoggedIn = true;
                }
                else
                {
                    ViewBag.IsLoggedIn = false;
                }


                // ========== LẤY 4 ĐIỆN THOẠI GIÁ CAO NHẤT ==========
                var topPhones = await _context.Products
                    .Include(p => p.PrimaryImage)
                    .Where(p => p.StatusId == 1 && p.CategoryId == 1)
                    .OrderByDescending(p => p.Price)
                    .Take(4)
                    .ToListAsync();

                // ========== LẤY 4 LAPTOP GIÁ CAO NHẤT ==========
                var topLaptops = await _context.Products
                    .Include(p => p.PrimaryImage)
                    .Where(p => p.StatusId == 1 && p.CategoryId == 2)
                    .OrderByDescending(p => p.Price)
                    .Take(4)
                    .ToListAsync();

                ViewBag.TopPhones = topPhones;
                ViewBag.TopLaptops = topLaptops;

                // Lấy banner
                var bannerPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/banners");
                var bannerFiles = Directory.Exists(bannerPath)
                    ? Directory.GetFiles(bannerPath).Select(Path.GetFileName).ToList()
                    : new List<string>();

                ViewBag.Banners = bannerFiles;

                return View();
            }
            catch (Exception ex)
            {
                // Log lỗi và redirect đến trang login nếu có lỗi
                Console.WriteLine($"Lỗi HomeUser Index: {ex.Message}");
                return RedirectToAction("Login", "Account");
            }
        }

        

        // Các action khác giữ nguyên...
        public async Task<IActionResult> ProductDetails(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductStatus)
                .Include(p => p.PrimaryImage)
                .Include(p => p.Images)
                .Include(p => p.PhoneConfiguration)
                .Include(p => p.LaptopConfiguration)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            // Lấy thông tin khách hàng nếu đã đăng nhập
            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("RoleName");

            if (!string.IsNullOrEmpty(userId) && role == "Customer")
            {
                var customerId = int.Parse(userId);
                var customer = await _customerRepository.GetCustomerByIdAsync(customerId);

                if (customer != null)
                {
                    ViewBag.CustomerName = customer.FullName;
                    ViewBag.CustomerEmail = customer.Email;
                    ViewBag.CustomerPhone = customer.Phone;
                    ViewBag.IsLoggedIn = true;
                }
            }

            var orderedImages = new List<ProductImage>();
            if (product.PrimaryImage != null)
            {
                orderedImages.Add(product.PrimaryImage);
            }
            if (product.Images != null)
            {
                orderedImages.AddRange(product.Images.Where(img => img.ImageId != product.ImageId));
            }

            var viewModel = new ProductDetailsVm
            {
                ProductId = product.ProductId,
                SKU = product.SKU,
                Name = product.Name,
                Brand = product.Brand,
                Price = product.Price,
                OldPrice = product.OldPrice,
                StockCode = product.StockCode,
                Color = product.Color,
                Size = product.Size,
                ShortDescription = product.ShortDescription,
                StatusId = product.StatusId,
                StatusName = product.ProductStatus?.StatusName,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.CategoryName,
                CreatedAt = product.CreatedAt,
                PrimaryImageId = product.ImageId,
                Images = orderedImages.Select(img => new ProductImageVm { ImageId = img.ImageId }).ToList(),

                PhoneConfiguration = product.PhoneConfiguration != null ? new PhoneConfigurationVm
                {
                    CPU = product.PhoneConfiguration.CPU,
                    Cores = product.PhoneConfiguration.Cores,
                    RAM = product.PhoneConfiguration.RAM,
                    InternalStorage = product.PhoneConfiguration.InternalStorage,
                    Battery = product.PhoneConfiguration.Battery,
                    Camera = product.PhoneConfiguration.Camera
                } : null,

                LaptopConfiguration = product.LaptopConfiguration != null ? new LaptopConfigurationVm
                {
                    CPU = product.LaptopConfiguration.CPU,
                    RAM = product.LaptopConfiguration.RAM,
                    Storage = product.LaptopConfiguration.Storage,
                    Battery = product.LaptopConfiguration.Battery,
                    OperatingSystem = product.LaptopConfiguration.OperatingSystem,
                    ScreenSize = product.LaptopConfiguration.ScreenSize
                } : null,
            };

            return View(viewModel);
        }

        public async Task<JsonResult> GetRelatedProducts(int categoryId, int excludeProductId, int count = 4)
        {
            var relatedProducts = await _context.Products
                .Include(p => p.PrimaryImage)
                .Where(p => p.CategoryId == categoryId && p.ProductId != excludeProductId)
                .OrderBy(p => Guid.NewGuid())
                .Take(count)
                .Select(p => new
                {
                    productId = p.ProductId,
                    name = p.Name,
                    price = p.Price,
                    primaryImageId = p.ImageId
                })
                .ToListAsync();

            return Json(relatedProducts);
        }

        public IActionResult GetProductImage(int imageId)
        {
            var image = _context.ProductImages.FirstOrDefault(pi => pi.ImageId == imageId);

            if (image?.ImagePath != null && image.ImagePath.Length > 0)
            {
                string contentType = ImageHelper.GetContentType(image.ImagePath);
                return File(image.ImagePath, contentType);
            }

            return File("~/images/default-product.png", "image/png");
        }

        public async Task<IActionResult> Smartphones(string? brand, string? sort)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("RoleName");

            if (!string.IsNullOrEmpty(userId) && role == "Customer")
            {
                var customerId = int.Parse(userId);
                var customer = await _customerRepository.GetCustomerByIdAsync(customerId);

                if (customer != null)
                {
                    ViewBag.CustomerName = customer.FullName;
                    ViewBag.CustomerEmail = customer.Email;
                    ViewBag.CustomerPhone = customer.Phone;
                    ViewBag.IsLoggedIn = true;
                }
            }

            var query = _context.Products
                .Include(p => p.PrimaryImage)
                .Where(p => p.CategoryId == 1 && p.StatusId == 1);

            if (!string.IsNullOrEmpty(brand))
                query = query.Where(p => p.Brand == brand);

            switch (sort)
            {
                case "price_asc":
                    query = query.OrderBy(p => p.Price);
                    break;

                case "price_desc":
                    query = query.OrderByDescending(p => p.Price);
                    break;

                case "newest":
                    query = query.OrderByDescending(p => p.CreatedAt);
                    break;

                default:
                    query = query.OrderByDescending(p => p.CreatedAt);
                    break;
            }

            ViewBag.Brands = await _context.Products
                .Where(p => p.CategoryId == 1)
                .Select(p => p.Brand)
                .Distinct()
                .ToListAsync();

            return View(await query.ToListAsync());
        }


                public async Task<IActionResult> Laptops(string? brand, string? sort)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("RoleName");

            if (!string.IsNullOrEmpty(userId) && role == "Customer")
            {
                var customerId = int.Parse(userId);
                var customer = await _customerRepository.GetCustomerByIdAsync(customerId);

                if (customer != null)
                {
                    ViewBag.CustomerName = customer.FullName;
                    ViewBag.CustomerEmail = customer.Email;
                    ViewBag.CustomerPhone = customer.Phone;
                    ViewBag.IsLoggedIn = true;
                }
            }
            var query = _context.Products
                .Include(p => p.PrimaryImage)
                .Where(p => p.CategoryId == 2 && p.StatusId == 1);

            if (!string.IsNullOrEmpty(brand))
                query = query.Where(p => p.Brand == brand);

            switch (sort)
            {
                case "price_asc":
                    query = query.OrderBy(p => p.Price);
                    break;

                case "price_desc":
                    query = query.OrderByDescending(p => p.Price);
                    break;

                case "newest":
                    query = query.OrderByDescending(p => p.CreatedAt);
                    break;

                default:
                    query = query.OrderByDescending(p => p.CreatedAt);
                    break;
            }

            ViewBag.Brands = await _context.Products
                .Where(p => p.CategoryId == 2)
                .Select(p => p.Brand)
                .Distinct()
                .ToListAsync();

            return View(await query.ToListAsync());
        }

        public async Task<IActionResult> AllProducts(string? brand, string? sort, int page = 1)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("RoleName");

            if (!string.IsNullOrEmpty(userId) && role == "Customer")
            {
                var customerId = int.Parse(userId);
                var customer = await _customerRepository.GetCustomerByIdAsync(customerId);

                if (customer != null)
                {
                    ViewBag.CustomerName = customer.FullName;
                    ViewBag.CustomerEmail = customer.Email;
                    ViewBag.CustomerPhone = customer.Phone;
                    ViewBag.IsLoggedIn = true;
                }
            }

            var query = _context.Products
                .Include(p => p.PrimaryImage)
                .Where(p => p.StatusId == 1);

            if (!string.IsNullOrEmpty(brand))
                query = query.Where(p => p.Brand == brand);

            switch (sort)
            {
                case "price_asc":
                    query = query.OrderBy(p => p.Price);
                    break;

                case "price_desc":
                    query = query.OrderByDescending(p => p.Price);
                    break;

                case "newest":
                    query = query.OrderByDescending(p => p.CreatedAt);
                    break;

                default:
                    query = query.OrderByDescending(p => p.CreatedAt);
                    break;
            }

            ViewBag.Brands = await _context.Products
                .Select(p => p.Brand)
                .Distinct()
                .ToListAsync();

            const int PageSize = 12;
            var totalItems = await query.CountAsync();
            var products = await query.Skip((page - 1) * PageSize).Take(PageSize).ToListAsync();

            ViewData["TotalPages"] = (int)Math.Ceiling(totalItems / (double)PageSize);
            ViewData["CurrentPage"] = page;


            return View(products);
        }



    }
}
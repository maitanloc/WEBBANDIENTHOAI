using FluentAssertions.Common;
using Microsoft.EntityFrameworkCore;
using WEBBANDIENTHOAI.Controllers.NguoiDung;
using WEBBANDIENTHOAI.Data;
using WEBBANDIENTHOAI.Repository.Admin;
using WEBBANDIENTHOAI.Repository.NguoiDung;
using WEBBANDIENTHOAI.Repository.TaiKhoan;
using WEBBANDIENTHOAI.Services;
using WEBBANDIENTHOAI.Services.VNPay;

var builder = WebApplication.CreateBuilder(args);

// ========================
// 1) Load connection string
// ========================
var conn = builder.Configuration.GetConnectionString("DefaultConnection")
           ?? "Data Source=DELL\\SQLEXPRESS02;Initial Catalog=web_dien_tu;Integrated Security=True;TrustServerCertificate=True";

// ========================
// 2) Add MVC services
// ========================
builder.Services.AddControllersWithViews().AddRazorOptions(options =>
{
    // Cho controller thường (không phải Area) tìm view bên ngoài
    options.ViewLocationFormats.Add("/Views/{1}/{0}.cshtml");
    options.ViewLocationFormats.Add("/Views/Shared/{0}.cshtml");
    
    // Cho Area controller
    options.AreaViewLocationFormats.Add("/Views/{2}/{1}/{0}.cshtml");
    options.AreaViewLocationFormats.Add("/Views/Shared/{0}.cshtml");
});

// ========================
// 3) Add DbContext với retry policy và Lazy Loading Proxy
// ========================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseLazyLoadingProxies() // <--- Kích hoạt Lazy Loading Proxy ở đây
           .UseSqlServer(conn, sqlOptions =>
    {
        // TẮT TẠM RETRY STRATEGY ĐỂ DEBUG
        // sqlOptions.EnableRetryOnFailure(
        //     maxRetryCount: 5,
        //     maxRetryDelay: TimeSpan.FromSeconds(30),
        //     errorNumbersToAdd: null);
        sqlOptions.CommandTimeout(60);
    }));
// ========================
// 4) Register Repositories
// ========================
builder.Services.AddScoped<IProductStatusRepository, ProductStatusRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IKhohangRepository, KhohangRepository>();
builder.Services.AddScoped<IOrderStatusRepository, OrderStatusRepository>();
// Thêm vào phần 4) Register Repositories
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderDetailsRepository, OrderDetailsRepository>();
// Đăng ký ExportReceiptRepository
builder.Services.AddScoped<IExportReceiptRepository, ExportReceiptRepository>();
// Thêm vào phần ConfigureServices hoặc builder.Services
builder.Services.AddScoped<IExportReceiptDetailsRepository, ExportReceiptDetailsRepository>();
// Đăng ký ImportReceiptRepository
builder.Services.AddScoped<IImportReceiptRepository, ImportReceiptRepository>();

// ===== PROMOTION & LOYALTY =====
builder.Services.AddScoped<IPromotionService, PromotionService>();

// Đăng ký services
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IOTPService, OTPService>();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IMailService, MailService>();

//Connect VNPay API
builder.Services.AddScoped<IVnPayService, VnPayService>();

// ===============================================
// 5) Cấu hình Session - QUAN TRỌNG
// ===============================================
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(4);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = "PhoneShop.Session";
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// 5a) Đăng ký HttpClient để gọi API Gemini
builder.Services.AddHttpClient<ChatController>();

// 5b) Đăng ký IHttpContextAccessor để sử dụng trong view
builder.Services.AddHttpContextAccessor();

// ========================
// Build app
// ========================
var app = builder.Build();

// ========================
// 6) Middleware pipeline - SỬA LẠI THỨ TỰ
// ========================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseStaticFiles();
app.UseRouting();

// QUAN TRỌNG: UseSession PHẢI được gọi trước khi sử dụng Session
app.UseSession();

// MIDDLEWARE DEBUG - PHẢI ĐẶT SAU UseSession()
app.Use(async (context, next) =>
{
    // Chỉ debug các request đến OrderHistory để tránh log nhiều
    if (context.Request.Path.StartsWithSegments("/OrderHistory"))
    {
        var userId = context.Session.GetString("UserId");
        var role = context.Session.GetString("RoleName");
        var customerId = context.Session.GetString("CustomerId");

        Console.WriteLine($"=== SESSION DEBUG ===");
        Console.WriteLine($"Path: {context.Request.Path}");
        Console.WriteLine($"UserId: {userId}");
        Console.WriteLine($"CustomerId: {customerId}");
        Console.WriteLine($"Role: {role}");
        Console.WriteLine($"=== END DEBUG ===");
    }

    await next();
});



app.UseAuthorization();

// ========================
// 7) Routes
// ========================
app.MapControllerRoute(
    name: "Admin",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ========================
// 8) Khởi tạo Database & Admin mặc định (dành cho DB trống)
// ========================
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        // 1. Tạo Role Admin nếu chưa có
        var adminRole = context.Roles.FirstOrDefault(r => r.RoleName == "Admin");
        if (adminRole == null)
        {
            adminRole = new WEBBANDIENTHOAI.Models.Role { RoleName = "Admin", Description = "Quản trị viên hệ thống" };
            context.Roles.Add(adminRole);
            context.SaveChanges();
            Console.WriteLine("Đã tạo Role Admin mặc định.");
        }

        // 2. Tạo tài khoản admin nếu chưa có, hoặc cập nhật password nếu đã có
        var adminUser = context.Users.FirstOrDefault(u => u.Username == "admin");
        if (adminUser == null)
        {
            adminUser = new WEBBANDIENTHOAI.Models.User
            {
                Username = "admin",
                FullName = "Quản Trị Viên",
                Email = "admin@foxmobile.vn",
                PasswordHash = WEBBANDIENTHOAI.Services.PasswordHasher.Hash("123456"),
                RoleId = adminRole.RoleId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            context.Users.Add(adminUser);
            context.SaveChanges();
            Console.WriteLine("Đã tạo User Admin mặc định (admin / 123456).");
        }
        else
        {
            // Reset bắt buộc để user chắc chắn có thể đăng nhập bằng 123456 thay vì mật khẩu cũ
            adminUser.PasswordHash = WEBBANDIENTHOAI.Services.PasswordHasher.Hash("123456");
            context.SaveChanges();
            Console.WriteLine("Đã reset mật khẩu Admin có sẵn về 123456.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Lỗi khi kiểm tra/khởi tạo Admin: {ex.Message}");
    }
}

app.Run();
using FluentAssertions.Common;
using Microsoft.EntityFrameworkCore;
using WEBBANDIENTHOAI.Data;
using WEBBANDIENTHOAI.Repositories;
using WEBBANDIENTHOAI.Repository;
using WEBBANDIENTHOAI.Services;

var builder = WebApplication.CreateBuilder(args);

// ========================
// 1) Load connection string
// ========================
var conn = builder.Configuration.GetConnectionString("DefaultConnection")
           ?? "Data Source=DELL\\SQLEXPRESS02;Initial Catalog=web_dien_tu;Integrated Security=True;TrustServerCertificate=True";

// ========================
// 2) Add MVC services
// ========================
builder.Services.AddControllersWithViews();

// ========================
// 3) Add DbContext
// ========================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(conn));

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
// Đăng ký services
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IOTPService, OTPService>();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IMailService, MailService>();

// ===============================================
// 5) Cấu hình quan trọng cho Chatbot và Session
// ===============================================

// 5a) Đăng ký HttpClient để gọi API Gemini
builder.Services.AddHttpClient();

// 5b) Thêm Session (Đã có trong code của bạn, giữ nguyên cấu hình này)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(4);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ========================
// Build app
// ========================
var app = builder.Build();

// ========================
// 6) Middleware pipeline
// ========================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();

// Cần phải có UseSession TRƯỚC UseAuthorization
app.UseSession();
app.UseAuthorization();

// ========================
// 7) Default Route
// ========================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
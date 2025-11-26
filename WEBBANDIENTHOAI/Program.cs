using FluentAssertions.Common;
using Microsoft.EntityFrameworkCore;
using WEBBANDIENTHOAI.Data;
using WEBBANDIENTHOAI.Repositories;
using WEBBANDIENTHOAI.Repository;
using WEBBANDIENTHOAI.Services; // Thêm dòng này

var builder = WebApplication.CreateBuilder(args);

// ========================
// 1) Load connection string
// ========================
var conn = builder.Configuration.GetConnectionString("DefaultConnection")
           ?? "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=web_dien_tu;Integrated Security=True;Trust Server Certificate=True";

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
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});



// ========================
// 5) Enable Session
// ========================
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

app.UseSession();       // must be before UseAuthorization
app.UseAuthorization();

// ========================
// 7) Default Route
// ========================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

// ========================
// Run
// ========================
app.Run();
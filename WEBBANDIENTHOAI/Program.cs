using System;
using Microsoft.EntityFrameworkCore;
using WEBBANDIENTHOAI.Models;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// DbContext (giữ như bạn đã cấu hình)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Middleware pipeline (CHÚ Ý THỨ TỰ)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Serve static files (css/js/images) from wwwroot
// -> Không gọi UseDefaultFiles() ở đây
app.UseStaticFiles();

app.UseRouting();

// session must be between routing and endpoints
app.UseSession();

app.UseAuthorization();

// Default MVC route -> truy cập "/" sẽ tới HomeController.Index
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");



app.Run();

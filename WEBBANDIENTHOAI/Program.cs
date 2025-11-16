using Microsoft.EntityFrameworkCore;
using WEBBANDIENTHOAI.Data;

var builder = WebApplication.CreateBuilder(args);

// ========================
// 1) Load connection string
// ========================
var conn = builder.Configuration.GetConnectionString("DefaultConnection")
           ?? "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=PhoneShopFull;Integrated Security=True;TrustServerCertificate=True";

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
// 4) Enable Session
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
// 5) Middleware pipeline
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
// 6) Default Route
// ========================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

// ========================
// Run
// ========================
app.Run();

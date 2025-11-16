using Microsoft.EntityFrameworkCore;
using WebBanDienThoai.Models;
using WEBBANDIENTHOAI.Models;

namespace WEBBANDIENTHOAI.Data
{
    /// <summary>
    /// AppDbContext - ánh xạ các bảng trong database.
    /// Ghi chú: các class model (Product, Inventory, ExportReceipt, ImportReceipt, ...) phải có trong namespace WEBBANDIENTHOAI.Models.
    /// Nếu thiếu model nào, bạn tạo model tương ứng theo schema SQL trước khi migrate / run.
    /// </summary>
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Core entities (Users/Customers/Roles)
        public DbSet<User> Users { get; set; }                 // (Users: Người dùng/nhân viên)
        public DbSet<Role> Roles { get; set; }                 // (Roles: Quyền)
        public DbSet<Customer> Customers { get; set; }         // (Customers: Khách hàng)

        // Catalog
        public DbSet<Category> Categories { get; set; }        // (Categories: Danh mục)
        public DbSet<ProductStatus> ProductStatuses { get; set; } // (ProductStatuses: Trạng thái sản phẩm)
        public DbSet<Product> Products { get; set; }           // (Products: Sản phẩm)
        public DbSet<ProductImage> ProductImages { get; set; } // (ProductImages: Ảnh sản phẩm)

        // Inventory / Warehouse
        public DbSet<Inventory> Inventory { get; set; }        // (Inventory: Tồn kho) -- đặt tên Inventory để khớp controller cũ
        public DbSet<Inventory> Inventories { get; set; }      // (Inventories: cùng entity - optional, tiện cho plural)
        public DbSet<InventoryHistory> InventoryHistory { get; set; } // (InventoryHistory: Lịch sử tồn kho)

        // Import / Export receipts
        public DbSet<ImportReceipt> ImportReceipts { get; set; }         // (ImportReceipts: Phiếu nhập)
        public DbSet<ImportReceiptDetail> ImportReceiptDetails { get; set; } // (chi tiết nhập)
        public DbSet<ExportReceipt> ExportReceipts { get; set; }         // (ExportReceipts: Phiếu xuất)
        public DbSet<ExportReceiptDetail> ExportReceiptDetails { get; set; } // (chi tiết xuất)

        // Orders / Cart
        public DbSet<Order> Orders { get; set; }               // (Orders: Đơn hàng)
        public DbSet<OrderDetail> OrderDetails { get; set; }   // (OrderDetails: Chi tiết đơn)
        public DbSet<Cart> Carts { get; set; }                 // (Carts: Giỏ hàng)
        public DbSet<CartDetail> CartDetails { get; set; }     // (CartDetails)

        // Device configurations
        public DbSet<LaptopConfiguration> LaptopConfigurations { get; set; }
        public DbSet<PhoneConfiguration> PhoneConfigurations { get; set; }

        // Audit / Logs
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Map entity -> table names (theo script SQL bạn dùng)
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Role>().ToTable("Roles");
            modelBuilder.Entity<Customer>().ToTable("Customers");

            modelBuilder.Entity<Category>().ToTable("Categories");
            modelBuilder.Entity<ProductStatus>().ToTable("ProductStatuses");
            modelBuilder.Entity<Product>().ToTable("Products");
            modelBuilder.Entity<ProductImage>().ToTable("ProductImages");

            modelBuilder.Entity<Inventory>().ToTable("Inventory");
            modelBuilder.Entity<InventoryHistory>().ToTable("InventoryHistory");

            modelBuilder.Entity<ImportReceipt>().ToTable("ImportReceipts");
            modelBuilder.Entity<ImportReceiptDetail>().ToTable("ImportReceiptDetails");
            modelBuilder.Entity<ExportReceipt>().ToTable("ExportReceipts");
            modelBuilder.Entity<ExportReceiptDetail>().ToTable("ExportReceiptDetails");

            modelBuilder.Entity<Order>().ToTable("Orders");
            modelBuilder.Entity<OrderDetail>().ToTable("OrderDetails");

            modelBuilder.Entity<Cart>().ToTable("Carts");
            modelBuilder.Entity<CartDetail>().ToTable("CartDetails");

            modelBuilder.Entity<LaptopConfiguration>().ToTable("LaptopConfigurations");
            modelBuilder.Entity<PhoneConfiguration>().ToTable("PhoneConfigurations");

            modelBuilder.Entity<AuditLog>().ToTable("AuditLogs");

            // Indexes / constraints you may want (ví dụ)
            modelBuilder.Entity<Product>().HasIndex(p => p.SKU).IsUnique(false);
            modelBuilder.Entity<Inventory>().HasIndex(i => i.StockCode);
            modelBuilder.Entity<Order>().HasIndex(o => o.OrderDate);
        }
    }
}

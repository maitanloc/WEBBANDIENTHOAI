using Microsoft.EntityFrameworkCore;
using WebBanDienThoai.Models;
using WEBBANDIENTHOAI.Models;

namespace WEBBANDIENTHOAI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Core entities (Users/Customers/Roles)
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Customer> Customers { get; set; }

        // Catalog
        public DbSet<Category> Categories { get; set; }
        public DbSet<ProductStatus> ProductStatuses { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }

        // Inventory / Warehouse
        public DbSet<Inventory> Inventory { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<InventoryHistory> InventoryHistory { get; set; }

        // Import / Export receipts
        public DbSet<ImportReceipt> ImportReceipts { get; set; }
        public DbSet<ImportReceiptDetail> ImportReceiptDetails { get; set; }
        public DbSet<ExportReceipt> ExportReceipts { get; set; }
        public DbSet<ExportReceiptDetail> ExportReceiptDetails { get; set; }

        // Orders / Cart
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<OrderStatus> OrderStatuses { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartDetail> CartDetails { get; set; }

        // Device configurations
        public DbSet<LaptopConfiguration> LaptopConfigurations { get; set; }
        public DbSet<PhoneConfiguration> PhoneConfigurations { get; set; }

        // Audit / Logs
        public DbSet<AuditLog> AuditLogs { get; set; }

        // 🔥 THÊM CÁC MODELS MỚI CHO FORGOT PASSWORD
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
        public DbSet<OTPCode> OTPCodes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Map entity -> table names
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

            // 🔥 THÊM CẤU HÌNH CHO CÁC TABLE MỚI
            modelBuilder.Entity<PasswordResetToken>().ToTable("PasswordResetTokens");
            modelBuilder.Entity<OTPCode>().ToTable("OTPCodes");

            // 🔥 CẤU HÌNH QUAN HỆ Product ↔ ProductImage
            modelBuilder.Entity<Product>()
                .HasMany(p => p.Images)
                .WithOne(pi => pi.Product)
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Quan hệ optional cho PrimaryImage
            modelBuilder.Entity<Product>()
                .HasOne(p => p.PrimaryImage)
                .WithOne()
                .HasForeignKey<Product>(p => p.ImageId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            // 🔥 CẤU HÌNH CHO PASSWORD RESET TOKENS
            modelBuilder.Entity<PasswordResetToken>(entity =>
            {
                entity.HasKey(prt => prt.TokenId);
                entity.Property(prt => prt.Token).IsRequired().HasMaxLength(100);
                entity.Property(prt => prt.Email).IsRequired().HasMaxLength(150);
                entity.Property(prt => prt.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.HasIndex(prt => prt.Token).IsUnique();
                entity.HasIndex(prt => prt.Email);
            });

            // 🔥 CẤU HÌNH CHO OTP CODES
            modelBuilder.Entity<OTPCode>(entity =>
            {
                entity.HasKey(otp => otp.OTPId);
                entity.Property(otp => otp.Code).IsRequired().HasMaxLength(6);
                entity.Property(otp => otp.Email).IsRequired().HasMaxLength(150);
                entity.Property(otp => otp.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.HasIndex(otp => new { otp.Email, otp.Code });
                entity.HasIndex(otp => otp.CreatedAt);
            });

            // Indexes / constraints
            modelBuilder.Entity<Product>().HasIndex(p => p.SKU).IsUnique(false);
            modelBuilder.Entity<Inventory>().HasIndex(i => i.StockCode);
            modelBuilder.Entity<Order>().HasIndex(o => o.OrderDate);

            // 🔥 THÊM INDEX CHO CUSTOMER EMAIL (quan trọng cho forgot password)
            modelBuilder.Entity<Customer>().HasIndex(c => c.Email).IsUnique();
        }
    }
}
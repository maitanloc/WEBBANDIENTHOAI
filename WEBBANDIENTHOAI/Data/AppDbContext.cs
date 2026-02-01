using Microsoft.EntityFrameworkCore;
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
            modelBuilder.Entity<OrderStatus>().ToTable("OrderStatuses"); // Thêm dòng này nếu chưa có

            modelBuilder.Entity<Cart>().ToTable("Carts");
            modelBuilder.Entity<CartDetail>().ToTable("CartDetails");

            modelBuilder.Entity<LaptopConfiguration>().ToTable("LaptopConfigurations");
            modelBuilder.Entity<PhoneConfiguration>().ToTable("PhoneConfigurations");

            modelBuilder.Entity<AuditLog>().ToTable("AuditLogs");

            // CẤU HÌNH CHO CÁC TABLE MỚI
            modelBuilder.Entity<PasswordResetToken>().ToTable("PasswordResetTokens");
            modelBuilder.Entity<OTPCode>().ToTable("OTPCodes");

            // CẤU HÌNH QUAN HỆ Product ↔ ProductImage
            modelBuilder.Entity<Product>()
                .HasMany(p => p.Images)
                .WithOne(pi => pi.Product)
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.ClientCascade); // Đổi từ Cascade để tránh lỗi multiple cascade paths

            // Quan hệ optional cho PrimaryImage
            modelBuilder.Entity<Product>()
                .HasOne(p => p.PrimaryImage)
                .WithOne()
                .HasForeignKey<Product>(p => p.ImageId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            // CẤU HÌNH CHO PASSWORD RESET TOKENS
            modelBuilder.Entity<PasswordResetToken>(entity =>
            {
                entity.HasKey(prt => prt.TokenId);
                entity.Property(prt => prt.Token).IsRequired().HasMaxLength(100);
                entity.Property(prt => prt.Email).IsRequired().HasMaxLength(150);
                entity.Property(prt => prt.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.HasIndex(prt => prt.Token).IsUnique();
                entity.HasIndex(prt => prt.Email);
            });

            // CẤU HÌNH CHO OTP CODES
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
            modelBuilder.Entity<Customer>().HasIndex(c => c.Email).IsUnique();

            // =============== DATA SEEDING ===============
            // Gieo dữ liệu cho Bảng Role
            modelBuilder.Entity<Role>().HasData(
                new Role { RoleId = 1, RoleName = "Admin", Description = "Quản trị viên cấp cao nhất" },
                new Role { RoleId = 2, RoleName = "Staff", Description = "Nhân viên quản lý" },
                new Role { RoleId = 3, RoleName = "Customer", Description = "Khách hàng" }
            );

            // Gieo dữ liệu cho Bảng OrderStatus
            modelBuilder.Entity<OrderStatus>().HasData(
                new OrderStatus { StatusId = 1, StatusName = "Pending", Description = "Đơn hàng đang chờ xử lý" },
                new OrderStatus { StatusId = 2, StatusName = "Processing", Description = "Đơn hàng đang được chuẩn bị" },
                new OrderStatus { StatusId = 3, StatusName = "Shipped", Description = "Đơn hàng đã được giao cho đơn vị vận chuyển" },
                new OrderStatus { StatusId = 4, StatusName = "Delivered", Description = "Đơn hàng đã giao thành công" },
                new OrderStatus { StatusId = 5, StatusName = "Cancelled", Description = "Đơn hàng đã bị hủy" }
            );

            // Gieo dữ liệu cho Bảng ProductStatus
            modelBuilder.Entity<ProductStatus>().HasData(
                new ProductStatus { StatusId = 1, StatusName = "Available", Description = "Sản phẩm có sẵn, đang kinh doanh" },
                new ProductStatus { StatusId = 2, StatusName = "Out of Stock", Description = "Sản phẩm đã hết hàng" }
            );

            // Gieo dữ liệu cho Bảng Category
                        modelBuilder.Entity<Category>().HasData(
                            new Category { CategoryId = 1, CategoryName = "Smartphones", Description = "Các loại điện thoại thông minh", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                            new Category { CategoryId = 2, CategoryName = "Laptops", Description = "Các loại máy tính xách tay", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
                        );
            
                        // Gieo dữ liệu cho User Admin
                        modelBuilder.Entity<User>().HasData(
                            new User
                            {
                                UserId = 1,
                                Username = "admin",
                                // Password "admin123" -> hashed with SHA256
                                PasswordHash = new byte[] { 147, 186, 166, 224, 128, 110, 18, 16, 209, 138, 14, 3, 140, 101, 193, 223, 134, 242, 179, 21, 25, 20, 208, 182, 112, 183, 17, 199, 15, 222, 119, 93 },
                                FullName = "Administrator",
                                Email = "admin@example.com",
                                RoleId = 1, // Foreign key for Admin role
                                IsActive = true,
                                                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                                                }
                                            );
                                
                                            // Hash the password "customer123"
                                            byte[] customerPasswordHash;
                                            using (var sha256 = System.Security.Cryptography.SHA256.Create())
                                            {
                                                customerPasswordHash = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes("customer123"));
                                            }
                                
                                            // Gieo dữ liệu cho User (Customer)
                                            modelBuilder.Entity<User>().HasData(
                                                new User
                                                {
                                                    UserId = 2,
                                                    Username = "customer",
                                                    PasswordHash = customerPasswordHash,
                                                    FullName = "Nguyễn Văn A",
                                                    Email = "customer@example.com",
                                                    RoleId = 3, // Customer role
                                                    IsActive = true,
                                                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                                                }
                                            );
                                
                                            // Gieo dữ liệu cho Customer
                                            modelBuilder.Entity<Customer>().HasData(
                                                new Customer
                                                {
                                                    CustomerId = 1,
                                                    FullName = "Nguyễn Văn A",
                                                    Email = "customer@example.com",
                                                    PasswordHash = customerPasswordHash,
                                                    Phone = "0987654321",
                                                    CitizenID = "0123456789",
                                                    Address = "123 Đường ABC, Quận 1, TP. HCM",
                                                    IsActive = true,
                                                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                                                }
                                            );
                                
                                            // Gieo dữ liệu cho ProductImages
                                            modelBuilder.Entity<ProductImage>().HasData(
                                                // Placeholder images for iPhone 15 Pro
                                                new ProductImage { ImageId = 1, ProductId = 1, ImagePath = new byte[0], IsPrimary = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                                                new ProductImage { ImageId = 2, ProductId = 1, ImagePath = new byte[0], IsPrimary = false, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                                                // Placeholder images for MacBook Pro 14
                                                new ProductImage { ImageId = 3, ProductId = 2, ImagePath = new byte[0], IsPrimary = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
                                            );
                                
                                                        // Gieo dữ liệu cho Products
                                                        modelBuilder.Entity<Product>().HasData(
                                                            new Product
                                                            {
                                                                ProductId = 1,
                                                                CategoryId = 1,
                                                                SKU = "IP15P256",
                                                                Name = "iPhone 15 Pro 256GB",
                                                                Brand = "Apple",
                                                                Price = 28990000m,
                                                                OldPrice = 30990000m,
                                                                StockCode = "SC-IP15P256",
                                                                Color = "Titan tự nhiên",
                                                                ShortDescription = "Chip A17 Pro, Màn hình Super Retina XDR, Camera Pro 48MP.",
                                                                StatusId = 1,
                                                                ImageId = null, // Đặt là null để phá vỡ tham chiếu vòng tròn
                                                                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                                                            },
                                                            new Product
                                                            {
                                                                ProductId = 2,
                                                                CategoryId = 2,
                                                                SKU = "MBP14M3",
                                                                Name = "MacBook Pro 14 inch M3",
                                                                Brand = "Apple",
                                                                Price = 49990000m,
                                                                OldPrice = 52990000m,
                                                                StockCode = "SC-MBP14M3",
                                                                Color = "Space Gray",
                                                                ShortDescription = "Chip M3 Pro, 18GB RAM, 512GB SSD, Màn hình Liquid Retina XDR.",
                                                                StatusId = 1,
                                                                ImageId = null, // Đặt là null để phá vỡ tham chiếu vòng tròn
                                                                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                                                            }
                                                        );                                
                                                        // Gieo dữ liệu cho PhoneConfiguration
                                                        modelBuilder.Entity<PhoneConfiguration>().HasData(
                                                            new PhoneConfiguration
                                                            {
                                                                ConfigurationId = 1,
                                                                ProductId = 1,
                                                                CPU = "Apple A17 Pro",
                                                                RAM = "8 GB",
                                                                InternalStorage = "256 GB",
                                                                Screen = "6.1-inch Super Retina XDR",
                                                                OperatingSystem = "iOS 17",
                                                                Battery = "Li-Ion, sạc nhanh",
                                                                Camera = "Chính 48 MP & Phụ 12 MP, 12 MP",
                                                                Color = "Titan tự nhiên",
                                                                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                                                            }
                                                        );
                                            
                                                        // Gieo dữ liệu cho LaptopConfiguration
                                                        modelBuilder.Entity<LaptopConfiguration>().HasData(
                                                            new LaptopConfiguration
                                                            {
                                                                ConfigurationId = 1,
                                                                ProductId = 2,
                                                                CPU = "Apple M3 Pro 11-core",
                                                                RAM = "18 GB",
                                                                Storage = "512 GB SSD",
                                                                GraphicsCard = "14-core GPU",
                                                                ScreenSize = "14.2 inch",
                                                                ScreenTechnology = "Liquid Retina XDR display",
                                                                OperatingSystem = "macOS Sonoma",
                                                                Color = "Space Gray",
                                                                Weight = "1.55 kg",
                                                                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                                                            }
                                                        );                                
                                            // Gieo dữ liệu cho Inventory
                                            modelBuilder.Entity<Inventory>().HasData(
                                                new Inventory
                                                {
                                                    InventoryId = 1,
                                                    ProductId = 1,
                                                    StockCode = "SC-IP15P256",
                                                    CurrentQuantity = 50,
                                                    MinimumQuantity = 10,
                                                    Location = "Kho A1",
                                                    LastUpdated = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                                                },
                                                new Inventory
                                                {
                                                    InventoryId = 2,
                                                    ProductId = 2,
                                                    StockCode = "SC-MBP14M3",
                                                    CurrentQuantity = 30,
                                                    MinimumQuantity = 5,
                                                    Location = "Kho B2",
                                                    LastUpdated = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                                                }
                                            );
                                        }
                                    }
                                }
                                
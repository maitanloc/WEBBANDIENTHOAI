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

        // Product Options (tùy chọn cấu hình sản phẩm)
        public DbSet<ProductOption> ProductOptions { get; set; }

        // Cross-sell rules (bán chéo)
        public DbSet<CrossSellRule> CrossSellRules { get; set; }

        // Audit / Logs
        public DbSet<AuditLog> AuditLogs { get; set; }

        // Forgot Password
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
        public DbSet<OTPCode> OTPCodes { get; set; }

        // ===== PROMOTIONS & LOYALTY =====
        public DbSet<CustomerTier> CustomerTiers { get; set; }
        public DbSet<Voucher> Vouchers { get; set; }
        public DbSet<UserVoucher> UserVouchers { get; set; }
        public DbSet<UserPointHistory> UserPointHistories { get; set; }
        public DbSet<SpinHistory> SpinHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Map entity -> table names
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Role>().ToTable("Roles");
            modelBuilder.Entity<Customer>().ToTable("Customers");

            modelBuilder.Entity<Category>().ToTable("Categories");
            modelBuilder.Entity<ProductStatus>().ToTable("ProductStatuses");
            modelBuilder.Entity<Product>().ToTable("Products", tb => tb.HasTrigger("ProductTrigger"));
            modelBuilder.Entity<ProductImage>().ToTable("ProductImages", tb => tb.HasTrigger("ProductImageTrigger"));

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

            modelBuilder.Entity<LaptopConfiguration>().ToTable("LaptopConfigurations", tb => tb.HasTrigger("LaptopConfigTrigger"));
            modelBuilder.Entity<PhoneConfiguration>().ToTable("PhoneConfigurations", tb => tb.HasTrigger("PhoneConfigTrigger"));

            modelBuilder.Entity<AuditLog>().ToTable("AuditLogs");

            modelBuilder.Entity<PasswordResetToken>().ToTable("PasswordResetTokens");
            modelBuilder.Entity<OTPCode>().ToTable("OTPCodes");

            modelBuilder.Entity<ProductOption>().ToTable("ProductOptions");
            modelBuilder.Entity<CrossSellRule>().ToTable("CrossSellRules");

            // Promotion & Loyalty tables
            modelBuilder.Entity<CustomerTier>().ToTable("CustomerTiers");
            modelBuilder.Entity<Voucher>().ToTable("Vouchers");
            modelBuilder.Entity<UserVoucher>().ToTable("UserVouchers");
            modelBuilder.Entity<UserPointHistory>().ToTable("UserPointHistories");
            modelBuilder.Entity<SpinHistory>().ToTable("SpinHistories");

            // CẤU HÌNH QUAN HỆ Product ↔ Category
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // CẤU HÌNH QUAN HỆ Product ↔ ProductStatus
            modelBuilder.Entity<Product>()
                .HasOne(p => p.ProductStatus)
                .WithMany(ps => ps.Products)
                .HasForeignKey(p => p.StatusId)
                .OnDelete(DeleteBehavior.Restrict);

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

            // CẤU HÌNH QUAN HỆ LaptopConfiguration ↔ Product
            modelBuilder.Entity<LaptopConfiguration>()
                .HasOne(lc => lc.Product)
                .WithOne(p => p.LaptopConfiguration)
                .HasForeignKey<LaptopConfiguration>(lc => lc.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // CẤU HÌNH QUAN HỆ PhoneConfiguration ↔ Product
            modelBuilder.Entity<PhoneConfiguration>()
                .HasOne(pc => pc.Product)
                .WithOne(p => p.PhoneConfiguration)
                .HasForeignKey<PhoneConfiguration>(pc => pc.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // CẤU HÌNH QUAN HỆ ProductOption ↔ Product (1-N)
            modelBuilder.Entity<ProductOption>()
                .HasOne(po => po.Product)
                .WithMany(p => p.ProductOptions)
                .HasForeignKey(po => po.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // CẤU HÌNH QUAN HỆ CrossSellRule ↔ Product (2 FK, no cascade)
            modelBuilder.Entity<CrossSellRule>()
                .HasOne(r => r.TriggerProduct)
                .WithMany(p => p.CrossSellRulesTrigger)
                .HasForeignKey(r => r.TriggerProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CrossSellRule>()
                .HasOne(r => r.SuggestedProduct)
                .WithMany()
                .HasForeignKey(r => r.SuggestedProductId)
                .OnDelete(DeleteBehavior.Restrict);

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

            // ===== PROMOTION & LOYALTY RELATIONSHIPS =====
            // CustomerTier → Customer (1-N)
            modelBuilder.Entity<Customer>()
                .HasOne(c => c.Tier)
                .WithMany(t => t.Customers)
                .HasForeignKey(c => c.TierId)
                .OnDelete(DeleteBehavior.Restrict);

            // Customer → UserVoucher (1-N)
            modelBuilder.Entity<UserVoucher>()
                .HasOne(uv => uv.Customer)
                .WithMany(c => c.UserVouchers)
                .HasForeignKey(uv => uv.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Voucher → UserVoucher (1-N)
            modelBuilder.Entity<UserVoucher>()
                .HasOne(uv => uv.Voucher)
                .WithMany(v => v.UserVouchers)
                .HasForeignKey(uv => uv.VoucherId)
                .OnDelete(DeleteBehavior.Restrict);

            // Order → UserVoucher (1-N, optional)
            modelBuilder.Entity<UserVoucher>()
                .HasOne(uv => uv.Order)
                .WithMany(o => o.UserVouchers)
                .HasForeignKey(uv => uv.OrderId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction);

            // Order → Voucher (N-1, optional)
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Voucher)
                .WithMany(v => v.Orders)
                .HasForeignKey(o => o.VoucherId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            // Customer → UserPointHistory (1-N)
            modelBuilder.Entity<UserPointHistory>()
                .HasOne(ph => ph.Customer)
                .WithMany(c => c.PointHistories)
                .HasForeignKey(ph => ph.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Order → UserPointHistory (1-N, optional)
            modelBuilder.Entity<UserPointHistory>()
                .HasOne(ph => ph.Order)
                .WithMany(o => o.PointHistories)
                .HasForeignKey(ph => ph.OrderId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction);

            // Unique constraint: Voucher.Code
            modelBuilder.Entity<Voucher>()
                .HasIndex(v => v.Code)
                .IsUnique();

            // SpinHistory → Customer (N-1)
            modelBuilder.Entity<SpinHistory>()
                .HasOne(s => s.Customer)
                .WithMany()
                .HasForeignKey(s => s.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            // SpinHistory → Voucher (N-1, optional)
            modelBuilder.Entity<SpinHistory>()
                .HasOne(s => s.Voucher)
                .WithMany()
                .HasForeignKey(s => s.VoucherId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            // Index: tìm lịch sử quay theo customer và ngày
            modelBuilder.Entity<SpinHistory>()
                .HasIndex(s => new { s.CustomerId, s.SpinAt });

            // Unique: 1 customer chỉ có 1 record chưa dùng của 1 voucher
            modelBuilder.Entity<UserVoucher>()
                .HasIndex(uv => new { uv.CustomerId, uv.VoucherId, uv.IsUsed })
                .HasFilter("[IsUsed] = 0");

            // =============== DATA SEEDING ===============
            // Gieo dữ liệu cho Bảng Role
            modelBuilder.Entity<Role>().HasData(
                new Role { RoleId = 1, RoleName = "Admin", Description = "Quản trị viên cấp cao nhất" },
                new Role { RoleId = 2, RoleName = "Staff", Description = "Nhân viên quản lý" },
                new Role { RoleId = 3, RoleName = "Customer", Description = "Khách hàng" }
            );

            // ===== SEED: CustomerTier =====
            modelBuilder.Entity<CustomerTier>().HasData(
                new CustomerTier { TierId = 1, TierName = "Member",   MinSpending = 0m,          BonusMultiplier = 1.0m, Description = "Thành viên thường – nhận 1 điểm / 10.000đ" },
                new CustomerTier { TierId = 2, TierName = "Silver",   MinSpending = 10_000_000m, BonusMultiplier = 1.2m, Description = "Thành viên Bạc – Chi tiêu > 10Tr, hệ số điểm x1.2" },
                new CustomerTier { TierId = 3, TierName = "Gold",     MinSpending = 50_000_000m, BonusMultiplier = 1.5m, Description = "Thành viên Vàng – Chi tiêu > 50Tr, hệ số điểm x1.5" },
                new CustomerTier { TierId = 4, TierName = "Diamond",  MinSpending = 100_000_000m, BonusMultiplier = 2.0m, Description = "Thành viên Kim Cương – Chi tiêu > 100Tr, hệ số điểm x2.0" }
            );

            // ===== SEED: OrderStatus (thêm Return/Refund) =====
            modelBuilder.Entity<OrderStatus>().HasData(
                new OrderStatus { StatusId = 1, StatusName = "Pending",    Description = "Đơn hàng đang chờ xử lý" },
                new OrderStatus { StatusId = 2, StatusName = "Processing", Description = "Đơn hàng đang được chuẩn bị" },
                new OrderStatus { StatusId = 3, StatusName = "Shipped",    Description = "Đơn hàng đã được giao cho đơn vị vận chuyển" },
                new OrderStatus { StatusId = 4, StatusName = "Delivered",  Description = "Đơn hàng đã giao thành công" },
                new OrderStatus { StatusId = 5, StatusName = "Cancelled",  Description = "Đơn hàng đã bị hủy" },
                new OrderStatus { StatusId = 6, StatusName = "Returned",   Description = "Đơn hàng đã được hoàn trả / hoàn tiền" }
            );

            // ===== SEED: Voucher (5 loại) =====
            modelBuilder.Entity<Voucher>().HasData(
                // 1. Voucher chào mừng thành viên mới - Fixed 50k
                new Voucher
                {
                    VoucherId    = 1,
                    Code         = "WELCOME50",
                    Description  = "Voucher chào mừng thành viên mới - giảm 50.000đ",
                    DiscountType = DiscountType.Fixed,
                    Value        = 50000m,
                    MinOrderValue = 500000m,
                    StartDate    = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    EndDate      = new DateTime(2026, 12, 31, 23, 59, 59, DateTimeKind.Utc),
                    Quantity     = 0, // Không giới hạn
                    UsedCount    = 0,
                    IsActive     = true,
                    CreatedAt    = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                // 2. Voucher giảm 10% (tối đa 100k)
                new Voucher
                {
                    VoucherId      = 2,
                    Code           = "SALE10PCT",
                    Description    = "Giảm 10% tối đa 100.000đ cho đơn từ 1.000.000đ",
                    DiscountType   = DiscountType.Percent,
                    Value          = 10m,
                    MaxDiscountAmount = 100000m,
                    MinOrderValue  = 1000000m,
                    StartDate      = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    EndDate        = new DateTime(2026, 12, 31, 23, 59, 59, DateTimeKind.Utc),
                    Quantity       = 500,
                    UsedCount      = 0,
                    IsActive       = true,
                    CreatedAt      = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                // 3. Voucher Silver – giảm 100k
                new Voucher
                {
                    VoucherId    = 3,
                    Code         = "SILVER100",
                    Description  = "Ưu đãi hạng Bạc - giảm 100.000đ cho đơn từ 2.000.000đ",
                    DiscountType = DiscountType.Fixed,
                    Value        = 100000m,
                    MinOrderValue = 2000000m,
                    StartDate    = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    EndDate      = new DateTime(2026, 12, 31, 23, 59, 59, DateTimeKind.Utc),
                    Quantity     = 200,
                    UsedCount    = 0,
                    IsActive     = true,
                    CreatedAt    = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                // 4. Voucher Gold – giảm 15% (tối đa 300k)
                new Voucher
                {
                    VoucherId      = 4,
                    Code           = "GOLD15PCT",
                    Description    = "Ưu đãi hạng Vàng - giảm 15% tối đa 300.000đ",
                    DiscountType   = DiscountType.Percent,
                    Value          = 15m,
                    MaxDiscountAmount = 300000m,
                    MinOrderValue  = 5000000m,
                    StartDate      = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    EndDate        = new DateTime(2026, 12, 31, 23, 59, 59, DateTimeKind.Utc),
                    Quantity       = 100,
                    UsedCount      = 0,
                    IsActive       = true,
                    CreatedAt      = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                // 5. Voucher Diamond VIP – giảm 20% (tối đa 1 triệu)
                new Voucher
                {
                    VoucherId      = 5,
                    Code           = "DIAMOND20",
                    Description    = "Ưu đãi hạng Kim Cương - giảm 20% tối đa 1.000.000đ",
                    DiscountType   = DiscountType.Percent,
                    Value          = 20m,
                    MaxDiscountAmount = 1000000m,
                    MinOrderValue  = 10000000m,
                    StartDate      = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    EndDate        = new DateTime(2026, 12, 31, 23, 59, 59, DateTimeKind.Utc),
                    Quantity       = 50,
                    UsedCount      = 0,
                    IsActive       = true,
                    CreatedAt      = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            // ===== SEED: Customer mẫu (cần TierId) - cập nhật Customer seeded ID=1 có TierId=1 =====
            // (Customer seeding phía dưới sẽ tự điền TierId = 1 mặc định theo default value)

            // ===== SEED: UserVoucher (5 record – gán voucher cho customer mẫu ID=1) =====
            modelBuilder.Entity<UserVoucher>().HasData(
                new UserVoucher { UserVoucherId = 1, CustomerId = 1, VoucherId = 1, AssignedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsUsed = false },
                new UserVoucher { UserVoucherId = 2, CustomerId = 1, VoucherId = 2, AssignedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsUsed = false },
                new UserVoucher { UserVoucherId = 3, CustomerId = 1, VoucherId = 3, AssignedAt = new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc), IsUsed = false },
                new UserVoucher { UserVoucherId = 4, CustomerId = 1, VoucherId = 4, AssignedAt = new DateTime(2024, 2, 15, 0, 0, 0, DateTimeKind.Utc), IsUsed = false },
                new UserVoucher { UserVoucherId = 5, CustomerId = 1, VoucherId = 5, AssignedAt = new DateTime(2024, 3, 1, 0, 0, 0, DateTimeKind.Utc), IsUsed = false }
            );

            // ===== SEED: UserPointHistory (6 record – lịch sử điểm giả lập cho customer ID=1) =====
            modelBuilder.Entity<UserPointHistory>().HasData(
                new UserPointHistory { HistoryId = 1, CustomerId = 1, OrderId = null, Points =  500, Reason = "Tích điểm đơn hàng #1001 - Giao thành công",  CreatedAt = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc) },
                new UserPointHistory { HistoryId = 2, CustomerId = 1, OrderId = null, Points =  300, Reason = "Tích điểm đơn hàng #1002 - Giao thành công",  CreatedAt = new DateTime(2024, 2,  5, 0, 0, 0, DateTimeKind.Utc) },
                new UserPointHistory { HistoryId = 3, CustomerId = 1, OrderId = null, Points = -200, Reason = "Dùng điểm thanh toán đơn hàng #1003",          CreatedAt = new DateTime(2024, 2, 20, 0, 0, 0, DateTimeKind.Utc) },
                new UserPointHistory { HistoryId = 4, CustomerId = 1, OrderId = null, Points =  150, Reason = "Tích điểm đơn hàng #1004 - Giao thành công",  CreatedAt = new DateTime(2024, 3,  1, 0, 0, 0, DateTimeKind.Utc) },
                new UserPointHistory { HistoryId = 5, CustomerId = 1, OrderId = null, Points =  100, Reason = "Thưởng sự kiện Vòng quay may mắn",            CreatedAt = new DateTime(2024, 3, 10, 0, 0, 0, DateTimeKind.Utc) },
                new UserPointHistory { HistoryId = 6, CustomerId = 1, OrderId = null, Points = -100, Reason = "Hoàn trả điểm đơn hàng #1005 - Hoàn hàng",   CreatedAt = new DateTime(2024, 3, 15, 0, 0, 0, DateTimeKind.Utc) }
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
                                                // iPhone 16 Pro Max
                                                new ProductImage { ImageId = 1, ProductId = 1, ImagePath = new byte[0], IsPrimary = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                                                new ProductImage { ImageId = 2, ProductId = 1, ImagePath = new byte[0], IsPrimary = false, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                                                // MacBook Pro 14 M4
                                                new ProductImage { ImageId = 3, ProductId = 2, ImagePath = new byte[0], IsPrimary = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                                                // Samsung S24 Ultra
                                                new ProductImage { ImageId = 4, ProductId = 3, ImagePath = new byte[0], IsPrimary = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                                                // Dell XPS 16
                                                new ProductImage { ImageId = 5, ProductId = 4, ImagePath = new byte[0], IsPrimary = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
                                            );
                                
                                                        // Gieo dữ liệu cho Products
                                                        modelBuilder.Entity<Product>().HasData(
                                                            new Product
                                                            {
                                                                ProductId = 1,
                                                                CategoryId = 1,
                                                                SKU = "IP16PM256",
                                                                Name = "iPhone 16 Pro Max 256GB",
                                                                Brand = "Apple",
                                                                Price = 34990000m,
                                                                OldPrice = 36990000m,
                                                                StockCode = "SC-IP16PM256",
                                                                Color = "Titan Sa Mạc",
                                                                ShortDescription = "Chip A18 Pro, Apple Intelligence, Màn hình 6.9 inch Super Retina XDR.",
                                                                StatusId = 1,
                                                                ImageId = null,
                                                                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                                                            },
                                                            new Product
                                                            {
                                                                ProductId = 2,
                                                                CategoryId = 2,
                                                                SKU = "MBP14M4",
                                                                Name = "MacBook Pro 14 inch M4",
                                                                Brand = "Apple",
                                                                Price = 42990000m,
                                                                OldPrice = 45990000m,
                                                                StockCode = "SC-MBP14M4",
                                                                Color = "Space Black",
                                                                ShortDescription = "Chip M4 tiên tiến, 16GB RAM, 512GB SSD, Màn hình Liquid Retina XDR.",
                                                                StatusId = 1,
                                                                ImageId = null,
                                                                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                                                            },
                                                            new Product
                                                            {
                                                                ProductId = 3,
                                                                CategoryId = 1,
                                                                SKU = "S24U256",
                                                                Name = "Samsung Galaxy S24 Ultra 256GB",
                                                                Brand = "Samsung",
                                                                Price = 33990000m,
                                                                OldPrice = 35990000m,
                                                                StockCode = "SC-S24U256",
                                                                Color = "Xám Titan",
                                                                ShortDescription = "Galaxy AI, Khung viền Titan, Camera 200MP zoom quang 5x.",
                                                                StatusId = 1,
                                                                ImageId = null,
                                                                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                                                            },
                                                            new Product
                                                            {
                                                                ProductId = 4,
                                                                CategoryId = 2,
                                                                SKU = "DXPS162026",
                                                                Name = "Dell XPS 16 (2026)",
                                                                Brand = "Dell",
                                                                Price = 65990000m,
                                                                OldPrice = 69990000m,
                                                                StockCode = "SC-DXPS16",
                                                                Color = "Platinum",
                                                                ShortDescription = "Intel Core Ultra 9, 32GB RAM, 1TB SSD, RTX 4070, Màn hình 4K+ OLED Touch.",
                                                                StatusId = 1,
                                                                ImageId = null,
                                                                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                                                            }
                                                        );                                
                                                        // Gieo dữ liệu cho PhoneConfiguration
                                                        modelBuilder.Entity<PhoneConfiguration>().HasData(
                                                            new PhoneConfiguration
                                                            {
                                                                ConfigurationId = 1,
                                                                ProductId = 1,
                                                                CPU = "Apple A18 Pro",
                                                                RAM = "8 GB",
                                                                InternalStorage = "256 GB",
                                                                Screen = "6.9-inch Super Retina XDR",
                                                                OperatingSystem = "iOS 18",
                                                                Battery = "Li-Ion, sạc nhanh 45W",
                                                                Camera = "Chính 48 MP & Phụ 48 MP, 12 MP",
                                                                Color = "Titan Sa Mạc",
                                                                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                                                            },
                                                            new PhoneConfiguration
                                                            {
                                                                ConfigurationId = 2,
                                                                ProductId = 3,
                                                                CPU = "Snapdragon 8 Gen 3 for Galaxy",
                                                                RAM = "12 GB",
                                                                InternalStorage = "256 GB",
                                                                Screen = "6.8-inch Dynamic AMOLED 2X",
                                                                OperatingSystem = "Android 14, One UI 6.1",
                                                                Battery = "5000 mAh, sạc nhanh 45W",
                                                                Camera = "200 MP & Phụ 50 MP, 12 MP, 10 MP",
                                                                Color = "Xám Titan",
                                                                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                                                            }
                                                        );
                                            
                                                        // Gieo dữ liệu cho LaptopConfiguration
                                                        modelBuilder.Entity<LaptopConfiguration>().HasData(
                                                            new LaptopConfiguration
                                                            {
                                                                ConfigurationId = 1,
                                                                ProductId = 2,
                                                                CPU = "Apple M4 10-core",
                                                                RAM = "16 GB",
                                                                Storage = "512 GB SSD",
                                                                GraphicsCard = "10-core GPU",
                                                                ScreenSize = "14.2 inch",
                                                                ScreenTechnology = "Liquid Retina XDR display",
                                                                OperatingSystem = "macOS Sequoia",
                                                                Color = "Space Black",
                                                                Weight = "1.55 kg",
                                                                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                                                            },
                                                            new LaptopConfiguration
                                                            {
                                                                ConfigurationId = 2,
                                                                ProductId = 4,
                                                                CPU = "Intel Core Ultra 9 185H",
                                                                RAM = "32 GB LPDDR5x",
                                                                Storage = "1 TB PCIe 4.0 NVMe",
                                                                GraphicsCard = "NVIDIA GeForce RTX 4070 8GB GDDR6",
                                                                ScreenSize = "16.3 inch",
                                                                ScreenTechnology = "OLED Touch 4K+ (3840x2400)",
                                                                OperatingSystem = "Windows 11 Pro",
                                                                Color = "Platinum",
                                                                Weight = "2.13 kg",
                                                                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                                                            }
                                                        );

                                            // Gieo dữ liệu cho ProductOption
                                            modelBuilder.Entity<ProductOption>().HasData(
                                                // iPhone 16 Pro Max - Màu sắc
                                                new ProductOption { ProductOptionId = 1, ProductId = 1, GroupName = "Màu sắc", OptionName = "Titan Sa Mạc", AdditionalPrice = 0m, DisplayOrder = 1, IsActive = true },
                                                new ProductOption { ProductOptionId = 2, ProductId = 1, GroupName = "Màu sắc", OptionName = "Titan Tự Nhiên", AdditionalPrice = 0m, DisplayOrder = 2, IsActive = true },
                                                new ProductOption { ProductOptionId = 3, ProductId = 1, GroupName = "Màu sắc", OptionName = "Titan Đen", AdditionalPrice = 0m, DisplayOrder = 3, IsActive = true },
                                                new ProductOption { ProductOptionId = 4, ProductId = 1, GroupName = "Màu sắc", OptionName = "Titan Trắng", AdditionalPrice = 0m, DisplayOrder = 4, IsActive = true },
                                                // iPhone 16 Pro Max - Bộ nhớ
                                                new ProductOption { ProductOptionId = 5, ProductId = 1, GroupName = "Bộ nhớ trong", OptionName = "256GB", AdditionalPrice = 0m, DisplayOrder = 1, IsActive = true },
                                                new ProductOption { ProductOptionId = 6, ProductId = 1, GroupName = "Bộ nhớ trong", OptionName = "512GB", AdditionalPrice = 5000000m, DisplayOrder = 2, IsActive = true },
                                                new ProductOption { ProductOptionId = 7, ProductId = 1, GroupName = "Bộ nhớ trong", OptionName = "1TB", AdditionalPrice = 11000000m, DisplayOrder = 3, IsActive = true },
                                                
                                                // MacBook Pro 14 M4
                                                new ProductOption { ProductOptionId = 8, ProductId = 2, GroupName = "RAM", OptionName = "16GB", AdditionalPrice = 0m, DisplayOrder = 1, IsActive = true },
                                                new ProductOption { ProductOptionId = 9, ProductId = 2, GroupName = "RAM", OptionName = "24GB", AdditionalPrice = 5000000m, DisplayOrder = 2, IsActive = true },
                                                new ProductOption { ProductOptionId = 10, ProductId = 2, GroupName = "SSD", OptionName = "512GB", AdditionalPrice = 0m, DisplayOrder = 1, IsActive = true },
                                                new ProductOption { ProductOptionId = 11, ProductId = 2, GroupName = "SSD", OptionName = "1TB", AdditionalPrice = 5000000m, DisplayOrder = 2, IsActive = true },

                                                // Samsung S24 Ultra
                                                new ProductOption { ProductOptionId = 16, ProductId = 3, GroupName = "Bộ nhớ trong", OptionName = "256GB", AdditionalPrice = 0m, DisplayOrder = 1, IsActive = true },
                                                new ProductOption { ProductOptionId = 17, ProductId = 3, GroupName = "Bộ nhớ trong", OptionName = "512GB", AdditionalPrice = 3500000m, DisplayOrder = 2, IsActive = true },

                                                // Dell XPS 16
                                                new ProductOption { ProductOptionId = 18, ProductId = 4, GroupName = "Màn hình", OptionName = "OLED Touch 4K+", AdditionalPrice = 0m, DisplayOrder = 1, IsActive = true },
                                                new ProductOption { ProductOptionId = 19, ProductId = 4, GroupName = "Màn hình", OptionName = "FHD+ Non-Touch", AdditionalPrice = -4000000m, DisplayOrder = 2, IsActive = true }
                                            );

                                            // Gieo dữ liệu cho CrossSellRule
                                            modelBuilder.Entity<CrossSellRule>().HasData(
                                                new CrossSellRule
                                                {
                                                    CrossSellRuleId = 1,
                                                    TriggerProductId = 1, // iPhone 16 Pro Max
                                                    SuggestedProductId = 2, // MBP 14 M4
                                                    DiscountedPrice = 40990000m,
                                                    DisplayMessage = "Hoàn thiện hệ sinh thái Apple 2026 của bạn!",
                                                    IsActive = true,
                                                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                                                },
                                                new CrossSellRule
                                                {
                                                    CrossSellRuleId = 2,
                                                    TriggerProductId = 3, // Samsung S24 Ultra
                                                    SuggestedProductId = 4, // Dell XPS 16
                                                    DiscountedPrice = 61990000m,
                                                    DisplayMessage = "Bộ đôi làm việc đa nhiệm siêu mạnh mẽ",
                                                    IsActive = true,
                                                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                                                }
                                            );

                                            // Gieo dữ liệu cho Inventory
                                            modelBuilder.Entity<Inventory>().HasData(
                                                new Inventory
                                                {
                                                    InventoryId = 1,
                                                    ProductId = 1,
                                                    StockCode = "SC-IP16PM256",
                                                    CurrentQuantity = 100,
                                                    MinimumQuantity = 15,
                                                    Location = "Kho A1",
                                                    LastUpdated = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                                                },
                                                new Inventory
                                                {
                                                    InventoryId = 2,
                                                    ProductId = 2,
                                                    StockCode = "SC-MBP14M4",
                                                    CurrentQuantity = 45,
                                                    MinimumQuantity = 10,
                                                    Location = "Kho B2",
                                                    LastUpdated = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                                                },
                                                new Inventory
                                                {
                                                    InventoryId = 3,
                                                    ProductId = 3,
                                                    StockCode = "SC-S24U256",
                                                    CurrentQuantity = 80,
                                                    MinimumQuantity = 20,
                                                    Location = "Kho A2",
                                                    LastUpdated = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                                                },
                                                new Inventory
                                                {
                                                    InventoryId = 4,
                                                    ProductId = 4,
                                                    StockCode = "SC-DXPS16",
                                                    CurrentQuantity = 25,
                                                    MinimumQuantity = 5,
                                                    Location = "Kho C1",
                                                    LastUpdated = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                                                }
                                            );
                                        }
                                    }
                                }
                                
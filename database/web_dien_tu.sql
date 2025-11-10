/**********************************************
 * PhoneShopFull_Init_FullRealistic.sql (MODIFIED)
 * Full DB schema + seed data (realistic, ~30 products)
 * Target: Microsoft SQL Server 2016+
 **********************************************/

SET NOCOUNT ON;
GO

-- Create database if not exists
IF DB_ID(N'PhoneShopFull') IS NULL
BEGIN
    CREATE DATABASE PhoneShopFull;
    PRINT 'Database PhoneShopFull created.'
END
GO

USE PhoneShopFull;
GO

/***************************************
 * Drop existing objects (safe for re-run)
 ***************************************/
-- Drop in dependency order where necessary
IF OBJECT_ID('dbo.trg_OrderDetails_Insert','TR') IS NOT NULL DROP TRIGGER dbo.trg_OrderDetails_Insert;
IF OBJECT_ID('dbo.sp_CreateOrderFromCart','P') IS NOT NULL DROP PROC dbo.sp_CreateOrderFromCart;
IF OBJECT_ID('dbo.fn_GetOrderTotal','FN') IS NOT NULL DROP FUNCTION dbo.fn_GetOrderTotal;
IF OBJECT_ID('dbo.sp_DeleteProduct','P') IS NOT NULL DROP PROC dbo.sp_DeleteProduct;
IF OBJECT_ID('dbo.sp_UpdateProduct','P') IS NOT NULL DROP PROC dbo.sp_UpdateProduct;
IF OBJECT_ID('dbo.sp_AddProduct','P') IS NOT NULL DROP PROC dbo.sp_AddProduct;
IF OBJECT_ID('dbo.sp_GetProductById','P') IS NOT NULL DROP PROC dbo.sp_GetProductById;
IF OBJECT_ID('dbo.sp_GetProductsPaged','P') IS NOT NULL DROP PROC dbo.sp_GetProductsPaged;
IF OBJECT_ID('dbo.sp_AuthenticateCustomer','P') IS NOT NULL DROP PROC dbo.sp_AuthenticateCustomer;
IF OBJECT_ID('dbo.sp_AuthenticateUser','P') IS NOT NULL DROP PROC dbo.sp_AuthenticateUser;

IF OBJECT_ID('dbo.vw_SalesByMonth','V') IS NOT NULL DROP VIEW dbo.vw_SalesByMonth;
IF OBJECT_ID('dbo.vw_SalesByProduct','V') IS NOT NULL DROP VIEW dbo.vw_SalesByProduct;
IF OBJECT_ID('dbo.vw_ProductStock','V') IS NOT NULL DROP VIEW dbo.vw_ProductStock;

-- Drop tables if exist (order-insensitive here because we'll recreate FK)
IF OBJECT_ID('dbo.CauHinhSanPhamDT','U') IS NOT NULL DROP TABLE dbo.CauHinhSanPhamDT;
IF OBJECT_ID('dbo.CauHinhSanPhamLAPTOP','U') IS NOT NULL DROP TABLE dbo.CauHinhSanPhamLAPTOP;
IF OBJECT_ID('dbo.OrderDetails','U') IS NOT NULL DROP TABLE dbo.OrderDetails;
IF OBJECT_ID('dbo.Orders','U') IS NOT NULL DROP TABLE dbo.Orders;
IF OBJECT_ID('dbo.CartDetails','U') IS NOT NULL DROP TABLE dbo.CartDetails;
IF OBJECT_ID('dbo.Carts','U') IS NOT NULL DROP TABLE dbo.Carts;
IF OBJECT_ID('dbo.ProductImages','U') IS NOT NULL DROP TABLE dbo.ProductImages;
IF OBJECT_ID('dbo.Products','U') IS NOT NULL DROP TABLE dbo.Products;
IF OBJECT_ID('dbo.Categories','U') IS NOT NULL DROP TABLE dbo.Categories;
IF OBJECT_ID('dbo.Customers','U') IS NOT NULL DROP TABLE dbo.Customers;
IF OBJECT_ID('dbo.Users','U') IS NOT NULL DROP TABLE dbo.Users;
IF OBJECT_ID('dbo.Roles','U') IS NOT NULL DROP TABLE dbo.Roles;
IF OBJECT_ID('dbo.AuditLogs','U') IS NOT NULL DROP TABLE dbo.AuditLogs;
IF OBJECT_ID('dbo.Brands','U') IS NOT NULL DROP TABLE dbo.Brands; -- New table
GO

/***************************************
 * Tables (create) - MODIFIED
 ***************************************/

-- Roles
CREATE TABLE dbo.Roles
(
    RoleId INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(50) NOT NULL UNIQUE,
    Description NVARCHAR(250) NULL
);
GO

-- Users
CREATE TABLE dbo.Users
(
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash VARBINARY(64) NOT NULL,
    FullName NVARCHAR(150) NULL,
    Email NVARCHAR(150) NULL,
    RoleId INT NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Users_Roles FOREIGN KEY(RoleId) REFERENCES dbo.Roles(RoleId)
);
GO

-- Customers
CREATE TABLE dbo.Customers
(
    CustomerId INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(150) NOT NULL,
    Email NVARCHAR(150) NOT NULL UNIQUE,
    PasswordHash VARBINARY(64) NOT NULL,
    Phone NVARCHAR(30) NULL,
    Address NVARCHAR(300) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    IsActive BIT NOT NULL DEFAULT 1
);
GO

-- Categories
CREATE TABLE dbo.Categories
(
    CategoryId INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName NVARCHAR(120) NOT NULL,
    Description NVARCHAR(500) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

-- Brands (NEW TABLE)
CREATE TABLE dbo.Brands
(
    BrandId INT IDENTITY(1,1) PRIMARY KEY,
    BrandName NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(500) NULL
);
GO

-- Products (MODIFIED: BrandId replaces Brand)
CREATE TABLE dbo.Products
(
    ProductId INT IDENTITY(1,1) PRIMARY KEY,
    CategoryId INT NOT NULL,
    BrandId INT NOT NULL, -- NEW: Foreign Key to Brands
    SKU NVARCHAR(60) NOT NULL UNIQUE,
    Name NVARCHAR(250) NOT NULL,
    Price DECIMAL(18,2) NOT NULL DEFAULT 0,
    OldPrice DECIMAL(18,2) NULL,
    Stock INT NOT NULL DEFAULT 0,
    DefaultImage NVARCHAR(300) NULL,
    ShortDescription NVARCHAR(1000) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    IsActive BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_Products_Categories FOREIGN KEY(CategoryId) REFERENCES dbo.Categories(CategoryId),
    CONSTRAINT FK_Products_Brands FOREIGN KEY(BrandId) REFERENCES dbo.Brands(BrandId) -- NEW FK
);
GO

CREATE INDEX IX_Products_BrandId ON dbo.Products(BrandId);
CREATE INDEX IX_Products_Price ON dbo.Products(Price);
GO

-- ProductImages
CREATE TABLE dbo.ProductImages
(
    ImageId INT IDENTITY(1,1) PRIMARY KEY,
    ProductId INT NOT NULL,
    ImagePath NVARCHAR(300) NOT NULL,
    IsPrimary BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_ProductImages_Products FOREIGN KEY(ProductId) REFERENCES dbo.Products(ProductId) ON DELETE CASCADE
);
GO

-- Carts
CREATE TABLE dbo.Carts
(
    CartId INT IDENTITY(1,1) PRIMARY KEY,
    CustomerId INT NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_Carts_Customers FOREIGN KEY(CustomerId) REFERENCES dbo.Customers(CustomerId)
);
GO

-- CartDetails
CREATE TABLE dbo.CartDetails
(
    CartDetailId INT IDENTITY(1,1) PRIMARY KEY,
    CartId INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL DEFAULT 1,
    UnitPrice DECIMAL(18,2) NOT NULL,
    CONSTRAINT FK_CartDetails_Carts FOREIGN KEY(CartId) REFERENCES dbo.Carts(CartId) ON DELETE CASCADE,
    CONSTRAINT FK_CartDetails_Products FOREIGN KEY(ProductId) REFERENCES dbo.Products(ProductId)
);
GO

-- Orders
CREATE TABLE dbo.Orders
(
    OrderId INT IDENTITY(1,1) PRIMARY KEY,
    CustomerId INT NOT NULL,
    OrderDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    Total DECIMAL(18,2) NOT NULL DEFAULT 0,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending',
    ShippingAddress NVARCHAR(300) NULL,
    CreatedByUserId INT NULL,
    CONSTRAINT FK_Orders_Customers FOREIGN KEY(CustomerId) REFERENCES dbo.Customers(CustomerId),
    CONSTRAINT FK_Orders_Users FOREIGN KEY(CreatedByUserId) REFERENCES dbo.Users(UserId)
);
GO

-- OrderDetails
CREATE TABLE dbo.OrderDetails
(
    OrderDetailId INT IDENTITY(1,1) PRIMARY KEY,
    OrderId INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    CONSTRAINT FK_OrderDetails_Orders FOREIGN KEY(OrderId) REFERENCES dbo.Orders(OrderId) ON DELETE CASCADE,
    CONSTRAINT FK_OrderDetails_Products FOREIGN KEY(ProductId) REFERENCES dbo.Products(ProductId)
);
GO

-- AuditLogs
CREATE TABLE dbo.AuditLogs
(
    LogId INT IDENTITY(1,1) PRIMARY KEY,
    LogTime DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    Username NVARCHAR(150) NULL,
    Action NVARCHAR(250) NULL,
    Details NVARCHAR(MAX) NULL
);
GO

/***************************************
 * Bảng cấu hình Laptop
 ***************************************/
CREATE TABLE dbo.CauHinhSanPhamLAPTOP
(
    ID_CAUHINH INT IDENTITY(1,1) PRIMARY KEY,
    MaSanPham INT NOT NULL, -- FK tới Products
    CPU NVARCHAR(100) NULL,
    SoNhan NVARCHAR(50) NULL,
    SoLuongLuongo NVARCHAR(50) NULL,
    RAM NVARCHAR(50) NULL,
    OCung NVARCHAR(100) NULL,
    CardDoHoa NVARCHAR(100) NULL,
    Pin NVARCHAR(50) NULL,
    OS NVARCHAR(50) NULL,
    KichThuocManHinh NVARCHAR(50) NULL,
    CongNgheManHinh NVARCHAR(50) NULL,
    DoPhanGiai NVARCHAR(50) NULL,
    CongGiaoTiep NVARCHAR(200) NULL,
    MauSac NVARCHAR(50) NULL,
    TrongLuong NVARCHAR(50) NULL,
    CONSTRAINT FK_CauHinhSanPhamLAPTOP_Products FOREIGN KEY(MaSanPham) REFERENCES dbo.Products(ProductId)
);
GO

/***************************************
 * Bảng cấu hình Điện thoại
 ***************************************/
CREATE TABLE dbo.CauHinhSanPhamDT
(
    ID_CAUHINH INT IDENTITY(1,1) PRIMARY KEY,
    MaSanPham INT NOT NULL, -- FK tới Products
    CPU NVARCHAR(100) NULL,
    SoNhan NVARCHAR(50) NULL,
    SoLuongLuongo NVARCHAR(50) NULL,
    RAM NVARCHAR(50) NULL,
    BoNhoTrong NVARCHAR(50) NULL,
    Pin NVARCHAR(50) NULL,
    HeDieuHanh NVARCHAR(50) NULL,
    ManHinh NVARCHAR(50) NULL,
    CongNgheManHinh NVARCHAR(50) NULL,
    DoPhanGiai NVARCHAR(50) NULL,
    Camera NVARCHAR(200) NULL,
    CongGiaoTiep NVARCHAR(200) NULL,
    MauSac NVARCHAR(50) NULL,
    CONSTRAINT FK_CauHinhSanPhamDT_Products FOREIGN KEY(MaSanPham) REFERENCES dbo.Products(ProductId)
);
GO

/***************************************
 * Seed data (roles/users/customers/categories/brands/products ~30) - MODIFIED
 ***************************************/

-- Roles
INSERT INTO dbo.Roles(RoleName, Description) VALUES
('Admin','System administrator'),
('Staff','Store staff / product manager'),
('Customer','End user / buyer');
GO

-- Users (demo hashed passwords)
INSERT INTO dbo.Users(Username, PasswordHash, FullName, Email, RoleId)
VALUES
('admin', HASHBYTES('SHA2_256', 'admin123'), 'Administrator', 'admin@phoneshop.local', 1),
('staff1', HASHBYTES('SHA2_256', 'staff123'), 'Staff One', 'staff1@phoneshop.local', 2);
GO

-- Customers
INSERT INTO dbo.Customers(FullName, Email, PasswordHash, Phone, Address)
VALUES
('Nguyen Van A','a.nguyen@example.com', HASHBYTES('SHA2_256','pass123'), '0912345678','HCM, District 1'),
('Tran Thi B','b.tran@example.com', HASHBYTES('SHA2_256','pass123'), '0987654321','Da Nang'),
('Le Van C','c.le@example.com', HASHBYTES('SHA2_256','pass123'), '0901122334','Ha Noi');
GO

-- Categories
INSERT INTO dbo.Categories(CategoryName, Description)
VALUES
('Smartphones','Điện thoại thông minh: Android & iOS'),
('Laptops','Laptop: Ultrabook, Gaming, Workstation'),
('Televisions','TV: LED, OLED, QLED, Smart TV'),
('Phone Accessories','Phụ kiện điện thoại: ốp, cáp, sạc, tai nghe'),
('Computer Accessories','Phụ kiện máy tính: chuột, bàn phím, sạc, ổ cứng'),
('Audio & Wearables','Tai nghe, loa, smartwatch, earbuds');
GO

-- Brands (NEW DATA)
INSERT INTO dbo.Brands(BrandName, Description)
VALUES
('Apple', 'Thương hiệu công nghệ hàng đầu thế giới'),
('Samsung', 'Thương hiệu điện tử Hàn Quốc'),
('Google', 'Thương hiệu sản phẩm của Google'),
('Xiaomi', 'Thương hiệu điện thoại & thiết bị thông minh Trung Quốc'),
('OnePlus', 'Thương hiệu smartphone cao cấp'),
('Dell', 'Thương hiệu máy tính cá nhân Mỹ'),
('Razer', 'Thương hiệu thiết bị chơi game'),
('ASUS', 'Thương hiệu máy tính Đài Loan'),
('Lenovo', 'Thương hiệu máy tính Trung Quốc'),
('LG', 'Thương hiệu điện tử Hàn Quốc'),
('AccessoryBrand', 'Thương hiệu chung cho phụ kiện'),
('Logitech', 'Thương hiệu thiết bị ngoại vi'),
('KeyBrand', 'Thương hiệu bàn phím chung'),
('StorageBrand', 'Thương hiệu ổ cứng chung'),
('Sony', 'Thương hiệu điện tử Nhật Bản'),
('JBL', 'Thương hiệu âm thanh Mỹ'),
('AudioBrand', 'Thương hiệu thiết bị âm thanh chung');
GO

-- Get BrandIds for use in Products INSERT
DECLARE @AppleId INT = (SELECT BrandId FROM dbo.Brands WHERE BrandName = 'Apple');
DECLARE @SamsungId INT = (SELECT BrandId FROM dbo.Brands WHERE BrandName = 'Samsung');
DECLARE @GoogleId INT = (SELECT BrandId FROM dbo.Brands WHERE BrandName = 'Google');
DECLARE @XiaomiId INT = (SELECT BrandId FROM dbo.Brands WHERE BrandName = 'Xiaomi');
DECLARE @OnePlusId INT = (SELECT BrandId FROM dbo.Brands WHERE BrandName = 'OnePlus');
DECLARE @DellId INT = (SELECT BrandId FROM dbo.Brands WHERE BrandName = 'Dell');
DECLARE @RazerId INT = (SELECT BrandId FROM dbo.Brands WHERE BrandName = 'Razer');
DECLARE @ASUSId INT = (SELECT BrandId FROM dbo.Brands WHERE BrandName = 'ASUS');
DECLARE @LenovoId INT = (SELECT BrandId FROM dbo.Brands WHERE BrandName = 'Lenovo');
DECLARE @LGId INT = (SELECT BrandId FROM dbo.Brands WHERE BrandName = 'LG');
DECLARE @AccessoryBrandId INT = (SELECT BrandId FROM dbo.Brands WHERE BrandName = 'AccessoryBrand');
DECLARE @LogitechId INT = (SELECT BrandId FROM dbo.Brands WHERE BrandName = 'Logitech');
DECLARE @KeyBrandId INT = (SELECT BrandId FROM dbo.Brands WHERE BrandName = 'KeyBrand');
DECLARE @StorageBrandId INT = (SELECT BrandId FROM dbo.Brands WHERE BrandName = 'StorageBrand');
DECLARE @SonyId INT = (SELECT BrandId FROM dbo.Brands WHERE BrandName = 'Sony');
DECLARE @JBLId INT = (SELECT BrandId FROM dbo.Brands WHERE BrandName = 'JBL');
DECLARE @AudioBrandId INT = (SELECT BrandId FROM dbo.Brands WHERE BrandName = 'AudioBrand');


-- Products (~30 realistic) - MODIFIED to use BrandId
INSERT INTO dbo.Products (CategoryId, BrandId, SKU, Name, Price, OldPrice, Stock, DefaultImage, ShortDescription)
VALUES
-- Smartphones (CategoryId = 1)
(1,@AppleId,'IP16PM-001','iPhone 16 Pro Max',34990000,37990000,15,'/images/iphone16promax.jpg','Apple A18 Pro, 48MP camera, OLED'),
(1,@AppleId,'IP16-001','iPhone 16',25990000,28990000,20,'/images/iphone16.jpg','Apple A17, camera kép, MagSafe'),
(1,@SamsungId,'S24U-001','Samsung Galaxy S24 Ultra',32990000,35990000,12,'/images/s24ultra.jpg','Snapdragon/Exynos, 200MP camera'),
(1,@GoogleId,'PX9P-001','Google Pixel 9 Pro',25990000,27990000,9,'/images/pixel9pro.jpg','Tensor G4, AI camera'),
(1,@XiaomiId,'XIAO-14-U','Xiaomi 14 Ultra',22990000,24990000,10,'/images/xiaomi14ultra.jpg','Leica camera, 6.73\" AMOLED'),
(1,@OnePlusId,'OP12-001','OnePlus 12',21990000,23990000,14,'/images/oneplus12.jpg','Snapdragon top-tier, smooth OS'),
(1,@SamsungId,'S24-001','Samsung Galaxy S24',19990000,21990000,18,'/images/s24.jpg','Flagship compact'),

-- Laptops (CategoryId = 2)
(2,@AppleId,'MBP-16-2025','MacBook Pro 16 (M4)',64990000,69990000,8,'/images/macbookpro16_m4.jpg','Apple M4, up to 24-core CPU'),
(2,@AppleId,'MB-13-2025','MacBook Air 13 (M4)',32990000,34990000,10,'/images/macbookair13_m4.jpg','M4, thin & light'),
(2,@DellId,'XPS-15-2025','Dell XPS 15',42990000,46990000,10,'/images/dell_xps15.jpg','Intel i9, up to 32GB RAM'),
(2,@RazerId,'RZ-17G-2025','Razer Blade 17',54990000,57990000,5,'/images/razerblade17.jpg','Intel i9, NVIDIA RTX 40-series, gaming'),
(2,@ASUSId,'AS-GF-15','Asus ROG Flow',38990000,41990000,7,'/images/asus_rog.jpg','Gaming machine, RTX options'),
(2,@LenovoId,'LENO-IDEA7','Lenovo IdeaPad 7',17990000,19990000,18,'/images/lenovo_ideapad7.jpg','Efficient performance, Ryzen'),

-- Televisions (CategoryId = 3)
(3,@LGId,'OLED55-2025','LG OLED 55\" C-Series',24990000,27990000,6,'/images/lg_oled55.jpg','OLED 4K, Smart TV'),
(3,@SamsungId,'QLED65-2025','Samsung QLED 65\"',31990000,34990000,4,'/images/samsung_qled65.jpg','QLED 4K'),

-- Phone Accessories (CategoryId = 4)
(4,@AccessoryBrandId,'CASE-001','Ốp lưng chống sốc (Universal)',299000,399000,120,'/images/case001.jpg','Ốp lưng bảo vệ cho nhiều dòng'),
(4,@AccessoryBrandId,'CHG-65W','Adapter sạc nhanh 65W',399000,499000,200,'/images/charger65w.jpg','Sạc PD 65W'),
(4,@AccessoryBrandId,'CABLE-USBC-1M','Cáp USB-C 1m',99000,129000,300,'/images/usb_cable.jpg','Cáp sạc & dữ liệu'),
(4,@AccessoryBrandId,'PROT-GLASS','Kính cường lực',99000,129000,200,'/images/screen_protector.jpg','Tempered glass'),

-- Computer Accessories (CategoryId = 5)
(5,@LogitechId,'MOUSE-G502','Logitech G502 HERO',1290000,1490000,60,'/images/logitech_g502.jpg','Gaming mouse, high DPI'),
(5,@KeyBrandId,'KB-MECH-01','Bàn phím cơ RGB',990000,1190000,50,'/images/keyboard_mech.jpg','Hot-swap, RGB'),
(5,@StorageBrandId,'SSD-1TB','SSD NVMe 1TB',2399000,2799000,40,'/images/ssd_1tb.jpg','Fast NVMe storage'),
(5,@StorageBrandId,'EXT-HDD-2TB','HDD External 2TB',1999000,2299000,35,'/images/hdd_2tb.jpg','Portable HDD'),

-- Audio & Wearables (CategoryId = 6)
(6,@SonyId,'BH-ANC1','Sony WH-1000XM5',6790000,7290000,20,'/images/sony_wh1000xm5.jpg','ANC, long battery'),
(6,@AppleId,'WATCH-5','Apple Watch Series 9',11990000,12990000,12,'/images/apple_watch9.jpg','Health & fitness'),
(6,@JBLId,'SPEAKER-1','JBL Flip 6',1999000,2299000,30,'/images/jbl_flip6.jpg','Waterproof Bluetooth speaker'),
(6,@AudioBrandId,'EAR-SPORTS','Earbuds Sport',499000,699000,60,'/images/earbuds_sport.jpg','Sweat resistant');
GO

-- product images (set primary images)
INSERT INTO dbo.ProductImages (ProductId, ImagePath, IsPrimary)
SELECT ProductId, DefaultImage, 1 FROM dbo.Products WHERE DefaultImage IS NOT NULL;
GO

-- Create carts for first two customers
INSERT INTO dbo.Carts(CustomerId) VALUES (1), (2);
GO

-- Sample cart details
INSERT INTO dbo.CartDetails (CartId, ProductId, Quantity, UnitPrice)
VALUES
(1, (SELECT TOP 1 ProductId FROM dbo.Products WHERE SKU='IP16PM-001'), 1, (SELECT Price FROM dbo.Products WHERE SKU='IP16PM-001')),
(1, (SELECT TOP 1 ProductId FROM dbo.Products WHERE SKU='CABLE-USBC-1M'), 2, (SELECT Price FROM dbo.Products WHERE SKU='CABLE-USBC-1M')),
(2, (SELECT TOP 1 ProductId FROM dbo.Products WHERE SKU='MBP-16-2025'), 1, (SELECT Price FROM dbo.Products WHERE SKU='MBP-16-2025'));
GO

/***************************************
 * Seed configurations (use SKU -> ProductId lookup)
 ***************************************/
-- Configuration seed data remains the same, referencing ProductId

-- ... (Configuration data for CauHinhSanPhamDT and CauHinhSanPhamLAPTOP is unchanged) ...

-- iPhone 16 Pro Max
INSERT INTO dbo.CauHinhSanPhamDT
(MaSanPham, CPU, SoNhan, SoLuongLuongo, RAM, BoNhoTrong, Pin, HeDieuHanh, ManHinh, CongNgheManHinh, DoPhanGiai, Camera, CongGiaoTiep, MauSac)
VALUES
((SELECT TOP 1 ProductId FROM dbo.Products WHERE SKU='IP16PM-001'),
 'Apple A18 Pro', NULL, NULL, '8GB','256GB','4500mAh','iOS 18','6.7 inch','XDR OLED','2796x1290','48MP + 12MP + 12MP','Lightning, 5G, WiFi6E','Titanium');

-- iPhone 16
INSERT INTO dbo.CauHinhSanPhamDT
(MaSanPham, CPU, RAM, BoNhoTrong, Pin, HeDieuHanh, ManHinh, CongNgheManHinh, DoPhanGiai, Camera, CongGiaoTiep, MauSac)
VALUES
((SELECT TOP 1 ProductId FROM dbo.Products WHERE SKU='IP16-001'),
 'Apple A17', '6GB','128GB','3800mAh','iOS 17','6.1 inch','OLED','2556x1179','48MP dual','Lightning, 5G','Black');

-- Samsung Galaxy S24 Ultra
INSERT INTO dbo.CauHinhSanPhamDT
(MaSanPham, CPU, RAM, BoNhoTrong, Pin, HeDieuHanh, ManHinh, CongNgheManHinh, DoPhanGiai, Camera, CongGiaoTiep, MauSac)
VALUES
((SELECT TOP 1 ProductId FROM dbo.Products WHERE SKU='S24U-001'),
 'Snapdragon 8 Gen 3','12GB','256GB','5000mAh','Android 14','6.8 inch','Dynamic AMOLED','3088x1440','200MP + 12MP + 10MP','USB-C, 5G, WiFi6E','Phantom Black');

-- Google Pixel 9 Pro
INSERT INTO dbo.CauHinhSanPhamDT
(MaSanPham, CPU, RAM, BoNhoTrong, Pin, HeDieuHanh, ManHinh, CongNgheManHinh, DoPhanGiai, Camera, CongGiaoTiep, MauSac)
VALUES
((SELECT TOP 1 ProductId FROM dbo.Products WHERE SKU='PX9P-001'),
 'Google Tensor G4','12GB','256GB','4900mAh','Android 14','6.7 inch','LTPO OLED','3120x1440','50MP + 48MP + 12MP','USB-C, 5G','Seafoam');

-- Xiaomi 14 Ultra
INSERT INTO dbo.CauHinhSanPhamDT
(MaSanPham, CPU, RAM, BoNhoTrong, Pin, HeDieuHanh, ManHinh, CongNgheManHinh, DoPhanGiai, Camera, CongGiaoTiep, MauSac)
VALUES
((SELECT TOP 1 ProductId FROM dbo.Products WHERE SKU='XIAO-14-U'),
 'Snapdragon 8 Gen 3','12GB','512GB','4900mAh','Android 14','6.73 inch','AMOLED','3200x1440','50MP (Leica)','USB-C, 5G','White');

-- OnePlus 12
INSERT INTO dbo.CauHinhSanPhamDT
(MaSanPham, CPU, RAM, BoNhoTrong, Pin, HeDieuHanh, ManHinh, CongNgheManHinh, DoPhanGiai, Camera, CongGiaoTiep, MauSac)
VALUES
((SELECT TOP 1 ProductId FROM dbo.Products WHERE SKU='OP12-001'),
 'Snapdragon 8 Gen 3','12GB','256GB','5000mAh','Android 14','6.82 inch','AMOLED','3168x1440','50MP + 48MP','USB-C, 5G','Black');

-- Samsung S24 (compact)
INSERT INTO dbo.CauHinhSanPhamDT
(MaSanPham, CPU, RAM, BoNhoTrong, Pin, HeDieuHanh, ManHinh, CongNgheManHinh, DoPhanGiai, Camera, CongGiaoTiep, MauSac)
VALUES
((SELECT TOP 1 ProductId FROM dbo.Products WHERE SKU='S24-001'),
 'Snapdragon 8 Gen 3','8GB','128GB','3900mAh','Android 14','6.2 inch','Dynamic AMOLED','2340x1080','50MP','USB-C, 5G','Cream');

-- MacBook Pro 16 (M4)
INSERT INTO dbo.CauHinhSanPhamLAPTOP
(MaSanPham, CPU, SoNhan, SoLuongLuongo, RAM, OCung, CardDoHoa, Pin, OS, KichThuocManHinh, CongNgheManHinh, DoPhanGiai, CongGiaoTiep, MauSac, TrongLuong)
VALUES
((SELECT TOP 1 ProductId FROM dbo.Products WHERE SKU='MBP-16-2025'),
 'Apple M4', NULL, NULL, '16GB','1TB SSD','Integrated Apple GPU','99Wh','macOS','16 inch','Liquid Retina XDR','3456x2234','Thunderbolt 4, HDMI','Space Gray','2.1 kg');

-- MacBook Air 13 (M4)
INSERT INTO dbo.CauHinhSanPhamLAPTOP
(MaSanPham, CPU, RAM, OCung, CardDoHoa, Pin, OS, KichThuocManHinh, CongNgheManHinh, DoPhanGiai, CongGiaoTiep, MauSac, TrongLuong)
VALUES
((SELECT TOP 1 ProductId FROM dbo.Products WHERE SKU='MB-13-2025'),
 'Apple M4','16GB','512GB SSD','Integrated Apple GPU','52Wh','macOS','13.6 inch','Liquid Retina','2560x1664','Thunderbolt 4','Silver','1.24 kg');

-- Dell XPS 15
INSERT INTO dbo.CauHinhSanPhamLAPTOP
(MaSanPham, CPU, SoNhan, SoLuongLuongo, RAM, OCung, CardDoHoa, Pin, OS, KichThuocManHinh, CongNgheManHinh, DoPhanGiai, CongGiaoTiep, MauSac, TrongLuong)
VALUES
((SELECT TOP 1 ProductId FROM dbo.Products WHERE SKU='XPS-15-2025'),
 'Intel Core i9-13900HK','8','16','32GB','1TB SSD','NVIDIA RTX 4060 (mobile)','86Wh','Windows 11','15.6 inch','OLED / FHD options','2880x1800','USB-C, HDMI','Silver','1.8 kg');

-- Razer Blade 17 (gaming)
INSERT INTO dbo.CauHinhSanPhamLAPTOP
(MaSanPham, CPU, SoNhan, SoLuongLuongo, RAM, OCung, CardDoHoa, Pin, OS, KichThuocManHinh, CongNgheManHinh, DoPhanGiai, CongGiaoTiep, MauSac, TrongLuong)
VALUES
((SELECT TOP 1 ProductId FROM dbo.Products WHERE SKU='RZ-17G-2025'),
 'Intel Core i9-13950HX','16','32','32GB','1TB SSD','NVIDIA RTX 4080 (mobile)','95Wh','Windows 11','17.3 inch','IPS 240Hz','2560x1600','USB-C, HDMI, Ethernet','Black','2.75 kg');

-- Asus ROG Flow
INSERT INTO dbo.CauHinhSanPhamLAPTOP
(MaSanPham, CPU, SoNhan, RAM, OCung, CardDoHoa, Pin, OS, KichThuocManHinh, CongNgheManHinh, DoPhanGiai, CongGiaoTiep, MauSac, TrongLuong)
VALUES
((SELECT TOP 1 ProductId FROM dbo.Products WHERE SKU='AS-GF-15'),
 'AMD Ryzen 9 7945HS','16','32GB','1TB SSD','NVIDIA RTX 4070 (mobile)','90Wh','Windows 11','15.6 inch','IPS','2560x1440','USB-C, HDMI','Black','1.9 kg');

-- Lenovo IdeaPad 7
INSERT INTO dbo.CauHinhSanPhamLAPTOP
(MaSanPham, CPU, SoNhan, RAM, OCung, CardDoHoa, Pin, OS, KichThuocManHinh, CongNgheManHinh, DoPhanGiai, CongGiaoTiep, MauSac, TrongLuong)
VALUES
((SELECT TOP 1 ProductId FROM dbo.Products WHERE SKU='LENO-IDEA7'),
 'AMD Ryzen 7 7840U','8','16GB','512GB SSD','Integrated','70Wh','Windows 11','15.6 inch','IPS','1920x1080','USB-C','Grey','1.7 kg');

-- Accessories & others: no config required but leave tables ready
GO

/***************************************
 * Stored Procedures & Functions - MODIFIED
 ***************************************/

-- Authenticate user (username) - UNCHANGED
CREATE PROCEDURE dbo.sp_AuthenticateUser
    @username NVARCHAR(100),
    @plaintextPassword NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @h VARBINARY(64) = HASHBYTES('SHA2_256', @plaintextPassword);

    SELECT u.UserId, u.Username, u.FullName, u.Email, u.RoleId, r.RoleName
    FROM dbo.Users u
    INNER JOIN dbo.Roles r ON u.RoleId = r.RoleId
    WHERE u.Username = @username AND u.PasswordHash = @h AND u.IsActive = 1;
END
GO

-- Authenticate customer (email) - UNCHANGED
CREATE PROCEDURE dbo.sp_AuthenticateCustomer
    @email NVARCHAR(150),
    @plaintextPassword NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @h VARBINARY(64) = HASHBYTES('SHA2_256', @plaintextPassword);

    SELECT c.CustomerId, c.FullName, c.Email
    FROM dbo.Customers c
    WHERE c.Email = @email AND c.PasswordHash = @h AND c.IsActive = 1;
END
GO

-- Get products paged - MODIFIED to join Brands table
CREATE PROCEDURE dbo.sp_GetProductsPaged
    @CategoryId INT = NULL,
    @Search NVARCHAR(250) = NULL,
    @BrandId INT = NULL, -- NEW parameter for filtering by brand
    @MinPrice DECIMAL(18,2) = NULL,
    @MaxPrice DECIMAL(18,2) = NULL,
    @PageIndex INT = 1,
    @PageSize INT = 12
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Offset INT = (@PageIndex - 1) * @PageSize;

    SELECT p.ProductId, p.SKU, p.Name, b.BrandName, p.Price, p.OldPrice, p.Stock, p.DefaultImage, p.ShortDescription,
           c.CategoryName
    FROM dbo.Products p
    INNER JOIN dbo.Categories c ON p.CategoryId = c.CategoryId
    INNER JOIN dbo.Brands b ON p.BrandId = b.BrandId -- NEW JOIN
    WHERE (@CategoryId IS NULL OR p.CategoryId = @CategoryId)
      AND (@BrandId IS NULL OR p.BrandId = @BrandId) -- NEW filter
      AND (@Search IS NULL OR p.Name LIKE '%' + @Search + '%')
      AND (@MinPrice IS NULL OR p.Price >= @MinPrice)
      AND (@MaxPrice IS NULL OR p.Price <= @MaxPrice)
    ORDER BY p.CreatedAt DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- Get product by id (includes category and brand) - MODIFIED
CREATE PROCEDURE dbo.sp_GetProductById
    @ProductId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT p.*, c.CategoryName, b.BrandName
    FROM dbo.Products p
    INNER JOIN dbo.Categories c ON p.CategoryId = c.CategoryId
    INNER JOIN dbo.Brands b ON p.BrandId = b.BrandId -- NEW JOIN
    WHERE p.ProductId = @ProductId;
END
GO

-- Add product - MODIFIED to use BrandId
CREATE PROCEDURE dbo.sp_AddProduct
    @CategoryId INT,
    @BrandId INT, -- MODIFIED parameter
    @SKU NVARCHAR(60),
    @Name NVARCHAR(250),
    @Price DECIMAL(18,2),
    @OldPrice DECIMAL(18,2) = NULL,
    @Stock INT = 0,
    @DefaultImage NVARCHAR(300) = NULL,
    @ShortDescription NVARCHAR(1000) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.Products (CategoryId, BrandId, SKU, Name, Price, OldPrice, Stock, DefaultImage, ShortDescription)
    VALUES (@CategoryId, @BrandId, @SKU, @Name, @Price, @OldPrice, @Stock, @DefaultImage, @ShortDescription);

    SELECT SCOPE_IDENTITY() AS NewProductId;
END
GO

-- Update product - MODIFIED to use BrandId
CREATE PROCEDURE dbo.sp_UpdateProduct
    @ProductId INT,
    @CategoryId INT,
    @BrandId INT, -- MODIFIED parameter
    @SKU NVARCHAR(60),
    @Name NVARCHAR(250),
    @Price DECIMAL(18,2),
    @OldPrice DECIMAL(18,2) = NULL,
    @Stock INT = 0,
    @DefaultImage NVARCHAR(300) = NULL,
    @ShortDescription NVARCHAR(1000) = NULL,
    @IsActive BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Products SET
        CategoryId = @CategoryId,
        BrandId = @BrandId, -- MODIFIED column
        SKU = @SKU,
        Name = @Name,
        Price = @Price,
        OldPrice = @OldPrice,
        Stock = @Stock,
        DefaultImage = @DefaultImage,
        ShortDescription = @ShortDescription,
        IsActive = @IsActive
    WHERE ProductId = @ProductId;
END
GO

-- Delete product - UNCHANGED
CREATE PROCEDURE dbo.sp_DeleteProduct
    @ProductId INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM dbo.ProductImages WHERE ProductId = @ProductId;
    DELETE FROM dbo.Products WHERE ProductId = @ProductId;
END
GO

-- Function: order total - UNCHANGED
CREATE FUNCTION dbo.fn_GetOrderTotal(@OrderId INT)
RETURNS DECIMAL(18,2)
AS
BEGIN
    DECLARE @total DECIMAL(18,2) = 0;
    SELECT @total = SUM(UnitPrice * Quantity) FROM dbo.OrderDetails WHERE OrderId = @OrderId;
    RETURN ISNULL(@total,0);
END
GO

-- Create order from cart - UNCHANGED
CREATE PROCEDURE dbo.sp_CreateOrderFromCart
    @CartId INT,
    @ShippingAddress NVARCHAR(300),
    @CreatedByUserId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        IF NOT EXISTS (SELECT 1 FROM dbo.Carts WHERE CartId = @CartId)
        BEGIN
            THROW 51000, 'Cart not found', 1;
        END

        IF EXISTS (
            SELECT 1 FROM dbo.CartDetails cd
            JOIN dbo.Products p ON cd.ProductId = p.ProductId
            WHERE cd.CartId = @CartId AND cd.Quantity > p.Stock
        )
        BEGIN
            THROW 51001, 'Insufficient stock for one or more items', 1;
        END

        DECLARE @total DECIMAL(18,2) = 0;
        SELECT @total = SUM(UnitPrice * Quantity) FROM dbo.CartDetails WHERE CartId = @CartId;

        INSERT INTO dbo.Orders (CustomerId, Total, ShippingAddress, Status, CreatedByUserId)
        SELECT c.CustomerId, @total, @ShippingAddress, 'Processing', @CreatedByUserId
        FROM dbo.Carts c WHERE c.CartId = @CartId;

        DECLARE @orderId INT = SCOPE_IDENTITY();

        INSERT INTO dbo.OrderDetails (OrderId, ProductId, Quantity, UnitPrice)
        SELECT @orderId, cd.ProductId, cd.Quantity, cd.UnitPrice
        FROM dbo.CartDetails cd WHERE cd.CartId = @CartId;

        UPDATE p
        SET p.Stock = p.Stock - od.Quantity
        FROM dbo.Products p
        JOIN dbo.OrderDetails od ON p.ProductId = od.ProductId
        WHERE od.OrderId = @orderId;

        DELETE FROM dbo.CartDetails WHERE CartId = @CartId;

        COMMIT TRANSACTION;

        SELECT @orderId AS NewOrderId;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
        DECLARE @errMsg NVARCHAR(4000) = ERROR_MESSAGE();
        THROW 51002, @errMsg, 1;
    END CATCH
END
GO

/***************************************
 * Triggers (stock check) - UNCHANGED
 ***************************************/
CREATE TRIGGER dbo.trg_OrderDetails_Insert
ON dbo.OrderDetails
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1 FROM inserted i
        JOIN dbo.Products p ON i.ProductId = p.ProductId
        WHERE i.Quantity > p.Stock
    )
    BEGIN
        RAISERROR('Insufficient stock for product during order insert',16,1);
        ROLLBACK TRANSACTION;
        RETURN;
    END

    UPDATE p
    SET p.Stock = p.Stock - i.Quantity
    FROM dbo.Products p
    JOIN inserted i ON p.ProductId = i.ProductId;
END
GO

/***************************************
 * Views for reports - MODIFIED
 ***************************************/
CREATE VIEW dbo.vw_ProductStock
AS
SELECT p.ProductId, p.SKU, p.Name, b.BrandName, p.Stock, p.Price, p.DefaultImage, c.CategoryName
FROM dbo.Products p
JOIN dbo.Categories c ON p.CategoryId = c.CategoryId
JOIN dbo.Brands b ON p.BrandId = b.BrandId; -- NEW JOIN
GO

CREATE VIEW dbo.vw_SalesByProduct
AS
SELECT p.ProductId, p.Name, b.BrandName, -- MODIFIED to use BrandName
       SUM(od.Quantity) AS TotalQty,
       SUM(od.Quantity * od.UnitPrice) AS TotalRevenue
FROM dbo.OrderDetails od
JOIN dbo.Products p ON od.ProductId = p.ProductId
JOIN dbo.Brands b ON p.BrandId = b.BrandId -- NEW JOIN
GROUP BY p.ProductId, p.Name, b.BrandName;
GO

CREATE VIEW dbo.vw_SalesByMonth
AS
SELECT 
    YEAR(o.OrderDate) AS Yr, 
    MONTH(o.OrderDate) AS Mth,
    COUNT(DISTINCT o.OrderId) AS OrdersCount,
    SUM(o.Total) AS Revenue
FROM dbo.Orders o
GROUP BY YEAR(o.OrderDate), MONTH(o.OrderDate);
GO

/***************************************
 * Final checks
 ***************************************/
PRINT 'Setup completed. Summary:';
SELECT
    (SELECT COUNT(*) FROM dbo.Roles) AS RoleCount,
    (SELECT COUNT(*) FROM dbo.Users) AS UserCount,
    (SELECT COUNT(*) FROM dbo.Customers) AS CustomerCount,
    (SELECT COUNT(*) FROM dbo.Categories) AS CategoryCount,
    (SELECT COUNT(*) FROM dbo.Brands) AS BrandCount, -- NEW COUNT
    (SELECT COUNT(*) FROM dbo.Products) AS ProductCount,
    (SELECT COUNT(*) FROM dbo.Carts) AS CartCount,
    (SELECT COUNT(*) FROM dbo.Orders) AS OrderCount;
GO

-- Usage examples:
-- EXEC dbo.sp_AuthenticateUser @username='admin', @plaintextPassword='admin123';
-- EXEC dbo.sp_GetProductsPaged @PageIndex=1, @PageSize=12, @BrandId=@AppleId;
-- EXEC dbo.sp_CreateOrderFromCart @CartId=1, @ShippingAddress='HCM, District 1';
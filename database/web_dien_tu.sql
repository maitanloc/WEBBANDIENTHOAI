/**********************************************
 * PhoneShopFull_Init_Extended.sql
 * Full DB schema + seed data (extended categories)
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
 * Tables (drop if exists then create)
 ***************************************/

-- Roles
IF OBJECT_ID('dbo.Roles','U') IS NOT NULL DROP TABLE dbo.Roles;
CREATE TABLE dbo.Roles
(
    RoleId INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(50) NOT NULL UNIQUE,
    Description NVARCHAR(250) NULL
);
GO

-- Users
IF OBJECT_ID('dbo.Users','U') IS NOT NULL DROP TABLE dbo.Users;
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
IF OBJECT_ID('dbo.Customers','U') IS NOT NULL DROP TABLE dbo.Customers;
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
IF OBJECT_ID('dbo.Categories','U') IS NOT NULL DROP TABLE dbo.Categories;
CREATE TABLE dbo.Categories
(
    CategoryId INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName NVARCHAR(120) NOT NULL,
    Description NVARCHAR(500) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

-- Products - Updated: Removed Stock column, added StockCode, KEPT ALL OTHER COLUMNS
IF OBJECT_ID('dbo.Products','U') IS NOT NULL DROP TABLE dbo.Products;
CREATE TABLE dbo.Products
(
    ProductId INT IDENTITY(1,1) PRIMARY KEY,
    CategoryId INT NOT NULL,
    SKU NVARCHAR(60) NOT NULL UNIQUE,
    Name NVARCHAR(250) NOT NULL,
    Brand NVARCHAR(100) NULL,
    Price DECIMAL(18,2) NOT NULL DEFAULT 0,
    OldPrice DECIMAL(18,2) NULL,
    StockCode NVARCHAR(50) NOT NULL, -- Mã tồn kho thay vì số lượng
    Color NVARCHAR(100) NULL,
    Size NVARCHAR(100) NULL,
    DefaultImage NVARCHAR(300) NULL,
    ShortDescription NVARCHAR(1000) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    IsActive BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_Products_Categories FOREIGN KEY(CategoryId) REFERENCES dbo.Categories(CategoryId)
);
GO

CREATE INDEX IX_Products_Brand ON dbo.Products(Brand);
CREATE INDEX IX_Products_Price ON dbo.Products(Price);
CREATE INDEX IX_Products_StockCode ON dbo.Products(StockCode);
GO

-- Orders (MOVED UP - must be created before ExportReceipts)
IF OBJECT_ID('dbo.Orders','U') IS NOT NULL DROP TABLE dbo.Orders;
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

-- OrderDetails (MOVED UP - must be created before ExportReceipts)
IF OBJECT_ID('dbo.OrderDetails','U') IS NOT NULL DROP TABLE dbo.OrderDetails;
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

-- Inventory - Bảng tồn kho mới
IF OBJECT_ID('dbo.Inventory','U') IS NOT NULL DROP TABLE dbo.Inventory;
CREATE TABLE dbo.Inventory
(
    InventoryId INT IDENTITY(1,1) PRIMARY KEY,
    StockCode NVARCHAR(50) NOT NULL UNIQUE, -- Mã tồn kho
    ProductId INT NOT NULL,
    CurrentQuantity INT NOT NULL DEFAULT 0, -- Số lượng hiện tại
    MinimumQuantity INT NOT NULL DEFAULT 0, -- Số lượng tối thiểu
    MaximumQuantity INT NOT NULL DEFAULT 1000, -- Số lượng tối đa
    Location NVARCHAR(100) NULL, -- Vị trí trong kho
    LastUpdated DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Inventory_Products FOREIGN KEY(ProductId) REFERENCES dbo.Products(ProductId)
);
GO

-- ImportReceipts - Phiếu nhập kho
IF OBJECT_ID('dbo.ImportReceipts','U') IS NOT NULL DROP TABLE dbo.ImportReceipts;
CREATE TABLE dbo.ImportReceipts
(
    ImportReceiptId INT IDENTITY(1,1) PRIMARY KEY,
    ReceiptNumber NVARCHAR(50) NOT NULL UNIQUE, -- Số phiếu nhập
    ImportDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    SupplierName NVARCHAR(200) NULL, -- Tên nhà cung cấp
    TotalQuantity INT NOT NULL DEFAULT 0, -- Tổng số lượng
    TotalValue DECIMAL(18,2) NOT NULL DEFAULT 0, -- Tổng giá trị
    CreatedByUserId INT NOT NULL, -- Người tạo phiếu
    Notes NVARCHAR(500) NULL, -- Ghi chú
    CONSTRAINT FK_ImportReceipts_Users FOREIGN KEY(CreatedByUserId) REFERENCES dbo.Users(UserId)
);
GO

-- ImportReceiptDetails - Chi tiết phiếu nhập
IF OBJECT_ID('dbo.ImportReceiptDetails','U') IS NOT NULL DROP TABLE dbo.ImportReceiptDetails;
CREATE TABLE dbo.ImportReceiptDetails
(
    ImportDetailId INT IDENTITY(1,1) PRIMARY KEY,
    ImportReceiptId INT NOT NULL,
    ProductId INT NOT NULL,
    StockCode NVARCHAR(50) NOT NULL, -- Mã tồn kho
    Quantity INT NOT NULL DEFAULT 0, -- Số lượng nhập
    UnitCost DECIMAL(18,2) NOT NULL DEFAULT 0, -- Giá nhập
    TotalCost DECIMAL(18,2) NOT NULL DEFAULT 0, -- Thành tiền
    BatchNumber NVARCHAR(100) NULL, -- Số lô
    ExpiryDate DATE NULL, -- Ngày hết hạn
    CONSTRAINT FK_ImportDetails_Receipts FOREIGN KEY(ImportReceiptId) REFERENCES dbo.ImportReceipts(ImportReceiptId) ON DELETE CASCADE,
    CONSTRAINT FK_ImportDetails_Products FOREIGN KEY(ProductId) REFERENCES dbo.Products(ProductId)
);
GO

-- ExportReceipts - Phiếu xuất kho (NOW CREATED AFTER Orders table exists)
IF OBJECT_ID('dbo.ExportReceipts','U') IS NOT NULL DROP TABLE dbo.ExportReceipts;
CREATE TABLE dbo.ExportReceipts
(
    ExportReceiptId INT IDENTITY(1,1) PRIMARY KEY,
    ReceiptNumber NVARCHAR(50) NOT NULL UNIQUE, -- Số phiếu xuất
    ExportDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CustomerId INT NULL, -- Khách hàng (nếu xuất bán)
    OrderId INT NULL, -- Liên kết với đơn hàng
    TotalQuantity INT NOT NULL DEFAULT 0, -- Tổng số lượng
    TotalValue DECIMAL(18,2) NOT NULL DEFAULT 0, -- Tổng giá trị
    CreatedByUserId INT NOT NULL, -- Người tạo phiếu
    Notes NVARCHAR(500) NULL, -- Ghi chú
    CONSTRAINT FK_ExportReceipts_Customers FOREIGN KEY(CustomerId) REFERENCES dbo.Customers(CustomerId),
    CONSTRAINT FK_ExportReceipts_Orders FOREIGN KEY(OrderId) REFERENCES dbo.Orders(OrderId),
    CONSTRAINT FK_ExportReceipts_Users FOREIGN KEY(CreatedByUserId) REFERENCES dbo.Users(UserId)
);
GO

-- ExportReceiptDetails - Chi tiết phiếu xuất
IF OBJECT_ID('dbo.ExportReceiptDetails','U') IS NOT NULL DROP TABLE dbo.ExportReceiptDetails;
CREATE TABLE dbo.ExportReceiptDetails
(
    ExportDetailId INT IDENTITY(1,1) PRIMARY KEY,
    ExportReceiptId INT NOT NULL,
    ProductId INT NOT NULL,
    StockCode NVARCHAR(50) NOT NULL, -- Mã tồn kho
    Quantity INT NOT NULL DEFAULT 0, -- Số lượng xuất
    UnitPrice DECIMAL(18,2) NOT NULL DEFAULT 0, -- Giá xuất
    TotalPrice DECIMAL(18,2) NOT NULL DEFAULT 0, -- Thành tiền
    CONSTRAINT FK_ExportDetails_Receipts FOREIGN KEY(ExportReceiptId) REFERENCES dbo.ExportReceipts(ExportReceiptId) ON DELETE CASCADE,
    CONSTRAINT FK_ExportDetails_Products FOREIGN KEY(ProductId) REFERENCES dbo.Products(ProductId)
);
GO

-- LaptopConfigurations - Cấu hình laptop
IF OBJECT_ID('dbo.LaptopConfigurations','U') IS NOT NULL DROP TABLE dbo.LaptopConfigurations;
CREATE TABLE dbo.LaptopConfigurations
(
    ConfigurationId INT IDENTITY(1,1) PRIMARY KEY,
    ProductId INT NOT NULL UNIQUE,
    CPU NVARCHAR(200) NULL,
    RAM NVARCHAR(100) NULL,
    Storage NVARCHAR(200) NULL, -- Ổ cứng
    GraphicsCard NVARCHAR(200) NULL, -- Card đồ họa
    Battery NVARCHAR(100) NULL, -- Pin
    OperatingSystem NVARCHAR(100) NULL, -- Hệ điều hành
    ScreenSize NVARCHAR(50) NULL, -- Kích thước màn hình
    ScreenTechnology NVARCHAR(100) NULL, -- Công nghệ màn hình
    Resolution NVARCHAR(100) NULL, -- Độ phân giải
    Ports NVARCHAR(500) NULL, -- Cổng giao tiếp
    Color NVARCHAR(50) NULL, -- Màu sắc
    Weight NVARCHAR(50) NULL, -- Trọng lượng
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_LaptopConfigs_Products FOREIGN KEY(ProductId) REFERENCES dbo.Products(ProductId)
);
GO

-- PhoneConfigurations - Cấu hình điện thoại
IF OBJECT_ID('dbo.PhoneConfigurations','U') IS NOT NULL DROP TABLE dbo.PhoneConfigurations;
CREATE TABLE dbo.PhoneConfigurations
(
    ConfigurationId INT IDENTITY(1,1) PRIMARY KEY,
    ProductId INT NOT NULL UNIQUE,
    CPU NVARCHAR(200) NULL,
    Cores NVARCHAR(50) NULL, -- Số nhân
    Threads NVARCHAR(50) NULL, -- Số luồng
    RAM NVARCHAR(100) NULL,
    InternalStorage NVARCHAR(100) NULL, -- Bộ nhớ trong
    Battery NVARCHAR(100) NULL, -- Pin
    OperatingSystem NVARCHAR(100) NULL, -- Hệ điều hành
    Screen NVARCHAR(200) NULL, -- Màn hình
    ScreenTechnology NVARCHAR(100) NULL, -- Công nghệ màn hình
    Resolution NVARCHAR(100) NULL, -- Độ phân giải
    Camera NVARCHAR(500) NULL, -- Camera
    Ports NVARCHAR(500) NULL, -- Cổng giao tiếp
    Color NVARCHAR(50) NULL, -- Màu sắc
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_PhoneConfigs_Products FOREIGN KEY(ProductId) REFERENCES dbo.Products(ProductId)
);
GO

-- ProductImages
IF OBJECT_ID('dbo.ProductImages','U') IS NOT NULL DROP TABLE dbo.ProductImages;
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
IF OBJECT_ID('dbo.Carts','U') IS NOT NULL DROP TABLE dbo.Carts;
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
IF OBJECT_ID('dbo.CartDetails','U') IS NOT NULL DROP TABLE dbo.CartDetails;
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

-- AuditLogs
IF OBJECT_ID('dbo.AuditLogs','U') IS NOT NULL DROP TABLE dbo.AuditLogs;
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
 * Seed data (extended categories & products)
 ***************************************/

-- Roles
INSERT INTO dbo.Roles(RoleName, Description) VALUES
('Admin','System administrator'),
('Staff','Store staff / product manager'),
('Customer','End user / buyer');
GO

-- Admin + staff users (hashed password demo)
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

-- Categories (expanded)
INSERT INTO dbo.Categories(CategoryName, Description)
VALUES
('Smartphones','Điện thoại thông minh: Android & iOS'),
('Laptops','Laptop: Ultrabook, Gaming, Workstation'),
('Televisions','TV: LED, OLED, QLED, Smart TV'),
('Phone Accessories','Phụ kiện điện thoại: ốp, cáp, sạc, tai nghe'),
('Computer Accessories','Phụ kiện máy tính: chuột, bàn phím, sạc, ổ cứng'),
('Audio & Wearables','Tai nghe, loa, smartwatch, earbuds');
GO

/****************************************
 * Products (FULL VERSION - với đầy đủ các cột)
 ****************************************/

INSERT INTO dbo.Products (CategoryId, SKU, Name, Brand, Price, OldPrice, StockCode, Color, Size, DefaultImage, ShortDescription)
VALUES
-- Smartphones (CategoryId = 1)
(1,'IP16PM-001','iPhone 16 Pro Max','Apple',34990000,37990000,'STK-IP16PM-001','Titanium','6.7 inch','/images/iphone16promax.jpg','Chip A18 Pro, Camera 48MP, OLED'),
(1,'IP16-001','iPhone 16','Apple',25990000,28990000,'STK-IP16-001','Black','6.1 inch','/images/iphone16.jpg','Chip A17, camera kép, MagSafe'),
(1,'S24U-001','Samsung Galaxy S24 Ultra','Samsung',32990000,35990000,'STK-S24U-001','Phantom Black','6.8 inch','/images/s24ultra.jpg','Camera zoom up to 100x'),
(1,'PX9P-001','Google Pixel 9 Pro','Google',25990000,27990000,'STK-PX9P-001','Seafoam','6.7 inch','/images/pixel9pro.jpg','AI camera'),

-- Laptops (CategoryId = 2)
(2,'MBP-16-2025','MacBook Pro 16 (M4)','Apple',64990000,69990000,'STK-MBP16-001','Space Gray','16 inch','/images/macbookpro16_m4.jpg','M4 chip, 16GB/1TB'),
(2,'XPS-15-2025','Dell XPS 15','Dell',42990000,46990000,'STK-XPS15-001','Silver','15.6 inch','/images/dell_xps15.jpg','Intel i9, 32GB RAM'),
(2,'RZ-17G-2025','Razer Blade 17','Razer',54990000,57990000,'STK-RZ17-001','Black','17 inch','/images/razerblade17.jpg','Gaming high-end, RTX'),
(2,'AS-GF-15','Asus ROG Flow','ASUS',38990000,41990000,'STK-ASGF15-001','Black','15.6 inch','/images/asus_rog.jpg','Gaming lightweight'),

-- Televisions (CategoryId = 3)
(3,'OLED55-2025','LG OLED 55" C-Series','LG',24990000,27990000,'STK-OLED55-001','Black','55 inch','/images/lg_oled55.jpg','OLED 4K, Smart TV'),
(3,'QLED65-2025','Samsung QLED 65"','Samsung',31990000,34990000,'STK-QLED65-001','Black','65 inch','/images/samsung_qled65.jpg','QLED 4K'),

-- Phone Accessories (CategoryId = 4)
(4,'CASE-001','Ốp lưng chống sốc (Universal)','AccessoryBrand',299000,399000,'STK-CASE-001','Black','Universal','/images/case001.jpg','Ốp lưng bảo vệ cho nhiều dòng'),
(4,'CHG-65W','Adapter sạc nhanh 65W','AccessoryBrand',399000,499000,'STK-CHG65W-001','White','Standard','/images/charger65w.jpg','Sạc PD 65W'),
(4,'CABLE-USBC-1M','Cáp USB-C 1m','AccessoryBrand',99000,129000,'STK-CABLE-001','White','1m','/images/usb_cable.jpg','Cáp sạc & dữ liệu'),
(4,'EAR-TRUE-WIRE','Tai nghe true wireless','AudioBrand',1990000,2490000,'STK-EARTW-001','White','In-ear','/images/earbuds1.jpg','ANC, Bluetooth 5.3'),

-- Computer Accessories (CategoryId = 5)
(5,'MOUSE-G502','Logitech G502 HERO','Logitech',1290000,1490000,'STK-MOUSE-001','Black','Standard','/images/logitech_g502.jpg','Gaming mouse, high DPI'),
(5,'KB-MECH-01','Bàn phím cơ RGB','KeyBrand',990000,1190000,'STK-KB01-001','Black','Full','/images/keyboard_mech.jpg','Hot-swap, RGB'),
(5,'SSD-1TB','SSD NVMe 1TB','StorageBrand',2399000,2799000,'STK-SSD1TB-001','','M.2 2280','/images/ssd_1tb.jpg','Fast NVMe storage'),

-- Audio & Wearables (CategoryId = 6)
(6,'BH-ANC1','Sony WH-1000XM5','Sony',6790000,7290000,'STK-BHANC1-001','Black','Over-ear','/images/sony_wh1000xm5.jpg','ANC, long battery'),
(6,'WATCH-5','Apple Watch Series 9','Apple',11990000,12990000,'STK-WATCH5-001','Silver','45mm','/images/apple_watch9.jpg','Health & fitness'),
(6,'SPEAKER-1','JBL Flip 6','JBL',1999000,2299000,'STK-SPK1-001','Blue','Portable','/images/jbl_flip6.jpg','Waterproof Bluetooth speaker'),

-- More Smartphones & Laptops to reach variety
(1,'XIAO-14-U','Xiaomi 14 Ultra','Xiaomi',22990000,24990000,'STK-XIAO14U-001','White','6.73 inch','/images/xiaomi14ultra.jpg','Leica camera'),
(1,'OP12-001','OnePlus 12','OnePlus',21990000,23990000,'STK-OP12-001','Black','6.82 inch','/images/oneplus12.jpg','Smooth OS'),
(2,'MB-13-2025','MacBook Air 13 (M4)','Apple',32990000,34990000,'STK-MBA13-001','Silver','13.6 inch','/images/macbookair13_m4.jpg','Lightweight M4'),
(2,'LENO-IDEA7','Lenovo IdeaPad 7','Lenovo',17990000,19990000,'STK-LENO7-001','Grey','15.6 inch','/images/lenovo_ideapad7.jpg','Efficient performance'),

-- Extra accessories
(4,'PROT-GLASS','Kính cường lực','AccessoryBrand',99000,129000,'STK-PROT-001','Transparent','Universal','/images/screen_protector.jpg','Tempered glass'),
(5,'EXT-HDD-2TB','HDD External 2TB','StorageBrand',1999000,2299000,'STK-HDD2TB-001','Black','2TB','/images/hdd_2tb.jpg','Portable HDD'),
(6,'EAR-SPORTS','Earbuds Sport','AudioBrand',499000,699000,'STK-EARSP-001','Black','In-ear','/images/earbuds_sport.jpg','Sweat resistant');
GO

/****************************************
 * Inventory data (sample stock quantities)
 ****************************************/

INSERT INTO dbo.Inventory (StockCode, ProductId, CurrentQuantity, MinimumQuantity, MaximumQuantity, Location)
SELECT 
    p.StockCode,
    p.ProductId,
    CASE 
        WHEN p.CategoryId = 1 THEN 15 -- Smartphones
        WHEN p.CategoryId = 2 THEN 8  -- Laptops  
        WHEN p.CategoryId = 3 THEN 5  -- TVs
        WHEN p.CategoryId = 4 THEN 100 -- Phone accessories
        WHEN p.CategoryId = 5 THEN 50 -- Computer accessories
        WHEN p.CategoryId = 6 THEN 25 -- Audio & wearables
        ELSE 10
    END,
    CASE 
        WHEN p.CategoryId IN (1,2) THEN 2  -- High value items
        ELSE 5
    END,
    CASE 
        WHEN p.CategoryId IN (1,2) THEN 50  -- High value items
        WHEN p.CategoryId IN (4,5) THEN 500 -- Accessories
        ELSE 100
    END,
    CASE 
        WHEN p.CategoryId = 1 THEN 'A1-Smartphones'
        WHEN p.CategoryId = 2 THEN 'A2-Laptops'
        WHEN p.CategoryId = 3 THEN 'B1-TVs'
        WHEN p.CategoryId = 4 THEN 'C1-Phone-Accessories'
        WHEN p.CategoryId = 5 THEN 'C2-Computer-Accessories'
        WHEN p.CategoryId = 6 THEN 'D1-Audio-Wearables'
        ELSE 'E1-General'
    END
FROM dbo.Products p;
GO

/****************************************
 * Sample configurations for laptops and phones
 ****************************************/

-- Laptop configurations
INSERT INTO dbo.LaptopConfigurations (ProductId, CPU, RAM, Storage, GraphicsCard, Battery, OperatingSystem, ScreenSize, ScreenTechnology, Resolution, Ports, Color, Weight)
VALUES
((SELECT ProductId FROM Products WHERE SKU = 'MBP-16-2025'), 'Apple M4 chip', '16GB Unified Memory', '1TB SSD', '16-core GPU', '100-watt-hour lithium-polymer', 'macOS Sonoma', '16.2 inch', 'Liquid Retina XDR', '3456 x 2234', 'Thunderbolt 4, HDMI, SDXC, MagSafe 3', 'Space Gray', '2.1 kg'),
((SELECT ProductId FROM Products WHERE SKU = 'XPS-15-2025'), 'Intel Core i9-14900H', '32GB DDR5', '1TB NVMe SSD', 'NVIDIA GeForce RTX 4070', '86Wh', 'Windows 11 Pro', '15.6 inch', 'OLED', '3840 x 2400', 'Thunderbolt 4, USB-C, HDMI, SD card', 'Silver', '1.8 kg'),
((SELECT ProductId FROM Products WHERE SKU = 'RZ-17G-2025'), 'Intel Core i9-14900HX', '32GB DDR5', '2TB NVMe SSD', 'NVIDIA GeForce RTX 4090', '82Wh', 'Windows 11 Home', '17.3 inch', 'IPS', '2560 x 1440', 'Thunderbolt 4, USB-C, HDMI, Ethernet', 'Black', '2.7 kg');

-- Phone configurations
INSERT INTO dbo.PhoneConfigurations (ProductId, CPU, Cores, Threads, RAM, InternalStorage, Battery, OperatingSystem, Screen, ScreenTechnology, Resolution, Camera, Ports, Color)
VALUES
((SELECT ProductId FROM Products WHERE SKU = 'IP16PM-001'), 'Apple A18 Pro', '6 cores', '6 threads', '8GB', '1TB', '4422 mAh', 'iOS 18', '6.7 inch', 'Super Retina XDR', '2796 x 1290', '48MP Main, 12MP Ultra Wide, 12MP Telephoto', 'USB-C, MagSafe', 'Titanium'),
((SELECT ProductId FROM Products WHERE SKU = 'S24U-001'), 'Snapdragon 8 Gen 3', '8 cores', '8 threads', '12GB', '512GB', '5000 mAh', 'Android 14', '6.8 inch', 'Dynamic AMOLED 2X', '3088 x 1440', '200MP Wide, 50MP Telephoto, 12MP Ultra Wide', 'USB-C', 'Phantom Black'),
((SELECT ProductId FROM Products WHERE SKU = 'PX9P-001'), 'Google Tensor G4', '8 cores', '8 threads', '12GB', '256GB', '4500 mAh', 'Android 14', '6.7 inch', 'LTPO OLED', '1344 x 2992', '50MP main, 48MP ultrawide, 48MP telephoto', 'USB-C', 'Seafoam');
GO

/****************************************
 * Sample import/export receipts
 ****************************************/

-- Sample import receipt
INSERT INTO dbo.ImportReceipts (ReceiptNumber, ImportDate, SupplierName, TotalQuantity, TotalValue, CreatedByUserId, Notes)
VALUES ('IMP-2024-001', '2024-01-15', 'Apple Vietnam', 50, 1250000000, 1, 'Initial stock import for new products');

INSERT INTO dbo.ImportReceiptDetails (ImportReceiptId, ProductId, StockCode, Quantity, UnitCost, TotalCost, BatchNumber)
VALUES 
(1, (SELECT ProductId FROM Products WHERE SKU = 'IP16PM-001'), 'STK-IP16PM-001', 15, 30000000, 450000000, 'BATCH-IP16-001'),
(1, (SELECT ProductId FROM Products WHERE SKU = 'IP16-001'), 'STK-IP16-001', 20, 22000000, 440000000, 'BATCH-IP16-002'),
(1, (SELECT ProductId FROM Products WHERE SKU = 'MBP-16-2025'), 'STK-MBP16-001', 8, 55000000, 440000000, 'BATCH-MBP-001'),
(1, (SELECT ProductId FROM Products WHERE SKU = 'MB-13-2025'), 'STK-MBA13-001', 7, 28000000, 196000000, 'BATCH-MBA-001');
GO

-- ProductImages
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

PRINT 'Extended setup completed successfully with ALL columns preserved!';
PRINT 'All product columns (Name, Brand, Price, OldPrice, ShortDescription) are intact.';
PRINT 'New inventory management system added.';
GO

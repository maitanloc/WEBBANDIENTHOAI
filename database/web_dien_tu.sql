/**********************************************
 Revised PhoneShopFull schema (Safe import/export + snapshots + protections)
 SQL Server 2016+
 Author: Mai Tấn Lộc
 **********************************************/

SET NOCOUNT ON;
GO

-- ========== CREATE DATABASE (nếu chưa có) ==========
IF DB_ID(N'PhoneShopFull') IS NULL
BEGIN
    CREATE DATABASE PhoneShopFull;
    PRINT 'Database PhoneShopFull created.';
END
GO

USE PhoneShopFull;
GO

-- ============================
-- Utility: nếu cần xóa kiểu bảng TVP trước
-- ============================
IF TYPE_ID(N'dbo.ImportItemType') IS NOT NULL
    DROP TYPE dbo.ImportItemType;
GO

IF TYPE_ID(N'dbo.ExportItemType') IS NOT NULL
    DROP TYPE dbo.ExportItemType;
GO

-- ============================
-- Drop tables (clean slate)
-- ============================
-- Note: drop order must respect FK dependencies
IF OBJECT_ID('dbo.ExportReceiptDetails','U') IS NOT NULL DROP TABLE dbo.ExportReceiptDetails;
IF OBJECT_ID('dbo.ExportReceipts','U') IS NOT NULL DROP TABLE dbo.ExportReceipts;
IF OBJECT_ID('dbo.ImportReceiptDetails','U') IS NOT NULL DROP TABLE dbo.ImportReceiptDetails;
IF OBJECT_ID('dbo.ImportReceipts','U') IS NOT NULL DROP TABLE dbo.ImportReceipts;
IF OBJECT_ID('dbo.InventoryHistory','U') IS NOT NULL DROP TABLE dbo.InventoryHistory;
IF OBJECT_ID('dbo.Inventory','U') IS NOT NULL DROP TABLE dbo.Inventory;
IF OBJECT_ID('dbo.ProductImages','U') IS NOT NULL DROP TABLE dbo.ProductImages;
IF OBJECT_ID('dbo.PhoneConfigurations','U') IS NOT NULL DROP TABLE dbo.PhoneConfigurations;
IF OBJECT_ID('dbo.LaptopConfigurations','U') IS NOT NULL DROP TABLE dbo.LaptopConfigurations;
IF OBJECT_ID('dbo.OrderDetails','U') IS NOT NULL DROP TABLE dbo.OrderDetails;
IF OBJECT_ID('dbo.Orders','U') IS NOT NULL DROP TABLE dbo.Orders;
IF OBJECT_ID('dbo.CartDetails','U') IS NOT NULL DROP TABLE dbo.CartDetails;
IF OBJECT_ID('dbo.Carts','U') IS NOT NULL DROP TABLE dbo.Carts;
IF OBJECT_ID('dbo.ProductImages','U') IS NOT NULL DROP TABLE dbo.ProductImages;
IF OBJECT_ID('dbo.ProductStatuses','U') IS NOT NULL DROP TABLE dbo.ProductStatuses;
IF OBJECT_ID('dbo.Products','U') IS NOT NULL DROP TABLE dbo.Products;
IF OBJECT_ID('dbo.Categories','U') IS NOT NULL DROP TABLE dbo.Categories;
IF OBJECT_ID('dbo.Customers','U') IS NOT NULL DROP TABLE dbo.Customers;
IF OBJECT_ID('dbo.Users','U') IS NOT NULL DROP TABLE dbo.Users;
IF OBJECT_ID('dbo.Roles','U') IS NOT NULL DROP TABLE dbo.Roles;
IF OBJECT_ID('dbo.AuditLogs','U') IS NOT NULL DROP TABLE dbo.AuditLogs;
GO

-- ============================
-- Create tables
-- ============================

-- Roles (RoleId: Mã quyền, RoleName: Tên quyền)
CREATE TABLE dbo.Roles
(
    RoleId INT IDENTITY(1,1) PRIMARY KEY, -- (RoleId: Mã quyền)
    RoleName NVARCHAR(50) NOT NULL UNIQUE, -- (RoleName: Tên quyền: Admin/Staff/Customer)
    Description NVARCHAR(250) NULL -- (Description: Mô tả)
);
GO

-- Users (UserId: nhân viên/admin)
CREATE TABLE dbo.Users
(
    UserId INT IDENTITY(1,1) PRIMARY KEY, -- (UserId: Mã user)
    Username NVARCHAR(100) NOT NULL UNIQUE, -- (Username: Tên đăng nhập)
    PasswordHash VARBINARY(64) NOT NULL, -- (PasswordHash: Hash mật khẩu SHA2_256)
    FullName NVARCHAR(150) NULL, -- (FullName: Tên đầy đủ)
    Email NVARCHAR(150) NULL, -- (Email)
    RoleId INT NOT NULL, -- (RoleId: FK tới Roles)
    IsActive BIT NOT NULL DEFAULT 1, -- (IsActive: Có hoạt động)
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(), -- (CreatedAt: Thời gian tạo)
    CONSTRAINT FK_Users_Roles FOREIGN KEY(RoleId) REFERENCES dbo.Roles(RoleId)
);
GO

-- Customers (CustomerId: khách hàng)
CREATE TABLE dbo.Customers
(
    CustomerId INT IDENTITY(1,1) PRIMARY KEY, -- (CustomerId: Mã khách hàng)
    FullName NVARCHAR(150) NOT NULL, -- (FullName: Tên khách)
    Email NVARCHAR(150) NOT NULL UNIQUE, -- (Email)
    PasswordHash VARBINARY(64) NOT NULL, -- (PasswordHash)
    Phone NVARCHAR(30) NULL, -- (Phone: SĐT)
    Address NVARCHAR(300) NULL, -- (Address: Địa chỉ)
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(), -- (CreatedAt)
    IsActive BIT NOT NULL DEFAULT 1 -- (IsActive)
);
GO

-- Categories (CategoryId: danh mục sản phẩm)
CREATE TABLE dbo.Categories
(
    CategoryId INT IDENTITY(1,1) PRIMARY KEY, -- (CategoryId: Mã danh mục)
    CategoryName NVARCHAR(120) NOT NULL, -- (CategoryName: Tên danh mục)
    Description NVARCHAR(500) NULL, -- (Description: Mô tả)
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME() -- (CreatedAt)
);
GO

-- ProductStatuses (StatusId: trạng thái sản phẩm)
CREATE TABLE dbo.ProductStatuses
(
    StatusId TINYINT IDENTITY(1,1) PRIMARY KEY, -- (StatusId: Mã trạng thái)
    StatusName NVARCHAR(30) NOT NULL UNIQUE, -- (StatusName: Tên trạng thái)
    Description NVARCHAR(150) NULL -- (Description)
);
GO

-- Products (ProductId: danh mục sản phẩm hiện hành)
CREATE TABLE dbo.Products
(
    ProductId INT IDENTITY(1,1) PRIMARY KEY, -- (ProductId: Mã sản phẩm)
    CategoryId INT NOT NULL, -- (CategoryId: FK danh mục)
    SKU NVARCHAR(60) NOT NULL UNIQUE, -- (SKU: Mã hàng / mã SKU - KHÔNG NÊN THAY ĐỔI nếu đã có tồn kho)
    Name NVARCHAR(250) NOT NULL, -- (Name: Tên sản phẩm)
    Brand NVARCHAR(100) NULL, -- (Brand: Thương hiệu)
    Price DECIMAL(18,2) NOT NULL DEFAULT 0, -- (Price: Giá hiện tại bán)
    OldPrice DECIMAL(18,2) NULL, -- (OldPrice: Giá cũ)
    StockCode NVARCHAR(50) NOT NULL, -- (StockCode: Mã tồn kho - KHÔNG NÊN THAY ĐỔI nếu đã có tồn kho)
    Color NVARCHAR(100) NULL, -- (Color: Màu)
    Size NVARCHAR(100) NULL, -- (Size: Kích thước)
    DefaultImage NVARCHAR(300) NULL, -- (DefaultImage: Đường dẫn ảnh chính)
    ShortDescription NVARCHAR(1000) NULL, -- (ShortDescription: Mô tả ngắn)
    StatusId TINYINT NOT NULL DEFAULT 1, -- (StatusId: FK trạng thái)
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(), -- (CreatedAt)
    CONSTRAINT FK_Products_Categories FOREIGN KEY(CategoryId) REFERENCES dbo.Categories(CategoryId),
    CONSTRAINT FK_Products_Status FOREIGN KEY(StatusId) REFERENCES dbo.ProductStatuses(StatusId)
);
GO

CREATE INDEX IX_Products_Brand ON dbo.Products(Brand);
CREATE INDEX IX_Products_Price ON dbo.Products(Price);
CREATE INDEX IX_Products_StockCode ON dbo.Products(StockCode);
GO

-- ProductImages (ảnh nhiều cho 1 sản phẩm)
CREATE TABLE dbo.ProductImages
(
    ImageId INT IDENTITY(1,1) PRIMARY KEY, -- (ImageId: Mã ảnh)
    ProductId INT NOT NULL, -- (ProductId: FK sang Products)
    ImagePath NVARCHAR(300) NOT NULL, -- (ImagePath: Đường dẫn ảnh)
    IsPrimary BIT NOT NULL DEFAULT 0, -- (IsPrimary: 1 nếu ảnh chính)
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(), -- (CreatedAt)
    CONSTRAINT FK_ProductImages_Products FOREIGN KEY(ProductId) REFERENCES dbo.Products(ProductId) ON DELETE CASCADE
);
GO

-- Inventory (quản lý tồn kho cho từng StockCode)
CREATE TABLE dbo.Inventory
(
    InventoryId INT IDENTITY(1,1) PRIMARY KEY, -- (InventoryId: Mã tồn kho)
    StockCode NVARCHAR(50) NOT NULL UNIQUE, -- (StockCode: Mã tồn kho - unique)
    ProductId INT NOT NULL, -- (ProductId: FK tới sản phẩm)
    CurrentQuantity INT NOT NULL DEFAULT 0, -- (CurrentQuantity: Số lượng hiện tại)
    MinimumQuantity INT NOT NULL DEFAULT 0, -- (MinimumQuantity: Ngưỡng cảnh báo)
    MaximumQuantity INT NOT NULL DEFAULT 1000, -- (MaximumQuantity: Sức chứa tối đa)
    Location NVARCHAR(100) NULL, -- (Location: Vị trí kho)
    LastUpdated DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(), -- (LastUpdated)
    CONSTRAINT FK_Inventory_Products FOREIGN KEY(ProductId) REFERENCES dbo.Products(ProductId)
);
GO

CREATE INDEX IX_Inventory_ProductId ON dbo.Inventory(ProductId);
CREATE INDEX IX_Inventory_StockCode ON dbo.Inventory(StockCode);
GO

-- InventoryHistory (ghi lại mọi thay đổi tồn kho để audit)
CREATE TABLE dbo.InventoryHistory
(
    HistoryId INT IDENTITY(1,1) PRIMARY KEY, -- (HistoryId: Mã lịch sử)
    InventoryId INT NOT NULL, -- (InventoryId: FK inventory)
    ChangeQuantity INT NOT NULL, -- (ChangeQuantity: Số lượng thay đổi (+/-))
    Operation NVARCHAR(50) NOT NULL, -- (Operation: 'IMPORT' | 'EXPORT' | 'ADJUST' | 'TRANSFER')
    ReferenceId INT NULL, -- (ReferenceId: Id liên quan, như ImportReceiptId/ExportReceiptId)
    Note NVARCHAR(500) NULL, -- (Note: Ghi chú)
    CreatedByUserId INT NULL, -- (CreatedByUserId: Người thực hiện)
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(), -- (CreatedAt)
    CONSTRAINT FK_InventoryHistory_Inventory FOREIGN KEY(InventoryId) REFERENCES dbo.Inventory(InventoryId)
);
GO

-- ImportReceipts (phiếu nhập kho) - lưu summary phiếu
CREATE TABLE dbo.ImportReceipts
(
    ImportReceiptId INT IDENTITY(1,1) PRIMARY KEY, -- (ImportReceiptId: Mã phiếu nhập)
    ReceiptNumber NVARCHAR(50) NOT NULL UNIQUE, -- (ReceiptNumber: Số phiếu)
    ImportDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(), -- (ImportDate: Ngày nhập)
    SupplierName NVARCHAR(200) NULL, -- (SupplierName: Tên nhà cung cấp)
    TotalQuantity INT NOT NULL DEFAULT 0, -- (TotalQuantity: Tổng số lượng)
    TotalValue DECIMAL(18,2) NOT NULL DEFAULT 0, -- (TotalValue: Tổng giá trị tiền)
    CreatedByUserId INT NOT NULL, -- (CreatedByUserId: Người tạo phiếu)
    Notes NVARCHAR(500) NULL, -- (Notes: Ghi chú)
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(), -- (CreatedAt)
    CONSTRAINT FK_ImportReceipts_Users FOREIGN KEY(CreatedByUserId) REFERENCES dbo.Users(UserId)
);
GO

-- ImportReceiptDetails (chi tiết phiếu nhập)
-- LƯU Ý: lưu cả snapshot (tĩnh) thông tin sản phẩm tại thời điểm nhập
CREATE TABLE dbo.ImportReceiptDetails
(
    ImportDetailId INT IDENTITY(1,1) PRIMARY KEY, -- (ImportDetailId: Mã chi tiết nhập)
    ImportReceiptId INT NOT NULL, -- (ImportReceiptId: FK sang ImportReceipts)
    ProductId INT NULL, -- (ProductId: FK sản phẩm nếu có mapping)
    SnapshotSKU NVARCHAR(60) NOT NULL, -- (SnapshotSKU: SKU khi import - snapshot)
    SnapshotName NVARCHAR(250) NULL, -- (SnapshotName: Tên khi import - snapshot)
    SnapshotBrand NVARCHAR(100) NULL, -- (SnapshotBrand: Thương hiệu snapshot)
    StockCode NVARCHAR(50) NOT NULL, -- (StockCode: Mã tồn kho dòng này)
    Quantity INT NOT NULL DEFAULT 0, -- (Quantity: Số lượng nhập)
    UnitCost DECIMAL(18,2) NOT NULL DEFAULT 0, -- (UnitCost: Giá nhập 1 đơn vị)
    TotalCost DECIMAL(18,2) NOT NULL DEFAULT 0, -- (TotalCost: Thành tiền)
    BatchNumber NVARCHAR(100) NULL, -- (BatchNumber: Số lô)
    ExpiryDate DATE NULL, -- (ExpiryDate: Hạn dùng)
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(), -- (CreatedAt)
    CONSTRAINT FK_ImportDetails_Receipts FOREIGN KEY(ImportReceiptId) REFERENCES dbo.ImportReceipts(ImportReceiptId) ON DELETE CASCADE,
    CONSTRAINT FK_ImportDetails_Products FOREIGN KEY(ProductId) REFERENCES dbo.Products(ProductId)
);
GO

CREATE INDEX IX_ImportDetails_ProductId ON dbo.ImportReceiptDetails(ProductId);
GO

-- ExportReceipts (phiếu xuất kho / bán hàng)
CREATE TABLE dbo.ExportReceipts
(
    ExportReceiptId INT IDENTITY(1,1) PRIMARY KEY, -- (ExportReceiptId: Mã phiếu xuất)
    ReceiptNumber NVARCHAR(50) NOT NULL UNIQUE, -- (ReceiptNumber: Số phiếu xuất)
    ExportDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(), -- (ExportDate)
    CustomerId INT NULL, -- (CustomerId: Khách hàng nếu xuất bán)
    OrderId INT NULL, -- (OrderId: Liên kết đơn hàng nếu có)
    TotalQuantity INT NOT NULL DEFAULT 0, -- (TotalQuantity)
    TotalValue DECIMAL(18,2) NOT NULL DEFAULT 0, -- (TotalValue)
    CreatedByUserId INT NOT NULL, -- (CreatedByUserId)
    Notes NVARCHAR(500) NULL, -- (Notes)
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(), -- (CreatedAt)
    CONSTRAINT FK_ExportReceipts_Customers FOREIGN KEY(CustomerId) REFERENCES dbo.Customers(CustomerId),
    CONSTRAINT FK_ExportReceipts_Orders FOREIGN KEY(OrderId) REFERENCES dbo.Orders(OrderId),
    CONSTRAINT FK_ExportReceipts_Users FOREIGN KEY(CreatedByUserId) REFERENCES dbo.Users(UserId)
);
GO

-- ExportReceiptDetails (chi tiết phiếu xuất) - Lưu snapshot
CREATE TABLE dbo.ExportReceiptDetails
(
    ExportDetailId INT IDENTITY(1,1) PRIMARY KEY, -- (ExportDetailId: Mã chi tiết xuất)
    ExportReceiptId INT NOT NULL, -- (ExportReceiptId: FK sang ExportReceipts)
    ProductId INT NULL, -- (ProductId: FK sản phẩm nếu có mapping)
    SnapshotSKU NVARCHAR(60) NOT NULL, -- (SnapshotSKU: SKU snapshot)
    SnapshotName NVARCHAR(250) NULL, -- (SnapshotName)
    SnapshotBrand NVARCHAR(100) NULL, -- (SnapshotBrand)
    StockCode NVARCHAR(50) NOT NULL, -- (StockCode: Mã tồn kho)
    Quantity INT NOT NULL DEFAULT 0, -- (Quantity: Số lượng xuất)
    UnitPrice DECIMAL(18,2) NOT NULL DEFAULT 0, -- (UnitPrice: Giá bán 1 đơn vị)
    TotalPrice DECIMAL(18,2) NOT NULL DEFAULT 0, -- (TotalPrice)
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(), -- (CreatedAt)
    CONSTRAINT FK_ExportDetails_Receipts FOREIGN KEY(ExportReceiptId) REFERENCES dbo.ExportReceipts(ExportReceiptId) ON DELETE CASCADE,
    CONSTRAINT FK_ExportDetails_Products FOREIGN KEY(ProductId) REFERENCES dbo.Products(ProductId)
);
GO

CREATE INDEX IX_ExportDetails_ProductId ON dbo.ExportReceiptDetails(ProductId);
GO

-- Orders & OrderDetails (đơn bán hàng)
CREATE TABLE dbo.Orders
(
    OrderId INT IDENTITY(1,1) PRIMARY KEY, -- (OrderId: Mã đơn)
    CustomerId INT NOT NULL, -- (CustomerId: Khách hàng)
    OrderDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(), -- (OrderDate)
    Total DECIMAL(18,2) NOT NULL DEFAULT 0, -- (Total: Tổng tiền đơn)
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending', -- (Status: Trạng thái)
    ShippingAddress NVARCHAR(300) NULL, -- (ShippingAddress)
    CreatedByUserId INT NULL, -- (CreatedByUserId)
    CONSTRAINT FK_Orders_Customers FOREIGN KEY(CustomerId) REFERENCES dbo.Customers(CustomerId),
    CONSTRAINT FK_Orders_Users FOREIGN KEY(CreatedByUserId) REFERENCES dbo.Users(UserId)
);
GO

CREATE TABLE dbo.OrderDetails
(
    OrderDetailId INT IDENTITY(1,1) PRIMARY KEY, -- (OrderDetailId: Mã chi tiết đơn)
    OrderId INT NOT NULL, -- (OrderId)
    ProductId INT NOT NULL, -- (ProductId)
    Quantity INT NOT NULL, -- (Quantity)
    UnitPrice DECIMAL(18,2) NOT NULL, -- (UnitPrice)
    CONSTRAINT FK_OrderDetails_Orders FOREIGN KEY(OrderId) REFERENCES dbo.Orders(OrderId) ON DELETE CASCADE,
    CONSTRAINT FK_OrderDetails_Products FOREIGN KEY(ProductId) REFERENCES dbo.Products(ProductId)
);
GO

-- Carts & CartDetails (giỏ hàng tạm)
CREATE TABLE dbo.Carts
(
    CartId INT IDENTITY(1,1) PRIMARY KEY, -- (CartId)
    CustomerId INT NOT NULL, -- (CustomerId)
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(), -- (CreatedAt)
    UpdatedAt DATETIME2 NULL, -- (UpdatedAt)
    CONSTRAINT FK_Carts_Customers FOREIGN KEY(CustomerId) REFERENCES dbo.Customers(CustomerId)
);
GO

CREATE TABLE dbo.CartDetails
(
    CartDetailId INT IDENTITY(1,1) PRIMARY KEY, -- (CartDetailId)
    CartId INT NOT NULL, -- (CartId)
    ProductId INT NOT NULL, -- (ProductId)
    Quantity INT NOT NULL DEFAULT 1, -- (Quantity)
    UnitPrice DECIMAL(18,2) NOT NULL, -- (UnitPrice)
    CONSTRAINT FK_CartDetails_Carts FOREIGN KEY(CartId) REFERENCES dbo.Carts(CartId) ON DELETE CASCADE,
    CONSTRAINT FK_CartDetails_Products FOREIGN KEY(ProductId) REFERENCES dbo.Products(ProductId)
);
GO

-- LaptopConfigurations (chi tiết cấu hình laptop)
CREATE TABLE dbo.LaptopConfigurations
(
    ConfigurationId INT IDENTITY(1,1) PRIMARY KEY, -- (ConfigurationId)
    ProductId INT NOT NULL UNIQUE, -- (ProductId)
    CPU NVARCHAR(200) NULL, -- (CPU)
    RAM NVARCHAR(100) NULL, -- (RAM)
    Storage NVARCHAR(200) NULL, -- (Storage)
    GraphicsCard NVARCHAR(200) NULL, -- (GraphicsCard)
    Battery NVARCHAR(100) NULL, -- (Battery)
    OperatingSystem NVARCHAR(100) NULL, -- (OS)
    ScreenSize NVARCHAR(50) NULL, -- (ScreenSize)
    ScreenTechnology NVARCHAR(100) NULL, -- (ScreenTechnology)
    Resolution NVARCHAR(100) NULL, -- (Resolution)
    Ports NVARCHAR(500) NULL, -- (Ports)
    Color NVARCHAR(50) NULL, -- (Color)
    Weight NVARCHAR(50) NULL, -- (Weight)
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(), -- (CreatedAt)
    CONSTRAINT FK_LaptopConfigs_Products FOREIGN KEY(ProductId) REFERENCES dbo.Products(ProductId)
);
GO

-- PhoneConfigurations (chi tiết cấu hình điện thoại)
CREATE TABLE dbo.PhoneConfigurations
(
    ConfigurationId INT IDENTITY(1,1) PRIMARY KEY, -- (ConfigurationId)
    ProductId INT NOT NULL UNIQUE, -- (ProductId)
    CPU NVARCHAR(200) NULL, -- (CPU)
    Cores NVARCHAR(50) NULL, -- (Cores)
    Threads NVARCHAR(50) NULL, -- (Threads)
    RAM NVARCHAR(100) NULL, -- (RAM)
    InternalStorage NVARCHAR(100) NULL, -- (InternalStorage)
    Battery NVARCHAR(100) NULL, -- (Battery)
    OperatingSystem NVARCHAR(100) NULL, -- (OS)
    Screen NVARCHAR(200) NULL, -- (Screen)
    ScreenTechnology NVARCHAR(100) NULL, -- (ScreenTechnology)
    Resolution NVARCHAR(100) NULL, -- (Resolution)
    Camera NVARCHAR(500) NULL, -- (Camera)
    Ports NVARCHAR(500) NULL, -- (Ports)
    Color NVARCHAR(50) NULL, -- (Color)
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(), -- (CreatedAt)
    CONSTRAINT FK_PhoneConfigs_Products FOREIGN KEY(ProductId) REFERENCES dbo.Products(ProductId)
);
GO

-- ProductImages table already created earlier (kept)
-- AuditLogs (lưu action, phục vụ debug / audit)
CREATE TABLE dbo.AuditLogs
(
    LogId INT IDENTITY(1,1) PRIMARY KEY, -- (LogId)
    LogTime DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(), -- (LogTime)
    Username NVARCHAR(150) NULL, -- (Username)
    Action NVARCHAR(250) NULL, -- (Action: Hành động)
    Details NVARCHAR(MAX) NULL -- (Details: Chi tiết)
);
GO

-- ============================
-- User-defined table types (TVP) cho stored proc import/export
-- ============================
CREATE TYPE dbo.ImportItemType AS TABLE
(
    ProductId INT NULL, -- nếu biết mapping -> ProductId
    StockCode NVARCHAR(50) NOT NULL,
    SKU NVARCHAR(60) NULL,
    Name NVARCHAR(250) NULL,
    Quantity INT NOT NULL,
    UnitCost DECIMAL(18,2) NOT NULL,
    BatchNumber NVARCHAR(100) NULL,
    ExpiryDate DATE NULL
);
GO

CREATE TYPE dbo.ExportItemType AS TABLE
(
    ProductId INT NULL,
    StockCode NVARCHAR(50) NOT NULL,
    SKU NVARCHAR(60) NULL,
    Name NVARCHAR(250) NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL
);
GO

-- ============================
-- Triggers: bảo vệ khi chỉnh SKU / StockCode (Phương án A)
-- Nếu cố gắng UPDATE SKU hoặc StockCode mà sản phẩm có tồn kho > 0 hoặc đã có import/export detail -> rollback
-- ============================
IF OBJECT_ID('dbo.trg_Products_PreventKeyChange','TR') IS NOT NULL
    DROP TRIGGER dbo.trg_Products_PreventKeyChange;
GO

CREATE TRIGGER dbo.trg_Products_PreventKeyChange
ON dbo.Products
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    -- chỉ quan tâm khi SKU hoặc StockCode bị thay đổi
    IF NOT (UPDATE(SKU) OR UPDATE(StockCode))
        RETURN;

    -- tìm các dòng đã thay đổi
    IF EXISTS (
        SELECT 1
        FROM deleted d
        JOIN inserted i ON d.ProductId = i.ProductId
        WHERE (ISNULL(d.SKU,'') <> ISNULL(i.SKU,''))
           OR (ISNULL(d.StockCode,'') <> ISNULL(i.StockCode,''))
    )
    BEGIN
        -- kiểm tra tồn kho > 0
        IF EXISTS (
            SELECT 1
            FROM deleted d
            JOIN dbo.Inventory inv ON inv.ProductId = d.ProductId
            WHERE inv.CurrentQuantity > 0
        )
        BEGIN
            RAISERROR('Không thể đổi SKU/StockCode: Sản phẩm đang có tồn kho. Nếu cần đổi mã, hãy tạo sản phẩm mới hoặc tiến hành chuyển kho có kiểm duyệt.',16,1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- kiểm tra lịch sử (đã có phiếu nhập/xuất tham chiếu)
        IF EXISTS (
            SELECT 1 FROM deleted d
            WHERE EXISTS (SELECT 1 FROM dbo.ImportReceiptDetails id WHERE id.ProductId = d.ProductId)
               OR EXISTS (SELECT 1 FROM dbo.ExportReceiptDetails ed WHERE ed.ProductId = d.ProductId)
        )
        BEGIN
            RAISERROR('Không thể đổi SKU/StockCode: Sản phẩm đã có lịch sử nhập/xuất. Lịch sử phải được giữ nguyên (immutable).',16,1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
    END
END;
GO

-- Trigger: ngăn xóa sản phẩm nếu còn tồn kho hoặc có lịch sử
IF OBJECT_ID('dbo.trg_Products_PreventDelete','TR') IS NOT NULL
    DROP TRIGGER dbo.trg_Products_PreventDelete;
GO

CREATE TRIGGER dbo.trg_Products_PreventDelete
ON dbo.Products
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1 FROM deleted d
        WHERE EXISTS (SELECT 1 FROM dbo.Inventory inv WHERE inv.ProductId = d.ProductId AND inv.CurrentQuantity > 0)
           OR EXISTS (SELECT 1 FROM dbo.ImportReceiptDetails id WHERE id.ProductId = d.ProductId)
           OR EXISTS (SELECT 1 FROM dbo.ExportReceiptDetails ed WHERE ed.ProductId = d.ProductId)
    )
    BEGIN
        RAISERROR('Không thể xóa sản phẩm: còn tồn kho hoặc đã có lịch sử nhập/xuất. Vui lòng điều chỉnh bằng cách tạo sản phẩm mới hoặc xử lý tồn kho trước.',16,1);
        RETURN;
    END

    -- nếu an toàn -> thực hiện xóa
    DELETE FROM dbo.Products WHERE ProductId IN (SELECT ProductId FROM deleted);
END;
GO

-- ============================
-- Stored procedure: tạo ImportReceipt an toàn (transactional)
-- Parameters:
-- @ReceiptNumber, @SupplierName, @CreatedByUserId, @Notes, @Items TVP (ImportItemType)
-- Hành vi:
--  - Tạo ImportReceipt
--  - Với mỗi item: Insert ImportReceiptDetails (với snapshot)
--  - Upsert Inventory (nếu chưa có thì insert)
--  - Ghi InventoryHistory
-- ============================
IF OBJECT_ID('dbo.usp_CreateImportReceipt','P') IS NOT NULL
    DROP PROCEDURE dbo.usp_CreateImportReceipt;
GO

CREATE PROCEDURE dbo.usp_CreateImportReceipt
    @ReceiptNumber NVARCHAR(50),
    @SupplierName NVARCHAR(200) = NULL,
    @CreatedByUserId INT,
    @Notes NVARCHAR(500) = NULL,
    @Items dbo.ImportItemType READONLY
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRAN;

        -- create receipt
        INSERT INTO dbo.ImportReceipts (ReceiptNumber, SupplierName, TotalQuantity, TotalValue, CreatedByUserId, Notes, CreatedAt)
        VALUES (@ReceiptNumber, @SupplierName, 0, 0, @CreatedByUserId, @Notes, SYSUTCDATETIME());

        DECLARE @ImportReceiptId INT = SCOPE_IDENTITY();

        -- accumulate totals
        DECLARE @TotalQty INT = 0;
        DECLARE @TotalVal DECIMAL(18,2) = 0;

        -- process items
        INSERT INTO dbo.ImportReceiptDetails
        (
            ImportReceiptId, ProductId,
            SnapshotSKU, SnapshotName, SnapshotBrand,
            StockCode, Quantity, UnitCost, TotalCost, BatchNumber, ExpiryDate, CreatedAt
        )
        SELECT
            @ImportReceiptId,
            i.ProductId,
            COALESCE(i.SKU, ''), -- snapshot SKU
            COALESCE(i.Name, ''), -- snapshot name
            NULL, -- SnapshotBrand (can be filled by app before calling proc)
            i.StockCode,
            i.Quantity,
            i.UnitCost,
            i.Quantity * i.UnitCost,
            i.BatchNumber,
            i.ExpiryDate,
            SYSUTCDATETIME()
        FROM @Items i;

        -- update totals from details
        SELECT @TotalQty = SUM(Quantity), @TotalVal = SUM(TotalCost)
        FROM dbo.ImportReceiptDetails
        WHERE ImportReceiptId = @ImportReceiptId;

        UPDATE dbo.ImportReceipts
        SET TotalQuantity = ISNULL(@TotalQty,0), TotalValue = ISNULL(@TotalVal,0)
        WHERE ImportReceiptId = @ImportReceiptId;

        -- For each detail, upsert inventory and write history
        DECLARE cur CURSOR FOR
            SELECT ImportDetailId, ProductId, StockCode, Quantity, UnitCost
            FROM dbo.ImportReceiptDetails
            WHERE ImportReceiptId = @ImportReceiptId;

        DECLARE @ImportDetailId INT, @ProductId INT, @StockCode NVARCHAR(50), @Qty INT, @UnitCost DECIMAL(18,2);

        OPEN cur;
        FETCH NEXT FROM cur INTO @ImportDetailId, @ProductId, @StockCode, @Qty, @UnitCost;
        WHILE @@FETCH_STATUS = 0
        BEGIN
            -- try update existing inventory by StockCode
            IF EXISTS (SELECT 1 FROM dbo.Inventory WHERE StockCode = @StockCode)
            BEGIN
                UPDATE dbo.Inventory
                SET CurrentQuantity = CurrentQuantity + @Qty,
                    LastUpdated = SYSUTCDATETIME()
                WHERE StockCode = @StockCode;

                -- log history
                INSERT INTO dbo.InventoryHistory (InventoryId, ChangeQuantity, Operation, ReferenceId, Note, CreatedByUserId, CreatedAt)
                SELECT InventoryId, @Qty, 'IMPORT', @ImportReceiptId, CONCAT('ImportDetailId=', @ImportDetailId), @CreatedByUserId, SYSUTCDATETIME()
                FROM dbo.Inventory WHERE StockCode = @StockCode;
            END
            ELSE
            BEGIN
                -- create new inventory row (link to product if provided)
                INSERT INTO dbo.Inventory (StockCode, ProductId, CurrentQuantity, MinimumQuantity, MaximumQuantity, Location, LastUpdated)
                VALUES (@StockCode, @ProductId, @Qty, 0, 1000, NULL, SYSUTCDATETIME());

                DECLARE @NewInvId INT = SCOPE_IDENTITY();
                INSERT INTO dbo.InventoryHistory (InventoryId, ChangeQuantity, Operation, ReferenceId, Note, CreatedByUserId, CreatedAt)
                VALUES (@NewInvId, @Qty, 'IMPORT', @ImportReceiptId, CONCAT('Created by import detail ', @ImportDetailId), @CreatedByUserId, SYSUTCDATETIME());
            END

            FETCH NEXT FROM cur INTO @ImportDetailId, @ProductId, @StockCode, @Qty, @UnitCost;
        END
        CLOSE cur;
        DEALLOCATE cur;

        COMMIT TRAN;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRAN;
        DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR('usp_CreateImportReceipt failed: %s',16,1,@ErrMsg);
        RETURN;
    END CATCH
END
GO

-- ============================
-- Stored procedure: tạo ExportReceipt (giảm tồn) (transactional)
-- - Kiểm tra tồn kho đủ trước khi trừ
-- ============================
IF OBJECT_ID('dbo.usp_CreateExportReceipt','P') IS NOT NULL
    DROP PROCEDURE dbo.usp_CreateExportReceipt;
GO

CREATE PROCEDURE dbo.usp_CreateExportReceipt
    @ReceiptNumber NVARCHAR(50),
    @CustomerId INT = NULL,
    @OrderId INT = NULL,
    @CreatedByUserId INT,
    @Notes NVARCHAR(500) = NULL,
    @Items dbo.ExportItemType READONLY
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRAN;

        -- Check availability for every item first
        IF EXISTS (
            SELECT 1
            FROM @Items i
            LEFT JOIN dbo.Inventory inv ON inv.StockCode = i.StockCode
            WHERE inv.CurrentQuantity IS NULL OR inv.CurrentQuantity < i.Quantity
        )
        BEGIN
            RAISERROR('Không đủ tồn kho cho ít nhất 1 sản phẩm trong danh sách xuất.',16,1);
            ROLLBACK TRAN;
            RETURN;
        END

        -- create export receipt
        INSERT INTO dbo.ExportReceipts (ReceiptNumber, CustomerId, OrderId, TotalQuantity, TotalValue, CreatedByUserId, Notes, CreatedAt)
        VALUES (@ReceiptNumber, @CustomerId, @OrderId, 0, 0, @CreatedByUserId, @Notes, SYSUTCDATETIME());

        DECLARE @ExportReceiptId INT = SCOPE_IDENTITY();

        -- insert export details (snapshot)
        INSERT INTO dbo.ExportReceiptDetails
        (
            ExportReceiptId, ProductId,
            SnapshotSKU, SnapshotName, SnapshotBrand,
            StockCode, Quantity, UnitPrice, TotalPrice, CreatedAt
        )
        SELECT
            @ExportReceiptId,
            i.ProductId,
            COALESCE(i.SKU,''), -- snapshot sku
            COALESCE(i.Name,''), -- snapshot name
            NULL, -- snapshot brand (app có thể điền)
            i.StockCode,
            i.Quantity,
            i.UnitPrice,
            i.Quantity * i.UnitPrice,
            SYSUTCDATETIME()
        FROM @Items i;

        -- update totals
        DECLARE @TotalQty INT = 0;
        DECLARE @TotalVal DECIMAL(18,2) = 0;
        SELECT @TotalQty = SUM(Quantity), @TotalVal = SUM(TotalPrice)
        FROM dbo.ExportReceiptDetails WHERE ExportReceiptId = @ExportReceiptId;

        UPDATE dbo.ExportReceipts SET TotalQuantity = ISNULL(@TotalQty,0), TotalValue = ISNULL(@TotalVal,0)
        WHERE ExportReceiptId = @ExportReceiptId;

        -- Deduct inventory & log history
        DECLARE cur2 CURSOR FOR
            SELECT ExportDetailId, StockCode, Quantity
            FROM dbo.ExportReceiptDetails
            WHERE ExportReceiptId = @ExportReceiptId;

        DECLARE @ExportDetailId INT, @StockCode NVARCHAR(50), @Qty INT;

        OPEN cur2;
        FETCH NEXT FROM cur2 INTO @ExportDetailId, @StockCode, @Qty;
        WHILE @@FETCH_STATUS = 0
        BEGIN
            -- reduce inventory
            UPDATE dbo.Inventory
            SET CurrentQuantity = CurrentQuantity - @Qty,
                LastUpdated = SYSUTCDATETIME()
            WHERE StockCode = @StockCode;

            -- log history
            INSERT INTO dbo.InventoryHistory (InventoryId, ChangeQuantity, Operation, ReferenceId, Note, CreatedByUserId, CreatedAt)
            SELECT InventoryId, -@Qty, 'EXPORT', @ExportReceiptId, CONCAT('ExportDetailId=', @ExportDetailId), @CreatedByUserId, SYSUTCDATETIME()
            FROM dbo.Inventory WHERE StockCode = @StockCode;

            FETCH NEXT FROM cur2 INTO @ExportDetailId, @StockCode, @Qty;
        END
        CLOSE cur2;
        DEALLOCATE cur2;

        COMMIT TRAN;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRAN;
        DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR('usp_CreateExportReceipt failed: %s',16,1,@ErrMsg);
        RETURN;
    END CATCH
END
GO

-- ============================
-- Seed initial data (roles, users, statuses, categories, some products + inventory)
-- (sample data; bạn có thể sửa/extend sau)
-- ============================
-- Roles
INSERT INTO dbo.Roles (RoleName, Description) VALUES
('Admin','Quản trị hệ thống'), -- (Admin)
('Staff','Nhân viên cửa hàng'), -- (Staff)
('Customer','Khách hàng'); -- (Customer)
GO

-- ProductStatuses
INSERT INTO dbo.ProductStatuses (StatusName, Description) VALUES
('InStock','Còn hàng'),
('OutOfStock','Hết hàng'),
('PreOrder','Đặt trước'),
('Discontinued','Ngừng kinh doanh');
GO

-- Basic users (passwords are hashed via HASHBYTES('SHA2_256','plain') for demo)
INSERT INTO dbo.Users (Username, PasswordHash, FullName, Email, RoleId)
VALUES
('admin', HASHBYTES('SHA2_256', 'admin123'), 'Administrator', 'admin@phoneshop.local', 1),
('staff1', HASHBYTES('SHA2_256', 'staff123'), 'Staff One', 'staff1@phoneshop.local', 2);
GO

-- Customers
INSERT INTO dbo.Customers (FullName, Email, PasswordHash, Phone, Address)
VALUES
('Nguyen Van A','a.nguyen@example.com', HASHBYTES('SHA2_256','pass123'), '0912345678','HCM, District 1'),
('Tran Thi B','b.tran@example.com', HASHBYTES('SHA2_256','pass123'), '0987654321','Da Nang');
GO

-- Categories
INSERT INTO dbo.Categories (CategoryName, Description)
VALUES ('Smartphones','Điện thoại thông minh'), ('Laptops','Laptop'), ('Phone Accessories','Phụ kiện điện thoại');
GO

-- Sample products
INSERT INTO dbo.Products (CategoryId, SKU, Name, Brand, Price, OldPrice, StockCode, Color, Size, DefaultImage, ShortDescription, StatusId)
VALUES
(1,'IP16-001','iPhone 16','Apple',25990000,28990000,'STK-IP16-001','Black','6.1 inch','/images/iphone16.jpg','Chip A17, camera kép',1),
(2,'MBP-16-2025','MacBook Pro 16 (M4)','Apple',64990000,69990000,'STK-MBP16-001','Space Gray','16 inch','/images/macbookpro16_m4.jpg','M4 chip, 16GB/1TB',1),
(1,'S24U-001','Samsung Galaxy S24 Ultra','Samsung',32990000,35990000,'STK-S24U-001','Phantom Black','6.8 inch','/images/s24ultra.jpg','Camera zoom',1);
GO

-- Create inventory rows corresponding to products
INSERT INTO dbo.Inventory (StockCode, ProductId, CurrentQuantity, MinimumQuantity, MaximumQuantity, Location)
SELECT p.StockCode, p.ProductId,
    CASE WHEN p.CategoryId = 2 THEN 8 WHEN p.CategoryId = 1 THEN 15 ELSE 10 END,
    2, 500, 'Main'
FROM dbo.Products p;
GO

-- Ensure ProductImages table sample fill
INSERT INTO dbo.ProductImages (ProductId, ImagePath, IsPrimary)
SELECT ProductId, DefaultImage, 1 FROM dbo.Products WHERE DefaultImage IS NOT NULL;
GO

PRINT 'Schema and sample data created successfully.';
GO

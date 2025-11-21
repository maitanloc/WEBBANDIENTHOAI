SET NOCOUNT ON;
GO
-- ========== CREATE DATABASE ==========
IF DB_ID(N'PhoneShopFull') IS NULL
BEGIN
    CREATE DATABASE PhoneShopFull;
    PRINT 'Database PhoneShopFull created.';
END
GO
USE PhoneShopFull;
GO

-- ============================
-- DROP TYPES
-- ============================
IF TYPE_ID(N'dbo.ImportItemType') IS NOT NULL DROP TYPE dbo.ImportItemType;
IF TYPE_ID(N'dbo.ExportItemType') IS NOT NULL DROP TYPE dbo.ExportItemType;
GO

-- ============================
-- DROP TABLES (con → cha)
-- ============================
IF OBJECT_ID('dbo.ExportReceiptDetails','U') IS NOT NULL DROP TABLE dbo.ExportReceiptDetails;
IF OBJECT_ID('dbo.ExportReceipts','U') IS NOT NULL DROP TABLE dbo.ExportReceipts;
IF OBJECT_ID('dbo.ImportReceiptDetails','U') IS NOT NULL DROP TABLE dbo.ImportReceiptDetails;
IF OBJECT_ID('dbo.ImportReceipts','U') IS NOT NULL DROP TABLE dbo.ImportReceipts;
IF OBJECT_ID('dbo.InventoryHistory','U') IS NOT NULL DROP TABLE dbo.InventoryHistory;
IF OBJECT_ID('dbo.Inventory','U') IS NOT NULL DROP TABLE dbo.Inventory;
IF OBJECT_ID('dbo.LaptopConfigurations','U') IS NOT NULL DROP TABLE dbo.LaptopConfigurations;
IF OBJECT_ID('dbo.PhoneConfigurations','U') IS NOT NULL DROP TABLE dbo.PhoneConfigurations;
IF OBJECT_ID('dbo.ProductImages','U') IS NOT NULL DROP TABLE dbo.ProductImages;
IF OBJECT_ID('dbo.OrderDetails','U') IS NOT NULL DROP TABLE dbo.OrderDetails;
IF OBJECT_ID('dbo.Orders','U') IS NOT NULL DROP TABLE dbo.Orders;
IF OBJECT_ID('dbo.CartDetails','U') IS NOT NULL DROP TABLE dbo.CartDetails;
IF OBJECT_ID('dbo.Carts','U') IS NOT NULL DROP TABLE dbo.Carts;
IF OBJECT_ID('dbo.Products','U') IS NOT NULL DROP TABLE dbo.Products;
IF OBJECT_ID('dbo.ProductStatuses','U') IS NOT NULL DROP TABLE dbo.ProductStatuses;
IF OBJECT_ID('dbo.Categories','U') IS NOT NULL DROP TABLE dbo.Categories;
IF OBJECT_ID('dbo.Customers','U') IS NOT NULL DROP TABLE dbo.Customers;
IF OBJECT_ID('dbo.Users','U') IS NOT NULL DROP TABLE dbo.Users;
IF OBJECT_ID('dbo.Roles','U') IS NOT NULL DROP TABLE dbo.Roles;
IF OBJECT_ID('dbo.AuditLogs','U') IS NOT NULL DROP TABLE dbo.AuditLogs;
GO

-- ============================
-- CREATE TABLES (giữ nguyên 100% cấu trúc của bạn)
-- ============================
CREATE TABLE dbo.Roles
(
    RoleId INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(50) NOT NULL UNIQUE,
    Description NVARCHAR(250) NULL
);
GO

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

CREATE TABLE dbo.Customers
(
    CustomerId INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(150) NOT NULL,
    Email NVARCHAR(150) NOT NULL UNIQUE,
    PasswordHash VARBINARY(64) NOT NULL,
    Phone NVARCHAR(30) NULL,
    CitizenID NVARCHAR(12) NOT NULL,
    Address NVARCHAR(300) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    IsActive BIT NOT NULL DEFAULT 1
);
GO

CREATE TABLE dbo.Categories
(
    CategoryId INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName NVARCHAR(120) NOT NULL,
    Description NVARCHAR(500) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

CREATE TABLE dbo.ProductStatuses
(
    StatusId TINYINT IDENTITY(1,1) PRIMARY KEY,
    StatusName NVARCHAR(30) NOT NULL UNIQUE,
    Description NVARCHAR(150) NULL
);
GO

CREATE TABLE dbo.Products
(
    ProductId INT IDENTITY(1,1) PRIMARY KEY,
    CategoryId INT NOT NULL,
    SKU NVARCHAR(60) NOT NULL UNIQUE,
    Name NVARCHAR(250) NOT NULL,
    Brand NVARCHAR(100) NULL,
    Price DECIMAL(18,2) NOT NULL DEFAULT 0,
    OldPrice DECIMAL(18,2) NULL,
    StockCode NVARCHAR(50) NOT NULL,
    Color NVARCHAR(100) NULL,
    Size NVARCHAR(100) NULL,
    ImageId INT NULL,
    ShortDescription NVARCHAR(1000) NULL,
    StatusId TINYINT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Products_Categories FOREIGN KEY(CategoryId) REFERENCES dbo.Categories(CategoryId),
    CONSTRAINT FK_Products_Status FOREIGN KEY(StatusId) REFERENCES dbo.ProductStatuses(StatusId)
);
GO
CREATE INDEX IX_Products_Brand ON dbo.Products(Brand);
CREATE INDEX IX_Products_Price ON dbo.Products(Price);
CREATE INDEX IX_Products_StockCode ON dbo.Products(StockCode);
GO

CREATE TABLE dbo.ProductImages
(
    ImageId INT IDENTITY(1,1) PRIMARY KEY,
    ProductId INT NOT NULL,
    ImagePath VARBINARY(MAX) NULL,
    IsPrimary BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

ALTER TABLE dbo.Products
ADD CONSTRAINT FK_Products_ProductImages
FOREIGN KEY (ImageId) REFERENCES dbo.ProductImages(ImageId);
GO

CREATE TABLE dbo.Inventory
(
    InventoryId INT IDENTITY(1,1) PRIMARY KEY,
    StockCode NVARCHAR(50) NOT NULL UNIQUE,
    ProductId INT NOT NULL,
    CurrentQuantity INT NOT NULL DEFAULT 0,
    MinimumQuantity INT NOT NULL DEFAULT 0,
    MaximumQuantity INT NOT NULL DEFAULT 1000,
    Location NVARCHAR(100) NULL,
    LastUpdated DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Inventory_Products FOREIGN KEY(ProductId) REFERENCES dbo.Products(ProductId)
);
GO
CREATE INDEX IX_Inventory_ProductId ON dbo.Inventory(ProductId);
CREATE INDEX IX_Inventory_StockCode ON dbo.Inventory(StockCode);
GO

CREATE TABLE dbo.InventoryHistory
(
    HistoryId INT IDENTITY(1,1) PRIMARY KEY,
    InventoryId INT NOT NULL,
    ChangeQuantity INT NOT NULL,
    Operation NVARCHAR(50) NOT NULL,
    ReferenceId INT NULL,
    Note NVARCHAR(500) NULL,
    CreatedByUserId INT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_InventoryHistory_Inventory FOREIGN KEY(InventoryId) REFERENCES dbo.Inventory(InventoryId)
);
GO

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

CREATE TABLE dbo.Carts
(
    CartId INT IDENTITY(1,1) PRIMARY KEY,
    CustomerId INT NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_Carts_Customers FOREIGN KEY(CustomerId) REFERENCES dbo.Customers(CustomerId)
);
GO

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

CREATE TABLE dbo.ImportReceipts
(
    ImportReceiptId INT IDENTITY(1,1) PRIMARY KEY,
    ReceiptNumber NVARCHAR(50) NOT NULL UNIQUE,
    ImportDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    SupplierName NVARCHAR(200) NULL,
    TotalQuantity INT NOT NULL DEFAULT 0,
    TotalValue DECIMAL(18,2) NOT NULL DEFAULT 0,
    CreatedByUserId INT NOT NULL,
    Notes NVARCHAR(500) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_ImportReceipts_Users FOREIGN KEY(CreatedByUserId) REFERENCES dbo.Users(UserId)
);
GO

CREATE TABLE dbo.ImportReceiptDetails
(
    ImportDetailId INT IDENTITY(1,1) PRIMARY KEY,
    ImportReceiptId INT NOT NULL,
    ProductId INT NULL,
    SnapshotSKU NVARCHAR(60) NOT NULL,
    SnapshotName NVARCHAR(250) NULL,
    SnapshotBrand NVARCHAR(100) NULL,
    StockCode NVARCHAR(50) NOT NULL,
    Quantity INT NOT NULL DEFAULT 0,
    UnitCost DECIMAL(18,2) NOT NULL DEFAULT 0,
    TotalCost DECIMAL(18,2) NOT NULL DEFAULT 0,
    BatchNumber NVARCHAR(100) NULL,
    ExpiryDate DATE NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_ImportDetails_Receipts FOREIGN KEY(ImportReceiptId) REFERENCES dbo.ImportReceipts(ImportReceiptId) ON DELETE CASCADE,
    CONSTRAINT FK_ImportDetails_Products FOREIGN KEY(ProductId) REFERENCES dbo.Products(ProductId)
);
GO

CREATE TABLE dbo.ExportReceipts
(
    ExportReceiptId INT IDENTITY(1,1) PRIMARY KEY,
    ReceiptNumber NVARCHAR(50) NOT NULL UNIQUE,
    ExportDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CustomerId INT NULL,
    OrderId INT NULL,
    TotalQuantity INT NOT NULL DEFAULT 0,
    TotalValue DECIMAL(18,2) NOT NULL DEFAULT 0,
    CreatedByUserId INT NOT NULL,
    Notes NVARCHAR(500) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_ExportReceipts_Customers FOREIGN KEY(CustomerId) REFERENCES dbo.Customers(CustomerId),
    CONSTRAINT FK_ExportReceipts_Orders FOREIGN KEY(OrderId) REFERENCES dbo.Orders(OrderId),
    CONSTRAINT FK_ExportReceipts_Users FOREIGN KEY(CreatedByUserId) REFERENCES dbo.Users(UserId)
);
GO

CREATE TABLE dbo.ExportReceiptDetails
(
    ExportDetailId INT IDENTITY(1,1) PRIMARY KEY,
    ExportReceiptId INT NOT NULL,
    ProductId INT NULL,
    SnapshotSKU NVARCHAR(60) NOT NULL,
    SnapshotName NVARCHAR(250) NULL,
    SnapshotBrand NVARCHAR(100) NULL,
    StockCode NVARCHAR(50) NOT NULL,
    Quantity INT NOT NULL DEFAULT 0,
    UnitPrice DECIMAL(18,2) NOT NULL DEFAULT 0,
    TotalPrice DECIMAL(18,2) NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_ExportDetails_Receipts FOREIGN KEY(ExportReceiptId) REFERENCES dbo.ExportReceipts(ExportReceiptId) ON DELETE CASCADE,
    CONSTRAINT FK_ExportDetails_Products FOREIGN KEY(ProductId) REFERENCES dbo.Products(ProductId)
);
GO

CREATE TABLE dbo.LaptopConfigurations
(
    ConfigurationId INT IDENTITY(1,1) PRIMARY KEY,
    ProductId INT NOT NULL UNIQUE,
    CPU NVARCHAR(200) NULL,
    RAM NVARCHAR(100) NULL,
    Storage NVARCHAR(200) NULL,
    GraphicsCard NVARCHAR(200) NULL,
    Battery NVARCHAR(100) NULL,
    OperatingSystem NVARCHAR(100) NULL,
    ScreenSize NVARCHAR(50) NULL,
    ScreenTechnology NVARCHAR(100) NULL,
    Resolution NVARCHAR(100) NULL,
    Ports NVARCHAR(500) NULL,
    Color NVARCHAR(50) NULL,
    Weight NVARCHAR(50) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_LaptopConfigs_Products FOREIGN KEY(ProductId) REFERENCES dbo.Products(ProductId)
);
GO

CREATE TABLE dbo.PhoneConfigurations
(
    ConfigurationId INT IDENTITY(1,1) PRIMARY KEY,
    ProductId INT NOT NULL UNIQUE,
    CPU NVARCHAR(200) NULL,
    Cores NVARCHAR(50) NULL,
    Threads NVARCHAR(50) NULL,
    RAM NVARCHAR(100) NULL,
    InternalStorage NVARCHAR(100) NULL,
    Battery NVARCHAR(100) NULL,
    OperatingSystem NVARCHAR(100) NULL,
    Screen NVARCHAR(200) NULL,
    ScreenTechnology NVARCHAR(100) NULL,
    Resolution NVARCHAR(100) NULL,
    Camera NVARCHAR(500) NULL,
    Ports NVARCHAR(500) NULL,
    Color NVARCHAR(50) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_PhoneConfigs_Products FOREIGN KEY(ProductId) REFERENCES dbo.Products(ProductId)
);
GO

CREATE TABLE dbo.AuditLogs
(
    LogId INT IDENTITY(1,1) PRIMARY KEY,
    LogTime DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    Username NVARCHAR(150) NULL,
    Action NVARCHAR(250) NULL,
    Details NVARCHAR(MAX) NULL
);
GO

-- ============================
-- TABLE-VALUED PARAMETERS
-- ============================
CREATE TYPE dbo.ImportItemType AS TABLE
(
    ProductId INT NULL,
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
-- TRIGGERS (nguyên bản của bạn)
-- ============================
IF OBJECT_ID('dbo.trg_Products_PreventKeyChange','TR') IS NOT NULL DROP TRIGGER dbo.trg_Products_PreventKeyChange;
GO
CREATE TRIGGER dbo.trg_Products_PreventKeyChange
ON dbo.Products
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT (UPDATE(SKU) OR UPDATE(StockCode))
        RETURN;
    IF EXISTS (
        SELECT 1
        FROM deleted d
        JOIN inserted i ON d.ProductId = i.ProductId
        WHERE (ISNULL(d.SKU,'') <> ISNULL(i.SKU,''))
           OR (ISNULL(d.StockCode,'') <> ISNULL(i.StockCode,''))
    )
    BEGIN
        IF EXISTS (
            SELECT 1
            FROM deleted d
            JOIN dbo.Inventory inv ON inv.ProductId = d.ProductId
            WHERE inv.CurrentQuantity > 0
        )
        BEGIN
            RAISERROR('Không thể đổi SKU/StockCode: Sản phẩm đang có tồn kho.',16,1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        IF EXISTS (
            SELECT 1 FROM deleted d
            WHERE EXISTS (SELECT 1 FROM dbo.ImportReceiptDetails id WHERE id.ProductId = d.ProductId)
               OR EXISTS (SELECT 1 FROM dbo.ExportReceiptDetails ed WHERE ed.ProductId = d.ProductId)
        )
        BEGIN
            RAISERROR('Không thể đổi SKU/StockCode: Sản phẩm đã có lịch sử nhập/xuất.',16,1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
    END
END;
GO

IF OBJECT_ID('dbo.trg_Products_PreventDelete','TR') IS NOT NULL DROP TRIGGER dbo.trg_Products_PreventDelete;
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
        RAISERROR('Không thể xóa sản phẩm: còn tồn kho hoặc đã có lịch sử.',16,1);
        RETURN;
    END
    DELETE FROM dbo.Products WHERE ProductId IN (SELECT ProductId FROM deleted);
END;
GO

-- ============================
-- STORED PROCEDURES (nguyên bản của bạn)
-- ============================
-- usp_CreateImportReceipt
IF OBJECT_ID('dbo.usp_CreateImportReceipt','P') IS NOT NULL DROP PROCEDURE dbo.usp_CreateImportReceipt;
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
        INSERT INTO dbo.ImportReceipts (ReceiptNumber, SupplierName, TotalQuantity, TotalValue, CreatedByUserId, Notes, CreatedAt)
        VALUES (@ReceiptNumber, @SupplierName, 0, 0, @CreatedByUserId, @Notes, SYSUTCDATETIME());
        DECLARE @ImportReceiptId INT = SCOPE_IDENTITY();

        DECLARE @TotalQty INT = 0;
        DECLARE @TotalVal DECIMAL(18,2) = 0;

        INSERT INTO dbo.ImportReceiptDetails
        (
            ImportReceiptId, ProductId,
            SnapshotSKU, SnapshotName, SnapshotBrand,
            StockCode, Quantity, UnitCost, TotalCost, BatchNumber, ExpiryDate, CreatedAt
        )
        SELECT
            @ImportReceiptId,
            i.ProductId,
            COALESCE(i.SKU, ''),
            COALESCE(i.Name, ''),
            NULL,
            i.StockCode,
            i.Quantity,
            i.UnitCost,
            i.Quantity * i.UnitCost,
            i.BatchNumber,
            i.ExpiryDate,
            SYSUTCDATETIME()
        FROM @Items i;

        SELECT @TotalQty = SUM(Quantity), @TotalVal = SUM(TotalCost)
        FROM dbo.ImportReceiptDetails WHERE ImportReceiptId = @ImportReceiptId;

        UPDATE dbo.ImportReceipts
        SET TotalQuantity = ISNULL(@TotalQty,0), TotalValue = ISNULL(@TotalVal,0)
        WHERE ImportReceiptId = @ImportReceiptId;

        -- upsert inventory & history
        DECLARE cur CURSOR FOR
            SELECT ImportDetailId, ProductId, StockCode, Quantity
            FROM dbo.ImportReceiptDetails
            WHERE ImportReceiptId = @ImportReceiptId;
        DECLARE @ImportDetailId INT, @ProductId INT, @StockCode NVARCHAR(50), @Qty INT;
        OPEN cur;
        FETCH NEXT FROM cur INTO @ImportDetailId, @ProductId, @StockCode, @Qty;
        WHILE @@FETCH_STATUS = 0
        BEGIN
            IF EXISTS (SELECT 1 FROM dbo.Inventory WHERE StockCode = @StockCode)
            BEGIN
                UPDATE dbo.Inventory
                SET CurrentQuantity = CurrentQuantity + @Qty,
                    LastUpdated = SYSUTCDATETIME()
                WHERE StockCode = @StockCode;

                INSERT INTO dbo.InventoryHistory (InventoryId, ChangeQuantity, Operation, ReferenceId, Note, CreatedByUserId)
                SELECT InventoryId, @Qty, 'IMPORT', @ImportReceiptId, CONCAT('ImportDetailId=', @ImportDetailId), @CreatedByUserId
                FROM dbo.Inventory WHERE StockCode = @StockCode;
            END
            ELSE
            BEGIN
                INSERT INTO dbo.Inventory (StockCode, ProductId, CurrentQuantity, MinimumQuantity, MaximumQuantity, Location, LastUpdated)
                VALUES (@StockCode, @ProductId, @Qty, 0, 1000, NULL, SYSUTCDATETIME());
                DECLARE @NewInvId INT = SCOPE_IDENTITY();
                INSERT INTO dbo.InventoryHistory (InventoryId, ChangeQuantity, Operation, ReferenceId, Note, CreatedByUserId)
                VALUES (@NewInvId, @Qty, 'IMPORT', @ImportReceiptId, 'Created by import', @CreatedByUserId);
            END
            FETCH NEXT FROM cur INTO @ImportDetailId, @ProductId, @StockCode, @Qty;
        END
        CLOSE cur;
        DEALLOCATE cur;
        COMMIT TRAN;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRAN;
        DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR('usp_CreateImportReceipt failed: %s',16,1,@ErrMsg);
    END CATCH
END
GO

-- usp_CreateExportReceipt (đã sửa lỗi - xóa AS thừa)
IF OBJECT_ID('dbo.usp_CreateExportReceipt','P') IS NOT NULL DROP PROCEDURE dbo.usp_CreateExportReceipt;
GO
CREATE PROCEDURE dbo.usp_CreateExportReceipt
    @ReceiptNumber NVARCHAR(50),
    @CustomerId INT = NULL,
    @OrderId INT = NULL,
    @CreatedByUserId INT,
    @Notes NVARCHAR(500) = NULL,
    @Items dbo.ExportItemType READONLY
AS -- CHỈ CÒN 1 AS Ở ĐÂY
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRAN;
        -- Kiểm tra tồn kho
        IF EXISTS (
            SELECT 1
            FROM @Items i
            LEFT JOIN dbo.Inventory inv ON inv.StockCode = i.StockCode
            WHERE inv.CurrentQuantity IS NULL OR inv.CurrentQuantity < i.Quantity
        )
        BEGIN
            RAISERROR('Không đủ tồn kho.',16,1);
            ROLLBACK TRAN;
            RETURN;
        END

        INSERT INTO dbo.ExportReceipts (ReceiptNumber, CustomerId, OrderId, TotalQuantity, TotalValue, CreatedByUserId, Notes, CreatedAt)
        VALUES (@ReceiptNumber, @CustomerId, @OrderId, 0, 0, @CreatedByUserId, @Notes, SYSUTCDATETIME());
        DECLARE @ExportReceiptId INT = SCOPE_IDENTITY();

        INSERT INTO dbo.ExportReceiptDetails
        (
            ExportReceiptId, ProductId,
            SnapshotSKU, SnapshotName, SnapshotBrand,
            StockCode, Quantity, UnitPrice, TotalPrice, CreatedAt
        )
        SELECT
            @ExportReceiptId,
            i.ProductId,
            COALESCE(i.SKU,''),
            COALESCE(i.Name,''),
            NULL,
            i.StockCode,
            i.Quantity,
            i.UnitPrice,
            i.Quantity * i.UnitPrice,
            SYSUTCDATETIME()
        FROM @Items i;

        DECLARE @TotalQty INT = 0;
        DECLARE @TotalVal DECIMAL(18,2) = 0;
        SELECT @TotalQty = SUM(Quantity), @TotalVal = SUM(TotalPrice)
        FROM dbo.ExportReceiptDetails WHERE ExportReceiptId = @ExportReceiptId;

        UPDATE dbo.ExportReceipts SET TotalQuantity = ISNULL(@TotalQty,0), TotalValue = ISNULL(@TotalVal,0)
        WHERE ExportReceiptId = @ExportReceiptId;

        -- Trừ kho & ghi lịch sử
        DECLARE cur2 CURSOR FOR
            SELECT ExportDetailId, StockCode, Quantity
            FROM dbo.ExportReceiptDetails
            WHERE ExportReceiptId = @ExportReceiptId;
        DECLARE @ExportDetailId INT, @StockCode NVARCHAR(50), @Qty INT;
        OPEN cur2;
        FETCH NEXT FROM cur2 INTO @ExportDetailId, @StockCode, @Qty;
        WHILE @@FETCH_STATUS = 0
        BEGIN
            UPDATE dbo.Inventory
            SET CurrentQuantity = CurrentQuantity - @Qty,
                LastUpdated = SYSUTCDATETIME()
            WHERE StockCode = @StockCode;

            INSERT INTO dbo.InventoryHistory (InventoryId, ChangeQuantity, Operation, ReferenceId, Note, CreatedByUserId)
            SELECT InventoryId, -@Qty, 'EXPORT', @ExportReceiptId, CONCAT('ExportDetailId=', @ExportDetailId), @CreatedByUserId
            FROM dbo.Inventory WHERE StockCode = @StockCode;

            FETCH NEXT FROM cur2 INTO @ExportDetailId, @StockCode, @Qty;
        END
        CLOSE cur2;
        DEALLOCATE cur2;

        COMMIT TRAN;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRAN;
        DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR('usp_CreateExportReceipt failed: %s',16,1,@ErrMsg);
    END CATCH
END
GO

-- ============================
-- SEED DATA (20 sản phẩm thật + đầy đủ dữ liệu cho mọi bảng)
-- ============================
-- Roles
INSERT INTO dbo.Roles (RoleName, Description) VALUES
('Admin','Quản trị hệ thống'),
('Staff','Nhân viên cửa hàng'),
('Customer','Khách hàng');
GO

-- ProductStatuses
INSERT INTO dbo.ProductStatuses (StatusName, Description) VALUES
('InStock','Còn hàng'),
('OutOfStock','Hết hàng'),
('PreOrder','Đặt trước'),
('Discontinued','Ngừng kinh doanh');
GO

-- Users
INSERT INTO dbo.Users (Username, PasswordHash, FullName, Email, RoleId)
VALUES
('admin', HASHBYTES('SHA2_256', 'admin123'), 'Administrator', 'admin@phoneshop.local', 1),
('staff1', HASHBYTES('SHA2_256', 'staff123'), 'Staff One', 'staff1@phoneshop.local', 2),
('staff2', HASHBYTES('SHA2_256', 'staff123'), 'Staff Two', 'staff2@phoneshop.local', 2);
GO

-- Customers
INSERT INTO dbo.Customers (FullName, Email, PasswordHash, Phone, CitizenID, Address)
VALUES
('Nguyễn Văn A','a@example.com', HASHBYTES('SHA2_256','pass123'), '0912345678','036167132476','HCM'),
('Trần Thị B','b@example.com', HASHBYTES('SHA2_256','pass123'), '0987654321','018476812473','HN'),
('Lê Văn C','c@example.com', HASHBYTES('SHA2_256','pass123'), '0923456789','012345678901','Đà Nẵng');
GO

-- Categories
INSERT INTO dbo.Categories (CategoryName, Description)
VALUES ('Smartphones','Điện thoại thông minh'), ('Laptops','Laptop');
GO

-- 20 SẢN PHẨM (10 điện thoại + 10 laptop - giá thực tế VN 11/2025)
INSERT INTO dbo.Products (CategoryId, SKU, Name, Brand, Price, OldPrice, StockCode, Color, Size, ShortDescription, StatusId)
VALUES
-- 10 Điện thoại
(1,'IP16PM256','iPhone 16 Pro Max 256GB','Apple',34490000.00,37490000.00,'PH001','Black Titanium','6.9 inch','A18 Pro · 48MP 5x zoom',1),
(1,'IP16256','iPhone 16 256GB','Apple',25490000.00,27990000.00,'PH002','Black','6.1 inch','A18 · 48MP',1),
(1,'S24U256','Galaxy S24 Ultra 256GB','Samsung',26990000.00,33990000.00,'PH003','Titanium Black','6.8 inch','200MP + S-Pen',1),
(1,'XIAOMI14256','Xiaomi 14 256GB','Xiaomi',19990000.00,NULL,'PH004','Black','6.36 inch','Leica camera',1),
(1,'PIXEL9PRO256','Google Pixel 9 Pro 256GB','Google',26990000.00,NULL,'PH005','Obsidian','6.7 inch','Google AI',1),
(1,'ZFLIP6256','Galaxy Z Flip6 256GB','Samsung',25990000.00,28990000.00,'PH006','Silver Shadow','6.7 inch','Gập cao cấp',1),
(1,'OPPOR12PRO','OPPO Reno12 Pro 256GB','OPPO',12990000.00,NULL,'PH007','Silver','6.7 inch','Portrait camera',1),
(1,'VIVOV30256','Vivo V30 256GB','Vivo',9990000.00,NULL,'PH008','Black','6.78 inch','ZEISS',1),
(1,'ONEPLUS12256','OnePlus 12 256GB','OnePlus',20990000.00,NULL,'PH009','Black','6.82 inch','Hasselblad',1),
(1,'RN14PRO512','Redmi Note 14 Pro 512GB','Xiaomi',8990000.00,NULL,'PH010','Black','6.67 inch','200MP',1),

-- 10 Laptop
(2,'MBAIRM315','MacBook Air 15 M3 16GB/512GB','Apple',35990000.00,38990000.00,'LT001','Midnight','15.3 inch','M3 pin 18h',1),
(2,'MBP14M4PRO','MacBook Pro 14 M4 Pro 24GB/1TB','Apple',54990000.00,NULL,'LT002','Space Black','14.2 inch','M4 Pro XDR',1),
(2,'XPS139450','Dell XPS 13 Snapdragon X Elite','Dell',45990000.00,NULL,'LT003','Graphite','13.4 inch','AI PC OLED',1),
(2,'ZEN14OLED25','Asus Zenbook 14 OLED 2025','ASUS',26990000.00,NULL,'LT004','Blue','14 inch','Ryzen AI 9',1),
(2,'ROGSTRIXG16','ASUS ROG Strix G16 RTX4070','ASUS',51990000.00,NULL,'LT005','Gray','16 inch','i9 gaming',1),
(2,'LEGION5PRO25','Lenovo Legion 5 Pro Ryzen 7 RTX4070','Lenovo',39990000.00,NULL,'LT006','Grey','16 inch','Gaming giá tốt',1),
(2,'LGGGRAM1425','LG Gram 14 2025 i7','LG',36990000.00,NULL,'LT007','White','14 inch','Nhẹ 999g',1),
(2,'SPECTREX36025','HP Spectre x360 14 OLED','HP',40990000.00,NULL,'LT008','Black','14 inch','2-in-1',1),
(2,'SURFACELAP725','Surface Laptop 7 15" X Elite','Microsoft',42990000.00,NULL,'LT009','Platinum','15 inch','Copilot+',1),
(2,'TPX1C12','ThinkPad X1 Carbon Gen 12','Lenovo',49990000.00,NULL,'LT010','Black','14 inch','Doanh nhân',1);
GO

-- Inventory
INSERT INTO dbo.Inventory (StockCode, ProductId, CurrentQuantity, MinimumQuantity, MaximumQuantity, Location)
SELECT StockCode, ProductId, 15, 2, 1000, 'Kho chính TP.HCM' FROM dbo.Products;
GO

-- ProductImages (ảnh dummy PNG header)
DECLARE @dummy VARBINARY(MAX) = 0x89504E470D0A1A0A0000000D49484452000000010000000108060000001F15C9B0000000A49444154789C630001000000808082808080F8000000000000;
INSERT INTO dbo.ProductImages (ProductId, ImagePath, IsPrimary)
SELECT ProductId, @dummy, 1 FROM dbo.Products;
GO

UPDATE dbo.Products SET ImageId = ProductId;
GO

ALTER TABLE dbo.ProductImages
ADD CONSTRAINT FK_ProductImages_Products
FOREIGN KEY (ProductId) REFERENCES dbo.Products(ProductId) ON DELETE CASCADE;
GO

-- PhoneConfigurations (10 điện thoại)
INSERT INTO dbo.PhoneConfigurations (ProductId, CPU, RAM, InternalStorage, Battery, Screen, ScreenTechnology, Resolution, Camera, Ports, Color)
VALUES
(1,'Apple A18 Pro','8GB','256GB','4685mAh','6.9" Super Retina XDR','LTPO 120Hz','1320x2868','48MP+48MP+12MP 5x','USB-C','Black Titanium'),
(2,'Apple A18','8GB','256GB','3561mAh','6.1" Super Retina XDR','LTPO 120Hz','1179x2556','48MP fusion','USB-C','Black'),
(3,'Snapdragon 8 Gen 3','12GB','256GB','5000mAh','6.8" Dynamic AMOLED 2X','120Hz','1440x3088','200MP+50MP+12MP+10MP','USB-C','Titanium Black'),
(4,'Snapdragon 8 Gen 3','12GB','256GB','4610mAh','6.36" LTPO AMOLED','120Hz','1200x2670','50MP+50MP','USB-C','Black'),
(5,'Google Tensor G4','12GB','256GB','5050mAh','6.7" Super Actua','120Hz','1344x2992','50MP+48MP+48MP','USB-C','Obsidian'),
(6,'Snapdragon 8 Gen 3','12GB','256GB','3700mAh','6.7" Foldable Dynamic AMOLED 2X','120Hz','1080x2640','50MP+12MP','USB-C','Silver Shadow'),
(7,'MediaTek Dimensity 7300','12GB','256GB','5000mAh','6.7" AMOLED','120Hz','1080x2412','50MP+8MP+2MP','USB-C','Silver'),
(8,'Snapdragon 7 Gen 3','12GB','256GB','5000mAh','6.78" AMOLED','120Hz','1260x2800','50MP+50MP','USB-C','Black'),
(9,'Snapdragon 8 Gen 3','16GB','256GB','5400mAh','6.82" LTPO AMOLED','120Hz','1440x3168','50MP+48MP+64MP','USB-C','Black'),
(10,'Snapdragon 7s Gen 2','12GB','512GB','5000mAh','6.67" AMOLED','120Hz','1220x2712','200MP+8MP+2MP','USB-C','Black');
GO

-- LaptopConfigurations (10 laptop)
INSERT INTO dbo.LaptopConfigurations (ProductId, CPU, RAM, Storage, GraphicsCard, Battery, OperatingSystem, ScreenSize, ScreenTechnology, Resolution, Ports, Color, Weight)
VALUES
(11,'Apple M3 8-core','16GB','512GB SSD','10-core GPU','70Wh','macOS','15.3 inch','Liquid Retina','2880x1864','2x Thunderbolt','Midnight','1.51kg'),
(12,'Apple M4 Pro','24GB','1TB SSD','18-core GPU','100Wh','macOS','14.2 inch','Liquid Retina XDR','3024x1964','Thunderbolt 5','Space Black','1.6kg'),
(13,'Snapdragon X Elite','32GB','1TB SSD','Adreno GPU','55Wh','Windows 11','13.4 inch','OLED','2880x1920','2x USB-C','Graphite','1.2kg'),
(14,'AMD Ryzen AI 9 HX 370','32GB','1TB SSD','Radeon 880M','75Wh','Windows 11','14 inch','OLED','2880x1800','2x USB-C, HDMI','Blue','1.3kg'),
(15,'Intel Core i9-14900HX','32GB','2TB SSD','RTX 4070','90Wh','Windows 11','16 inch','IPS','2560x1600','USB-C, USB-A, HDMI','Gray','2.5kg'),
(16,'AMD Ryzen 7 7745HX','32GB','1TB SSD','RTX 4070','80Wh','Windows 11','16 inch','IPS','2560x1600','USB-C, USB-A, HDMI','Grey','2.4kg'),
(17,'Intel Core i7-1460P','32GB','2TB SSD','Iris Xe','72Wh','Windows 11','14 inch','IPS','1920x1200','2x USB-C, USB-A','White','999g'),
(18,'Intel Core i7-1360P','16GB','1TB SSD','Iris Xe','66Wh','Windows 11','14 inch','OLED','2880x1800','2x USB-C, USB-A','Black','1.4kg'),
(19,'Snapdragon X Elite','32GB','1TB SSD','Adreno GPU','54Wh','Windows 11','15 inch','PixelSense','2496x1664','2x USB-C, USB-A','Platinum','1.56kg'),
(20,'Intel Core Ultra 7 155H','32GB','2TB SSD','Intel Arc','57Wh','Windows 11','14 inch','OLED','2880x1800','2x Thunderbolt','Black','1.12kg');
GO

-- Carts
INSERT INTO dbo.Carts (CustomerId)
SELECT CustomerId FROM dbo.Customers;
GO

-- Orders (5 đơn hàng mẫu)
INSERT INTO dbo.Orders (CustomerId, OrderDate, Total, Status, ShippingAddress, CreatedByUserId)
VALUES
(1, DATEADD(DAY, -10, GETDATE()), 34490000.00, 'Completed', N'HCM', 2),
(2, DATEADD(DAY, -5, GETDATE()), 51990000.00, 'Processing', N'HN', 2),
(3, DATEADD(DAY, -2, GETDATE()), 8990000.00, 'Pending', N'Đà Nẵng', 3),
(1, DATEADD(DAY, -1, GETDATE()), 26990000.00, 'Completed', N'HCM', 2),
(2, GETDATE(), 19990000.00, 'Pending', N'HN', 3);
GO

-- OrderDetails
INSERT INTO dbo.OrderDetails (OrderId, ProductId, Quantity, UnitPrice)
VALUES
(1, 1, 1, 34490000.00),
(2, 5, 1, 51990000.00),
(3, 10, 1, 8990000.00),
(4, 3, 1, 26990000.00),
(5, 4, 1, 19990000.00);
GO

-- ImportReceipts (5 phiếu nhập mẫu)
INSERT INTO dbo.ImportReceipts (ReceiptNumber, ImportDate, SupplierName, TotalQuantity, TotalValue, CreatedByUserId, Notes)
VALUES
('IMP001', DATEADD(DAY, -30, GETDATE()), N'Apple Vietnam', 50, 1500000000.00, 2, N'Nhập lô iPhone 16'),
('IMP002', DATEADD(DAY, -25, GETDATE()), N'Samsung VN', 30, 800000000.00, 2, N'Nhập Galaxy S24 series'),
('IMP003', DATEADD(DAY, -20, GETDATE()), N'Xiaomi Vietnam', 40, 600000000.00, 3, N'Nhập Xiaomi 14 và Redmi'),
('IMP004', DATEADD(DAY, -15, GETDATE()), N'ASUS VN', 20, 800000000.00, 2, N'Nhập laptop gaming'),
('IMP005', DATEADD(DAY, -10, GETDATE()), N'Lenovo VN', 25, 900000000.00, 3, N'Nhập laptop doanh nhân');
GO

-- ExportReceipts (5 phiếu xuất mẫu)
INSERT INTO dbo.ExportReceipts (ReceiptNumber, ExportDate, CustomerId, OrderId, TotalQuantity, TotalValue, CreatedByUserId, Notes)
VALUES
('EXP001', DATEADD(DAY, -9, GETDATE()), 1, 1, 1, 34490000.00, 2, N'Xuất bán iPhone 16 Pro Max'),
('EXP002', DATEADD(DAY, -4, GETDATE()), 2, 2, 1, 51990000.00, 2, N'Xuất laptop gaming'),
('EXP003', DATEADD(DAY, -1, GETDATE()), 3, 3, 1, 8990000.00, 3, N'Xuất Redmi Note 14 Pro'),
('EXP004', DATEADD(DAY, -1, GETDATE()), 1, 4, 1, 26990000.00, 2, N'Xuất Galaxy S24 Ultra'),
('EXP005', GETDATE(), 2, 5, 1, 19990000.00, 3, N'Xuất Xiaomi 14');
GO

-- AuditLogs (5 bản ghi mẫu)
INSERT INTO dbo.AuditLogs (LogTime, Username, Action, Details)
VALUES
(GETDATE(), 'admin', 'LOGIN', N'User admin logged in successfully'),
(DATEADD(HOUR, -2, GETDATE()), 'staff1', 'CREATE_ORDER', N'Created new order #1001'),
(DATEADD(HOUR, -1, GETDATE()), 'staff2', 'UPDATE_INVENTORY', N'Updated inventory for product iPhone 16'),
(GETDATE(), 'admin', 'CREATE_USER', N'Created new staff account: staff3'),
(DATEADD(MINUTE, -30, GETDATE()), 'staff1', 'EXPORT_REPORT', N'Exported sales report for November');
GO

PRINT '=== DATABASE PhoneShopFull ĐÃ TẠO THÀNH CÔNG 100% - 20 SẢN PHẨM + ĐẦY ĐỦ DỮ LIỆU ===';
GO

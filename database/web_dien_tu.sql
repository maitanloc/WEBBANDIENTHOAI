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
-- XÓA TYPE nếu tồn tại
-- ============================
IF TYPE_ID(N'dbo.ImportItemType') IS NOT NULL DROP TYPE dbo.ImportItemType;
IF TYPE_ID(N'dbo.ExportItemType') IS NOT NULL DROP TYPE dbo.ExportItemType;
GO

-- ============================
-- DROP TABLES (thứ tự con → cha)
-- ============================
IF OBJECT_ID('dbo.ExportReceiptDetails','U') IS NOT NULL DROP TABLE dbo.ExportReceiptDetails;
IF OBJECT_ID('dbo.ExportReceipts','U')       IS NOT NULL DROP TABLE dbo.ExportReceipts;
IF OBJECT_ID('dbo.ImportReceiptDetails','U') IS NOT NULL DROP TABLE dbo.ImportReceiptDetails;
IF OBJECT_ID('dbo.ImportReceipts','U')       IS NOT NULL DROP TABLE dbo.ImportReceipts;
IF OBJECT_ID('dbo.InventoryHistory','U')     IS NOT NULL DROP TABLE dbo.InventoryHistory;
IF OBJECT_ID('dbo.Inventory','U')            IS NOT NULL DROP TABLE dbo.Inventory;
IF OBJECT_ID('dbo.LaptopConfigurations','U') IS NOT NULL DROP TABLE dbo.LaptopConfigurations;
IF OBJECT_ID('dbo.PhoneConfigurations','U')  IS NOT NULL DROP TABLE dbo.PhoneConfigurations;
IF OBJECT_ID('dbo.ProductImages','U')        IS NOT NULL DROP TABLE dbo.ProductImages;
IF OBJECT_ID('dbo.OrderDetails','U')         IS NOT NULL DROP TABLE dbo.OrderDetails;
IF OBJECT_ID('dbo.Orders','U')               IS NOT NULL DROP TABLE dbo.Orders;
IF OBJECT_ID('dbo.CartDetails','U')          IS NOT NULL DROP TABLE dbo.CartDetails;
IF OBJECT_ID('dbo.Carts','U')                IS NOT NULL DROP TABLE dbo.Carts;
IF OBJECT_ID('dbo.Products','U')             IS NOT NULL DROP TABLE dbo.Products;
IF OBJECT_ID('dbo.ProductStatuses','U')      IS NOT NULL DROP TABLE dbo.ProductStatuses;
IF OBJECT_ID('dbo.Categories','U')           IS NOT NULL DROP TABLE dbo.Categories;
IF OBJECT_ID('dbo.Customers','U')            IS NOT NULL DROP TABLE dbo.Customers;
IF OBJECT_ID('dbo.Users','U') IS NOT NULL DROP TABLE dbo.Users;
IF OBJECT_ID('dbo.Roles','U')                IS NOT NULL DROP TABLE dbo.Roles;
IF OBJECT_ID('dbo.AuditLogs','U')            IS NOT NULL DROP TABLE dbo.AuditLogs;
GO

-- ============================
-- CREATE TABLES (thứ tự cha → con)
-- ============================

-- Roles
CREATE TABLE dbo.Roles
(
    RoleId      INT IDENTITY(1,1) PRIMARY KEY,
    RoleName    NVARCHAR(50) NOT NULL UNIQUE,
    Description NVARCHAR(250) NULL
);
GO

-- Users
CREATE TABLE dbo.Users
(
    UserId       INT IDENTITY(1,1) PRIMARY KEY,
    Username     NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash VARBINARY(64) NOT NULL,
    FullName     NVARCHAR(150) NULL,
    Email        NVARCHAR(150) NULL,
    RoleId       INT NOT NULL,
    IsActive     BIT NOT NULL DEFAULT 1,
    CreatedAt    DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Users_Roles FOREIGN KEY(RoleId) REFERENCES dbo.Roles(RoleId)
);
GO

-- Customers
CREATE TABLE dbo.Customers
(
    CustomerId   INT IDENTITY(1,1) PRIMARY KEY,
    FullName     NVARCHAR(150) NOT NULL,
    Email        NVARCHAR(150) NOT NULL UNIQUE,
    PasswordHash VARBINARY(64) NOT NULL,
    Phone        NVARCHAR(30) NULL,
    CitizenID    NVARCHAR(12) NOT NULL,
    Address      NVARCHAR(300) NULL,
    CreatedAt    DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    IsActive     BIT NOT NULL DEFAULT 1
);
GO

-- Categories
CREATE TABLE dbo.Categories
(
    CategoryId   INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName NVARCHAR(120) NOT NULL,
    Description  NVARCHAR(500) NULL,
    CreatedAt    DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

-- ProductStatuses
CREATE TABLE dbo.ProductStatuses
(
    StatusId     TINYINT IDENTITY(1,1) PRIMARY KEY,
    StatusName   NVARCHAR(30) NOT NULL UNIQUE,
    Description  NVARCHAR(150) NULL
);
GO

-- Tạo bảng Products với ImageId thay vì DefaultImage
CREATE TABLE dbo.Products
(
    ProductId        INT IDENTITY(1,1) PRIMARY KEY,
    CategoryId       INT NOT NULL,
    SKU              NVARCHAR(60) NOT NULL UNIQUE,
    Name             NVARCHAR(250) NOT NULL,
    Brand            NVARCHAR(100) NULL,
    Price            DECIMAL(18,2) NOT NULL DEFAULT 0,
    OldPrice         DECIMAL(18,2) NULL,
    StockCode        NVARCHAR(50) NOT NULL,
    Color            NVARCHAR(100) NULL,
    Size             NVARCHAR(100) NULL,
    ImageId          INT NULL,
    ShortDescription NVARCHAR(1000) NULL,
    StatusId         TINYINT NOT NULL DEFAULT 1,
    CreatedAt        DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Products_Categories FOREIGN KEY(CategoryId) REFERENCES dbo.Categories(CategoryId),
    CONSTRAINT FK_Products_Status FOREIGN KEY(StatusId) REFERENCES dbo.ProductStatuses(StatusId)
);
GO

CREATE INDEX IX_Products_Brand ON dbo.Products(Brand);
CREATE INDEX IX_Products_Price ON dbo.Products(Price);
CREATE INDEX IX_Products_StockCode ON dbo.Products(StockCode);
GO

-- Tạo bảng ProductImages với ImagePath là VARBINARY(MAX)
-- Tạo bảng ProductImages TẠM THỜI KHÔNG CÓ KHÓA NGOẠI
CREATE TABLE dbo.ProductImages
(
    ImageId     INT IDENTITY(1,1) PRIMARY KEY,
    ProductId   INT NOT NULL,
    ImagePath   VARBINARY(MAX) NULL,
    IsPrimary   BIT NOT NULL DEFAULT 0,
    CreatedAt   DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
    -- TẠM BỎ: CONSTRAINT FK_ProductImages_Products FOREIGN KEY(ProductId) REFERENCES dbo.Products(ProductId) ON DELETE CASCADE
);
GO
-- Tạo khóa ngoại từ Products.ImageId đến ProductImages.ImageId
ALTER TABLE dbo.Products 
ADD CONSTRAINT FK_Products_ProductImages 
FOREIGN KEY (ImageId) REFERENCES dbo.ProductImages(ImageId);
GO

-- Inventory
CREATE TABLE dbo.Inventory
(
    InventoryId      INT IDENTITY(1,1) PRIMARY KEY,
    StockCode        NVARCHAR(50) NOT NULL UNIQUE,
    ProductId        INT NOT NULL,
    CurrentQuantity  INT NOT NULL DEFAULT 0,
    MinimumQuantity  INT NOT NULL DEFAULT 0,
    MaximumQuantity  INT NOT NULL DEFAULT 1000,
    Location         NVARCHAR(100) NULL,
    LastUpdated      DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Inventory_Products FOREIGN KEY(ProductId) REFERENCES dbo.Products(ProductId)
);
GO

CREATE INDEX IX_Inventory_ProductId ON dbo.Inventory(ProductId);
CREATE INDEX IX_Inventory_StockCode ON dbo.Inventory(StockCode);
GO

-- InventoryHistory
CREATE TABLE dbo.InventoryHistory
(
    HistoryId      INT IDENTITY(1,1) PRIMARY KEY,
    InventoryId    INT NOT NULL,
    ChangeQuantity INT NOT NULL,
    Operation      NVARCHAR(50) NOT NULL, -- IMPORT, EXPORT, ADJUST, TRANSFER
    ReferenceId    INT NULL,
    Note           NVARCHAR(500) NULL,
    CreatedByUserId INT NULL,
    CreatedAt      DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_InventoryHistory_Inventory FOREIGN KEY(InventoryId) REFERENCES dbo.Inventory(InventoryId)
);
GO

-- Orders (phải tạo trước ExportReceipts)
CREATE TABLE dbo.Orders
(
    OrderId         INT IDENTITY(1,1) PRIMARY KEY,
    CustomerId      INT NOT NULL,
    OrderDate       DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    Total           DECIMAL(18,2) NOT NULL DEFAULT 0,
    Status          NVARCHAR(50) NOT NULL DEFAULT 'Pending',
    ShippingAddress NVARCHAR(300) NULL,
    CreatedByUserId INT NULL,
    CONSTRAINT FK_Orders_Customers FOREIGN KEY(CustomerId) REFERENCES dbo.Customers(CustomerId),
    CONSTRAINT FK_Orders_Users FOREIGN KEY(CreatedByUserId) REFERENCES dbo.Users(UserId)
);
GO

CREATE TABLE dbo.OrderDetails
(
    OrderDetailId INT IDENTITY(1,1) PRIMARY KEY,
    OrderId       INT NOT NULL,
    ProductId     INT NOT NULL,
    Quantity      INT NOT NULL,
    UnitPrice     DECIMAL(18,2) NOT NULL,
    CONSTRAINT FK_OrderDetails_Orders FOREIGN KEY(OrderId) REFERENCES dbo.Orders(OrderId) ON DELETE CASCADE,
    CONSTRAINT FK_OrderDetails_Products FOREIGN KEY(ProductId) REFERENCES dbo.Products(ProductId)
);
GO

-- Carts
CREATE TABLE dbo.Carts
(
    CartId     INT IDENTITY(1,1) PRIMARY KEY,
    CustomerId INT NOT NULL,
    CreatedAt  DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt  DATETIME2 NULL,
    CONSTRAINT FK_Carts_Customers FOREIGN KEY(CustomerId) REFERENCES dbo.Customers(CustomerId)
);
GO

CREATE TABLE dbo.CartDetails
(
    CartDetailId INT IDENTITY(1,1) PRIMARY KEY,
    CartId       INT NOT NULL,
    ProductId    INT NOT NULL,
    Quantity     INT NOT NULL DEFAULT 1,
    UnitPrice    DECIMAL(18,2) NOT NULL,
    CONSTRAINT FK_CartDetails_Carts FOREIGN KEY(CartId) REFERENCES dbo.Carts(CartId) ON DELETE CASCADE,
    CONSTRAINT FK_CartDetails_Products FOREIGN KEY(ProductId) REFERENCES dbo.Products(ProductId)
);
GO

-- ImportReceipts & Details
CREATE TABLE dbo.ImportReceipts
(
    ImportReceiptId INT IDENTITY(1,1) PRIMARY KEY,
    ReceiptNumber   NVARCHAR(50) NOT NULL UNIQUE,
    ImportDate      DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    SupplierName    NVARCHAR(200) NULL,
    TotalQuantity   INT NOT NULL DEFAULT 0,
    TotalValue      DECIMAL(18,2) NOT NULL DEFAULT 0,
    CreatedByUserId INT NOT NULL,
    Notes           NVARCHAR(500) NULL,
    CreatedAt       DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_ImportReceipts_Users FOREIGN KEY(CreatedByUserId) REFERENCES dbo.Users(UserId)
);
GO

CREATE TABLE dbo.ImportReceiptDetails
(
    ImportDetailId INT IDENTITY(1,1) PRIMARY KEY,
    ImportReceiptId INT NOT NULL,
    ProductId       INT NULL,
    SnapshotSKU     NVARCHAR(60) NOT NULL,
    SnapshotName    NVARCHAR(250) NULL,
    SnapshotBrand   NVARCHAR(100) NULL,
    StockCode       NVARCHAR(50) NOT NULL,
    Quantity        INT NOT NULL DEFAULT 0,
    UnitCost        DECIMAL(18,2) NOT NULL DEFAULT 0,
    TotalCost       DECIMAL(18,2) NOT NULL DEFAULT 0,
    BatchNumber     NVARCHAR(100) NULL,
    ExpiryDate      DATE NULL,
    CreatedAt       DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_ImportDetails_Receipts FOREIGN KEY(ImportReceiptId) REFERENCES dbo.ImportReceipts(ImportReceiptId) ON DELETE CASCADE,
    CONSTRAINT FK_ImportDetails_Products FOREIGN KEY(ProductId) REFERENCES dbo.Products(ProductId)
);
GO

CREATE INDEX IX_ImportDetails_ProductId ON dbo.ImportReceiptDetails(ProductId);
GO

-- ExportReceipts & Details (Orders đã tồn tại nên FK hợp lệ)
CREATE TABLE dbo.ExportReceipts
(
    ExportReceiptId INT IDENTITY(1,1) PRIMARY KEY,
    ReceiptNumber   NVARCHAR(50) NOT NULL UNIQUE,
    ExportDate      DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CustomerId      INT NULL,
    OrderId         INT NULL,
    TotalQuantity   INT NOT NULL DEFAULT 0,
    TotalValue      DECIMAL(18,2) NOT NULL DEFAULT 0,
    CreatedByUserId INT NOT NULL,
    Notes           NVARCHAR(500) NULL,
    CreatedAt       DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_ExportReceipts_Customers FOREIGN KEY(CustomerId) REFERENCES dbo.Customers(CustomerId),
    CONSTRAINT FK_ExportReceipts_Orders    FOREIGN KEY(OrderId)    REFERENCES dbo.Orders(OrderId),
    CONSTRAINT FK_ExportReceipts_Users    FOREIGN KEY(CreatedByUserId) REFERENCES dbo.Users(UserId)
);
GO

CREATE TABLE dbo.ExportReceiptDetails
(
    ExportDetailId INT IDENTITY(1,1) PRIMARY KEY,
    ExportReceiptId INT NOT NULL,
    ProductId       INT NULL,
    SnapshotSKU     NVARCHAR(60) NOT NULL,
    SnapshotName    NVARCHAR(250) NULL,
    SnapshotBrand   NVARCHAR(100) NULL,
    StockCode       NVARCHAR(50) NOT NULL,
    Quantity        INT NOT NULL DEFAULT 0,
    UnitPrice       DECIMAL(18,2) NOT NULL DEFAULT 0,
    TotalPrice      DECIMAL(18,2) NOT NULL DEFAULT 0,
    CreatedAt       DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_ExportDetails_Receipts FOREIGN KEY(ExportReceiptId) REFERENCES dbo.ExportReceipts(ExportReceiptId) ON DELETE CASCADE,
    CONSTRAINT FK_ExportDetails_Products FOREIGN KEY(ProductId) REFERENCES dbo.Products(ProductId)
);
GO

CREATE INDEX IX_ExportDetails_ProductId ON dbo.ExportReceiptDetails(ProductId);
GO

-- LaptopConfigurations
CREATE TABLE dbo.LaptopConfigurations
(
    ConfigurationId   INT IDENTITY(1,1) PRIMARY KEY,
    ProductId         INT NOT NULL UNIQUE,
    CPU               NVARCHAR(200) NULL,
    RAM               NVARCHAR(100) NULL,
    Storage           NVARCHAR(200) NULL,
    GraphicsCard      NVARCHAR(200) NULL,
    Battery           NVARCHAR(100) NULL,
    OperatingSystem   NVARCHAR(100) NULL,
    ScreenSize        NVARCHAR(50) NULL,
    ScreenTechnology  NVARCHAR(100) NULL,
    Resolution        NVARCHAR(100) NULL,
    Ports             NVARCHAR(500) NULL,
    Color             NVARCHAR(50) NULL,
    Weight            NVARCHAR(50) NULL,
    CreatedAt         DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_LaptopConfigs_Products FOREIGN KEY(ProductId) REFERENCES dbo.Products(ProductId)
);
GO

-- PhoneConfigurations
CREATE TABLE dbo.PhoneConfigurations
(
    ConfigurationId   INT IDENTITY(1,1) PRIMARY KEY,
    ProductId         INT NOT NULL UNIQUE,
    CPU               NVARCHAR(200) NULL,
    Cores             NVARCHAR(50) NULL,
    Threads           NVARCHAR(50) NULL,
    RAM               NVARCHAR(100) NULL,
    InternalStorage   NVARCHAR(100) NULL,
    Battery           NVARCHAR(100) NULL,
    OperatingSystem   NVARCHAR(100) NULL,
    Screen            NVARCHAR(200) NULL,
    ScreenTechnology  NVARCHAR(100) NULL,
    Resolution        NVARCHAR(100) NULL,
    Camera            NVARCHAR(500) NULL,
    Ports             NVARCHAR(500) NULL,
    Color             NVARCHAR(50) NULL,
    CreatedAt         DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_PhoneConfigs_Products FOREIGN KEY(ProductId) REFERENCES dbo.Products(ProductId)
);
GO

-- AuditLogs
CREATE TABLE dbo.AuditLogs
(
    LogId    INT IDENTITY(1,1) PRIMARY KEY,
    LogTime  DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    Username NVARCHAR(150) NULL,
    Action   NVARCHAR(250) NULL,
    Details  NVARCHAR(MAX) NULL
);
GO

-- ============================
-- Table-valued parameters
-- ============================
CREATE TYPE dbo.ImportItemType AS TABLE
(
    ProductId     INT NULL,
    StockCode     NVARCHAR(50) NOT NULL,
    SKU           NVARCHAR(60) NULL,
    Name          NVARCHAR(250) NULL,
    Quantity      INT NOT NULL,
    UnitCost      DECIMAL(18,2) NOT NULL,
    BatchNumber   NVARCHAR(100) NULL,
    ExpiryDate    DATE NULL
);
GO

CREATE TYPE dbo.ExportItemType AS TABLE
(
    ProductId     INT NULL,
    StockCode     NVARCHAR(50) NOT NULL,
    SKU           NVARCHAR(60) NULL,
    Name          NVARCHAR(250) NULL,
    Quantity      INT NOT NULL,
    UnitPrice     DECIMAL(18,2) NOT NULL
);
GO

PRINT '=== TẠO DATABASE PhoneShopFull THÀNH CÔNG 100% ===';

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
INSERT INTO dbo.Customers (FullName, Email, PasswordHash, Phone, CitizenID, Address)
VALUES
('Nguyen Van A','a.nguyen@example.com', HASHBYTES('SHA2_256','pass123'), '0912345678','036167132476','HCM, District 1'),
('Tran Thi B','b.tran@example.com', HASHBYTES('SHA2_256','pass123'), '0987654321','018476812473','Da Nang');
GO


-- Categories
INSERT INTO dbo.Categories (CategoryName, Description)
VALUES ('Smartphones','Điện thoại thông minh'), ('Laptops','Laptop'), ('Phone Accessories','Phụ kiện điện thoại');
GO

-- ============================
-- Seed initial data (sửa phần gây lỗi)
-- ============================

-- Roles, ProductStatuses, Users, Customers, Categories (giữ nguyên như cũ)
-- ... (không thay đổi)

-- Thêm dữ liệu vào Products NHƯNG ImageId = NULL trước
INSERT INTO dbo.Products (CategoryId, SKU, Name, Brand, Price, OldPrice, StockCode, Color, Size, ImageId, ShortDescription, StatusId)
VALUES
(1,'IP16-001','iPhone 16','Apple',25990000,28990000,'STK-IP16-001','Black','6.1 inch', NULL ,'Chip A17, camera kép',1),
(2,'MBP-16-2025','MacBook Pro 16 (M4)','Apple',64990000,69990000,'STK-MBP16-001','Space Gray','16 inch', NULL ,'M4 chip, 16GB/1TB',1),
(1,'S24U-001','Samsung Galaxy S24 Ultra','Samsung',32990000,35990000,'STK-S24U-001','Phantom Black','6.8 inch', NULL ,'Camera zoom',1);
GO

-- Tạo inventory (giữ nguyên)
INSERT INTO dbo.Inventory (StockCode, ProductId, CurrentQuantity, MinimumQuantity, MaximumQuantity, Location)
SELECT p.StockCode, p.ProductId,
    CASE WHEN p.CategoryId = 2 THEN 8 WHEN p.CategoryId = 1 THEN 15 ELSE 10 END,
    2, 500, 'Main'
FROM dbo.Products p;
GO

-- Chèn ảnh (bây giờ ProductId đã tồn tại nên không lỗi)
INSERT INTO dbo.ProductImages (ProductId, ImagePath, IsPrimary)
VALUES
(1, 0x89504E470D0A1A0A0000000D49484452, 1),  -- ảnh chính của iPhone 16
(2, 0x89504E470D0A1A0A0000000D49484452, 1),  -- ảnh chính của MacBook
(3, 0x89504E470D0A1A0A0000000D49484452, 1);  -- ảnh chính của S24 Ultra
GO

-- Cập nhật lại ImageId cho sản phẩm (lấy ảnh có IsPrimary = 1)
UPDATE p
SET ImageId = i.ImageId
FROM dbo.Products p
INNER JOIN dbo.ProductImages i ON i.ProductId = p.ProductId AND i.IsPrimary = 1;
GO

-- Bây giờ mới thêm FK ngược lại (an toàn vì dữ liệu đã hợp lệ)
ALTER TABLE dbo.ProductImages
ADD CONSTRAINT FK_ProductImages_Products
FOREIGN KEY (ProductId) REFERENCES dbo.Products(ProductId) ON DELETE CASCADE;
GO

PRINT 'Seed data completed successfully - ImageId đã được gán đúng';
GO

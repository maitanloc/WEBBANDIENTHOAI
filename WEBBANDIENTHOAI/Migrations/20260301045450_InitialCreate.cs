using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WEBBANDIENTHOAI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    LogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LogTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.LogId);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.CategoryId);
                });

            migrationBuilder.CreateTable(
                name: "CustomerTiers",
                columns: table => new
                {
                    TierId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TierName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MinSpending = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BonusMultiplier = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerTiers", x => x.TierId);
                });

            migrationBuilder.CreateTable(
                name: "ImportReceipts",
                columns: table => new
                {
                    ImportReceiptId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReceiptNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ImportDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SupplierName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TotalQuantity = table.Column<int>(type: "int", nullable: true),
                    TotalValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsFinalized = table.Column<bool>(type: "bit", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportReceipts", x => x.ImportReceiptId);
                });

            migrationBuilder.CreateTable(
                name: "OrderStatuses",
                columns: table => new
                {
                    StatusId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StatusName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderStatuses", x => x.StatusId);
                });

            migrationBuilder.CreateTable(
                name: "OTPCodes",
                columns: table => new
                {
                    OTPId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false),
                    Attempts = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OTPCodes", x => x.OTPId);
                });

            migrationBuilder.CreateTable(
                name: "PasswordResetTokens",
                columns: table => new
                {
                    TokenId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Token = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordResetTokens", x => x.TokenId);
                });

            migrationBuilder.CreateTable(
                name: "ProductStatuses",
                columns: table => new
                {
                    StatusId = table.Column<byte>(type: "tinyint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StatusName = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductStatuses", x => x.StatusId);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.RoleId);
                });

            migrationBuilder.CreateTable(
                name: "Vouchers",
                columns: table => new
                {
                    VoucherId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DiscountType = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxDiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MinOrderValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UsedCount = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vouchers", x => x.VoucherId);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    CustomerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PasswordHash = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CitizenID = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LoyaltyPoints = table.Column<int>(type: "int", nullable: false),
                    TierId = table.Column<int>(type: "int", nullable: false),
                    TotalSpent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(18,8)", nullable: true),
                    Longitude = table.Column<decimal>(type: "decimal(18,8)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.CustomerId);
                    table.ForeignKey(
                        name: "FK_Customers_CustomerTiers_TierId",
                        column: x => x.TierId,
                        principalTable: "CustomerTiers",
                        principalColumn: "TierId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_Users_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Carts",
                columns: table => new
                {
                    CartId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Carts", x => x.CartId);
                    table.ForeignKey(
                        name: "FK_Carts_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "CustomerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    OrderId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    ShippingAddress = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    PaymentMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    VoucherId = table.Column<int>(type: "int", nullable: true),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PointsEarned = table.Column<int>(type: "int", nullable: false),
                    PointsUsed = table.Column<int>(type: "int", nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(18,8)", nullable: true),
                    Longitude = table.Column<decimal>(type: "decimal(18,8)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.OrderId);
                    table.ForeignKey(
                        name: "FK_Orders_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "CustomerId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Orders_OrderStatuses_StatusId",
                        column: x => x.StatusId,
                        principalTable: "OrderStatuses",
                        principalColumn: "StatusId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Orders_Vouchers_VoucherId",
                        column: x => x.VoucherId,
                        principalTable: "Vouchers",
                        principalColumn: "VoucherId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SpinHistories",
                columns: table => new
                {
                    SpinId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    VoucherId = table.Column<int>(type: "int", nullable: true),
                    SpinAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SpinResult = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpinHistories", x => x.SpinId);
                    table.ForeignKey(
                        name: "FK_SpinHistories_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "CustomerId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SpinHistories_Vouchers_VoucherId",
                        column: x => x.VoucherId,
                        principalTable: "Vouchers",
                        principalColumn: "VoucherId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ExportReceipts",
                columns: table => new
                {
                    ExportReceiptId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReceiptNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ExportDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: true),
                    OrderId = table.Column<int>(type: "int", nullable: true),
                    TotalQuantity = table.Column<int>(type: "int", nullable: false),
                    TotalValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExportReceipts", x => x.ExportReceiptId);
                    table.ForeignKey(
                        name: "FK_ExportReceipts_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "CustomerId");
                    table.ForeignKey(
                        name: "FK_ExportReceipts_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "OrderId");
                    table.ForeignKey(
                        name: "FK_ExportReceipts_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserPointHistories",
                columns: table => new
                {
                    HistoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    OrderId = table.Column<int>(type: "int", nullable: true),
                    Points = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPointHistories", x => x.HistoryId);
                    table.ForeignKey(
                        name: "FK_UserPointHistories_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "CustomerId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserPointHistories_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "OrderId");
                });

            migrationBuilder.CreateTable(
                name: "UserVouchers",
                columns: table => new
                {
                    UserVoucherId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    VoucherId = table.Column<int>(type: "int", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OrderId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserVouchers", x => x.UserVoucherId);
                    table.ForeignKey(
                        name: "FK_UserVouchers_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "CustomerId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserVouchers_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "OrderId");
                    table.ForeignKey(
                        name: "FK_UserVouchers_Vouchers_VoucherId",
                        column: x => x.VoucherId,
                        principalTable: "Vouchers",
                        principalColumn: "VoucherId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CartDetails",
                columns: table => new
                {
                    CartDetailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CartId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SelectedOptions = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    OptionsPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartDetails", x => x.CartDetailId);
                    table.ForeignKey(
                        name: "FK_CartDetails_Carts_CartId",
                        column: x => x.CartId,
                        principalTable: "Carts",
                        principalColumn: "CartId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CrossSellRules",
                columns: table => new
                {
                    CrossSellRuleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TriggerProductId = table.Column<int>(type: "int", nullable: false),
                    SuggestedProductId = table.Column<int>(type: "int", nullable: false),
                    DiscountedPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DisplayMessage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrossSellRules", x => x.CrossSellRuleId);
                });

            migrationBuilder.CreateTable(
                name: "ExportReceiptDetails",
                columns: table => new
                {
                    ExportDetailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExportReceiptId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: true),
                    StockCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SnapshotSKU = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    SnapshotName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    SnapshotBrand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExportReceiptDetails", x => x.ExportDetailId);
                    table.ForeignKey(
                        name: "FK_ExportReceiptDetails_ExportReceipts_ExportReceiptId",
                        column: x => x.ExportReceiptId,
                        principalTable: "ExportReceipts",
                        principalColumn: "ExportReceiptId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ImportReceiptDetails",
                columns: table => new
                {
                    ImportDetailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ImportReceiptId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: true),
                    SnapshotSKU = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    SnapshotName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    SnapshotBrand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StockCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BatchNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportReceiptDetails", x => x.ImportDetailId);
                    table.ForeignKey(
                        name: "FK_ImportReceiptDetails_ImportReceipts_ImportReceiptId",
                        column: x => x.ImportReceiptId,
                        principalTable: "ImportReceipts",
                        principalColumn: "ImportReceiptId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Inventory",
                columns: table => new
                {
                    InventoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StockCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: true),
                    CurrentQuantity = table.Column<int>(type: "int", nullable: false),
                    MinimumQuantity = table.Column<int>(type: "int", nullable: false),
                    MaximumQuantity = table.Column<int>(type: "int", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inventory", x => x.InventoryId);
                });

            migrationBuilder.CreateTable(
                name: "InventoryHistory",
                columns: table => new
                {
                    HistoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InventoryId = table.Column<int>(type: "int", nullable: false),
                    ChangeQuantity = table.Column<int>(type: "int", nullable: false),
                    Operation = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ReferenceId = table.Column<int>(type: "int", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryHistory", x => x.HistoryId);
                    table.ForeignKey(
                        name: "FK_InventoryHistory_Inventory_InventoryId",
                        column: x => x.InventoryId,
                        principalTable: "Inventory",
                        principalColumn: "InventoryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LaptopConfigurations",
                columns: table => new
                {
                    ConfigurationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    CPU = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RAM = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Storage = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    GraphicsCard = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Battery = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OperatingSystem = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ScreenSize = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ScreenTechnology = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Resolution = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Ports = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Color = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Weight = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LaptopConfigurations", x => x.ConfigurationId);
                });

            migrationBuilder.CreateTable(
                name: "OrderDetails",
                columns: table => new
                {
                    OrderDetailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    SelectedOptions = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderDetails", x => x.OrderDetailId);
                    table.ForeignKey(
                        name: "FK_OrderDetails_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "OrderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PhoneConfigurations",
                columns: table => new
                {
                    ConfigurationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    CPU = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Cores = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Threads = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RAM = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    InternalStorage = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Battery = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OperatingSystem = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Screen = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ScreenTechnology = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Resolution = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Camera = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Ports = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Color = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhoneConfigurations", x => x.ConfigurationId);
                });

            migrationBuilder.CreateTable(
                name: "ProductImages",
                columns: table => new
                {
                    ImageId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ImagePath = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductImages", x => x.ImageId);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    SKU = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OldPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    StockCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Color = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Size = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ImageId = table.Column<int>(type: "int", nullable: true),
                    ShortDescription = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    StatusId = table.Column<byte>(type: "tinyint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.ProductId);
                    table.ForeignKey(
                        name: "FK_Products_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Products_ProductImages_ImageId",
                        column: x => x.ImageId,
                        principalTable: "ProductImages",
                        principalColumn: "ImageId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Products_ProductStatuses_StatusId",
                        column: x => x.StatusId,
                        principalTable: "ProductStatuses",
                        principalColumn: "StatusId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductOptions",
                columns: table => new
                {
                    ProductOptionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    GroupName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OptionName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    AdditionalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductOptions", x => x.ProductOptionId);
                    table.ForeignKey(
                        name: "FK_ProductOptions_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "CategoryId", "CategoryName", "CreatedAt", "Description" },
                values: new object[,]
                {
                    { 1, "Smartphones", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Các loại điện thoại thông minh" },
                    { 2, "Laptops", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Các loại máy tính xách tay" }
                });

            migrationBuilder.InsertData(
                table: "CustomerTiers",
                columns: new[] { "TierId", "BonusMultiplier", "Description", "MinSpending", "TierName" },
                values: new object[,]
                {
                    { 1, 1.0m, "Thành viên thường – nhận 1 điểm / 10.000đ", 0m, "Member" },
                    { 2, 1.2m, "Thành viên Bạc – Chi tiêu > 10Tr, hệ số điểm x1.2", 10000000m, "Silver" },
                    { 3, 1.5m, "Thành viên Vàng – Chi tiêu > 50Tr, hệ số điểm x1.5", 50000000m, "Gold" },
                    { 4, 2.0m, "Thành viên Kim Cương – Chi tiêu > 100Tr, hệ số điểm x2.0", 100000000m, "Diamond" }
                });

            migrationBuilder.InsertData(
                table: "OrderStatuses",
                columns: new[] { "StatusId", "Description", "StatusName" },
                values: new object[,]
                {
                    { 1, "Đơn hàng đang chờ xử lý", "Pending" },
                    { 2, "Đơn hàng đang được chuẩn bị", "Processing" },
                    { 3, "Đơn hàng đã được giao cho đơn vị vận chuyển", "Shipped" },
                    { 4, "Đơn hàng đã giao thành công", "Delivered" },
                    { 5, "Đơn hàng đã bị hủy", "Cancelled" },
                    { 6, "Đơn hàng đã được hoàn trả / hoàn tiền", "Returned" }
                });

            migrationBuilder.InsertData(
                table: "ProductStatuses",
                columns: new[] { "StatusId", "Description", "StatusName" },
                values: new object[,]
                {
                    { (byte)1, "Sản phẩm có sẵn, đang kinh doanh", "Available" },
                    { (byte)2, "Sản phẩm đã hết hàng", "Out of Stock" }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "RoleId", "Description", "RoleName" },
                values: new object[,]
                {
                    { 1, "Quản trị viên cấp cao nhất", "Admin" },
                    { 2, "Nhân viên quản lý", "Staff" },
                    { 3, "Khách hàng", "Customer" }
                });

            migrationBuilder.InsertData(
                table: "Vouchers",
                columns: new[] { "VoucherId", "Code", "CreatedAt", "Description", "DiscountType", "EndDate", "IsActive", "MaxDiscountAmount", "MinOrderValue", "Quantity", "StartDate", "UsedCount", "Value" },
                values: new object[,]
                {
                    { 1, "WELCOME50", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Voucher chào mừng thành viên mới - giảm 50.000đ", 2, new DateTime(2026, 12, 31, 23, 59, 59, 0, DateTimeKind.Utc), true, null, 500000m, 0, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0, 50000m },
                    { 2, "SALE10PCT", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Giảm 10% tối đa 100.000đ cho đơn từ 1.000.000đ", 1, new DateTime(2026, 12, 31, 23, 59, 59, 0, DateTimeKind.Utc), true, 100000m, 1000000m, 500, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0, 10m },
                    { 3, "SILVER100", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Ưu đãi hạng Bạc - giảm 100.000đ cho đơn từ 2.000.000đ", 2, new DateTime(2026, 12, 31, 23, 59, 59, 0, DateTimeKind.Utc), true, null, 2000000m, 200, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0, 100000m },
                    { 4, "GOLD15PCT", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Ưu đãi hạng Vàng - giảm 15% tối đa 300.000đ", 1, new DateTime(2026, 12, 31, 23, 59, 59, 0, DateTimeKind.Utc), true, 300000m, 5000000m, 100, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0, 15m },
                    { 5, "DIAMOND20", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Ưu đãi hạng Kim Cương - giảm 20% tối đa 1.000.000đ", 1, new DateTime(2026, 12, 31, 23, 59, 59, 0, DateTimeKind.Utc), true, 1000000m, 10000000m, 50, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0, 20m }
                });

            migrationBuilder.InsertData(
                table: "Customers",
                columns: new[] { "CustomerId", "Address", "CitizenID", "CreatedAt", "Email", "FullName", "IsActive", "Latitude", "Longitude", "LoyaltyPoints", "PasswordHash", "Phone", "TierId", "TotalSpent" },
                values: new object[] { 1, "123 Đường ABC, Quận 1, TP. HCM", "0123456789", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "customer@example.com", "Nguyễn Văn A", true, null, null, 0, new byte[] { 176, 65, 192, 174, 179, 91, 176, 250, 74, 166, 104, 202, 90, 146, 11, 89, 1, 150, 253, 175, 154, 0, 235, 133, 44, 155, 127, 77, 18, 60, 198, 214 }, "0987654321", 1, 0m });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "ProductId", "Brand", "CategoryId", "Color", "CreatedAt", "ImageId", "Name", "OldPrice", "Price", "SKU", "ShortDescription", "Size", "StatusId", "StockCode" },
                values: new object[,]
                {
                    { 1, "Apple", 1, "Titan Sa Mạc", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "iPhone 16 Pro Max 256GB", 36990000m, 34990000m, "IP16PM256", "Chip A18 Pro, Apple Intelligence, Màn hình 6.9 inch Super Retina XDR.", null, (byte)1, "SC-IP16PM256" },
                    { 2, "Apple", 2, "Space Black", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "MacBook Pro 14 inch M4", 45990000m, 42990000m, "MBP14M4", "Chip M4 tiên tiến, 16GB RAM, 512GB SSD, Màn hình Liquid Retina XDR.", null, (byte)1, "SC-MBP14M4" },
                    { 3, "Samsung", 1, "Xám Titan", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Samsung Galaxy S24 Ultra 256GB", 35990000m, 33990000m, "S24U256", "Galaxy AI, Khung viền Titan, Camera 200MP zoom quang 5x.", null, (byte)1, "SC-S24U256" },
                    { 4, "Dell", 2, "Platinum", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Dell XPS 16 (2026)", 69990000m, 65990000m, "DXPS162026", "Intel Core Ultra 9, 32GB RAM, 1TB SSD, RTX 4070, Màn hình 4K+ OLED Touch.", null, (byte)1, "SC-DXPS16" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "CreatedAt", "Email", "FullName", "IsActive", "PasswordHash", "RoleId", "Username" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin@example.com", "Administrator", true, new byte[] { 147, 186, 166, 224, 128, 110, 18, 16, 209, 138, 14, 3, 140, 101, 193, 223, 134, 242, 179, 21, 25, 20, 208, 182, 112, 183, 17, 199, 15, 222, 119, 93 }, 1, "admin" },
                    { 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "customer@example.com", "Nguyễn Văn A", true, new byte[] { 176, 65, 192, 174, 179, 91, 176, 250, 74, 166, 104, 202, 90, 146, 11, 89, 1, 150, 253, 175, 154, 0, 235, 133, 44, 155, 127, 77, 18, 60, 198, 214 }, 3, "customer" }
                });

            migrationBuilder.InsertData(
                table: "CrossSellRules",
                columns: new[] { "CrossSellRuleId", "CreatedAt", "DiscountedPrice", "DisplayMessage", "IsActive", "SuggestedProductId", "TriggerProductId" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 40990000m, "Hoàn thiện hệ sinh thái Apple 2026 của bạn!", true, 2, 1 },
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 61990000m, "Bộ đôi làm việc đa nhiệm siêu mạnh mẽ", true, 4, 3 }
                });

            migrationBuilder.InsertData(
                table: "Inventory",
                columns: new[] { "InventoryId", "CurrentQuantity", "LastUpdated", "Location", "MaximumQuantity", "MinimumQuantity", "ProductId", "StockCode" },
                values: new object[,]
                {
                    { 1, 100, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Kho A1", 1000, 15, 1, "SC-IP16PM256" },
                    { 2, 45, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Kho B2", 1000, 10, 2, "SC-MBP14M4" },
                    { 3, 80, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Kho A2", 1000, 20, 3, "SC-S24U256" },
                    { 4, 25, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Kho C1", 1000, 5, 4, "SC-DXPS16" }
                });

            migrationBuilder.InsertData(
                table: "LaptopConfigurations",
                columns: new[] { "ConfigurationId", "Battery", "CPU", "Color", "CreatedAt", "GraphicsCard", "OperatingSystem", "Ports", "ProductId", "RAM", "Resolution", "ScreenSize", "ScreenTechnology", "Storage", "Weight" },
                values: new object[,]
                {
                    { 1, null, "Apple M4 10-core", "Space Black", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "10-core GPU", "macOS Sequoia", null, 2, "16 GB", null, "14.2 inch", "Liquid Retina XDR display", "512 GB SSD", "1.55 kg" },
                    { 2, null, "Intel Core Ultra 9 185H", "Platinum", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "NVIDIA GeForce RTX 4070 8GB GDDR6", "Windows 11 Pro", null, 4, "32 GB LPDDR5x", null, "16.3 inch", "OLED Touch 4K+ (3840x2400)", "1 TB PCIe 4.0 NVMe", "2.13 kg" }
                });

            migrationBuilder.InsertData(
                table: "PhoneConfigurations",
                columns: new[] { "ConfigurationId", "Battery", "CPU", "Camera", "Color", "Cores", "CreatedAt", "InternalStorage", "OperatingSystem", "Ports", "ProductId", "RAM", "Resolution", "Screen", "ScreenTechnology", "Threads" },
                values: new object[,]
                {
                    { 1, "Li-Ion, sạc nhanh 45W", "Apple A18 Pro", "Chính 48 MP & Phụ 48 MP, 12 MP", "Titan Sa Mạc", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "256 GB", "iOS 18", null, 1, "8 GB", null, "6.9-inch Super Retina XDR", null, null },
                    { 2, "5000 mAh, sạc nhanh 45W", "Snapdragon 8 Gen 3 for Galaxy", "200 MP & Phụ 50 MP, 12 MP, 10 MP", "Xám Titan", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "256 GB", "Android 14, One UI 6.1", null, 3, "12 GB", null, "6.8-inch Dynamic AMOLED 2X", null, null }
                });

            migrationBuilder.InsertData(
                table: "ProductImages",
                columns: new[] { "ImageId", "CreatedAt", "ImagePath", "IsPrimary", "ProductId" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new byte[0], true, 1 },
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new byte[0], false, 1 },
                    { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new byte[0], true, 2 },
                    { 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new byte[0], true, 3 },
                    { 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new byte[0], true, 4 }
                });

            migrationBuilder.InsertData(
                table: "ProductOptions",
                columns: new[] { "ProductOptionId", "AdditionalPrice", "DisplayOrder", "GroupName", "IsActive", "OptionName", "ProductId" },
                values: new object[,]
                {
                    { 1, 0m, 1, "Màu sắc", true, "Titan Sa Mạc", 1 },
                    { 2, 0m, 2, "Màu sắc", true, "Titan Tự Nhiên", 1 },
                    { 3, 0m, 3, "Màu sắc", true, "Titan Đen", 1 },
                    { 4, 0m, 4, "Màu sắc", true, "Titan Trắng", 1 },
                    { 5, 0m, 1, "Bộ nhớ trong", true, "256GB", 1 },
                    { 6, 5000000m, 2, "Bộ nhớ trong", true, "512GB", 1 },
                    { 7, 11000000m, 3, "Bộ nhớ trong", true, "1TB", 1 },
                    { 8, 0m, 1, "RAM", true, "16GB", 2 },
                    { 9, 5000000m, 2, "RAM", true, "24GB", 2 },
                    { 10, 0m, 1, "SSD", true, "512GB", 2 },
                    { 11, 5000000m, 2, "SSD", true, "1TB", 2 },
                    { 16, 0m, 1, "Bộ nhớ trong", true, "256GB", 3 },
                    { 17, 3500000m, 2, "Bộ nhớ trong", true, "512GB", 3 },
                    { 18, 0m, 1, "Màn hình", true, "OLED Touch 4K+", 4 },
                    { 19, -4000000m, 2, "Màn hình", true, "FHD+ Non-Touch", 4 }
                });

            migrationBuilder.InsertData(
                table: "UserPointHistories",
                columns: new[] { "HistoryId", "CreatedAt", "CustomerId", "OrderId", "Points", "Reason" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), 1, null, 500, "Tích điểm đơn hàng #1001 - Giao thành công" },
                    { 2, new DateTime(2024, 2, 5, 0, 0, 0, 0, DateTimeKind.Utc), 1, null, 300, "Tích điểm đơn hàng #1002 - Giao thành công" },
                    { 3, new DateTime(2024, 2, 20, 0, 0, 0, 0, DateTimeKind.Utc), 1, null, -200, "Dùng điểm thanh toán đơn hàng #1003" },
                    { 4, new DateTime(2024, 3, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, null, 150, "Tích điểm đơn hàng #1004 - Giao thành công" },
                    { 5, new DateTime(2024, 3, 10, 0, 0, 0, 0, DateTimeKind.Utc), 1, null, 100, "Thưởng sự kiện Vòng quay may mắn" },
                    { 6, new DateTime(2024, 3, 15, 0, 0, 0, 0, DateTimeKind.Utc), 1, null, -100, "Hoàn trả điểm đơn hàng #1005 - Hoàn hàng" }
                });

            migrationBuilder.InsertData(
                table: "UserVouchers",
                columns: new[] { "UserVoucherId", "AssignedAt", "CustomerId", "IsUsed", "OrderId", "UsedAt", "VoucherId" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, false, null, null, 1 },
                    { 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, false, null, null, 2 },
                    { 3, new DateTime(2024, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, false, null, null, 3 },
                    { 4, new DateTime(2024, 2, 15, 0, 0, 0, 0, DateTimeKind.Utc), 1, false, null, null, 4 },
                    { 5, new DateTime(2024, 3, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, false, null, null, 5 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CartDetails_CartId",
                table: "CartDetails",
                column: "CartId");

            migrationBuilder.CreateIndex(
                name: "IX_CartDetails_ProductId",
                table: "CartDetails",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Carts_CustomerId",
                table: "Carts",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CrossSellRules_SuggestedProductId",
                table: "CrossSellRules",
                column: "SuggestedProductId");

            migrationBuilder.CreateIndex(
                name: "IX_CrossSellRules_TriggerProductId",
                table: "CrossSellRules",
                column: "TriggerProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Email",
                table: "Customers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_TierId",
                table: "Customers",
                column: "TierId");

            migrationBuilder.CreateIndex(
                name: "IX_ExportReceiptDetails_ExportReceiptId",
                table: "ExportReceiptDetails",
                column: "ExportReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_ExportReceiptDetails_ProductId",
                table: "ExportReceiptDetails",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ExportReceipts_CreatedByUserId",
                table: "ExportReceipts",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ExportReceipts_CustomerId",
                table: "ExportReceipts",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_ExportReceipts_OrderId",
                table: "ExportReceipts",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportReceiptDetails_ImportReceiptId",
                table: "ImportReceiptDetails",
                column: "ImportReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportReceiptDetails_ProductId",
                table: "ImportReceiptDetails",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Inventory_ProductId",
                table: "Inventory",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Inventory_StockCode",
                table: "Inventory",
                column: "StockCode");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryHistory_InventoryId",
                table: "InventoryHistory",
                column: "InventoryId");

            migrationBuilder.CreateIndex(
                name: "IX_LaptopConfigurations_ProductId",
                table: "LaptopConfigurations",
                column: "ProductId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_OrderId",
                table: "OrderDetails",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_ProductId",
                table: "OrderDetails",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CustomerId",
                table: "Orders",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderDate",
                table: "Orders",
                column: "OrderDate");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_StatusId",
                table: "Orders",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_VoucherId",
                table: "Orders",
                column: "VoucherId");

            migrationBuilder.CreateIndex(
                name: "IX_OTPCodes_CreatedAt",
                table: "OTPCodes",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_OTPCodes_Email_Code",
                table: "OTPCodes",
                columns: new[] { "Email", "Code" });

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetTokens_Email",
                table: "PasswordResetTokens",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetTokens_Token",
                table: "PasswordResetTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PhoneConfigurations_ProductId",
                table: "PhoneConfigurations",
                column: "ProductId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_ProductId",
                table: "ProductImages",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductOptions_ProductId",
                table: "ProductOptions",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_ImageId",
                table: "Products",
                column: "ImageId",
                unique: true,
                filter: "[ImageId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Products_SKU",
                table: "Products",
                column: "SKU");

            migrationBuilder.CreateIndex(
                name: "IX_Products_StatusId",
                table: "Products",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_SpinHistories_CustomerId_SpinAt",
                table: "SpinHistories",
                columns: new[] { "CustomerId", "SpinAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SpinHistories_VoucherId",
                table: "SpinHistories",
                column: "VoucherId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPointHistories_CustomerId",
                table: "UserPointHistories",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPointHistories_OrderId",
                table: "UserPointHistories",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserVouchers_CustomerId_VoucherId_IsUsed",
                table: "UserVouchers",
                columns: new[] { "CustomerId", "VoucherId", "IsUsed" },
                filter: "[IsUsed] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_UserVouchers_OrderId",
                table: "UserVouchers",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_UserVouchers_VoucherId",
                table: "UserVouchers",
                column: "VoucherId");

            migrationBuilder.CreateIndex(
                name: "IX_Vouchers_Code",
                table: "Vouchers",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CartDetails_Products_ProductId",
                table: "CartDetails",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CrossSellRules_Products_SuggestedProductId",
                table: "CrossSellRules",
                column: "SuggestedProductId",
                principalTable: "Products",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CrossSellRules_Products_TriggerProductId",
                table: "CrossSellRules",
                column: "TriggerProductId",
                principalTable: "Products",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExportReceiptDetails_Products_ProductId",
                table: "ExportReceiptDetails",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_ImportReceiptDetails_Products_ProductId",
                table: "ImportReceiptDetails",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_Inventory_Products_ProductId",
                table: "Inventory",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_LaptopConfigurations_Products_ProductId",
                table: "LaptopConfigurations",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetails_Products_ProductId",
                table: "OrderDetails",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PhoneConfigurations_Products_ProductId",
                table: "PhoneConfigurations",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductImages_Products_ProductId",
                table: "ProductImages",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductImages_Products_ProductId",
                table: "ProductImages");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "CartDetails");

            migrationBuilder.DropTable(
                name: "CrossSellRules");

            migrationBuilder.DropTable(
                name: "ExportReceiptDetails");

            migrationBuilder.DropTable(
                name: "ImportReceiptDetails");

            migrationBuilder.DropTable(
                name: "InventoryHistory");

            migrationBuilder.DropTable(
                name: "LaptopConfigurations");

            migrationBuilder.DropTable(
                name: "OrderDetails");

            migrationBuilder.DropTable(
                name: "OTPCodes");

            migrationBuilder.DropTable(
                name: "PasswordResetTokens");

            migrationBuilder.DropTable(
                name: "PhoneConfigurations");

            migrationBuilder.DropTable(
                name: "ProductOptions");

            migrationBuilder.DropTable(
                name: "SpinHistories");

            migrationBuilder.DropTable(
                name: "UserPointHistories");

            migrationBuilder.DropTable(
                name: "UserVouchers");

            migrationBuilder.DropTable(
                name: "Carts");

            migrationBuilder.DropTable(
                name: "ExportReceipts");

            migrationBuilder.DropTable(
                name: "ImportReceipts");

            migrationBuilder.DropTable(
                name: "Inventory");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "OrderStatuses");

            migrationBuilder.DropTable(
                name: "Vouchers");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "CustomerTiers");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "ProductImages");

            migrationBuilder.DropTable(
                name: "ProductStatuses");
        }
    }
}

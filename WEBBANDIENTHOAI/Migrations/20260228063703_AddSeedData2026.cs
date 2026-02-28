using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WEBBANDIENTHOAI.Migrations
{
    /// <inheritdoc />
    public partial class AddSeedData2026 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ProductOptions",
                keyColumn: "ProductOptionId",
                keyValue: 12);

            migrationBuilder.UpdateData(
                table: "CrossSellRules",
                keyColumn: "CrossSellRuleId",
                keyValue: 1,
                columns: new[] { "CreatedAt", "DiscountedPrice", "DisplayMessage" },
                values: new object[] { new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 40990000m, "Hoàn thiện hệ sinh thái Apple 2026 của bạn!" });

            migrationBuilder.UpdateData(
                table: "Inventory",
                keyColumn: "InventoryId",
                keyValue: 1,
                columns: new[] { "CurrentQuantity", "LastUpdated", "MinimumQuantity", "StockCode" },
                values: new object[] { 100, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 15, "SC-IP16PM256" });

            migrationBuilder.UpdateData(
                table: "Inventory",
                keyColumn: "InventoryId",
                keyValue: 2,
                columns: new[] { "CurrentQuantity", "LastUpdated", "MinimumQuantity", "StockCode" },
                values: new object[] { 45, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 10, "SC-MBP14M4" });

            migrationBuilder.UpdateData(
                table: "LaptopConfigurations",
                keyColumn: "ConfigurationId",
                keyValue: 1,
                columns: new[] { "CPU", "Color", "CreatedAt", "GraphicsCard", "OperatingSystem", "RAM" },
                values: new object[] { "Apple M4 10-core", "Space Black", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "10-core GPU", "macOS Sequoia", "16 GB" });

            migrationBuilder.UpdateData(
                table: "PhoneConfigurations",
                keyColumn: "ConfigurationId",
                keyValue: 1,
                columns: new[] { "Battery", "CPU", "Camera", "Color", "CreatedAt", "OperatingSystem", "Screen" },
                values: new object[] { "Li-Ion, sạc nhanh 45W", "Apple A18 Pro", "Chính 48 MP & Phụ 48 MP, 12 MP", "Titan Sa Mạc", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "iOS 18", "6.9-inch Super Retina XDR" });

            migrationBuilder.UpdateData(
                table: "ProductImages",
                keyColumn: "ImageId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "ProductImages",
                keyColumn: "ImageId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "ProductImages",
                keyColumn: "ImageId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "ProductOptions",
                keyColumn: "ProductOptionId",
                keyValue: 1,
                column: "OptionName",
                value: "Titan Sa Mạc");

            migrationBuilder.UpdateData(
                table: "ProductOptions",
                keyColumn: "ProductOptionId",
                keyValue: 2,
                column: "OptionName",
                value: "Titan Tự Nhiên");

            migrationBuilder.UpdateData(
                table: "ProductOptions",
                keyColumn: "ProductOptionId",
                keyValue: 6,
                column: "AdditionalPrice",
                value: 5000000m);

            migrationBuilder.UpdateData(
                table: "ProductOptions",
                keyColumn: "ProductOptionId",
                keyValue: 7,
                column: "AdditionalPrice",
                value: 11000000m);

            migrationBuilder.UpdateData(
                table: "ProductOptions",
                keyColumn: "ProductOptionId",
                keyValue: 8,
                column: "OptionName",
                value: "16GB");

            migrationBuilder.UpdateData(
                table: "ProductOptions",
                keyColumn: "ProductOptionId",
                keyValue: 9,
                column: "OptionName",
                value: "24GB");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 1,
                columns: new[] { "Color", "CreatedAt", "Name", "OldPrice", "Price", "SKU", "ShortDescription", "StockCode" },
                values: new object[] { "Titan Sa Mạc", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "iPhone 16 Pro Max 256GB", 36990000m, 34990000m, "IP16PM256", "Chip A18 Pro, Apple Intelligence, Màn hình 6.9 inch Super Retina XDR.", "SC-IP16PM256" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 2,
                columns: new[] { "Color", "CreatedAt", "Name", "OldPrice", "Price", "SKU", "ShortDescription", "StockCode" },
                values: new object[] { "Space Black", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "MacBook Pro 14 inch M4", 45990000m, 42990000m, "MBP14M4", "Chip M4 tiên tiến, 16GB RAM, 512GB SSD, Màn hình Liquid Retina XDR.", "SC-MBP14M4" });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "ProductId", "Brand", "CategoryId", "Color", "CreatedAt", "ImageId", "Name", "OldPrice", "Price", "SKU", "ShortDescription", "Size", "StatusId", "StockCode" },
                values: new object[,]
                {
                    { 3, "Samsung", 1, "Xám Titan", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Samsung Galaxy S24 Ultra 256GB", 35990000m, 33990000m, "S24U256", "Galaxy AI, Khung viền Titan, Camera 200MP zoom quang 5x.", null, (byte)1, "SC-S24U256" },
                    { 4, "Dell", 2, "Platinum", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Dell XPS 16 (2026)", 69990000m, 65990000m, "DXPS162026", "Intel Core Ultra 9, 32GB RAM, 1TB SSD, RTX 4070, Màn hình 4K+ OLED Touch.", null, (byte)1, "SC-DXPS16" }
                });

            migrationBuilder.InsertData(
                table: "CrossSellRules",
                columns: new[] { "CrossSellRuleId", "CreatedAt", "DiscountedPrice", "DisplayMessage", "IsActive", "SuggestedProductId", "TriggerProductId" },
                values: new object[] { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 61990000m, "Bộ đôi làm việc đa nhiệm siêu mạnh mẽ", true, 4, 3 });

            migrationBuilder.InsertData(
                table: "Inventory",
                columns: new[] { "InventoryId", "CurrentQuantity", "LastUpdated", "Location", "MaximumQuantity", "MinimumQuantity", "ProductId", "StockCode" },
                values: new object[,]
                {
                    { 3, 80, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Kho A2", 1000, 20, 3, "SC-S24U256" },
                    { 4, 25, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Kho C1", 1000, 5, 4, "SC-DXPS16" }
                });

            migrationBuilder.InsertData(
                table: "LaptopConfigurations",
                columns: new[] { "ConfigurationId", "Battery", "CPU", "Color", "CreatedAt", "GraphicsCard", "OperatingSystem", "Ports", "ProductId", "RAM", "Resolution", "ScreenSize", "ScreenTechnology", "Storage", "Weight" },
                values: new object[] { 2, null, "Intel Core Ultra 9 185H", "Platinum", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "NVIDIA GeForce RTX 4070 8GB GDDR6", "Windows 11 Pro", null, 4, "32 GB LPDDR5x", null, "16.3 inch", "OLED Touch 4K+ (3840x2400)", "1 TB PCIe 4.0 NVMe", "2.13 kg" });

            migrationBuilder.InsertData(
                table: "PhoneConfigurations",
                columns: new[] { "ConfigurationId", "Battery", "CPU", "Camera", "Color", "Cores", "CreatedAt", "InternalStorage", "OperatingSystem", "Ports", "ProductId", "RAM", "Resolution", "Screen", "ScreenTechnology", "Threads" },
                values: new object[] { 2, "5000 mAh, sạc nhanh 45W", "Snapdragon 8 Gen 3 for Galaxy", "200 MP & Phụ 50 MP, 12 MP, 10 MP", "Xám Titan", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "256 GB", "Android 14, One UI 6.1", null, 3, "12 GB", null, "6.8-inch Dynamic AMOLED 2X", null, null });

            migrationBuilder.InsertData(
                table: "ProductImages",
                columns: new[] { "ImageId", "CreatedAt", "ImagePath", "IsPrimary", "ProductId" },
                values: new object[,]
                {
                    { 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new byte[0], true, 3 },
                    { 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new byte[0], true, 4 }
                });

            migrationBuilder.InsertData(
                table: "ProductOptions",
                columns: new[] { "ProductOptionId", "AdditionalPrice", "DisplayOrder", "GroupName", "IsActive", "OptionName", "ProductId" },
                values: new object[,]
                {
                    { 16, 0m, 1, "Bộ nhớ trong", true, "256GB", 3 },
                    { 17, 3500000m, 2, "Bộ nhớ trong", true, "512GB", 3 },
                    { 18, 0m, 1, "Màn hình", true, "OLED Touch 4K+", 4 },
                    { 19, -4000000m, 2, "Màn hình", true, "FHD+ Non-Touch", 4 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "CrossSellRules",
                keyColumn: "CrossSellRuleId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Inventory",
                keyColumn: "InventoryId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Inventory",
                keyColumn: "InventoryId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "LaptopConfigurations",
                keyColumn: "ConfigurationId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "PhoneConfigurations",
                keyColumn: "ConfigurationId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ProductImages",
                keyColumn: "ImageId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "ProductImages",
                keyColumn: "ImageId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "ProductOptions",
                keyColumn: "ProductOptionId",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "ProductOptions",
                keyColumn: "ProductOptionId",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "ProductOptions",
                keyColumn: "ProductOptionId",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "ProductOptions",
                keyColumn: "ProductOptionId",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 4);

            migrationBuilder.UpdateData(
                table: "CrossSellRules",
                keyColumn: "CrossSellRuleId",
                keyValue: 1,
                columns: new[] { "CreatedAt", "DiscountedPrice", "DisplayMessage" },
                values: new object[] { new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 45990000m, "Hoàn thiện bộ đôi Apple của bạn!" });

            migrationBuilder.UpdateData(
                table: "Inventory",
                keyColumn: "InventoryId",
                keyValue: 1,
                columns: new[] { "CurrentQuantity", "LastUpdated", "MinimumQuantity", "StockCode" },
                values: new object[] { 50, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 10, "SC-IP15P256" });

            migrationBuilder.UpdateData(
                table: "Inventory",
                keyColumn: "InventoryId",
                keyValue: 2,
                columns: new[] { "CurrentQuantity", "LastUpdated", "MinimumQuantity", "StockCode" },
                values: new object[] { 30, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 5, "SC-MBP14M3" });

            migrationBuilder.UpdateData(
                table: "LaptopConfigurations",
                keyColumn: "ConfigurationId",
                keyValue: 1,
                columns: new[] { "CPU", "Color", "CreatedAt", "GraphicsCard", "OperatingSystem", "RAM" },
                values: new object[] { "Apple M3 Pro 11-core", "Space Gray", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "14-core GPU", "macOS Sonoma", "18 GB" });

            migrationBuilder.UpdateData(
                table: "PhoneConfigurations",
                keyColumn: "ConfigurationId",
                keyValue: 1,
                columns: new[] { "Battery", "CPU", "Camera", "Color", "CreatedAt", "OperatingSystem", "Screen" },
                values: new object[] { "Li-Ion, sạc nhanh", "Apple A17 Pro", "Chính 48 MP & Phụ 12 MP, 12 MP", "Titan tự nhiên", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "iOS 17", "6.1-inch Super Retina XDR" });

            migrationBuilder.UpdateData(
                table: "ProductImages",
                keyColumn: "ImageId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "ProductImages",
                keyColumn: "ImageId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "ProductImages",
                keyColumn: "ImageId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "ProductOptions",
                keyColumn: "ProductOptionId",
                keyValue: 1,
                column: "OptionName",
                value: "Titan Tự Nhiên");

            migrationBuilder.UpdateData(
                table: "ProductOptions",
                keyColumn: "ProductOptionId",
                keyValue: 2,
                column: "OptionName",
                value: "Titan Xanh");

            migrationBuilder.UpdateData(
                table: "ProductOptions",
                keyColumn: "ProductOptionId",
                keyValue: 6,
                column: "AdditionalPrice",
                value: 3000000m);

            migrationBuilder.UpdateData(
                table: "ProductOptions",
                keyColumn: "ProductOptionId",
                keyValue: 7,
                column: "AdditionalPrice",
                value: 6000000m);

            migrationBuilder.UpdateData(
                table: "ProductOptions",
                keyColumn: "ProductOptionId",
                keyValue: 8,
                column: "OptionName",
                value: "18GB");

            migrationBuilder.UpdateData(
                table: "ProductOptions",
                keyColumn: "ProductOptionId",
                keyValue: 9,
                column: "OptionName",
                value: "36GB");

            migrationBuilder.InsertData(
                table: "ProductOptions",
                columns: new[] { "ProductOptionId", "AdditionalPrice", "DisplayOrder", "GroupName", "IsActive", "OptionName", "ProductId" },
                values: new object[] { 12, 10000000m, 3, "SSD", true, "2TB", 2 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 1,
                columns: new[] { "Color", "CreatedAt", "Name", "OldPrice", "Price", "SKU", "ShortDescription", "StockCode" },
                values: new object[] { "Titan tự nhiên", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "iPhone 15 Pro 256GB", 30990000m, 28990000m, "IP15P256", "Chip A17 Pro, Màn hình Super Retina XDR, Camera Pro 48MP.", "SC-IP15P256" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 2,
                columns: new[] { "Color", "CreatedAt", "Name", "OldPrice", "Price", "SKU", "ShortDescription", "StockCode" },
                values: new object[] { "Space Gray", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "MacBook Pro 14 inch M3", 52990000m, 49990000m, "MBP14M3", "Chip M3 Pro, 18GB RAM, 512GB SSD, Màn hình Liquid Retina XDR.", "SC-MBP14M3" });
        }
    }
}

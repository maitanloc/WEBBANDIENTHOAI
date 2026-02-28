using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WEBBANDIENTHOAI.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLoyaltyToSpending : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinPoints",
                table: "CustomerTiers");

            migrationBuilder.AddColumn<decimal>(
                name: "MinSpending",
                table: "CustomerTiers",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalSpent",
                table: "Customers",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.UpdateData(
                table: "CustomerTiers",
                keyColumn: "TierId",
                keyValue: 1,
                column: "MinSpending",
                value: 0m);

            migrationBuilder.UpdateData(
                table: "CustomerTiers",
                keyColumn: "TierId",
                keyValue: 2,
                columns: new[] { "Description", "MinSpending" },
                values: new object[] { "Thành viên Bạc – Chi tiêu > 10Tr, hệ số điểm x1.2", 10000000m });

            migrationBuilder.UpdateData(
                table: "CustomerTiers",
                keyColumn: "TierId",
                keyValue: 3,
                columns: new[] { "Description", "MinSpending" },
                values: new object[] { "Thành viên Vàng – Chi tiêu > 50Tr, hệ số điểm x1.5", 50000000m });

            migrationBuilder.UpdateData(
                table: "CustomerTiers",
                keyColumn: "TierId",
                keyValue: 4,
                columns: new[] { "Description", "MinSpending" },
                values: new object[] { "Thành viên Kim Cương – Chi tiêu > 100Tr, hệ số điểm x2.0", 100000000m });

            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: 1,
                column: "TotalSpent",
                value: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinSpending",
                table: "CustomerTiers");

            migrationBuilder.DropColumn(
                name: "TotalSpent",
                table: "Customers");

            migrationBuilder.AddColumn<int>(
                name: "MinPoints",
                table: "CustomerTiers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "CustomerTiers",
                keyColumn: "TierId",
                keyValue: 1,
                column: "MinPoints",
                value: 0);

            migrationBuilder.UpdateData(
                table: "CustomerTiers",
                keyColumn: "TierId",
                keyValue: 2,
                columns: new[] { "Description", "MinPoints" },
                values: new object[] { "Thành viên Bạc – nhân hệ số điểm x1.2", 1000 });

            migrationBuilder.UpdateData(
                table: "CustomerTiers",
                keyColumn: "TierId",
                keyValue: 3,
                columns: new[] { "Description", "MinPoints" },
                values: new object[] { "Thành viên Vàng – nhân hệ số điểm x1.5", 5000 });

            migrationBuilder.UpdateData(
                table: "CustomerTiers",
                keyColumn: "TierId",
                keyValue: 4,
                columns: new[] { "Description", "MinPoints" },
                values: new object[] { "Thành viên Kim Cương – nhân hệ số điểm x2.0", 10000 });
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WEBBANDIENTHOAI.Migrations
{
    /// <inheritdoc />
    public partial class AddSpinHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.CreateIndex(
                name: "IX_SpinHistories_CustomerId_SpinAt",
                table: "SpinHistories",
                columns: new[] { "CustomerId", "SpinAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SpinHistories_VoucherId",
                table: "SpinHistories",
                column: "VoucherId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SpinHistories");
        }
    }
}

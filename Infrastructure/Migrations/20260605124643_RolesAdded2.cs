using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RolesAdded2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "634026d1-cfd6-4701-8a58-70a24b0ea2c4");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "af9cd3a0-0307-48d8-b5c8-507336e98cb1");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "05a91d31-b845-4874-a457-0742b3583021", null, "Admin", "ADMIN" },
                    { "eb2be54e-4575-4dad-9fae-9af4441ccc19", null, "Customer", "CUSTOMER" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "05a91d31-b845-4874-a457-0742b3583021");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "eb2be54e-4575-4dad-9fae-9af4441ccc19");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "634026d1-cfd6-4701-8a58-70a24b0ea2c4", null, "Admin", "ADMIN" },
                    { "af9cd3a0-0307-48d8-b5c8-507336e98cb1", null, "Customer", "CUSTOMER" }
                });
        }
    }
}

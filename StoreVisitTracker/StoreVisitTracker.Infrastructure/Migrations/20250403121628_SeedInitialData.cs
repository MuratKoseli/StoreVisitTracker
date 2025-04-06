using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace StoreVisitTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserName",
                table: "Users",
                newName: "Username");

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Category", "CreatedAt", "Name" },
                values: new object[,]
                {
                    { 1, "Groceries", new DateTime(2025, 4, 3, 12, 16, 28, 474, DateTimeKind.Utc).AddTicks(6832), "Chocolate" },
                    { 2, "Electronics", new DateTime(2025, 4, 3, 12, 16, 28, 474, DateTimeKind.Utc).AddTicks(6833), "TV" },
                    { 3, "Clothing", new DateTime(2025, 4, 3, 12, 16, 28, 474, DateTimeKind.Utc).AddTicks(6835), "Shoes" },
                    { 4, "Home & Kitchen", new DateTime(2025, 4, 3, 12, 16, 28, 474, DateTimeKind.Utc).AddTicks(6836), "Knife Set" },
                    { 5, "Sports", new DateTime(2025, 4, 3, 12, 16, 28, 474, DateTimeKind.Utc).AddTicks(6838), "Basketball" }
                });

            migrationBuilder.InsertData(
                table: "Stores",
                columns: new[] { "Id", "CreatedAt", "Location", "Name" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 4, 3, 12, 16, 28, 474, DateTimeKind.Utc).AddTicks(6801), "City A", "Store A" },
                    { 2, new DateTime(2025, 4, 3, 12, 16, 28, 474, DateTimeKind.Utc).AddTicks(6803), "City B", "Store B" },
                    { 3, new DateTime(2025, 4, 3, 12, 16, 28, 474, DateTimeKind.Utc).AddTicks(6804), "City C", "Store C" },
                    { 4, new DateTime(2025, 4, 3, 12, 16, 28, 474, DateTimeKind.Utc).AddTicks(6806), "City D", "Store D" },
                    { 5, new DateTime(2025, 4, 3, 12, 16, 28, 474, DateTimeKind.Utc).AddTicks(6807), "City E", "Store E" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Role", "Username" },
                values: new object[,]
                {
                    { 1, "Admin", "admin1" },
                    { 2, "Admin", "admin2" },
                    { 3, "Standard", "user1" },
                    { 4, "Standard", "user2" },
                    { 5, "Standard", "user3" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Stores",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Stores",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Stores",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Stores",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Stores",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.RenameColumn(
                name: "Username",
                table: "Users",
                newName: "UserName");
        }
    }
}

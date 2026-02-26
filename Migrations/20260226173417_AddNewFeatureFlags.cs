using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yuka2Back.Migrations
{
    /// <inheritdoc />
    public partial class AddNewFeatureFlags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AccountDeletionEnabled",
                table: "AppConfigs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "BiometricAuthEnabled",
                table: "AppConfigs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HistoryEnabled",
                table: "AppConfigs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "OpenFoodFactsEnabled",
                table: "AppConfigs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PasswordResetEnabled",
                table: "AppConfigs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 17, 34, 17, 383, DateTimeKind.Utc).AddTicks(1941), new DateTime(2026, 2, 26, 17, 34, 17, 383, DateTimeKind.Utc).AddTicks(1945) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 17, 34, 17, 383, DateTimeKind.Utc).AddTicks(1959), new DateTime(2026, 2, 26, 17, 34, 17, 383, DateTimeKind.Utc).AddTicks(1959) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 17, 34, 17, 383, DateTimeKind.Utc).AddTicks(1963), new DateTime(2026, 2, 26, 17, 34, 17, 383, DateTimeKind.Utc).AddTicks(1963) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 17, 34, 17, 383, DateTimeKind.Utc).AddTicks(1967), new DateTime(2026, 2, 26, 17, 34, 17, 383, DateTimeKind.Utc).AddTicks(1967) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 17, 34, 17, 383, DateTimeKind.Utc).AddTicks(1970), new DateTime(2026, 2, 26, 17, 34, 17, 383, DateTimeKind.Utc).AddTicks(1971) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 17, 34, 17, 383, DateTimeKind.Utc).AddTicks(1974), new DateTime(2026, 2, 26, 17, 34, 17, 383, DateTimeKind.Utc).AddTicks(1974) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 17, 34, 17, 383, DateTimeKind.Utc).AddTicks(1977), new DateTime(2026, 2, 26, 17, 34, 17, 383, DateTimeKind.Utc).AddTicks(1978) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 17, 34, 17, 383, DateTimeKind.Utc).AddTicks(1982), new DateTime(2026, 2, 26, 17, 34, 17, 383, DateTimeKind.Utc).AddTicks(1983) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 17, 34, 17, 383, DateTimeKind.Utc).AddTicks(1986), new DateTime(2026, 2, 26, 17, 34, 17, 383, DateTimeKind.Utc).AddTicks(1986) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 17, 34, 17, 383, DateTimeKind.Utc).AddTicks(1990), new DateTime(2026, 2, 26, 17, 34, 17, 383, DateTimeKind.Utc).AddTicks(1990) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountDeletionEnabled",
                table: "AppConfigs");

            migrationBuilder.DropColumn(
                name: "BiometricAuthEnabled",
                table: "AppConfigs");

            migrationBuilder.DropColumn(
                name: "HistoryEnabled",
                table: "AppConfigs");

            migrationBuilder.DropColumn(
                name: "OpenFoodFactsEnabled",
                table: "AppConfigs");

            migrationBuilder.DropColumn(
                name: "PasswordResetEnabled",
                table: "AppConfigs");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 17, 15, 31, 545, DateTimeKind.Utc).AddTicks(6850), new DateTime(2026, 2, 26, 17, 15, 31, 545, DateTimeKind.Utc).AddTicks(6854) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 17, 15, 31, 545, DateTimeKind.Utc).AddTicks(6872), new DateTime(2026, 2, 26, 17, 15, 31, 545, DateTimeKind.Utc).AddTicks(6872) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 17, 15, 31, 545, DateTimeKind.Utc).AddTicks(6876), new DateTime(2026, 2, 26, 17, 15, 31, 545, DateTimeKind.Utc).AddTicks(6876) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 17, 15, 31, 545, DateTimeKind.Utc).AddTicks(6879), new DateTime(2026, 2, 26, 17, 15, 31, 545, DateTimeKind.Utc).AddTicks(6880) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 17, 15, 31, 545, DateTimeKind.Utc).AddTicks(6883), new DateTime(2026, 2, 26, 17, 15, 31, 545, DateTimeKind.Utc).AddTicks(6883) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 17, 15, 31, 545, DateTimeKind.Utc).AddTicks(6886), new DateTime(2026, 2, 26, 17, 15, 31, 545, DateTimeKind.Utc).AddTicks(6887) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 17, 15, 31, 545, DateTimeKind.Utc).AddTicks(6890), new DateTime(2026, 2, 26, 17, 15, 31, 545, DateTimeKind.Utc).AddTicks(6890) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 17, 15, 31, 545, DateTimeKind.Utc).AddTicks(6894), new DateTime(2026, 2, 26, 17, 15, 31, 545, DateTimeKind.Utc).AddTicks(6894) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 17, 15, 31, 545, DateTimeKind.Utc).AddTicks(6897), new DateTime(2026, 2, 26, 17, 15, 31, 545, DateTimeKind.Utc).AddTicks(6897) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 17, 15, 31, 545, DateTimeKind.Utc).AddTicks(6901), new DateTime(2026, 2, 26, 17, 15, 31, 545, DateTimeKind.Utc).AddTicks(6901) });
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Yuka2Back.Migrations
{
    /// <inheritdoc />
    public partial class APpConfigs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdminUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApiLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Method = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Path = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    StatusCode = table.Column<int>(type: "int", nullable: false),
                    DurationMs = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaintenanceMode = table.Column<bool>(type: "bit", nullable: false),
                    MaintenanceMessage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MinAppVersion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    LatestAppVersion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ForceUpdateMessage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SessionTimeoutMinutes = table.Column<int>(type: "int", nullable: false),
                    ScanEnabled = table.Column<bool>(type: "bit", nullable: false),
                    SearchEnabled = table.Column<bool>(type: "bit", nullable: false),
                    FavoritesEnabled = table.Column<bool>(type: "bit", nullable: false),
                    RegistrationEnabled = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    DeviceId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DeviceModel = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DeviceOS = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AppVersion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Barcode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NutriScore = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: true),
                    NovaGroup = table.Column<int>(type: "int", nullable: true),
                    HealthScore = table.Column<int>(type: "int", nullable: true),
                    Calories = table.Column<double>(type: "float", nullable: true),
                    Fat = table.Column<double>(type: "float", nullable: true),
                    SaturatedFat = table.Column<double>(type: "float", nullable: true),
                    Sugars = table.Column<double>(type: "float", nullable: true),
                    Salt = table.Column<double>(type: "float", nullable: true),
                    Fiber = table.Column<double>(type: "float", nullable: true),
                    Proteins = table.Column<double>(type: "float", nullable: true),
                    Carbohydrates = table.Column<double>(type: "float", nullable: true),
                    Ingredients = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                    Allergens = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Categories = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Quantity = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsOrganic = table.Column<bool>(type: "bit", nullable: false),
                    IsPalmOilFree = table.Column<bool>(type: "bit", nullable: false),
                    IsVegan = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AvatarUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DietType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Allergies = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DietaryGoals = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NotificationsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    DarkModeEnabled = table.Column<bool>(type: "bit", nullable: false),
                    Language = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    FailedLoginAttempts = table.Column<int>(type: "int", nullable: false),
                    LockoutEnd = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsEmailVerified = table.Column<bool>(type: "bit", nullable: false),
                    EmailVerificationToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PasswordResetToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PasswordResetTokenExpiry = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalScans = table.Column<int>(type: "int", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AnalyticsEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    EventType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EventData = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DeviceId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalyticsEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnalyticsEvents_AppSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "AppSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PageViews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    PageName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    EnteredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExitedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PageViews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PageViews_AppSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "AppSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FavoriteProducts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FavoriteProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FavoriteProducts_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FavoriteProducts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Token = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByIp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RevokedByIp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReplacedByToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RevokeReason = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScanHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ScannedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScanHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScanHistories_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ScanHistories_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Allergens", "Barcode", "Brand", "Calories", "Carbohydrates", "Categories", "CreatedAt", "Fat", "Fiber", "HealthScore", "ImageUrl", "Ingredients", "IsOrganic", "IsPalmOilFree", "IsVegan", "Name", "NovaGroup", "NutriScore", "Proteins", "Quantity", "Salt", "SaturatedFat", "Sugars", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "Lait, Soja, Fruits à coque", "3017620422003", "Ferrero", 539.0, 57.5, "Pâtes à tartiner", new DateTime(2026, 2, 26, 17, 15, 31, 545, DateTimeKind.Utc).AddTicks(6850), 30.899999999999999, 0.0, 15, null, "Sucre, huile de palme, noisettes 13%, cacao maigre 7.4%, lait écrémé en poudre 6.6%, lactosérum en poudre, émulsifiants : lécithines (soja), vanilline", false, false, false, "Nutella", 4, "E", 6.2999999999999998, "400g", 0.107, 10.6, 56.299999999999997, new DateTime(2026, 2, 26, 17, 15, 31, 545, DateTimeKind.Utc).AddTicks(6854) },
                    { 2, "", "3175680011480", "Volvic", 0.0, 0.0, "Boissons, Eaux", new DateTime(2026, 2, 26, 17, 15, 31, 545, DateTimeKind.Utc).AddTicks(6872), 0.0, 0.0, 100, null, "Eau minérale naturelle", false, true, true, "Eau minérale naturelle", 1, "A", 0.0, "1.5L", 0.0, 0.0, 0.0, new DateTime(2026, 2, 26, 17, 15, 31, 545, DateTimeKind.Utc).AddTicks(6872) },
                    { 3, "Lait", "3228857000166", "Président", 299.0, 0.5, "Fromages", new DateTime(2026, 2, 26, 17, 15, 31, 545, DateTimeKind.Utc).AddTicks(6876), 24.0, 0.0, 35, null, "Lait pasteurisé, sel, ferments lactiques, coagulant", false, true, false, "Camembert", 3, "D", 20.0, "250g", 1.6000000000000001, 16.600000000000001, 0.5, new DateTime(2026, 2, 26, 17, 15, 31, 545, DateTimeKind.Utc).AddTicks(6876) },
                    { 4, "", "3560070472734", "Carrefour Bio", 19.0, 1.8, "Légumes, Salades", new DateTime(2026, 2, 26, 17, 15, 31, 545, DateTimeKind.Utc).AddTicks(6879), 0.29999999999999999, 1.8, 92, null, "Salade mêlée issue de l'agriculture biologique", true, true, true, "Salade mêlée", 1, "A", 1.5, "150g", 0.02, 0.0, 0.59999999999999998, new DateTime(2026, 2, 26, 17, 15, 31, 545, DateTimeKind.Utc).AddTicks(6880) },
                    { 5, "Gluten, Lait, Soja", "3017760000109", "LU", 468.0, 70.0, "Biscuits, Snacks sucrés", new DateTime(2026, 2, 26, 17, 15, 31, 545, DateTimeKind.Utc).AddTicks(6883), 17.0, 2.2999999999999998, 22, null, "Farine de blé 47%, sucre, huiles végétales (palme, colza), cacao maigre en poudre 5.1%, sirop de glucose, amidon de blé, poudre à lever", false, false, false, "Prince Chocolat", 4, "D", 6.2000000000000002, "300g", 0.68000000000000005, 8.1999999999999993, 32.0, new DateTime(2026, 2, 26, 17, 15, 31, 545, DateTimeKind.Utc).AddTicks(6883) },
                    { 6, "", "3274080005003", "Andros", 52.0, 11.9, "Compotes, Fruits", new DateTime(2026, 2, 26, 17, 15, 31, 545, DateTimeKind.Utc).AddTicks(6886), 0.10000000000000001, 1.7, 78, null, "Pommes 100%", false, true, true, "Compote Pomme", 1, "A", 0.29999999999999999, "750g", 0.0, 0.0, 10.699999999999999, new DateTime(2026, 2, 26, 17, 15, 31, 545, DateTimeKind.Utc).AddTicks(6887) },
                    { 7, "Lait", "3256540001305", "Danone", 46.0, 4.7000000000000002, "Produits laitiers, Yaourts", new DateTime(2026, 2, 26, 17, 15, 31, 545, DateTimeKind.Utc).AddTicks(6890), 1.0, 0.0, 85, null, "Lait écrémé, ferments lactiques", false, true, false, "Yaourt Nature", 1, "A", 4.2000000000000002, "4x125g", 0.13, 0.69999999999999996, 4.7000000000000002, new DateTime(2026, 2, 26, 17, 15, 31, 545, DateTimeKind.Utc).AddTicks(6890) },
                    { 8, "", "3560070452712", "Lay's", 536.0, 50.0, "Chips, Snacks salés", new DateTime(2026, 2, 26, 17, 15, 31, 545, DateTimeKind.Utc).AddTicks(6894), 35.0, 4.2999999999999998, 38, null, "Pommes de terre, huile de tournesol 33%, sel", false, true, true, "Chips nature", 3, "C", 6.5, "150g", 1.3, 3.0, 0.5, new DateTime(2026, 2, 26, 17, 15, 31, 545, DateTimeKind.Utc).AddTicks(6894) },
                    { 9, "", "3073781155211", "Bonduelle", 57.0, 6.4000000000000004, "Conserves, Légumes", new DateTime(2026, 2, 26, 17, 15, 31, 545, DateTimeKind.Utc).AddTicks(6897), 2.2999999999999998, 1.6000000000000001, 82, null, "Tomates 38%, courgettes 21%, poivrons rouges 12%, aubergines 9%, oignons, huile de tournesol, sel, ail, basilic, thym, poivre", false, true, true, "Ratatouille cuisinée", 3, "A", 1.1000000000000001, "660g", 0.65000000000000002, 0.29999999999999999, 4.2999999999999998, new DateTime(2026, 2, 26, 17, 15, 31, 545, DateTimeKind.Utc).AddTicks(6897) },
                    { 10, "Lait, Soja", "3046920028363", "Lindt", 545.0, 33.0, "Chocolats", new DateTime(2026, 2, 26, 17, 15, 31, 545, DateTimeKind.Utc).AddTicks(6901), 41.0, 11.0, 55, null, "Pâte de cacao, sucre, beurre de cacao, vanille", false, true, false, "Chocolat noir 70%", 3, "C", 9.3000000000000007, "100g", 0.02, 25.0, 26.0, new DateTime(2026, 2, 26, 17, 15, 31, 545, DateTimeKind.Utc).AddTicks(6901) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdminUsers_Email",
                table: "AdminUsers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdminUsers_Username",
                table: "AdminUsers",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsEvents_CreatedAt",
                table: "AnalyticsEvents",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsEvents_DeviceId",
                table: "AnalyticsEvents",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsEvents_EventType",
                table: "AnalyticsEvents",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsEvents_SessionId",
                table: "AnalyticsEvents",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsEvents_UserId",
                table: "AnalyticsEvents",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ApiLogs_CreatedAt",
                table: "ApiLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ApiLogs_Path",
                table: "ApiLogs",
                column: "Path");

            migrationBuilder.CreateIndex(
                name: "IX_ApiLogs_StatusCode",
                table: "ApiLogs",
                column: "StatusCode");

            migrationBuilder.CreateIndex(
                name: "IX_ApiLogs_UserId",
                table: "ApiLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AppSessions_DeviceId",
                table: "AppSessions",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_AppSessions_StartedAt",
                table: "AppSessions",
                column: "StartedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AppSessions_UserId",
                table: "AppSessions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_FavoriteProducts_ProductId",
                table: "FavoriteProducts",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_FavoriteProducts_UserId_ProductId",
                table: "FavoriteProducts",
                columns: new[] { "UserId", "ProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PageViews_EnteredAt",
                table: "PageViews",
                column: "EnteredAt");

            migrationBuilder.CreateIndex(
                name: "IX_PageViews_PageName",
                table: "PageViews",
                column: "PageName");

            migrationBuilder.CreateIndex(
                name: "IX_PageViews_SessionId",
                table: "PageViews",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_PageViews_UserId",
                table: "PageViews",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Barcode",
                table: "Products",
                column: "Barcode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_Token",
                table: "RefreshTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ScanHistories_ProductId",
                table: "ScanHistories",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ScanHistories_UserId",
                table: "ScanHistories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdminUsers");

            migrationBuilder.DropTable(
                name: "AnalyticsEvents");

            migrationBuilder.DropTable(
                name: "ApiLogs");

            migrationBuilder.DropTable(
                name: "AppConfigs");

            migrationBuilder.DropTable(
                name: "FavoriteProducts");

            migrationBuilder.DropTable(
                name: "PageViews");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "ScanHistories");

            migrationBuilder.DropTable(
                name: "AppSessions");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}

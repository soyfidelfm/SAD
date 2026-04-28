using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SADWebApi.Migrations
{
    /// <inheritdoc />
    public partial class InitPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "sales");

            migrationBuilder.EnsureSchema(
                name: "catalog");

            migrationBuilder.EnsureSchema(
                name: "auth");

            migrationBuilder.CreateTable(
                name: "CreditCardProducts",
                schema: "catalog",
                columns: table => new
                {
                    CreditCardProductId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductCode = table.Column<string>(type: "character varying(50)", unicode: false, maxLength: 50, nullable: false),
                    ProductName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditCardProducts", x => x.CreditCardProductId);
                });

            migrationBuilder.CreateTable(
                name: "IdentityProviders",
                schema: "catalog",
                columns: table => new
                {
                    IdentityProviderId = table.Column<byte>(type: "smallint", nullable: false),
                    ProviderCode = table.Column<string>(type: "character varying(20)", unicode: false, maxLength: 20, nullable: false),
                    ProviderName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdentityProviders", x => x.IdentityProviderId);
                });

            migrationBuilder.CreateTable(
                name: "MembershipProducts",
                schema: "catalog",
                columns: table => new
                {
                    MembershipProductId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductCode = table.Column<string>(type: "character varying(50)", unicode: false, maxLength: 50, nullable: false),
                    ProductName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MembershipProducts", x => x.MembershipProductId);
                });

            migrationBuilder.CreateTable(
                name: "Sales",
                schema: "sales",
                columns: table => new
                {
                    SaleId = table.Column<Guid>(type: "uuid", nullable: false),
                    StoreId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SaleDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Subtotal = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    Tax = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    PaymentMethod = table.Column<string>(type: "character varying(30)", unicode: false, maxLength: 30, nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sales", x => x.SaleId);
                });

            migrationBuilder.CreateTable(
                name: "SaleStatus",
                schema: "catalog",
                columns: table => new
                {
                    StatusId = table.Column<byte>(type: "smallint", nullable: false),
                    StatusCode = table.Column<string>(type: "character varying(20)", unicode: false, maxLength: 20, nullable: false),
                    StatusName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsFinal = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaleStatus", x => x.StatusId);
                });

            migrationBuilder.CreateTable(
                name: "Stores",
                schema: "catalog",
                columns: table => new
                {
                    StoreId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StoreNumber = table.Column<int>(type: "integer", nullable: false),
                    StoreName = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stores", x => x.StoreId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "auth",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DisplayName = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LastLoginAtUtc = table.Column<DateTime>(type: "timestamp(0) with time zone", precision: 0, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp(0) with time zone", precision: 0, nullable: false),
                    StoreId = table.Column<int>(type: "integer", nullable: true),
                    Anumber = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "CreditCardApplications",
                schema: "sales",
                columns: table => new
                {
                    CreditCardApplicationId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    StoreId = table.Column<int>(type: "integer", nullable: false),
                    CreditCardProductId = table.Column<int>(type: "integer", nullable: false),
                    StatusId = table.Column<byte>(type: "smallint", nullable: false),
                    SubmittedAtUtc = table.Column<DateTime>(type: "timestamp(0) with time zone", precision: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditCardApplications", x => x.CreditCardApplicationId);
                    table.ForeignKey(
                        name: "FK_CreditCardApplications_CreditCardProducts_CreditCardProduct~",
                        column: x => x.CreditCardProductId,
                        principalSchema: "catalog",
                        principalTable: "CreditCardProducts",
                        principalColumn: "CreditCardProductId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CreditCardApplications_SaleStatus_StatusId",
                        column: x => x.StatusId,
                        principalSchema: "catalog",
                        principalTable: "SaleStatus",
                        principalColumn: "StatusId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CreditCardApplications_Stores_StoreId",
                        column: x => x.StoreId,
                        principalSchema: "catalog",
                        principalTable: "Stores",
                        principalColumn: "StoreId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CreditCardApplications_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "auth",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MembershipSales",
                schema: "sales",
                columns: table => new
                {
                    MembershipSaleId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    StoreId = table.Column<int>(type: "integer", nullable: false),
                    MembershipProductId = table.Column<int>(type: "integer", nullable: false),
                    StatusId = table.Column<byte>(type: "smallint", nullable: false),
                    SoldAtUtc = table.Column<DateTime>(type: "timestamp(0) with time zone", precision: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MembershipSales", x => x.MembershipSaleId);
                    table.ForeignKey(
                        name: "FK_MembershipSales_MembershipProducts_MembershipProductId",
                        column: x => x.MembershipProductId,
                        principalSchema: "catalog",
                        principalTable: "MembershipProducts",
                        principalColumn: "MembershipProductId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MembershipSales_SaleStatus_StatusId",
                        column: x => x.StatusId,
                        principalSchema: "catalog",
                        principalTable: "SaleStatus",
                        principalColumn: "StatusId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MembershipSales_Stores_StoreId",
                        column: x => x.StoreId,
                        principalSchema: "catalog",
                        principalTable: "Stores",
                        principalColumn: "StoreId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MembershipSales_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "auth",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserDailySettings",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SettingDate = table.Column<DateTime>(type: "date", nullable: false),
                    SalesGoalAmount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    AppsGoal = table.Column<int>(type: "integer", nullable: false),
                    MembershipsGoal = table.Column<int>(type: "integer", nullable: false),
                    StoreId = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDailySettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserDailySettings_Stores_StoreId",
                        column: x => x.StoreId,
                        principalSchema: "catalog",
                        principalTable: "Stores",
                        principalColumn: "StoreId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserDailySettings_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "auth",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserExternalLogins",
                schema: "auth",
                columns: table => new
                {
                    UserExternalLoginId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IdentityProviderId = table.Column<byte>(type: "smallint", nullable: false),
                    ProviderSubject = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp(0) with time zone", precision: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserExternalLogins", x => x.UserExternalLoginId);
                    table.ForeignKey(
                        name: "FK_UserExternalLogins_IdentityProviders_IdentityProviderId",
                        column: x => x.IdentityProviderId,
                        principalSchema: "catalog",
                        principalTable: "IdentityProviders",
                        principalColumn: "IdentityProviderId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserExternalLogins_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "auth",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CreditCardApplications_CreditCardProductId",
                schema: "sales",
                table: "CreditCardApplications",
                column: "CreditCardProductId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditCardApplications_StatusId",
                schema: "sales",
                table: "CreditCardApplications",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditCardApplications_StoreId",
                schema: "sales",
                table: "CreditCardApplications",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditCardApplications_UserId",
                schema: "sales",
                table: "CreditCardApplications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditCardProducts_ProductCode",
                schema: "catalog",
                table: "CreditCardProducts",
                column: "ProductCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IdentityProviders_ProviderCode",
                schema: "catalog",
                table: "IdentityProviders",
                column: "ProviderCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MembershipProducts_ProductCode",
                schema: "catalog",
                table: "MembershipProducts",
                column: "ProductCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MembershipSales_MembershipProductId",
                schema: "sales",
                table: "MembershipSales",
                column: "MembershipProductId");

            migrationBuilder.CreateIndex(
                name: "IX_MembershipSales_StatusId",
                schema: "sales",
                table: "MembershipSales",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_MembershipSales_StoreId",
                schema: "sales",
                table: "MembershipSales",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_MembershipSales_UserId",
                schema: "sales",
                table: "MembershipSales",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Sales_StoreId_SaleDate",
                schema: "sales",
                table: "Sales",
                columns: new[] { "StoreId", "SaleDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Sales_UserId_SaleDate",
                schema: "sales",
                table: "Sales",
                columns: new[] { "UserId", "SaleDate" });

            migrationBuilder.CreateIndex(
                name: "IX_SaleStatus_StatusCode",
                schema: "catalog",
                table: "SaleStatus",
                column: "StatusCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Stores_StoreNumber",
                schema: "catalog",
                table: "Stores",
                column: "StoreNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserDailySettings_StoreId",
                schema: "catalog",
                table: "UserDailySettings",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDailySettings_UserId_SettingDate",
                schema: "catalog",
                table: "UserDailySettings",
                columns: new[] { "UserId", "SettingDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserExternalLogins_IdentityProviderId_ProviderSubject",
                schema: "auth",
                table: "UserExternalLogins",
                columns: new[] { "IdentityProviderId", "ProviderSubject" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserExternalLogins_UserId",
                schema: "auth",
                table: "UserExternalLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                schema: "auth",
                table: "Users",
                column: "Email",
                filter: "\"Email\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CreditCardApplications",
                schema: "sales");

            migrationBuilder.DropTable(
                name: "MembershipSales",
                schema: "sales");

            migrationBuilder.DropTable(
                name: "Sales",
                schema: "sales");

            migrationBuilder.DropTable(
                name: "UserDailySettings",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "UserExternalLogins",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "CreditCardProducts",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "MembershipProducts",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "SaleStatus",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "Stores",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "IdentityProviders",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "auth");
        }
    }
}

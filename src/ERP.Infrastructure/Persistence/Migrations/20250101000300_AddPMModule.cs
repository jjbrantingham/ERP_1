using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPMModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create pm schema
            migrationBuilder.EnsureSchema(
                name: "pm");

            // Create Projects table
            migrationBuilder.CreateTable(
                name: "Projects",
                schema: "pm",
                columns: table => new
                {
                    ProjectId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ClientId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ProjectType = table.Column<byte>(type: "tinyint", nullable: false),
                    BillingMode = table.Column<byte>(type: "tinyint", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BudgetAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    BudgetCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    ProjectManagerId = table.Column<long>(type: "bigint", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.ProjectId);
                    table.ForeignKey(
                        name: "FK_Projects_Clients_ClientId",
                        column: x => x.ClientId,
                        principalSchema: "crm",
                        principalTable: "Clients",
                        principalColumn: "ClientId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Projects_Employees_ProjectManagerId",
                        column: x => x.ProjectManagerId,
                        principalSchema: "hr",
                        principalTable: "Employees",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Restrict);
                });

            // Create WBSItems table
            migrationBuilder.CreateTable(
                name: "WBSItems",
                schema: "pm",
                columns: table => new
                {
                    WBSItemId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<long>(type: "bigint", nullable: false),
                    ParentId = table.Column<long>(type: "bigint", nullable: true),
                    WBSCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    EstimatedHours = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    ActualHours = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    PercentComplete = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    BudgetAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    BudgetCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WBSItems", x => x.WBSItemId);
                    table.ForeignKey(
                        name: "FK_WBSItems_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalSchema: "pm",
                        principalTable: "Projects",
                        principalColumn: "ProjectId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WBSItems_WBSItems_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "pm",
                        principalTable: "WBSItems",
                        principalColumn: "WBSItemId",
                        onDelete: ReferentialAction.Restrict);
                });

            // Create Contracts table
            migrationBuilder.CreateTable(
                name: "Contracts",
                schema: "pm",
                columns: table => new
                {
                    ContractId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<long>(type: "bigint", nullable: false),
                    ContractNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ContractType = table.Column<byte>(type: "tinyint", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ContractValueAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    ContractValueCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SignedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Terms = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contracts", x => x.ContractId);
                    table.ForeignKey(
                        name: "FK_Contracts_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalSchema: "pm",
                        principalTable: "Projects",
                        principalColumn: "ProjectId",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create indexes for Projects
            migrationBuilder.CreateIndex(
                name: "IX_Projects_TenantId_ProjectNumber",
                schema: "pm",
                table: "Projects",
                columns: new[] { "TenantId", "ProjectNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ClientId",
                schema: "pm",
                table: "Projects",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ProjectManagerId",
                schema: "pm",
                table: "Projects",
                column: "ProjectManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Status",
                schema: "pm",
                table: "Projects",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ProjectType",
                schema: "pm",
                table: "Projects",
                column: "ProjectType");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_TenantId_Status",
                schema: "pm",
                table: "Projects",
                columns: new[] { "TenantId", "Status" });

            // Create indexes for WBSItems
            migrationBuilder.CreateIndex(
                name: "IX_WBSItems_ProjectId_WBSCode",
                schema: "pm",
                table: "WBSItems",
                columns: new[] { "ProjectId", "WBSCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WBSItems_ParentId",
                schema: "pm",
                table: "WBSItems",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_WBSItems_ProjectId_SortOrder",
                schema: "pm",
                table: "WBSItems",
                columns: new[] { "ProjectId", "SortOrder" });

            // Create indexes for Contracts
            migrationBuilder.CreateIndex(
                name: "IX_Contracts_TenantId_ContractNumber",
                schema: "pm",
                table: "Contracts",
                columns: new[] { "TenantId", "ContractNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_ProjectId",
                schema: "pm",
                table: "Contracts",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_ContractType",
                schema: "pm",
                table: "Contracts",
                column: "ContractType");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_IsActive_EndDate",
                schema: "pm",
                table: "Contracts",
                columns: new[] { "IsActive", "EndDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Contracts",
                schema: "pm");

            migrationBuilder.DropTable(
                name: "WBSItems",
                schema: "pm");

            migrationBuilder.DropTable(
                name: "Projects",
                schema: "pm");
        }
    }
}

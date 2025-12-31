using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Infrastructure.Persistence.Migrations
{
    public partial class AddTEModule : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(name: "te");

            migrationBuilder.CreateTable(
                name: "Timesheets",
                schema: "te",
                columns: table => new
                {
                    TimesheetId = table.Column<long>(type: "bigint", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<long>(type: "bigint", nullable: false),
                    PeriodStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    TotalHours = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SubmittedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    ApprovalComments = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Timesheets", x => x.TimesheetId);
                    table.ForeignKey(name: "FK_Timesheets_Employees_EmployeeId", column: x => x.EmployeeId, principalSchema: "hr", principalTable: "Employees", principalColumn: "EmployeeId", onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TimesheetEntries",
                schema: "te",
                columns: table => new
                {
                    TimesheetEntryId = table.Column<long>(type: "bigint", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TimesheetId = table.Column<long>(type: "bigint", nullable: false),
                    ProjectId = table.Column<long>(type: "bigint", nullable: true),
                    WBSItemId = table.Column<long>(type: "bigint", nullable: true),
                    WorkDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Hours = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    IsBillable = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimesheetEntries", x => x.TimesheetEntryId);
                    table.ForeignKey(name: "FK_TimesheetEntries_Timesheets_TimesheetId", column: x => x.TimesheetId, principalSchema: "te", principalTable: "Timesheets", principalColumn: "TimesheetId", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExpenseReports",
                schema: "te",
                columns: table => new
                {
                    ExpenseReportId = table.Column<long>(type: "bigint", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<long>(type: "bigint", nullable: false),
                    ReportNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Purpose = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReportDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    SubmittedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    ApprovalComments = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ReimbursedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpenseReports", x => x.ExpenseReportId);
                    table.ForeignKey(name: "FK_ExpenseReports_Employees_EmployeeId", column: x => x.EmployeeId, principalSchema: "hr", principalTable: "Employees", principalColumn: "EmployeeId", onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExpenseItems",
                schema: "te",
                columns: table => new
                {
                    ExpenseItemId = table.Column<long>(type: "bigint", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExpenseReportId = table.Column<long>(type: "bigint", nullable: false),
                    ProjectId = table.Column<long>(type: "bigint", nullable: true),
                    ExpenseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Category = table.Column<byte>(type: "tinyint", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Merchant = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ReceiptNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    HasReceipt = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsReimbursable = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpenseItems", x => x.ExpenseItemId);
                    table.ForeignKey(name: "FK_ExpenseItems_ExpenseReports_ExpenseReportId", column: x => x.ExpenseReportId, principalSchema: "te", principalTable: "ExpenseReports", principalColumn: "ExpenseReportId", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(name: "IX_Timesheets_EmployeeId", schema: "te", table: "Timesheets", column: "EmployeeId");
            migrationBuilder.CreateIndex(name: "IX_Timesheets_Status", schema: "te", table: "Timesheets", column: "Status");
            migrationBuilder.CreateIndex(name: "IX_Timesheets_EmployeeId_Period", schema: "te", table: "Timesheets", columns: new[] { "EmployeeId", "PeriodStart", "PeriodEnd" });
            migrationBuilder.CreateIndex(name: "IX_Timesheets_TenantId_Status", schema: "te", table: "Timesheets", columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(name: "IX_TimesheetEntries_TimesheetId", schema: "te", table: "TimesheetEntries", column: "TimesheetId");
            migrationBuilder.CreateIndex(name: "IX_TimesheetEntries_ProjectId", schema: "te", table: "TimesheetEntries", column: "ProjectId");
            migrationBuilder.CreateIndex(name: "IX_TimesheetEntries_WorkDate", schema: "te", table: "TimesheetEntries", column: "WorkDate");
            migrationBuilder.CreateIndex(name: "IX_TimesheetEntries_ProjectId_IsBillable", schema: "te", table: "TimesheetEntries", columns: new[] { "ProjectId", "IsBillable" });

            migrationBuilder.CreateIndex(name: "IX_ExpenseReports_TenantId_ReportNumber", schema: "te", table: "ExpenseReports", columns: new[] { "TenantId", "ReportNumber" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_ExpenseReports_EmployeeId", schema: "te", table: "ExpenseReports", column: "EmployeeId");
            migrationBuilder.CreateIndex(name: "IX_ExpenseReports_Status", schema: "te", table: "ExpenseReports", column: "Status");
            migrationBuilder.CreateIndex(name: "IX_ExpenseReports_TenantId_Status", schema: "te", table: "ExpenseReports", columns: new[] { "TenantId", "Status" });
            migrationBuilder.CreateIndex(name: "IX_ExpenseReports_ReportDate", schema: "te", table: "ExpenseReports", column: "ReportDate");

            migrationBuilder.CreateIndex(name: "IX_ExpenseItems_ExpenseReportId", schema: "te", table: "ExpenseItems", column: "ExpenseReportId");
            migrationBuilder.CreateIndex(name: "IX_ExpenseItems_Category", schema: "te", table: "ExpenseItems", column: "Category");
            migrationBuilder.CreateIndex(name: "IX_ExpenseItems_ExpenseDate", schema: "te", table: "ExpenseItems", column: "ExpenseDate");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "TimesheetEntries", schema: "te");
            migrationBuilder.DropTable(name: "Timesheets", schema: "te");
            migrationBuilder.DropTable(name: "ExpenseItems", schema: "te");
            migrationBuilder.DropTable(name: "ExpenseReports", schema: "te");
        }
    }
}

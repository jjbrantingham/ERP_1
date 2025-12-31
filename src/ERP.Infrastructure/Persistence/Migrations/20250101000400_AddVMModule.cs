using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVMModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create vm schema
            migrationBuilder.EnsureSchema(
                name: "vm");

            // Create Vendors table
            migrationBuilder.CreateTable(
                name: "Vendors",
                schema: "vm",
                columns: table => new
                {
                    VendorId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VendorNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    VendorType = table.Column<byte>(type: "tinyint", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    TaxId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Website = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Fax = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AddressStreet = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AddressStreet2 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AddressCity = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AddressStateProvince = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AddressPostalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    AddressCountry = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PaymentTerms = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PaymentDueDays = table.Column<int>(type: "int", nullable: true),
                    PreferredPaymentMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AccountNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreditLimit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vendors", x => x.VendorId);
                });

            // Create VendorContacts table
            migrationBuilder.CreateTable(
                name: "VendorContacts",
                schema: "vm",
                columns: table => new
                {
                    VendorContactId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VendorId = table.Column<long>(type: "bigint", nullable: false),
                    ContactType = table.Column<byte>(type: "tinyint", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Mobile = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Notes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendorContacts", x => x.VendorContactId);
                    table.ForeignKey(
                        name: "FK_VendorContacts_Vendors_VendorId",
                        column: x => x.VendorId,
                        principalSchema: "vm",
                        principalTable: "Vendors",
                        principalColumn: "VendorId",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create VendorNotes table
            migrationBuilder.CreateTable(
                name: "VendorNotes",
                schema: "vm",
                columns: table => new
                {
                    VendorNoteId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VendorId = table.Column<long>(type: "bigint", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NoteDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendorNotes", x => x.VendorNoteId);
                    table.ForeignKey(
                        name: "FK_VendorNotes_Vendors_VendorId",
                        column: x => x.VendorId,
                        principalSchema: "vm",
                        principalTable: "Vendors",
                        principalColumn: "VendorId",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create indexes for Vendors
            migrationBuilder.CreateIndex(
                name: "IX_Vendors_TenantId_VendorNumber",
                schema: "vm",
                table: "Vendors",
                columns: new[] { "TenantId", "VendorNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vendors_Status",
                schema: "vm",
                table: "Vendors",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Vendors_VendorType",
                schema: "vm",
                table: "Vendors",
                column: "VendorType");

            migrationBuilder.CreateIndex(
                name: "IX_Vendors_TenantId_Name",
                schema: "vm",
                table: "Vendors",
                columns: new[] { "TenantId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_Vendors_TenantId_TaxId",
                schema: "vm",
                table: "Vendors",
                columns: new[] { "TenantId", "TaxId" });

            // Create indexes for VendorContacts
            migrationBuilder.CreateIndex(
                name: "IX_VendorContacts_VendorId",
                schema: "vm",
                table: "VendorContacts",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_VendorContacts_ContactType",
                schema: "vm",
                table: "VendorContacts",
                column: "ContactType");

            migrationBuilder.CreateIndex(
                name: "IX_VendorContacts_VendorId_IsPrimary",
                schema: "vm",
                table: "VendorContacts",
                columns: new[] { "VendorId", "IsPrimary" });

            // Create indexes for VendorNotes
            migrationBuilder.CreateIndex(
                name: "IX_VendorNotes_VendorId",
                schema: "vm",
                table: "VendorNotes",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_VendorNotes_NoteDate",
                schema: "vm",
                table: "VendorNotes",
                column: "NoteDate");

            migrationBuilder.CreateIndex(
                name: "IX_VendorNotes_VendorId_NoteDate",
                schema: "vm",
                table: "VendorNotes",
                columns: new[] { "VendorId", "NoteDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VendorContacts",
                schema: "vm");

            migrationBuilder.DropTable(
                name: "VendorNotes",
                schema: "vm");

            migrationBuilder.DropTable(
                name: "Vendors",
                schema: "vm");
        }
    }
}

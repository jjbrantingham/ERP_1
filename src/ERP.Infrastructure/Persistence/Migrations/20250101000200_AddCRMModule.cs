using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCRMModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create crm schema
            migrationBuilder.EnsureSchema(
                name: "crm");

            // Create Clients table
            migrationBuilder.CreateTable(
                name: "Clients",
                schema: "crm",
                columns: table => new
                {
                    ClientId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ClientType = table.Column<byte>(type: "tinyint", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    LegalName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TaxId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Website = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Industry = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PrimaryEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    PrimaryPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    BillingStreet = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    BillingStreet2 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    BillingCity = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BillingStateProvince = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BillingPostalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    BillingCountry = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ShippingStreet = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ShippingStreet2 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ShippingCity = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ShippingStateProvince = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ShippingPostalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ShippingCountry = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FirstContactDate = table.Column<DateTime>(type: "date", nullable: true),
                    LastContactDate = table.Column<DateTime>(type: "date", nullable: true),
                    AccountManagerId = table.Column<long>(type: "bigint", nullable: true),
                    PaymentTerms = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreditLimit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<long>(type: "bigint", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.ClientId);
                });

            // Create Contacts table
            migrationBuilder.CreateTable(
                name: "Contacts",
                schema: "crm",
                columns: table => new
                {
                    ContactId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<long>(type: "bigint", nullable: false),
                    ContactType = table.Column<byte>(type: "tinyint", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MiddleName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    JobTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Department = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    MobileNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Notes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<long>(type: "bigint", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contacts", x => x.ContactId);
                    table.ForeignKey(
                        name: "FK_Contacts_Clients_ClientId",
                        column: x => x.ClientId,
                        principalSchema: "crm",
                        principalTable: "Clients",
                        principalColumn: "ClientId",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create Notes table
            migrationBuilder.CreateTable(
                name: "Notes",
                schema: "crm",
                columns: table => new
                {
                    NoteId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<long>(type: "bigint", nullable: false),
                    ContactId = table.Column<long>(type: "bigint", nullable: true),
                    NoteType = table.Column<byte>(type: "tinyint", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    NoteDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FollowUpDate = table.Column<DateTime>(type: "date", nullable: true),
                    IsFollowUpComplete = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    AuthorId = table.Column<long>(type: "bigint", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<long>(type: "bigint", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notes", x => x.NoteId);
                    table.ForeignKey(
                        name: "FK_Notes_Clients_ClientId",
                        column: x => x.ClientId,
                        principalSchema: "crm",
                        principalTable: "Clients",
                        principalColumn: "ClientId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Notes_Contacts_ContactId",
                        column: x => x.ContactId,
                        principalSchema: "crm",
                        principalTable: "Contacts",
                        principalColumn: "ContactId",
                        onDelete: ReferentialAction.Restrict);
                });

            // Create indexes for Clients
            migrationBuilder.CreateIndex(
                name: "IX_Clients_TenantId_ClientNumber",
                schema: "crm",
                table: "Clients",
                columns: new[] { "TenantId", "ClientNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clients_TenantId_Name",
                schema: "crm",
                table: "Clients",
                columns: new[] { "TenantId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_Clients_Status",
                schema: "crm",
                table: "Clients",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_ClientType",
                schema: "crm",
                table: "Clients",
                column: "ClientType");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_Industry",
                schema: "crm",
                table: "Clients",
                column: "Industry");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_AccountManagerId",
                schema: "crm",
                table: "Clients",
                column: "AccountManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_IsActive",
                schema: "crm",
                table: "Clients",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_LastContactDate",
                schema: "crm",
                table: "Clients",
                column: "LastContactDate");

            // Create indexes for Contacts
            migrationBuilder.CreateIndex(
                name: "IX_Contacts_ClientId",
                schema: "crm",
                table: "Contacts",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_TenantId_Email",
                schema: "crm",
                table: "Contacts",
                columns: new[] { "TenantId", "Email" });

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_ContactType",
                schema: "crm",
                table: "Contacts",
                column: "ContactType");

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_IsPrimary",
                schema: "crm",
                table: "Contacts",
                column: "IsPrimary");

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_IsActive",
                schema: "crm",
                table: "Contacts",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_FirstName_LastName",
                schema: "crm",
                table: "Contacts",
                columns: new[] { "FirstName", "LastName" });

            // Create indexes for Notes
            migrationBuilder.CreateIndex(
                name: "IX_Notes_ClientId",
                schema: "crm",
                table: "Notes",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_ContactId",
                schema: "crm",
                table: "Notes",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_NoteType",
                schema: "crm",
                table: "Notes",
                column: "NoteType");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_NoteDate",
                schema: "crm",
                table: "Notes",
                column: "NoteDate");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_FollowUpDate",
                schema: "crm",
                table: "Notes",
                column: "FollowUpDate");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_AuthorId",
                schema: "crm",
                table: "Notes",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_IsFollowUpComplete_FollowUpDate",
                schema: "crm",
                table: "Notes",
                columns: new[] { "IsFollowUpComplete", "FollowUpDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notes",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "Contacts",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "Clients",
                schema: "crm");
        }
    }
}

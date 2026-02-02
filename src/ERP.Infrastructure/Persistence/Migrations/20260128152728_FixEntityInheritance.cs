using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixEntityInheritance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // No-op: RowVersion columns are already correctly configured as rowversion type.
            // The model change is only a nullability change in the C# model (byte[]? to byte[])
            // which doesn't require a database schema change since rowversion columns are
            // always non-null and auto-populated by SQL Server.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No-op: Nothing to revert
        }
    }
}

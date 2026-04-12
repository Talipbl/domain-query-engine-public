using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DomainQueryEngine.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class DomainUpdate2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Domains_DomainName",
                table: "Domains");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Domains_DomainName",
                table: "Domains",
                column: "DomainName",
                unique: true);
        }
    }
}

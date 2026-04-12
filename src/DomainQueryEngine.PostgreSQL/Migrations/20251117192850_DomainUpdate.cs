using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DomainQueryEngine.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class DomainUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "DomainVisits");

            migrationBuilder.DropColumn(
                name: "UserAgent",
                table: "DomainVisits");

            migrationBuilder.DropColumn(
                name: "UserIp",
                table: "DomainVisits");

            migrationBuilder.AddColumn<string>(
                name: "SessionId",
                table: "Domains",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserAgent",
                table: "Domains",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserIp",
                table: "Domains",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "Domains");

            migrationBuilder.DropColumn(
                name: "UserAgent",
                table: "Domains");

            migrationBuilder.DropColumn(
                name: "UserIp",
                table: "Domains");

            migrationBuilder.AddColumn<string>(
                name: "SessionId",
                table: "DomainVisits",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserAgent",
                table: "DomainVisits",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserIp",
                table: "DomainVisits",
                type: "text",
                nullable: true);
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DomainQueryEngine.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Domains",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DomainName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Tld = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Domains", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Providers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BaseUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    VisitLinkTemplate = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    LogoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Providers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DomainLookupResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DomainId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsSuccess = table.Column<bool>(type: "boolean", nullable: false),
                    IsAvailable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ResponseTimeMs = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    RawResponse = table.Column<string>(type: "text", nullable: false),
                    PriceAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    PriceCurrency = table.Column<string>(type: "text", nullable: true),
                    PricePeriod = table.Column<int>(type: "integer", nullable: true),
                    CheckedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DomainLookupResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DomainLookupResults_Domains_DomainId",
                        column: x => x.DomainId,
                        principalTable: "Domains",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DomainLookupResults_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DomainVisits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DomainId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderId = table.Column<Guid>(type: "uuid", nullable: false),
                    VisitLink = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    VisitedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UserIp = table.Column<string>(type: "text", nullable: true),
                    UserAgent = table.Column<string>(type: "text", nullable: true),
                    SessionId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DomainVisits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DomainVisits_Domains_DomainId",
                        column: x => x.DomainId,
                        principalTable: "Domains",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DomainVisits_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DomainLookupResults_DomainId",
                table: "DomainLookupResults",
                column: "DomainId");

            migrationBuilder.CreateIndex(
                name: "IX_DomainLookupResults_ProviderId",
                table: "DomainLookupResults",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_Domains_DomainName",
                table: "Domains",
                column: "DomainName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DomainVisits_DomainId",
                table: "DomainVisits",
                column: "DomainId");

            migrationBuilder.CreateIndex(
                name: "IX_DomainVisits_ProviderId",
                table: "DomainVisits",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_Providers_Name",
                table: "Providers",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DomainLookupResults");

            migrationBuilder.DropTable(
                name: "DomainVisits");

            migrationBuilder.DropTable(
                name: "Domains");

            migrationBuilder.DropTable(
                name: "Providers");
        }
    }
}

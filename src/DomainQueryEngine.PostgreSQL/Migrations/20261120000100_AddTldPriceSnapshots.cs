using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DomainQueryEngine.PostgreSQL.Migrations
{
    public partial class AddTldPriceSnapshots : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TldPriceSync",
                columns: table => new
                {
                    ProviderSlug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SourceUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    LastAttemptAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastSuccessAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PriceCount = table.Column<int>(type: "integer", nullable: false),
                    LastError = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table => table.PrimaryKey("PK_TldPriceSync", x => x.ProviderSlug));

            migrationBuilder.CreateTable(
                name: "TldProviderPrices",
                columns: table => new
                {
                    ProviderSlug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Tld = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Registration = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    RegularRegistration = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    RegistrationTermYears = table.Column<int>(type: "integer", nullable: false),
                    Renewal = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    RenewalTermYears = table.Column<int>(type: "integer", nullable: false),
                    SourceUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ObservedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_TldProviderPrices", x => new { x.ProviderSlug, x.Tld }));

            migrationBuilder.CreateIndex(name: "IX_TldProviderPrices_ObservedAt", table: "TldProviderPrices", column: "ObservedAt");
            migrationBuilder.CreateIndex(name: "IX_TldProviderPrices_Tld", table: "TldProviderPrices", column: "Tld");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "TldProviderPrices");
            migrationBuilder.DropTable(name: "TldPriceSync");
        }
    }
}

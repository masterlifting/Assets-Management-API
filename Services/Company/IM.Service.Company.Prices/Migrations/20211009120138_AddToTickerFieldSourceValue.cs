using Microsoft.EntityFrameworkCore.Migrations;

namespace IM.Service.Company.Prices.Migrations
{
    public partial class AddToTickerFieldSourceValue : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SourceValue",
                table: "Tickers",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SourceValue",
                table: "Tickers");
        }
    }
}

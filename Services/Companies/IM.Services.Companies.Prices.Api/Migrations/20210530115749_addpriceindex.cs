using Microsoft.EntityFrameworkCore.Migrations;

namespace IM.Services.Companies.Prices.Api.Migrations
{
    public partial class addpriceindex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Prices_Date",
                table: "Prices",
                column: "Date");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Prices_Date",
                table: "Prices");
        }
    }
}

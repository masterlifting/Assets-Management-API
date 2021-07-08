using Microsoft.EntityFrameworkCore.Migrations;

namespace IM.Services.Companies.Reports.Api.Migrations
{
    public partial class Remove_Report_IsChecked : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsChecked",
                table: "Reports");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsChecked",
                table: "Reports",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
